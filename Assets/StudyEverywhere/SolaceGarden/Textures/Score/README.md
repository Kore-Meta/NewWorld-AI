# The garden piano's score

Frédéric Chopin, Nocturne in E-flat major, Op. 9 No. 2 (composed 1830–32; the composition is in the public domain).

Engraving: the Mutopia Project, piece 1590, typeset in LilyPond by Renato Biolcati Rinaldi (2014) from the 1881
G. Schirmer edition. Reference: Mutopia-2014/12/18-1590.

- Piece page: https://www.mutopiaproject.org/cgibin/piece-info.cgi?id=1590
- PDF used: https://www.mutopiaproject.org/ftp/ChopinFF/O9/chopin_nocturne_op9_n2/chopin_nocturne_op9_n2-a4.pdf (A4, 3 pages)
- Licence: Creative Commons Attribution-ShareAlike 3.0 Unported, https://creativecommons.org/licenses/by-sa/3.0/

The three PNGs here are that PDF rendered page by page, unchanged, in greyscale at 1024 x 1448
(`pdftoppm -gray -png -scale-to-x 1024 -scale-to-y 1448`, poppler from MiKTeX). Each page keeps the Mutopia
credit and licence line at its foot. The rendered pages are shared under the same licence (CC BY-SA 3.0),
credited as above.

The builder (`ParisWorldBuilder.ScorePageMaterials` in Editor/SolaceGardenBuilder.cs) imports them clamped,
mipmapped, ASTC 4x4 on Quest, and `ScorePages` shows two facing pages on the music desk, turned two at a time.
