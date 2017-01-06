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
        public PL prev, brotherY, brotherO;
        public string pos;
        public int indent, index;
        public PL()
        {
            this.words = new List<WordAndPOS>();
            this.next = new List<PL>();
            this.pos = "";
            this.indent = 0;
            this.index = 0;
        }
    }
    class Sentence
    {
        //取得文章的斷行點(之後會在PLArticle插入null，以便尋找Antecedent時找到中斷點)
        private List<int> getSegmentList(string article, StanfordCoreNLP.ResultSet rs)
        {
            List<int> segmentList = new List<int>();
            for (int i = 0; i < rs.sentenceList.Count; i++)
                if (article.IndexOf(rs.sentenceList[i] + "\n") != -1)
                    segmentList.Add(i + 1);
            return segmentList;
        }

        //將填空的標籤取代成NNans + 標籤字母
        private string replaceSlot(string sentence)
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
        //去掉<U>標籤(保留<B>)
        private string removeULabel(string sentence)
        {
            int first = 0, last = 0;
            first = sentence.IndexOf("<U");
            while (first != -1)
            {
                last = sentence.IndexOf(">", first) + 1;
                sentence = sentence.Remove(first, last - first);

                first = sentence.IndexOf("</U", first);
                last = sentence.IndexOf(">", first) + 1;
                sentence = sentence.Remove(first, last - first);

                first = sentence.IndexOf("<U", first);
            }
            return sentence;
        }

        //取得句子的詞性標記結果(依行分割)
        private string[] getLPLines(string POSTree)
        {
            POSTree = POSTree.Trim(new char[] { '\n', '\r' });
            return POSTree.Split('\n');
        }
        //取得PLList(處理words, pos, indent & index)
        private List<PL> getPLListW(string[] LPLines)
        {
            List<PL> PLList = new List<PL>();
            int first = 0, last = 0, index = 0;
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
                    tempPL.index = index;
                    tempPL.indent = line.IndexOf("(");
                    PLList.Add(tempPL);
                    index += 1;
                }
            }
            return PLList;
        }
        //取得PLList(處理next & prev)
        private void getPLListN(List<PL> PLList)
        {
            for (int i = 0; i < PLList.Count; i++)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (PLList[i].indent > PLList[j].indent)
                    {
                        PLList[i].prev = PLList[j];
                        PLList[j].next.Add(PLList[i]);
                        break;
                    }
                }
            }
        }
        //取得PLList(處理brotherY & brotherO)
        private void getPLListB(List<PL> PLList)
        {
            for (int i = 0; i < PLList.Count; i++)
            {
                for (int j = i - 1; j >= 0; j--)
                {
                    if (PLList[i].indent == PLList[j].indent)
                    {
                        PLList[i].brotherY = PLList[j];
                        PLList[j].brotherO = PLList[i];
                        break;
                    }
                    else if (PLList[i].indent > PLList[j].indent) break;
                }
            }
        }

        //印出PLList
        public void printPLList(List<PL> PLList)
        {
            if (PLList == null) return;
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
        public List<List<PL>> getPLArticle(string sentence)
        {
            List<List<PL>> PLArticle = new List<List<PL>>();

            //將填空的標籤取代成NNans
            //sentence = replaceSlot(sentence);

            sentence = removeULabel(sentence);  //去掉<U>標籤(保留<B>)

            //取得詞性標記結果
            StanfordCoreNLP.Parser sc = new StanfordCoreNLP.Parser();
            StanfordCoreNLP.ResultSet rs = sc.getResultSet(sentence);

            foreach (string POSTree in rs.POSTreeList)
            {
                string[] LPLines = getLPLines(POSTree); //取得句子的詞性標記結果(依行分割)
                
                List<PL> tempPLList = getPLListW(LPLines);  //取得PLList(處理words, pos, indent & index)
                getPLListN(tempPLList);                     //取得PLList(處理next)
                getPLListB(tempPLList);                     //取得PLList(處理brotherY & brotherO)

                PLArticle.Add(tempPLList);
            }
            //取得文章的斷行點，並插入null
            List<int> segmentList = getSegmentList(sentence, rs);
            for (int i = segmentList.Count - 1; i >= 0; i--) PLArticle.Insert(segmentList[i], null);
            return PLArticle;
        }
        //取得PLArticle
        public List<List<PL>> getPLArticle(List<string> sentenceList)
        {
            //將List<string>合併為單一string
            string s = "";
            foreach (string sentence in sentenceList)
            {
                if (sentence == "\n") s = s.Trim();
                s += sentence + " ";
            }
            return getPLArticle(s.Trim());
        }
    }
}
