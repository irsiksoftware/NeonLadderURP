using UnityEngine;
using System.Collections.Generic;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Sample boss banter content for creating Dialogue Database
    /// This provides the foundation for multi-language banter lines
    /// </summary>
    public static class BossBanterContent
    {
        /// <summary>
        /// Sample banter lines for each boss in multiple languages
        /// Use this as reference for creating Pixel Crushers Dialogue Database
        /// </summary>
        public static readonly Dictionary<string, BossDialogueSet> BossBanterLines = new Dictionary<string, BossDialogueSet>
        {
            ["Wrath"] = new BossDialogueSet
            {
                bossName = "Wrath",
                conversationName = "Wrath_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("Your anger feeds my strength!", "你的愤怒滋养着我的力量！", "تمہارا غصہ میری طاقت کو بڑھاتا ہے!"),
                    new MultiLanguageLine("Still standing? How... irritating.", "还站着？真是...恼人。", "ابھی بھی کھڑے ہو؟ کتنا... پریشان کن۔"),
                    new MultiLanguageLine("Your defiance only stokes my fury!", "你的反抗只会激起我的怒火！", "تمہاری بغاوت صرف میرے غضب کو بھڑکاتی ہے!"),
                    new MultiLanguageLine("You think you can contain the storm?", "你认为你能遏制风暴吗？", "تم سمجھتے ہو کہ تم طوفان کو قابو کر سکتے ہو؟"),
                    new MultiLanguageLine("I am the rage that burns eternal!", "我是永恒燃烧的愤怒！", "میں وہ غضب ہوں جو ہمیشہ جلتا رہتا ہے!")
                }
            },

            ["Envy"] = new BossDialogueSet
            {
                bossName = "Envy",
                conversationName = "Envy_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("What I wouldn't give for your... position.", "我愿意为了你的...地位付出什么。", "تمہاری... حیثیت کے لیے میں کیا نہیں دوں گا۔"),
                    new MultiLanguageLine("You have something that should be mine.", "你拥有本应属于我的东西。", "تمہارے پاس کچھ ایسا ہے جو میرا ہونا چاہیے تھا۔"),
                    new MultiLanguageLine("Such power... it should belong to me!", "这样的力量...它应该属于我！", "ایسی طاقت... یہ میری ہونی چاہیے تھی!"),
                    new MultiLanguageLine("Why do you get to be the hero?", "为什么你可以成为英雄？", "تم کیوں ہیرو بن سکتے ہو؟"),
                    new MultiLanguageLine("I covet what you possess most.", "我最垂涎你所拥有的。", "جو تمہارے پاس سب سے زیادہ ہے اس کی مجھے لالچ ہے۔")
                }
            },

            ["Greed"] = new BossDialogueSet
            {
                bossName = "Greed",
                conversationName = "Greed_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("Your soul would fetch a fine price...", "你的灵魂会卖个好价钱...", "ਤੁਹਾਡੀ ਰੂਹ ਚੰਗੀ ਕੀਮਤ ਲਿਆਵੇਗੀ..."),
                    new MultiLanguageLine("Everything has a price. Even you.", "一切都有价格。包括你。", "ਹਰ ਚੀਜ਼ ਦੀ ਕੀਮਤ ਹੈ। ਤੁਸੀਂ ਵੀ।"),
                    new MultiLanguageLine("I'll take what's yours... and more.", "我会拿走你的...还有更多。", "ਮੈਂ ਤੁਹਾਡਾ ਲੈ ਲਵਾਂਗਾ... ਅਤੇ ਹੋਰ ਵੀ।"),
                    new MultiLanguageLine("Your power could make me wealthy beyond measure!", "你的力量可以让我富有到无法估量！", "ਤੁਹਾਡੀ ਸ਼ਕਤੀ ਮੈਨੂੰ ਬੇਅੰਤ ਅਮੀਰ ਬਣਾ ਸਕਦੀ ਹੈ!"),
                    new MultiLanguageLine("I must have it all... ALL OF IT!", "我必须拥有一切...所有的！", "ਮੇਰੇ ਕੋਲ ਸਭ ਕੁਝ ਹੋਣਾ ਚਾਹੀਦਾ ਹੈ... ਸਭ ਕੁਝ!")
                }
            },

            ["Lust"] = new BossDialogueSet
            {
                bossName = "Lust",
                conversationName = "Lust_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("Such determination... how alluring.", "如此坚定...多么诱人。", "ਅਜਿਹਾ ਦ੍ਰਿੜਤਾ... ਕਿੰਨਾ ਆਕਰਸ਼ਕ।"),
                    new MultiLanguageLine("You're more interesting than most who come here.", "你比大多数来这里的人都有趣。", "ਤੁਸੀਂ ਇੱਥੇ ਆਉਣ ਵਾਲੇ ਜ਼ਿਆਦਾਤਰ ਲੋਕਾਂ ਨਾਲੋਂ ਦਿਲਚਸਪ ਹੋ।"),
                    new MultiLanguageLine("Stay a while... we could have such fun.", "留一会儿...我们可以玩得很开心。", "ਥੋੜੀ ਦੇਰ ਰੁਕੋ... ਅਸੀਂ ਬਹੁਤ ਮਜ਼ਾ ਕਰ ਸਕਦੇ ਹਾਂ।"),
                    new MultiLanguageLine("Your resistance is... intoxicating.", "你的抵抗是...令人陶醉的。", "ਤੁਹਾਡਾ ਵਿਰੋਧ... ਨਸ਼ਾ ਹੈ।"),
                    new MultiLanguageLine("I desire what cannot be possessed.", "我渴望无法拥有的东西。", "ਮੈਂ ਉਸ ਚੀਜ਼ ਦੀ ਇੱਛਾ ਰੱਖਦਾ ਹਾਂ ਜੋ ਮਿਲ ਨਹੀਂ ਸਕਦੀ।")
                }
            },

            ["Gluttony"] = new BossDialogueSet
            {
                bossName = "Gluttony",
                conversationName = "Gluttony_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("I hunger for more than just flesh...", "我渴望的不仅仅是血肉...", "ਮੈਨੂੰ ਸਿਰਫ਼ ਮਾਸ ਤੋਂ ਵੱਧ ਦੀ ਭੁੱਖ ਹੈ..."),
                    new MultiLanguageLine("Your essence looks... delicious.", "你的精华看起来...美味。", "ਤੁਹਾਡਾ ਤੱਤ... ਸੁਆਦਿਸ਼ਟ ਲੱਗ ਰਿਹਾ ਹੈ।"),
                    new MultiLanguageLine("I must devour everything in my path!", "我必须吞噬路上的一切！", "ਮੈਨੂੰ ਆਪਣੇ ਰਸਤੇ ਦੀ ਹਰ ਚੀਜ਼ ਨੂੰ ਨਿਗਲਣਾ ਚਾਹੀਦਾ ਹੈ!"),
                    new MultiLanguageLine("Never enough... there's never enough!", "永远不够...永远不够！", "ਕਦੇ ਕਾਫੀ ਨਹੀਂ... ਕਦੇ ਕਾਫੀ ਨਹੀਂ!"),
                    new MultiLanguageLine("Such power would be a feast to remember!", "这样的力量将是一顿难忘的盛宴！", "ਅਜਿਹੀ ਸ਼ਕਤੀ ਯਾਦਗਾਰ ਦਾਵਤ ਹੋਵੇਗੀ!")
                }
            },

            ["Sloth"] = new BossDialogueSet
            {
                bossName = "Sloth",
                conversationName = "Sloth_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("Why... do you... keep... trying?", "为什么...你...一直...尝试？", "ਕਿਉਂ... ਤੁਸੀਂ... ਕੋਸ਼ਿਸ਼... ਕਰਦੇ ਰਹਿੰਦੇ ਹੋ?"),
                    new MultiLanguageLine("So much... effort... for nothing...", "这么多...努力...毫无意义...", "ਇੰਨੀ... ਮਿਹਨਤ... ਬੇਕਾਰ..."),
                    new MultiLanguageLine("Rest... why don't you... just rest?", "休息...你为什么不...就休息呢？", "ਆਰਾਮ... ਤੁਸੀਂ ਕਿਉਂ ਨਹੀਂ... ਬਸ ਆਰਾਮ ਕਰਦੇ?"),
                    new MultiLanguageLine("All this... movement... so... tiresome...", "所有这些...动作...如此...累人...", "ਇਹ ਸਭ... ਹਿਲਜੁਲ... ਬਹੁਤ... ਥਕਾਉਣੇ..."),
                    new MultiLanguageLine("I can't... be bothered... to fight properly...", "我不能...被打扰...去正常战斗...", "ਮੈਂ... ਸਹੀ ਤਰੀਕੇ ਨਾਲ... ਲੜਨ ਦੀ ਪਰਵਾਹ ਨਹੀਂ ਕਰ ਸਕਦਾ...")
                }
            },

            ["Pride"] = new BossDialogueSet
            {
                bossName = "Pride",
                conversationName = "Pride_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("You dare challenge perfection itself?", "你敢挑战完美本身？", "ਤੁਸੀਂ ਸੰਪੂਰਨਤਾ ਨੂੰ ਚੁਣੌਤੀ ਦੇਣ ਦੀ ਹਿੰਮਤ ਕਰਦੇ ਹੋ?"),
                    new MultiLanguageLine("I am flawless. You are... not.", "我是完美的。你...不是。", "ਮੈਂ ਨਿਰਦੋਸ਼ ਹਾਂ। ਤੁਸੀਂ... ਨਹੀਂ।"),
                    new MultiLanguageLine("Your very existence offends my superiority!", "你的存在本身就冒犯了我的优越性！", "ਤੁਹਾਡੀ ਹੋਂਦ ਹੀ ਮੇਰੀ ਉੱਤਮਤਾ ਦਾ ਅਪਮਾਨ ਹੈ!"),
                    new MultiLanguageLine("How amusing that you think you could win...", "你认为你能赢，多么有趣...", "ਇਹ ਕਿੰਨਾ ਮਜ਼ਾਕੀਆ ਹੈ ਕਿ ਤੁਸੀਂ ਸੋਚਦੇ ਹੋ ਕਿ ਤੁਸੀਂ ਜਿੱਤ ਸਕਦੇ ਹੋ..."),
                    new MultiLanguageLine("I am the pinnacle of all creation!", "我是所有创造的巅峰！", "ਮੈਂ ਸਾਰੀ ਸ੍ਰਿਸ਼ਟੀ ਦਾ ਸਿਖਰ ਹਾਂ!")
                }
            },

            ["Devil"] = new BossDialogueSet
            {
                bossName = "Devil",
                conversationName = "Devil_Banter",
                banterLines = new MultiLanguageLine[]
                {
                    new MultiLanguageLine("So, you've come to face me at last.", "所以，你终于来面对我了。", "ਇਸ ਲਈ, ਤੁਸੀਂ ਅਖੀਰ ਮੇਰਾ ਸਾਮਣਾ ਕਰਨ ਆਏ ਹੋ।"),
                    new MultiLanguageLine("My children spoke highly of your... persistence.", "我的孩子们高度评价你的...坚持。", "ਮੇਰੇ ਬੱਚਿਆਂ ਨੇ ਤੁਹਾਡੀ... ਦ੍ਰਿੜਤਾ ਦੀ ਬਹੁਤ ਪ੍ਰਸ਼ੰਸਾ ਕੀਤੀ।"),
                    new MultiLanguageLine("You've destroyed everything I built... impressive.", "你摧毁了我建造的一切...令人印象深刻。", "ਤੁਸੀਂ ਮੇਰੀ ਬਣਾਈ ਹਰ ਚੀਜ਼ ਨੂੰ ਨਸ਼ਟ ਕਰ ਦਿੱਤਾ... ਪ੍ਰਭਾਵਸ਼ਾਲੀ।"),
                    new MultiLanguageLine("But can you face the father of all sins?", "但你能面对所有罪恶之父吗？", "ਪਰ ਕੀ ਤੁਸੀਂ ਸਾਰੇ ਪਾਪਾਂ ਦੇ ਪਿਤਾ ਦਾ ਸਾਮਣਾ ਕਰ ਸਕਦੇ ਹੋ?"),
                    new MultiLanguageLine("This realm was meant to be my eternal kingdom.", "这个领域本来是我的永恒王国。", "ਇਹ ਖੇਤਰ ਮੇਰਾ ਸਦੀਵੀ ਰਾਜ ਹੋਣਾ ਸੀ।"),
                    new MultiLanguageLine("You think yourself the hero? How... quaint.", "你认为自己是英雄？多么...古雅。", "ਤੁਸੀਂ ਆਪਣੇ ਆਪ ਨੂੰ ਹੀਰੋ ਸਮਝਦੇ ਹੋ? ਕਿੰਨਾ... ਅਜੀਬ।"),
                    new MultiLanguageLine("I am inevitable. I am eternal.", "我是不可避免的。我是永恒的。", "ਮੈਂ ਅਟੱਲ ਹਾਂ। ਮੈਂ ਸਦੀਵੀ ਹਾਂ।"),
                    new MultiLanguageLine("Welcome to your final test, little mortal.", "欢迎来到你的最终考验，小凡人。", "ਆਪਣੀ ਅੰਤਿਮ ਪਰੀਖਿਆ ਵਿੱਚ ਸੁਆਗਤ ਹੈ, ਛੋਟੇ ਮਰਣਸ਼ੀਲ।")
                }
            }
        };

        [System.Serializable]
        public class BossDialogueSet
        {
            public string bossName;
            public string conversationName;
            public MultiLanguageLine[] banterLines;
        }

        [System.Serializable]
        public class MultiLanguageLine
        {
            public string english;
            public string chineseSimplified;
            public string urdu;

            public MultiLanguageLine(string en, string zhHans, string ur)
            {
                english = en;
                chineseSimplified = zhHans;
                urdu = ur;
            }

            public string GetLocalizedText(string languageCode)
            {
                return languageCode switch
                {
                    "en" => english,
                    "zh-Hans" => chineseSimplified,
                    "ur" => urdu,
                    _ => english // Default to English
                };
            }
        }
    }
}