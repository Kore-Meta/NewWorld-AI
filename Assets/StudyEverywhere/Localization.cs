using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyEverywhere
{
    // New World's languages: the UI's words in English, Korean, Japanese, Spanish and French, chosen from any menu's
    // Language row and remembered. Text is keyed by its English wording (as the builders write it), so Localizer can
    // swap any text in a scene without per-label wiring; code that sets text at run time calls Tr. Korean and Japanese
    // glyphs come from the Noto Sans fallback fonts (added to every TextMeshPro font by the builders). Orbit is asked
    // to reply in the chosen language too.
    // The table is split by area: the core words here, each place's in its own file (Localization.<Area>.cs, a partial
    // of this class implementing its Add method). Proper nouns (Orbit, Hori, Calypso) are written as they sound
    // in each script: 오빗, ヘイヴン, and so on.
    public static partial class Localization
    {
        static partial void AddLobby(Dictionary<string, string[]> t);
        static partial void AddCafe(Dictionary<string, string[]> t);
        static partial void AddLibraries(Dictionary<string, string[]> t);
        static partial void AddRest(Dictionary<string, string[]> t);
        static partial void AddOrbitDeck(Dictionary<string, string[]> t);
        static partial void AddOrbitSpace(Dictionary<string, string[]> t);
        static partial void AddTimeStation(Dictionary<string, string[]> t);
        static partial void AddRealObjects(Dictionary<string, string[]> t);
        static partial void AddComposers(Dictionary<string, string[]> t);
        static partial void AddHistory(Dictionary<string, string[]> t);
        static partial void AddTheory(Dictionary<string, string[]> t);
        static Dictionary<string, string[]> table;
        static Dictionary<string, string[]> Table
        {
            get
            {
                if (table != null) return table;
                var t = new Dictionary<string, string[]>(Core);
                AddLobby(t); AddCafe(t); AddLibraries(t); AddRest(t); AddOrbitDeck(t); AddOrbitSpace(t); AddTimeStation(t); AddRealObjects(t); AddComposers(t); AddHistory(t); AddTheory(t);
                return table = t;
            }
        }
        public static readonly string[] Codes = { "en", "ko", "ja", "es", "fr" };
        public static readonly string[] Names = { "English", "한국어", "日本語", "Español", "Français" };
        public static readonly string[] EnglishNames = { "English", "Korean", "Japanese", "Spanish", "French" };
        const string Key = "NewWorld.Language";

        public static event Action Changed;
        public static int Current
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(Key, 0), 0, Codes.Length - 1);
            set { int v = Mathf.Clamp(value, 0, Codes.Length - 1); if (v == Current) return; PlayerPrefs.SetInt(Key, v); PlayerPrefs.Save(); Changed?.Invoke(); }
        }
        public static string Code => Codes[Current];
        public static string LanguageName => EnglishNames[Current];
        static bool Latin => Current == 0 || Current >= 3;

        // The words for the current language, or the English as given when there is no translation.
        public static string Tr(string english)
        {
            if (string.IsNullOrEmpty(english) || Current == 0) return english;
            if (Table.TryGetValue(english, out var t)) return t[Current - 1];
            // labels the builders write in capitals: translate the plain form, capitalise again for Latin scripts
            var upper = english.ToUpperInvariant();
            if (english == upper && Upper.TryGetValue(english, out var plain)) { var x = Table[plain][Current - 1]; return Latin ? x.ToUpperInvariant() : x; }
            // the same words with spaces round them ("Next  >" is its own key; " Close " is Close)
            var trimmed = english.Trim();
            if (trimmed.Length != english.Length && trimmed.Length > 0 && Knows(trimmed)) return english.Replace(trimmed, Tr(trimmed));
            return english;
        }
        public static bool Knows(string english)
        {
            if (string.IsNullOrEmpty(english)) return false;
            if (Table.ContainsKey(english) || Upper.ContainsKey(english)) return true;
            var trimmed = english.Trim(); return trimmed.Length != english.Length && trimmed.Length > 0 && (Table.ContainsKey(trimmed) || Upper.ContainsKey(trimmed));
        }
        public static string Format(string english, params object[] args) => string.Format(Tr(english), args);

        // English -> Korean, Japanese, Spanish, French
        static readonly Dictionary<string, string[]> Core = new Dictionary<string, string[]>
        {
            // ---- proper nouns, as they sound
            ["Orbit"] = new[] { "오빗", "オービット", "Orbit", "Orbit" },
            ["Hori"] = new[] { "호리", "ホリ", "Hori", "Hori" },
            ["Tutorial"] = new[] { "튜토리얼", "チュートリアル", "Tutorial", "Tutoriel" },
            ["Done"] = new[] { "완료", "完了", "Listo", "Terminé" },
            // ---- menus
            ["Quick settings"] = new[] { "빠른 설정", "クイック設定", "Ajustes rápidos", "Réglages rapides" },
            ["Music"] = new[] { "음악", "音楽", "Música", "Musique" },
            ["Music volume"] = new[] { "음악 볼륨", "音楽の音量", "Volumen de la música", "Volume de la musique" },
            ["Nature sounds"] = new[] { "자연의 소리", "自然の音", "Sonidos de la naturaleza", "Sons de la nature" },
            ["Time of day"] = new[] { "시간대", "時間帯", "Hora del día", "Moment de la journée" },
            ["Morning"] = new[] { "아침", "朝", "Mañana", "Matin" },
            ["Afternoon"] = new[] { "오후", "午後", "Tarde", "Après-midi" },
            ["Sunset"] = new[] { "해 질 녘", "夕暮れ", "Atardecer", "Coucher du soleil" },
            ["Night"] = new[] { "밤", "夜", "Noche", "Nuit" },
            ["Cycle"] = new[] { "순환", "サイクル", "Ciclo", "Cycle" },
            ["Lights"] = new[] { "조명", "照明", "Luces", "Lumières" },
            ["On"] = new[] { "켜짐", "オン", "Activado", "Activé" },
            ["Off"] = new[] { "꺼짐", "オフ", "Desactivado", "Désactivé" },
            ["View height"] = new[] { "시점 높이", "視点の高さ", "Altura de la vista", "Hauteur de vue" },
            ["Recenter"] = new[] { "다시 맞추기", "リセンター", "Recentrar", "Recentrer" },
            ["Close"] = new[] { "닫기", "閉じる", "Cerrar", "Fermer" },
            ["Back"] = new[] { "뒤로", "戻る", "Atrás", "Retour" },
            ["Go to"] = new[] { "이동", "移動", "Ir a", "Aller à" },
            ["Locations"] = new[] { "장소", "場所", "Lugares", "Lieux" },
            ["Watch"] = new[] { "시계", "時計", "Reloj", "Montre" },
            ["Left wrist"] = new[] { "왼쪽 손목", "左手首", "Muñeca izquierda", "Poignet gauche" },
            ["Right wrist"] = new[] { "오른쪽 손목", "右手首", "Muñeca derecha", "Poignet droit" },
            ["Point at any chair and pinch to sit."] = new[] { "의자를 가리키고 손가락을 모아 앉으세요.", "椅子を指してピンチすると座れます。", "Señala una silla y pellizca para sentarte.", "Visez une chaise et pincez pour vous asseoir." },
            ["Language"] = new[] { "언어", "言語", "Idioma", "Langue" },
            ["AI"] = new[] { "AI", "AI", "IA", "IA" },
            ["Clouds"] = new[] { "구름", "雲", "Nubes", "Nuages" },
            // the pets' and the nature rows (Oct 25)
            ["Talk to Hori"] = new[] { "호리와 이야기하기", "ホリと話す", "Hablar con Hori", "Parler à Hori" },
            ["Listening"] = new[] { "듣기", "聞き取り", "Escucha", "Écoute" },
            ["Clear"] = new[] { "맑음", "快晴", "Despejado", "Dégagé" },
            ["Some"] = new[] { "약간", "少し", "Algunas", "Quelques" },
            ["Cloudy"] = new[] { "흐림", "曇り", "Nublado", "Nuageux" },
            ["Gravity"] = new[] { "중력", "重力", "Gravedad", "Gravité" },
            ["Floating can cause motion discomfort: turn gravity back on if you feel unwell. Off: grab the air and pull, or hold A / X to jet."] = new[]
                { "떠다니면 멀미가 날 수 있어요. 불편하면 중력을 다시 켜세요. 끄면: 허공을 잡아 당기거나 A / X를 눌러 분사하세요.", "浮遊すると酔うことがあります。気分が悪くなったら重力をオンに戻してください。オフ時：空中をつかんで引くか、A / X を押して噴射。", "Flotar puede causar mareo: vuelve a activar la gravedad si te sientes mal. Sin gravedad: agarra el aire y tira, o mantén A / X para propulsarte.", "Flotter peut donner le mal des transports : réactivez la gravité si vous ne vous sentez pas bien. Sans gravité : saisissez l'air et tirez, ou maintenez A / X pour propulser." },
            ["Moon"] = new[] { "달", "月", "Luna", "Lune" },
            ["Moon phase"] = new[] { "달의 모양", "月の満ち欠け", "Fase lunar", "Phase de la lune" },
            ["Still"] = new[] { "고정", "固定", "Quieta", "Fixe" },
            ["Rise & set"] = new[] { "뜨고 지기", "出入り", "Sale y se pone", "Lever et coucher" },
            ["Fireflies"] = new[] { "반딧불이", "ホタル", "Luciérnagas", "Lucioles" },
            ["Weather"] = new[] { "날씨", "天気", "Tiempo", "Météo" },
            ["Eiffel lights"] = new[] { "에펠탑 조명", "エッフェル塔の灯り", "Luces de la Torre Eiffel", "Lumières de la tour Eiffel" },
            ["Café cat"] = new[] { "카페 고양이", "カフェの猫", "Gato del café", "Chat du café" },
            ["Listens to you"] = new[] { "말을 들어요", "話を聞きます", "Te escucha", "Vous écoute" },
            ["Menu"] = new[] { "메뉴", "メニュー", "Menú", "Menu" },
            // ---- places
            ["Desk"] = new[] { "책상", "デスク", "Escritorio", "Bureau" },
            ["Window"] = new[] { "창가", "窓辺", "Ventana", "Fenêtre" },
            ["Lounge"] = new[] { "라운지", "ラウンジ", "Salón", "Salon" },
            ["Stand"] = new[] { "서기", "立つ", "De pie", "Debout" },
            ["Table"] = new[] { "테이블", "テーブル", "Mesa", "Table" },
            ["Stage"] = new[] { "무대", "ステージ", "Escenario", "Scène" },
            ["Garden"] = new[] { "정원", "庭", "Jardín", "Jardin" },
            ["Reading"] = new[] { "독서", "読書", "Lectura", "Lecture" },
            ["Deck"] = new[] { "홀로덱", "ホロデッキ", "Holocubierta", "Holopont" },
            ["Sit here"] = new[] { "여기 앉기", "ここに座る", "Sentarse aquí", "S'asseoir ici" },
            ["Stand up"] = new[] { "일어서기", "立ち上がる", "Levantarse", "Se lever" },
            ["Choose a place"] = new[] { "장소 선택", "場所を選ぶ", "Elige un lugar", "Choisir un lieu" },
            ["Lobby"] = new[] { "로비", "ロビー", "Vestíbulo", "Hall d'accueil" },
            ["Where everyone arrives"] = new[] { "모두가 도착하는 곳", "みんなが集まる場所", "Donde todos llegan", "Là où tout le monde arrive" },
            ["You are here"] = new[] { "현재 위치", "現在地", "Estás aquí", "Vous êtes ici" },
            ["Coming soon"] = new[] { "곧 공개", "近日公開", "Próximamente", "Bientôt" },
            ["Premium"] = new[] { "프리미엄", "プレミアム", "Premium", "Premium" },
            ["Library"] = new[] { "도서관", "図書館", "Biblioteca", "Bibliothèque" },
            ["Libraries"] = new[] { "도서관", "図書館", "Bibliotecas", "Bibliothèques" },
            ["Café"] = new[] { "카페", "カフェ", "Café", "Café" },
            ["Cafés"] = new[] { "카페", "カフェ", "Cafés", "Cafés" },
            ["Rest Area"] = new[] { "휴식 공간", "休憩エリア", "Zona de descanso", "Espace détente" },
            ["Rest Areas"] = new[] { "휴식 공간", "休憩エリア", "Zonas de descanso", "Espaces détente" },
            ["A library in orbit round the Earth"] = new[] { "지구 궤도를 도는 도서관", "地球を周回する図書館", "Una biblioteca en órbita alrededor de la Tierra", "Une bibliothèque en orbite autour de la Terre" },
            ["A sunlit library by the lake"] = new[] { "호숫가의 햇살 가득한 도서관", "湖畔の陽だまりの図書館", "Una biblioteca soleada junto al lago", "Une bibliothèque ensoleillée au bord du lac" },
            ["A glasshouse library in the garden"] = new[] { "정원 속 온실 도서관", "庭のガラスハウス図書館", "Una biblioteca invernadero en el jardín", "Une bibliothèque-serre au jardin" },
            ["A night under the stars"] = new[] { "별빛 아래의 밤", "星空の下の夜", "Una noche bajo las estrellas", "Une nuit à la belle étoile" },
            ["Seoul Café"] = new[] { "서울 카페", "ソウルのカフェ", "Café de Seúl", "Café de Séoul" },
            ["Tokyo Café"] = new[] { "도쿄 카페", "東京のカフェ", "Café de Tokio", "Café de Tokyo" },
            ["Seabreeze"] = new[] { "시브리즈", "シーブリーズ", "Seabreeze", "Seabreeze" },
            ["A palm beach by the lagoon"] = new[] { "석호 곁 야자수 해변", "ラグーンのヤシの浜辺", "Una playa de palmeras junto a la laguna", "Une plage de palmiers au bord du lagon" },
            ["Garden Lounge"] = new[] { "정원 라운지", "ガーデンラウンジ", "Salón del jardín", "Salon du jardin" },
            ["Enter"] = new[] { "입장", "入る", "Entrar", "Entrer" },
            ["Go"] = new[] { "이동", "行く", "Ir", "Aller" },
            ["Quiet library"] = new[] { "조용한 도서관", "静かな図書館", "Biblioteca tranquila", "Bibliothèque calme" },
            ["Study café"] = new[] { "스터디 카페", "勉強カフェ", "Café de estudio", "Café d'étude" },
            // ---- desk panels
            ["Now playing"] = new[] { "재생 중", "再生中", "Sonando", "En cours" },
            ["Volume"] = new[] { "볼륨", "音量", "Volumen", "Volume" },
            ["Prev"] = new[] { "이전", "前へ", "Anterior", "Préc." },
            ["Next"] = new[] { "다음", "次へ", "Siguiente", "Suiv." },
            ["Pause"] = new[] { "일시정지", "一時停止", "Pausa", "Pause" },
            ["Play"] = new[] { "재생", "再生", "Reproducir", "Lecture" },
            ["Mute"] = new[] { "음소거", "ミュート", "Silenciar", "Muet" },
            ["Unmute"] = new[] { "음소거 해제", "ミュート解除", "Activar sonido", "Son" },
            ["Workspace"] = new[] { "작업 공간", "ワークスペース", "Espacio de trabajo", "Espace de travail" },
            ["A quiet place to focus"] = new[] { "집중하기 좋은 조용한 곳", "集中できる静かな場所", "Un lugar tranquilo para concentrarse", "Un endroit calme pour se concentrer" },
            ["Bring your computer's screens into your study space."] = new[] { "컴퓨터 화면을 공부 공간으로 가져오세요.", "パソコンの画面を学習スペースに。", "Trae las pantallas de tu ordenador a tu espacio de estudio.", "Amenez les écrans de votre ordinateur dans votre espace d'étude." },
            ["Choose a source"] = new[] { "소스 선택", "ソースを選択", "Elige una fuente", "Choisir une source" },
            ["What would you like on your study screen?"] = new[] { "공부 화면에 무엇을 띄울까요?", "学習画面に何を表示しますか？", "¿Qué quieres en tu pantalla de estudio?", "Que voulez-vous sur votre écran d'étude ?" },
            ["Connect a computer"] = new[] { "컴퓨터 연결", "パソコンを接続", "Conectar un ordenador", "Connecter un ordinateur" },
            ["A Windows PC on the same Wi-Fi, with the New World desktop app"] = new[] { "같은 Wi-Fi의 Windows PC와 New World 데스크톱 앱", "同じ Wi-Fi の Windows PC と New World デスクトップアプリ", "Un PC con Windows en la misma Wi-Fi, con la app de escritorio New World", "Un PC Windows sur le même Wi-Fi, avec l'appli de bureau New World" },
            ["Pair your computer"] = new[] { "컴퓨터 페어링", "パソコンをペアリング", "Empareja tu ordenador", "Associer votre ordinateur" },
            ["Enter this code in the New World desktop app,\nthen approve screen sharing on your computer."] = new[] { "New World 데스크톱 앱에 이 코드를 입력한 뒤\n컴퓨터에서 화면 공유를 허용하세요.", "New World デスクトップアプリにこのコードを入力し、\nパソコンで画面共有を許可してください。", "Introduce este código en la app de escritorio New World\ny aprueba compartir pantalla en tu ordenador.", "Saisissez ce code dans l'appli de bureau New World,\npuis autorisez le partage d'écran sur votre ordinateur." },
            ["Install the New World Desktop app on a Windows PC on this Wi-Fi: {0}"] = new[] { "같은 Wi-Fi의 Windows PC에 New World 데스크톱 앱을 설치하세요: {0}", "同じ Wi-Fi の Windows PC に New World デスクトップアプリをインストールしてください: {0}", "Instala la app New World Desktop en un PC con Windows de esta red Wi-Fi: {0}", "Installez l'app New World Desktop sur un PC Windows de ce Wi-Fi : {0}" },
            ["The code expired before the computer approved. Is the New World Desktop app installed and open on your PC? Get it at {0}, then Retry."] = new[] { "컴퓨터에서 허용하기 전에 코드가 만료되었어요. PC에 New World 데스크톱 앱이 설치되어 열려 있나요? {0}에서 받은 뒤 다시 시도하세요.", "パソコンで許可される前にコードの有効期限が切れました。PC に New World デスクトップアプリがインストールされ、開いていますか？ {0} から入手して、再試行してください。", "El código caducó antes de que el ordenador lo aprobara. ¿Está la app New World Desktop instalada y abierta en tu PC? Descárgala en {0} y vuelve a intentarlo.", "Le code a expiré avant l'approbation sur l'ordinateur. L'app New World Desktop est-elle installée et ouverte sur votre PC ? Téléchargez-la sur {0}, puis réessayez." },
            ["Cancel"] = new[] { "취소", "キャンセル", "Cancelar", "Annuler" },
            ["Connecting"] = new[] { "연결 중", "接続中", "Conectando", "Connexion" },
            ["Retry"] = new[] { "다시 시도", "再試行", "Reintentar", "Réessayer" },
            ["Stop sharing"] = new[] { "공유 중지", "共有を停止", "Dejar de compartir", "Arrêter le partage" },
            ["Stop"] = new[] { "중지", "停止", "Detener", "Arrêter" },
            ["Picture paused"] = new[] { "화면 일시정지됨", "映像が一時停止中", "Imagen en pausa", "Image en pause" },
            ["Reconnecting to your computer. Nothing new is arriving."] = new[] { "컴퓨터에 다시 연결하는 중입니다. 새 화면이 오지 않아요.", "パソコンに再接続しています。新しい映像が届いていません。", "Reconectando con tu ordenador. No llega nada nuevo.", "Reconnexion à votre ordinateur. Rien de nouveau n'arrive." },
            ["Resize"] = new[] { "크기 조절", "サイズ変更", "Redimensionar", "Redimensionner" },
            ["Display"] = new[] { "디스플레이", "ディスプレイ", "Pantalla", "Écran" },
            ["Live"] = new[] { "실시간", "ライブ", "En directo", "En direct" },
            ["Press"] = new[] { "누르기", "押す", "Pulsa", "Appuyez" },
            ["Browser"] = new[] { "브라우저", "ブラウザ", "Navegador", "Navigateur" },
            ["Document"] = new[] { "문서", "ドキュメント", "Documento", "Document" },
            // ---- Orbit's deck
            ["Power"] = new[] { "전원", "電源", "Encender", "Allumer" },
            ["Parts"] = new[] { "부품", "パーツ", "Piezas", "Pièces" },
            ["Assemble"] = new[] { "조립", "組み立て", "Montar", "Assembler" },
            ["Realistic"] = new[] { "실사", "リアル", "Realista", "Réaliste" },
            ["Hologram"] = new[] { "홀로그램", "ホログラム", "Holograma", "Hologramme" },
            ["Wireframe"] = new[] { "와이어프레임", "ワイヤーフレーム", "Malla", "Filaire" },
            ["Spacecraft"] = new[] { "우주선", "宇宙機", "Naves", "Engins spatiaux" },
            ["Solar System"] = new[] { "태양계", "太陽系", "Sistema solar", "Système solaire" },
            ["Universe"] = new[] { "우주", "宇宙", "Universo", "Univers" },
            ["System"] = new[] { "전체 보기", "全体", "Sistema", "Système" },
            ["Start"] = new[] { "처음으로", "最初へ", "Inicio", "Début" },
            ["Math"] = new[] { "수학", "数学", "Matemáticas", "Maths" },
            ["Replay"] = new[] { "다시 보기", "リプレイ", "Repetir", "Rejouer" },
            ["Calculator"] = new[] { "계산기", "電卓", "Calculadora", "Calculatrice" },
            ["Variables"] = new[] { "변수", "変数", "Variables", "Variables" },
            ["Mission"] = new[] { "미션", "ミッション", "Misión", "Mission" },
            ["Ask Orbit"] = new[] { "오빗에게 묻기", "オービットに聞く", "Pregunta a Orbit", "Demander à Orbit" },
            ["Tell me about it"] = new[] { "설명해 줘", "教えて", "Cuéntame", "Raconte-moi" },
            ["A fun fact"] = new[] { "재미있는 사실", "豆知識", "Un dato curioso", "Une anecdote" },
            ["Show the parts"] = new[] { "부품 보기", "パーツを見る", "Mostrar las piezas", "Montrer les pièces" },
            ["Next craft"] = new[] { "다음 우주선", "次の機体", "Siguiente nave", "Engin suivant" },
            ["Say “Orbit” to ask   ·   pinch the air to turn   ·   two hands to resize   ·   sweep to change"] = new[] { "“오빗”이라고 말해 질문   ·   허공을 집어 돌리기   ·   두 손으로 크기 조절   ·   쓸어서 바꾸기", "「オービット」と言って質問   ·   空中をつまんで回す   ·   両手で拡大縮小   ·   スワイプで切り替え", "Di «Orbit» para preguntar   ·   pellizca el aire para girar   ·   dos manos para escalar   ·   desliza para cambiar", "Dites « Orbit » pour demander   ·   pincez l'air pour tourner   ·   deux mains pour redimensionner   ·   balayez pour changer" },
            ["Say “Orbit”, press Ask Orbit, or tap a question below."] = new[] { "“오빗”이라고 말하거나, 오빗에게 묻기를 누르거나, 아래 질문을 눌러 보세요.", "「オービット」と言うか、オービットに聞くを押すか、下の質問をタップしてください。", "Di «Orbit», pulsa Pregunta a Orbit o toca una pregunta abajo.", "Dites « Orbit », appuyez sur Demander à Orbit ou touchez une question ci-dessous." },
            ["Listening…"] = new[] { "듣는 중…", "聞いています…", "Escuchando…", "J'écoute…" },
            ["thinking…"] = new[] { "생각 중…", "考え中…", "pensando…", "je réfléchis…" },
            ["I didn't catch that. Try again, say “Orbit” first, or tap a question."] = new[] { "잘 못 들었어요. 다시 말하거나, 먼저 “오빗”이라고 하거나, 질문을 눌러 주세요.", "聞き取れませんでした。もう一度、「オービット」と言ってから話すか、質問をタップしてください。", "No te he entendido. Inténtalo de nuevo, di «Orbit» primero o toca una pregunta.", "Je n'ai pas compris. Réessayez, dites « Orbit » d'abord, ou touchez une question." },
            ["from the fact sheet (Orbit's relay is offline)"] = new[] { "자료집에서 답변 (오빗 연결이 꺼져 있음)", "資料から回答（オービットの中継がオフライン）", "de la ficha (el enlace de Orbit está desconectado)", "d'après la fiche (le relais d'Orbit est hors ligne)" },
            ["I'm listening. Ask me anything about {0}, or tap a question below."] = new[] { "듣고 있어요. {0}에 대해 무엇이든 물어보거나 아래 질문을 눌러 보세요.", "聞いています。{0}について何でも聞いてください。下の質問をタップしても大丈夫です。", "Te escucho. Pregúntame lo que quieras sobre {0}, o toca una pregunta abajo.", "Je vous écoute. Demandez-moi ce que vous voulez sur {0}, ou touchez une question ci-dessous." },
            ["these spacecraft"] = new[] { "이 우주선들", "これらの宇宙機", "estas naves", "ces engins" },
            ["My microphone isn't set up yet, but tap a question below and I'll answer."] = new[] { "아직 마이크가 설정되지 않았어요. 아래 질문을 누르면 답해 드릴게요.", "マイクはまだ設定されていませんが、下の質問をタップすれば答えます。", "Mi micrófono aún no está configurado, pero toca una pregunta abajo y te responderé.", "Mon micro n'est pas encore réglé, mais touchez une question ci-dessous et je répondrai." },
            ["Models not to scale"] = new[] { "모형은 실제 비율이 아닙니다", "模型の縮尺は実物と異なります", "Modelos no a escala", "Modèles pas à l'échelle" },
        };
        // capitals -> the plain key (labels the builders write in capitals)
        static Dictionary<string, string> upper;
        static Dictionary<string, string> Upper => upper ??= BuildUpper();
        static Dictionary<string, string> BuildUpper()
        {
            var d = new Dictionary<string, string>();
            foreach (var k in Table.Keys) { var u = k.ToUpperInvariant(); if (u != k && !d.ContainsKey(u)) d[u] = k; }
            return d;
        }
    }
}
