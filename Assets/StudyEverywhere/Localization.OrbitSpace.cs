using System.Collections.Generic;

namespace StudyEverywhere
{
    // The Orbit holodeck's Solar System, Universe, Math and Mission modes (OrbitPlanets, OrbitSolar, HoloSolar,
    // HoloUniverse, HoloMath, MathExpr, OrbitMissions, and the static labels SolarBuilder and MathBuilder write).
    // Keys are the exact English the code and builders use; composed lines are format strings ({0}...) that the
    // runtime scripts fill through Localization.Format. Formulas, numbers and function names stay as they are.
    public static partial class Localization
    {
        static partial void AddOrbitSpace(Dictionary<string, string[]> t)
        {
            void A(string en, string ko, string ja, string es, string fr) => t[en] = new[] { ko, ja, es, fr };

            // ================================================================== names
            A("The Sun", "태양", "太陽", "El Sol", "Le Soleil");
            A("Mercury", "수성", "水星", "Mercurio", "Mercure");
            A("Venus", "금성", "金星", "Venus", "Vénus");
            A("Earth", "지구", "地球", "Tierra", "Terre");
            A("Mars", "화성", "火星", "Marte", "Mars");
            A("Jupiter", "목성", "木星", "Júpiter", "Jupiter");
            A("Saturn", "토성", "土星", "Saturno", "Saturne");
            A("Uranus", "천왕성", "天王星", "Urano", "Uranus");
            A("Neptune", "해왕성", "海王星", "Neptuno", "Neptune");
            A("Pluto", "명왕성", "冥王星", "Plutón", "Pluton");
            A("The Moon", "달", "月", "La Luna", "La Lune");
            A("Phobos", "포보스", "フォボス", "Fobos", "Phobos");
            A("Deimos", "데이모스", "ダイモス", "Deimos", "Déimos");
            A("Io", "이오", "イオ", "Ío", "Io");
            A("Europa", "유로파", "エウロパ", "Europa", "Europe");
            A("Ganymede", "가니메데", "ガニメデ", "Ganimedes", "Ganymède");
            A("Callisto", "칼리스토", "カリスト", "Calisto", "Callisto");
            A("Mimas", "미마스", "ミマス", "Mimas", "Mimas");
            A("Enceladus", "엔셀라두스", "エンケラドゥス", "Encélado", "Encelade");
            A("Tethys", "테티스", "テティス", "Tetis", "Téthys");
            A("Dione", "디오네", "ディオネ", "Dione", "Dioné");
            A("Rhea", "레아", "レア", "Rea", "Rhéa");
            A("Titan", "타이탄", "タイタン", "Titán", "Titan");
            A("Iapetus", "이아페투스", "イアペトゥス", "Jápeto", "Japet");
            A("Miranda", "미란다", "ミランダ", "Miranda", "Miranda");
            A("Ariel", "아리엘", "アリエル", "Ariel", "Ariel");
            A("Umbriel", "움브리엘", "ウンブリエル", "Umbriel", "Umbriel");
            A("Titania", "티타니아", "チタニア", "Titania", "Titania");
            A("Oberon", "오베론", "オベロン", "Oberón", "Obéron");
            A("Proteus", "프로테우스", "プロテウス", "Proteo", "Protée");
            A("Triton", "트리톤", "トリトン", "Tritón", "Triton");
            A("Nereid", "네레이드", "ネレイド", "Nereida", "Néréide");
            A("Charon", "카론", "カロン", "Caronte", "Charon");
            A("Sirius A", "시리우스 A", "シリウスA", "Sirio A", "Sirius A");
            A("Pollux", "폴룩스", "ポルックス", "Pólux", "Pollux");
            A("Arcturus", "아르크투루스", "アークトゥルス", "Arturo", "Arcturus");
            A("Rigel", "리겔", "リゲル", "Rigel", "Rigel");
            A("Betelgeuse", "베텔게우스", "ベテルギウス", "Betelgeuse", "Bételgeuse");
            A("VY Canis Majoris", "큰개자리 VY", "おおいぬ座VY星", "VY Canis Majoris", "VY Canis Majoris");
            A("Stephenson 2-18", "스티븐슨 2-18", "スティーブンソン2-18", "Stephenson 2-18", "Stephenson 2-18");
            A("Our Solar System", "우리 태양계", "私たちの太陽系", "Nuestro sistema solar", "Notre système solaire");
            A("The Oort Cloud", "오르트 구름", "オールトの雲", "La nube de Oort", "Le nuage d'Oort");
            A("The Milky Way", "우리은하", "天の川銀河", "La Vía Láctea", "La Voie lactée");
            A("The Local Group", "국부 은하군", "局部銀河群", "El Grupo Local", "Le Groupe local");
            A("The Observable Universe", "관측 가능한 우주", "観測可能な宇宙", "El universo observable", "L'Univers observable");

            // ================================================================== the planets (OrbitPlanets, also the pillar cards)
            A("Rocky planet, closest to the Sun", "태양에 가장 가까운 암석 행성", "太陽に最も近い岩石惑星", "Planeta rocoso, el más cercano al Sol", "Planète rocheuse, la plus proche du Soleil");
            A("The smallest planet and the fastest, racing round the Sun every 88 days. With almost no air to hold heat, its days reach 430 °C and its nights fall to −180 °C, and ice may hide in craters at its poles that sunlight never reaches.",
                "가장 작고 가장 빠른 행성으로, 88일마다 태양을 한 바퀴 돕니다. 열을 붙잡아 둘 대기가 거의 없어 낮에는 430 °C까지 오르고 밤에는 −180 °C까지 떨어지며, 햇빛이 닿지 않는 극지방의 크레이터에는 얼음이 숨어 있을지도 모릅니다.",
                "最も小さく最も速い惑星で、88日ごとに太陽を一周します。熱を保つ大気がほとんどないため、昼は430 °Cに達し、夜は−180 °Cまで下がります。日光が届かない極のクレーターには氷が隠れているかもしれません。",
                "El planeta más pequeño y el más rápido: da la vuelta al Sol cada 88 días. Casi sin aire que retenga el calor, sus días alcanzan 430 °C y sus noches bajan a −180 °C, y puede haber hielo escondido en cráteres de sus polos adonde nunca llega la luz del Sol.",
                "La plus petite planète et la plus rapide, qui fait le tour du Soleil en 88 jours. Presque sans air pour retenir la chaleur, ses journées atteignent 430 °C et ses nuits tombent à −180 °C, et de la glace se cache peut-être dans des cratères de ses pôles que le Soleil n'éclaire jamais.");
            A("Diameter: 4,879 km", "지름: 4,879 km", "直径：4,879 km", "Diámetro: 4879 km", "Diamètre : 4 879 km");
            A("From the Sun: 57.9 million km", "태양까지: 5,790만 km", "太陽から：5,790万 km", "Distancia al Sol: 57,9 millones de km", "Distance au Soleil : 57,9 millions de km");
            A("Year: 88 Earth days", "1년: 지구일로 88일", "1年：地球の88日", "Año: 88 días terrestres", "Année : 88 jours terrestres");
            A("One day (sunrise to sunrise): 176 Earth days", "하루(해돋이부터 다음 해돋이까지): 지구일로 176일", "1日（日の出から次の日の出まで）：地球の176日", "Un día (de amanecer a amanecer): 176 días terrestres", "Un jour (d'un lever de Soleil au suivant) : 176 jours terrestres");
            A("Moons: none", "위성: 없음", "衛星：なし", "Lunas: ninguna", "Lunes : aucune");
            A("Visited by Mariner 10 and MESSENGER; BepiColombo is on its way", "매리너 10호와 메신저가 방문했고, 베피콜롬보가 가는 중", "マリナー10号とメッセンジャーが訪問、ベピコロンボが向かっている途中", "Visitado por la Mariner 10 y MESSENGER; BepiColombo va de camino", "Visitée par Mariner 10 et MESSENGER ; BepiColombo est en route");

            A("Rocky planet, wrapped in cloud", "구름에 싸인 암석 행성", "雲に包まれた岩石惑星", "Planeta rocoso, envuelto en nubes", "Planète rocheuse, enveloppée de nuages");
            A("Nearly Earth's twin in size, but its thick carbon dioxide air traps so much heat that the surface is about 465 °C, hotter than Mercury. It spins backwards, so the Sun rises in the west, and a day there is longer than its year.",
                "크기는 지구의 쌍둥이에 가깝지만, 두꺼운 이산화탄소 대기가 열을 가두어 표면 온도가 약 465 °C로 수성보다 뜨겁습니다. 거꾸로 자전해서 해가 서쪽에서 뜨고, 하루가 1년보다 깁니다.",
                "大きさは地球の双子のようですが、厚い二酸化炭素の大気が熱を閉じ込めるため、地表は約465 °Cと水星よりも高温です。逆向きに自転するので太陽は西から昇り、1日が1年よりも長いのです。",
                "Casi gemelo de la Tierra en tamaño, pero su espesa atmósfera de dióxido de carbono atrapa tanto calor que la superficie está a unos 465 °C, más caliente que Mercurio. Gira al revés, así que el Sol sale por el oeste, y un día allí dura más que su año.",
                "Presque la jumelle de la Terre par la taille, mais son air épais de dioxyde de carbone retient tant de chaleur que la surface atteint environ 465 °C, plus que Mercure. Elle tourne à l'envers, si bien que le Soleil s'y lève à l'ouest, et un jour y dure plus longtemps que son année.");
            A("Diameter: 12,104 km", "지름: 12,104 km", "直径：12,104 km", "Diámetro: 12.104 km", "Diamètre : 12 104 km");
            A("From the Sun: 108.2 million km", "태양까지: 1억 820만 km", "太陽から：1億820万 km", "Distancia al Sol: 108,2 millones de km", "Distance au Soleil : 108,2 millions de km");
            A("Year: 225 Earth days", "1년: 지구일로 225일", "1年：地球の225日", "Año: 225 días terrestres", "Année : 225 jours terrestres");
            A("Turns once in 243 Earth days, backwards", "지구일로 243일에 한 번, 거꾸로 자전", "地球の243日で1回、逆向きに自転", "Gira una vez cada 243 días terrestres, al revés", "Fait un tour en 243 jours terrestres, à l'envers");
            A("Surface pressure: about 92 times Earth's", "표면 기압: 지구의 약 92배", "地表の気圧：地球の約92倍", "Presión en superficie: unas 92 veces la de la Tierra", "Pression au sol : environ 92 fois celle de la Terre");

            A("Rocky planet, the red one", "붉은 암석 행성", "赤い岩石惑星", "Planeta rocoso, el rojo", "Planète rocheuse, la rouge");
            A("Rusty iron dust makes Mars red. It has the tallest volcano known, Olympus Mons, nearly three times the height of Everest, and a canyon, Valles Marineris, as long as the United States is wide. Rovers such as Perseverance are searching it for signs of ancient life.",
                "녹슨 철 먼지 때문에 화성은 붉습니다. 알려진 가장 높은 화산인 올림푸스 산은 에베레스트의 거의 세 배 높이이고, 마리너스 협곡은 미국의 동서 폭만큼 깁니다. 퍼서비어런스 같은 로버가 고대 생명의 흔적을 찾고 있습니다.",
                "さびた鉄の塵が火星を赤くしています。知られている中で最も高い火山オリンポス山はエベレストのほぼ3倍の高さで、マリネリス峡谷はアメリカ合衆国の東西の幅ほどの長さがあります。パーサヴィアランスなどの探査車が古代の生命の痕跡を探しています。",
                "El polvo de hierro oxidado hace que Marte sea rojo. Tiene el volcán más alto conocido, el Olympus Mons, casi tres veces la altura del Everest, y un cañón, Valles Marineris, tan largo como ancho es Estados Unidos. Róveres como Perseverance buscan en él señales de vida antigua.",
                "Une poussière de fer rouillé rend Mars rouge. Elle possède le plus haut volcan connu, Olympus Mons, presque trois fois la hauteur de l'Everest, et un canyon, Valles Marineris, aussi long que les États-Unis sont larges. Des rovers comme Perseverance y cherchent des traces de vie ancienne.");
            A("Diameter: 6,779 km", "지름: 6,779 km", "直径：6,779 km", "Diámetro: 6779 km", "Diamètre : 6 779 km");
            A("From the Sun: 227.9 million km", "태양까지: 2억 2,790만 km", "太陽から：2億2,790万 km", "Distancia al Sol: 227,9 millones de km", "Distance au Soleil : 227,9 millions de km");
            A("Year: 687 Earth days", "1년: 지구일로 687일", "1年：地球の687日", "Año: 687 días terrestres", "Année : 687 jours terrestres");
            A("Day: 24 hours 37 minutes", "하루: 24시간 37분", "1日：24時間37分", "Día: 24 horas 37 minutos", "Jour : 24 heures 37 minutes");
            A("Moons: 2, Phobos and Deimos", "위성: 2개, 포보스와 데이모스", "衛星：2個、フォボスとダイモス", "Lunas: 2, Fobos y Deimos", "Lunes : 2, Phobos et Déimos");
            A("Olympus Mons: about 22 km high", "올림푸스 산: 높이 약 22 km", "オリンポス山：高さ約22 km", "Olympus Mons: unos 22 km de altura", "Olympus Mons : environ 22 km de haut");

            A("Gas giant, the largest planet", "가장 큰 행성인 가스 거인", "最大の惑星、巨大ガス惑星", "Gigante gaseoso, el planeta más grande", "Géante gazeuse, la plus grande planète");
            A("More than twice as massive as all the other planets put together. Its stripes are bands of cloud racing in opposite directions, and the Great Red Spot is a storm wider than the Earth that has raged for centuries. Its moon Europa may hide an ocean under its ice.",
                "다른 모든 행성을 합친 것보다 두 배 넘게 무겁습니다. 줄무늬는 서로 반대 방향으로 내달리는 구름 띠이고, 대적점은 지구보다 넓은 폭풍으로 수백 년째 몰아치고 있습니다. 위성 유로파는 얼음 아래에 바다를 숨기고 있을지도 모릅니다.",
                "ほかのすべての惑星を合わせたより2倍以上重い惑星です。縞模様は逆向きに流れる雲の帯で、大赤斑は地球より大きな嵐で、何世紀も吹き荒れています。衛星エウロパは氷の下に海を隠しているかもしれません。",
                "Tiene más del doble de masa que todos los demás planetas juntos. Sus franjas son bandas de nubes que corren en direcciones opuestas, y la Gran Mancha Roja es una tormenta más ancha que la Tierra que lleva siglos activa. Su luna Europa podría esconder un océano bajo el hielo.",
                "Plus de deux fois plus massive que toutes les autres planètes réunies. Ses bandes sont des nuages filant en sens opposés, et la Grande Tache rouge est une tempête plus large que la Terre qui fait rage depuis des siècles. Sa lune Europe cache peut-être un océan sous sa glace.");
            A("Diameter: 139,820 km, 11 Earths across", "지름: 139,820 km, 지구 11개 너비", "直径：139,820 km、地球11個分", "Diámetro: 139.820 km, 11 Tierras", "Diamètre : 139 820 km, 11 Terres");
            A("From the Sun: 778.5 million km", "태양까지: 7억 7,850만 km", "太陽から：7億7,850万 km", "Distancia al Sol: 778,5 millones de km", "Distance au Soleil : 778,5 millions de km");
            A("Year: 11.9 Earth years", "1년: 지구 시간으로 11.9년", "1年：地球の11.9年", "Año: 11,9 años terrestres", "Année : 11,9 années terrestres");
            A("Day: 9 hours 56 minutes, the shortest of any planet", "하루: 9시간 56분, 행성 중 가장 짧음", "1日：9時間56分、惑星で最短", "Día: 9 horas 56 minutos, el más corto de todos los planetas", "Jour : 9 heures 56 minutes, le plus court de toutes les planètes");
            A("Moons: 95 officially recognised", "위성: 공식 인정 95개", "衛星：公式に認められたもの95個", "Lunas: 95 reconocidas oficialmente", "Lunes : 95 officiellement reconnues");
            A("Visited by Juno, Galileo, the Voyagers and Pioneers", "주노, 갈릴레오, 보이저호와 파이어니어호가 방문", "ジュノー、ガリレオ、ボイジャー、パイオニアが訪問", "Visitado por Juno, Galileo, las Voyager y las Pioneer", "Visitée par Juno, Galileo, les Voyager et les Pioneer");

            A("Gas giant, the ringed planet", "고리를 가진 가스 거인", "環をもつ巨大ガス惑星", "Gigante gaseoso, el planeta de los anillos", "Géante gazeuse, la planète aux anneaux");
            A("Its rings are billions of pieces of ice and rock, some as small as sand grains and some as big as houses, spread 280,000 km wide but mostly only about ten metres thick. Saturn is so light for its size that it would float in a big enough bath.",
                "고리는 수십억 개의 얼음과 암석 조각으로, 모래알만 한 것부터 집채만 한 것까지 있으며 폭은 28만 km에 달하지만 두께는 대부분 10미터 정도에 불과합니다. 토성은 크기에 비해 아주 가벼워서 충분히 큰 욕조가 있다면 물에 뜰 것입니다.",
                "環は何十億もの氷と岩のかけらでできていて、砂粒ほどのものから家ほどのものまであります。幅は28万 kmに広がりますが、厚さはほとんどの場所でわずか10メートルほどです。土星は大きさのわりにとても軽く、十分大きな浴槽があれば水に浮くでしょう。",
                "Sus anillos son miles de millones de trozos de hielo y roca, algunos pequeños como granos de arena y otros grandes como casas, extendidos a lo largo de 280.000 km pero en su mayoría de solo unos diez metros de grosor. Saturno es tan ligero para su tamaño que flotaría en una bañera lo bastante grande.",
                "Ses anneaux sont des milliards de morceaux de glace et de roche, certains petits comme des grains de sable, d'autres grands comme des maisons, étalés sur 280 000 km de large mais épais pour la plupart d'une dizaine de mètres seulement. Saturne est si légère pour sa taille qu'elle flotterait dans une baignoire assez grande.");
            A("Diameter: 116,460 km", "지름: 116,460 km", "直径：116,460 km", "Diámetro: 116.460 km", "Diamètre : 116 460 km");
            A("From the Sun: 1.43 billion km", "태양까지: 14억 3천만 km", "太陽から：14億3,000万 km", "Distancia al Sol: 1430 millones de km", "Distance au Soleil : 1,43 milliard de km");
            A("Year: 29.4 Earth years", "1년: 지구 시간으로 29.4년", "1年：地球の29.4年", "Año: 29,4 años terrestres", "Année : 29,4 années terrestres");
            A("Day: 10 hours 33 minutes", "하루: 10시간 33분", "1日：10時間33分", "Día: 10 horas 33 minutos", "Jour : 10 heures 33 minutes");
            A("Moons: 274 known (2025), Titan the largest", "위성: 274개 확인(2025년), 가장 큰 것은 타이탄", "衛星：274個が既知（2025年）、最大はタイタン", "Lunas: 274 conocidas (2025), Titán la mayor", "Lunes : 274 connues (2025), Titan la plus grande");
            A("Cassini orbited it from 2004 to 2017", "카시니가 2004년부터 2017년까지 궤도를 돎", "カッシーニが2004年から2017年まで周回", "La Cassini lo orbitó de 2004 a 2017", "Cassini l'a orbitée de 2004 à 2017");

            A("Ice giant, tipped on its side", "옆으로 누운 얼음 거인", "横倒しの巨大氷惑星", "Gigante helado, tumbado de lado", "Géante de glace, couchée sur le côté");
            A("Something huge once knocked Uranus over: it spins tilted 98 degrees, rolling round the Sun like a ball, so each pole has 42 years of sunlight and then 42 years of night. Methane in its air gives it its pale blue-green.",
                "아주 오래전 거대한 무언가가 천왕성을 넘어뜨렸습니다. 자전축이 98도 기울어 공처럼 굴러가며 태양을 돌기 때문에, 각 극은 42년 동안 햇빛을 받고 이어서 42년 동안 밤을 맞습니다. 대기 속 메탄이 옅은 청록색을 띠게 합니다.",
                "かつて何か巨大なものが天王星を横倒しにしました。自転軸が98度傾き、ボールが転がるように太陽を回るため、それぞれの極は42年間日が当たり、その後42年間夜が続きます。大気中のメタンが淡い青緑色を生んでいます。",
                "Algo enorme derribó una vez a Urano: gira inclinado 98 grados, rodando alrededor del Sol como una pelota, así que cada polo tiene 42 años de luz solar y luego 42 años de noche. El metano de su atmósfera le da su color verde azulado pálido.",
                "Quelque chose d'énorme a un jour renversé Uranus : elle tourne inclinée de 98 degrés, roulant autour du Soleil comme une balle, si bien que chaque pôle connaît 42 ans de soleil puis 42 ans de nuit. Le méthane de son atmosphère lui donne sa teinte bleu-vert pâle.");
            A("Diameter: 50,724 km", "지름: 50,724 km", "直径：50,724 km", "Diámetro: 50.724 km", "Diamètre : 50 724 km");
            A("From the Sun: 2.87 billion km", "태양까지: 28억 7천만 km", "太陽から：28億7,000万 km", "Distancia al Sol: 2870 millones de km", "Distance au Soleil : 2,87 milliards de km");
            A("Year: 84 Earth years", "1년: 지구 시간으로 84년", "1年：地球の84年", "Año: 84 años terrestres", "Année : 84 années terrestres");
            A("Day: 17 hours 14 minutes", "하루: 17시간 14분", "1日：17時間14分", "Día: 17 horas 14 minutos", "Jour : 17 heures 14 minutes");
            A("Moons: 29 known", "위성: 29개 확인", "衛星：29個が既知", "Lunas: 29 conocidas", "Lunes : 29 connues");
            A("Visited once, by Voyager 2 in 1986", "1986년 보이저 2호가 단 한 번 방문", "1986年にボイジャー2号が一度だけ訪問", "Visitado una sola vez, por la Voyager 2 en 1986", "Visitée une seule fois, par Voyager 2 en 1986");

            A("Ice giant, the farthest planet", "가장 먼 행성인 얼음 거인", "最も遠い惑星、巨大氷惑星", "Gigante helado, el planeta más lejano", "Géante de glace, la planète la plus lointaine");
            A("Dark, cold and windy: Neptune has the fastest winds in the solar system, over 2,000 km/h. It was the first planet found by mathematics, predicted from the way it tugged on Uranus before anyone saw it in 1846.",
                "어둡고 춥고 바람이 거셉니다. 해왕성에는 태양계에서 가장 빠른 바람이 시속 2,000 km 넘게 붑니다. 수학으로 발견된 첫 행성으로, 1846년 누군가 직접 보기 전에 천왕성을 끌어당기는 모습에서 그 존재가 예측되었습니다.",
                "暗く、冷たく、風の強い惑星です。海王星には太陽系で最も速い時速2,000 km以上の風が吹いています。数学によって見つかった最初の惑星で、1846年に誰かが目にする前に、天王星を引っ張る様子からその存在が予言されました。",
                "Oscuro, frío y ventoso: Neptuno tiene los vientos más rápidos del sistema solar, de más de 2000 km/h. Fue el primer planeta hallado gracias a las matemáticas, predicho por cómo tiraba de Urano antes de que nadie lo viera en 1846.",
                "Sombre, froide et venteuse : Neptune a les vents les plus rapides du système solaire, plus de 2 000 km/h. Ce fut la première planète trouvée par les mathématiques, prédite d'après la façon dont elle tirait sur Uranus avant que quiconque ne la voie en 1846.");
            A("Diameter: 49,244 km", "지름: 49,244 km", "直径：49,244 km", "Diámetro: 49.244 km", "Diamètre : 49 244 km");
            A("From the Sun: 4.5 billion km", "태양까지: 45억 km", "太陽から：45億 km", "Distancia al Sol: 4500 millones de km", "Distance au Soleil : 4,5 milliards de km");
            A("Year: 165 Earth years", "1년: 지구 시간으로 165년", "1年：地球の165年", "Año: 165 años terrestres", "Année : 165 années terrestres");
            A("Day: 16 hours 6 minutes", "하루: 16시간 6분", "1日：16時間6分", "Día: 16 horas 6 minutos", "Jour : 16 heures 6 minutes");
            A("Moons: 16 known, Triton the largest", "위성: 16개 확인, 가장 큰 것은 트리톤", "衛星：16個が既知、最大はトリトン", "Lunas: 16 conocidas, Tritón la mayor", "Lunes : 16 connues, Triton la plus grande");
            A("Visited once, by Voyager 2 in 1989", "1989년 보이저 2호가 단 한 번 방문", "1989年にボイジャー2号が一度だけ訪問", "Visitado una sola vez, por la Voyager 2 en 1989", "Visitée une seule fois, par Voyager 2 en 1989");

            A("Dwarf planet in the Kuiper Belt", "카이퍼 벨트의 왜행성", "カイパーベルトの準惑星", "Planeta enano del cinturón de Kuiper", "Planète naine de la ceinture de Kuiper");
            A("Counted as the ninth planet until 2006, Pluto is now a dwarf planet: a world of nitrogen ice, mountains of water ice and a hazy blue sky. NASA's New Horizons flew past in 2015 and found a bright heart-shaped plain, Tombaugh Regio.",
                "2006년까지 아홉 번째 행성으로 꼽혔던 명왕성은 이제 왜행성입니다. 질소 얼음, 물 얼음으로 된 산, 뿌연 푸른 하늘을 가진 세계죠. NASA의 뉴호라이즌스가 2015년에 곁을 지나가며 밝은 하트 모양의 평원, 톰보 지역을 발견했습니다.",
                "2006年まで9番目の惑星とされていた冥王星は、今では準惑星です。窒素の氷、水の氷でできた山々、かすんだ青い空をもつ世界です。NASAのニュー・ホライズンズが2015年に近くを通過し、明るいハート形の平原、トンボー領域を見つけました。",
                "Considerado el noveno planeta hasta 2006, Plutón es ahora un planeta enano: un mundo de hielo de nitrógeno, montañas de hielo de agua y un cielo azul brumoso. La New Horizons de la NASA pasó junto a él en 2015 y encontró una brillante llanura con forma de corazón, Tombaugh Regio.",
                "Considérée comme la neuvième planète jusqu'en 2006, Pluton est aujourd'hui une planète naine : un monde de glace d'azote, de montagnes de glace d'eau et de ciel bleu brumeux. La sonde New Horizons de la NASA l'a survolée en 2015 et y a découvert une plaine claire en forme de cœur, Tombaugh Regio.");
            A("Diameter: 2,377 km, smaller than our Moon", "지름: 2,377 km, 우리 달보다 작음", "直径：2,377 km、月より小さい", "Diámetro: 2377 km, menor que nuestra Luna", "Diamètre : 2 377 km, plus petit que notre Lune");
            A("Average distance from the Sun: 5.9 billion km", "태양까지 평균 거리: 59억 km", "太陽からの平均距離：59億 km", "Distancia media al Sol: 5900 millones de km", "Distance moyenne au Soleil : 5,9 milliards de km");
            A("Year: 248 Earth years", "1년: 지구 시간으로 248년", "1年：地球の248年", "Año: 248 años terrestres", "Année : 248 années terrestres");
            A("Day: 6.4 Earth days", "하루: 지구일로 6.4일", "1日：地球の6.4日", "Día: 6,4 días terrestres", "Jour : 6,4 jours terrestres");
            A("Moons: 5, Charon the largest", "위성: 5개, 가장 큰 것은 카론", "衛星：5個、最大はカロン", "Lunas: 5, Caronte la mayor", "Lunes : 5, Charon la plus grande");
            A("New Horizons flew by on 14 July 2015", "2015년 7월 14일 뉴호라이즌스가 근접 비행", "2015年7月14日にニュー・ホライズンズが通過", "La New Horizons pasó junto a él el 14 de julio de 2015", "New Horizons l'a survolée le 14 juillet 2015");

            // ---- the Sun and the Earth (OrbitSolar)
            A("Our star, a yellow dwarf", "우리의 별, 황색 왜성", "私たちの恒星、黄色矮星", "Nuestra estrella, una enana amarilla", "Notre étoile, une naine jaune");
            A("A ball of hot plasma holding 99.8 percent of the Solar System's mass. In its core, 15 million °C, it fuses about 600 million tonnes of hydrogen into helium every second, and the light that leaves its surface now reaches us in about eight minutes.",
                "태양계 질량의 99.8퍼센트를 차지하는 뜨거운 플라스마 덩어리입니다. 1,500만 °C인 중심핵에서 매초 약 6억 톤의 수소를 헬륨으로 융합하며, 지금 표면을 떠난 빛은 약 8분 뒤에 우리에게 닿습니다.",
                "太陽系の質量の99.8パーセントを占める高温のプラズマの球です。1,500万 °Cの中心核で毎秒約6億トンの水素をヘリウムに融合しており、いま表面を離れた光は約8分で私たちに届きます。",
                "Una bola de plasma caliente que reúne el 99,8 por ciento de la masa del sistema solar. En su núcleo, a 15 millones de °C, fusiona cada segundo unos 600 millones de toneladas de hidrógeno en helio, y la luz que sale ahora de su superficie nos llega en unos ocho minutos.",
                "Une boule de plasma brûlant qui contient 99,8 % de la masse du système solaire. En son cœur, à 15 millions de °C, elle fusionne chaque seconde environ 600 millions de tonnes d'hydrogène en hélium, et la lumière qui quitte sa surface maintenant nous parvient en environ huit minutes.");
            A("Diameter: 1.39 million km, 109 Earths across", "지름: 139만 km, 지구 109개 너비", "直径：139万 km、地球109個分", "Diámetro: 1,39 millones de km, 109 Tierras", "Diamètre : 1,39 million de km, 109 Terres");
            A("Surface: about 5,500 °C", "표면: 약 5,500 °C", "表面：約5,500 °C", "Superficie: unos 5500 °C", "Surface : environ 5 500 °C");
            A("Age: about 4.6 billion years", "나이: 약 46억 년", "年齢：約46億年", "Edad: unos 4600 millones de años", "Âge : environ 4,6 milliards d'années");
            A("Turns once in about 25 days at its equator", "적도에서 약 25일에 한 번 자전", "赤道で約25日に1回自転", "Gira una vez cada 25 días aproximadamente en su ecuador", "Fait un tour en environ 25 jours à l'équateur");
            A("Light to the Earth: 8 minutes 20 seconds", "빛이 지구까지 가는 시간: 8분 20초", "地球まで光で：8分20秒", "Luz hasta la Tierra: 8 minutos 20 segundos", "Lumière jusqu'à la Terre : 8 minutes 20 secondes");
            A("Parker Solar Probe flew closer than anything made by people (6.1 million km, 2024); SOHO and the Solar Dynamics Observatory watch it every day.",
                "파커 태양 탐사선이 인류가 만든 어떤 물체보다 가까이 날아갔고(610만 km, 2024년), SOHO와 태양 활동 관측 위성(SDO)이 매일 태양을 지켜봅니다.",
                "パーカー・ソーラー・プローブが人類の作ったどんな物よりも近くを飛び（610万 km、2024年）、SOHOとソーラー・ダイナミクス・オブザーバトリーが毎日見守っています。",
                "La sonda solar Parker voló más cerca que cualquier cosa hecha por el ser humano (6,1 millones de km, 2024); SOHO y el Observatorio de Dinámica Solar lo vigilan cada día.",
                "La sonde solaire Parker s'en est approchée plus que tout objet fabriqué par l'homme (6,1 millions de km, 2024) ; SOHO et le Solar Dynamics Observatory l'observent chaque jour.");
            A("Rocky planet, our home", "우리의 집인 암석 행성", "私たちのふるさと、岩石惑星", "Planeta rocoso, nuestro hogar", "Planète rocheuse, notre maison");
            A("The only world known to have life, and liquid water open on its surface. Its magnetic field and air shield that life from the Sun's wind and from space, and its one large Moon steadies its tilt, which keeps the seasons mild.",
                "생명이 있다고 알려진 유일한 세계이며, 표면에 액체 상태의 물이 드러나 있습니다. 자기장과 대기가 태양풍과 우주로부터 생명을 지켜 주고, 하나뿐인 커다란 달이 자전축의 기울기를 안정시켜 계절을 온화하게 유지합니다.",
                "生命が知られている唯一の世界で、地表には液体の水がむき出しになっています。磁場と大気が太陽風や宇宙からその生命を守り、ひとつの大きな月が自転軸の傾きを安定させて、季節を穏やかに保っています。",
                "El único mundo conocido con vida y con agua líquida al aire libre en su superficie. Su campo magnético y su atmósfera protegen esa vida del viento solar y del espacio, y su única gran Luna estabiliza su inclinación, lo que mantiene suaves las estaciones.",
                "Le seul monde connu à abriter la vie, avec de l'eau liquide à l'air libre à sa surface. Son champ magnétique et son atmosphère protègent cette vie du vent solaire et de l'espace, et sa grande Lune unique stabilise son inclinaison, ce qui garde des saisons douces.");
            A("Diameter: 12,742 km", "지름: 12,742 km", "直径：12,742 km", "Diámetro: 12.742 km", "Diamètre : 12 742 km");
            A("From the Sun: 149.6 million km (1 AU)", "태양까지: 1억 4,960만 km (1 AU)", "太陽から：1億4,960万 km（1 AU）", "Distancia al Sol: 149,6 millones de km (1 UA)", "Distance au Soleil : 149,6 millions de km (1 ua)");
            A("Year: 365.26 days", "1년: 365.26일", "1年：365.26日", "Año: 365,26 días", "Année : 365,26 jours");
            A("Day: 24 hours", "하루: 24시간", "1日：24時間", "Día: 24 horas", "Jour : 24 heures");
            A("Moons: 1", "위성: 1개", "衛星：1個", "Lunas: 1", "Lunes : 1");
            A("About 280 people from 23 countries have lived on the International Space Station since 2000; SpaceX's Crew Dragon has flown crews there since 2020.",
                "2000년부터 23개국 약 280명이 국제우주정거장에서 살았고, 2020년부터는 SpaceX의 크루 드래건이 승무원을 실어 나르고 있습니다.",
                "2000年以降、23か国の約280人が国際宇宙ステーションで暮らしてきました。2020年からはSpaceXのクルードラゴンがクルーを運んでいます。",
                "Unas 280 personas de 23 países han vivido en la Estación Espacial Internacional desde 2000; la Crew Dragon de SpaceX lleva tripulaciones allí desde 2020.",
                "Environ 280 personnes de 23 pays ont vécu à bord de la Station spatiale internationale depuis 2000 ; le Crew Dragon de SpaceX y transporte des équipages depuis 2020.");

            // ---- who went (OrbitSolar explorers)
            A("Mariner 10 (1974–75) and MESSENGER, which orbited it from 2011 to 2015; ESA and JAXA's BepiColombo arrives in 2026.",
                "매리너 10호(1974–75)와 2011년부터 2015년까지 궤도를 돈 메신저. ESA와 JAXA의 베피콜롬보가 2026년에 도착합니다.",
                "マリナー10号（1974–75年）と、2011年から2015年まで周回したメッセンジャー。ESAとJAXAのベピコロンボが2026年に到着します。",
                "La Mariner 10 (1974–75) y MESSENGER, que lo orbitó de 2011 a 2015; BepiColombo, de la ESA y la JAXA, llega en 2026.",
                "Mariner 10 (1974–75) et MESSENGER, en orbite de 2011 à 2015 ; BepiColombo, de l'ESA et de la JAXA, arrive en 2026.");
            A("The Soviet Venera landers sent the only pictures from its surface; NASA's Magellan mapped it by radar (1990–94).",
                "소련의 베네라 착륙선이 표면에서 찍은 유일한 사진을 보냈고, NASA의 마젤란이 레이더로 지도를 만들었습니다(1990–94).",
                "ソ連のベネラ着陸機が地表から唯一の写真を送り、NASAのマゼランがレーダーで地図を作りました（1990–94年）。",
                "Los módulos soviéticos Venera enviaron las únicas fotos de su superficie; la Magallanes de la NASA la cartografió por radar (1990–94).",
                "Les atterrisseurs soviétiques Venera ont envoyé les seules images de sa surface ; Magellan, de la NASA, l'a cartographiée au radar (1990–94).");
            A("NASA's rovers Sojourner, Spirit, Opportunity, Curiosity and Perseverance; the Ingenuity helicopter made the first powered flight on another world (2021).",
                "NASA의 로버 소저너, 스피릿, 오퍼튜니티, 큐리오시티, 퍼서비어런스. 인저뉴어티 헬리콥터는 다른 세계에서 처음으로 동력 비행을 했습니다(2021년).",
                "NASAの探査車ソジャーナ、スピリット、オポチュニティ、キュリオシティ、パーサヴィアランス。ヘリコプターのインジェニュイティは地球以外の世界で初めて動力飛行をしました（2021年）。",
                "Los róveres de la NASA Sojourner, Spirit, Opportunity, Curiosity y Perseverance; el helicóptero Ingenuity hizo el primer vuelo propulsado en otro mundo (2021).",
                "Les rovers de la NASA Sojourner, Spirit, Opportunity, Curiosity et Perseverance ; l'hélicoptère Ingenuity a réalisé le premier vol motorisé sur un autre monde (2021).");
            A("Pioneers 10 and 11, Voyagers 1 and 2, Galileo (orbited 1995–2003), Juno (in orbit since 2016); Europa Clipper arrives in 2030.",
                "파이어니어 10·11호, 보이저 1·2호, 갈릴레오(1995–2003년 궤도 비행), 주노(2016년부터 궤도 비행 중). 유로파 클리퍼가 2030년에 도착합니다.",
                "パイオニア10号・11号、ボイジャー1号・2号、ガリレオ（1995–2003年に周回）、ジュノー（2016年から周回中）。エウロパ・クリッパーが2030年に到着します。",
                "Las Pioneer 10 y 11, las Voyager 1 y 2, Galileo (en órbita de 1995 a 2003), Juno (en órbita desde 2016); Europa Clipper llega en 2030.",
                "Pioneer 10 et 11, Voyager 1 et 2, Galileo (en orbite de 1995 à 2003), Juno (en orbite depuis 2016) ; Europa Clipper arrive en 2030.");
            A("Pioneer 11, Voyagers 1 and 2, and Cassini, which orbited it for 13 years and dropped the Huygens probe onto Titan in 2005.",
                "파이어니어 11호, 보이저 1·2호, 그리고 13년 동안 궤도를 돌며 2005년 하위헌스 탐사선을 타이탄에 내려보낸 카시니.",
                "パイオニア11号、ボイジャー1号・2号、そして13年間周回し、2005年にホイヘンス・プローブをタイタンに降ろしたカッシーニ。",
                "La Pioneer 11, las Voyager 1 y 2, y Cassini, que lo orbitó durante 13 años y dejó caer la sonda Huygens sobre Titán en 2005.",
                "Pioneer 11, Voyager 1 et 2, et Cassini, qui l'a orbitée pendant 13 ans et a largué la sonde Huygens sur Titan en 2005.");
            A("Only Voyager 2, which flew past in January 1986.", "1986년 1월에 곁을 지나간 보이저 2호뿐입니다.", "1986年1月に通過したボイジャー2号だけです。", "Solo la Voyager 2, que pasó junto a él en enero de 1986.", "Seulement Voyager 2, qui l'a survolée en janvier 1986.");
            A("Only Voyager 2, which flew past in August 1989.", "1989년 8월에 곁을 지나간 보이저 2호뿐입니다.", "1989年8月に通過したボイジャー2号だけです。", "Solo la Voyager 2, que pasó junto a él en agosto de 1989.", "Seulement Voyager 2, qui l'a survolée en août 1989.");
            A("NASA's New Horizons flew past on 14 July 2015 and showed its heart-shaped plain.", "NASA의 뉴호라이즌스가 2015년 7월 14일에 곁을 지나가며 하트 모양의 평원을 보여 주었습니다.", "NASAのニュー・ホライズンズが2015年7月14日に通過し、ハート形の平原を見せてくれました。", "La New Horizons de la NASA pasó el 14 de julio de 2015 y mostró su llanura en forma de corazón.", "New Horizons, de la NASA, l'a survolée le 14 juillet 2015 et a révélé sa plaine en forme de cœur.");

            // ---- the moons
            A("Probably made from the debris of a Mars-sized world hitting the young Earth. Twelve people walked on it on Apollo missions, 1969 to 1972, first Neil Armstrong and Buzz Aldrin.",
                "아마도 화성만 한 천체가 어린 지구에 부딪혀 생긴 파편으로 만들어졌을 것입니다. 1969년부터 1972년까지 아폴로 임무로 열두 명이 달 위를 걸었고, 처음은 닐 암스트롱과 버즈 올드린이었습니다.",
                "おそらく火星ほどの天体が若い地球に衝突したときの破片からできました。1969年から1972年のアポロ計画で12人が月面を歩き、最初はニール・アームストロングとバズ・オルドリンでした。",
                "Probablemente se formó con los restos de un mundo del tamaño de Marte que chocó contra la joven Tierra. Doce personas caminaron sobre ella en las misiones Apolo, de 1969 a 1972, los primeros Neil Armstrong y Buzz Aldrin.",
                "Probablement formée des débris d'un monde de la taille de Mars qui a percuté la jeune Terre. Douze personnes y ont marché lors des missions Apollo, de 1969 à 1972, d'abord Neil Armstrong et Buzz Aldrin.");
            A("A lumpy moon about 22 km across, so close it circles three times a day, rising in the west. It is slowly spiralling in.",
                "지름 약 22 km의 울퉁불퉁한 위성으로, 아주 가까워서 하루에 세 번 돌며 서쪽에서 떠오릅니다. 천천히 나선을 그리며 화성으로 다가가고 있습니다.",
                "直径約22 kmのでこぼこした衛星で、とても近いため1日に3回まわり、西から昇ります。少しずつ渦を巻くように火星へ近づいています。",
                "Una luna irregular de unos 22 km de ancho, tan cercana que da tres vueltas al día y sale por el oeste. Poco a poco se acerca en espiral.",
                "Une lune bosselée d'environ 22 km de large, si proche qu'elle fait trois tours par jour et se lève à l'ouest. Elle se rapproche lentement en spirale.");
            A("The smaller and farther moon, about 12 km across, smoothed by dust. Both may be captured asteroids.",
                "더 작고 더 먼 위성으로, 지름 약 12 km이며 먼지에 덮여 매끈합니다. 두 위성 모두 붙잡힌 소행성일지도 모릅니다.",
                "小さくて遠いほうの衛星で、直径約12 km、塵に覆われてなめらかです。どちらも捕獲された小惑星かもしれません。",
                "La luna más pequeña y lejana, de unos 12 km, alisada por el polvo. Ambas podrían ser asteroides capturados.",
                "La lune la plus petite et la plus lointaine, environ 12 km de large, lissée par la poussière. Les deux sont peut-être des astéroïdes capturés.");
            A("The most volcanic world known, with hundreds of volcanoes, squeezed and heated by Jupiter's pull.",
                "알려진 가장 화산 활동이 활발한 세계로, 수백 개의 화산이 있으며 목성의 인력에 쥐어짜이며 달궈집니다.",
                "知られている中で最も火山活動が活発な世界で、何百もの火山があり、木星の引力にもまれて熱せられています。",
                "El mundo más volcánico que se conoce, con cientos de volcanes, comprimido y calentado por la atracción de Júpiter.",
                "Le monde le plus volcanique connu, avec des centaines de volcans, comprimé et chauffé par l'attraction de Jupiter.");
            A("Under its cracked ice shell is an ocean with more water than all of Earth's: one of the best places to look for life.",
                "갈라진 얼음 껍질 아래에 지구의 모든 물보다 많은 물을 담은 바다가 있습니다. 생명을 찾기에 가장 좋은 곳 중 하나입니다.",
                "ひび割れた氷の殻の下には、地球の水をすべて合わせたより多くの水をたたえた海があります。生命を探すのに最も有望な場所のひとつです。",
                "Bajo su capa de hielo agrietada hay un océano con más agua que toda la de la Tierra: uno de los mejores lugares para buscar vida.",
                "Sous sa croûte de glace fissurée se trouve un océan contenant plus d'eau que toute celle de la Terre : l'un des meilleurs endroits où chercher la vie.");
            A("The largest moon in the Solar System, bigger than Mercury, and the only moon with its own magnetic field.",
                "태양계에서 가장 큰 위성으로 수성보다 크며, 자체 자기장을 가진 유일한 위성입니다.",
                "太陽系最大の衛星で水星より大きく、独自の磁場をもつ唯一の衛星です。",
                "La luna más grande del sistema solar, mayor que Mercurio, y la única con campo magnético propio.",
                "La plus grande lune du système solaire, plus grande que Mercure, et la seule à posséder son propre champ magnétique.");
            A("The most heavily cratered surface known: an ancient face, barely changed in four billion years.",
                "알려진 것 중 크레이터가 가장 빽빽한 표면으로, 40억 년 동안 거의 변하지 않은 오래된 얼굴입니다.",
                "知られている中で最もクレーターだらけの表面。40億年ほとんど変わっていない古い顔です。",
                "La superficie con más cráteres que se conoce: un rostro antiguo, casi sin cambios en cuatro mil millones de años.",
                "La surface la plus criblée de cratères connue : un visage ancien, à peine changé en quatre milliards d'années.");
            A("Its giant crater Herschel makes it look like the Death Star.", "거대한 허셜 크레이터 때문에 ‘데스 스타’처럼 보입니다.", "巨大なハーシェル・クレーターのせいで「デス・スター」のように見えます。", "Su enorme cráter Herschel le da el aspecto de la Estrella de la Muerte.", "Son cratère géant Herschel lui donne l'air de l'Étoile de la mort.");
            A("Geysers at its south pole spray water from an ocean under the ice into space; Cassini flew through them.",
                "남극의 간헐천이 얼음 아래 바다의 물을 우주로 뿜어내며, 카시니가 그 사이를 날아 통과했습니다.",
                "南極の間欠泉が氷の下の海の水を宇宙へ噴き上げ、カッシーニはその中を通り抜けました。",
                "Los géiseres de su polo sur lanzan al espacio agua de un océano bajo el hielo; Cassini los atravesó.",
                "Des geysers à son pôle Sud projettent dans l'espace l'eau d'un océan sous la glace ; Cassini les a traversés.");
            A("Almost pure water ice, with a canyon running three-quarters of the way round it.", "거의 순수한 물 얼음으로 되어 있고, 둘레의 4분의 3을 가로지르는 협곡이 있습니다.", "ほぼ純粋な水の氷でできていて、周囲の4分の3にわたる峡谷があります。", "Casi hielo de agua puro, con un cañón que recorre tres cuartas partes de su contorno.", "Presque de la glace d'eau pure, avec un canyon qui en fait les trois quarts du tour.");
            A("Bright ice cliffs streak its trailing side.", "공전 방향 뒤쪽 면에 밝은 얼음 절벽이 줄무늬처럼 나 있습니다.", "公転の後ろ側の面に、明るい氷の崖が筋のように走っています。", "Brillantes acantilados de hielo surcan su cara posterior.", "De brillantes falaises de glace strient sa face arrière.");
            A("Saturn's second-largest moon, an icy ball heavily cratered.", "토성에서 두 번째로 큰 위성으로, 크레이터투성이인 얼음 공입니다.", "土星で2番目に大きな衛星で、クレーターだらけの氷の球です。", "La segunda luna más grande de Saturno, una bola helada llena de cráteres.", "La deuxième plus grande lune de Saturne, une boule glacée criblée de cratères.");
            A("Bigger than Mercury, with thick orange air and lakes of liquid methane: the only moon with a real atmosphere. NASA's Dragonfly will fly there.",
                "수성보다 크고, 두꺼운 주황색 대기와 액체 메탄 호수가 있습니다. 진짜 대기를 가진 유일한 위성이죠. NASA의 드래곤플라이가 그곳을 날게 됩니다.",
                "水星より大きく、厚いオレンジ色の大気と液体メタンの湖があります。本物の大気をもつ唯一の衛星です。NASAのドラゴンフライがそこを飛ぶ予定です。",
                "Mayor que Mercurio, con una espesa atmósfera anaranjada y lagos de metano líquido: la única luna con una atmósfera de verdad. Dragonfly, de la NASA, volará allí.",
                "Plus grande que Mercure, avec un air orange épais et des lacs de méthane liquide : la seule lune dotée d'une vraie atmosphère. Dragonfly, de la NASA, y volera.");
            A("Two-toned: one side dark as coal, the other bright as snow.", "두 가지 색을 띱니다. 한쪽은 석탄처럼 어둡고, 다른 쪽은 눈처럼 밝습니다.", "ツートンカラーで、片側は石炭のように暗く、反対側は雪のように明るいのです。", "De dos tonos: una cara oscura como el carbón y la otra brillante como la nieve.", "Bicolore : une face sombre comme le charbon, l'autre claire comme la neige.");
            A("A patchwork world with cliffs about 20 km high, the tallest known.", "조각보 같은 세계로, 높이 약 20 km의 절벽은 알려진 것 중 가장 높습니다.", "つぎはぎのような世界で、高さ約20 kmの崖は知られている中で最も高いものです。", "Un mundo hecho de retazos con acantilados de unos 20 km de altura, los más altos conocidos.", "Un monde en patchwork aux falaises d'environ 20 km de haut, les plus hautes connues.");
            A("The brightest of Uranus's large moons, with long valleys.", "천왕성의 큰 위성 중 가장 밝으며, 긴 계곡이 있습니다.", "天王星の大きな衛星の中で最も明るく、長い谷があります。", "La más brillante de las grandes lunas de Urano, con largos valles.", "La plus brillante des grandes lunes d'Uranus, aux longues vallées.");
            A("The darkest of them, with a mysterious bright ring on one crater.", "그중 가장 어둡고, 한 크레이터에 수수께끼 같은 밝은 고리가 있습니다.", "その中で最も暗く、あるクレーターに謎の明るい輪があります。", "La más oscura de ellas, con un misterioso anillo brillante en un cráter.", "La plus sombre d'entre elles, avec un mystérieux anneau clair sur un cratère.");
            A("Uranus's largest moon, cut by huge faults.", "천왕성에서 가장 큰 위성으로, 거대한 단층이 가로지릅니다.", "天王星最大の衛星で、巨大な断層が走っています。", "La luna más grande de Urano, surcada por enormes fallas.", "La plus grande lune d'Uranus, entaillée d'immenses failles.");
            A("The outermost large moon, old and cratered.", "가장 바깥쪽의 큰 위성으로, 오래되고 크레이터가 많습니다.", "最も外側の大きな衛星で、古くクレーターに覆われています。", "La gran luna más exterior, antigua y llena de cráteres.", "La grande lune la plus extérieure, ancienne et cratérisée.");
            A("One of the darkest objects known, almost as big as a body can be without being pulled round.",
                "알려진 가장 어두운 천체 중 하나로, 자체 중력에 둥글게 끌어당겨지지 않고 버틸 수 있는 거의 최대 크기입니다.",
                "知られている中で最も暗い天体のひとつで、自分の重力で丸くならずにいられる限界近くの大きさです。",
                "Uno de los objetos más oscuros conocidos, casi tan grande como puede ser un cuerpo sin que su gravedad lo vuelva redondo.",
                "L'un des objets les plus sombres connus, presque aussi grand qu'un corps peut l'être sans que sa gravité ne l'arrondisse.");
            A("Goes round backwards, so it was probably captured from the Kuiper Belt. Voyager 2 saw geysers of nitrogen on it.",
                "거꾸로 돌기 때문에 아마도 카이퍼 벨트에서 붙잡혀 왔을 것입니다. 보이저 2호가 그곳에서 질소 간헐천을 보았습니다.",
                "逆向きに回っているので、おそらくカイパーベルトから捕獲されたのでしょう。ボイジャー2号は窒素の間欠泉を目撃しました。",
                "Gira al revés, así que probablemente fue capturada del cinturón de Kuiper. La Voyager 2 vio en ella géiseres de nitrógeno.",
                "Elle tourne à l'envers, elle a donc sans doute été capturée dans la ceinture de Kuiper. Voyager 2 y a vu des geysers d'azote.");
            A("On a long, stretched orbit that takes almost a year.", "길게 늘어진 궤도를 돌며, 한 바퀴에 거의 1년이 걸립니다.", "細長く引き伸ばされた軌道を回り、一周にほぼ1年かかります。", "En una órbita larga y alargada que tarda casi un año.", "Sur une orbite longue et étirée qui dure presque un an.");
            A("Half Pluto's size: the two always face each other and circle a point between them.", "명왕성의 절반 크기로, 둘은 언제나 서로 마주 보며 그 사이의 한 점을 중심으로 돕니다.", "冥王星の半分の大きさで、2つはいつも向かい合い、その間の一点を中心に回っています。", "La mitad del tamaño de Plutón: los dos siempre se miran de frente y giran en torno a un punto entre ellos.", "La moitié de la taille de Pluton : les deux se font toujours face et tournent autour d'un point situé entre eux.");

            // ---- the Solar System card (HoloSolar)
            A("The Solar System", "태양계", "太陽系", "El sistema solar", "Le système solaire");
            A("The Sun, eight planets and the dwarf planet Pluto", "태양, 여덟 행성, 그리고 왜행성 명왕성", "太陽、8つの惑星、そして準惑星の冥王星", "El Sol, ocho planetas y el planeta enano Plutón", "Le Soleil, huit planètes et la planète naine Pluton");
            A("Everything here goes round the Sun, the inner planets fastest. Poke or pinch any world, or use the arrows, to bring it up close with its biggest moons.",
                "여기 있는 모든 것은 태양 주위를 돌며, 안쪽 행성일수록 빠릅니다. 아무 천체나 찌르거나 집거나 화살표를 써서 가장 큰 위성들과 함께 가까이 불러오세요.",
                "ここにあるものはすべて太陽の周りを回り、内側の惑星ほど速く回ります。どれかの天体をつつくかつまむか、矢印を使うと、大きな衛星と一緒に間近に表示されます。",
                "Todo aquí gira alrededor del Sol, los planetas interiores más rápido. Toca o pellizca cualquier mundo, o usa las flechas, para verlo de cerca con sus lunas más grandes.",
                "Tout ici tourne autour du Soleil, les planètes intérieures le plus vite. Touchez ou pincez un monde, ou utilisez les flèches, pour le voir de près avec ses plus grandes lunes.");
            A("Sizes, distances and speeds are not to scale: true to scale, the Earth would be a grain of sand 10 metres from a beach-ball Sun.",
                "크기, 거리, 속도는 실제 비율이 아닙니다. 실제 비율이라면 지구는 비치볼만 한 태양에서 10미터 떨어진 모래알 하나일 것입니다.",
                "大きさ・距離・速さは実際の比率ではありません。実際の比率なら、地球はビーチボール大の太陽から10メートル離れた砂粒ひとつです。",
                "Tamaños, distancias y velocidades no están a escala: a escala real, la Tierra sería un grano de arena a 10 metros de un Sol del tamaño de una pelota de playa.",
                "Tailles, distances et vitesses ne sont pas à l'échelle : à l'échelle, la Terre serait un grain de sable à 10 mètres d'un Soleil grand comme un ballon de plage.");
            A("Figures from NASA's planetary fact sheets and NASA/JPL.", "수치 출처: NASA 행성 자료표와 NASA/JPL.", "数値はNASAの惑星ファクトシートとNASA/JPLによる。", "Datos de las fichas planetarias de la NASA y de NASA/JPL.", "Chiffres tirés des fiches planétaires de la NASA et de NASA/JPL.");
            A("MOON OF {0}", "{0}의 위성", "{0}の衛星", "{0} · LUNA", "{0} · LUNE");
            A("{0:N0} km across", "지름 {0:N0} km", "直径{0:N0} km", "{0:N0} km de ancho", "{0:N0} km de large");
            A("Distance from {0}: {1:N0} km", "{0}에서의 거리: {1:N0} km", "{0}からの距離：{1:N0} km", "Distancia al planeta: {1:N0} km", "Distance à la planète : {1:N0} km");
            A("Goes round every {0} days", "{0}일마다 한 바퀴", "{0}日で一周", "Da una vuelta cada {0} días", "Fait le tour en {0} jours");
            A("Goes round every {0} days, backwards", "{0}일마다 거꾸로 한 바퀴", "{0}日で逆向きに一周", "Da una vuelta cada {0} días, al revés", "Fait le tour en {0} jours, à l'envers");
            A("Pick the planet to go back to it.", "행성을 고르면 다시 돌아갑니다.", "惑星を選ぶと戻ります。", "Elige el planeta para volver a él.", "Choisissez la planète pour y revenir.");
            A("Shown circling it: {0}", "주위를 도는 위성: {0}", "周りを回る衛星：{0}", "Girando a su alrededor: {0}", "En orbite autour : {0}");
            A("Explorers: {0}", "탐사: {0}", "探査：{0}", "Exploración: {0}", "Exploration : {0}");
            A("Moon sizes true to the planet; their distances not to scale.", "위성 크기는 행성과 실제 비율이며, 거리는 실제 비율이 아닙니다.", "衛星の大きさは惑星と同じ比率ですが、距離は実際の比率ではありません。", "Tamaño de las lunas a escala con el planeta; sus distancias, no.", "Taille des lunes à l'échelle de la planète ; leurs distances, non.");
            A("SOLAR SYSTEM · OUR STAR", "태양계 · 우리의 별", "太陽系 · 私たちの恒星", "SISTEMA SOLAR · NUESTRA ESTRELLA", "SYSTÈME SOLAIRE · NOTRE ÉTOILE");
            A("SOLAR SYSTEM · DWARF PLANET", "태양계 · 왜행성", "太陽系 · 準惑星", "SISTEMA SOLAR · PLANETA ENANO", "SYSTÈME SOLAIRE · PLANÈTE NAINE");
            A("SOLAR SYSTEM · PLANET {0} FROM THE SUN", "태양계 · 태양에서 {0}번째 행성", "太陽系 · 太陽から{0}番目の惑星", "SISTEMA SOLAR · PLANETA N.º {0} DESDE EL SOL", "SYSTÈME SOLAIRE · PLANÈTE N° {0} DEPUIS LE SOLEIL");

            // ================================================================== the Universe size steps (OrbitSolar.Steps, HoloUniverse)
            A("Our Moon is about a quarter of the Earth's width. Start here and keep going.", "우리 달은 지구 너비의 약 4분의 1입니다. 여기서 시작해 계속 나아가 보세요.", "月の幅は地球の約4分の1。ここから始めて、どんどん進みましょう。", "Nuestra Luna mide como una cuarta parte del ancho de la Tierra. Empieza aquí y sigue adelante.", "Notre Lune fait environ un quart de la largeur de la Terre. Commencez ici et continuez.");
            A("Planet", "행성", "惑星", "Planeta", "Planète");
            A("Our home. Fifty Moons would fit inside it.", "우리의 집. 달 50개가 들어갈 수 있습니다.", "私たちのふるさと。月が50個入ります。", "Nuestro hogar. Cabrían cincuenta Lunas dentro.", "Notre maison. Cinquante Lunes y tiendraient.");
            A("Ice giant", "얼음 거인", "巨大氷惑星", "Gigante helado", "Géante de glace");
            A("About four Earths wide; nearly 58 Earths would fit inside.", "너비는 지구 약 4개, 안에는 지구가 거의 58개 들어갑니다.", "幅は地球約4個分、中には地球がほぼ58個入ります。", "Unas cuatro Tierras de ancho; cabrían casi 58 Tierras dentro.", "Environ quatre Terres de large ; près de 58 Terres y tiendraient.");
            A("Gas giant", "가스 거인", "巨大ガス惑星", "Gigante gaseoso", "Géante gazeuse");
            A("The largest planet: 11 Earths across, and more than 1,300 Earths would fit inside it.", "가장 큰 행성. 너비는 지구 11개, 안에는 지구가 1,300개 넘게 들어갑니다.", "最大の惑星。幅は地球11個分、中には地球が1,300個以上入ります。", "El planeta más grande: 11 Tierras de ancho, y cabrían más de 1300 Tierras dentro.", "La plus grande planète : 11 Terres de large, et plus de 1 300 Terres y tiendraient.");
            A("Star (yellow dwarf)", "항성(황색 왜성)", "恒星（黄色矮星）", "Estrella (enana amarilla)", "Étoile (naine jaune)");
            A("Ten Jupiters across, 109 Earths: about 1.3 million Earths would fit inside. And it is an ordinary star.", "목성 10개, 지구 109개 너비로, 안에는 지구가 약 130만 개 들어갑니다. 그런데도 평범한 별이죠.", "幅は木星10個分、地球109個分で、中には地球が約130万個入ります。それでもごく普通の恒星です。", "Diez Júpiter de ancho, 109 Tierras: cabrían dentro alrededor de 1,3 millones de Tierras. Y es una estrella corriente.", "Dix Jupiter de large, 109 Terres : environ 1,3 million de Terres y tiendraient. Et c'est une étoile ordinaire.");
            A("Star, 8.6 light-years away", "항성, 8.6광년 거리", "恒星、8.6光年先", "Estrella, a 8,6 años luz", "Étoile, à 8,6 années-lumière");
            A("The brightest star in our night sky, about 1.7 times the Sun's width and 25 times as bright.", "밤하늘에서 가장 밝은 별로, 너비는 태양의 약 1.7배, 밝기는 25배입니다.", "夜空で最も明るい星で、幅は太陽の約1.7倍、明るさは25倍です。", "La estrella más brillante de nuestro cielo nocturno, unas 1,7 veces el ancho del Sol y 25 veces más luminosa.", "L'étoile la plus brillante de notre ciel nocturne, environ 1,7 fois la largeur du Soleil et 25 fois plus lumineuse.");
            A("Orange giant, 34 light-years away", "주황색 거성, 34광년 거리", "オレンジ色の巨星、34光年先", "Gigante naranja, a 34 años luz", "Géante orange, à 34 années-lumière");
            A("A giant star in Gemini about 9 times the Sun's width.", "쌍둥이자리의 거성으로, 너비가 태양의 약 9배입니다.", "ふたご座の巨星で、幅は太陽の約9倍です。", "Una estrella gigante de Géminis, unas 9 veces el ancho del Sol.", "Une étoile géante des Gémeaux, environ 9 fois la largeur du Soleil.");
            A("Red giant, 37 light-years away", "적색 거성, 37광년 거리", "赤色巨星、37光年先", "Gigante roja, a 37 años luz", "Géante rouge, à 37 années-lumière");
            A("About 25 Suns across. Our Sun will swell into a giant like this in about five billion years.", "너비는 태양 약 25개. 우리 태양도 약 50억 년 뒤에는 이런 거성으로 부풀어 오를 것입니다.", "幅は太陽約25個分。私たちの太陽も約50億年後には、このような巨星にふくらみます。", "Unos 25 Soles de ancho. Nuestro Sol se hinchará hasta ser una gigante así dentro de unos cinco mil millones de años.", "Environ 25 Soleils de large. Notre Soleil gonflera en une géante comme celle-ci dans environ cinq milliards d'années.");
            A("Blue supergiant, about 860 light-years away", "청색 초거성, 약 860광년 거리", "青色超巨星、約860光年先", "Supergigante azul, a unos 860 años luz", "Supergéante bleue, à environ 860 années-lumière");
            A("Orion's bright foot: around 79 Suns wide and some 100,000 times as bright.", "오리온자리의 밝은 발. 너비는 태양 약 79개, 밝기는 약 10만 배입니다.", "オリオン座の明るい足。幅は太陽約79個分、明るさは約10万倍です。", "El pie brillante de Orión: unos 79 Soles de ancho y unas 100.000 veces más luminosa.", "Le pied brillant d'Orion : environ 79 Soleils de large et quelque 100 000 fois plus lumineuse.");
            A("Red supergiant, about 550 light-years away", "적색 초거성, 약 550광년 거리", "赤色超巨星、約550光年先", "Supergigante roja, a unos 550 años luz", "Supergéante rouge, à environ 550 années-lumière");
            A("Orion's red shoulder. If it sat where the Sun is, it would reach past the orbit of Mars. Estimates range from about 640 to 1,000 Suns across.",
                "오리온자리의 붉은 어깨. 태양 자리에 놓으면 화성 궤도 너머까지 닿습니다. 너비 추정치는 태양 약 640개에서 1,000개까지 다양합니다.",
                "オリオン座の赤い肩。太陽の位置に置くと、火星の軌道の外まで届きます。幅の推定は太陽約640個分から1,000個分までさまざまです。",
                "El hombro rojo de Orión. Si estuviera donde está el Sol, llegaría más allá de la órbita de Marte. Las estimaciones van de unos 640 a 1000 Soles de ancho.",
                "L'épaule rouge d'Orion. Placée à la place du Soleil, elle dépasserait l'orbite de Mars. Les estimations vont d'environ 640 à 1 000 Soleils de large.");
            A("Red hypergiant, about 3,900 light-years away", "적색 극대거성, 약 3,900광년 거리", "赤色極超巨星、約3,900光年先", "Hipergigante roja, a unos 3900 años luz", "Hypergéante rouge, à environ 3 900 années-lumière");
            A("One of the largest stars known, around 1,400 Suns wide: light would take nearly two hours to cross it.", "알려진 가장 큰 별 중 하나로, 너비는 태양 약 1,400개입니다. 빛이 가로지르는 데 거의 두 시간이 걸립니다.", "知られている中で最大級の恒星で、幅は太陽約1,400個分。光でも横切るのにほぼ2時間かかります。", "Una de las estrellas más grandes conocidas, de unos 1400 Soles de ancho: la luz tardaría casi dos horas en cruzarla.", "L'une des plus grandes étoiles connues, environ 1 400 Soleils de large : la lumière mettrait près de deux heures à la traverser.");
            A("Red supergiant, about 19,000 light-years away", "적색 초거성, 약 19,000광년 거리", "赤色超巨星、約19,000光年先", "Supergigante roja, a unos 19.000 años luz", "Supergéante rouge, à environ 19 000 années-lumière");
            A("Possibly the largest star known, perhaps 2,150 Suns across, wider than Saturn's orbit. Its size is very uncertain.", "알려진 가장 큰 별일 수도 있으며, 너비는 태양 약 2,150개로 토성 궤도보다 넓을지도 모릅니다. 크기는 매우 불확실합니다.", "知られている中で最大の恒星かもしれず、幅は太陽約2,150個分と、土星の軌道より広い可能性があります。大きさはとても不確かです。", "Posiblemente la estrella más grande conocida, quizá de 2150 Soles de ancho, más que la órbita de Saturno. Su tamaño es muy incierto.", "Peut-être la plus grande étoile connue, environ 2 150 Soleils de large, plus que l'orbite de Saturne. Sa taille est très incertaine.");
            A("The Sun and its planets, out to Neptune", "태양과 행성들, 해왕성까지", "太陽とその惑星、海王星まで", "El Sol y sus planetas, hasta Neptuno", "Le Soleil et ses planètes, jusqu'à Neptune");
            A("Neptune's orbit is 9 billion km across: light takes about 8 hours to cross it. Stephenson 2-18 would fill a third of it.", "해왕성 궤도의 지름은 90억 km로, 빛이 가로지르는 데 약 8시간이 걸립니다. 스티븐슨 2-18은 그 3분의 1을 채웁니다.", "海王星の軌道は直径90億 km。光で横切るのに約8時間かかります。スティーブンソン2-18はその3分の1を占めます。", "La órbita de Neptuno mide 9000 millones de km de ancho: la luz tarda unas 8 horas en cruzarla. Stephenson 2-18 ocuparía un tercio.", "L'orbite de Neptune fait 9 milliards de km de large : la lumière met environ 8 heures à la traverser. Stephenson 2-18 en remplirait un tiers.");
            A("A shell of icy bodies round the Sun", "태양을 둘러싼 얼음 천체들의 껍질", "太陽を取り巻く氷の天体の殻", "Una capa de cuerpos helados alrededor del Sol", "Une coquille de corps glacés autour du Soleil");
            A("Trillions of comets may circle far beyond the planets, out to perhaps 100,000 times the Earth's distance from the Sun, more than a light-year. No one has seen it directly yet.",
                "수조 개의 혜성이 행성들 훨씬 너머, 아마도 지구와 태양 사이 거리의 10만 배, 1광년이 넘는 곳까지 돌고 있을 것입니다. 아직 아무도 직접 본 적은 없습니다.",
                "何兆個もの彗星が惑星のはるか外側、地球と太陽の距離のおそらく10万倍、1光年以上先まで回っているかもしれません。まだ誰も直接見たことはありません。",
                "Billones de cometas podrían girar mucho más allá de los planetas, hasta quizá 100.000 veces la distancia de la Tierra al Sol, más de un año luz. Nadie la ha visto directamente todavía.",
                "Des milliers de milliards de comètes tourneraient bien au-delà des planètes, jusqu'à peut-être 100 000 fois la distance Terre-Soleil, plus d'une année-lumière. Personne ne l'a encore vu directement.");
            A("Our galaxy, a barred spiral", "우리 은하, 막대나선은하", "私たちの銀河、棒渦巻銀河", "Nuestra galaxia, una espiral barrada", "Notre galaxie, une spirale barrée");
            A("Some 100 to 400 billion stars in a disc about 100,000 light-years across. The Sun is about 26,000 light-years from the centre and takes 230 million years to go round once.",
                "지름 약 10만 광년의 원반에 1,000억에서 4,000억 개의 별이 있습니다. 태양은 중심에서 약 26,000광년 떨어져 있고, 한 바퀴 도는 데 2억 3천만 년이 걸립니다.",
                "直径約10万光年の円盤に、1,000億から4,000億個の星があります。太陽は中心から約26,000光年離れていて、一周するのに2億3,000万年かかります。",
                "Entre 100.000 y 400.000 millones de estrellas en un disco de unos 100.000 años luz de ancho. El Sol está a unos 26.000 años luz del centro y tarda 230 millones de años en dar una vuelta.",
                "Quelque 100 à 400 milliards d'étoiles dans un disque d'environ 100 000 années-lumière de large. Le Soleil est à environ 26 000 années-lumière du centre et met 230 millions d'années à en faire le tour.");
            A("Our neighbourhood of galaxies", "우리 은하의 이웃들", "私たちのご近所の銀河", "Nuestro vecindario de galaxias", "Notre voisinage de galaxies");
            A("More than 80 galaxies within about 10 million light-years, led by the Milky Way and Andromeda, which will merge in a few billion years.",
                "약 1,000만 광년 안에 있는 80개가 넘는 은하로, 우리은하와 안드로메다가 이끌며 두 은하는 수십억 년 뒤 합쳐질 것입니다.",
                "約1,000万光年以内にある80以上の銀河で、天の川銀河とアンドロメダ銀河が中心です。この2つは数十億年後に合体します。",
                "Más de 80 galaxias en unos 10 millones de años luz, encabezadas por la Vía Láctea y Andrómeda, que se fusionarán dentro de unos miles de millones de años.",
                "Plus de 80 galaxies dans un rayon d'environ 10 millions d'années-lumière, menées par la Voie lactée et Andromède, qui fusionneront dans quelques milliards d'années.");
            A("Everything whose light has had time to reach us", "빛이 우리에게 닿을 시간이 있었던 모든 것", "光が私たちに届くだけの時間があったすべてのもの", "Todo aquello cuya luz ha tenido tiempo de llegarnos", "Tout ce dont la lumière a eu le temps de nous parvenir");
            A("About 93 billion light-years across, with hundreds of billions of galaxies (some estimates say two trillion) strung along a cosmic web. Beyond it there is surely more, but its light has not reached us yet.",
                "지름 약 930억 광년으로, 수천억 개의 은하(2조 개라는 추정도 있음)가 우주 거대 구조를 따라 늘어서 있습니다. 그 너머에도 분명 더 있겠지만, 그 빛은 아직 우리에게 닿지 않았습니다.",
                "直径約930億光年。数千億の銀河（2兆個という推定もあります）が宇宙の大規模構造に沿って連なっています。その先にもきっと続いていますが、その光はまだ届いていません。",
                "Unos 93.000 millones de años luz de ancho, con cientos de miles de millones de galaxias (algunas estimaciones dicen dos billones) dispuestas a lo largo de una red cósmica. Más allá seguro que hay más, pero su luz aún no nos ha llegado.",
                "Environ 93 milliards d'années-lumière de large, avec des centaines de milliards de galaxies (deux mille milliards selon certaines estimations) alignées le long d'une toile cosmique. Au-delà, il y en a sûrement davantage, mais sa lumière ne nous est pas encore parvenue.");
            // sizes in words (OrbitSolar.SizeLine) and the card's composed lines
            A("{0:N0} km across, {1:N0} Earths", "지름 {0:N0} km, 지구 {1:N0}개", "直径{0:N0} km、地球{1:N0}個分", "{0:N0} km de ancho, {1:N0} Tierras", "{0:N0} km de large, {1:N0} Terres");
            A("{0} times the Sun's width", "태양 너비의 {0}배", "太陽の幅の{0}倍", "{0} veces el ancho del Sol", "{0} fois la largeur du Soleil");
            A("{0:N0} AU across ({1:N1} light-hours)", "지름 {0:N0} AU (빛으로 {1:N1}시간)", "直径{0:N0} AU（光で{1:N1}時間）", "{0:N0} UA de ancho ({1:N1} horas luz)", "{0:N0} ua de large ({1:N1} heures-lumière)");
            A("{0:N1} light-years across", "지름 {0:N1}광년", "直径{0:N1}光年", "{0:N1} años luz de ancho", "{0:N1} années-lumière de large");
            A("{0:N0} light-years across", "지름 {0:N0}광년", "直径{0:N0}光年", "{0:N0} años luz de ancho", "{0:N0} années-lumière de large");
            A("About {0} (an estimate)", "{0} 정도 (추정치)", "{0}ほど（推定）", "Aprox. {0} (una estimación)", "Environ {0} (estimation)");
            A("UNIVERSE · STEP {0} OF {1}", "우주 · {1}단계 중 {0}단계", "宇宙 · ステップ {0}/{1}", "UNIVERSO · PASO {0} DE {1}", "UNIVERS · ÉTAPE {0} SUR {1}");
            A("{0} times as wide as {1}", "{1}보다 {0}배 넓음", "{1}の{0}倍の幅", "{0} veces el ancho del paso anterior ({1})", "{0} fois la largeur de l'étape précédente ({1})");
            A("{0} times as wide as {1} (roughly)", "{1}보다 대략 {0}배 넓음", "{1}のおよそ{0}倍の幅", "Unas {0} veces el ancho del paso anterior ({1})", "Environ {0} fois la largeur de l'étape précédente ({1})");
            A("Next: {0}. Arrows or a sweep to go on or back.", "다음: {0}. 화살표나 손으로 쓸어 앞뒤로 이동하세요.", "次は{0}。矢印かスワイプで進む・戻る。", "Siguiente: {0}. Flechas o un gesto de barrido para avanzar o retroceder.", "Suivant : {0}. Flèches ou balayage pour avancer ou reculer.");
            A("The end of what we can see. Start goes back to the Moon.", "우리가 볼 수 있는 끝입니다. ‘처음으로’를 누르면 달로 돌아갑니다.", "ここが見える範囲の果てです。「最初へ」で月に戻ります。", "El final de lo que podemos ver. Inicio vuelve a la Luna.", "La fin de ce que nous pouvons voir. Début ramène à la Lune.");
            // the intro over the deck (SolarBuilder)
            A("How big is everything?", "모든 것은 얼마나 클까?", "すべてはどれくらい大きい？", "¿Qué tamaño tiene todo?", "Quelle taille fait tout ?");
            A("From the Moon to the edge of the observable universe in 17 steps. Each brings in something bigger, with everything before it shrunk beside it at true scale.",
                "달에서 관측 가능한 우주의 끝까지 17단계. 단계마다 더 큰 것이 나타나고, 앞의 것들은 실제 비율로 줄어들어 그 옆에 놓입니다.",
                "月から観測可能な宇宙の果てまで17ステップ。ステップごとにもっと大きなものが現れ、それまでのものは実際の比率で縮んでその横に並びます。",
                "De la Luna al borde del universo observable en 17 pasos. Cada uno trae algo más grande, con todo lo anterior encogido a su lado a escala real.",
                "De la Lune aux confins de l'Univers observable en 17 étapes. Chacune fait apparaître quelque chose de plus grand, avec tout ce qui précède réduit à côté à l'échelle réelle.");
            A("Press  >  or sweep a hand to go on", ">를 누르거나 손으로 쓸어 계속하세요", ">を押すか、手でスワイプして進む", "Pulsa  >  o desliza la mano para seguir", "Appuyez sur  >  ou balayez de la main pour continuer");

            // ================================================================== Math (HoloMath, MathBuilder, MathExpr)
            A("Your equation", "내 방정식", "あなたの式", "Tu ecuación", "Votre équation");
            A("Space round a black hole", "블랙홀 주변의 공간", "ブラックホールのまわりの空間", "El espacio alrededor de un agujero negro", "L'espace autour d'un trou noir");
            A("Penrose diagram", "펜로즈 다이어그램", "ペンローズ図", "Diagrama de Penrose", "Diagramme de Penrose");
            A("Penrose: extremal hole", "펜로즈: 극한 블랙홀", "ペンローズ図：極限ブラックホール", "Penrose: agujero extremo", "Penrose : trou extrémal");
            A("Aretakis instability", "아레타키스 불안정성", "アレタキス不安定性", "Inestabilidad de Aretakis", "Instabilité d'Aretakis");
            // the pop-up's keyboard and variables
            A("Type with x, y and t (time). With y it is a surface; with t it moves.", "x, y, t(시간)로 입력하세요. y가 있으면 곡면, t가 있으면 움직입니다.", "x、y、t（時間）で入力。yがあれば曲面、tがあれば動きます。", "Escribe con x, y y t (tiempo). Con y es una superficie; con t se mueve.", "Tapez avec x, y et t (temps). Avec y, c'est une surface ; avec t, elle bouge.");
            A("Del", "지우기", "削除", "Borrar", "Suppr.");
            A("Plot", "그리기", "描画", "Graficar", "Tracer");
            A("Plotted", "그렸습니다", "描画しました", "Graficada", "Tracé");
            A("TRY", "해 보기", "試す", "PRUEBA", "ESSAYEZ");
            A("Waves", "물결", "波", "Ondas", "Vagues");
            A("Ripples", "잔물결", "さざ波", "Ondulaciones", "Ondulations");
            A("Saddle", "안장", "鞍", "Silla de montar", "Selle");
            A("Hill", "언덕", "丘", "Colina", "Colline");
            A("Sine", "사인", "サイン", "Seno", "Sinus");
            A("Beats", "맥놀이", "うなり", "Pulsaciones", "Battements");
            A("THE ARETAKIS INSTABILITY · VARIABLES", "아레타키스 불안정성 · 변수", "アレタキス不安定性 · 変数", "LA INESTABILIDAD DE ARETAKIS · VARIABLES", "L'INSTABILITÉ D'ARETAKIS · VARIABLES");
            A("Charge  Q / M", "전하  Q / M", "電荷  Q / M", "Carga  Q / M", "Charge  Q / M");
            A("Mode  ℓ", "모드  ℓ", "モード  ℓ", "Modo  ℓ", "Mode  ℓ");
            A("Pulse starts", "펄스 시작 위치", "パルスの開始位置", "Inicio del pulso", "Départ de l'impulsion");
            A("Speed", "속도", "速さ", "Velocidad", "Vitesse");
            A("1 extremal", "1 극한", "1 極限", "1 extremo", "1 extrémal");
            A("0  (spherical)", "0  (구형)", "0  （球対称）", "0  (esférico)", "0  (sphérique)");
            A("1  (dipole)", "1  (쌍극자)", "1  （双極子）", "1  (dipolo)", "1  (dipôle)");
            A("on the horizon", "지평선 위", "地平面上", "en el horizonte", "sur l'horizon");
            A("near it", "가까이", "その近く", "cerca", "tout près");
            A("away", "멀리", "遠く", "lejos", "loin");
            A("Watch the slope at the horizon (the needle) and the middle trace: on an extremal hole they never fade.", "지평선의 기울기(바늘)와 가운데 그래프를 보세요. 극한 블랙홀에서는 결코 사라지지 않습니다.", "地平面での傾き（針）と真ん中のグラフに注目。極限ブラックホールでは決して消えません。", "Observa la pendiente en el horizonte (la aguja) y la traza del medio: en un agujero extremo nunca se desvanecen.", "Regardez la pente à l'horizon (l'aiguille) et le tracé du milieu : sur un trou extrémal, ils ne s'estompent jamais.");
            // what the equation reader says when it cannot read the equation (MathExpr)
            A("Unexpected '{0}' at {1}", "{1}번째 자리에 예상치 못한 '{0}'", "{1}文字目に予期しない「{0}」", "'{0}' inesperado en la posición {1}", "« {0} » inattendu en position {1}");
            A("Type an expression", "식을 입력하세요", "式を入力してください", "Escribe una expresión", "Tapez une expression");
            A("Something is missing at the end", "끝에 무언가 빠졌어요", "最後に何かが足りません", "Falta algo al final", "Il manque quelque chose à la fin");
            A("Missing ')'", "')'가 빠졌어요", "「)」がありません", "Falta ')'", "« ) » manquante");
            A("Bad number at {0}", "{0}번째 자리의 숫자가 잘못됐어요", "{0}文字目の数値が正しくありません", "Número no válido en la posición {0}", "Nombre invalide en position {0}");
            A("{0} needs brackets: {0}(…)", "{0}에는 괄호가 필요해요: {0}(…)", "{0}にはかっこが必要です：{0}(…)", "{0} necesita paréntesis: {0}(…)", "{0} a besoin de parenthèses : {0}(…)");
            A("{0} takes {1} values", "{0}에는 값이 {1}개 필요해요", "{0}には値が{1}個必要です", "{0} necesita {1} valores", "{0} prend {1} valeurs");
            A("I don't know '{0}'", "'{0}'은(는) 모르는 이름이에요", "「{0}」はわかりません", "No conozco '{0}'", "Je ne connais pas « {0} »");
            // the card: your equation
            A("SURFACE", "곡면", "曲面", "SUPERFICIE", "SURFACE");
            A("CURVE", "곡선", "曲線", "CURVA", "COURBE");
            A("ANIMATED", "애니메이션", "アニメーション", "ANIMADA", "ANIMÉE");
            A("A surface over the x–y plane, x and y from −4 to 4", "x–y 평면 위의 곡면, x와 y는 −4부터 4까지", "x–y平面上の曲面、xとyは−4から4まで", "Una superficie sobre el plano x–y, con x e y de −4 a 4", "Une surface au-dessus du plan x–y, x et y de −4 à 4");
            A("A curve, x from −4 to 4", "곡선, x는 −4부터 4까지", "曲線、xは−4から4まで", "Una curva, con x de −4 a 4", "Une courbe, x de −4 à 4");
            A("t is the time in seconds since Plot, so the graph moves. Height rescales itself to fit the deck.", "t는 ‘그리기’를 누른 뒤 흐른 시간(초)이라 그래프가 움직입니다. 높이는 홀로덱에 맞게 저절로 조절됩니다.", "tは「描画」を押してからの秒数なので、グラフが動きます。高さはホロデッキに合わせて自動で調整されます。", "t es el tiempo en segundos desde que pulsaste Graficar, así que la gráfica se mueve. La altura se ajusta sola a la holocubierta.", "t est le temps en secondes depuis Tracer, donc le graphe bouge. La hauteur s'ajuste d'elle-même au holopont.");
            A("Add t to the equation to animate it: for example sin(x − t). Height rescales itself to fit the deck.", "방정식에 t를 넣으면 움직입니다. 예: sin(x − t). 높이는 홀로덱에 맞게 저절로 조절됩니다.", "式にtを加えると動きます。例：sin(x − t)。高さはホロデッキに合わせて自動で調整されます。", "Añade t a la ecuación para animarla: por ejemplo sin(x − t). La altura se ajusta sola a la holocubierta.", "Ajoutez t à l'équation pour l'animer : par exemple sin(x − t). La hauteur s'ajuste d'elle-même au holopont.");
            A("Functions: sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow", "함수: sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow", "関数：sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow", "Funciones: sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow", "Fonctions : sin cos tan asin acos atan sinh cosh tanh exp ln log sqrt abs floor sign min max pow");
            A("Constants: pi, e. Powers with ^. Products can be implicit: 2x, 3sin(x), (x+1)(x−1).", "상수: pi, e. 거듭제곱은 ^. 곱셈 기호는 생략할 수 있어요: 2x, 3sin(x), (x+1)(x−1).", "定数：pi、e。累乗は^。掛け算の記号は省略できます：2x、3sin(x)、(x+1)(x−1)。", "Constantes: pi, e. Potencias con ^. Los productos pueden ser implícitos: 2x, 3sin(x), (x+1)(x−1).", "Constantes : pi, e. Puissances avec ^. Les produits peuvent être implicites : 2x, 3sin(x), (x+1)(x−1).");
            A("Try: sin(sqrt(x^2+y^2) − 2t), exp(−(x^2+y^2)/4)cos(3t), x^2 − y^2, sin(x)sin(y)sin(t)", "해 보기: sin(sqrt(x^2+y^2) − 2t), exp(−(x^2+y^2)/4)cos(3t), x^2 − y^2, sin(x)sin(y)sin(t)", "試してみよう：sin(sqrt(x^2+y^2) − 2t)、exp(−(x^2+y^2)/4)cos(3t)、x^2 − y^2、sin(x)sin(y)sin(t)", "Prueba: sin(sqrt(x^2+y^2) − 2t), exp(−(x^2+y^2)/4)cos(3t), x^2 − y^2, sin(x)sin(y)sin(t)", "Essayez : sin(sqrt(x^2+y^2) − 2t), exp(−(x^2+y^2)/4)cos(3t), x^2 − y^2, sin(x)sin(y)sin(t)");
            // the card: the funnel
            A("MATH · GENERAL RELATIVITY", "수학 · 일반 상대성 이론", "数学 · 一般相対性理論", "MATEMÁTICAS · RELATIVIDAD GENERAL", "MATHS · RELATIVITÉ GÉNÉRALE");
            A("Flamm's paraboloid, with a planet and a ray of light", "플람 포물면, 행성 하나와 빛줄기 하나", "フラムの放物面と、惑星と光線", "El paraboloide de Flamm, con un planeta y un rayo de luz", "Le paraboloïde de Flamm, avec une planète et un rayon de lumière");
            A("The funnel is the curved space of a Schwarzschild black hole of mass M: a slice through its equator, with its stretching drawn as depth (z = 2√(2M(r − 2M))). The throat is the event horizon at r = 2M. The planet follows a true orbit, integrated from the geodesic equation d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴: its ellipse turns a little every lap, the precession that Mercury shows. The ray of light (d²u/dφ² + u = 3Mu², u = 1/r) sweeps in near the photon sphere at r = 3M and is bent away again.",
                "이 깔때기는 질량 M인 슈바르츠실트 블랙홀 주변의 휘어진 공간입니다. 적도를 따라 자른 단면이며, 공간이 늘어난 정도를 깊이로 그렸습니다(z = 2√(2M(r − 2M))). 목 부분이 r = 2M의 사건의 지평선입니다. 행성은 측지선 방정식 d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴를 적분한 실제 궤도를 따르며, 타원이 한 바퀴마다 조금씩 돌아갑니다. 수성에서 볼 수 있는 세차 운동이죠. 빛줄기(d²u/dφ² + u = 3Mu², u = 1/r)는 r = 3M의 광자구 가까이 휩쓸려 들어왔다가 다시 휘어져 나갑니다.",
                "この漏斗は質量Mのシュヴァルツシルト・ブラックホールのまわりの曲がった空間です。赤道に沿った断面で、空間の伸びを深さとして描いています（z = 2√(2M(r − 2M))）。のど元がr = 2Mの事象の地平面です。惑星は測地線方程式 d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴ を積分した本物の軌道をたどり、楕円が一周ごとに少しずつ回転します。水星に見られる近日点移動です。光線（d²u/dφ² + u = 3Mu²、u = 1/r）はr = 3Mの光子球の近くまで回り込み、再び曲がって離れていきます。",
                "El embudo es el espacio curvo de un agujero negro de Schwarzschild de masa M: un corte por su ecuador, con su estiramiento dibujado como profundidad (z = 2√(2M(r − 2M))). La garganta es el horizonte de sucesos en r = 2M. El planeta sigue una órbita real, integrada a partir de la ecuación de la geodésica d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴: su elipse gira un poco en cada vuelta, la precesión que muestra Mercurio. El rayo de luz (d²u/dφ² + u = 3Mu², u = 1/r) pasa cerca de la esfera de fotones en r = 3M y vuelve a desviarse.",
                "L'entonnoir est l'espace courbe d'un trou noir de Schwarzschild de masse M : une coupe selon son équateur, son étirement dessiné comme une profondeur (z = 2√(2M(r − 2M))). La gorge est l'horizon des événements en r = 2M. La planète suit une vraie orbite, intégrée à partir de l'équation des géodésiques d²r/dτ² = −M/r² + L²/r³ − 3ML²/r⁴ : son ellipse tourne un peu à chaque tour, la précession que montre Mercure. Le rayon de lumière (d²u/dφ² + u = 3Mu², u = 1/r) frôle la sphère de photons en r = 3M puis est de nouveau dévié.");
            A("Rings: the horizon (2M), the photon sphere (3M, brighter), then 4M, 6M, 8M, 10M, 12M", "고리: 지평선(2M), 광자구(3M, 더 밝음), 그다음 4M, 6M, 8M, 10M, 12M", "リング：地平面（2M）、光子球（3M、明るめ）、続いて4M、6M、8M、10M、12M", "Anillos: el horizonte (2M), la esfera de fotones (3M, más brillante), luego 4M, 6M, 8M, 10M, 12M", "Anneaux : l'horizon (2M), la sphère de photons (3M, plus brillante), puis 4M, 6M, 8M, 10M, 12M");
            A("Depth is the stretching of space, not a real dip: a model, not a picture", "깊이는 공간이 늘어난 정도이지 실제로 움푹 팬 것이 아닙니다. 사진이 아니라 모형입니다", "深さは空間の伸びで、本当のくぼみではありません。写真ではなく模型です", "La profundidad es el estiramiento del espacio, no un hundimiento real: un modelo, no una imagen", "La profondeur est l'étirement de l'espace, pas un vrai creux : un modèle, pas une image");
            A("Dots: the planet (warm) and the ray of light (bright)", "점: 행성(따뜻한 색)과 빛줄기(밝은 색)", "点：惑星（暖色）と光線（明るい色）", "Puntos: el planeta (cálido) y el rayo de luz (brillante)", "Points : la planète (chaude) et le rayon de lumière (brillant)");
            A("event horizon  r = 2M", "사건의 지평선  r = 2M", "事象の地平面  r = 2M", "horizonte de sucesos  r = 2M", "horizon des événements  r = 2M");
            A("photon sphere  r = 3M", "광자구  r = 3M", "光子球  r = 3M", "esfera de fotones  r = 3M", "sphère de photons  r = 3M");
            A("planet", "행성", "惑星", "planeta", "planète");
            // the card: Penrose diagrams
            A("MATH · PENROSE DIAGRAM", "수학 · 펜로즈 다이어그램", "数学 · ペンローズ図", "MATEMÁTICAS · DIAGRAMA DE PENROSE", "MATHS · DIAGRAMME DE PENROSE");
            A("Schwarzschild black hole", "슈바르츠실트 블랙홀", "シュヴァルツシルト・ブラックホール", "Agujero negro de Schwarzschild", "Trou noir de Schwarzschild");
            A("All of spacetime on one card, light always at 45°", "시공간 전체를 한 장에, 빛은 언제나 45°", "時空全体を1枚に、光はいつも45°", "Todo el espaciotiempo en una tarjeta, la luz siempre a 45°", "Tout l'espace-temps sur une carte, la lumière toujours à 45°");
            A("Each point is a sphere; time runs up, light moves along the diagonals. The diagram is computed from Kruskal–Szekeres coordinates, squashed with the arctangent so that infinity fits on the page. The right diamond is our universe outside the hole; the triangle above it is the inside, ending at the singularity r = 0 (a moment in time, not a place). The moving dot is someone falling in: their light cone stays at 45°, so once past the horizon every future path ends at r = 0. The faint lines are constant r and constant t.",
                "각 점은 하나의 구입니다. 시간은 위로 흐르고, 빛은 대각선을 따라 움직입니다. 이 다이어그램은 크루스칼–세케레시 좌표로 계산한 뒤 아크탄젠트로 눌러 무한대가 한 장에 들어가게 했습니다. 오른쪽 마름모는 블랙홀 바깥의 우리 우주이고, 그 위의 삼각형은 내부로, r = 0의 특이점(장소가 아니라 시간 속의 한 순간)에서 끝납니다. 움직이는 점은 안으로 떨어지는 사람입니다. 그 사람의 광원뿔은 45°를 유지하므로, 지평선을 지나면 모든 미래의 경로가 r = 0에서 끝납니다. 희미한 선은 r이 일정한 선과 t가 일정한 선입니다.",
                "各点はひとつの球面です。時間は上へ進み、光は対角線に沿って動きます。この図はクラスカル–シェケレシュ座標から計算し、無限遠が1枚に収まるようアークタンジェントで縮めたものです。右のひし形はブラックホールの外にある私たちの宇宙で、その上の三角形が内部です。内部はr = 0の特異点（場所ではなく、時間の中のある瞬間）で終わります。動く点は落ちていく人です。その光円錐は45°のままなので、地平面を越えると未来のどの道もr = 0で終わります。薄い線はrが一定の線とtが一定の線です。",
                "Cada punto es una esfera; el tiempo avanza hacia arriba y la luz se mueve por las diagonales. El diagrama se calcula a partir de las coordenadas de Kruskal–Szekeres, comprimidas con la arcotangente para que el infinito quepa en la página. El rombo de la derecha es nuestro universo fuera del agujero; el triángulo de encima es el interior, que termina en la singularidad r = 0 (un momento en el tiempo, no un lugar). El punto que se mueve es alguien que cae: su cono de luz se mantiene a 45°, así que, una vez pasado el horizonte, todo camino futuro termina en r = 0. Las líneas tenues son de r constante y t constante.",
                "Chaque point est une sphère ; le temps monte, la lumière suit les diagonales. Le diagramme est calculé à partir des coordonnées de Kruskal–Szekeres, comprimées par l'arc tangente pour que l'infini tienne sur la page. Le losange de droite est notre univers hors du trou ; le triangle au-dessus est l'intérieur, qui s'achève sur la singularité r = 0 (un instant dans le temps, pas un lieu). Le point mobile est quelqu'un qui tombe : son cône de lumière reste à 45°, donc une fois l'horizon passé, tout chemin futur finit en r = 0. Les lignes pâles sont à r constant et à t constant.");
            A("i0: spatial infinity. i+ and i−: the far future and past. The upper and lower right edges: where light goes and comes from (null infinity)", "i0: 공간적 무한대. i+와 i−: 아주 먼 미래와 과거. 오른쪽 위와 아래 가장자리: 빛이 가는 곳과 오는 곳(널 무한대)", "i0：空間的無限遠。i+とi−：はるかな未来と過去。右上と右下の辺：光が向かう先と来るところ（ヌル無限遠）", "i0: infinito espacial. i+ e i−: el futuro y el pasado lejanos. Los bordes derechos superior e inferior: adonde va la luz y de donde viene (infinito nulo)", "i0 : l'infini spatial. i+ et i− : le futur et le passé lointains. Les bords droits supérieur et inférieur : là où va la lumière et d'où elle vient (infini nul)");
            A("The horizon is the diagonal from the centre to i+: nothing special happens there to the one falling", "지평선은 중심에서 i+로 이어지는 대각선입니다. 떨어지는 사람에게 그곳에서 특별한 일은 일어나지 않습니다", "地平面は中心からi+への対角線です。落ちていく人には、そこで特別なことは何も起きません", "El horizonte es la diagonal del centro a i+: allí no le pasa nada especial a quien cae", "L'horizon est la diagonale du centre vers i+ : rien de particulier n'y arrive à celui qui tombe");
            A("The left diamond and the lower triangle are the other exterior and the white hole of the full solution, never reached from here", "왼쪽 마름모와 아래 삼각형은 완전한 해에 있는 또 다른 바깥 영역과 화이트홀로, 여기서는 결코 닿을 수 없습니다", "左のひし形と下の三角形は、完全な解に含まれるもうひとつの外部領域とホワイトホールで、ここからは決して到達できません", "El rombo izquierdo y el triángulo inferior son el otro exterior y el agujero blanco de la solución completa, a los que nunca se llega desde aquí", "Le losange de gauche et le triangle inférieur sont l'autre extérieur et le trou blanc de la solution complète, jamais atteints d'ici");
            A("r = 0  singularity", "r = 0  특이점", "r = 0  特異点", "r = 0  singularidad", "r = 0  singularité");
            A("event horizon", "사건의 지평선", "事象の地平面", "horizonte de sucesos", "horizon des événements");
            A("light out →", "나가는 빛 →", "出ていく光 →", "luz que sale →", "lumière sortante →");
            A("← light in", "← 들어오는 빛", "← 入ってくる光", "← luz que entra", "← lumière entrante");
            A("falling in, r = {0:0.0}M", "떨어지는 중, r = {0:0.0}M", "落下中、r = {0:0.0}M", "cayendo, r = {0:0.0}M", "en chute, r = {0:0.0}M");
            A("inside, r = {0:0.0}M", "내부, r = {0:0.0}M", "内部、r = {0:0.0}M", "dentro, r = {0:0.0}M", "à l'intérieur, r = {0:0.0}M");
            A("the end", "끝", "終わり", "el final", "la fin");
            A("Extremal Reissner–Nordström", "극한 라이스너–노르드스트룀", "極限ライスナー–ノルドストロム", "Reissner–Nordström extremo", "Reissner–Nordström extrémal");
            A("A charged hole with charge equal to its mass (Q = M): schematic", "전하가 질량과 같은(Q = M) 대전 블랙홀: 개략도", "電荷が質量に等しい（Q = M）帯電ブラックホール：模式図", "Un agujero cargado con carga igual a su masa (Q = M): esquema", "Un trou chargé dont la charge égale la masse (Q = M) : schéma");
            A("With Q = M the two horizons of a charged black hole merge into one at r = M, and the diagram changes shape: the singularity r = 0 is time-like, a vertical line you can steer away from, and beyond the horizon lies an inner (Cauchy) horizon and then, in the mathematics, another exterior region, and another, without end. The curve is an infaller passing through to the next region. Extremal holes have zero surface gravity and zero Hawking temperature; they are the setting of the Aretakis instability.",
                "Q = M이면 대전 블랙홀의 두 지평선이 r = M에서 하나로 합쳐지고, 다이어그램의 모양이 바뀝니다. 특이점 r = 0은 시간꼴이 되어 피해 갈 수 있는 수직선이 되고, 지평선 너머에는 안쪽(코시) 지평선이 있으며, 수학적으로는 그다음에 또 다른 바깥 영역, 그리고 또 다른 영역이 끝없이 이어집니다. 곡선은 다음 영역으로 지나가는 낙하자입니다. 극한 블랙홀은 표면 중력과 호킹 온도가 0이며, 아레타키스 불안정성의 무대입니다.",
                "Q = Mのとき、帯電ブラックホールの2つの地平面はr = Mでひとつに合わさり、図の形が変わります。特異点r = 0は時間的になり、避けて進める縦の線となります。地平面の先には内部（コーシー）地平面があり、数学の上ではその先にまた別の外部領域が、さらにまた別の領域が、果てしなく続きます。曲線は次の領域へ通り抜けていく落下者です。極限ブラックホールは表面重力もホーキング温度もゼロで、アレタキス不安定性の舞台です。",
                "Con Q = M, los dos horizontes de un agujero negro cargado se funden en uno en r = M, y el diagrama cambia de forma: la singularidad r = 0 es de tipo tiempo, una línea vertical que se puede esquivar, y más allá del horizonte hay un horizonte interior (de Cauchy) y luego, en las matemáticas, otra región exterior, y otra, sin fin. La curva es alguien que cae y pasa a la región siguiente. Los agujeros extremos tienen gravedad superficial nula y temperatura de Hawking nula; son el escenario de la inestabilidad de Aretakis.",
                "Avec Q = M, les deux horizons d'un trou noir chargé fusionnent en un seul en r = M, et le diagramme change de forme : la singularité r = 0 est de genre temps, une ligne verticale que l'on peut éviter, et au-delà de l'horizon se trouvent un horizon intérieur (de Cauchy) puis, dans les mathématiques, une autre région extérieure, et une autre, sans fin. La courbe est quelqu'un qui tombe et passe dans la région suivante. Les trous extrémaux ont une gravité de surface et une température de Hawking nulles ; ils sont le cadre de l'instabilité d'Aretakis.");
            A("Schematic: the shapes are right, the spacing is not computed", "개략도: 모양은 맞지만 간격은 계산한 것이 아닙니다", "模式図：形は正しいが、間隔は計算していません", "Esquema: las formas son correctas, las distancias no están calculadas", "Schéma : les formes sont justes, les espacements ne sont pas calculés");
            A("Real charged holes are never exactly extremal, but near-extremal black holes and the extremal Kerr limit (a maximally spinning hole) behave much like this", "실제 대전 블랙홀이 정확히 극한일 수는 없지만, 극한에 가까운 블랙홀과 극한 커 블랙홀(최대로 회전하는 블랙홀)은 이와 매우 비슷하게 행동합니다", "現実の帯電ブラックホールが厳密に極限になることはありませんが、極限に近いブラックホールや極限カー・ブラックホール（最大限に回転するブラックホール）はこれとよく似たふるまいをします", "Los agujeros cargados reales nunca son exactamente extremos, pero los agujeros negros casi extremos y el límite de Kerr extremo (un agujero que gira al máximo) se comportan de forma muy parecida", "Les vrais trous chargés ne sont jamais exactement extrémaux, mais les trous noirs presque extrémaux et la limite de Kerr extrémale (un trou en rotation maximale) se comportent à peu près ainsi");
            A("Next: the Aretakis instability on this horizon", "다음: 이 지평선 위의 아레타키스 불안정성", "次は：この地平面上のアレタキス不安定性", "Siguiente: la inestabilidad de Aretakis en este horizonte", "Suivant : l'instabilité d'Aretakis sur cet horizon");
            A("event horizon  r = M", "사건의 지평선  r = M", "事象の地平面  r = M", "horizonte de sucesos  r = M", "horizon des événements  r = M");
            A("r = 0  time-like", "r = 0  시간꼴", "r = 0  時間的", "r = 0  tipo tiempo", "r = 0  genre temps");
            A("inner horizon", "안쪽 지평선", "内部地平面", "horizonte interior", "horizon intérieur");
            A("another exterior", "또 다른 바깥 영역", "別の外部領域", "otro exterior", "un autre extérieur");
            A("and so on…", "계속 이어짐…", "以下同様…", "y así sucesivamente…", "et ainsi de suite…");
            A("schematic", "개략도", "模式図", "esquema", "schéma");
            // the card: the Aretakis instability
            A("MATH · HORIZON INSTABILITY", "수학 · 지평선 불안정성", "数学 · 地平面の不安定性", "MATEMÁTICAS · INESTABILIDAD DEL HORIZONTE", "MATHS · INSTABILITÉ DE L'HORIZON");
            A("The Aretakis instability", "아레타키스 불안정성", "アレタキス不安定性", "La inestabilidad de Aretakis", "L'instabilité d'Aretakis");
            A("The same wave on a sub-extremal hole", "아극한 블랙홀 위의 같은 파동", "非極限ブラックホール上の同じ波", "La misma onda en un agujero subextremo", "La même onde sur un trou sous-extrémal");
            A("A scalar wave on {0}, computed live. Change its variables with the pill.", "{0} 위의 스칼라 파동을 실시간으로 계산합니다. 변수 버튼으로 값을 바꿔 보세요.", "{0}上のスカラー波をリアルタイムで計算しています。変数ボタンで値を変えられます。", "Una onda escalar en {0}, calculada en vivo. Cambia sus variables con el botón.", "Une onde scalaire sur {0}, calculée en direct. Changez ses variables avec le bouton.");
            A("an extremal Reissner–Nordström black hole (Q = M)", "극한 라이스너–노르드스트룀 블랙홀(Q = M)", "極限ライスナー–ノルドストロム・ブラックホール（Q = M）", "un agujero negro de Reissner–Nordström extremo (Q = M)", "un trou noir de Reissner–Nordström extrémal (Q = M)");
            A("a Reissner–Nordström black hole with Q = {0:0.00} M", "Q = {0:0.00} M인 라이스너–노르드스트룀 블랙홀", "Q = {0:0.00} Mのライスナー–ノルドストロム・ブラックホール", "un agujero negro de Reissner–Nordström con Q = {0:0.00} M", "un trou noir de Reissner–Nordström avec Q = {0:0.00} M");
            A("A scalar field ψ obeys the wave equation 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0 (φ = rψ, ingoing coordinates v and r, f = 1 − 2M/r + Q²/r²) and is solved here step by step from a pulse. The halo round the hole is the field's profile ψ(r) now: it collapses as the wave leaves and falls in.",
                "스칼라장 ψ는 파동 방정식 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0 (φ = rψ, 유입 좌표 v와 r, f = 1 − 2M/r + Q²/r²)을 따르며, 여기서는 하나의 펄스에서 시작해 한 단계씩 풀어 냅니다. 블랙홀 주위의 후광은 지금 이 순간 장의 모양 ψ(r)로, 파동이 빠져나가고 안으로 떨어지면서 무너집니다.",
                "スカラー場ψは波動方程式 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0（φ = rψ、内向き座標vとr、f = 1 − 2M/r + Q²/r²）に従い、ここではひとつのパルスから一歩ずつ解いています。ブラックホールを囲む光の輪は、いまの場の形ψ(r)です。波が出ていき、落ち込むにつれてしぼんでいきます。",
                "Un campo escalar ψ obedece la ecuación de ondas 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0 (φ = rψ, coordenadas entrantes v y r, f = 1 − 2M/r + Q²/r²) y aquí se resuelve paso a paso a partir de un pulso. El halo alrededor del agujero es el perfil del campo ψ(r) en este momento: se desvanece a medida que la onda se aleja y cae dentro.",
                "Un champ scalaire ψ obéit à l'équation d'onde 2∂v∂rφ + ∂r(f∂rφ) − V φ = 0 (φ = rψ, coordonnées entrantes v et r, f = 1 − 2M/r + Q²/r²) et est résolu ici pas à pas à partir d'une impulsion. Le halo autour du trou est le profil du champ ψ(r) à cet instant : il s'effondre à mesure que l'onde s'en va et tombe.");
            A("On an extremal horizon f and f′ both vanish, so there the equation says ∂v(∂rφ) = 0: the slope of the field at the horizon never decays. It is a conserved charge, the Aretakis constant H₀, and it forces the next derivative to grow in proportion to v: watch the kink at the horizon stay sharp while the rest of the halo flattens. Stefanos Aretakis proved this in 2011.",
                "극한 지평선에서는 f와 f′가 모두 0이 되므로, 그곳에서 방정식은 ∂v(∂rφ) = 0이 됩니다. 즉 지평선에서 장의 기울기는 결코 줄어들지 않습니다. 이것은 보존되는 전하인 아레타키스 상수 H₀이며, 그다음 도함수가 v에 비례해 커지도록 만듭니다. 후광의 나머지가 평평해지는 동안 지평선의 꺾임이 날카롭게 남아 있는 것을 지켜보세요. 스테파노스 아레타키스가 2011년에 이를 증명했습니다.",
                "極限地平面ではfとf′がともにゼロになるため、そこで方程式は∂v(∂rφ) = 0となります。つまり地平面での場の傾きは決して減衰しません。これは保存される電荷、アレタキス定数H₀で、次の導関数をvに比例して増大させます。光の輪のほかの部分が平らになっていく間も、地平面の折れ曲がりが鋭いまま残るのを見てください。ステファノス・アレタキスが2011年にこれを証明しました。",
                "En un horizonte extremo, f y f′ se anulan, así que allí la ecuación dice ∂v(∂rφ) = 0: la pendiente del campo en el horizonte nunca decae. Es una carga conservada, la constante de Aretakis H₀, y obliga a la derivada siguiente a crecer en proporción a v: observa cómo el quiebre en el horizonte sigue nítido mientras el resto del halo se aplana. Stefanos Aretakis lo demostró en 2011.",
                "Sur un horizon extrémal, f et f′ s'annulent toutes deux, donc l'équation y devient ∂v(∂rφ) = 0 : la pente du champ à l'horizon ne décroît jamais. C'est une charge conservée, la constante d'Aretakis H₀, et elle force la dérivée suivante à croître proportionnellement à v : regardez le coude à l'horizon rester net pendant que le reste du halo s'aplatit. Stefanos Aretakis l'a démontré en 2011.");
            A("Away from extremality f′ is not zero on the horizon, and there the equation reads ∂v(∂rφ) = −½f′∂rφ: the slope decays at the surface-gravity rate κ, and so does everything else. Set Q = M to see the difference.",
                "극한이 아니면 지평선에서 f′가 0이 아니므로, 그곳의 방정식은 ∂v(∂rφ) = −½f′∂rφ가 됩니다. 기울기는 표면 중력 κ의 비율로 줄어들고, 다른 모든 것도 마찬가지입니다. Q = M으로 설정해 차이를 확인해 보세요.",
                "極限から外れると地平面でf′はゼロではなく、そこで方程式は∂v(∂rφ) = −½f′∂rφとなります。傾きは表面重力κの割合で減衰し、ほかのすべても同様です。Q = Mに設定して違いを見てみましょう。",
                "Lejos de la extremalidad, f′ no es cero en el horizonte, y allí la ecuación queda ∂v(∂rφ) = −½f′∂rφ: la pendiente decae al ritmo de la gravedad superficial κ, y todo lo demás también. Pon Q = M para ver la diferencia.",
                "Hors de l'extrémalité, f′ n'est pas nulle sur l'horizon, et l'équation y devient ∂v(∂rφ) = −½f′∂rφ : la pente décroît au rythme de la gravité de surface κ, et tout le reste aussi. Réglez Q = M pour voir la différence.");
            A("Variables: Q/M = {0:0.00}, r₊ = {1:0.00}M, surface gravity κ = {2:0.000}/M, mode ℓ = {3}, pulse {4}", "변수: Q/M = {0:0.00}, r₊ = {1:0.00}M, 표면 중력 κ = {2:0.000}/M, 모드 ℓ = {3}, 펄스: {4}", "変数：Q/M = {0:0.00}、r₊ = {1:0.00}M、表面重力 κ = {2:0.000}/M、モード ℓ = {3}、パルス：{4}", "Variables: Q/M = {0:0.00}, r₊ = {1:0.00}M, gravedad superficial κ = {2:0.000}/M, modo ℓ = {3}, pulso {4}", "Variables : Q/M = {0:0.00}, r₊ = {1:0.00}M, gravité de surface κ = {2:0.000}/M, mode ℓ = {3}, impulsion {4}");
            A("Traces at the right, on the horizon: the field, its slope and its curvature against v", "오른쪽 그래프는 지평선 위의 값: v에 따른 장, 기울기, 곡률", "右のグラフは地平面上の値：vに対する場、その傾き、その曲率", "Trazas a la derecha, en el horizonte: el campo, su pendiente y su curvatura frente a v", "Tracés à droite, sur l'horizon : le champ, sa pente et sa courbure en fonction de v");
            A("Numerics: 240 points in r from the horizon to 12M, third-order Runge–Kutta in v, a little smoothing; a model of the equations, not an exact solution", "수치 계산: 지평선부터 12M까지 r 방향 240개 점, v 방향 3차 룽게–쿠타, 약간의 평활화. 정확한 해가 아니라 방정식의 모형입니다", "数値計算：地平面から12Mまでrに240点、vに3次のルンゲ–クッタ法、わずかな平滑化。厳密解ではなく方程式のモデルです", "Cálculo numérico: 240 puntos en r del horizonte a 12M, Runge–Kutta de tercer orden en v, un poco de suavizado; un modelo de las ecuaciones, no una solución exacta", "Calcul numérique : 240 points en r de l'horizon à 12M, Runge–Kutta d'ordre trois en v, un peu de lissage ; un modèle des équations, pas une solution exacte");
            A("∂rφ on the horizon", "지평선 위의 ∂rφ", "地平面上の∂rφ", "∂rφ en el horizonte", "∂rφ sur l'horizon");
            A("∂r²φ: stays (H₁, the Aretakis constant for ℓ = 1)", "∂r²φ: 유지됨 (H₁, ℓ = 1의 아레타키스 상수)", "∂r²φ：一定のまま（H₁、ℓ = 1のアレタキス定数）", "∂r²φ: se mantiene (H₁, la constante de Aretakis para ℓ = 1)", "∂r²φ : reste (H₁, la constante d'Aretakis pour ℓ = 1)");
            A("∂r²φ: fades", "∂r²φ: 사라짐", "∂r²φ：減衰", "∂r²φ: se desvanece", "∂r²φ : s'estompe");
            A("∂r³φ: grows", "∂r³φ: 커짐", "∂r³φ：増大", "∂r³φ: crece", "∂r³φ : croît");
            A("∂r³φ: fades", "∂r³φ: 사라짐", "∂r³φ：減衰", "∂r³φ: se desvanece", "∂r³φ : s'estompe");
            A("φ on the horizon: fades", "지평선 위의 φ: 사라짐", "地平面上のφ：減衰", "φ en el horizonte: se desvanece", "φ sur l'horizon : s'estompe");
            A("∂rφ: stays at H₀, the Aretakis constant", "∂rφ: 아레타키스 상수 H₀로 유지됨", "∂rφ：アレタキス定数H₀のまま", "∂rφ: se mantiene en H₀, la constante de Aretakis", "∂rφ : reste à H₀, la constante d'Aretakis");
            A("∂rφ: fades at rate κ", "∂rφ: 비율 κ로 사라짐", "∂rφ：割合κで減衰", "∂rφ: se desvanece al ritmo κ", "∂rφ : s'estompe au taux κ");
            A("∂r²φ: grows ∝ v", "∂r²φ: v에 비례해 커짐", "∂r²φ：vに比例して増大", "∂r²φ: crece ∝ v", "∂r²φ : croît ∝ v");
            A("extremal hole  r = M", "극한 블랙홀  r = M", "極限ブラックホール  r = M", "agujero extremo  r = M", "trou extrémal  r = M");
            A("hole  r₊ = {0:0.00}M", "블랙홀  r₊ = {0:0.00}M", "ブラックホール  r₊ = {0:0.00}M", "agujero  r₊ = {0:0.00}M", "trou  r₊ = {0:0.00}M");
            A("the slope at the horizon", "지평선에서의 기울기", "地平面での傾き", "la pendiente en el horizonte", "la pente à l'horizon");
            A("v = {0:0}M  (done: Replay)", "v = {0:0}M  (끝: 다시 보기)", "v = {0:0}M  （終了：リプレイ）", "v = {0:0}M  (terminado: Repetir)", "v = {0:0}M  (terminé : Rejouer)");
            A("extremal: κ = 0, so the horizon keeps the slope", "극한: κ = 0이므로 지평선이 기울기를 유지합니다", "極限：κ = 0なので地平面は傾きを保ちます", "extremo: κ = 0, así que el horizonte conserva la pendiente", "extrémal : κ = 0, donc l'horizon garde la pente");
            A("sub-extremal: κ = {0:0.000}/M, so the slope fades like e^(−κv)", "아극한: κ = {0:0.000}/M이므로 기울기가 e^(−κv)처럼 줄어듭니다", "非極限：κ = {0:0.000}/Mなので傾きはe^(−κv)のように減衰します", "subextremo: κ = {0:0.000}/M, así que la pendiente decae como e^(−κv)", "sous-extrémal : κ = {0:0.000}/M, donc la pente décroît comme e^(−κv)");
            A("on the horizon now:   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}", "지금 지평선 위:   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}", "いまの地平面上：   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}", "en el horizonte ahora:   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}", "sur l'horizon maintenant :   φ = {0:0.000}    ∂rφ = {1:0.000}    ∂r²φ = {2:0.00}    ∂r³φ = {3:0.0}");
            A("conserved charge H{0} = ∂r^{1}φ|H at the start: {2:0.000}   now: {3:0.000}", "보존 전하 H{0} = ∂r^{1}φ|H  처음: {2:0.000}   지금: {3:0.000}", "保存電荷 H{0} = ∂r^{1}φ|H  最初：{2:0.000}   いま：{3:0.000}", "carga conservada H{0} = ∂r^{1}φ|H al inicio: {2:0.000}   ahora: {3:0.000}", "charge conservée H{0} = ∂r^{1}φ|H au départ : {2:0.000}   maintenant : {3:0.000}");
            A("(the same: that is the instability)", "(그대로: 이것이 불안정성입니다)", "（同じ：これが不安定性です）", "(igual: esa es la inestabilidad)", "(identique : c'est l'instabilité)");
            A("(decaying)", "(감소 중)", "（減衰中）", "(decayendo)", "(en décroissance)");

            // ================================================================== Mission mode (OrbitMissions: the stages, and the pictures beside the deck)
            // ("Second stage" and "Parachutes" are also part names on the spacecraft deck, translated with the deck's words.)
            A("Ignition", "점화", "点火", "Ignición", "Allumage");
            A("Liftoff", "이륙", "リフトオフ", "Despegue", "Décollage");
            A("Max Q", "최대 동압(Max Q)", "マックスQ", "Max Q", "Max Q");
            A("Stage separation", "단 분리", "段分離", "Separación de etapas", "Séparation des étages");
            A("Fairing separation", "페어링 분리", "フェアリング分離", "Separación de la cofia", "Séparation de la coiffe");
            A("Flip and grid fins", "뒤집기와 그리드 핀", "反転とグリッドフィン", "Giro y aletas de rejilla", "Retournement et ailerons en grille");
            A("Entry burn", "재진입 연소", "再突入噴射", "Encendido de reentrada", "Allumage de rentrée");
            A("Landing burn", "착륙 연소", "着陸噴射", "Encendido de aterrizaje", "Allumage d'atterrissage");
            A("Touchdown", "착지", "着地", "Toma de contacto", "Posé");
            A("Dragon separates", "드래건 분리", "ドラゴン分離", "Separación de la Dragon", "Séparation de Dragon");
            A("Rendezvous", "랑데부", "ランデブー", "Encuentro orbital", "Rendez-vous");
            A("Docked", "도킹 완료", "ドッキング完了", "Acoplada", "Amarré");
            A("Undocking", "도킹 해제", "ドッキング解除", "Desacople", "Désamarrage");
            A("Deorbit and trunk", "궤도 이탈과 트렁크 분리", "軌道離脱とトランク分離", "Salida de órbita y tronco", "Désorbitation et trunk");
            A("Re-entry", "재진입", "再突入", "Reentrada", "Rentrée atmosphérique");
            A("Splashdown", "착수", "着水", "Amerizaje", "Amerrissage");
            A("Deployment", "전개", "展開", "Despliegue", "Déploiement");
            A("In orbit", "궤도에서", "軌道上", "En órbita", "En orbite");
            A("Pillars of Creation", "창조의 기둥", "創造の柱", "Pilares de la Creación", "Piliers de la création");
            A("Ultra Deep Field", "울트라 딥 필드", "ウルトラ・ディープ・フィールド", "Campo Ultra Profundo", "Champ ultra-profond");
            A("Mystic Mountain", "신비의 산", "ミスティック・マウンテン", "Montaña Mística", "Montagne mystique");
            A("Crab Nebula", "게성운", "かに星雲", "Nebulosa del Cangrejo", "Nébuleuse du Crabe");
            A("Still observing", "여전히 관측 중", "今も観測中", "Sigue observando", "Toujours en observation");
            A("Launch", "발사", "打ち上げ", "Lanzamiento", "Lancement");
            A("Cruise", "순항", "巡航", "Crucero", "Croisière");
            A("Io's volcanoes", "이오의 화산", "イオの火山", "Los volcanes de Ío", "Les volcans d'Io");
            A("Saturn and Titan", "토성과 타이탄", "土星とタイタン", "Saturno y Titán", "Saturne et Titan");
            A("The Pale Blue Dot", "창백한 푸른 점", "ペイル・ブルー・ドット", "El punto azul pálido", "Le point bleu pâle");
            A("Interstellar space", "성간 우주", "星間空間", "Espacio interestelar", "Espace interstellaire");
            A("Still calling home", "여전히 지구와 교신 중", "今も地球と交信中", "Sigue llamando a casa", "Appelle toujours la maison");
            // Falcon 9
            A("Nine Merlin engines light three seconds before liftoff. Clamps hold the rocket down until the flight computers see full thrust.",
                "이륙 3초 전 멀린 엔진 9기가 점화됩니다. 비행 컴퓨터가 최대 추력을 확인할 때까지 고정 장치가 로켓을 붙잡아 둡니다.",
                "リフトオフの3秒前、9基のマーリンエンジンが点火します。飛行コンピューターが最大推力を確認するまで、クランプがロケットを押さえています。",
                "Nueve motores Merlin se encienden tres segundos antes del despegue. Unas abrazaderas sujetan el cohete hasta que los ordenadores de vuelo detectan el empuje completo.",
                "Neuf moteurs Merlin s'allument trois secondes avant le décollage. Des pinces retiennent la fusée jusqu'à ce que les ordinateurs de vol constatent la pleine poussée.");
            A("Falcon 9 rises on about 7,600 kilonewtons of thrust, burning rocket-grade kerosene and liquid oxygen.",
                "팰컨 9은 로켓용 등유와 액체 산소를 태우며 약 7,600킬로뉴턴의 추력으로 솟아오릅니다.",
                "ファルコン9はロケット用ケロシンと液体酸素を燃やし、約7,600キロニュートンの推力で上昇します。",
                "El Falcon 9 se eleva con unos 7600 kilonewtons de empuje, quemando queroseno de grado cohete y oxígeno líquido.",
                "Falcon 9 s'élève avec environ 7 600 kilonewtons de poussée, en brûlant du kérosène de qualité fusée et de l'oxygène liquide.");
            A("About 70 seconds in, already faster than sound, the rocket passes Max Q: the moment the air pushes hardest on it.",
                "약 70초 뒤, 이미 음속보다 빠른 로켓이 최대 동압(Max Q)을 지납니다. 공기가 로켓을 가장 세게 누르는 순간이죠.",
                "約70秒後、すでに音速を超えたロケットはマックスQを通過します。空気が最も強くロケットを押す瞬間です。",
                "A unos 70 segundos, ya más rápido que el sonido, el cohete pasa por Max Q: el momento en que el aire lo empuja con más fuerza.",
                "Vers 70 secondes, déjà plus rapide que le son, la fusée passe Max Q : le moment où l'air pousse le plus fort sur elle.");
            A("Two and a half minutes up, the first stage engines cut off and pneumatic pushers split the stages apart.",
                "발사 2분 30초 뒤, 1단 엔진이 꺼지고 공압식 푸셔가 두 단을 떼어 냅니다.",
                "打ち上げから2分半、第1段のエンジンが停止し、空気圧式のプッシャーが段を切り離します。",
                "A los dos minutos y medio, los motores de la primera etapa se apagan y unos empujadores neumáticos separan las etapas.",
                "Au bout de deux minutes et demie, les moteurs du premier étage s'éteignent et des poussoirs pneumatiques séparent les étages.");
            A("One Merlin Vacuum engine, with a huge nozzle made for thin air, lights to carry the payload on to orbit.",
                "희박한 공기에 맞춘 거대한 노즐을 단 멀린 배큐엄 엔진 1기가 점화되어 탑재물을 궤도까지 실어 갑니다.",
                "薄い空気用の巨大なノズルを備えたマーリン・バキュームエンジン1基が点火し、ペイロードを軌道まで運びます。",
                "Un motor Merlin Vacuum, con una enorme tobera hecha para el aire enrarecido, se enciende para llevar la carga hasta la órbita.",
                "Un moteur Merlin Vacuum, doté d'une immense tuyère conçue pour l'air raréfié, s'allume pour emmener la charge utile en orbite.");
            A("Above the thick air, the nose fairing splits in two and falls away. Each half steers under a parafoil and is caught to fly again.",
                "짙은 대기를 벗어나면 앞부분의 페어링이 둘로 갈라져 떨어집니다. 각 반쪽은 패러포일로 방향을 잡으며 회수되어 다시 비행합니다.",
                "濃い大気を抜けると、先端のフェアリングが2つに割れて離れていきます。それぞれの半分はパラフォイルで進路を取り、回収されて再び飛びます。",
                "Por encima del aire denso, la cofia se abre en dos y se desprende. Cada mitad se guía bajo un parapente y se recupera para volver a volar.",
                "Au-dessus de l'air dense, la coiffe se sépare en deux et s'éloigne. Chaque moitié se dirige sous un parafoil et est récupérée pour revoler.");
            A("Meanwhile the booster turns itself around with cold-gas thrusters and unfolds four titanium grid fins to steer as it falls.",
                "그사이 부스터는 냉가스 추력기로 몸을 돌리고, 떨어지는 동안 방향을 잡도록 티타늄 그리드 핀 4개를 펼칩니다.",
                "その間にブースターはコールドガス・スラスターで向きを変え、落下中の操縦のためにチタン製グリッドフィン4枚を広げます。",
                "Mientras tanto, el propulsor se da la vuelta con propulsores de gas frío y despliega cuatro aletas de rejilla de titanio para guiarse al caer.",
                "Pendant ce temps, le propulseur se retourne grâce à des propulseurs à gaz froid et déploie quatre ailerons en grille en titane pour se diriger dans sa chute.");
            A("Three engines relight to slow the booster before it hits the thick lower atmosphere, so it does not burn up.",
                "엔진 3기가 다시 점화해, 짙은 하층 대기에 부딪히기 전에 부스터의 속도를 줄여 타 버리지 않게 합니다.",
                "3基のエンジンが再点火し、濃い下層大気に突入する前にブースターを減速させ、燃え尽きないようにします。",
                "Tres motores se reencienden para frenar el propulsor antes de que alcance la densa atmósfera baja, para que no se queme.",
                "Trois moteurs se rallument pour freiner le propulseur avant qu'il n'atteigne la basse atmosphère dense, afin qu'il ne brûle pas.");
            A("One engine relights for the landing burn, and four carbon-fibre legs swing down just before touchdown on a drone ship at sea.",
                "엔진 1기가 다시 점화해 착륙 연소를 하고, 바다 위 드론십에 닿기 직전 탄소섬유 다리 4개가 펼쳐집니다.",
                "1基のエンジンが再点火して着陸噴射を行い、海上のドローン船に着地する直前に炭素繊維の脚4本が開きます。",
                "Un motor se reenciende para el encendido de aterrizaje, y cuatro patas de fibra de carbono se despliegan justo antes de posarse en un barco dron en el mar.",
                "Un moteur se rallume pour l'allumage d'atterrissage, et quatre jambes en fibre de carbone se déploient juste avant de se poser sur une barge en mer.");
            A("The booster stands on the drone ship, ready to fly again. Some boosters have now flown more than thirty times.",
                "부스터가 드론십 위에 똑바로 서서 다시 날 준비를 마칩니다. 이제 서른 번 넘게 비행한 부스터도 있습니다.",
                "ブースターはドローン船の上に立ち、再び飛ぶ準備が整います。今では30回以上飛んだブースターもあります。",
                "El propulsor queda en pie sobre el barco dron, listo para volver a volar. Algunos propulsores ya han volado más de treinta veces.",
                "Le propulseur se tient debout sur la barge, prêt à revoler. Certains propulseurs ont désormais volé plus de trente fois.");
            // Crew Dragon
            A("The crew access arm swings back, and nine Merlin engines light beneath Dragon and its crew.",
                "승무원 탑승용 통로가 뒤로 젖혀지고, 드래건과 승무원 아래에서 멀린 엔진 9기가 점화됩니다.",
                "クルーアクセスアームが離れ、ドラゴンとクルーの下で9基のマーリンエンジンが点火します。",
                "El brazo de acceso de la tripulación se retira y nueve motores Merlin se encienden bajo la Dragon y su tripulación.",
                "Le bras d'accès de l'équipage pivote, et neuf moteurs Merlin s'allument sous Dragon et son équipage.");
            A("Falcon 9 lifts Dragon off Launch Complex 39A in Florida, the pad that launched Apollo 11 and the Space Shuttle.",
                "팰컨 9이 플로리다의 39A 발사대에서 드래건을 들어 올립니다. 아폴로 11호와 우주왕복선을 쏘아 올린 바로 그 발사대입니다.",
                "ファルコン9がフロリダの39A発射施設からドラゴンを打ち上げます。アポロ11号やスペースシャトルを打ち上げた発射台です。",
                "El Falcon 9 eleva la Dragon desde el Complejo de Lanzamiento 39A en Florida, la plataforma que lanzó el Apolo 11 y el transbordador espacial.",
                "Falcon 9 emporte Dragon depuis le complexe de lancement 39A en Floride, le pas de tir d'Apollo 11 et de la navette spatiale.");
            A("About a minute in, the rocket passes Max Q, the greatest air pressure of the climb. The crew feel about three g.",
                "약 1분 뒤 로켓은 상승 중 공기 압력이 가장 큰 최대 동압(Max Q)을 지납니다. 승무원은 약 3G를 느낍니다.",
                "約1分後、ロケットは上昇中に空気の圧力が最大となるマックスQを通過します。クルーは約3Gを感じます。",
                "Cerca del minuto, el cohete pasa por Max Q, la mayor presión del aire en el ascenso. La tripulación siente unas tres g.",
                "Au bout d'une minute environ, la fusée passe Max Q, la plus forte pression de l'air pendant la montée. L'équipage ressent environ trois g.");
            A("The first stage shuts down and drops away, to land itself on a drone ship or back at the coast.",
                "1단이 꺼지고 떨어져 나가, 드론십이나 해안의 착륙장에 스스로 착륙합니다.",
                "第1段が停止して切り離され、ドローン船か海岸の着陸場に自ら着陸します。",
                "La primera etapa se apaga y se separa, para aterrizar sola en un barco dron o de vuelta en la costa.",
                "Le premier étage s'éteint et se détache, pour se poser seul sur une barge ou de retour sur la côte.");
            A("The second stage's single Merlin Vacuum engine pushes Dragon the rest of the way to orbit.",
                "2단의 멀린 배큐엄 엔진 1기가 남은 길을 밀어 드래건을 궤도에 올립니다.",
                "第2段のマーリン・バキュームエンジン1基が、残りの道のりを押してドラゴンを軌道へ運びます。",
                "El único motor Merlin Vacuum de la segunda etapa empuja la Dragon el resto del camino hasta la órbita.",
                "L'unique moteur Merlin Vacuum du second étage pousse Dragon jusqu'en orbite.");
            A("About twelve minutes after liftoff Dragon flies free, then opens its nosecone to uncover the docking port and its guidance sensors.",
                "이륙 약 12분 뒤 드래건이 분리되어 홀로 날고, 노즈콘을 열어 도킹 포트와 유도 센서를 드러냅니다.",
                "リフトオフから約12分後、ドラゴンは切り離されて単独で飛行し、ノーズコーンを開いてドッキングポートと誘導センサーをあらわにします。",
                "Unos doce minutos después del despegue, la Dragon vuela libre y abre su cono de morro para descubrir el puerto de acoplamiento y sus sensores de guiado.",
                "Environ douze minutes après le décollage, Dragon vole seul, puis ouvre son cône avant pour dégager le port d'amarrage et ses capteurs de guidage.");
            A("Dragon flies itself to the International Space Station, nudged along by sixteen small Draco thrusters, usually in about a day.",
                "드래건은 작은 드레이코 추력기 16개로 조금씩 방향을 바꾸며 스스로 국제우주정거장까지 날아갑니다. 보통 하루 정도 걸립니다.",
                "ドラゴンは16基の小さなドラコ・スラスターで少しずつ進みながら、自力で国際宇宙ステーションへ向かいます。ふつう1日ほどかかります。",
                "La Dragon vuela por sí sola hasta la Estación Espacial Internacional, impulsada por dieciséis pequeños propulsores Draco, normalmente en un día.",
                "Dragon rejoint seul la Station spatiale internationale, poussé par seize petits propulseurs Draco, généralement en une journée environ.");
            A("Dragon docks automatically to the station's port. After leak checks the hatch opens and the crew float aboard.",
                "드래건이 정거장 포트에 자동으로 도킹합니다. 누출 점검 후 해치가 열리고 승무원들이 둥둥 떠서 들어갑니다.",
                "ドラゴンはステーションのポートに自動でドッキングします。漏れの点検のあとハッチが開き、クルーが浮かびながら乗り込みます。",
                "La Dragon se acopla automáticamente al puerto de la estación. Tras comprobar que no hay fugas, se abre la escotilla y la tripulación entra flotando.",
                "Dragon s'amarre automatiquement au port de la station. Après les tests d'étanchéité, l'écoutille s'ouvre et l'équipage entre en flottant.");
            A("Months later, the crew climb back in and Dragon backs away from the station.",
                "몇 달 뒤 승무원들이 다시 탑승하고 드래건이 정거장에서 물러납니다.",
                "数か月後、クルーが再び乗り込み、ドラゴンはステーションから離れます。",
                "Meses después, la tripulación vuelve a subir y la Dragon se aleja de la estación.",
                "Des mois plus tard, l'équipage remonte à bord et Dragon s'éloigne de la station.");
            A("The nosecone closes, the Dracos fire to drop Dragon out of orbit, and the trunk with its solar cells is let go.",
                "노즈콘이 닫히고, 드레이코 추력기가 분사해 드래건을 궤도에서 떨어뜨리며, 태양 전지가 붙은 트렁크가 분리됩니다.",
                "ノーズコーンが閉じ、ドラコが噴射してドラゴンを軌道から離脱させ、太陽電池のついたトランクが切り離されます。",
                "El cono de morro se cierra, los Draco se encienden para sacar la Dragon de la órbita y se suelta el tronco con sus células solares.",
                "Le cône avant se referme, les Draco s'allument pour faire quitter l'orbite à Dragon, et le trunk avec ses cellules solaires est largué.");
            A("Dragon hits the atmosphere at about 27,000 kilometres an hour. Its heat shield faces temperatures near 1,900 degrees Celsius.",
                "드래건이 시속 약 27,000킬로미터로 대기에 진입합니다. 열 방패는 섭씨 1,900도에 가까운 온도를 견딥니다.",
                "ドラゴンは時速約27,000キロメートルで大気に突入します。耐熱シールドは摂氏1,900度近い温度にさらされます。",
                "La Dragon entra en la atmósfera a unos 27.000 kilómetros por hora. Su escudo térmico soporta temperaturas cercanas a 1900 grados Celsius.",
                "Dragon frappe l'atmosphère à environ 27 000 kilomètres à l'heure. Son bouclier thermique affronte des températures proches de 1 900 degrés Celsius.");
            A("Two drogue parachutes steady the capsule about five and a half kilometres up, then four main parachutes open.",
                "고도 약 5.5킬로미터에서 보조 낙하산 2개가 캡슐을 안정시키고, 이어서 주 낙하산 4개가 펼쳐집니다.",
                "高度約5.5キロメートルで2つのドローグパラシュートがカプセルを安定させ、続いて4つのメインパラシュートが開きます。",
                "Dos paracaídas de frenado estabilizan la cápsula a unos cinco kilómetros y medio de altura, y luego se abren cuatro paracaídas principales.",
                "Deux parachutes de freinage stabilisent la capsule à environ cinq kilomètres et demi d'altitude, puis quatre parachutes principaux s'ouvrent.");
            A("Dragon splashes down at about 25 kilometres an hour, and a recovery ship lifts the crew aboard within the hour.",
                "드래건이 시속 약 25킬로미터로 바다에 내려앉고, 한 시간 안에 회수선이 승무원을 배 위로 들어 올립니다.",
                "ドラゴンは時速約25キロメートルで着水し、1時間以内に回収船がクルーを船上に引き上げます。",
                "La Dragon amariza a unos 25 kilómetros por hora, y un barco de recuperación sube a bordo a la tripulación en menos de una hora.",
                "Dragon amerrit à environ 25 kilomètres à l'heure, et un navire de récupération hisse l'équipage à bord en moins d'une heure.");
            // Hubble
            A("On 25 April 1990 the shuttle Discovery's robot arm let Hubble go, about 600 kilometres up. Its antennas swung out and its solar arrays unrolled.",
                "1990년 4월 25일, 우주왕복선 디스커버리호의 로봇 팔이 고도 약 600킬로미터에서 허블을 놓아주었습니다. 안테나가 펼쳐지고 태양 전지판이 풀려 나왔습니다.",
                "1990年4月25日、スペースシャトル・ディスカバリーのロボットアームが高度約600キロメートルでハッブルを放しました。アンテナが開き、太陽電池パネルが広がりました。",
                "El 25 de abril de 1990, el brazo robótico del transbordador Discovery soltó el Hubble a unos 600 kilómetros de altura. Sus antenas se desplegaron y sus paneles solares se desenrollaron.",
                "Le 25 avril 1990, le bras robotique de la navette Discovery a lâché Hubble à environ 600 kilomètres d'altitude. Ses antennes se sont déployées et ses panneaux solaires se sont déroulés.");
            A("Hubble circles the Earth every 95 minutes at about 27,000 kilometres an hour, above the blur of the atmosphere.",
                "허블은 흐릿한 대기 위에서 시속 약 27,000킬로미터로 95분마다 지구를 한 바퀴 돕니다.",
                "ハッブルは大気のゆらぎの上を、時速約27,000キロメートルで95分ごとに地球を一周しています。",
                "El Hubble da la vuelta a la Tierra cada 95 minutos a unos 27.000 kilómetros por hora, por encima de la borrosidad de la atmósfera.",
                "Hubble fait le tour de la Terre en 95 minutes à environ 27 000 kilomètres à l'heure, au-dessus du flou de l'atmosphère.");
            A("Towers of cold gas and dust in the Eagle Nebula, 6,500 light-years away, where new stars are forming. Hubble first photographed them in 1995.",
                "6,500광년 떨어진 독수리 성운의 차가운 가스와 먼지 기둥으로, 새로운 별이 태어나는 곳입니다. 허블이 1995년에 처음 촬영했습니다.",
                "6,500光年かなたのわし星雲にそびえる冷たいガスと塵の塔で、新しい星が生まれている場所です。ハッブルが1995年に初めて撮影しました。",
                "Torres de gas frío y polvo en la nebulosa del Águila, a 6500 años luz, donde se forman estrellas nuevas. El Hubble las fotografió por primera vez en 1995.",
                "Des tours de gaz froid et de poussière dans la nébuleuse de l'Aigle, à 6 500 années-lumière, où naissent de nouvelles étoiles. Hubble les a photographiées pour la première fois en 1995.");
            A("A speck of sky smaller than a grain of sand held at arm's length, filled with about 10,000 galaxies, some seen as they were 13 billion years ago.",
                "팔을 뻗어 든 모래알보다 작은 하늘 한 조각에 약 1만 개의 은하가 가득하며, 그중 일부는 130억 년 전의 모습입니다.",
                "腕を伸ばして持った砂粒より小さな空の一角に約1万個の銀河がひしめき、その一部は130億年前の姿です。",
                "Un punto de cielo más pequeño que un grano de arena sostenido con el brazo extendido, lleno de unas 10.000 galaxias, algunas vistas tal como eran hace 13.000 millones de años.",
                "Un coin de ciel plus petit qu'un grain de sable tenu à bout de bras, rempli d'environ 10 000 galaxies, certaines vues telles qu'elles étaient il y a 13 milliards d'années.");
            A("A three-light-year pillar in the Carina Nebula, 7,500 light-years away, with jets of gas firing from the young stars inside it. Taken for Hubble's 20th birthday.",
                "7,500광년 떨어진 용골자리 성운의 3광년 높이 기둥으로, 안에 있는 어린 별들이 가스 제트를 내뿜고 있습니다. 허블의 20번째 생일을 맞아 촬영했습니다.",
                "7,500光年かなたのりゅうこつ座星雲にある高さ3光年の柱で、内部の若い星からガスのジェットが噴き出しています。ハッブルの20歳の誕生日を記念して撮影されました。",
                "Un pilar de tres años luz en la nebulosa de Carina, a 7500 años luz, con chorros de gas que salen de las estrellas jóvenes de su interior. Tomada para el 20.º cumpleaños del Hubble.",
                "Un pilier de trois années-lumière dans la nébuleuse de la Carène, à 7 500 années-lumière, d'où jaillissent des jets de gaz émis par les jeunes étoiles qu'il abrite. Prise pour les 20 ans de Hubble.");
            A("What is left of a star that exploded in the year 1054, a blast Chinese astronomers saw in daylight. A neutron star at its heart spins 30 times a second.",
                "1054년에 폭발한 별의 잔해로, 중국 천문학자들이 대낮에도 그 폭발을 보았습니다. 중심의 중성자별은 1초에 30번 회전합니다.",
                "1054年に爆発した星の残骸で、中国の天文学者はその爆発を昼間にも見ました。中心の中性子星は1秒間に30回自転しています。",
                "Lo que queda de una estrella que explotó en el año 1054, una explosión que los astrónomos chinos vieron a plena luz del día. Una estrella de neutrones en su centro gira 30 veces por segundo.",
                "Ce qui reste d'une étoile qui a explosé en l'an 1054, une explosion que les astronomes chinois ont vue en plein jour. Une étoile à neutrons en son cœur tourne 30 fois par seconde.");
            A("Astronauts upgraded Hubble on five shuttle missions between 1993 and 2009. It has made more than 1.6 million observations, and it is still at work.",
                "우주비행사들은 1993년부터 2009년까지 다섯 번의 우주왕복선 임무로 허블을 업그레이드했습니다. 허블은 160만 회가 넘는 관측을 했고, 지금도 일하고 있습니다.",
                "宇宙飛行士たちは1993年から2009年までの5回のシャトルミッションでハッブルを改修しました。ハッブルは160万回以上の観測を行い、今も働き続けています。",
                "Los astronautas mejoraron el Hubble en cinco misiones del transbordador entre 1993 y 2009. Ha realizado más de 1,6 millones de observaciones y sigue trabajando.",
                "Des astronautes ont amélioré Hubble lors de cinq missions de la navette entre 1993 et 2009. Il a réalisé plus de 1,6 million d'observations, et il est toujours au travail.");
            // Voyager 1
            A("Voyager 1 left Earth on 5 September 1977 on a Titan IIIE-Centaur rocket, sixteen days after its twin, Voyager 2.",
                "보이저 1호는 쌍둥이인 보이저 2호보다 16일 늦은 1977년 9월 5일, 타이탄 IIIE-센타우르 로켓을 타고 지구를 떠났습니다.",
                "ボイジャー1号は双子のボイジャー2号の16日後、1977年9月5日にタイタンIIIE-セントールロケットで地球を出発しました。",
                "La Voyager 1 dejó la Tierra el 5 de septiembre de 1977 a bordo de un cohete Titan IIIE-Centaur, dieciséis días después de su gemela, la Voyager 2.",
                "Voyager 1 a quitté la Terre le 5 septembre 1977 à bord d'une fusée Titan IIIE-Centaur, seize jours après sa jumelle, Voyager 2.");
            A("On a faster path, it overtook Voyager 2 and crossed the asteroid belt on the way to Jupiter. These planets are not to scale.",
                "더 빠른 경로로 날아 보이저 2호를 앞질렀고, 목성으로 가는 길에 소행성대를 건넜습니다. 이 행성들은 실제 비율이 아닙니다.",
                "より速い軌道でボイジャー2号を追い越し、木星へ向かう途中で小惑星帯を横切りました。この惑星たちは実際の比率ではありません。",
                "Por una trayectoria más rápida, adelantó a la Voyager 2 y cruzó el cinturón de asteroides camino de Júpiter. Estos planetas no están a escala.",
                "Sur une trajectoire plus rapide, elle a dépassé Voyager 2 et traversé la ceinture d'astéroïdes en route vers Jupiter. Ces planètes ne sont pas à l'échelle.");
            A("Voyager 1 passed Jupiter in March 1979, and its pictures showed the Great Red Spot, a storm bigger than the Earth, churning in detail.",
                "보이저 1호는 1979년 3월 목성을 지나갔고, 그 사진은 지구보다 큰 폭풍인 대적점이 소용돌이치는 모습을 자세히 보여 주었습니다.",
                "ボイジャー1号は1979年3月に木星を通過し、その写真は地球より大きな嵐、大赤斑が渦巻く様子を詳しく映し出しました。",
                "La Voyager 1 pasó junto a Júpiter en marzo de 1979, y sus imágenes mostraron con detalle la Gran Mancha Roja, una tormenta mayor que la Tierra, en plena agitación.",
                "Voyager 1 a frôlé Jupiter en mars 1979, et ses images ont montré en détail la Grande Tache rouge, une tempête plus grande que la Terre, en plein tourbillon.");
            A("It found volcanoes erupting on Jupiter's moon Io, the first active volcanoes ever seen beyond the Earth. Jupiter's gravity then flung it toward Saturn.",
                "목성의 위성 이오에서 분화하는 화산을 발견했는데, 지구 밖에서 처음으로 본 활화산이었습니다. 이어서 목성의 중력이 보이저를 토성 쪽으로 던졌습니다.",
                "木星の衛星イオで噴火する火山を発見しました。地球の外で初めて見つかった活火山です。その後、木星の重力がボイジャーを土星へと投げ出しました。",
                "Descubrió volcanes en erupción en Ío, la luna de Júpiter: los primeros volcanes activos vistos fuera de la Tierra. Luego la gravedad de Júpiter la lanzó hacia Saturno.",
                "Elle a découvert des volcans en éruption sur Io, la lune de Jupiter : les premiers volcans actifs jamais vus au-delà de la Terre. La gravité de Jupiter l'a ensuite lancée vers Saturne.");
            A("In November 1980 it passed Saturn and swung close by Titan to study its thick haze. That flyby bent its path up and out of the plane of the planets.",
                "1980년 11월 토성을 지나며 타이탄 가까이 돌아 두꺼운 안개를 관측했습니다. 그 근접 비행으로 경로가 휘어 행성들이 도는 평면 위쪽으로 벗어났습니다.",
                "1980年11月に土星を通過し、タイタンのすぐそばを回って厚いもやを調べました。そのフライバイで進路が曲がり、惑星の公転面の上へと抜け出しました。",
                "En noviembre de 1980 pasó junto a Saturno y se acercó a Titán para estudiar su espesa neblina. Ese sobrevuelo desvió su trayectoria hacia arriba, fuera del plano de los planetas.",
                "En novembre 1980, elle est passée près de Saturne et a frôlé Titan pour étudier sa brume épaisse. Ce survol a dévié sa trajectoire vers le haut, hors du plan des planètes.");
            A("On 14 February 1990, six billion kilometres out, Voyager 1 looked back and photographed the Earth: a single pale blue pixel in a sunbeam.",
                "1990년 2월 14일, 60억 킬로미터 떨어진 곳에서 보이저 1호가 뒤돌아 지구를 찍었습니다. 햇살 속에 떠 있는 창백한 푸른 픽셀 하나였습니다.",
                "1990年2月14日、60億キロメートルの彼方で、ボイジャー1号は振り返って地球を撮影しました。太陽光の筋の中に浮かぶ、淡く青いたった1ピクセルでした。",
                "El 14 de febrero de 1990, a seis mil millones de kilómetros, la Voyager 1 miró atrás y fotografió la Tierra: un único píxel azul pálido en un rayo de sol.",
                "Le 14 février 1990, à six milliards de kilomètres, Voyager 1 s'est retournée et a photographié la Terre : un seul pixel bleu pâle dans un rayon de soleil.");
            A("On 25 August 2012 it crossed the heliopause, where the Sun's wind gives way to the gas between the stars, the first human-made object to do so.",
                "2012년 8월 25일, 태양풍이 별과 별 사이의 가스에 자리를 내주는 경계인 태양권계면을 넘었습니다. 인간이 만든 물체로는 처음이었습니다.",
                "2012年8月25日、太陽風が星々の間のガスに道を譲る境界、ヘリオポーズを越えました。人類が作った物体として初めてのことです。",
                "El 25 de agosto de 2012 cruzó la heliopausa, donde el viento solar cede ante el gas que hay entre las estrellas: el primer objeto fabricado por el ser humano en hacerlo.",
                "Le 25 août 2012, elle a franchi l'héliopause, là où le vent solaire cède la place au gaz entre les étoiles : le premier objet fabriqué par l'homme à le faire.");
            A("It is still talking to us, on a radio about as strong as a fridge light. Its signals now take nearly a whole day to reach the Earth.",
                "냉장고 전구 정도의 세기인 무선으로 지금도 우리와 교신하고 있습니다. 이제 그 신호가 지구에 닿기까지 거의 하루가 걸립니다.",
                "冷蔵庫の電球ほどの強さの無線で、今も私たちと交信しています。その信号は今では地球に届くまでほぼ丸1日かかります。",
                "Todavía nos habla, con una radio tan potente como la bombilla de una nevera. Sus señales tardan ahora casi un día entero en llegar a la Tierra.",
                "Elle nous parle encore, avec une radio à peu près aussi puissante qu'une ampoule de réfrigérateur. Ses signaux mettent désormais près d'une journée entière à atteindre la Terre.");
            // the pictures beside the deck: titles and credits (agency names stay as they are)
            A("Pillars of Creation, Eagle Nebula", "창조의 기둥, 독수리 성운", "創造の柱、わし星雲", "Pilares de la Creación, nebulosa del Águila", "Piliers de la création, nébuleuse de l'Aigle");
            A("Hubble Ultra Deep Field", "허블 울트라 딥 필드", "ハッブル・ウルトラ・ディープ・フィールド", "Campo Ultra Profundo del Hubble", "Champ ultra-profond de Hubble");
            A("Mystic Mountain, Carina Nebula", "신비의 산, 용골자리 성운", "ミスティック・マウンテン、りゅうこつ座星雲", "Montaña Mística, nebulosa de Carina", "Montagne mystique, nébuleuse de la Carène");
            A("Jupiter's Great Red Spot", "목성의 대적점", "木星の大赤斑", "La Gran Mancha Roja de Júpiter", "La Grande Tache rouge de Jupiter");
            A("Jupiter with Io", "목성과 이오", "木星とイオ", "Júpiter con Ío", "Jupiter avec Io");
            A("Saturn with Tethys and Dione", "토성과 테티스, 디오네", "土星とテティス、ディオネ", "Saturno con Tetis y Dione", "Saturne avec Téthys et Dioné");
            A("NASA, ESA and the Hubble Heritage Team (STScI/AURA)", "NASA, ESA, Hubble Heritage Team (STScI/AURA)", "NASA、ESA、Hubble Heritage Team（STScI/AURA）", "NASA, ESA y el Hubble Heritage Team (STScI/AURA)", "NASA, ESA et la Hubble Heritage Team (STScI/AURA)");
            A("NASA, ESA, H. Teplitz and M. Rafelski, A. Koekemoer, R. Windhorst, Z. Levay", "NASA, ESA, H. Teplitz, M. Rafelski, A. Koekemoer, R. Windhorst, Z. Levay", "NASA、ESA、H. Teplitz、M. Rafelski、A. Koekemoer、R. Windhorst、Z. Levay", "NASA, ESA, H. Teplitz y M. Rafelski, A. Koekemoer, R. Windhorst, Z. Levay", "NASA, ESA, H. Teplitz et M. Rafelski, A. Koekemoer, R. Windhorst, Z. Levay");
            A("NASA, ESA and M. Livio and the Hubble 20th Anniversary Team (STScI)", "NASA, ESA, M. Livio, Hubble 20th Anniversary Team (STScI)", "NASA、ESA、M. Livio、Hubble 20th Anniversary Team（STScI）", "NASA, ESA, M. Livio y el Hubble 20th Anniversary Team (STScI)", "NASA, ESA, M. Livio et la Hubble 20th Anniversary Team (STScI)");
            A("NASA, ESA, J. Hester and A. Loll (Arizona State University)", "NASA, ESA, J. Hester, A. Loll (애리조나 주립대학교)", "NASA、ESA、J. Hester、A. Loll（アリゾナ州立大学）", "NASA, ESA, J. Hester y A. Loll (Universidad Estatal de Arizona)", "NASA, ESA, J. Hester et A. Loll (Université d'État de l'Arizona)");
        }
    }
}
