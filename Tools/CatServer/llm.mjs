// The AI the relay asks: Muse, Meta's Model API (dev.meta.ai). NewWorld AI uses Muse only.
//
//   MUSE_API_KEY   the Model API key (MODEL_API_KEY, as Meta's docs call it, works too)
//   MUSE_MODEL     default muse-spark-1.3
//   MUSE_REASONING how much Muse thinks before it answers: default "minimal" (it always reasons, and that thinking was
//                  most of a 4-7 s reply; headset review, Nov 4). If Muse refuses the setting, the relay falls back to
//                  "low" by itself and stays there.
//
// Conversations are kept as content blocks (text, tool_use, tool_result); ask() takes and gives that shape and
// translates to and from Muse's OpenAI-compatible Chat Completions format, so each character's actions (tool calls)
// come back as the same blocks whatever it asked for.
const env = process.env;
export const PROVIDERS = {
  muse: { name: "Muse", key: env.MUSE_API_KEY || env.MODEL_API_KEY, model: env.MUSE_MODEL || "muse-spark-1.3", url: "https://api.meta.ai/v1/chat/completions" },
};
export const available = () => Object.keys(PROVIDERS).filter(id => PROVIDERS[id].key);
export const DEFAULT = "muse";
// Every request is answered by Muse, whatever the headset sends.
export const pick = () => DEFAULT;

export class AIError extends Error {
  constructor(provider, status, message) { super(message); this.provider = provider; this.status = status; }
}

// One reply: { content, stop_reason, usage: { input_tokens, cache_read_input_tokens, output_tokens } }.
export async function ask(provider, { system, tools, messages }) {
  try { return await askChat(provider, system, tools, messages); }
  catch (e) {
    if (e instanceof AIError) throw e;
    throw new AIError(provider, e?.status, String(e?.message ?? e?.name ?? "error"));   // a network failure has no status
  }
}

// The history -> Chat Completions messages (server-tool blocks, which only another provider could write, are left out).
function toChat(system, messages) {
  const out = [{ role: "system", content: system }];
  for (const m of messages) {
    const blocks = typeof m.content === "string" ? [{ type: "text", text: m.content }] : m.content;
    const text = blocks.filter(b => b.type === "text").map(b => b.text).join(m.role === "user" ? "\n" : "");
    if (m.role === "user") {
      // a tool's result must follow the assistant message that called it, before any new words
      for (const b of blocks) if (b.type === "tool_result")
        out.push({ role: "tool", tool_call_id: b.tool_use_id, content: typeof b.content === "string" ? b.content : JSON.stringify(b.content ?? "") });
      if (text) out.push({ role: "user", content: text });
    } else {
      const calls = blocks.filter(b => b.type === "tool_use")
        .map(b => ({ id: b.id, type: "function", function: { name: b.name, arguments: JSON.stringify(b.input ?? {}) } }));
      out.push({ role: "assistant", content: text || null, ...(calls.length ? { tool_calls: calls } : {}) });
    }
  }
  return out;
}

let reasoning = env.MUSE_REASONING || "minimal";

async function askChat(provider, system, tools, messages) {
  const p = PROVIDERS[provider];
  const functions = (tools ?? []).filter(t => !t.type)   // typed server tools (web search) are not Muse's
    .map(t => ({ type: "function", function: { name: t.name, description: t.description, parameters: t.input_schema, strict: !!t.strict } }));
  const post = effort => fetch(p.url, {
    method: "POST",
    headers: { "Content-Type": "application/json", Authorization: `Bearer ${p.key}` },
    body: JSON.stringify({
      model: p.model,
      messages: toChat(system, messages),
      max_completion_tokens: 4000,   // counts the reasoning as well as the words (Muse always reasons)
      reasoning_effort: effort,
      ...(functions.length ? { tools: functions } : {}),
    }),
    signal: AbortSignal.timeout(90000),
  });
  let res = await post(reasoning), data = await res.json().catch(() => ({}));
  if (res.status === 400 && reasoning !== "low" && /reasoning/i.test(JSON.stringify(data?.error ?? data))) {
    console.log(`[muse] reasoning_effort "${reasoning}" refused; using "low"`);
    reasoning = "low";
    res = await post(reasoning); data = await res.json().catch(() => ({}));
  }
  if (!res.ok) throw new AIError(provider, res.status, String(data?.error?.message ?? res.statusText));
  const choice = data.choices?.[0] ?? {}, message = choice.message ?? {};
  const content = [];
  if (message.content) content.push({ type: "text", text: message.content });
  for (const [i, call] of (message.tool_calls ?? []).entries()) {
    let input = {};
    try { input = JSON.parse(call.function?.arguments || "{}"); } catch { /* an unreadable call acts with no arguments */ }
    content.push({ type: "tool_use", id: call.id || `call_${Date.now()}_${i}`, name: call.function?.name ?? "", input });
  }
  const finish = choice.finish_reason;
  const stop_reason = message.refusal || finish === "content_filter" ? "refusal"
    : finish === "tool_calls" || (message.tool_calls?.length && finish !== "length") ? "tool_use"
    : finish === "length" ? "max_tokens" : "end_turn";
  const u = data.usage ?? {}, cached = u.prompt_tokens_details?.cached_tokens ?? 0;
  return { content, stop_reason, usage: { input_tokens: (u.prompt_tokens ?? 0) - cached, cache_read_input_tokens: cached, output_tokens: u.completion_tokens ?? 0 } };
}
