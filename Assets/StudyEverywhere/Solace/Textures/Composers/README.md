# Solace's composer portraits

Eight portraits hang in Solace (`ParisWorldBuilder.BuildSolaceComposers` in Editor/SolaceComposers.cs): four between
the shelves down the shelf side, two either side of each end door. All are public domain and come from Wikimedia
Commons; each file page's licence tag is given below (checked 2026-10-22). The other pictures, the music and the
Beethoven statue are in Solace/Composers (see its MEDIA.md).

| File here | Sitter | Work | Author, date | Commons file and licence |
|---|---|---|---|---|
| Mozart.jpg | Wolfgang Amadeus Mozart | Posthumous portrait in oils, commissioned by Joseph Sonnleithner | Barbara Krafft, 1819 | https://commons.wikimedia.org/wiki/File:Wolfgang-amadeus-mozart_1.jpg (original: https://upload.wikimedia.org/wikipedia/commons/1/1e/Wolfgang-amadeus-mozart_1.jpg). Public domain: PD-Art (PD-old-100-expired) |
| Beethoven.jpg | Ludwig van Beethoven | Portrait with the score of the Missa solemnis | Joseph Karl Stieler, 1820 | https://commons.wikimedia.org/wiki/File:Beethoven.jpg (original: https://upload.wikimedia.org/wikipedia/commons/6/6f/Beethoven.jpg). Public domain: PD-Art (PD-old-100-expired) |
| Chopin.jpg | Frédéric Chopin | Portrait in oils, Dordrechts Museum (DM-S-79) | Ary Scheffer, 1847 | https://commons.wikimedia.org/wiki/File:Ary_Scheffer_-_Portret_van_Fr%C3%A9d%C3%A9ric_Chopin_-_DM-S-79_-_Dordrechts_Museum.jpg (original: https://upload.wikimedia.org/wikipedia/commons/c/c8/Ary_Scheffer_-_Portret_van_Fr%C3%A9d%C3%A9ric_Chopin_-_DM-S-79_-_Dordrechts_Museum.jpg). Public domain: PD-Art (PD-old-auto-expired / PD-old-100-expired) |
| Rachmaninoff.jpg | Sergei Rachmaninoff | Head-and-shoulders studio photograph (Library of Congress, LCCN 91727711) | Kubey-Rembrandt Studios, Philadelphia, 1921 | https://commons.wikimedia.org/wiki/File:Rachmaninov_in_1921_(cropped).jpg (original: https://upload.wikimedia.org/wikipedia/commons/d/d4/Rachmaninov_in_1921_%28cropped%29.jpg; LoC: https://www.loc.gov/pictures/item/91727711/). Public domain: PD-old-70-expired; published in the US in 1921, before 1929 |
| Bach.jpg | Johann Sebastian Bach | Portrait in oils | Elias Gottlob Haussmann, 1748 | https://commons.wikimedia.org/wiki/File:Johann_Sebastian_Bach.jpg (via Solace/Composers/Bach/Pictures). Public domain: PD-Art (PD-old-100) |
| Liszt.jpg | Franz Liszt | Photograph | Nadar, March 1886 | https://commons.wikimedia.org/wiki/File:Franz_Liszt_by_Nadar,_March_1886.png (via Solace/Composers/Liszt/Pictures). Public domain: PD-old-100 |
| Tchaikovsky.jpg | Pyotr Ilyich Tchaikovsky | Portrait in oils, Tretyakov Gallery | Nikolai Kuznetsov, 1893 | https://commons.wikimedia.org/wiki/File:Porträt_des_Komponisten_Pjotr_I._Tschaikowski_(1840-1893).jpg (via Solace/Composers/Tchaikovsky/Pictures). Public domain: PD-Art (PD-old-95) |
| Debussy.jpg | Claude Debussy | Photograph | Nadar (studio), c. 1908 | https://commons.wikimedia.org/wiki/File:Claude_Debussy_ca_1908,_foto_av_Félix_Nadar.jpg (via Solace/Composers/Debussy/Pictures). Public domain: PD-old-100 |

Each was cropped to 3:4 with System.Drawing (JPEG, quality 92): Mozart and Beethoven at full height, Chopin inside
the oval of the painting's own gilt frame (the frame and the museum photograph's black backdrop cropped away),
Rachmaninoff above the studio's blind stamp at the lower right, the other four centred. Nothing else was changed.

`Composer portraits.jpg` (2048 x 1366) is the eight laid out four by two (Mozart, Beethoven, Chopin, Rachmaninoff on
top; Bach, Liszt, Tchaikovsky, Debussy below), cells of 512 x 683, so all the canvases share one material and merge
into one mesh. The builder imports it clamped, mipmapped, unscaled, ASTC 6x6 on Quest. The single 576 x 768
portraits are kept for reference and are not used by the scene.
