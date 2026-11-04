using System.Collections.Generic;

namespace StudyEverywhere
{
    // The rest areas' words: the Starry Meadow's places, cloud row and moon (NightSky's phase and motion names), and Seabreeze's places and rows.
    public static partial class Localization
    {
        static partial void AddRest(Dictionary<string, string[]> t)
        {
            void A(string en, string ko, string ja, string es, string fr) => t[en] = new[] { ko, ja, es, fr };

            // Calypso, Seabreeze's voice companion: her card (her spoken replies are English, from the relay)
            A("Talk", "말하기", "話す", "Hablar", "Parler");
            A("Thinking…", "생각 중…", "考え中…", "Pensando…", "Je réfléchis…");
            A("Say “Calypso” to talk, or tap Talk.", "“칼립소”라고 부르거나 말하기를 누르세요.", "「カリプソ」と呼ぶか、「話す」をタップしてください。", "Di «Calypso» para hablar, o toca Hablar.", "Dites « Calypso » pour parler, ou touchez Parler.");
            A("Calypso's microphone isn't set up yet.", "칼립소의 마이크가 아직 설정되지 않았어요.", "カリプソのマイクはまだ設定されていません。", "El micrófono de Calypso aún no está configurado.", "Le micro de Calypso n'est pas encore configuré.");
            A("I didn't catch that. Say “Calypso”, then what you'd like.", "잘 못 들었어요. “칼립소”라고 부른 뒤 말씀해 주세요.", "聞き取れませんでした。「カリプソ」と呼んでから話してください。", "No te he entendido. Di «Calypso» y luego lo que quieras.", "Je n'ai pas compris. Dites « Calypso », puis ce que vous voulez.");
            A("There you go.", "여기 있어요.", "どうぞ。", "Listo.", "Voilà.");
            A("Mm, I'm here.", "음, 저 여기 있어요.", "うん、ここにいるよ。", "Mm, aquí estoy.", "Mm, je suis là.");
            A("There you go. I'll be here if you need anything else.", "다 됐어요. 더 필요한 게 있으면 말해 주세요.", "できました。ほかに何かあれば言ってね。", "Listo. Aquí estaré si necesitas algo más.", "Voilà. Je suis là si vous avez besoin d'autre chose.");
            A("My connection is resting right now, so I can't chat, but I can still change the time, hatch the turtles, calm the waves or put out the fire.", "지금은 연결이 쉬고 있어서 대화는 못 하지만, 시간 바꾸기, 거북이 부화, 파도 잠재우기, 불 끄기는 할 수 있어요.", "今は接続が休んでいてお話はできないけれど、時間の変更、カメのふ化、波を静める、火を消すことはできます。", "Ahora mi conexión está descansando y no puedo charlar, pero aún puedo cambiar la hora, hacer que nazcan las tortugas, calmar las olas o apagar el fuego.", "Ma connexion se repose pour l'instant, je ne peux pas discuter, mais je peux changer l'heure, faire éclore les tortues, calmer les vagues ou éteindre le feu.");
            A("Turtles hatching", "거북이 부화 중", "カメがふ化中", "Tortugas naciendo", "Les tortues éclosent");
            A("Day cycle on", "하루 순환 켜짐", "一日のサイクル オン", "Ciclo del día activado", "Cycle du jour activé");
            A("Day cycle paused", "하루 순환 멈춤", "一日のサイクル 一時停止", "Ciclo del día en pausa", "Cycle du jour en pause");
            A("Waves calmed", "파도 잔잔함", "波が静まった", "Olas en calma", "Vagues calmées");
            A("Waves rolling", "파도 일렁임", "波が寄せている", "Olas en movimiento", "Les vagues reviennent");
            A("Fire lit", "불 켜짐", "火がついた", "Fuego encendido", "Feu allumé");
            A("Fire out", "불 꺼짐", "火が消えた", "Fuego apagado", "Feu éteint");
            A("Sit", "앉기", "座る", "Sentado", "Assis");
            A("Recline", "기대기", "もたれる", "Reclinado", "Incliné");
            A("Lie back", "눕기", "寝転ぶ", "Tumbado", "Allongé");
            A("Few clouds", "구름 조금", "雲少なめ", "Pocas nubes", "Peu nuageux");
            A("Rise and set", "뜨고 지기", "出入り", "Sale y se pone", "Lever et coucher");
            A("New moon", "신월", "新月", "Luna nueva", "Nouvelle lune");
            A("Waxing crescent", "초승달", "三日月", "Luna creciente", "Premier croissant");
            A("First quarter", "상현달", "上弦の月", "Cuarto creciente", "Premier quartier");
            A("Waxing gibbous", "차오르는 달", "満ちゆく月", "Gibosa creciente", "Gibbeuse croissante");
            A("Full moon", "보름달", "満月", "Luna llena", "Pleine lune");
            A("Waning gibbous", "기우는 달", "欠けゆく月", "Gibosa menguante", "Gibbeuse décroissante");
            A("Last quarter", "하현달", "下弦の月", "Cuarto menguante", "Dernier quartier");
            // Seabreeze
            A("Shore", "물가", "波打ち際", "Orilla", "Rivage");
            A("Daybed", "데이베드", "デイベッド", "Cama de día", "Lit de jour");
            A("Fire", "모닥불", "焚き火", "Fuego", "Feu");
            A("Smooth the sand", "모래 고르기", "砂をならす", "Alisar la arena", "Lisser le sable");
            A("Write in the sand with a fingertip.", "손끝으로 모래에 글씨를 써 보세요.", "指先で砂に字を書いてみましょう。", "Escribe en la arena con la punta del dedo.", "Écrivez dans le sable du bout du doigt.");
            A("Waning crescent", "그믐달", "有明の月", "Luna menguante", "Dernier croissant");
        }
    }
}
