# Solace composer media: sources and licences

Every file here is public domain (PD) or CC0. **No attribution is legally required for any of them.** Where a source asks for a courtesy credit, it is listed under "Courtesy credit". An in-app credits line covering all of them is at the bottom of this file.

Processing (2026-10-22): each audio file is an excerpt from the start of the recording, with a short fade-in and a fade-out of about 4 s. It was loudness-normalised to about -18 LUFS (ffmpeg loudnorm, true peak -1.5 dBTP) and encoded as OGG Vorbis q5 at 44.1 kHz. Pictures were scaled to at most 1024 px on the long side and saved as JPG. The 3D model was decimated in Blender 4.5 and exported as FBX. All sources were downloaded from Wikimedia Commons; the "Source" link is each file's Commons page, where the full licence text is shown.

Historic recordings (Rachmaninoff 1920, Backhaus 1923) were restored from the original Commons files on 2026-10-23, before the steps above:
- rumble high-pass at 45 Hz
- `adeclick`, plus a second small-window pass to remove crackle
- `adeclip`
- `afftdn` hiss reduction, using a noise profile taken from the groove before the music starts (nr 14 for Rachmaninoff, nr 18 for Liszt)
- surface-noise low-pass at 7.5 kHz (Rachmaninoff) or 7 kHz (Liszt), with a gentle treble cut above 5 kHz (-2 or -3 dB)

The Liszt left and right channels were averaged to mono. Restoration does not change the licence.

Licence tags used below:
- **PD-old**: the creator died long ago (70 or 100+ years), so the work is public domain.
- **PD-Art**: a faithful photo of a public-domain 2D artwork. It is PD in the US and most countries. A few countries (for example the UK, arguably) may give the photographer a thin right.
- **PD-author (Musopen)**: the performers and Musopen released the recording into the public domain. Musopen asks, as a courtesy and not as a licence condition, that you credit "Musopen" and do not sell the recordings on their own. Using them inside an app is fine.
- **CC0**: a public-domain dedication by the rights holder.

---

## Beethoven

### Audio
| File | Work | Performer | Excerpt | Source | Date | Licence |
|---|---|---|---|---|---|---|
| `Beethoven/Audio/Moonlight Sonata - I. Adagio sostenuto (Paul Pitman).ogg` | Piano Sonata No. 14 "Moonlight", Op. 27 No. 2, I. Adagio sostenuto | Paul Pitman (for Musopen) | 1:45, stereo (source 0:04–1:49) | https://commons.wikimedia.org/wiki/File:Ludwig_van_Beethoven_-_sonata_no._14_in_c_sharp_minor_'moonlight',_op._27_no._2_-_i._adagio_sostenuto.ogg (from musopen.org/music/278) | Recording c. 2008 | Music PD-old-100; recording PD-author (Musopen, OTRS ticket 2008012110017088). Courtesy credit: "Musopen" |
| `Beethoven/Audio/Fur Elise (V. Gao).ogg` | Bagatelle in A minor "Für Elise", WoO 59 | V. Gao (Wikipedia user Gaodifan) | 1:21, stereo (source 0:02–1:22) | https://commons.wikimedia.org/wiki/File:FurElise.ogg | 2006-07-18 | Music PD-old; performance PD-self + CC0 |

### Pictures
| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Beethoven/Pictures/Portrait by Stieler 1820.jpg` | Portrait of Beethoven composing the Missa Solemnis | Joseph Karl Stieler | 1820 | https://commons.wikimedia.org/wiki/File:Beethoven.jpg (Beethoven-Haus Bonn) | PD-Art (PD-old-100) |
| `Beethoven/Pictures/Portrait by Hornemann 1803.jpg` | Miniature portrait of Beethoven | Christian Horneman | 1803 | https://commons.wikimedia.org/wiki/File:Beethoven_Hornemann.jpg | PD-Art (PD-old) |
| `Beethoven/Pictures/Manuscript Sonata Op 101 1816.jpg` | Piano Sonata in A major, Op. 101: manuscript sketch in Beethoven's hand | Ludwig van Beethoven | 1816 | https://commons.wikimedia.org/wiki/File:Beethoven_opus_101_manuscript.jpg (Library of Congress, molden-0508) | PD-old-100 |

### 3D model
| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Beethoven/Models/Beethoven Heiligenstadt Monument.fbx` | 3D scan of the Beethoven monument in Heiligenstädter Park, Vienna (Carrara marble, Robert Weigl, 1910) | Scan by noe-3d.at | Scan 2019-12-30 | https://commons.wikimedia.org/wiki/File:Ludwig_van_Beethoven_-_3D_model_by_noe-3d.at_-_Sketchfab.stl (originally https://sketchfab.com/3d-models/ludwig-van-beethoven-9b56c0a6a1444cd89f8933a11f2d55b6) | Scan CC0. The sculpture is PD-old (Weigl died 1902). |

Model notes: the source STL has 516,988 triangles. It was decimated to 60,000 triangles, given smooth shading, and its origin set to the base centre. Its size is about 6.84 × 6.87 × 15.34 units (Blender Z-up; scale is arbitrary) and it has no UVs or textures. The figure stands on a square plinth with a tree stump behind it.

---

## Mozart

### Audio
| File | Work | Performer | Excerpt | Source | Date | Licence |
|---|---|---|---|---|---|---|
| `Mozart/Audio/Symphony No. 40 - I. Molto allegro (Musopen Symphony).ogg` | Symphony No. 40 in G minor, K. 550, I. Molto allegro | Musopen Symphony (Musopen Kickstarter project) | 1:38, stereo (source 0:00–1:38) | https://commons.wikimedia.org/wiki/File:Mozart_-_Symphony_No._40_in_G_minor,_K550_-_I._Molto_allegro_(Musopen_Symphony).flac | 2012 | Music PD-old; recording PD-author (Musopen). Courtesy credit: "Musopen" |
| `Mozart/Audio/Piano Sonata No. 16 K. 545 - I. Allegro (Musopen).ogg` | Piano Sonata No. 16 in C major, K. 545 "Sonata facile", I. Allegro | Musopen recording (performer not named) | 1:42, stereo (source 0:01–1:43) | https://commons.wikimedia.org/wiki/File:Wolfgang_Amadeus_Mozart_-_sonata_no._16_in_c_major,_k.545_'sonata_facile'_-_i._allegro.ogg | c. 2008 | Music PD-old-100; recording PD-author (Musopen, OTRS 2008012110017088). Courtesy credit: "Musopen" |

### Pictures
| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Mozart/Pictures/Portrait by Krafft 1819.jpg` | Posthumous portrait of W. A. Mozart | Barbara Krafft | 1819 | https://commons.wikimedia.org/wiki/File:Wolfgang-amadeus-mozart_1.jpg | PD-Art (PD-old-100) |
| `Mozart/Pictures/Family portrait detail c1780.jpg` | Detail of Mozart from the Mozart family portrait | Possibly Johann Nepomuk della Croce | c. 1780 | https://commons.wikimedia.org/wiki/File:Croce-Mozart-Detail.jpg | PD-Art (PD-old) |
| `Mozart/Pictures/Manuscript Piano Concerto 21 1785.jpg` | Opening page of the autograph of Piano Concerto No. 21, K. 467 | W. A. Mozart | 1785-03-09 | https://commons.wikimedia.org/wiki/File:Mozart_-_Piano_Concerto_No._21_-_Opening_Page_of_the_Autograph_Manuscript.jpg (The Morgan Library) | PD-Art (PD-old) |

---

## Chopin

### Audio
| File | Work | Performer | Excerpt | Source | Date | Licence |
|---|---|---|---|---|---|---|
| `Chopin/Audio/Nocturne Op. 9 No. 2 (Frank Levy).ogg` | Nocturne in E-flat major, Op. 9 No. 2 | Frank Lévy (Musopen "Set Chopin Free" project) | 1:55, stereo (source 0:01–1:56) | https://commons.wikimedia.org/wiki/File:Chopin_-_Nocturne_No._2_in_E-flat_major,_Op._9_No._2_(Frank_Levy).flac | c. 2014 | Music PD-old; recording PD-author (Musopen). Courtesy credit: "Musopen" |
| `Chopin/Audio/Fantaisie-Impromptu Op. 66 (Frank Levy).ogg` | Fantaisie-Impromptu in C-sharp minor, Op. 66 | Frank Lévy (Musopen "Set Chopin Free" project) | 1:52, stereo (source 0:01–1:52) | https://commons.wikimedia.org/wiki/File:Chopin_-_Impromptu_No._4_in_C-sharp_minor,_Op._66_'Fantaisie-Impromptu'_(Frank_Levy).flac | c. 2014 | Music PD-old; recording PD-author (Musopen). Courtesy credit: "Musopen" |

### Pictures
| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Chopin/Pictures/Daguerreotype by Bisson 1849.jpg` | Photograph (daguerreotype) of Frédéric Chopin | Louis-Auguste Bisson | 1849 | https://commons.wikimedia.org/wiki/File:Frederic_Chopin_photo.jpeg | PD-old-100 |
| `Chopin/Pictures/Portrait by Delacroix 1838.jpg` | Portrait of Frédéric Chopin (from the double portrait with George Sand) | Eugène Delacroix | 1838 | https://commons.wikimedia.org/wiki/File:Eugène_Delacroix_-_Frédéric_Chopin_-_WGA06194.jpg (Louvre; via Web Gallery of Art) | PD-Art (PD-old-100) |
| `Chopin/Pictures/Manuscript Polonaise Op 53 1842.jpg` | Autograph manuscript of the Polonaise in A-flat major, Op. 53 | Frédéric Chopin | 1842 | https://commons.wikimedia.org/wiki/File:Chopin_polonaise_Op._53.jpg (Morgan Library, Heineman collection) | PD-old-100 |

---

## Rachmaninoff

### Audio
| File | Work | Performer | Excerpt | Source | Date | Licence |
|---|---|---|---|---|---|---|
| `Rachmaninoff/Audio/Prelude in C-sharp minor Op. 3 No. 2 (Rachmaninoff, 1920).ogg` | Prelude in C-sharp minor, Op. 3 No. 2 | **Sergei Rachmaninoff himself** (Edison Diamond Disc 82187-R) | 1:41, **mono** (source 0:01–1:42; the full recording is 3:48) | https://commons.wikimedia.org/wiki/File:Sergei_Rachmaninoff_performs_Rachmaninoff's_Prelude_in_C_sharp_minor,_Op._3.ogg (restored by Adam Cuerden from the U.S. National Park Service, Edison NHS transfer; unrestored: File:Prelude_3_2_Rach_playing.ogg) | Recorded 1919, issued Dec 1920 | Composition: published 1893, PD in the US and in life+70 countries (Rachmaninoff died 1943). Recording: PD in the US (sound recordings published before 1923 entered the public domain on 2022-01-01 under the Music Modernization Act). Commons tags it PD-old-80-record-expired. In the EU and most countries, recordings expire 50–70 years after publication, so a 1920 issue is long out of copyright there too. **Caveat: the US status is certain; worldwide status is very likely but not guaranteed in every country.** |
| `Rachmaninoff/Audio/Piano Concerto No. 2 - II. Adagio sostenuto (Musopen).ogg` | Piano Concerto No. 2 in C minor, Op. 18, II. Adagio sostenuto | Musopen recording (performers not named) | 2:00, stereo (source 0:00–2:00) | https://commons.wikimedia.org/wiki/File:Sergei_Rachmaninoff_-_piano_concerto_no._2_in_c_minor,_op._18_-_ii._adagio_sostenuto.ogg | c. 2008 | Recording PD-author (Musopen, OTRS 2008012110017088); courtesy credit "Musopen". Composition (1901) is PD in the US and in life+70 countries. **Caveat: in countries with life+80 or longer terms (for example Mexico, life+100, and Colombia, life+80), Rachmaninoff's compositions may still be protected. The CC0/PD-clean alternative everywhere is the Bach/Chopin/Mozart/Beethoven tracks; the same caveat applies to both Rachmaninoff tracks.** |

### Pictures
| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Rachmaninoff/Pictures/Photo Bain News Service.jpg` | Sergei Rachmaninoff standing at a piano (digitally restored) | Bain News Service | c. 1910s–1920s (not recorded) | https://commons.wikimedia.org/wiki/File:Sergei_Rachmaninoff_LOC_30160_cropped.jpg (Library of Congress, ggbain.30160) | PD-Bain (published before 1929; no known restrictions, LOC) |
| `Rachmaninoff/Pictures/Seated at Steinway Bain.jpg` | Rachmaninoff seated at a Steinway | Bain News Service | c. 1915–1920 | https://commons.wikimedia.org/wiki/File:Rachmaninoff_Seated_at_Steinway_Bain.jpg (LOC, lccn 2014710316) | PD-Bain |
| `Rachmaninoff/Pictures/Photo in overcoat Bain.jpg` | Rachmaninoff in an overcoat, holding his hat | Bain News Service | before 1929 (the Commons date "1900" is LOC's rough placeholder) | https://commons.wikimedia.org/wiki/File:Sergei_Rachmaninoff_LOC_33969u.jpg (LOC, lccn 2014714121) | PD-Bain |

---

## Bach (extra)

| File | Title / work | Author / performer | Date | Source | Licence |
|---|---|---|---|---|---|
| `Bach/Audio/Prelude in C major BWV 846 (Kimiko Ishizaka).ogg` | Well-Tempered Clavier Book 1, Prelude No. 1 in C major, BWV 846. Excerpt 1:58, stereo (source 0:01–1:58; full piece 2:43) | Kimiko Ishizaka ("Open Well-Tempered Clavier") | 2015 | https://commons.wikimedia.org/wiki/File:Kimiko_Ishizaka_-_Bach_-_Well-Tempered_Clavier,_Book_1_-_01_Prelude_No._1_in_C_major,_BWV_846.ogg (welltemperedclavier.org) | CC0 |
| `Bach/Pictures/Portrait by Haussmann 1748.jpg` | Portrait of J. S. Bach | Elias Gottlob Haussmann | 1748 | https://commons.wikimedia.org/wiki/File:Johann_Sebastian_Bach.jpg | PD-Art (PD-old-100) |
| `Bach/Pictures/Manuscript BWV 1001 Adagio 1720.jpg` | Autograph: Sonata for solo violin No. 1, BWV 1001, Adagio | J. S. Bach | 1720 | https://commons.wikimedia.org/wiki/File:BWV1001_adagio_autograph_manuscript_1720.jpeg (Bach-digital, Leipzig) | PD-old |

## Liszt (extra)

| File | Title / work | Author / performer | Date | Source | Licence |
|---|---|---|---|---|---|
| `Liszt/Audio/Liebestraum No. 3 (Wilhelm Backhaus, 1923).ogg` | Liebestraum No. 3 in A-flat major, S. 541/3. Excerpt 1:52, mono (source 0:03–1:55); a historic 78 rpm record; clicks and surface noise removed (see the restoration notes above) | Wilhelm Backhaus, piano (HMV D 788) | Recorded 1923-10-15, released 1924 | https://commons.wikimedia.org/wiki/File:PDP-CH_-_Wilhelm_Backhaus_-_Liebestraum_No._3_in_A-flat_major,_S.541-3_(Liszt)_-_Hmv-d788-05754.flac (Swiss Foundation Public Domain, pool.publicdomainproject.org) | Recording: PD in the EU (term expired) and in the US (pre-1925 publication, PD since 2025-01-01 under the MMA). Composition PD-old. **Caveat: the same "very likely worldwide, certain in US/EU" note as the Rachmaninoff 1920 disc.** |
| `Liszt/Pictures/Daguerreotype by Biow 1843.jpg` | Daguerreotype of Franz Liszt | Hermann Biow | 1843 | https://commons.wikimedia.org/wiki/File:Franz_Liszt_by_Herman_Biow-_1843.png | PD-old |
| `Liszt/Pictures/Photo by Nadar 1886.jpg` | Photograph of Franz Liszt | Nadar | 1886-03 | https://commons.wikimedia.org/wiki/File:Franz_Liszt_by_Nadar,_March_1886.png | PD-old-100 |

## Tchaikovsky (extra)

| File | Title / work | Author / performer | Date | Source | Licence |
|---|---|---|---|---|---|
| `Tchaikovsky/Audio/Symphony No. 6 - II. Allegro con grazia (Musopen Symphony).ogg` | Symphony No. 6 "Pathétique", Op. 74, II. Allegro con grazia (the 5/4 waltz). Excerpt 1:45, stereo (source 0:01–1:46) | Musopen Symphony (Musopen Kickstarter project) | 2012 | https://commons.wikimedia.org/wiki/File:Tchaikovsky_-_Symphony_No._6_in_B_minor,_Op._74_'Pathétique'_-_II._Allegro_con_grazia_(Musopen_Symphony).flac | Music PD-old; recording PD-author (Musopen). Courtesy credit: "Musopen" |
| `Tchaikovsky/Pictures/Photo by Reutlinger 1888.jpg` | Cabinet-card portrait of Tchaikovsky | Émile Reutlinger | 1888 (published 1904) | https://commons.wikimedia.org/wiki/File:Tchaikovsky_by_Reutlinger_(cropped).jpg (NYPL Digital Gallery 1158492) | PD-old-70 |
| `Tchaikovsky/Pictures/Portrait by Kuznetsov.jpg` | Portrait of Tchaikovsky (Tretyakov Gallery) | Nikolai Kuznetsov | painted 1893 (Commons lists 1906) | https://commons.wikimedia.org/wiki/File:Porträt_des_Komponisten_Pjotr_I._Tschaikowski_(1840-1893).jpg | PD-Art (PD-old-95; Kuznetsov died 1929) |

## Debussy (extra)

| File | Title | Author | Date | Source | Licence |
|---|---|---|---|---|---|
| `Debussy/Pictures/Photo by Nadar c1908.jpg` | Photograph of Claude Debussy | Nadar (studio) | c. 1908 | https://commons.wikimedia.org/wiki/File:Claude_Debussy_ca_1908,_foto_av_Félix_Nadar.jpg | PD-old-100 |

No Debussy audio is included. No PD or CC0 recording of a famous Debussy work could be cleanly cleared. A CC BY 3.0 alternative exists: "Clair de lune" by Laurens Goedhart (2011), https://commons.wikimedia.org/wiki/File:Clair_de_lune_(Claude_Debussy)_Suite_bergamasque.ogg. It would need the attribution text "Clair de lune (Debussy), performed by Laurens Goedhart, CC BY 3.0, https://creativecommons.org/licenses/by/3.0/". Commons also has Marcelle Meyer's 1956 recordings tagged PD, but their US status is doubtful, so they were not used.

---

## Suggested in-app credits line (optional; none legally required)

> Music recordings courtesy of Musopen (musopen.org), Kimiko Ishizaka (Open Well-Tempered Clavier, CC0), V. Gao (CC0), and historic recordings by Sergei Rachmaninoff (Edison, 1920) and Wilhelm Backhaus (HMV, 1923). Images: Wikimedia Commons, Library of Congress (Bain News Service), The Morgan Library. Beethoven monument scan: noe-3d.at (CC0).

---

## Portrait animation loops (`<Name>/Video/<name>-idle.mp4`, `<name>-talk.mp4`)

Generated on 2026-10-23 with Kling 3.0 (image-to-video, via the Artlist AI Suite under Kore Meta's subscription) from the public-domain portraits above, each clip starting and ending on the original image so it loops. 10 s, re-encoded to 576 px wide H.264 without audio. They are AI-generated animations of public-domain artworks, not historical footage, and are used under Artlist's terms for generated content.
