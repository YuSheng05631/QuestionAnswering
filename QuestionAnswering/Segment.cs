using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveonik.Stemmers;
using System.Text.RegularExpressions;

namespace QuestionAnswering
{
    //詞以及其詞性
    public class WordAndPOS
    {
        public string word;
        public string pos;
        public WordAndPOS(string word, string pos)
        {
            this.word = word;
            this.pos = pos;
        }
    }
    //一個句子中主要的詞
    public class MainTerm
    {
        public List<string> W;      //Wh-
        public List<string> SQ;     //倒裝動詞
        public List<string> S;      //主詞
        public List<string> O;      //受詞
        public List<string> V;      //動詞
        public List<string> VBN;    //被動
        public List<string> PP;     //介係詞
        public MainTerm()
        {
            W = new List<string>();
            SQ = new List<string>();
            S = new List<string>();
            O = new List<string>();
            V = new List<string>();
            VBN = new List<string>();
            PP = new List<string>();
        }
    }

    public class Segment
    {
        //取得句子的詞性標記結果(依行分割)
        private static string[] getLPLines(string sentence)
        {
            sentence = sentence.Trim();
            StanfordParserTools.StanfordParserTools sp = new StanfordParserTools.StanfordParserTools();
            string LPResult = sp.LexicalizedParser(sentence).Trim(new char[] { '\n', '\r' });
            string[] LPLines = LPResult.Split('\n');
            return LPLines;
        }
        //取得每一行所佔的詞彙數(累加)
        private static List<int> getWordCountList(string[] LPLines)
        {
            //正則表達式-判斷是否為英數
            Regex regexEN = new Regex(@"^[A-Za-z0-9]+$");

            //計算每一行佔了幾個詞
            int first = 0, ct = 0;
            List<int> wordCountList = new List<int>();  //詞數List
            wordCountList.Add(0);                       //第一個位置給0
            for (int i = 0; i < LPLines.Length; i++)
            {
                first = LPLines[i].IndexOf("(");
                while (first != -1)
                {
                    first = LPLines[i].IndexOf(" ", first);
                    if (first == -1) break;
                    if (regexEN.IsMatch(LPLines[i][first + 1].ToString())) ct++;  //逗號、句點、括號、破折號、's等標點符號不算斷句
                    first = LPLines[i].IndexOf("(", first);
                }
                wordCountList.Add(ct);
            }
            return wordCountList;
        }
        //取得sentence.Split(' ')的結果
        private static string[] getSentenceWords(string sentence)
        {
            return sentence.Split(' ');
        }
        //取得分割點(以詞計數)
        private static List<int> getSegmentPosList(string[] sentenceWords, string[] LPLines, List<int> wordCountList)
        {
            int first;
            List<int> segmentPosList = new List<int>();    //分割點List(以詞計數)
            segmentPosList.Add(0); //第一個位置給0

            //以各詞性作為分割點
            for (int i = 0; i < LPLines.Length; i++)
            {
                //以Wh-(或that)作為分割點
                first = LPLines[i].IndexOf("(W");
                if (first != -1 && wordCountList[i] != 0) segmentPosList.Add(wordCountList[i]);    //位置0不需加

                //以PP作為分割點
                first = LPLines[i].IndexOf("(PP");
                if (first != -1 && wordCountList[i] != 0) segmentPosList.Add(wordCountList[i]);

                //以CC作為分割點
                first = LPLines[i].IndexOf("(CC");
                if (first != -1 && wordCountList[i] != 0) segmentPosList.Add(wordCountList[i]);
            }

            //以(S、(SBAR、(SBARQ以及其縮排作為分割點
            int preIndent = 0, nowIndent = 0;
            List<int> indentList = new List<int>();
            for (int i = 0; i < LPLines.Length; i++)
            {
                indentList.Add(LPLines[i].IndexOf("("));  //記錄每一行的縮排
                nowIndent = LPLines[i].IndexOf("(S");     //如果有找到"(S"，且下一個字是"B"、" "、或"\r"
                if (nowIndent != -1)
                {
                    string cut = LPLines[i].Substring(nowIndent + 2, 1);
                    if (cut == "B" || cut == " " || cut == "\r")
                    {
                        if (preIndent >= nowIndent)
                        {
                            //往前回朔各行的縮排，相等就繼續，找到適合的分割點(ex. (CC but))
                            int seg = i;
                            for (int j = i - 1; j >= 0; j--)
                            {
                                if (indentList[j] == nowIndent) seg = j;
                                else break;
                            }
                            segmentPosList.Add(wordCountList[seg]);
                        }
                        preIndent = nowIndent;
                    }
                }
            }

            //以LexiclizedParser縮排減短的地方作為分割點
            /*int preIndent = 0, nowIndent = 0;
            for (int i = 0; i < LPLines.Length; i++)
            {
                nowIndent = LPLines[i].IndexOf("(");
                //上一個縮排比目前的縮排還要長 && 若該行最後是"\r"(解決句點被算進去的問題)
                if (preIndent > nowIndent && LPLines[i].Substring(LPLines[i].Length - 1) == "\r")
                {
                    segmentPosList.Add(wordCountList[i]);
                }
                preIndent = nowIndent;
            }*/

            segmentPosList.Add(sentenceWords.Length);   //最後一個位置給最後一個詞
            segmentPosList.Sort();              //排序
            IEnumerable<int> ie = segmentPosList.Distinct();    //藉IEnumerable去重複
            segmentPosList = ie.ToList();
            return segmentPosList;
        }
        //取得斷句
        private static List<string> getSegmentList(string[] sentenceWords, List<int> segmentPosList)
        {
            //依segmentPosList將sentence斷句
            List<string> segmentList = new List<string>();
            for (int i = 1; i < segmentPosList.Count; i++)
            {
                string s = "";
                for (int j = segmentPosList[i - 1]; j < segmentPosList[i]; j++) s += sentenceWords[j] + " ";
                segmentList.Add(s);
            }
            return segmentList;
        }
        //取得每個斷句中主要的詞
        private static List<MainTerm> getMTList(string[] LPLines, List<int> wordCountList, List<int> segmentPosList)
        {
            int first, last;

            //從LPLines取出需要的詞性
            List<WordAndPOS> WPList = new List<WordAndPOS>();   //每個詞以及其詞性
            for (int i = 0; i < LPLines.Length; i++)
            {
                string tempStr = "";
                first = LPLines[i].IndexOf("(");
                while (first != -1)
                {
                    first = LPLines[i].IndexOf(" ", first);
                    if (first == -1) break;
                    if (LPLines[i][first + 1] != '(')
                    {
                        last = LPLines[i].IndexOf(")", first);
                        tempStr += LPLines[i].Substring(first + 1, last - first - 1) + " ";
                    }
                    first = LPLines[i].IndexOf("(", first);
                }
                tempStr = tempStr.Trim();
                if (tempStr != "" && LPLines[i].IndexOf("(WH") != -1) WPList.Add(new WordAndPOS(tempStr, "WHNP"));        //Wh-
                else if (tempStr != "" && LPLines[i].IndexOf("(SQ") != -1) WPList.Add(new WordAndPOS(tempStr, "SQ"));     //SQ(屬於倒裝動詞-do-就不屬於動詞)
                else if (tempStr != "" && LPLines[i].IndexOf("(N") != -1) WPList.Add(new WordAndPOS(tempStr, "NP"));      //名詞(S & O)
                else if (tempStr != "" && LPLines[i].IndexOf("(VBN") != -1) WPList.Add(new WordAndPOS(tempStr, "VBN"));   //被動
                else if (tempStr != "" && LPLines[i].IndexOf("(V") != -1) WPList.Add(new WordAndPOS(tempStr, "VP"));      //動詞(be & V)
                else if (tempStr != "" && LPLines[i].IndexOf("(PP") != -1) WPList.Add(new WordAndPOS(tempStr, "PP"));     //介係詞
                else WPList.Add(new WordAndPOS(tempStr, ""));
            }

            //判斷WPList的各個詞彙屬於哪個斷句
            List<MainTerm> MTList = new List<MainTerm>();   //一個句子中主要的詞
            for (int i = 0; i < segmentPosList.Count - 1; i++)
            {
                //取出一段斷詞範圍(詞數)
                first = segmentPosList[i];
                if (i + 1 == segmentPosList.Count) last = segmentPosList[i + 1];
                else last = segmentPosList[i + 1] - 1;
                //依斷詞範圍取出LPLines範圍
                List<int> partList = new List<int>();
                for (int j = 0; j < wordCountList.Count; j++)
                {
                    if (wordCountList[j] >= first && wordCountList[j] <= last)
                    {
                        partList.Add(j);
                    }
                }
                first = partList[0];
                last = partList[partList.Count - 1];
                //組成mainTerm(收集需要的詞性)
                MainTerm MTTemp = new MainTerm();
                for (int j = first; j <= last; j++)
                {
                    if (WPList[j].word != "")
                    {
                        if (WPList[j].pos == "WHNP") MTTemp.W.Add(WPList[j].word);
                        else if (WPList[j].pos == "SQ") MTTemp.SQ.Add(WPList[j].word);
                        else if (WPList[j].pos == "NP" && MTTemp.V.Count == 0 && MTTemp.VBN.Count == 0) MTTemp.S.Add(WPList[j].word);
                        else if (WPList[j].pos == "NP" && (MTTemp.V.Count != 0 || MTTemp.VBN.Count != 0)) MTTemp.O.Add(WPList[j].word);
                        else if (WPList[j].pos == "VP") MTTemp.V.Add(WPList[j].word);
                        else if (WPList[j].pos == "VBN") MTTemp.VBN.Add(WPList[j].word);
                        else if (WPList[j].pos == "PP") MTTemp.PP.Add(WPList[j].word);
                    }
                }
                MTList.Add(MTTemp);
            }
            return MTList;
        }
        //取得斷句
        public static void getSegment(string sentence, out List<string> segmentList, out List<MainTerm> MTList)
        {
            //取得句子的詞性標記結果(依行分割)
            string[] LPLines = getLPLines(sentence);

            //取得每一行所佔的詞彙數(累加)
            List<int> wordCountList = getWordCountList(LPLines);

            //取得sentence.Split(' ')的結果
            string[] sentenceWords = getSentenceWords(sentence);

            //取得分割點(以詞計數)
            List<int> segmentPosList = getSegmentPosList(sentenceWords, LPLines, wordCountList);
            
            //取得斷句
            segmentList = getSegmentList(sentenceWords, segmentPosList);
            
            //取得每個斷句中主要的詞
            MTList = getMTList(LPLines, wordCountList, segmentPosList);
        }
    }
}
