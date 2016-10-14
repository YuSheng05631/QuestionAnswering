using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    public class WordAndPOS
    {
        public string word;
        public string pos;
        public WordAndPOS()
        {
            this.word = "";
            this.pos = "";
        }
        public WordAndPOS(string word, string pos)
        {
            this.word = word;
            this.pos = pos;
        }
    }
    public class PL
    {
        public List<WordAndPOS> words;
        public List<PL> next;
        public string pos;
        public int indent;
        public PL()
        {
            this.words = new List<WordAndPOS>();
            this.next = new List<PL>();
            this.pos = "";
            this.indent = 0;
        }
    }
    class Sentence
    {
        //將填空的標籤取代成NNans + 標籤字母
        private static string replaceSlot(string sentence)
        {
            int first = sentence.IndexOf("(");
            int last = first + 2;
            while (first != -1)
            {
                if (sentence[last] == ')')  //括號裡只有一個字
                {
                    string cut = sentence.Substring(first + 1, 1);
                    if (Convert.ToChar(cut) >= 65 && Convert.ToChar(cut) <= 90) //是大寫字母
                    {
                        sentence = sentence.Substring(0, first) + sentence.Substring(last + 1);
                        sentence = sentence.Insert(first, "NNans" + cut);
                    }
                }
                first = sentence.IndexOf("(", first + 1);
                last = first + 2;
            }
            return sentence;
        }

        //取得句子的詞性標記結果(依行分割)
        private static string[] getLPLines(string POSTree)
        {
            POSTree = POSTree.Trim(new char[] { '\n', '\r' });
            return POSTree.Split('\n');
        }
        //取得LPLines每一行的縮排
        private static List<int> getLPLinesIndent(string[] LPLines)
        {
            List<int> LPLinesIndent = new List<int>();
            foreach (string line in LPLines) LPLinesIndent.Add(line.IndexOf("("));
            return LPLinesIndent;
        }
        //取得LPLines各縮排數有哪些行
        private static List<List<int>> getLPLinesIndentNote(List<int> LPLinesIndent)
        {
            #region e.g.
            /*
            第0個縮排(縮排=0)有：第0行
            第1個縮排(縮排=2)有：第1、4行
            第2個縮排(縮排=4)有：第2、3、5行
            第4個縮排(縮排=6)有：第6行
             */
            #endregion
            List<List<int>> LPLinesIndentNote = new List<List<int>>();
            for (int n = 0; ; n += 2)
            {
                List<int> result = Enumerable.Range(0, LPLinesIndent.Count)
                    .Where(i => LPLinesIndent[i] == n)
                    .ToList();
                if (result.Count == 0) break;
                LPLinesIndentNote.Add(result);
            }
            if (LPLinesIndentNote[0].Count > 1) return new List<List<int>>();   //若有兩個ROOT以上
            return LPLinesIndentNote;
        }

        //取得PLList(處理words & pos)
        private static List<PL> getPLListWP(string[] LPLines)
        {
            List<PL> PLList = new List<PL>();
            int first = 0, last = 0;
            foreach (string line in LPLines)
            {
                first = line.IndexOf("(");
                if (first != -1)
                {
                    PL tempPL = new PL();
                    //抓phrase level pos
                    first += 1;
                    last = line.IndexOf(" ", first);
                    if (last == -1) //e.g. (ROOT
                    {
                        last = line.IndexOf("\r", first);
                        tempPL.pos = line.Substring(first, last - first);
                    }
                    else
                    {
                        tempPL.pos = line.Substring(first, last - first);
                        //抓words
                        first = line.IndexOf("(", first);   //e.g. (NP (NNP First) (NNP World))
                        if (first == -1)
                        {
                            first = line.IndexOf("(");      //e.g. (NN war))
                            tempPL.pos = "";    //清空pos (應依word level pos置換相對應的phrase level pos)
                        }
                        while (first != -1)
                        {
                            WordAndPOS tempWord = new WordAndPOS();
                            //抓word level pos
                            first += 1;
                            last = line.IndexOf(" ", first);
                            if (last == -1) last = line.IndexOf(" ˈ", first); //A bug of IndexOf(). e.g. (NP (JJ ˈalbɛɐ̯t) (NN ˈaɪnʃtaɪn))))))\r
                            tempWord.pos = line.Substring(first, last - first);
                            //抓word level word
                            first = last + 1;
                            last = line.IndexOf(")", first);
                            tempWord.word = line.Substring(first, last - first);
                            tempPL.words.Add(tempWord);
                            first = line.IndexOf("(", first);
                        }
                    }
                    PLList.Add(tempPL);
                }
            }
            return PLList;
        }
        //取得PLList(處理indent)
        private static List<PL> getPLListI(List<PL> PLList, List<int> LPLinesIndent)
        {
            for (int i = 0; i < PLList.Count; i++) PLList[i].indent = LPLinesIndent[i];
            return PLList;
        }
        //取得PLList(處理next)
        private static List<PL> getPLListN(List<PL> PLList, List<List<int>> LPLinesIndentNote)
        {
            //尋找上一個縮排中正確的母節點
            for (int i = 1; i < LPLinesIndentNote.Count; i++)
            {
                for (int j = 0; j < LPLinesIndentNote[i].Count; j++)
                {
                    int index = 0;
                    for (int m = 0; m < LPLinesIndentNote[i - 1].Count; m++)
                    {
                        if (LPLinesIndentNote[i - 1][m] < LPLinesIndentNote[i][j]) index = m;
                        else break;
                    }
                    PLList[LPLinesIndentNote[i - 1][index]].next.Add(PLList[LPLinesIndentNote[i][j]]);
                }
            }
            return PLList;
        }

        //印出PLList
        public static void printPLList(List<PL> PLList)
        {
            foreach (PL pl in PLList)
            {
                string space = "";
                for (int i = 0; i < pl.indent; i++) space += " ";
                
                Console.Write(space);
                if (pl.pos != "") Console.Write("(" + pl.pos + " ");
                foreach (WordAndPOS wap in pl.words) Console.Write("(" + wap.pos + " " + wap.word + ") ");
                Console.WriteLine();
            }
        }
        
        //取得PLArticle
        public static List<List<PL>> getPLArticle(string sentence)
        {
            List<List<PL>> PLArticle = new List<List<PL>>();

            //將填空的標籤取代成NNans
            sentence = replaceSlot(sentence);

            //取得詞性標記結果
            StanfordCoreNLP.Parser sc = new StanfordCoreNLP.Parser();
            StanfordCoreNLP.ResultSet rs = sc.getResultSet(sentence);

            foreach (string POSTree in rs.POSTreeList)
            {
                string[] LPLines = getLPLines(POSTree); //取得句子的詞性標記結果(依行分割)
                
                List<int> LPLinesIndent = getLPLinesIndent(LPLines);                        //取得LPLines每一行的縮排
                List<List<int>> LPLinesIndentNote = getLPLinesIndentNote(LPLinesIndent);    //取得LPLines各縮排數有哪些行
                
                List<PL> tempPLList = getPLListWP(LPLines);      //取得PLList(處理words & pos)
                tempPLList = getPLListI(tempPLList, LPLinesIndent);     //取得PLList(處理indent)
                tempPLList = getPLListN(tempPLList, LPLinesIndentNote); //取得PLList(處理next)

                PLArticle.Add(tempPLList);
            }
            return PLArticle;
        }
        //取得PLArticle
        public static List<List<PL>> getPLArticle(List<string> sentenceList)
        {
            //將List<string>合併為單一string
            string s = "";
            foreach (string sentence in sentenceList) s += sentence + "\n";
            return getPLArticle(s);
        }
    }
}
