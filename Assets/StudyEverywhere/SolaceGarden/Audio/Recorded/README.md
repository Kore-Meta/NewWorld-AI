# Solace Garden instruments: recorded samples

Every file in this folder is cut from **VS Chamber Orchestra 2: Community Edition (VSCO-2 CE) 1.1.0**.

- Source: https://github.com/sgossner/VSCO-2-CE (files taken from the `master` branch through raw.githubusercontent.com, 2026-10-22)
- Homepage: http://vis.versilstudios.net/vsco-community.html
- Licence: **CC0 1.0 Universal** (public domain dedication; the repository's `LICENSE`). No attribution is required.
- The library's Readme asks (not a condition of CC0) that the samples are not sold on directly as a sample library, and
  that credit go to Versilian Studios / Sam Gossner and Ivy Audio / Simon Dalzell (the upright piano: "Upright Piano
  sampled by Simon Dalzell of Ivy Audio, Winter 2015-16"). Suggested credit line:
  *Instrument samples: VSCO 2 Community Edition by Versilian Studios (Sam Gossner) and Ivy Audio (Simon Dalzell), CC0.*

Made by `node Tools/Audio/make-garden-recorded.js <folder with the VSCO-2 CE files>`: mono, 44.1 kHz 16-bit, cut
from just before the attack, trimmed and faded, sustained tones tuned and looped; Unity imports them as Vorbis.

| Files here | From (VSCO-2 CE path) |
|---|---|
| `Piano soft NNN`, `Piano NNN`, `Piano loud NNN` (NNN = 021, 025 .. 105) | `Keys/Upright Piano/Player_dyn1/2/3_rr1_000 .. 042.wav` (even numbers; see its MappingChart.txt) |
| `Violin p NNN`, `Violin f NNN` (NNN = 055 057 060 064 067 069 072 076 079 081 084), `Violin zones.txt` | `Strings/Solo Violin/Arco Vib/LLVln_ArcoVib_{G3,A3,C4,E4,G4,A4,C5,E5,G5,A5,C6}_{p,f}.wav` (all 22 downloaded 2026-10-22; the solo violin, not the section). Made by `Tools/Audio/garden-violin.js` (run by make-garden-recorded.js, or alone): both microphones summed, tuned, a light body EQ and a small room added; each file is the recorded attack, a ~3.5-4.5 s loop (its end matched to its start, cross-faded) and the recorded release (bow lift and room). `Violin zones.txt` holds each file's loop points in samples, read by the builder into BowedVoice. |
| `Flute 072 076 081 084 088 093 096` | `Woodwinds/Flute/susvib/LDFlute_susvib_{C4,E4,A4,C5,E5,A5,C6}_v1_1.wav` (VSCO names the flute an octave low) |
| `Drum kick`, `Drum kick soft` | `VSCO 1 Percussion/drums/bass/bdrum_muted_ff_1`, `bdrum_muted_mf_1` (raised 3 semitones) |
| `Drum snare`, `Drum snare soft` | `VSCO 1 Percussion/drums/snare/drum1/snare1_ff_1`, `snare1_mp_1` |
| `Drum tom high`, `Drum tom high soft` | `VSCO 1 Percussion/drums/tenor/tenor_higher/tenorH_f_1`, `tenorH_mp_1` (raised 4 semitones) |
| `Drum tom low`, `Drum tom low soft` | `VSCO 1 Percussion/drums/tenor/tenor_lower/tenor_f_1`, `tenor_mf_1` |
| `Drum floor tom` | `VSCO 1 Percussion/drums/bass/bdrum_f_1` (an octave up) |
| `Drum crash`, `Drum crash soft` | `VSCO 1 Percussion/varMetal/Cymbals/susp/susp_hit_woodmall_f`, `_mf` |
| `Drum hihat`, `Drum hihat open` | `susp_hit_woodmall_f` raised ~10 semitones, high-passed and choked short (the library has no hi-hat) |
