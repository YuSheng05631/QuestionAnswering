using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace QuestionAnswering
{
    public class AnaphoraInfo
    {
        public string word;
        public int number;  //0: unknown, 1: singular, 2: plural
        public int gender;  //0: unknown, 1: male, 2: female, 3: both
        public int human;   //0: unknown, 1: true, 2: false
        public AnaphoraInfo(string word)
        {
            this.word = word;
            this.number = 0;
            this.gender = 0;
            this.human = 0;
        }
    }
    class Anaphora
    {
        //代名詞List
        private static List<string> PRPList = new List<string>() { "I", "me", "my", "you", "your",
            "he", "him", "his", "she", "her", "it", "its", "we", "us", "our", "they", "them", "their", 
            "mine", "yours", "hers", "ours", "theirs", "myself", "ourselves", "yourself", "yourselves", 
            "himself", "herself", "itself", "themselves"};

        //藉由babynames.net取得姓名資訊
        private static int getNameInfo(string name)
        {
            //0: 找不到該名字
            //1: 男性
            //2: 女性
            //3: 中性
            //該網站搜尋空白字元會發生錯誤，因此將詞組分開各自搜尋
            string[] nameAry = name.Split(' ');
            bool is1 = false, is2 = false, is3 = false;
            for (int i = 0; i < nameAry.Length; i++)
            {
                string allWebData = getBabynamesData(nameAry[i]);   //取得babynames.net網頁原始碼
                int gender = getBabynamesGender(allWebData);        //由babynames.net原始碼取得名字性別
                if (gender == 1) is1 = true;
                else if (gender == 2) is2 = true;
                else if (gender == 3) is3 = true;
            }
            //許多個單詞可能找出不同結果，需確定整個詞組的性別
            if (is1 && is2) return 3;   //有男性、女性，回傳中性
            else if (is1) return 1;     //有男性沒女性，回傳男性
            else if (is2) return 2;     //沒男性有女性，回傳女性
            else if (is3) return 3;     //只有中性，回傳中性
            else return 0;              //找不到
        }
        //取得babynames.net網頁原始碼
        private static string getBabynamesData(string name)
        {
            string url = "http://babynames.net/names/" + name.ToLower();
            string allWebData = "";
            WebClient client = new WebClient();
            try
            {
                using (Stream data = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                    {
                        allWebData = reader.ReadToEnd();
                    }
                }
            }
            catch(WebException)
            {
                allWebData = "";
            }
            return allWebData;
        }
        //由babynames.net原始碼取得名字性別
        private static int getBabynamesGender(string allWebData)
        {
            //0: 找不到該名字
            //1: 男性
            //2: 女性
            //3: 中性
            int first = allWebData.IndexOf("result-gender");
            if (first == -1) return 0;  //找不到性別，也就是找不到該名字
            else
            {
                first = allWebData.IndexOf(" ", first) + 1;
                int last = allWebData.IndexOf("\"", first);
                string cut = allWebData.Substring(first, last - first);
                if (cut == "boy") return 1;
                else if (cut == "girl") return 2;
                else if (cut == "boygirl") return 3;
            }
            return 0;
        }

        //藉由dictionary.com取得名詞資訊
        private static AnaphoraInfo getNounInfo(string noun)
        {
            string allWebData = getDictionaryData(noun);            //取得dictionary.com網頁原始碼
            List<string> defList = getDictionaryDef(allWebData);    //由dictionary.com原始碼取得名詞解釋(前三個)
            AnaphoraInfo ai = new AnaphoraInfo("");
            ai.gender = getDictionaryGender(defList);               //由dictionary.com名詞解釋取得名詞性別
            ai.human = getDictionaryHuman(defList);                 //由dictionary.com名詞解釋取得名詞是人or not
            return ai;
        }
        //取得dictionary.com網頁原始碼
        private static string getDictionaryData(string noun)
        {
            string url = "http://www.dictionary.com/browse/" + noun.ToLower();
            string allWebData = "";
            WebClient client = new WebClient();
            try
            {
                using (Stream data = client.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                    {
                        allWebData = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                allWebData = "";
            }
            return allWebData;
        }
        //由dictionary.com原始碼取得名詞解釋(前一個)
        private static List<string> getDictionaryDef(string allWebData)
        {
            List<string> defList = new List<string>();
            int first = 0, last = 0, ct = 0;
            first = allWebData.IndexOf("def-content");
            while (first != -1 && ct < 1)
            {
                first = allWebData.IndexOf(" ", first);
                last = allWebData.IndexOf("</div>", first);
                string cut = allWebData.Substring(first, last - first);
                cut = removeLabel(cut); //去掉<>標籤的內容
                defList.Add(cut.Trim());
                first = allWebData.IndexOf("def-content", last);
                ct++;
            }
            return defList;
        }
        //去掉<>標籤的內容
        private static string removeLabel(string str)
        {
            int first = 0, last = 0;
            first = str.IndexOf("<");
            while (first != -1)
            {
                last = str.IndexOf(">") + 1;
                str = str.Substring(0, first) + str.Substring(last);
                first = str.IndexOf("<");
            }
            return str;
        }
        //由dictionary.com名詞解釋取得名詞性別
        private static int getDictionaryGender(List<string> defList)
        {
            foreach (string def in defList)
            {
                if (def.IndexOf(" woman ") != -1 || def.IndexOf(" female ") != -1) return 2;
                else if (def.IndexOf(" man ") != -1 || def.IndexOf(" male ") != -1) return 1;
            }
            return 0;
        }
        //由dictionary.com名詞解釋取得名詞是人or not
        private static int getDictionaryHuman(List<string> defList)
        {
            foreach (string def in defList)
            {
                if (def.IndexOf(" man ") != -1 || def.IndexOf(" woman ") != -1 || 
                    def.IndexOf(" person ") != -1 || def.IndexOf(" people ") != -1) return 1;
            }
            return 2;
        }

        //取得代名詞的AnaphoraInfo
        private static AnaphoraInfo getPRPAnaphoraInfo(string PRP)
        {
            AnaphoraInfo ai = new AnaphoraInfo(PRP);
            PRP = PRP.ToLower();
            if (PRP == "i" || PRP == "me" || PRP == "my" || PRP == "mine" || PRP == "myself" || PRP == "yourself")
            {
                ai.number = 1;  //singular
                ai.gender = 0;  //unknown
                ai.human = 1;   //true
            }
            else if (PRP == "you" || PRP == "your" || PRP == "yours")
            {
                ai.number = 0;  //unknown
                ai.gender = 0;  //unknown
                ai.human = 1;   //true
            }
            else if (PRP == "he" || PRP == "him" || PRP == "his" || PRP == "himself")
            {
                ai.number = 1;  //singular
                ai.gender = 1;  //male
                ai.human = 0;   //unknown
            }
            else if (PRP == "she" || PRP == "her" || PRP == "hers" || PRP == "herself")
            {
                ai.number = 1;  //singular
                ai.gender = 2;  //female
                ai.human = 0;   //unknown
            }
            else if (PRP == "it" || PRP == "its" || PRP == "itself")
            {
                ai.number = 1;  //singular
                ai.gender = 0;  //unknown
                ai.human = 2;   //false
            }
            else if (PRP == "we" || PRP == "us" || PRP == "our" || PRP == "ours" || PRP == "ourselves" || PRP == "yourselves")
            {
                ai.number = 2;  //plural
                ai.gender = 0;  //unknown
                ai.human = 1;   //true
            }
            else if (PRP == "they" || PRP == "them" || PRP == "their" || PRP == "theirs" || PRP == "themselves")
            {
                ai.number = 2;  //plural
                ai.gender = 0;  //unknown
                ai.human = 0;   //unknown
            }
            else Console.WriteLine(PRP + " 不是PRP。");
            return ai;
        }
        //取得專有名詞的AnaphoraInfo
        private static AnaphoraInfo getNNPAnaphoraInfo(string NNP)
        {
            AnaphoraInfo ai = new AnaphoraInfo(NNP);
            int gender = getNameInfo(NNP);  //藉由babynames.net取得姓名資訊
            ai.gender = gender;
            if (gender == 0 && NNP[NNP.Length - 1] == 's') ai.number = 2;   //plural
            else ai.number = 1;             //singular
            if (gender == 0) ai.human = 2;  //false
            else ai.human = 1;              //true
            return ai;
        }
        //取得普通名詞的AnaphoraInfo
        private static AnaphoraInfo getNPAnaphoraInfo(string NP, bool hasNNS)
        {
            AnaphoraInfo ai = new AnaphoraInfo(NP);
            AnaphoraInfo aiGH = getNounInfo(NP);    //藉由dictionary.com取得名詞資訊
            if (hasNNS) ai.number = 2;  //plural
            else ai.number = 1;         //singular
            ai.gender = aiGH.gender;
            ai.human = aiGH.human;
            return ai;
        }
        //檢查兩個AnaphoraInfo是否相同
        private static bool isSameAnaphoraInfo(AnaphoraInfo ai1, AnaphoraInfo ai2)
        {
            if (ai1 == null || ai2 == null) return false;
            if (ai1.word == "" || ai2.word == "") return false;
            if (ai1.number != 0 && ai2.number != 0 && ai1.number != ai2.number) return false;
            if (ai1.gender != 0 && ai2.gender != 0 && ai1.gender != ai2.gender) return false;
            if (ai1.human != 0 && ai2.human != 0 && ai1.human != ai2.human) return false;
            return true;
        }
        //印出AnaphoraInfo
        private static void printAnaphoraInfo(AnaphoraInfo ai)
        {
            string number = "", gender = "", human = "";

            if (ai.number == 0) number = "unknown";
            else if (ai.number == 1) number = "singular";
            else if (ai.number == 2) number = "plural";

            if (ai.gender == 0) gender = "unknown";
            else if (ai.gender == 1) gender = "male";
            else if (ai.gender == 2) gender = "female";

            if (ai.human == 0) human = "unknown";
            else if (ai.human == 1) human = "true";
            else if (ai.human == 2) human = "false";

            Console.WriteLine("word: {0}, number: {1}, gender: {2}, human: {3}",
                ai.word, number, gender, human);
        }

        //檢查PL裡是否有專有名詞
        private static bool hasNNPFromPL(PL pl)
        {
            for (int i = 0; i < pl.words.Count; i++)
                if (pl.words[i].pos.IndexOf("NNP") == 0)
                    return true;
            return false;
        }
        //檢查是否有複數名詞
        private static bool hasNNSFromPL(PL pl)
        {
            for (int i = 0; i < pl.words.Count; i++)
                if (pl.words[i].pos.IndexOf("NNS") == 0)
                    return true;
            return false;
        }
        //將PL裡的名詞擷取出來
        private static string getNounsFromPL(PL pl)
        {
            string nouns = "";
            for (int i = 0; i < pl.words.Count; i++)
            {
                if (pl.words[i].pos.IndexOf("N") == 0) //開頭是N
                {
                    nouns += pl.words[i].word + " ";
                }
            }
            return nouns.Trim();
        }

        //轉換Anaphora
        //rootList: 文章中每個句子的root
        public static void transformAnaphora(List<ROOT> rootList)
        {
            for (int i = 0; i < rootList.Count; i++)
            {
                findAnaphoraTraversal(rootList, i, rootList[i].S);  //找出Anaphora(Traversal)
            }
        }
        //找出Anaphora(Traversal)
        //rootList: 文章中每個句子的root
        //rootListIndex: 目前處理到第幾個句子
        private static void findAnaphoraTraversal(List<ROOT> rootList, int rootListIndex, S s)
        {
            if (s.WH != null)
                foreach (PL pl in s.WH)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.SQ != null)
                foreach (PL pl in s.SQ)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    for (int i = 0; i < pl.words.Count; i++)
                    {
                        if (pl.words[i].pos.IndexOf("PRP") == 0)    //代名詞
                        {
                            AnaphoraInfo ai = getPRPAnaphoraInfo(pl.words[i].word);                     //取得代名詞的AnaphoraInfo
                            PL AntecedentPL = findAntecedent(rootList, rootListIndex, ai, s.indent);    //找出Antecedent(Start)
                            if (AntecedentPL != null)   //找到Antecedent
                            {
                                pl.words = transformAnaphoraWAPList(pl.words, AntecedentPL.words);      //將Anaphora取代成Antecedent
                                break;
                            }
                        }
                    }
                    if (pl.next != null) findAnaphoraTraversal(rootList, rootListIndex, pl.next);
                }
            }
            if (s.VP != null)
                foreach (PL pl in s.VP)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.PP != null)
                foreach (PL pl in s.PP)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.ADJP != null)
                foreach (PL pl in s.ADJP)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.ADVP != null)
                foreach (PL pl in s.ADVP)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
            if (s.Ss != null)
                foreach (PL pl in s.Ss)
                    if (pl.next != null)
                        findAnaphoraTraversal(rootList, rootListIndex, pl.next);
        }
        //找出Antecedent(Start)
        //rootList: 文章中每個句子的root
        //rootListIndex: 目前處理到第幾個句子
        //ai: 要被取代的Anaphora資訊
        //indent: 受理的縮排最大值，若等於-1則沒有最大值
        private static PL findAntecedent(List<ROOT> rootList, int rootListIndex, AnaphoraInfo ai, int indent)
        {
            PL AntecedentPL = null;
            //向前找
            for (int i = rootListIndex; i >= 0; i--)
            {
                if (i != rootListIndex) indent = -1;            //第一句找indent縮排以內的名詞，其他句則無受限
                AntecedentPL = findAntecedentTraversal(ai, rootList[i].S, indent);
                if (AntecedentPL != null) return AntecedentPL;  //找到AntecedentPL
            }
            //向後找
            /*for (int i = rootListIndex + 1; i < rootList.Count; i++)
            {
                AntecedentPL = findAntecedentTraversal(ai, rootList[i].S);
                if (AntecedentPL != null) return AntecedentPL;  //找到AntecedentPL
            }*/
            return AntecedentPL;
        }
        //找出Antecedent(Traversal)
        //ai: 要被取代的Anaphora資訊
        //indent: 受理的縮排最大值，若等於-1則沒有最大值
        private static PL findAntecedentTraversal(AnaphoraInfo ai, S s, int indent)
        {
            if (indent != -1 && indent < s.indent) return null;
            PL AntecedentPL = null;
            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    if (AntecedentPL == null && evaluateAntecedent(ai, pl)) //評估Antecedent
                    {
                        AntecedentPL =  pl;
                        return AntecedentPL;
                    }
                    if (pl.next != null) AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    if (pl.next != null && AntecedentPL == null)
                    {
                        AntecedentPL = findAntecedentTraversal(ai, pl.next, indent);
                        if (AntecedentPL != null) return AntecedentPL;
                    }
                }
            }
            return AntecedentPL;
        }
        //評估Antecedent
        //ai: 要被取代的Anaphora資訊
        //pl: 要被評估的Antecedent
        private static bool evaluateAntecedent(AnaphoraInfo ai, PL pl)
        {
            bool hasNNP = hasNNPFromPL(pl);     //檢查PL裡是否有專有名詞
            bool hasNNS = hasNNSFromPL(pl);     //檢查是否有複數名詞
            string nouns = getNounsFromPL(pl);  //將PL裡的名詞擷取出來
            if (hasNNP)  //是專有名詞
            {
                AnaphoraInfo aiNNP = getNNPAnaphoraInfo(nouns); //取得專有名詞的AnaphoraInfo
                return isSameAnaphoraInfo(ai, aiNNP);           //檢查兩個AnaphoraInfo是否相同
            }
            else        //不是專有名詞
            {
                AnaphoraInfo aiNP = getNPAnaphoraInfo(nouns, hasNNS);   //取得普通名詞的AnaphoraInfo
                return isSameAnaphoraInfo(ai, aiNP);                    //檢查兩個AnaphoraInfo是否相同
            }
        }
        //將Anaphora取代成Antecedent
        private static List<WordAndPOS> transformAnaphoraWAPList(List<WordAndPOS> AnaphoraWAPList, List<WordAndPOS> AntecedentWAPList)
        {
            List<WordAndPOS> complexWAPList = new List<WordAndPOS>();
            bool hasTransformed = false;
            for (int i = 0; i < AnaphoraWAPList.Count; i++)
            {
                if (AnaphoraWAPList[i].pos == "PRP$" && !hasTransformed)    //所有格
                {
                    complexWAPList.AddRange(AntecedentWAPList);
                    complexWAPList.Add(new WordAndPOS("'s", "POS"));
                    hasTransformed = true;
                }
                else if (AnaphoraWAPList[i].pos == "PRP" && !hasTransformed)
                {
                    complexWAPList.AddRange(AntecedentWAPList);
                    hasTransformed = true;
                }
                else complexWAPList.Add(AnaphoraWAPList[i]);
            }
            printWAPList(AnaphoraWAPList, AntecedentWAPList);   //印出AnaphoraWAPList和AntecedentWAPList
            return complexWAPList;
        }
        //印出AnaphoraWAPList和AntecedentWAPList
        private static void printWAPList(List<WordAndPOS> AnaphoraWAPList, List<WordAndPOS> AntecedentWAPList)
        {
            Console.Write("將 ");
            foreach (WordAndPOS wap in AnaphoraWAPList) Console.Write(wap.word + " ");
            Console.Write("轉換為 ");
            foreach (WordAndPOS wap in AntecedentWAPList) Console.Write(wap.word + " ");
            Console.WriteLine("。");
        }
    }
}
