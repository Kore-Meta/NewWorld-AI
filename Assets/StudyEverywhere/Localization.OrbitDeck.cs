using System.Collections.Generic;

namespace StudyEverywhere
{
    // The Orbit library's holodeck: the console, the list, the card, the four spacecraft of OrbitCraft (names, taglines,
    // fact sheets, parts and their stories) and Orbit's own lines when her relay is offline. Keyed by the English exactly
    // as OrbitBuilder and the catalogue write it.
    public static partial class Localization
    {
        static partial void AddOrbitDeck(Dictionary<string, string[]> t)
        {
            void A(string en, string ko, string ja, string es, string fr) => t[en] = new[] { ko, ja, es, fr };

            // ---- the console and its list
            A("ORBIT HOLODECK", "오빗 홀로덱", "オービット ホロデッキ", "HOLOCUBIERTA ORBIT", "HOLOPONT ORBIT");
            A("Explore spacecraft, the Solar System, the Universe and the mathematics of space", "우주선, 태양계, 우주, 그리고 우주의 수학을 탐험하세요", "宇宙機、太陽系、宇宙、そして宇宙の数学を探検しよう", "Explora naves, el sistema solar, el universo y las matemáticas del espacio", "Explorez les engins spatiaux, le Système solaire, l'Univers et les mathématiques de l'espace");
            A("Menu size", "메뉴 크기", "メニューサイズ", "Tamaño del menú", "Taille du menu");
            A("JUMP TO", "바로 가기", "ジャンプ", "IR A", "ALLER À");
            A("< Back", "< 뒤로", "< 戻る", "< Atrás", "< Retour");
            A("Orbit library music", "오빗 도서관 음악", "オービット図書館の音楽", "Música de la biblioteca Orbit", "Musique de la bibliothèque Orbit");
            // ---- the card
            A("Say “Orbit” or press Ask Orbit to ask a question.", "“오빗”이라고 말하거나 오빗에게 묻기를 눌러 질문하세요.", "「オービット」と言うか、オービットに聞くを押して質問してください。", "Di «Orbit» o pulsa Pregunta a Orbit para hacer una pregunta.", "Dites « Orbit » ou appuyez sur Demander à Orbit pour poser une question.");
            A("I'm listening. Ask me anything about {0}, or about space.", "듣고 있어요. {0}에 대해서나 우주에 대해 무엇이든 물어보세요.", "聞いています。{0}のことでも、宇宙のことでも、何でも聞いてください。", "Te escucho. Pregúntame lo que quieras sobre {0}, o sobre el espacio.", "Je vous écoute. Demandez-moi ce que vous voulez sur {0}, ou sur l'espace.");
            A("My microphone isn't set up yet, so I can't hear you. The card shows everything about what is on the deck.", "아직 마이크가 설정되지 않아 말씀을 들을 수 없어요. 홀로덱에 있는 것에 대한 모든 내용은 카드에 나와 있어요.", "マイクがまだ設定されていないので、声が聞こえません。ホロデッキに映っているものについては、カードにすべて載っています。", "Mi micrófono aún no está configurado, así que no puedo oírte. La tarjeta muestra todo sobre lo que hay en la holocubierta.", "Mon micro n'est pas encore réglé, je ne peux donc pas vous entendre. La carte montre tout ce qu'il faut savoir sur ce qui est sur l'holopont.");
            // ---- Orbit's offline answers (OrbitAssistant)
            A("{0}: {1}", "{0}: {1}", "{0}：{1}", "{0}: {1}", "{0} : {1}");
            A("{0}, a moon of {1}: {2} It is {3} km across.", "{1}의 위성 {0}: {2} 지름은 {3}km입니다.", "{1}の衛星、{0}：{2} 直径は{3} kmです。", "{0}, una luna de {1}: {2} Mide {3} km de diámetro.", "{0}, une lune de {1} : {2} Elle mesure {3} km de diamètre.");
            A("Here's one about {0}: {1}.", "{0}에 관한 사실 하나: {1}.", "{0}について一つ：{1}。", "Un dato sobre {0}: {1}.", "En voici une sur {0} : {1}.");
            A("Here are the parts of {0}. Pinch or grab any piece to look closer; it floats back when you let go.", "{0}의 부품들이에요. 아무 부품이나 집거나 잡아서 자세히 보세요. 놓으면 제자리로 돌아가요.", "{0}のパーツです。どれでもつまむかつかんで近くで見てください。離すと元の場所に戻ります。", "Estas son las piezas de {0}. Pellizca o agarra cualquiera para verla de cerca; vuelve a su sitio al soltarla.", "Voici les pièces de {0}. Pincez ou saisissez-en une pour la regarder de près ; elle revient à sa place quand vous la lâchez.");
            A("This is {0}, by {1}: {2}. Tap Parts to open it up.", "이것은 {1}의 {0}, {2}입니다. 부품을 눌러 열어 보세요.", "これは{1}の{0}、{2}です。パーツを押すと分解して見られます。", "Te presento {0}, de {1}: {2}. Toca Piezas para ver su interior.", "Voici {0}, de {1} : {2}. Touchez Pièces pour l'ouvrir.");
            A("The Aretakis instability: on an extremal black hole, one whose charge or spin is as large as it can be, a wave's slope at the horizon never fades. It is conserved, and it makes the next derivative grow without end, even though the wave itself dies away. Stefanos Aretakis proved it in 2011. Open the Math mode's Aretakis piece to watch it happen.",
                "아레타키스 불안정성: 전하나 자전이 가능한 최대치인 극한 블랙홀에서는 지평선에서 파동의 기울기가 결코 사라지지 않습니다. 이 기울기는 보존되며, 파동 자체는 잦아들어도 그다음 도함수는 끝없이 커집니다. 스테파노스 아레타키스가 2011년에 증명했습니다. 수학 모드의 아레타키스 항목을 열어 직접 보세요.",
                "アレタキス不安定性：電荷や自転が限界まで大きい極限ブラックホールでは、地平面での波の傾きが決して消えません。傾きは保存され、波そのものは消えていっても、次の導関数は際限なく大きくなります。ステファノス・アレタキスが2011年に証明しました。数学モードのアレタキスの項目を開くと、その様子を見られます。",
                "La inestabilidad de Aretakis: en un agujero negro extremo, cuya carga o rotación es la máxima posible, la pendiente de una onda en el horizonte nunca se desvanece. Se conserva, y hace que la siguiente derivada crezca sin fin, aunque la onda misma se apague. Stefanos Aretakis lo demostró en 2011. Abre la pieza de Aretakis en el modo Matemáticas para verlo.",
                "L'instabilité d'Aretakis : sur un trou noir extrémal, dont la charge ou la rotation est aussi grande que possible, la pente d'une onde à l'horizon ne s'efface jamais. Elle est conservée, et elle fait croître sans fin la dérivée suivante, même si l'onde elle-même s'éteint. Stefanos Aretakis l'a démontré en 2011. Ouvrez la pièce Aretakis du mode Maths pour le voir.");
            A("A Penrose diagram fits all of a spacetime on one page: time runs up, light always travels at 45 degrees, and infinity is squeezed onto the edges. For a black hole it shows why nothing that crosses the horizon can get out: every future path inside ends at the singularity.",
                "펜로즈 다이어그램은 시공간 전체를 한 장에 담습니다. 시간은 위로 흐르고, 빛은 언제나 45도로 나아가며, 무한대는 가장자리로 압축됩니다. 블랙홀에서는 지평선을 넘은 것이 왜 빠져나올 수 없는지 보여 줍니다. 안쪽의 모든 미래 경로가 특이점에서 끝나기 때문입니다.",
                "ペンローズ図は、時空全体を1枚に収めます。時間は上へ進み、光は常に45度で進み、無限遠は縁に押し込められます。ブラックホールでは、地平面を越えたものがなぜ外に出られないのかを示します。内側の未来への道はすべて特異点で終わるのです。",
                "Un diagrama de Penrose reúne todo un espacio-tiempo en una página: el tiempo va hacia arriba, la luz siempre viaja a 45 grados y el infinito se comprime en los bordes. Para un agujero negro muestra por qué nada que cruce el horizonte puede salir: todo camino futuro en su interior termina en la singularidad.",
                "Un diagramme de Penrose fait tenir tout un espace-temps sur une page : le temps monte, la lumière va toujours à 45 degrés et l'infini est comprimé sur les bords. Pour un trou noir, il montre pourquoi rien de ce qui franchit l'horizon ne peut ressortir : tout chemin futur à l'intérieur aboutit à la singularité.");
            A("A black hole is where so much mass is packed so tightly that not even light can climb out. Its edge, the event horizon, is not a surface but a point of no return: for a hole like our Sun's mass it would be about 3 km in radius. The Math mode shows the curved space round one, and its Penrose diagram.",
                "블랙홀은 엄청난 질량이 너무 빽빽하게 뭉쳐 빛조차 빠져나올 수 없는 곳입니다. 그 경계인 사건의 지평선은 표면이 아니라 돌아올 수 없는 지점입니다. 태양 질량의 블랙홀이라면 반지름이 약 3km일 것입니다. 수학 모드에서 블랙홀 주변의 휘어진 공간과 펜로즈 다이어그램을 볼 수 있습니다.",
                "ブラックホールとは、膨大な質量がぎゅっと詰め込まれ、光さえ抜け出せない場所です。その縁である事象の地平面は表面ではなく、引き返せない境界です。太陽と同じ質量なら、半径は約3 kmになります。数学モードでは、その周りの曲がった空間とペンローズ図を見られます。",
                "Un agujero negro es un lugar donde tanta masa está tan apretada que ni siquiera la luz puede escapar. Su borde, el horizonte de sucesos, no es una superficie sino un punto sin retorno: para uno con la masa de nuestro Sol tendría unos 3 km de radio. El modo Matemáticas muestra el espacio curvado a su alrededor y su diagrama de Penrose.",
                "Un trou noir, c'est un endroit où tant de masse est si concentrée que même la lumière ne peut s'en échapper. Son bord, l'horizon des événements, n'est pas une surface mais un point de non-retour : pour un trou de la masse de notre Soleil, il ferait environ 3 km de rayon. Le mode Maths montre l'espace courbé autour de lui, et son diagramme de Penrose.");

            // ---- Voyager 1
            A("Voyager 1", "보이저 1호", "ボイジャー1号", "Voyager 1", "Voyager 1");
            A("The farthest human-made object", "인류가 만든 가장 먼 물체", "人類が作った最も遠い物体", "El objeto hecho por el ser humano más lejano", "L'objet fabriqué par l'homme le plus lointain");
            A("Launched on 5 September 1977 on a Titan IIIE-Centaur, Voyager 1 flew past Jupiter in March 1979 and Saturn in November 1980, then climbed out of the plane of the planets. In August 2012 it crossed the heliopause and became the first spacecraft in interstellar space. It is now more than 24 billion kilometres away: its radio signal takes over 22 hours to reach Earth, arriving at about 160 bits per second.",
                "1977년 9월 5일 타이탄 IIIE-센타우르 로켓에 실려 발사된 보이저 1호는 1979년 3월 목성을, 1980년 11월 토성을 지난 뒤 행성들이 도는 평면 밖으로 올라갔습니다. 2012년 8월에는 태양권계면을 넘어 성간 공간에 들어간 최초의 우주선이 되었습니다. 지금은 240억 km 넘게 떨어져 있어 전파 신호가 지구에 닿기까지 22시간이 넘게 걸리며, 초당 약 160비트로 도착합니다.",
                "1977年9月5日にタイタンIIIE・セントールで打ち上げられたボイジャー1号は、1979年3月に木星、1980年11月に土星のそばを通過し、惑星の公転面から離れていきました。2012年8月にはヘリオポーズを越え、星間空間に到達した最初の宇宙機となりました。現在は240億km以上離れており、電波が地球に届くまで22時間以上かかり、毎秒約160ビットで届きます。",
                "Lanzada el 5 de septiembre de 1977 en un Titan IIIE-Centaur, la Voyager 1 pasó junto a Júpiter en marzo de 1979 y Saturno en noviembre de 1980, y luego salió del plano de los planetas. En agosto de 2012 cruzó la heliopausa y se convirtió en la primera nave en el espacio interestelar. Hoy está a más de 24 000 millones de kilómetros: su señal de radio tarda más de 22 horas en llegar a la Tierra, a unos 160 bits por segundo.",
                "Lancée le 5 septembre 1977 par une Titan IIIE-Centaur, Voyager 1 a survolé Jupiter en mars 1979 et Saturne en novembre 1980, puis a quitté le plan des planètes. En août 2012, elle a franchi l'héliopause et est devenue le premier engin dans l'espace interstellaire. Elle est aujourd'hui à plus de 24 milliards de kilomètres : son signal radio met plus de 22 heures à atteindre la Terre, à environ 160 bits par seconde.");
            A("Launch: 5 September 1977, Cape Canaveral", "발사: 1977년 9월 5일, 케이프커내버럴", "打ち上げ：1977年9月5日、ケープカナベラル", "Lanzamiento: 5 de septiembre de 1977, Cabo Cañaveral", "Lancement : 5 septembre 1977, cap Canaveral");
            A("Mass at launch: 825 kg", "발사 질량: 825kg", "打ち上げ時の質量：825 kg", "Masa al lanzamiento: 825 kg", "Masse au lancement : 825 kg");
            A("Power: three radioisotope generators, 470 W at launch, slowly fading", "전력: 방사성 동위원소 발전기 3기, 발사 시 470W, 서서히 감소 중", "電力：原子力電池3基、打ち上げ時470 W、徐々に低下中", "Energía: tres generadores de radioisótopos, 470 W al lanzamiento, en lento declive", "Énergie : trois générateurs à radio-isotopes, 470 W au lancement, en lente baisse");
            A("Antenna: 3.7 m high-gain dish", "안테나: 3.7m 고이득 접시 안테나", "アンテナ：直径3.7 mの高利得パラボラ", "Antena: plato de alta ganancia de 3,7 m", "Antenne : parabole à grand gain de 3,7 m");
            A("Interstellar space since 25 August 2012", "2012년 8월 25일부터 성간 공간에", "2012年8月25日から星間空間に", "En el espacio interestelar desde el 25 de agosto de 2012", "Dans l'espace interstellaire depuis le 25 août 2012");
            A("Distance: over 24 billion km, more than 160 times the Earth–Sun distance", "거리: 240억 km 이상, 지구–태양 거리의 160배가 넘음", "距離：240億km以上、地球と太陽の距離の160倍以上", "Distancia: más de 24 000 millones de km, más de 160 veces la distancia Tierra–Sol", "Distance : plus de 24 milliards de km, plus de 160 fois la distance Terre–Soleil");
            A("High-gain antenna", "고이득 안테나", "高利得アンテナ", "Antena de alta ganancia", "Antenne à grand gain");
            A("A 3.7-metre dish that always points at Earth. It talks in S-band and X-band; at launch it sent 115 kilobits per second from Jupiter, and from interstellar space it manages about 160 bits per second, caught by NASA's Deep Space Network dishes.",
                "항상 지구를 향하는 지름 3.7m의 접시 안테나입니다. S밴드와 X밴드로 통신하며, 처음에는 목성에서 초당 115킬로비트를 보냈고, 성간 공간에서는 초당 약 160비트를 보내 NASA 심우주 통신망의 안테나가 받습니다.",
                "常に地球を向いている直径3.7 mのパラボラアンテナ。SバンドとXバンドで通信し、当初は木星から毎秒115キロビットを送っていました。星間空間からは毎秒約160ビットで、NASAの深宇宙ネットワークのアンテナが受信しています。",
                "Un plato de 3,7 metros que siempre apunta a la Tierra. Se comunica en banda S y banda X; al principio enviaba 115 kilobits por segundo desde Júpiter, y desde el espacio interestelar logra unos 160 bits por segundo, captados por las antenas de la Red de Espacio Profundo de la NASA.",
                "Une parabole de 3,7 mètres toujours pointée vers la Terre. Elle communique en bande S et en bande X ; au début, elle envoyait 115 kilobits par seconde depuis Jupiter, et depuis l'espace interstellaire elle atteint environ 160 bits par seconde, captés par les antennes du Deep Space Network de la NASA.");
            A("Spacecraft bus", "우주선 본체", "機体本体（バス）", "Bus de la nave", "Plateforme de l'engin");
            A("A ten-sided aluminium ring 1.78 m across and 47 cm high. Its bays hold the computers, radios and power electronics, and the spherical hydrazine tank sits in the middle. The whole spacecraft is built around it.",
                "지름 1.78m, 높이 47cm의 십각형 알루미늄 고리입니다. 칸마다 컴퓨터, 무선 장비, 전력 전자 장치가 들어 있고, 가운데에는 공 모양의 하이드라진 탱크가 있습니다. 우주선 전체가 이것을 중심으로 만들어졌습니다.",
                "直径1.78 m、高さ47 cmの十角形のアルミニウム製リング。区画にはコンピューター、無線機、電源装置が収まり、中央には球形のヒドラジンタンクがあります。機体全体がこれを中心に組み立てられています。",
                "Un anillo de aluminio de diez lados, de 1,78 m de ancho y 47 cm de alto. Sus compartimentos alojan los ordenadores, las radios y la electrónica de potencia, y el tanque esférico de hidracina está en el centro. Toda la nave está construida a su alrededor.",
                "Un anneau d'aluminium à dix côtés, de 1,78 m de large et 47 cm de haut. Ses baies abritent les ordinateurs, les radios et l'électronique de puissance, et le réservoir sphérique d'hydrazine se trouve au milieu. Tout l'engin est construit autour.");
            A("Radioisotope generators", "방사성 동위원소 발전기", "原子力電池（RTG）", "Generadores de radioisótopos", "Générateurs à radio-isotopes");
            A("Three radioisotope thermoelectric generators on a boom, turning the heat of decaying plutonium-238 into electricity. They made 470 watts at launch and lose about four watts a year, which is why instruments are being switched off one by one.",
                "붐 끝에 달린 방사성 동위원소 열전기 발전기 3기로, 붕괴하는 플루토늄-238의 열을 전기로 바꿉니다. 발사 때 470와트를 냈고 해마다 약 4와트씩 줄어들어, 관측 장비를 하나씩 끄고 있습니다.",
                "ブームの先に付いた3基の放射性同位体熱電気転換器で、崩壊するプルトニウム238の熱を電気に変えます。打ち上げ時は470ワットでしたが、毎年約4ワットずつ減っているため、観測機器を一つずつ止めています。",
                "Tres generadores termoeléctricos de radioisótopos en un brazo, que convierten en electricidad el calor del plutonio-238 al desintegrarse. Daban 470 vatios al lanzamiento y pierden unos cuatro vatios al año; por eso se van apagando los instrumentos uno a uno.",
                "Trois générateurs thermoélectriques à radio-isotopes sur un mât, qui transforment en électricité la chaleur du plutonium 238 qui se désintègre. Ils fournissaient 470 watts au lancement et perdent environ quatre watts par an : c'est pourquoi les instruments sont éteints un à un.");
            A("Science boom and scan platform", "과학 붐과 스캔 플랫폼", "科学ブームとスキャンプラットフォーム", "Brazo científico y plataforma de exploración", "Mât scientifique et plateforme de balayage");
            A("A 2.3-metre boom carrying the scan platform: the narrow- and wide-angle cameras that took the famous planet portraits and the Pale Blue Dot, the infrared and ultraviolet spectrometers, and the photopolarimeter.",
                "스캔 플랫폼을 단 2.3m 길이의 붐입니다. 유명한 행성 사진과 ‘창백한 푸른 점’을 찍은 협각·광각 카메라, 적외선·자외선 분광기, 광편광계가 실려 있습니다.",
                "スキャンプラットフォームを載せた長さ2.3 mのブーム。有名な惑星の写真や「ペイル・ブルー・ドット」を撮った狭角・広角カメラ、赤外線・紫外線分光計、光偏光計を搭載しています。",
                "Un brazo de 2,3 metros con la plataforma de exploración: las cámaras de ángulo estrecho y gran angular que tomaron los famosos retratos de los planetas y el Punto Azul Pálido, los espectrómetros infrarrojo y ultravioleta, y el fotopolarímetro.",
                "Un mât de 2,3 mètres portant la plateforme de balayage : les caméras grand-angle et à champ étroit qui ont pris les célèbres portraits des planètes et le Point bleu pâle, les spectromètres infrarouge et ultraviolet, et le photopolarimètre.");
            A("Magnetometer boom", "자력계 붐", "磁力計ブーム", "Brazo del magnetómetro", "Mât du magnétomètre");
            A("A 13-metre fibreglass boom that unfolded after launch, holding four magnetometers well away from the spacecraft's own magnetic field. These are the instruments that still measure the interstellar magnetic field today.",
                "발사 후 펼쳐진 13m 길이의 유리섬유 붐으로, 자력계 4개를 우주선 자체의 자기장에서 멀리 떨어뜨려 줍니다. 지금도 성간 자기장을 측정하고 있는 장비입니다.",
                "打ち上げ後に展開した長さ13 mのグラスファイバー製ブームで、4台の磁力計を機体自身の磁場から遠ざけています。今も星間磁場を測り続けている観測機器です。",
                "Un brazo de fibra de vidrio de 13 metros que se desplegó tras el lanzamiento y mantiene cuatro magnetómetros lejos del propio campo magnético de la nave. Son los instrumentos que aún hoy miden el campo magnético interestelar.",
                "Un mât en fibre de verre de 13 mètres, déployé après le lancement, qui tient quatre magnétomètres loin du champ magnétique de l'engin lui-même. Ce sont eux qui mesurent encore aujourd'hui le champ magnétique interstellaire.");
            A("Plasma wave antennas", "플라스마파 안테나", "プラズマ波アンテナ", "Antenas de ondas de plasma", "Antennes d'ondes de plasma");
            A("Two 10-metre whip antennas in a V, shared by the planetary radio astronomy and plasma wave instruments. The plasma wave instrument heard the 'hum' of interstellar plasma and still reports the density of the space around the craft.",
                "V자로 뻗은 10m 길이의 채찍 안테나 2개로, 행성 전파 천문 장비와 플라스마파 장비가 함께 씁니다. 플라스마파 장비는 성간 플라스마의 ‘웅웅거림’을 들었고, 지금도 우주선 주변 공간의 밀도를 알려 줍니다.",
                "V字形に伸びた長さ10 mのホイップアンテナ2本で、惑星電波天文観測装置とプラズマ波観測装置が共用しています。プラズマ波観測装置は星間プラズマの「ハム音」をとらえ、今も機体周辺の空間の密度を伝えています。",
                "Dos antenas de látigo de 10 metros en forma de V, compartidas por los instrumentos de radioastronomía planetaria y de ondas de plasma. El instrumento de ondas de plasma oyó el 'zumbido' del plasma interestelar y aún informa de la densidad del espacio alrededor de la nave.",
                "Deux antennes fouets de 10 mètres en V, partagées par les instruments de radioastronomie planétaire et d'ondes de plasma. L'instrument d'ondes de plasma a entendu le « bourdonnement » du plasma interstellaire et indique encore la densité de l'espace autour de l'engin.");
            A("Golden Record", "골든 레코드", "ゴールデンレコード", "Disco de Oro", "Disque d'or");
            A("A 12-inch gold-plated copper phonograph record bolted to the bus, with 115 images, greetings in 55 languages, sounds of Earth and 90 minutes of music. Its cover shows, in binary, how to play it and where Earth is.",
                "본체에 볼트로 고정된 12인치 금도금 구리 음반으로, 이미지 115장, 55개 언어의 인사말, 지구의 소리, 90분 분량의 음악이 담겨 있습니다. 표지에는 재생 방법과 지구의 위치가 이진법으로 그려져 있습니다.",
                "本体にボルトで留められた12インチの金メッキ銅製レコードで、115枚の画像、55の言語によるあいさつ、地球の音、90分の音楽が収められています。カバーには再生方法と地球の位置が2進法で示されています。",
                "Un disco de fonógrafo de cobre chapado en oro de 12 pulgadas, atornillado al bus, con 115 imágenes, saludos en 55 idiomas, sonidos de la Tierra y 90 minutos de música. Su cubierta muestra, en binario, cómo reproducirlo y dónde está la Tierra.",
                "Un disque phonographique de 12 pouces en cuivre plaqué or, boulonné à la plateforme, avec 115 images, des salutations en 55 langues, des sons de la Terre et 90 minutes de musique. Sa pochette indique, en binaire, comment le lire et où se trouve la Terre.");
            A("Attitude thrusters", "자세 제어 추력기", "姿勢制御スラスター", "Propulsores de orientación", "Propulseurs d'attitude");
            A("Sixteen small hydrazine thrusters keep the antenna pointed at Earth and steady the craft. Some had not fired since 1980 when engineers woke them in 2017 to take over from worn-out primaries, and they worked.",
                "작은 하이드라진 추력기 16개가 안테나를 지구 쪽으로 유지하고 우주선을 안정시킵니다. 그중 일부는 1980년 이후 한 번도 점화되지 않았는데, 2017년 기술자들이 낡은 주 추력기를 대신하도록 깨웠고, 제대로 작동했습니다.",
                "16基の小型ヒドラジンスラスターが、アンテナを地球に向け、機体を安定させます。1980年以来一度も噴射していなかったものもありましたが、2017年に技術者たちが摩耗した主スラスターの代わりに目覚めさせると、ちゃんと動きました。",
                "Dieciséis pequeños propulsores de hidracina mantienen la antena apuntando a la Tierra y estabilizan la nave. Algunos no se encendían desde 1980 cuando los ingenieros los despertaron en 2017 para sustituir a los principales, ya desgastados, y funcionaron.",
                "Seize petits propulseurs à hydrazine gardent l'antenne pointée vers la Terre et stabilisent l'engin. Certains n'avaient pas fonctionné depuis 1980 quand les ingénieurs les ont réveillés en 2017 pour remplacer les propulseurs principaux usés, et ils ont marché.");

            // ---- Hubble Space Telescope
            A("Hubble Space Telescope", "허블 우주 망원경", "ハッブル宇宙望遠鏡", "Telescopio espacial Hubble", "Télescope spatial Hubble");
            A("An observatory in low Earth orbit since 1990", "1990년부터 지구 저궤도에 있는 천문대", "1990年から地球低軌道にある天文台", "Un observatorio en órbita terrestre baja desde 1990", "Un observatoire en orbite terrestre basse depuis 1990");
            A("Hubble was carried to orbit by the shuttle Discovery on 24 April 1990 and deployed the next day. It is 13.2 m long and 4.2 m across, about the size of a school bus, and circles Earth every 95 minutes at roughly 540 km. Its 2.4-metre mirror was ground slightly wrong; a corrective package fitted in December 1993 fixed the blur, and five servicing missions kept it at the front of astronomy.",
                "허블은 1990년 4월 24일 우주왕복선 디스커버리호에 실려 궤도에 올랐고, 다음 날 배치되었습니다. 길이 13.2m, 지름 4.2m로 스쿨버스만 하며, 약 540km 높이에서 95분마다 지구를 한 바퀴 돕니다. 2.4m 거울이 살짝 잘못 연마되었지만 1993년 12월 설치한 보정 장치로 흐림을 고쳤고, 다섯 번의 수리 임무 덕분에 천문학의 최전선을 지켜 왔습니다.",
                "ハッブルは1990年4月24日にスペースシャトル・ディスカバリーで軌道へ運ばれ、翌日放出されました。長さ13.2 m、直径4.2 mとスクールバスほどの大きさで、高度約540 kmを95分で地球を一周します。直径2.4 mの鏡はわずかに誤って研磨されていましたが、1993年12月に取り付けた補正装置でぼやけが直り、5回の修理ミッションで天文学の最前線にあり続けました。",
                "El Hubble fue llevado a órbita por el transbordador Discovery el 24 de abril de 1990 y se desplegó al día siguiente. Mide 13,2 m de largo y 4,2 m de ancho, como un autobús escolar, y da una vuelta a la Tierra cada 95 minutos a unos 540 km. Su espejo de 2,4 metros se pulió con un ligero error; un sistema corrector instalado en diciembre de 1993 corrigió el desenfoque, y cinco misiones de servicio lo mantuvieron a la vanguardia de la astronomía.",
                "Hubble a été mis en orbite par la navette Discovery le 24 avril 1990 et déployé le lendemain. Long de 13,2 m pour 4,2 m de diamètre, soit la taille d'un bus scolaire, il fait le tour de la Terre en 95 minutes à environ 540 km. Son miroir de 2,4 mètres avait été poli avec une légère erreur ; une optique correctrice installée en décembre 1993 a corrigé le flou, et cinq missions de maintenance l'ont gardé à la pointe de l'astronomie.");
            A("Launch: 24 April 1990, shuttle Discovery, STS-31", "발사: 1990년 4월 24일, 우주왕복선 디스커버리호, STS-31", "打ち上げ：1990年4月24日、スペースシャトル・ディスカバリー、STS-31", "Lanzamiento: 24 de abril de 1990, transbordador Discovery, STS-31", "Lancement : 24 avril 1990, navette Discovery, STS-31");
            A("Length 13.2 m, diameter 4.2 m, about 11 tonnes at launch", "길이 13.2m, 지름 4.2m, 발사 시 약 11톤", "長さ13.2 m、直径4.2 m、打ち上げ時約11トン", "Longitud 13,2 m, diámetro 4,2 m, unas 11 toneladas al lanzamiento", "Longueur 13,2 m, diamètre 4,2 m, environ 11 tonnes au lancement");
            A("Orbit: about 540 km, one circuit every 95 minutes", "궤도: 약 540km, 95분에 한 바퀴", "軌道：高度約540 km、95分で一周", "Órbita: unos 540 km, una vuelta cada 95 minutos", "Orbite : environ 540 km, un tour toutes les 95 minutes");
            A("Primary mirror 2.4 m across", "주경 지름 2.4m", "主鏡の直径2.4 m", "Espejo primario de 2,4 m de diámetro", "Miroir primaire de 2,4 m de diamètre");
            A("Five servicing missions, 1993 to 2009", "수리 임무 5회, 1993~2009년", "修理ミッション5回（1993〜2009年）", "Cinco misiones de servicio, de 1993 a 2009", "Cinq missions de maintenance, de 1993 à 2009");
            A("Points steadily to 7 thousandths of an arcsecond", "1000분의 7초각 정밀도로 안정되게 조준", "1000分の7秒角の精度で安定して指向", "Apunta con una estabilidad de 7 milésimas de segundo de arco", "Vise avec une stabilité de 7 millièmes de seconde d'arc");
            A("Aperture door", "개구부 덮개", "開口部ドア", "Puerta de apertura", "Porte d'ouverture");
            A("A hinged lid at the front that closes if the telescope ever swings toward the Sun, so sunlight can never reach the mirrors. It stuck open for a while after launch and has since worked faithfully.",
                "망원경 앞쪽의 경첩 달린 덮개로, 망원경이 태양 쪽으로 돌면 닫혀 햇빛이 거울에 닿지 못하게 합니다. 발사 후 한동안 열린 채 꼼짝하지 않았지만, 그 뒤로는 충실히 작동해 왔습니다.",
                "前面にある蝶番付きのふたで、望遠鏡が太陽の方を向きそうになると閉じ、日光が鏡に届かないようにします。打ち上げ後しばらく開いたまま動かなくなりましたが、その後はずっと忠実に働いています。",
                "Una tapa con bisagra en la parte delantera que se cierra si el telescopio llega a girar hacia el Sol, para que la luz solar nunca alcance los espejos. Se quedó atascada abierta un tiempo tras el lanzamiento y desde entonces ha funcionado fielmente.",
                "Un couvercle à charnière à l'avant, qui se ferme si le télescope pivote vers le Soleil, pour que sa lumière n'atteigne jamais les miroirs. Il est resté coincé ouvert un temps après le lancement et fonctionne fidèlement depuis.");
            A("Light shield", "차광 통", "遮光フード", "Parasol", "Pare-lumière");
            A("The forward tube, lined with baffles that soak up stray light so only starlight from the aimed direction reaches the mirror. Hubble looks like a telescope tube, and this is the tube.",
                "앞쪽 통으로, 안쪽의 배플이 엉뚱한 빛을 흡수해 겨눈 방향의 별빛만 거울에 닿게 합니다. 허블은 망원경 통처럼 생겼는데, 바로 이 부분이 그 통입니다.",
                "前方の筒で、内側のバッフルが迷光を吸収し、狙った方向の星の光だけが鏡に届くようにします。ハッブルは望遠鏡の筒のような形をしていますが、これがその筒です。",
                "El tubo delantero, forrado de deflectores que absorben la luz parásita para que solo la luz de las estrellas en la dirección apuntada llegue al espejo. El Hubble parece un tubo de telescopio, y este es el tubo.",
                "Le tube avant, tapissé de déflecteurs qui absorbent la lumière parasite pour que seule la lumière des étoiles visées atteigne le miroir. Hubble ressemble à un tube de télescope, et voici ce tube.");
            A("Secondary mirror", "부경", "副鏡", "Espejo secundario", "Miroir secondaire");
            A("A 30-centimetre convex mirror held on a truss 4.9 m in front of the primary. It bounces the light back through a hole in the primary mirror to the instruments behind, the Ritchey–Chrétien arrangement.",
                "주경 앞 4.9m 지점에 트러스로 고정된 지름 30cm의 볼록 거울입니다. 빛을 다시 주경 가운데 구멍으로 반사해 뒤쪽 관측 장비로 보내는데, 이것이 리치-크레티앙 방식입니다.",
                "主鏡の4.9 m前方にトラスで支えられた直径30 cmの凸面鏡。光を主鏡の中央の穴へ跳ね返し、後ろの観測装置に送ります。これがリッチー・クレチアン式の配置です。",
                "Un espejo convexo de 30 centímetros sostenido por una estructura a 4,9 m delante del primario. Devuelve la luz a través de un orificio del espejo primario hacia los instrumentos de detrás: la configuración Ritchey–Chrétien.",
                "Un miroir convexe de 30 centimètres, tenu par une structure à 4,9 m devant le primaire. Il renvoie la lumière à travers un trou du miroir primaire vers les instruments situés derrière : c'est la configuration Ritchey–Chrétien.");
            A("Primary mirror", "주경", "主鏡", "Espejo primario", "Miroir primaire");
            A("The 2.4-metre, 828-kilogram main mirror, polished so smoothly that if it were scaled to the width of the Earth the biggest bump would be 15 cm high. Its edge was ground 2.2 micrometres too flat, the flaw that corrective optics fixed in 1993.",
                "지름 2.4m, 무게 828kg의 주 거울로, 지구만 한 크기로 키워도 가장 큰 요철이 15cm에 불과할 만큼 매끈하게 연마되었습니다. 가장자리가 2.2마이크로미터 너무 평평하게 깎였는데, 이 결함을 1993년 보정 광학 장치가 고쳤습니다.",
                "直径2.4 m、重さ828 kgの主鏡。地球の大きさに拡大しても最大の凹凸が15 cmにしかならないほど滑らかに磨かれています。縁が2.2マイクロメートル平らすぎたことが欠陥で、1993年に補正光学系で直されました。",
                "El espejo principal, de 2,4 metros y 828 kilogramos, pulido con tanta suavidad que, si se ampliara al ancho de la Tierra, la mayor irregularidad mediría 15 cm. Su borde quedó 2,2 micrómetros demasiado plano, el defecto que la óptica correctora arregló en 1993.",
                "Le miroir principal, de 2,4 mètres et 828 kilogrammes, poli si finement qu'à l'échelle de la Terre sa plus grosse bosse ne mesurerait que 15 cm. Son bord a été poli 2,2 micromètres trop plat : le défaut que l'optique correctrice a réparé en 1993.");
            A("Solar arrays", "태양 전지판", "太陽電池パドル", "Paneles solares", "Panneaux solaires");
            A("Two wings of solar cells, the third pair fitted in 2002, each about 7.6 by 2.6 metres. Together they make around 5.7 kilowatts and charge six nickel-hydrogen batteries for the dark half of each orbit.",
                "태양 전지 날개 두 장으로, 2002년에 단 세 번째 한 쌍이며 각각 약 7.6×2.6m입니다. 함께 약 5.7킬로와트를 만들어, 궤도의 어두운 절반 동안 쓸 니켈-수소 배터리 6개를 충전합니다.",
                "2枚の太陽電池パドルで、2002年に取り付けられた3代目。それぞれ約7.6×2.6 mです。合わせて約5.7キロワットを発電し、軌道の暗い半周に備えて6個のニッケル水素電池を充電します。",
                "Dos alas de células solares, el tercer par, instalado en 2002, de unos 7,6 por 2,6 metros cada una. Juntas producen unos 5,7 kilovatios y cargan seis baterías de níquel-hidrógeno para la mitad oscura de cada órbita.",
                "Deux ailes de cellules solaires, la troisième paire installée en 2002, d'environ 7,6 sur 2,6 mètres chacune. Ensemble, elles produisent environ 5,7 kilowatts et chargent six batteries nickel-hydrogène pour la moitié obscure de chaque orbite.");
            A("High-gain antennas", "고이득 안테나", "高利得アンテナ", "Antenas de alta ganancia", "Antennes à grand gain");
            A("Two 1.3-metre dishes on booms that relay data through NASA's TDRS satellites at up to a megabit per second: Hubble's pictures come down this way, several times a day.",
                "붐에 달린 지름 1.3m 접시 안테나 2개로, NASA의 TDRS 위성을 거쳐 초당 최대 1메가비트로 데이터를 중계합니다. 허블의 사진은 하루에도 여러 번 이 길로 내려옵니다.",
                "ブームに付いた直径1.3 mのパラボラ2基で、NASAのTDRS衛星を経由して毎秒最大1メガビットでデータを中継します。ハッブルの画像は1日に何度もこの経路で届きます。",
                "Dos platos de 1,3 metros en brazos que transmiten datos a través de los satélites TDRS de la NASA a hasta un megabit por segundo: así bajan las imágenes del Hubble, varias veces al día.",
                "Deux paraboles de 1,3 mètre sur des mâts, qui relaient les données via les satellites TDRS de la NASA jusqu'à un mégabit par seconde : c'est ainsi que descendent les images de Hubble, plusieurs fois par jour.");
            A("Equipment section", "장비부", "機器セクション", "Sección de equipos", "Section des équipements");
            A("The ring around the mirror holding what keeps Hubble alive and pointed: gyroscopes, four reaction wheels that turn it without any thruster, the batteries, computers and radios. Astronauts swapped parts here on every servicing mission.",
                "거울을 둘러싼 고리로, 허블을 살아 있게 하고 방향을 잡아 주는 장치가 들어 있습니다. 자이로스코프, 추력기 없이 망원경을 돌리는 반작용 휠 4개, 배터리, 컴퓨터, 무선 장비입니다. 우주비행사들은 수리 임무 때마다 여기서 부품을 교체했습니다.",
                "鏡を囲むリングで、ハッブルを動かし続け、向きを保つ装置が入っています。ジャイロスコープ、スラスターなしで向きを変える4基のリアクションホイール、電池、コンピューター、無線機です。宇宙飛行士は修理ミッションのたびにここで部品を交換しました。",
                "El anillo alrededor del espejo que guarda lo que mantiene vivo y orientado al Hubble: giroscopios, cuatro ruedas de reacción que lo giran sin ningún propulsor, las baterías, los ordenadores y las radios. Los astronautas cambiaron piezas aquí en cada misión de servicio.",
                "L'anneau autour du miroir qui abrite ce qui maintient Hubble en vie et pointé : gyroscopes, quatre roues de réaction qui le font tourner sans aucun propulseur, batteries, ordinateurs et radios. Les astronautes y ont échangé des pièces à chaque mission de maintenance.");
            A("Aft shroud and instruments", "후방 덮개와 관측 장비", "後部シュラウドと観測装置", "Carcasa trasera e instrumentos", "Carénage arrière et instruments");
            A("The rear section where the science instruments sit in bays behind the mirror: Wide Field Camera 3, the Cosmic Origins Spectrograph, the imaging spectrograph and the Advanced Camera for Surveys, with three fine guidance sensors that lock onto guide stars.",
                "거울 뒤 칸마다 과학 장비가 들어 있는 뒤쪽 부분입니다. 광시야 카메라 3, 우주 기원 분광기, 영상 분광기, 탐사용 고급 카메라가 있고, 안내별을 붙잡는 정밀 유도 센서 3개도 있습니다.",
                "鏡の後ろの区画に科学観測装置が並ぶ後部セクション。広視野カメラ3、宇宙起源分光器、撮像分光器、掃天観測用高性能カメラに加え、ガイド星をとらえる3台のファインガイダンスセンサーがあります。",
                "La sección trasera, donde los instrumentos científicos ocupan compartimentos detrás del espejo: la Cámara de Gran Angular 3, el Espectrógrafo de Orígenes Cósmicos, el espectrógrafo de imágenes y la Cámara Avanzada para Sondeos, con tres sensores de guiado fino que se fijan en estrellas guía.",
                "La section arrière, où les instruments scientifiques logent dans des baies derrière le miroir : la Wide Field Camera 3, le Cosmic Origins Spectrograph, le spectrographe imageur et l'Advanced Camera for Surveys, avec trois capteurs de guidage fin qui se verrouillent sur des étoiles guides.");

            // ---- Falcon 9 Block 5
            A("Falcon 9", "팰컨 9", "ファルコン9", "Falcon 9", "Falcon 9");
            A("Falcon 9 Block 5", "팰컨 9 블록 5", "ファルコン9 ブロック5", "Falcon 9 Block 5", "Falcon 9 Block 5");
            A("The first orbital rocket to land and fly again", "착륙한 뒤 다시 날아오른 최초의 궤도 로켓", "着陸して再び飛んだ初の軌道ロケット", "El primer cohete orbital que aterrizó y volvió a volar", "La première fusée orbitale à atterrir et à revoler");
            A("Falcon 9 is a two-stage rocket 70 m tall and 3.7 m wide, burning kerosene and liquid oxygen. Its first stage flies back and lands on legs, on a drone ship at sea or at the launch site; the first landing was on 21 December 2015, and Block 5, flying since May 2018, was designed for rapid reuse. Single boosters have now flown well over twenty missions, carrying Starlink, cargo and crews to the Space Station.",
                "팰컨 9은 높이 70m, 폭 3.7m의 2단 로켓으로, 등유와 액체 산소를 태웁니다. 1단은 되돌아와 바다 위 드론십이나 발사장에 다리를 펴고 착륙합니다. 첫 착륙은 2015년 12월 21일이었고, 2018년 5월부터 비행한 블록 5는 빠른 재사용을 위해 설계되었습니다. 지금은 부스터 하나가 스무 번을 훌쩍 넘게 날며 스타링크 위성, 화물, 그리고 우주정거장으로 가는 승무원을 실어 날랐습니다.",
                "ファルコン9は高さ70 m、幅3.7 mの2段式ロケットで、ケロシンと液体酸素を燃やします。1段目は戻ってきて、海上のドローン船か発射場に脚を広げて着陸します。初の着陸は2015年12月21日で、2018年5月から飛んでいるブロック5は素早い再使用のために設計されました。今では1基のブースターが20回をゆうに超えて飛行し、スターリンク、貨物、そして宇宙ステーションへのクルーを運んでいます。",
                "Falcon 9 es un cohete de dos etapas de 70 m de alto y 3,7 m de ancho que quema queroseno y oxígeno líquido. Su primera etapa regresa y aterriza sobre patas, en un barco dron en el mar o en el lugar de lanzamiento; el primer aterrizaje fue el 21 de diciembre de 2015, y el Block 5, que vuela desde mayo de 2018, se diseñó para reutilizarse rápidamente. Algunos propulsores ya han volado mucho más de veinte misiones, llevando satélites Starlink, carga y tripulaciones a la Estación Espacial.",
                "Falcon 9 est une fusée à deux étages de 70 m de haut et 3,7 m de large, qui brûle du kérosène et de l'oxygène liquide. Son premier étage revient et se pose sur ses jambes, sur une barge en mer ou sur le site de lancement ; le premier atterrissage a eu lieu le 21 décembre 2015, et le Block 5, qui vole depuis mai 2018, a été conçu pour une réutilisation rapide. Certains propulseurs ont désormais volé bien plus de vingt fois, emportant des satellites Starlink, du fret et des équipages vers la Station spatiale.");
            A("Height 70 m, diameter 3.7 m, mass 549 tonnes fuelled", "높이 70m, 지름 3.7m, 연료 주입 시 질량 549톤", "高さ70 m、直径3.7 m、燃料搭載時の質量549トン", "Altura 70 m, diámetro 3,7 m, masa con combustible 549 toneladas", "Hauteur 70 m, diamètre 3,7 m, masse avec ergols 549 tonnes");
            A("Nine Merlin 1D engines: 7,607 kN at sea level", "멀린 1D 엔진 9기: 해수면 기준 7,607kN", "マーリン1Dエンジン9基：海面で7,607 kN", "Nueve motores Merlin 1D: 7607 kN a nivel del mar", "Neuf moteurs Merlin 1D : 7 607 kN au niveau de la mer");
            A("To low Earth orbit: 22.8 tonnes", "지구 저궤도 운반 능력: 22.8톤", "地球低軌道への打ち上げ能力：22.8トン", "A órbita terrestre baja: 22,8 toneladas", "En orbite terrestre basse : 22,8 tonnes");
            A("Propellants: liquid oxygen and rocket-grade kerosene (RP-1)", "추진제: 액체 산소와 로켓용 등유(RP-1)", "推進剤：液体酸素とロケット用ケロシン（RP-1）", "Propelentes: oxígeno líquido y queroseno de grado cohete (RP-1)", "Ergols : oxygène liquide et kérosène de qualité fusée (RP-1)");
            A("First booster landing: 21 December 2015", "첫 부스터 착륙: 2015년 12월 21일", "ブースター初着陸：2015年12月21日", "Primer aterrizaje de un propulsor: 21 de diciembre de 2015", "Premier atterrissage d'un propulseur : 21 décembre 2015");
            A("Block 5 first flew on 11 May 2018", "블록 5 첫 비행: 2018년 5월 11일", "ブロック5の初飛行：2018年5月11日", "El Block 5 voló por primera vez el 11 de mayo de 2018", "Le Block 5 a volé pour la première fois le 11 mai 2018");
            A("Payload fairing", "페이로드 페어링", "フェアリング", "Cofia de la carga útil", "Coiffe");
            A("Two carbon-composite halves, 13.1 m long and 5.2 m across, that shelter the satellite through the atmosphere and split apart about three minutes into flight. They fall back under parachutes and are fished out of the sea to fly again.",
                "길이 13.1m, 지름 5.2m의 탄소 복합재 반쪽 두 개로, 대기를 지나는 동안 위성을 보호하다가 비행 약 3분 만에 갈라집니다. 낙하산을 펴고 떨어지면 바다에서 건져 다시 사용합니다.",
                "長さ13.1 m、直径5.2 mの炭素複合材の半割れ2枚で、大気中を通る間衛星を守り、飛行開始から約3分で分離します。パラシュートで落下し、海から回収されて再び飛びます。",
                "Dos mitades de material compuesto de carbono, de 13,1 m de largo y 5,2 m de ancho, que protegen el satélite al atravesar la atmósfera y se separan unos tres minutos después del despegue. Caen con paracaídas y se recogen del mar para volver a volar.",
                "Deux demi-coques en composite carbone, de 13,1 m de long et 5,2 m de large, qui protègent le satellite dans l'atmosphère et se séparent environ trois minutes après le décollage. Elles redescendent sous parachute et sont repêchées en mer pour revoler.");
            A("Second stage", "2단", "第2段", "Segunda etapa", "Deuxième étage");
            A("A single Merlin Vacuum engine with a large niobium nozzle, 981 kN of thrust, that takes the payload from the edge of space to orbit in a burn of about six and a half minutes. It can relight to reach higher orbits.",
                "커다란 나이오븀 노즐이 달린 멀린 진공 엔진 하나로, 981kN의 추력을 내며 약 6분 30초 동안 연소해 탑재물을 우주의 가장자리에서 궤도까지 올립니다. 더 높은 궤도로 가기 위해 재점화할 수도 있습니다.",
                "大きなニオブ製ノズルを持つマーリン・バキュームエンジン1基で、推力981 kN。約6分半の燃焼で、宇宙の入り口から軌道まで搭載物を運びます。より高い軌道へ向かうため再点火もできます。",
                "Un solo motor Merlin Vacuum con una gran tobera de niobio y 981 kN de empuje, que lleva la carga del borde del espacio a la órbita en un encendido de unos seis minutos y medio. Puede reencenderse para alcanzar órbitas más altas.",
                "Un unique moteur Merlin Vacuum à grande tuyère en niobium, de 981 kN de poussée, qui emmène la charge du bord de l'espace jusqu'en orbite en une combustion d'environ six minutes et demie. Il peut être rallumé pour atteindre des orbites plus hautes.");
            A("Interstage", "단간부", "インターステージ", "Interetapa", "Inter-étage");
            A("A composite sleeve joining the stages and housing the Merlin Vacuum nozzle. Pneumatic pushers separate the stages here about two and a half minutes after lift-off; the stage below then turns around and flies home.",
                "두 단을 잇고 멀린 진공 엔진의 노즐을 감싸는 복합재 덮개입니다. 이륙 약 2분 30초 뒤 공압식 밀대가 여기서 두 단을 떼어 내고, 아래 단은 방향을 돌려 집으로 날아갑니다.",
                "2つの段をつなぎ、マーリン・バキュームのノズルを収める複合材の筒。離昇から約2分半後、空気圧式のプッシャーがここで段を分離し、下の段は向きを変えて帰還します。",
                "Una camisa de material compuesto que une las etapas y aloja la tobera del Merlin Vacuum. Unos empujadores neumáticos separan aquí las etapas unos dos minutos y medio después del despegue; la etapa de abajo da entonces la vuelta y regresa.",
                "Un manchon en composite qui relie les étages et abrite la tuyère du Merlin Vacuum. Des poussoirs pneumatiques séparent ici les étages environ deux minutes et demie après le décollage ; l'étage inférieur fait alors demi-tour et rentre.");
            A("Grid fins", "그리드 핀", "グリッドフィン", "Aletas de rejilla", "Ailerons en grille");
            A("Four titanium lattice fins at the top of the booster that fold out during descent and steer it through the air, like the feathers on an arrow that can also turn. Titanium replaced aluminium in 2017 so they survive re-entry heat unrefurbished.",
                "부스터 꼭대기의 격자 모양 티타늄 핀 4개로, 하강할 때 펼쳐져 공기 속에서 부스터를 조종합니다. 방향까지 틀 수 있는 화살 깃과 같습니다. 2017년 알루미늄을 티타늄으로 바꿔 정비 없이도 재진입 열을 견딥니다.",
                "ブースター上部にある4枚のチタン製格子状フィンで、降下中に開いて空気の中で機体を操ります。向きも変えられる矢羽根のようなものです。2017年にアルミニウムからチタンに替わり、整備なしで再突入の熱に耐えられるようになりました。",
                "Cuatro aletas de rejilla de titanio en lo alto del propulsor que se despliegan en el descenso y lo guían por el aire, como las plumas de una flecha que además pudieran girar. En 2017 el titanio sustituyó al aluminio para que resistan el calor de la reentrada sin reacondicionarlas.",
                "Quatre ailerons en treillis de titane en haut du propulseur, qui se déploient pendant la descente et le dirigent dans l'air, comme les plumes d'une flèche qui pourraient aussi tourner. Le titane a remplacé l'aluminium en 2017 pour qu'ils supportent la chaleur de la rentrée sans remise en état.");
            A("First stage", "1단", "第1段", "Primera etapa", "Premier étage");
            A("The booster: 41 m of aluminium-lithium tanks holding subcooled liquid oxygen above kerosene, with cold-gas thrusters at the top to flip it after separation. Three engines relight for the boost-back and entry burns, and one for the landing.",
                "부스터입니다. 41m 길이의 알루미늄-리튬 탱크에 과냉각 액체 산소가 등유 위에 담겨 있고, 꼭대기에는 분리 후 몸체를 뒤집는 냉가스 추력기가 있습니다. 귀환 연소와 재진입 연소 때는 엔진 3개가, 착륙 때는 1개가 다시 점화됩니다.",
                "ブースター。長さ41 mのアルミニウム・リチウム合金タンクに、過冷却した液体酸素がケロシンの上に入っています。上部には分離後に機体を反転させるコールドガススラスターがあります。帰還噴射と再突入噴射では3基、着陸では1基のエンジンが再点火します。",
                "El propulsor: 41 m de tanques de aluminio-litio con oxígeno líquido subenfriado sobre el queroseno, y propulsores de gas frío arriba para darle la vuelta tras la separación. Tres motores se reencienden para los encendidos de regreso y de reentrada, y uno para el aterrizaje.",
                "Le propulseur : 41 m de réservoirs en aluminium-lithium contenant de l'oxygène liquide sous-refroidi au-dessus du kérosène, avec en haut des propulseurs à gaz froid pour le retourner après la séparation. Trois moteurs se rallument pour les poussées de retour et de rentrée, et un pour l'atterrissage.");
            A("Landing legs", "착륙 다리", "着陸脚", "Patas de aterrizaje", "Jambes d'atterrissage");
            A("Four carbon-fibre legs with aluminium honeycomb that stay folded against the booster and deploy seconds before touchdown, spanning about 18 metres. Crushable cores in the legs soak up a hard landing.",
                "알루미늄 벌집 구조를 넣은 탄소 섬유 다리 4개로, 부스터에 접혀 있다가 착지 몇 초 전에 펼쳐지며 폭이 약 18m에 이릅니다. 다리 속 찌그러지는 심이 거친 착지의 충격을 흡수합니다.",
                "アルミニウムのハニカム構造を持つ炭素繊維製の脚4本で、ブースターに沿って折りたたまれ、着地の数秒前に開いて約18 mに広がります。脚の中のつぶれる芯が強い着地の衝撃を吸収します。",
                "Cuatro patas de fibra de carbono con panal de aluminio que van plegadas contra el propulsor y se despliegan segundos antes de tocar tierra, abarcando unos 18 metros. Núcleos deformables en las patas absorben un aterrizaje brusco.",
                "Quatre jambes en fibre de carbone à nid d'abeilles d'aluminium, repliées contre le propulseur et déployées quelques secondes avant le contact, sur une envergure d'environ 18 mètres. Des noyaux écrasables dans les jambes absorbent un atterrissage brutal.");
            A("Octaweb and Merlin engines", "옥타웹과 멀린 엔진", "オクタウェブとマーリンエンジン", "Octaweb y motores Merlin", "Octaweb et moteurs Merlin");
            A("Nine Merlin 1D engines in an octagonal frame: eight around one centre engine, which is the one that lands the rocket. Together they produce 7,607 kilonewtons at lift-off, and the rocket can lose one and still reach orbit.",
                "팔각형 틀에 멀린 1D 엔진 9기가 들어 있습니다. 가운데 엔진 하나를 여덟 개가 둘러싸는데, 로켓을 착륙시키는 것이 바로 이 가운데 엔진입니다. 이륙 때 함께 7,607킬로뉴턴을 내며, 하나를 잃어도 궤도에 오를 수 있습니다.",
                "八角形のフレームに収まった9基のマーリン1Dエンジン。中央の1基を8基が囲み、ロケットを着陸させるのはこの中央のエンジンです。離昇時は合わせて7,607キロニュートンを出し、1基を失っても軌道に到達できます。",
                "Nueve motores Merlin 1D en un armazón octogonal: ocho alrededor de uno central, que es el que hace aterrizar el cohete. Juntos producen 7607 kilonewtons al despegar, y el cohete puede perder uno y aun así llegar a la órbita.",
                "Neuf moteurs Merlin 1D dans un cadre octogonal : huit autour d'un moteur central, celui qui fait atterrir la fusée. Ensemble, ils produisent 7 607 kilonewtons au décollage, et la fusée peut en perdre un et atteindre quand même l'orbite.");

            // ---- Crew Dragon
            A("Crew Dragon", "크루 드래곤", "クルードラゴン", "Crew Dragon", "Crew Dragon");
            A("The capsule that returned crew launches to the United States", "미국에서 유인 발사를 되살린 캡슐", "アメリカからの有人打ち上げを復活させたカプセル", "La cápsula que devolvió los lanzamientos tripulados a Estados Unidos", "La capsule qui a ramené les lancements habités aux États-Unis");
            A("Dragon 2 is a reusable capsule 8.1 m tall with its trunk and 4 m across, flying people and cargo to the International Space Station. Demo-2, on 30 May 2020, carried Bob Behnken and Doug Hurley: the first crew launched from American soil since the shuttle retired in 2011. It flies itself, docks autonomously, and comes home under four parachutes to a splashdown off Florida.",
                "드래곤 2는 트렁크를 포함해 높이 8.1m, 지름 4m의 재사용 캡슐로, 사람과 화물을 국제우주정거장으로 나릅니다. 2020년 5월 30일 데모-2 임무에서 밥 벤켄과 더그 헐리를 태웠는데, 2011년 우주왕복선이 퇴역한 뒤 미국 땅에서 발사된 첫 승무원이었습니다. 스스로 비행하고 자동으로 도킹하며, 낙하산 4개를 펴고 플로리다 앞바다에 착수해 돌아옵니다.",
                "ドラゴン2はトランクを含めて高さ8.1 m、直径4 mの再使用型カプセルで、人と貨物を国際宇宙ステーションへ運びます。2020年5月30日のデモ2ではボブ・ベンケンとダグ・ハーレーを乗せ、2011年のスペースシャトル退役以来、初めてアメリカの地から打ち上げられたクルーとなりました。自律飛行し、自動でドッキングし、4つのパラシュートでフロリダ沖に着水して帰還します。",
                "Dragon 2 es una cápsula reutilizable de 8,1 m de alto con su tronco y 4 m de ancho, que lleva personas y carga a la Estación Espacial Internacional. La Demo-2, el 30 de mayo de 2020, llevó a Bob Behnken y Doug Hurley: la primera tripulación lanzada desde suelo estadounidense desde la retirada del transbordador en 2011. Vuela sola, se acopla de forma autónoma y regresa con cuatro paracaídas hasta amerizar frente a Florida.",
                "Dragon 2 est une capsule réutilisable de 8,1 m de haut avec son tronc et 4 m de large, qui transporte personnes et fret vers la Station spatiale internationale. Demo-2, le 30 mai 2020, a emporté Bob Behnken et Doug Hurley : le premier équipage lancé depuis le sol américain depuis le retrait de la navette en 2011. Elle vole seule, s'amarre de façon autonome et revient sous quatre parachutes pour amerrir au large de la Floride.");
            A("Height 8.1 m with trunk, diameter 4 m", "높이 8.1m(트렁크 포함), 지름 4m", "高さ8.1 m（トランク含む）、直径4 m", "Altura 8,1 m con el tronco, diámetro 4 m", "Hauteur 8,1 m avec le tronc, diamètre 4 m");
            A("Pressurised volume 9.3 m³; carries up to 7, NASA flies 4", "여압 공간 9.3m³, 최대 7명 탑승 가능, NASA는 4명 탑승", "与圧容積9.3 m³、最大7人乗り、NASAは4人で運用", "Volumen presurizado 9,3 m³; lleva hasta 7 personas, la NASA vuela con 4", "Volume pressurisé 9,3 m³ ; jusqu'à 7 personnes, la NASA en fait voler 4");
            A("Cargo to the Station: 6,000 kg; back to Earth: 3,000 kg", "정거장으로 가는 화물 6,000kg, 지구로 돌아오는 화물 3,000kg", "ステーションへの貨物：6,000 kg、地球への帰還：3,000 kg", "Carga a la Estación: 6000 kg; de vuelta a la Tierra: 3000 kg", "Fret vers la Station : 6 000 kg ; retour sur Terre : 3 000 kg");
            A("First crewed flight: Demo-2, 30 May 2020", "첫 유인 비행: 데모-2, 2020년 5월 30일", "初の有人飛行：デモ2、2020年5月30日", "Primer vuelo tripulado: Demo-2, 30 de mayo de 2020", "Premier vol habité : Demo-2, 30 mai 2020");
            A("Launch escape: eight SuperDraco engines, 71 kN each", "발사 탈출: 슈퍼드라코 엔진 8기, 각 71kN", "緊急脱出：スーパードラコエンジン8基、各71 kN", "Escape en el lanzamiento: ocho motores SuperDraco de 71 kN cada uno", "Sauvetage au lancement : huit moteurs SuperDraco de 71 kN chacun");
            A("Lands at sea under four main parachutes", "주 낙하산 4개로 바다에 착수", "4つのメインパラシュートで海に着水", "Amerizaje con cuatro paracaídas principales", "Amerrit sous quatre parachutes principaux");
            A("Nosecone", "노즈콘", "ノーズコーン", "Cono de morro", "Cône avant");
            A("A hinged cap that protects the docking port during launch and re-entry and swings open in orbit. On Inspiration4 in 2021 the port behind it was replaced by a glass cupola, the largest window ever flown.",
                "발사와 재진입 동안 도킹 포트를 보호하고 궤도에서 열리는 경첩 달린 덮개입니다. 2021년 인스피레이션4 임무에서는 그 뒤의 포트 대신 유리 큐폴라를 달았는데, 지금까지 우주로 간 가장 큰 창문이었습니다.",
                "打ち上げと再突入の間ドッキングポートを守り、軌道上で開く蝶番付きのキャップ。2021年のインスピレーション4では、その奥のポートがガラスのキューポラに替えられ、宇宙を飛んだ史上最大の窓となりました。",
                "Una tapa con bisagra que protege el puerto de acoplamiento en el lanzamiento y la reentrada y se abre en órbita. En Inspiration4, en 2021, el puerto de detrás se sustituyó por una cúpula de cristal, la ventana más grande que ha volado nunca.",
                "Un capot à charnière qui protège le port d'amarrage pendant le lancement et la rentrée, et s'ouvre en orbite. Sur Inspiration4, en 2021, le port situé derrière a été remplacé par une coupole vitrée, le plus grand hublot jamais envoyé dans l'espace.");
            A("Docking system", "도킹 장치", "ドッキング機構", "Sistema de acoplamiento", "Système d'amarrage");
            A("A soft-capture ring and hooks built to the international docking standard, so Dragon can berth itself to the Station's ports without a robotic arm. The whole approach is automatic; the crew only watch.",
                "국제 도킹 표준에 맞춘 소프트 캡처 링과 고리로, 드래곤은 로봇 팔 없이 스스로 정거장 포트에 붙을 수 있습니다. 접근 과정은 모두 자동이고, 승무원은 지켜보기만 합니다.",
                "国際ドッキング規格に沿ったソフトキャプチャーリングとフックで、ドラゴンはロボットアームなしで自らステーションのポートに結合できます。接近はすべて自動で、クルーは見守るだけです。",
                "Un anillo de captura suave y ganchos según el estándar internacional de acoplamiento, para que Dragon se una sola a los puertos de la Estación sin brazo robótico. Toda la aproximación es automática; la tripulación solo observa.",
                "Un anneau de capture douce et des crochets conformes à la norme internationale d'amarrage, pour que Dragon s'amarre seule aux ports de la Station sans bras robotique. Toute l'approche est automatique ; l'équipage ne fait que regarder.");
            A("Capsule", "캡슐", "カプセル", "Cápsula", "Capsule");
            A("The pressurised cabin, 9.3 cubic metres, with four seats, three touch-screens in place of switch panels, and an environmental system good for a week on its own. The hull is reused: the same capsules have flown several crews.",
                "9.3세제곱미터의 여압 선실로, 좌석 4개, 스위치 패널 대신 터치스크린 3개, 혼자서도 일주일을 버티는 생명 유지 장치를 갖췄습니다. 선체는 재사용되어, 같은 캡슐이 여러 승무원을 태우고 날았습니다.",
                "容積9.3立方メートルの与圧キャビンで、座席4つ、スイッチパネルの代わりに3枚のタッチスクリーン、単独で1週間もつ環境制御システムを備えています。船体は再使用され、同じカプセルが何組ものクルーを運んできました。",
                "La cabina presurizada, de 9,3 metros cúbicos, con cuatro asientos, tres pantallas táctiles en lugar de paneles de interruptores y un sistema de soporte vital autónomo para una semana. El casco se reutiliza: las mismas cápsulas han llevado a varias tripulaciones.",
                "La cabine pressurisée, de 9,3 mètres cubes, avec quatre sièges, trois écrans tactiles à la place des panneaux d'interrupteurs et un système de survie autonome pour une semaine. La coque est réutilisée : les mêmes capsules ont emmené plusieurs équipages.");
            A("SuperDraco engines", "슈퍼드라코 엔진", "スーパードラコエンジン", "Motores SuperDraco", "Moteurs SuperDraco");
            A("Eight 3D-printed engines in four pods around the capsule, 71 kilonewtons each, burning stored hypergolic propellants. They fire only to pull the capsule away from a failing rocket, which they demonstrated in flight in January 2020.",
                "캡슐 둘레 네 곳의 포드에 나뉘어 달린 3D 프린팅 엔진 8기로, 각각 71킬로뉴턴을 내며 저장된 자연 발화성 추진제를 태웁니다. 고장 난 로켓에서 캡슐을 떼어 낼 때만 점화하며, 2020년 1월 비행 시험에서 이를 보여 주었습니다.",
                "カプセルの周囲4か所のポッドに収められた3Dプリント製エンジン8基で、各71キロニュートン。貯蔵したハイパーゴリック推進剤を燃やします。故障したロケットからカプセルを引き離すときにだけ噴射し、2020年1月の飛行試験でそれを実証しました。",
                "Ocho motores impresos en 3D en cuatro módulos alrededor de la cápsula, de 71 kilonewtons cada uno, que queman propelentes hipergólicos almacenados. Solo se encienden para alejar la cápsula de un cohete que falla, algo que demostraron en vuelo en enero de 2020.",
                "Huit moteurs imprimés en 3D, répartis en quatre modules autour de la capsule, de 71 kilonewtons chacun, brûlant des ergols hypergoliques stockés. Ils ne s'allument que pour éloigner la capsule d'une fusée défaillante, ce qu'ils ont démontré en vol en janvier 2020.");
            A("Draco thrusters", "드라코 추력기", "ドラコスラスター", "Propulsores Draco", "Propulseurs Draco");
            A("Sixteen small thrusters of about 400 newtons each, used for every manoeuvre in orbit: rendezvous, attitude control and the deorbit burn that brings the capsule home.",
                "각각 약 400뉴턴을 내는 작은 추력기 16개로, 궤도에서의 모든 기동에 쓰입니다. 랑데부, 자세 제어, 그리고 캡슐을 집으로 데려오는 궤도 이탈 연소까지요.",
                "それぞれ約400ニュートンの小型スラスター16基で、軌道上のあらゆる操作に使われます。ランデブー、姿勢制御、そしてカプセルを帰還させる軌道離脱噴射です。",
                "Dieciséis pequeños propulsores de unos 400 newtons cada uno, usados en todas las maniobras en órbita: el encuentro, el control de orientación y el encendido de salida de órbita que trae la cápsula a casa.",
                "Seize petits propulseurs d'environ 400 newtons chacun, utilisés pour toutes les manœuvres en orbite : le rendez-vous, le contrôle d'attitude et la poussée de désorbitation qui ramène la capsule.");
            A("Heat shield", "열 차폐막", "耐熱シールド", "Escudo térmico", "Bouclier thermique");
            A("A shield of PICA-X, SpaceX's version of NASA's phenolic-impregnated carbon ablator, covering the base. It chars and slowly erodes while re-entry heats it to around 1,600 °C, keeping the cabin cool.",
                "바닥을 덮은 PICA-X 차폐막으로, NASA의 페놀 함침 탄소 삭마재를 SpaceX가 개량한 것입니다. 재진입으로 약 1,600°C까지 달아오르는 동안 그을리며 천천히 깎여 나가 선실을 시원하게 지킵니다.",
                "底面を覆うPICA-X製のシールドで、NASAのフェノール含浸炭素アブレーターをSpaceXが改良したものです。再突入で約1,600 °Cまで熱せられる間、焦げてゆっくり削れながら、キャビンを涼しく保ちます。",
                "Un escudo de PICA-X, la versión de SpaceX del ablador de carbono impregnado de fenólico de la NASA, que cubre la base. Se carboniza y se erosiona lentamente mientras la reentrada lo calienta hasta unos 1600 °C, manteniendo fresca la cabina.",
                "Un bouclier en PICA-X, la version SpaceX de l'ablateur carbone imprégné de résine phénolique de la NASA, qui couvre la base. Il se carbonise et s'érode lentement tandis que la rentrée le chauffe à environ 1 600 °C, gardant la cabine au frais.");
            A("Trunk", "트렁크", "トランク", "Tronco", "Tronc");
            A("An unpressurised cylinder below the capsule carrying external cargo, with solar cells over half its surface and fins that steady the stack during an abort. It is jettisoned before the deorbit burn and burns up.",
                "캡슐 아래의 비여압 원통으로 외부 화물을 싣습니다. 표면의 절반은 태양 전지로 덮여 있고, 비상 탈출 때 전체를 안정시키는 핀이 달려 있습니다. 궤도 이탈 연소 전에 분리되어 불타 없어집니다.",
                "カプセルの下にある非与圧の円筒で、外部貨物を運びます。表面の半分は太陽電池で覆われ、緊急脱出時に機体を安定させるフィンがあります。軌道離脱噴射の前に切り離され、燃え尽きます。",
                "Un cilindro no presurizado bajo la cápsula que lleva carga externa, con células solares en la mitad de su superficie y aletas que estabilizan el conjunto durante un aborto. Se desecha antes del encendido de salida de órbita y se desintegra.",
                "Un cylindre non pressurisé sous la capsule qui transporte du fret externe, avec des cellules solaires sur la moitié de sa surface et des ailerons qui stabilisent l'ensemble lors d'un sauvetage. Il est largué avant la poussée de désorbitation et se consume.");
            A("Parachutes", "낙하산", "パラシュート", "Paracaídas", "Parachutes");
            A("Two drogue chutes to steady the capsule, then four Mark 3 main parachutes with reinforced Zylon lines, slowing it to about 25 km/h for splashdown. Any three mains are enough for a safe landing.",
                "먼저 보조 낙하산 2개가 캡슐을 안정시키고, 이어 자일론 줄을 보강한 마크 3 주 낙하산 4개가 펴져 착수 속도를 시속 약 25km로 줄입니다. 주 낙하산은 아무거나 3개만 있어도 안전하게 내려앉을 수 있습니다.",
                "まず2つのドローグシュートがカプセルを安定させ、続いてザイロン製の補強ラインを持つマーク3メインパラシュート4つが開き、着水時の速度を時速約25 kmまで落とします。メインは3つあれば安全に着水できます。",
                "Dos paracaídas de frenado para estabilizar la cápsula y luego cuatro paracaídas principales Mark 3 con cuerdas reforzadas de Zylon, que la frenan a unos 25 km/h para el amerizaje. Bastan tres de los principales para un aterrizaje seguro.",
                "Deux parachutes extracteurs pour stabiliser la capsule, puis quatre parachutes principaux Mark 3 aux suspentes renforcées en Zylon, qui la ralentissent à environ 25 km/h pour l'amerrissage. Trois parachutes principaux suffisent pour un retour sûr.");
        }
    }
}
