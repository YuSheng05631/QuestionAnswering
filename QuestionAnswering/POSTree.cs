using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    //詞彙及詞性
    public class WordAndPOS
    {
        public string word;
        public string pos;
        public WordAndPOS()
        {
            this.word = "";
            this.pos = "";
        }
    }
    //POSTree以行劃分的單位，有Phrase Level的POS、包含的詞彙(WordAndPOS)、下一層的單位List
    public class POSUnit
    {
        public List<WordAndPOS> words;
        public List<POSUnit> next;
        public string pos;
        public POSUnit()
        {
            this.words = new List<WordAndPOS>();
            this.next = new List<POSUnit>();
            this.pos = "";
        }
    }
    //ROOT
    public class ROOT
    {
        public S S;
        //public bool isQuestion;
        public ROOT()
        {
            this.S = new S();
        }
    }
    //Sentence
    public class S
    {
        //NN、CC、QP
        public List<PL> Ss, WH, SQ, NP, VP, PP, ADJP, ADVP;
        public S()
        {
            this.Ss = new List<PL>();
            this.WH = new List<PL>();
            this.SQ = new List<PL>();
            this.NP = new List<PL>();
            this.VP = new List<PL>();
            this.PP = new List<PL>();
            this.ADJP = new List<PL>();
            this.ADVP = new List<PL>();
        }
        //將空白的變數設成null
        public void setNull()
        {
            if (this.Ss.Count == 0) this.Ss = null;
            if (this.WH.Count == 0) this.WH = null;
            if (this.SQ.Count == 0) this.SQ = null;
            if (this.NP.Count == 0) this.NP = null;
            if (this.VP.Count == 0) this.VP = null;
            if (this.PP.Count == 0) this.PP = null;
            if (this.ADJP.Count == 0) this.ADJP = null;
            if (this.ADVP.Count == 0) this.ADVP = null;
        }
    }
    //Phrase Level
    public class PL
    {
        public List<WordAndPOS> words;
        public S next;
        public PL()
        {
            this.words = new List<WordAndPOS>();
            //this.next = new S();
        }
    }
    //PL及詞性
    public class PLAndPOS
    {
        public PL pl;
        public string pos;
        public PLAndPOS(PL pl, string pos)
        {
            this.pl = pl;
            this.pos = pos;
        }
    }
    class POSTree
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
        private static string[] getLPLines(string sentence)
        {
            sentence = sentence.Trim();
            StanfordParserTools.StanfordParserTools sp = new StanfordParserTools.StanfordParserTools();
            string LPResult = sp.LexicalizedParser(sentence).Trim(new char[] { '\n', '\r' });
            string[] LPLines = LPResult.Split('\n');
            return LPLines;
        }
        //取得LPLines每一行的縮排
        private static List<int> getLPLinesIndent(string[] LPLines)
        {
            List<int> LPLinesIndent = new List<int>();
            foreach (string line in LPLines)
            {
                LPLinesIndent.Add(line.IndexOf("("));
            }
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

        //將每一行的LPLines轉換成POSUnit(不處理next)
        private static List<POSUnit> getPOSUnitList(string[] LPLines)
        {
            List<POSUnit> POSUnitList = new List<POSUnit>();
            int first = 0, last = 0;
            for (int i = 0; i < LPLines.Length; i++)
            {
                first = LPLines[i].IndexOf("(");
                if (first != -1)
                {
                    POSUnit tempPOSUnit = new POSUnit();
                    //抓pos
                    first += 1;
                    last = LPLines[i].IndexOf(" ", first);
                    if (last == -1) //e.g. (ROOT
                    {
                        last = LPLines[i].IndexOf("\r", first);
                        tempPOSUnit.pos = LPLines[i].Substring(first, last - first);
                    }
                    else
                    {
                        tempPOSUnit.pos = LPLines[i].Substring(first, last - first);
                        //抓words
                        bool hasP = false;
                        first = LPLines[i].IndexOf("(", first);
                        while (first != -1) //e.g. (NP (NNP First) (NNP World))
                        {
                            hasP = true;
                            WordAndPOS tempWord = new WordAndPOS();
                            //抓word的pos
                            first += 1;
                            last = LPLines[i].IndexOf(" ", first);
                            tempWord.pos = LPLines[i].Substring(first, last - first);
                            //抓word的word
                            first = last + 1;
                            last = LPLines[i].IndexOf(")", first);
                            tempWord.word = LPLines[i].Substring(first, last - first);
                            tempPOSUnit.words.Add(tempWord);
                            first = LPLines[i].IndexOf("(", first);
                        }
                        if (!hasP)  //e.g. (NN war))
                        {
                            tempPOSUnit.pos = "";  //清空pos
                            first = LPLines[i].IndexOf("(");
                            while (first != -1)
                            {
                                WordAndPOS tempWord = new WordAndPOS();
                                //抓word的pos
                                first += 1;
                                last = LPLines[i].IndexOf(" ", first);
                                tempWord.pos = LPLines[i].Substring(first, last - first);
                                //抓word的word
                                first = last + 1;
                                last = LPLines[i].IndexOf(")", first);
                                tempWord.word = LPLines[i].Substring(first, last - first);
                                tempPOSUnit.words.Add(tempWord);
                                first = LPLines[i].IndexOf("(", first);
                            }
                        }
                    }
                    POSUnitList.Add(tempPOSUnit);
                }
            }
            return POSUnitList;
        }
        //修復POSUnitList中沒有pos的unit
        private static List<POSUnit> fixPOSUnitListNoPos(List<List<int>> LPLinesIndentNote, List<POSUnit> POSUnitList)
        {
            #region e.g.
            /*
            (NP (DT The)
              (ADJP (RB formerly) (VBN monopolized))
              (NN technology))
            
            (NN technology)沒有pos，則將pos設成前一個縮排的pos
            */
            #endregion
            for (int i = 0; i < POSUnitList.Count; i++)
            {
                if (POSUnitList[i].pos == "")   //沒有pos
                {
                    //找到這一行的縮排數
                    int indent = 0;
                    for (int j = 0; j < LPLinesIndentNote.Count; j++)
                        for (int k = 0; k < LPLinesIndentNote[j].Count; k++)
                            if (i == LPLinesIndentNote[j][k]) indent = j;
                    //從前一個縮排的行數中，找到小於這一行且最大的行數
                    int prevLine = i;
                    for (int j = 0; j < LPLinesIndentNote[indent - 1].Count; j++)
                        if (i > LPLinesIndentNote[indent - 1][j]) prevLine = LPLinesIndentNote[indent - 1][j];
                    //將這一行的pos設成前一個縮排的pos(兩個pos的首字母要一樣)
                    if (POSUnitList[i].words[0].pos[0] == POSUnitList[prevLine].pos[0])
                        POSUnitList[i].pos = POSUnitList[prevLine].pos;
                }
            }
            return POSUnitList;
        }
        //將POSUnitList轉換成Tree的形式(處理next)
        private static POSUnit getPOSUnitTree(List<List<int>> LPLinesIndentNote, List<POSUnit> POSUnitList)
        {
            POSUnit POSUnitTree = new POSUnit();
            POSUnitTree = POSUnitList[0]; //(ROOT
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
                    POSUnitList[LPLinesIndentNote[i - 1][index]].next.Add(POSUnitList[LPLinesIndentNote[i][j]]);
                }
            }
            return POSUnitTree;
        }
        //印出POSUnitTree
        private static void printPOSUnitTree(POSUnit POSUnitTree, int indent)
        {
            string line = "";
            for (int i = 0; i < indent; i++) line += "  ";
            if (POSUnitTree.pos != "") line += "(" + POSUnitTree.pos + " ";
            foreach (WordAndPOS w in POSUnitTree.words) line += "(" + w.pos + " " + w.word + ") ";
            Console.WriteLine(line);
            indent += 1;
            foreach (POSUnit n in POSUnitTree.next) printPOSUnitTree(n, indent);
        }

        //取得ROOT(Start)
        private static ROOT getROOT(POSUnit POSUnitTree)
        {
            ROOT root = new ROOT();
            root.S = getROOTTraversal(POSUnitTree.next[0]); //一般ROOT底下只有一個S
            return root;
        }
        //取得ROOT(Traversal)
        private static S getROOTTraversal(POSUnit unit)
        {
            //把自己的每個next存成List<S>
            List<S> SList = new List<S>();
            foreach (POSUnit unitNext in unit.next)
            {
                S sNext = getROOTTraversal(unitNext);
                if (sNext != null) SList.Add(sNext);
            }

            //依自己的pos，設置自己的words和next
            S sThis = new S();
            if (unit.pos == "S" || unit.pos.IndexOf("SBAR") == 0)
            {
                sThis.Ss.Add(new PL());
                sThis.Ss[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.Ss[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("WH") == 0)
            {
                sThis.WH.Add(new PL());
                sThis.WH[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.WH[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("SQ") == 0)
            {
                sThis.SQ.Add(new PL());
                sThis.SQ[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.SQ[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("NP") == 0)
            {
                sThis.NP.Add(new PL());
                sThis.NP[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.NP[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("VP") == 0)
            {
                sThis.VP.Add(new PL());
                sThis.VP[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.VP[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("PP") == 0)
            {
                sThis.PP.Add(new PL());
                sThis.PP[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.PP[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("ADJP") == 0)
            {
                sThis.ADJP.Add(new PL());
                sThis.ADJP[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.ADJP[0].next = sTemp;
            }
            else if (unit.pos.IndexOf("ADVP") == 0)
            {
                sThis.ADVP.Add(new PL());
                sThis.ADVP[0].words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.ADVP[0].next = sTemp;
            }
            else return null;   //不屬於以上pos就回傳null

            sThis.setNull();    //將空白的變數設成null
            return sThis;
        }
        //將SList的各pos彙集成一個S (getROOTTraversal用)
        private static S setPOS(List<S> SList)
        {
            if (SList.Count == 0) return null;
            S sTemp = new S();
            foreach (S s in SList)
            {
                if (s.Ss != null) sTemp.Ss.AddRange(s.Ss);
                if (s.WH != null) sTemp.WH.AddRange(s.WH);
                if (s.SQ != null) sTemp.SQ.AddRange(s.SQ);
                if (s.NP != null) sTemp.NP.AddRange(s.NP);
                if (s.VP != null) sTemp.VP.AddRange(s.VP);
                if (s.PP != null) sTemp.PP.AddRange(s.PP);
                if (s.ADJP != null) sTemp.ADJP.AddRange(s.ADJP);
                if (s.ADVP != null) sTemp.ADVP.AddRange(s.ADVP);
            }
            sTemp.setNull();
            return sTemp;
        }

        //印出ROOT(Start)
        public static void printROOT(ROOT root)
        {
            if (root.S == null)
            {
                Console.WriteLine("No S.");
                return;
            }
            Console.WriteLine("ROOT-S");
            printROOTTraversal(root.S, 0);
        }
        //印出ROOT(Traversal)
        private static void printROOTTraversal(S s, int indent)
        {
            string line = "";
            for (int i = 0; i < indent; i++) line += "  ";
            line += ".";
            indent += 1;

            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    Console.Write("WH  " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    Console.Write("SQ  " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    Console.Write("NP  " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    Console.Write("VP  " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    Console.Write("PP  " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    Console.Write("ADJP" + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    Console.Write("ADVP" + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    Console.Write("S   " + line);
                    foreach (WordAndPOS wap in pl.words) Console.Write(wap.word + " ");
                    Console.WriteLine();
                    if (pl.next != null) printROOTTraversal(pl.next, indent);
                }
            }
        }

        //取得POSTree的長度(Start)
        public static int getPOSTreeLength(ROOT root)
        {
            return getPOSTreeLengthTraversal(root.S);
        }
        //取得POSTree的長度(Traversal)
        private static int getPOSTreeLengthTraversal(S s)
        {
            int length = 0;
            if (s.Ss != null)
            {
                length += s.Ss.Count;
                foreach (PL pl in s.Ss) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.WH != null)
            {
                length += s.WH.Count;
                foreach (PL pl in s.WH) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.SQ != null)
            {
                length += s.SQ.Count;
                foreach (PL pl in s.SQ) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.NP != null)
            {
                length += s.NP.Count;
                foreach (PL pl in s.NP) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.VP != null)
            {
                length += s.VP.Count;
                foreach (PL pl in s.VP) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.PP != null)
            {
                length += s.PP.Count;
                foreach (PL pl in s.PP) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.ADJP != null)
            {
                length += s.ADJP.Count;
                foreach (PL pl in s.ADJP) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            if (s.ADVP != null)
            {
                length += s.ADVP.Count;
                foreach (PL pl in s.ADVP) if (pl.next != null) length += getPOSTreeLengthTraversal(pl.next);
            }
            return length;
        }

        //將POSTree轉換成List<PL>(Start)
        public static List<PLAndPOS> getPLList(ROOT root)
        {
            return getPLListTraversal(root.S);
        }
        //將POSTree轉換成List<PL>(Traversal)
        private static List<PLAndPOS> getPLListTraversal(S s)
        {
            List<PLAndPOS> PLList = new List<PLAndPOS>();
            if (s.WH != null)
            {
                foreach (PL pl in s.WH)
                {
                    PLList.Add(new PLAndPOS(pl, "WH"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.SQ != null)
            {
                foreach (PL pl in s.SQ)
                {
                    PLList.Add(new PLAndPOS(pl, "SQ"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.NP != null)
            {
                foreach (PL pl in s.NP)
                {
                    PLList.Add(new PLAndPOS(pl, "NP"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.VP != null)
            {
                foreach (PL pl in s.VP)
                {
                    PLList.Add(new PLAndPOS(pl, "VP"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.PP != null)
            {
                foreach (PL pl in s.PP)
                {
                    PLList.Add(new PLAndPOS(pl, "PP"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.ADJP != null)
            {
                foreach (PL pl in s.ADJP)
                {
                    PLList.Add(new PLAndPOS(pl, "ADJP"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.ADVP != null)
            {
                foreach (PL pl in s.ADVP)
                {
                    PLList.Add(new PLAndPOS(pl, "ADVP"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            if (s.Ss != null)
            {
                foreach (PL pl in s.Ss)
                {
                    PLList.Add(new PLAndPOS(pl, "Ss"));
                    if (pl.next != null) PLList.AddRange(getPLListTraversal(pl.next));
                }
            }
            return PLList;
        }

        //main
        public static ROOT getPOSTree(string sentence)
        {
            //將填空的標籤取代成NNans
            sentence = replaceSlot(sentence);

            //取得句子的詞性標記結果(依行分割)
            string[] LPLines = getLPLines(sentence);

            //取得LPLines每一行的縮排
            List<int> LPLinesIndent = getLPLinesIndent(LPLines);

            //取得LPLines各縮排數有哪些行
            List<List<int>> LPLinesIndentNote = getLPLinesIndentNote(LPLinesIndent);
            if (LPLinesIndentNote.Count == 0)
            {
                Console.WriteLine("有兩個ROOT。");
                return new ROOT();
            }

            //將每一行的LPLines轉成POSUnit(不處理next)
            List<POSUnit> POSUnitList = getPOSUnitList(LPLines);

            //修復POSUnitList中沒有pos的unit
            POSUnitList = fixPOSUnitListNoPos(LPLinesIndentNote, POSUnitList);

            //將POSUnitList轉換成Tree的形式(處理next)
            POSUnit POSUnitTree = getPOSUnitTree(LPLinesIndentNote, POSUnitList);
            
            //印出POSUnitTree
            //printPOSUnitTree(POSUnitTree, 0);
            //Console.WriteLine();

            //取得ROOT(Start)
            ROOT root = getROOT(POSUnitTree);

            //印出ROOT(Start)
            //printROOT(root);

            return root;
        }
    }
}
