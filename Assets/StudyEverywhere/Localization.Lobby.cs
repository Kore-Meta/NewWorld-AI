using System.Collections.Generic;

namespace StudyEverywhere
{
    // The lobby's words (the welcome, its lessons, the place menu and the place cards) and the panels shared by every
    // place: the workspace and its display panels, the music panel, the wrist menus' common rows.
    public static partial class Localization
    {
        static partial void AddLobby(Dictionary<string, string[]> t)
        {
            void A(string en, string ko, string ja, string es, string fr) => t[en] = new[] { ko, ja, es, fr };

            // ---- the welcome
            A("Welcome to New World", "New World에 오신 것을 환영합니다", "New World へようこそ", "Bienvenido a New World", "Bienvenue dans New World");
            A("created for you by Kore Meta", "Kore Meta가 당신을 위해 만들었습니다", "Kore Meta があなたのために制作しました", "creado para ti por Kore Meta", "créé pour vous par Kore Meta");
            // the tagline, in the words of the three destinations (Explore, Focus, Unwind: the user, Oct 25)
            A("Explore to discover.\nFocus to think clearly.\nUnwind for a fresh perspective.\n\nFind a place that brings out our best.",
              "탐험하며 발견하세요.\n집중하며 맑게 생각하세요.\n쉬어 가며 새로운 시선을 얻으세요.\n\n우리의 가장 좋은 모습을 끌어내는 곳을 찾아보세요.",
              "探索して、発見する。\n集中して、澄んだ思考を。\nくつろいで、新しい視点を。\n\n私たちの最高を引き出す場所を見つけましょう。",
              "Explora para descubrir.\nConcéntrate para pensar con claridad.\nRelájate para una nueva perspectiva.\n\nEncuentra un lugar que saque lo mejor de nosotros.",
              "Explorez pour découvrir.\nConcentrez-vous pour penser clairement.\nDétendez-vous pour un regard neuf.\n\nTrouvez un lieu qui révèle le meilleur de nous-mêmes.");
            A("Learn to move with", "이동 방법 배우기", "操作を覚える", "Aprende a moverte con", "Apprendre à se déplacer avec");
            A("Hands", "손", "ハンド", "Manos", "Mains");
            A("Controllers", "컨트롤러", "コントローラー", "Mandos", "Manettes");
            A("< Back", "< 뒤로", "< 戻る", "< Atrás", "< Retour");
            A("Skip", "건너뛰기", "スキップ", "Omitir", "Passer");
            A("Done", "완료", "完了", "Listo", "Terminé");
            // hand lessons
            A("Point and pinch", "가리키고 집기", "指してピンチ", "Señala y pellizca", "Viser et pincer");
            A("Point at a button with an open hand: a ray comes from your hand. Pinch your thumb and index finger together to press.",
              "손을 편 채로 버튼을 가리키면 손에서 광선이 나옵니다. 엄지와 검지를 모아 집으면 눌러져요.",
              "手を開いてボタンを指すと、手から光線が出ます。親指と人差し指をつまむと押せます。",
              "Señala un botón con la mano abierta: sale un rayo de tu mano. Junta el pulgar y el índice para pulsar.",
              "Visez un bouton main ouverte : un rayon part de votre main. Pincez le pouce et l'index pour appuyer.");
            A("Touch", "터치", "タッチ", "Tocar", "Toucher");
            A("Or press a button directly with your index fingertip, just like a real one.",
              "또는 진짜 버튼처럼 검지 끝으로 버튼을 직접 눌러도 돼요.",
              "または、本物のボタンのように人差し指の先で直接押すこともできます。",
              "O pulsa un botón directamente con la punta del índice, como si fuera real.",
              "Ou appuyez directement sur un bouton du bout de l'index, comme sur un vrai.");
            A("Grab", "잡기", "つかむ", "Agarrar", "Saisir");
            A("Close your hand round a cup, a book or a panel's edge to pick it up. Open your hand to let go.",
              "컵이나 책, 패널 가장자리를 손으로 감싸 쥐면 집어 들 수 있어요. 손을 펴면 놓아요.",
              "カップや本、パネルの縁を手で握るとつかめます。手を開くと離します。",
              "Cierra la mano alrededor de una taza, un libro o el borde de un panel para cogerlo. Abre la mano para soltarlo.",
              "Refermez la main sur une tasse, un livre ou le bord d'un panneau pour le prendre. Ouvrez la main pour le lâcher.");
            A("Teleport", "순간이동", "テレポート", "Teletransporte", "Téléportation");
            A("Turn your palm up and hold out your thumb and index finger, the other fingers curled: an arc appears. Pinch to aim, move your hand to turn the arrow, and let go to go there.",
              "손바닥을 위로 하고 엄지와 검지를 펴고 나머지 손가락은 구부리면 곡선이 나타나요. 집어서 조준하고, 손을 움직여 화살표 방향을 돌린 뒤, 놓으면 그곳으로 이동해요.",
              "手のひらを上に向け、親指と人差し指を伸ばしてほかの指を曲げると、弧が現れます。ピンチして狙いを定め、手を動かして矢印の向きを変え、離すとそこへ移動します。",
              "Gira la palma hacia arriba y extiende el pulgar y el índice, con los demás dedos doblados: aparece un arco. Pellizca para apuntar, mueve la mano para girar la flecha y suelta para ir allí.",
              "Tournez la paume vers le haut et tendez le pouce et l'index, les autres doigts repliés : un arc apparaît. Pincez pour viser, bougez la main pour orienter la flèche, et relâchez pour y aller.");
            A("Turn", "회전", "回転", "Girar", "Tourner");
            A("Tap your thumb on the side of your index finger: near the knuckle to turn right, near the fingertip to turn left.",
              "엄지로 검지 옆면을 톡 치세요. 손가락 마디 쪽은 오른쪽, 손끝 쪽은 왼쪽으로 돌아요.",
              "親指で人差し指の側面をタップします。付け根の近くで右へ、指先の近くで左へ回ります。",
              "Toca con el pulgar el lateral del índice: cerca del nudillo para girar a la derecha, cerca de la punta para girar a la izquierda.",
              "Tapotez le côté de l'index avec le pouce : près de l'articulation pour tourner à droite, près du bout pour tourner à gauche.");
            A("Tap the watch on your left wrist with a finger of your other hand to open the menu.",
              "다른 손의 손가락으로 왼쪽 손목의 시계를 톡 치면 메뉴가 열려요.",
              "反対の手の指で左手首の時計をタップすると、メニューが開きます。",
              "Toca el reloj de tu muñeca izquierda con un dedo de la otra mano para abrir el menú.",
              "Touchez la montre à votre poignet gauche avec un doigt de l'autre main pour ouvrir le menu.");
            // controller lessons
            A("Point and press", "가리키고 누르기", "指して押す", "Señala y pulsa", "Viser et appuyer");
            A("Point the controller at a button and pull the trigger (under your index finger) to press it.",
              "컨트롤러로 버튼을 가리키고 트리거(검지 아래)를 당기면 눌러져요.",
              "コントローラーでボタンを指し、トリガー（人差し指の下）を引くと押せます。",
              "Apunta el mando a un botón y aprieta el gatillo (bajo el índice) para pulsarlo.",
              "Pointez la manette vers un bouton et pressez la gâchette (sous l'index) pour appuyer.");
            A("Squeeze the grip button (under your middle finger) to pick something up; let go to drop it.",
              "그립 버튼(중지 아래)을 쥐면 물건을 집고, 놓으면 내려놓아요.",
              "グリップボタン（中指の下）を握ると物をつかみ、離すと落とします。",
              "Aprieta el botón de agarre (bajo el dedo corazón) para coger algo; suéltalo para dejarlo.",
              "Serrez le bouton de préhension (sous le majeur) pour prendre un objet ; relâchez pour le lâcher.");
            // the lobby glides now (the user, Oct 25): the left stick walks, the right stick teleports and turns
            A("Walk and teleport", "걷기와 순간이동", "歩く・テレポート", "Caminar y teletransportarse", "Marcher et se téléporter");
            A("Push the left thumbstick to walk. To jump ahead, push the right thumbstick forward: an arc shows where you will land; let go to go there.",
              "왼쪽 썸스틱을 밀면 걸어요. 멀리 가려면 오른쪽 썸스틱을 앞으로 밀어요. 도착할 곳이 곡선으로 보이고, 놓으면 그곳으로 이동해요.",
              "左スティックを倒すと歩きます。先へ移動するには右スティックを前に倒します。着地点が弧で示され、離すとそこへ移動します。",
              "Empuja el joystick izquierdo para caminar. Para saltar, empuja el derecho hacia delante: un arco muestra dónde aterrizarás; suéltalo para ir allí.",
              "Poussez le joystick gauche pour marcher. Pour sauter plus loin, poussez le droit vers l'avant : un arc montre où vous arriverez ; relâchez pour y aller.");
            A("Flick the right thumbstick left or right to turn; pull it back to turn round.",
              "오른쪽 썸스틱을 왼쪽이나 오른쪽으로 튕기면 돌고, 뒤로 당기면 뒤돌아봐요.",
              "右スティックを左右に弾くと回転し、手前に引くと振り返ります。",
              "Mueve el joystick derecho a izquierda o derecha para girar; tira de él hacia atrás para darte la vuelta.",
              "Donnez un coup de joystick droit à gauche ou à droite pour tourner ; tirez-le vers l'arrière pour faire demi-tour.");
            A("Press the menu button or Y on the left controller to open the menu.",
              "왼쪽 컨트롤러의 메뉴 버튼이나 Y를 누르면 메뉴가 열려요.",
              "左コントローラーのメニューボタンか Y を押すと、メニューが開きます。",
              "Pulsa el botón de menú o Y en el mando izquierdo para abrir el menú.",
              "Appuyez sur le bouton menu ou sur Y de la manette gauche pour ouvrir le menu.");

            // ---- the place menu (LobbyBuilder writes "{n} open" for each kind of place) and the place cards
            A("1 open", "1곳 이용 가능", "1か所 利用可能", "1 disponible", "1 disponible");
            A("2 open", "2곳 이용 가능", "2か所 利用可能", "2 disponibles", "2 disponibles");
            A("3 open", "3곳 이용 가능", "3か所 利用可能", "3 disponibles", "3 disponibles");
            A("4 open", "4곳 이용 가능", "4か所 利用可能", "4 disponibles", "4 disponibles");
            A("Opening…", "여는 중…", "開いています…", "Abriendo…", "Ouverture…");
            A("Read and focus", "읽고 집중하기", "読んで集中する", "Lee y concéntrate", "Lire et se concentrer");
            A("Silent study among the shelves.", "서가 사이에서 조용히 공부하세요.", "書架に囲まれて静かに学習。", "Estudio en silencio entre las estanterías.", "Étude en silence parmi les rayonnages.");
            A("Cafés to study in", "공부하기 좋은 카페", "勉強できるカフェ", "Cafés para estudiar", "Des cafés où étudier");
            A("Warm light, soft music, good coffee.", "따뜻한 조명, 잔잔한 음악, 맛있는 커피.", "温かな灯り、やさしい音楽、おいしいコーヒー。", "Luz cálida, música suave, buen café.", "Lumière chaude, musique douce, bon café.");
            A("Pause and breathe", "잠시 쉬어 가기", "ひと息つく", "Haz una pausa y respira", "Faire une pause et respirer");
            A("Stars, fireflies and a quiet breeze.", "별, 반딧불이, 그리고 잔잔한 바람.", "星とホタルと静かなそよ風。", "Estrellas, luciérnagas y una brisa tranquila.", "Des étoiles, des lucioles et une brise légère.");
            A("Orbit, Haven and Solace: quiet study among the shelves.", "오빗, 헤이븐, 솔라스: 서가 사이에서 조용히 공부하세요.", "オービット、ヘイヴン、ソレス：書架に囲まれて静かに学習。", "Orbit, Haven y Solace: estudio tranquilo entre las estanterías.", "Orbit, Haven et Solace : l'étude au calme parmi les rayonnages.");
            A("Paris by the Seine; Seoul and Tokyo to come.", "센강 옆 파리, 서울과 도쿄도 곧 열립니다.", "セーヌ河畔のパリ。ソウルと東京も近日公開。", "París junto al Sena; Seúl y Tokio, próximamente.", "Paris au bord de la Seine ; Séoul et Tokyo bientôt.");
            A("Paris by the Seine, New York over Central Park, Seoul above Bukchon's rooftops.", "센강 옆 파리, 센트럴 파크 위 뉴욕, 북촌 지붕 위 서울.", "セーヌ河畔のパリ、セントラルパークを望むニューヨーク、北村の屋根を見下ろすソウル。", "París junto al Sena, Nueva York sobre Central Park, Seúl sobre los tejados de Bukchon.", "Paris au bord de la Seine, New York au-dessus de Central Park, Séoul au-dessus des toits de Bukchon.");
            A("New York Café", "뉴욕 카페", "ニューヨークのカフェ", "Café de Nueva York", "Café de New York");
            // the headings and places as the user named them (Oct 25): Explore, Café, Unwind; the cafés by their cities; Celeste
            A("Explore", "탐험", "探索", "Explorar", "Explorer");
            // the entrance cards' titles: the tagline's three lines (Oct 25)
            A("Explore to discover", "탐험하며 발견하세요", "探索して、発見する", "Explora para descubrir", "Explorez pour découvrir");
            A("Focus to think clearly", "집중하며 맑게 생각하세요", "集中して、澄んだ思考を", "Concéntrate para pensar con claridad", "Concentrez-vous pour penser clairement");
            A("Unwind for a fresh perspective", "쉬어 가며 새로운 시선을 얻으세요", "くつろいで、新しい視点を", "Relájate para una nueva perspectiva", "Détendez-vous pour un regard neuf");
            A("Orbit, Haven and Solace: libraries of space, history and music.", "오빗, 헤이븐, 솔라스: 우주와 역사와 음악의 도서관.", "オービット、ヘイブン、ソレイス:宇宙と歴史と音楽の図書館。", "Orbit, Haven y Solace: bibliotecas del espacio, la historia y la música.", "Orbit, Haven et Solace : des bibliothèques de l'espace, de l'histoire et de la musique.");
            A("One chair on calm water, history all round", "잔잔한 물 위의 의자 하나, 사방의 역사", "静かな水の上の椅子ひとつ、まわりに広がる歴史", "Una silla sobre el agua en calma, la historia alrededor", "Une chaise sur l'eau calme, l'histoire tout autour");
            A("Seoul above Bukchon's rooftops, Paris by the Seine, New York over Central Park.", "북촌 지붕 위 서울, 센강 옆 파리, 센트럴 파크 위 뉴욕.", "北村の屋根を見下ろすソウル、セーヌ河畔のパリ、セントラルパークを望むニューヨーク。", "Seúl sobre los tejados de Bukchon, París junto al Sena, Nueva York sobre Central Park.", "Séoul au-dessus des toits de Bukchon, Paris au bord de la Seine, New York au-dessus de Central Park.");
            A("A palm lagoon, a starry meadow and an alpine lake.", "야자수 석호, 별이 빛나는 초원, 그리고 알프스의 호수.", "ヤシのラグーン、星降る草原、そしてアルプスの湖。", "Una laguna con palmeras, un prado estrellado y un lago alpino.", "Un lagon bordé de palmiers, une prairie étoilée et un lac alpin.");
            A("Focus", "집중", "集中", "Concentración", "Concentration");
            A("Unwind", "휴식", "くつろぐ", "Relájate", "Détente");
            A("Study Café", "스터디 카페", "スタディカフェ", "Café de estudio", "Café d'étude");
            A("Celeste", "셀레스트", "セレステ", "Celeste", "Céleste");
            A("Paris", "파리", "パリ", "París", "Paris");
            A("New York", "뉴욕", "ニューヨーク", "Nueva York", "New York");
            A("Seoul", "서울", "ソウル", "Seúl", "Séoul");
            A("By the Seine", "센강변", "セーヌ河畔", "A orillas del Sena", "Au bord de la Seine");
            A("Over Central Park", "센트럴 파크 위", "セントラルパークを望む", "Sobre Central Park", "Au-dessus de Central Park");
            A("A hanok tea room above Bukchon", "북촌 위 한옥 찻집", "北村を見下ろす韓屋の茶房", "Un salón de té hanok sobre Bukchon", "Un salon de thé hanok au-dessus de Bukchon");
            A("A starry meadow, a palm lagoon and an alpine lake.", "별이 빛나는 초원, 야자수 석호, 그리고 알프스의 호수.", "星降る草原、ヤシのラグーン、そしてアルプスの湖。", "Un prado estrellado, una laguna de palmeras y un lago alpino.", "Une prairie étoilée, un lagon bordé de palmiers et un lac alpin.");

            // ---- the workspace, its display panels and the music panel (shared by every place with a desk)
            A("Open Workspace", "작업 공간 열기", "ワークスペースを開く", "Abrir espacio de trabajo", "Ouvrir l'espace de travail");
            A("Open display", "디스플레이 열기", "ディスプレイを開く", "Abrir pantalla", "Ouvrir l'écran");
            A("Open Music", "음악 열기", "音楽を開く", "Abrir música", "Ouvrir la musique");
            A("Paused", "일시정지됨", "一時停止中", "En pausa", "En pause");
            A("Muted", "음소거됨", "ミュート中", "Silenciado", "Muet");
            A("Disconnected", "연결 끊김", "切断されました", "Desconectado", "Déconnecté");
            A("Code expires in 2:00", "코드 만료까지 2:00", "コードの有効期限 残り 2:00", "El código caduca en 2:00", "Le code expire dans 2:00");
            A("Code expires in {0}:{1:00}", "코드 만료까지 {0}:{1:00}", "コードの有効期限 残り {0}:{1:00}", "El código caduca en {0}:{1:00}", "Le code expire dans {0}:{1:00}");
            A("Pairing with your computer.", "컴퓨터와 페어링하는 중입니다.", "パソコンとペアリングしています。", "Emparejando con tu ordenador.", "Association avec votre ordinateur.");
            A("Approved on your computer. Starting encrypted streams.", "컴퓨터에서 허용했습니다. 암호화된 스트림을 시작합니다.", "パソコンで許可されました。暗号化ストリームを開始します。", "Aprobado en tu ordenador. Iniciando transmisiones cifradas.", "Approuvé sur votre ordinateur. Démarrage des flux chiffrés.");
            A("Sharing {0} displays. Only you can see these panels.", "디스플레이 {0}개를 공유 중입니다. 이 패널은 나만 볼 수 있어요.", "{0} 台のディスプレイを共有中。これらのパネルはあなただけに見えます。", "Compartiendo {0} pantallas. Solo tú puedes ver estos paneles.", "Partage de {0} écrans. Vous seul voyez ces panneaux.");
            A("Sharing your computer's screen. Only you can see this panel.", "컴퓨터 화면을 공유 중입니다. 이 패널은 나만 볼 수 있어요.", "パソコンの画面を共有中。このパネルはあなただけに見えます。", "Compartiendo la pantalla de tu ordenador. Solo tú puedes ver este panel.", "Partage de l'écran de votre ordinateur. Vous seul voyez ce panneau.");
            A("A display paused. Reconnecting to your computer.", "디스플레이 하나가 멈췄습니다. 컴퓨터에 다시 연결하는 중입니다.", "ディスプレイが一時停止しました。パソコンに再接続しています。", "Una pantalla se ha pausado. Reconectando con tu ordenador.", "Un écran est en pause. Reconnexion à votre ordinateur.");
            A("Your computer is no longer connected.", "컴퓨터 연결이 끊어졌습니다.", "パソコンとの接続が切れました。", "Tu ordenador ya no está conectado.", "Votre ordinateur n'est plus connecté.");
            A("Sharing stopped. Your computer is no longer sending its screens.", "공유가 중지되었습니다. 컴퓨터가 더 이상 화면을 보내지 않아요.", "共有を停止しました。パソコンからの画面送信は止まっています。", "Se ha dejado de compartir. Tu ordenador ya no envía sus pantallas.", "Partage arrêté. Votre ordinateur n'envoie plus ses écrans.");
            A("Your computer is no longer sharing any displays.", "컴퓨터가 더 이상 디스플레이를 공유하지 않아요.", "パソコンはどのディスプレイも共有していません。", "Tu ordenador ya no comparte ninguna pantalla.", "Votre ordinateur ne partage plus aucun écran.");
            A("The code expired before the computer approved. Retry for a new code.", "컴퓨터에서 허용하기 전에 코드가 만료되었습니다. 다시 시도해 새 코드를 받으세요.", "パソコンで許可される前にコードの有効期限が切れました。再試行して新しいコードを取得してください。", "El código caducó antes de que el ordenador lo aprobara. Reintenta para obtener uno nuevo.", "Le code a expiré avant l'approbation de l'ordinateur. Réessayez pour obtenir un nouveau code.");
            A("Couldn't reach the computer. Check it is awake and on the same Wi-Fi.", "컴퓨터에 연결할 수 없습니다. 컴퓨터가 켜져 있고 같은 Wi-Fi에 있는지 확인하세요.", "パソコンに接続できませんでした。起動していて同じ Wi-Fi にあるか確認してください。", "No se pudo conectar con el ordenador. Comprueba que esté encendido y en la misma Wi-Fi.", "Impossible de joindre l'ordinateur. Vérifiez qu'il est allumé et sur le même Wi-Fi.");
            A("The streams stopped. Check the computer is awake and on the same Wi-Fi.", "스트림이 멈췄습니다. 컴퓨터가 켜져 있고 같은 Wi-Fi에 있는지 확인하세요.", "ストリームが止まりました。パソコンが起動していて同じ Wi-Fi にあるか確認してください。", "Las transmisiones se detuvieron. Comprueba que el ordenador esté encendido y en la misma Wi-Fi.", "Les flux se sont arrêtés. Vérifiez que l'ordinateur est allumé et sur le même Wi-Fi.");
            A("Your computer approved sharing. Setting up an encrypted stream for each display on your local network.", "컴퓨터에서 공유를 허용했습니다. 로컬 네트워크에서 디스플레이마다 암호화된 스트림을 설정하는 중입니다.", "パソコンで共有が許可されました。ローカルネットワーク上で、ディスプレイごとに暗号化ストリームを準備しています。", "Tu ordenador aprobó compartir. Configurando una transmisión cifrada para cada pantalla en tu red local.", "Votre ordinateur a autorisé le partage. Mise en place d'un flux chiffré pour chaque écran sur votre réseau local.");
            A("Sharing was stopped on the computer.", "컴퓨터에서 공유가 중지되었습니다.", "パソコン側で共有が停止されました。", "Se dejó de compartir en el ordenador.", "Le partage a été arrêté sur l'ordinateur.");
            A("MAIN", "메인", "メイン", "PRINCIPAL", "PRINCIPAL");
            A("above main", "메인 위", "メインの上", "encima de la principal", "au-dessus de l'écran principal");
            A("below main", "메인 아래", "メインの下", "debajo de la principal", "sous l'écran principal");
            A("right of main", "메인 오른쪽", "メインの右", "a la derecha de la principal", "à droite de l'écran principal");
            A("left of main", "메인 왼쪽", "メインの左", "a la izquierda de la principal", "à gauche de l'écran principal");
        }
    }
}
