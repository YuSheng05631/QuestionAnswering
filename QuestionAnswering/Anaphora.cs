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
        public string pos;
        public int number;  //0: unknown, 1: singular, 2: plural
        public int gender;  //0: unknown, 1: male, 2: female, 3: both
        public int human;   //0: unknown, 1: true, 2: false
        public AnaphoraInfo(string word)
        {
            this.word = word;
            this.pos = "";
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

        //babynames.net

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

        //dictionary.com

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

        //get AnaphoraInfo

        //取得代名詞的AnaphoraInfo
        private static AnaphoraInfo getPRPAnaphoraInfo(string PRP)
        {
            AnaphoraInfo ai = new AnaphoraInfo(PRP);
            ai.pos = "PRP";
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
                ai.human = 1;   //true
            }
            else if (PRP == "she" || PRP == "her" || PRP == "hers" || PRP == "herself")
            {
                ai.number = 1;  //singular
                ai.gender = 2;  //female
                ai.human = 1;   //true
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
        public static AnaphoraInfo getNNPAnaphoraInfo(string NNP)
        {
            AnaphoraInfo ai = SaveData.loadAnaphoraInfo(NNP);       //載入AnaphoraInfo
            if (ai != null) return ai;

            string wikiTitle;
            ai = new AnaphoraInfo(NNP);
            Infobox infobox = Wiki.getInfobox(NNP, out wikiTitle);  //取得wiki的Infobox
            int nameGender = getNameInfo(NNP);                      //藉由babynames.net取得姓名資訊

            ai.pos = "NNP";
            ai.gender = nameGender;
            if (infobox != null && infobox.isHuman())
            {
                ai.human = 1;       //true
                if (infobox.isSingular() || wikiTitle.IndexOf(NNP) != -1) ai.number = 1;    //singular
                else ai.number = 2; //plural
            }
            else
            {
                if (wikiTitle.IndexOf(NNP) != -1)   //e.g. Tigris可以找到wiki頁面，代表是單數
                {
                    ai.number = 1;  //singular
                }
                else if (nameGender == 0 && NNP[NNP.Length - 1] == 's') //找不到名字，結尾又是s
                {
                    ai.number = 2;  //plural
                    //去掉s再重新搜尋
                    NNP = NNP.Substring(0, NNP.Length - 1);
                    nameGender = getNameInfo(NNP);  //藉由babynames.net取得姓名資訊
                    if (nameGender != 0)
                    {
                        ai.gender = nameGender;
                        ai.human = 1;   //true
                    }
                }
                else ai.number = 1; //singular
                if (nameGender == 0) ai.human = 2;  //false
                else ai.human = 1;  //true
            }
            SaveData.saveAnaphoraInfo(ai, NNP); //儲存AnaphoraInfo
            return ai;
        }
        //取得普通名詞的AnaphoraInfo
        private static AnaphoraInfo getNPAnaphoraInfo(string NP, bool hasNNS)
        {
            AnaphoraInfo ai = SaveData.loadAnaphoraInfo(NP);    //載入AnaphoraInfo
            if (ai != null) return ai;

            ai = new AnaphoraInfo(NP);
            AnaphoraInfo aiGH = getNounInfo(NP);    //藉由dictionary.com取得名詞資訊
            ai.pos = "NP";
            if (hasNNS) ai.number = 2;  //plural
            else ai.number = 1;         //singular
            ai.gender = aiGH.gender;
            ai.human = aiGH.human;

            SaveData.saveAnaphoraInfo(ai, NP); //儲存AnaphoraInfo
            return ai;
        }
        //檢查兩個AnaphoraInfo是否相同
        private static bool isSameAnaphoraInfo(AnaphoraInfo ai1, AnaphoraInfo ai2)
        {
            if (ai1 == null || ai2 == null) return false;
            if (ai1.word == "" || ai2.word == "") return false;
            if (ai1.number != 0 && ai2.number != 0 && ai1.number != ai2.number) return false;
            if (ai1.gender != 0 && ai2.gender != 0 && ai1.gender != 3 && ai2.gender != 3 && 
                ai1.gender != ai2.gender) return false;
            if (ai1.human != 0 && ai2.human != 0 && ai1.human != ai2.human) return false;
            return true;
        }
        //印出AnaphoraInfo
        public static void printAnaphoraInfo(AnaphoraInfo ai)
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

        //check PL

        //檢查PL裡是否有專有名詞
        private static bool hasNNPFromPL(PL pl)
        {
            for (int i = 0; i < pl.words.Count; i++)
                if (pl.words[i].pos.IndexOf("NNP") == 0)
                    return true;
            return false;
        }
        //檢查PL裡是否有複數名詞
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

        //False Subject

        //從POSTree抓取的詞性順序。e.g. NP + VP + NP
        private static List<string> retrievePLPOSList = new List<string>(); //phrase level
        private static List<string> retrieveWLPOSList = new List<string>(); //word level
        //設置retrievePLPOSList、retrieveWLPOSList(輸入type)
        private static void setRetrievePOSList(int type)
        {
            retrievePLPOSList = new List<string>();
            retrieveWLPOSList = new List<string>();
            switch (type)
            {
                case 0: //It + ... + that + ...
                    retrievePLPOSList = splitStringByComma("NP,S");
                    retrieveWLPOSList = splitStringByComma("PRP,IN");
                    break;
                case 1: //It + ... + to + ...
                    retrievePLPOSList = splitStringByComma("NP,VP");
                    retrieveWLPOSList = splitStringByComma("PRP,TO");
                    break;
                case 2: //It + ... + V-ing + ...
                    retrievePLPOSList = splitStringByComma("NP,VP");
                    retrieveWLPOSList = splitStringByComma("PRP,VBG");
                    break;
                default:
                    break;
            }
        }
        //設置retrievePOSList(輸入字串)
        private static List<string> splitStringByComma(string type)
        {
            List<string> tempList = new List<string>();
            string[] cut = type.Split(',');
            foreach (string c in cut) tempList.Add(c);
            cut.ToArray();
            return tempList;
        }
        //檢查是否為虛主詞(Start)
        private static bool isFalseSubject(S s, string word)
        {
            if (word.ToLower() == "it")
            {
                for (int i = 0; i <= 2; i++)
                {
                    setRetrievePOSList(i);
                    if (isFalseSubjectTraversal(s, 0)) return true; //檢查是否為虛主詞(Traversal)
                }
            }
            return false;
        }
        //檢查是否為虛主詞(Traversal)
        //rIndex: 接下來要抓retrievePOSList中第幾個索引的詞性
        private static bool isFalseSubjectTraversal(S s, int rIndex)
        {
            bool isFalseSubject = false;
            if (s.NP != null)
            {
                if (retrievePLPOSList[rIndex] == "NP")    //符合PL詞性
                {
                    bool hasWLPOS = false;
                    foreach (PL pl in s.NP)
                    {
                        if (pl.words.Count == 0 && retrieveWLPOSList[rIndex] == "") hasWLPOS = true;    //e.g. (S
                        foreach (WordAndPOS wap in pl.words)
                        {
                            if (retrieveWLPOSList[rIndex] == "") hasWLPOS = true;
                            if (retrieveWLPOSList[rIndex] == wap.pos) hasWLPOS = true;  //(只要其中一個)符合WL詞性
                            if (hasWLPOS) break;
                        }
                        if (hasWLPOS) break;
                    }
                    if (hasWLPOS) rIndex += 1;  //符合WL詞性，準備檢查下一個詞性
                    if (retrievePLPOSList.Count == rIndex) return true; //已經全數符合
                }
                foreach (PL pl in s.NP)
                {
                    if (pl.next != null)
                        isFalseSubject = isFalseSubject || isFalseSubjectTraversal(pl.next, rIndex);
                }
            }
            if (s.VP != null)
            {
                if (retrievePLPOSList[rIndex] == "VP")    //符合PL詞性
                {
                    bool hasWLPOS = false;
                    foreach (PL pl in s.VP)
                    {
                        if (pl.words.Count == 0 && retrieveWLPOSList[rIndex] == "") hasWLPOS = true;    //e.g. (S
                        foreach (WordAndPOS wap in pl.words)
                        {
                            if (retrieveWLPOSList[rIndex] == "") hasWLPOS = true;
                            if (retrieveWLPOSList[rIndex] == wap.pos) hasWLPOS = true;  //(只要其中一個)符合WL詞性
                            if (hasWLPOS) break;
                        }
                        if (hasWLPOS) break;
                    }
                    if (hasWLPOS) rIndex += 1;  //符合WL詞性，準備檢查下一個詞性
                    if (retrievePLPOSList.Count == rIndex) return true; //已經全數符合
                }
                foreach (PL pl in s.VP)
                {
                    if (pl.next != null)
                        isFalseSubject = isFalseSubject || isFalseSubjectTraversal(pl.next, rIndex);
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    if (pl.next != null)
                        isFalseSubject = isFalseSubject || isFalseSubjectTraversal(pl.next, rIndex);
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    if (pl.next != null)
                        isFalseSubject = isFalseSubject || isFalseSubjectTraversal(pl.next, rIndex);
                }
            }
            if (s.Ss != null)
            {
                if (retrievePLPOSList[rIndex].IndexOf("S") == 0)    //符合PL詞性
                {
                    bool hasWLPOS = false;
                    foreach (PL pl in s.Ss)
                    {
                        if (pl.words.Count == 0 && retrieveWLPOSList[rIndex] == "") hasWLPOS = true;    //e.g. (S
                        foreach (WordAndPOS wap in pl.words)
                        {
                            if (retrieveWLPOSList[rIndex] == "") hasWLPOS = true;
                            if (retrieveWLPOSList[rIndex] == wap.pos) hasWLPOS = true;  //(只要其中一個)符合WL詞性
                            if (hasWLPOS) break;
                        }
                        if (hasWLPOS) break;
                    }
                    if (hasWLPOS) rIndex += 1;  //符合WL詞性，準備檢查下一個詞性
                    if (retrievePLPOSList.Count == rIndex) return true; //已經全數符合
                }
                foreach (PL pl in s.Ss)
                {
                    if (pl.next != null)
                        isFalseSubject = isFalseSubject || isFalseSubjectTraversal(pl.next, rIndex);
                }
            }
            return isFalseSubject;
        }

        //Reflexive Pronoun

        //檢查是否為強調意義的反身代名詞(更改為只要是反身代名詞就不處理Anaphora)
        private static bool isReflexivePronoun(S s, string word)
        {
            //檢查是否為反身代名詞(利用PRPList)
            bool isRP = false;
            for (int i = 23; i <= 30; i++) if (word.ToLower() == PRPList[i]) isRP = true;
            if (!isRP) return false;
            return true;

            //尋找反身代名詞位在哪個NP & 哪個word
            /*int posP = 0, posW = 0;
            for (int i = 0; i < s.NP.Count; i++)
                for (int j = 0; j < s.NP[i].words.Count; j++)
                    if (s.NP[i].words[j].word == word)
                    {
                        posP = i;
                        posW = j;
                    }*/
            
            /*if (s.NP.Count > 1)                 //有其他的NP
            {
                //s.NP.RemoveAt(posP);            //移除掉反身代名詞的NP
                return true;
            }
            if (s.NP[0].words.Count > 1)        //有其他的word
            {
                //s.NP[0].words.RemoveAt(posW);   //移除掉反身代名詞的word
                return true;
            }*/
            //return false;
        }

        //Transform Anaphora

        //轉換Anaphora
        //rootList: 文章中每個句子的root
        public static void transformAnaphora(List<ROOT> rootList)
        {
            for (int i = 0; i < rootList.Count; i++)
            {
                findAnaphoraTraversal(rootList, i, rootList[i].S);  //找出Anaphora(Traversal)
            }
        }
        //轉換Anaphora(Traversal)
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
                        if (pl.words[i].pos.IndexOf("PRP") == 0)        //代名詞
                        {
                            if (isFalseSubject(s, pl.words[i].word))    //檢查是否為虛主詞
                            {
                                Console.WriteLine("擁有虛主詞 " + pl.words[i].word + " 。");
                            }
                            else if (isReflexivePronoun(s, pl.words[i].word))   //檢查是否為強調意義的反身代名詞
                            {
                                Console.WriteLine("擁有強調意義的反身代名詞 " + pl.words[i].word + " 。");
                            }
                            else    //不是虛主詞或反身代名詞，轉換Anaphora
                            {
                                AnaphoraInfo ai = getPRPAnaphoraInfo(pl.words[i].word);             //取得代名詞的AnaphoraInfo
                                PL antecedentPL = findAntecedent(rootList, rootListIndex, ai, pl);  //找出Antecedent(Start)
                                if (antecedentPL != null)   //找到Antecedent
                                {
                                    pl.words = transformPRPAnaphoraWAPList(pl.words, antecedentPL.words);   //將代名詞的Anaphora取代成Antecedent
                                    break;
                                }
                            }
                        }
                        else if (i == 0 && /*pl.words[i].pos == "DT" && 
                            pl.words[i].word.ToLower().IndexOf("th") == 0 &&*/ !hasNNPFromPL(pl)) //開頭是冠詞the、that等的普通名詞
                        {
                            bool hasNNS = hasNNSFromPL(pl);     //檢查是否有複數名詞
                            string nouns = getNounsFromPL(pl);  //將PL裡的名詞擷取出來
                            if (nouns == "") continue;
                            AnaphoraInfo ai = getNPAnaphoraInfo(nouns, hasNNS);                 //取得普通名詞的AnaphoraInfo
                            PL antecedentPL = findAntecedent(rootList, rootListIndex, ai, pl);  //找出普通名詞的Antecedent(Start)
                            if (antecedentPL != null)   //找到Antecedent
                            {
                                pl.words = transformNPAnaphoraWAPList(pl.words, antecedentPL.words);    //將普通名詞的Anaphora取代成Antecedent
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

        //Find Antecedent

        //找出Antecedent(Start)
        //rootList: 文章中每個句子的root
        //rootListIndex: 目前處理到第幾個句子
        //ai: 要被取代的Anaphora資訊
        //anaphoraPL: 要被取代的Anaphora所位於的PL，用於確認Antecedent在Anaphora之前
        private static PL findAntecedent(List<ROOT> rootList, int rootListIndex, AnaphoraInfo ai, PL anaphoraPL)
        {
            PL antecedentPL = null;
            int ct = 0;
            //向前找
            for (int i = rootListIndex; i >= 0; i--)
            {
                ct += 1;
                if (ct > 3) break;  //範圍至前三句
                bool stop = false;
                antecedentPL = findAntecedentTraversal(ai, rootList[i].S, anaphoraPL, out stop);
                if (antecedentPL != null) return antecedentPL;  //找到antecedentPL
            }
            return antecedentPL;
        }
        //找出Antecedent(Traversal)
        //ai: 要被取代的Anaphora資訊
        //anaphoraPL: 要被取代的Anaphora所在的PL，用於確認Antecedent在Anaphora之前
        private static PL findAntecedentTraversal(AnaphoraInfo ai, S s, PL anaphoraPL, out bool stop)
        {
            stop = false;
            PL antecedentPL = null;
            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    if (pl == anaphoraPL) stop = true;  //已經檢查到Anaphora所在的PL
                    else stop = false;
                    if (antecedentPL == null && !stop && pl.words.Count != 0 && evaluateAntecedent(ai, pl)) //評估Antecedent
                    {
                        antecedentPL = pl;
                        return antecedentPL;
                    }
                    if (pl.next != null) antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    if (pl.next != null && antecedentPL == null && !stop)
                    {
                        antecedentPL = findAntecedentTraversal(ai, pl.next, anaphoraPL, out stop);
                        if (antecedentPL != null) return antecedentPL;
                    }
                }
            }
            return antecedentPL;
        }

        //Evaluate Antecedent

        //評估Antecedent
        //ai: 要被取代的Anaphora資訊
        //pl: 要被評估的Antecedent PL
        private static bool evaluateAntecedent(AnaphoraInfo ai, PL pl)
        {
            bool hasNNP = hasNNPFromPL(pl);     //檢查PL裡是否有專有名詞
            bool hasNNS = hasNNSFromPL(pl);     //檢查PL裡是否有複數名詞
            string nouns = getNounsFromPL(pl);  //將PL裡的名詞擷取出來
            if (nouns == "") return false;
            if (ai.pos == "PRP")    //要被取代的Anaphora是代名詞
            {
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
            else if (ai.pos == "NP")    //要被取代的Anaphora是普通名詞
            {
                if (hasNNP)  //是專有名詞
                {
                    //除了AnaphoraInfo，還要檢查Anaphora的Infobox和Antecedent是否為同義詞
                    string wikiTitle;
                    bool isSyn = false;
                    Infobox infobox = Wiki.getInfobox(nouns, out wikiTitle);
                    if (infobox != null)
                    {
                        string[] ary = infobox.infobox.Split(' ');
                        foreach (string str in ary) if (Thesaurus.hasSynonym(str, ai.word)) isSyn = true;
                    }
                    //else isSyn = true;

                    AnaphoraInfo aiNNP = getNNPAnaphoraInfo(nouns); //取得專有名詞的AnaphoraInfo
                    return isSyn && isSameAnaphoraInfo(ai, aiNNP);  //檢查兩個AnaphoraInfo是否相同
                }
            }
            return false;
        }

        //Transform WAPList

        //將代名詞的Anaphora取代成Antecedent
        private static List<WordAndPOS> transformPRPAnaphoraWAPList(List<WordAndPOS> AnaphoraWAPList, List<WordAndPOS> AntecedentWAPList)
        {
            List<WordAndPOS> complexWAPList = new List<WordAndPOS>();
            bool hasTransformed = false;
            for (int i = 0; i < AnaphoraWAPList.Count; i++)
            {
                if (AnaphoraWAPList[i].pos == "PRP$" && !hasTransformed)    //所有格
                {
                    complexWAPList.AddRange(AntecedentWAPList);
                    if (!hasQuotationS(AntecedentWAPList))  //判斷List<WordAndPOS>是否有's
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
        //將普通名詞的Anaphora取代成Antecedent
        private static List<WordAndPOS> transformNPAnaphoraWAPList(List<WordAndPOS> AnaphoraWAPList, List<WordAndPOS> AntecedentWAPList)
        {
            printWAPList(AnaphoraWAPList, AntecedentWAPList);   //印出AnaphoraWAPList和AntecedentWAPList
            return AntecedentWAPList;   //整個都換
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
        //判斷List<WordAndPOS>是否有's
        private static bool hasQuotationS(List<WordAndPOS> wapList)
        {
            foreach (WordAndPOS wap in wapList) if (wap.word == "'s") return true;
            return false;
        }
    }
}
