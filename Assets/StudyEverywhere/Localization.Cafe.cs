using System.Collections.Generic;

namespace StudyEverywhere
{
    // The cafés' words: the weather row and the pets' introduction.
    public static partial class Localization
    {
        static partial void AddCafe(Dictionary<string, string[]> t)
        {
            void A(string en, string ko, string ja, string es, string fr) => t[en] = new[] { ko, ja, es, fr };

            A("Sunny", "화창함", "晴れ", "Soleado", "Ensoleillé");
            A("Rain", "비", "雨", "Lluvia", "Pluie");
            // every café pet's introduction as you come in (CatBubble.IntroLine; {0} is its name)
            A("My name is {0}. Call my name to talk with me. You can turn me off in the wrist menu.",
              "나는 {0}! 내 이름을 부르면 이야기할 수 있어요. 손목 메뉴에서 나를 끌 수도 있어요.",   // (no 예요/이에요 to agree with the name's last sound)
              "ぼくの名前は{0}。名前を呼んでくれたらお話しできるよ。手首のメニューでオフにもできるよ。",
              "Me llamo {0}. Di mi nombre para hablar conmigo. También puedes apagarme en el menú de la muñeca.",
              "Je m'appelle {0}. Dis mon nom pour me parler. Tu peux aussi m'éteindre dans le menu du poignet.");
        }
    }
}
