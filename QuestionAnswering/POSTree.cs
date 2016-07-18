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
        public List<S> S;
        //public bool isQuestion;
        public ROOT()
        {
            this.S = new List<S>();
        }
    }
    //Sentence
    public class S
    {
        //ADJP、ADVP、NN、CC
        public PL Ss, WH, SQ, NP, VP, PP;
        public S()
        {
            this.Ss = new PL();
            this.WH = new PL();
            this.SQ = new PL();
            this.NP = new PL();
            this.VP = new PL();
            this.PP = new PL();
        }
        //將空白的變數設成null
        public void setNull()
        {
            if (this.Ss.words.Count == 0 && this.Ss.next.Count == 0) this.Ss = null;
            if (this.WH.words.Count == 0 && this.WH.next.Count == 0) this.WH = null;
            if (this.SQ.words.Count == 0 && this.SQ.next.Count == 0) this.SQ = null;
            if (this.NP.words.Count == 0 && this.NP.next.Count == 0) this.NP = null;
            if (this.VP.words.Count == 0 && this.VP.next.Count == 0) this.VP = null;
            if (this.PP.words.Count == 0 && this.PP.next.Count == 0) this.PP = null;
        }
    }
    //Phrase Level
    public class PL
    {
        public List<WordAndPOS> words;
        public List<S> next;
        public PL()
        {
            this.words = new List<WordAndPOS>();
            this.next = new List<S>();
        }
    }
    public class POSTree
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
            List<S> SList = new List<S>();
            //將(ROOT底下的各個(S拿去Traversal，回傳的S組成一個List，成為ROOT.S
            //一般(ROOT底下只有一個(S，但還是寫成List形式以防例外
            foreach (POSUnit SUnit in POSUnitTree.next) SList.Add(getROOTTraversal(SUnit));
            root.S = SList;
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
                //若自己是S，next也是S，直接將Ss設置SList
                //若自己是S，next不是S，將SList的各pos彙集成一個S，並將Ss設成它
                sThis.Ss.words = unit.words;
                if (unit.next.Count != 0 && unit.next[0].pos == "S") sThis.Ss.next = SList;
                else sThis.Ss.next.Add(setPOS(SList));
            }
            else if (unit.pos.IndexOf("WH") == 0)
            {
                sThis.WH.words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.WH.next.Add(sTemp);
            }
            else if (unit.pos == "SQ")
            {
                sThis.SQ.words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.SQ.next.Add(sTemp);
            }
            else if (unit.pos == "NP")
            {
                sThis.NP.words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.NP.next.Add(sTemp);
            }
            else if (unit.pos == "VP")
            {
                sThis.VP.words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.VP.next.Add(sTemp);
            }
            else if (unit.pos == "PP")
            {
                sThis.PP.words = unit.words;
                S sTemp = setPOS(SList);
                if (sTemp != null) sThis.PP.next.Add(sTemp);
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
                if (s.Ss != null) sTemp.Ss = s.Ss;
                if (s.WH != null) sTemp.WH = s.WH;
                if (s.SQ != null) sTemp.SQ = s.SQ;
                if (s.NP != null) sTemp.NP = s.NP;
                if (s.VP != null) sTemp.VP = s.VP;
                if (s.PP != null) sTemp.PP = s.PP;
            }
            sTemp.setNull();
            return sTemp;
        }

        //印出ROOT(Start)
        private static void printROOT(ROOT root)
        {
            if (root.S.Count == 0 || root.S[0] == null)
            {
                Console.WriteLine("No S.");
                return;
            }
            foreach (S s in root.S)
            {
                Console.WriteLine("ROOT-S");
                printROOTTraversal(s, 0);
            }
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
                Console.Write("WH  " + line);
                foreach (WordAndPOS wap in s.WH.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.WH.next) printROOTTraversal(sNext, indent);
            }
            if (s.SQ != null)
            {
                Console.Write("SQ  " + line);
                foreach (WordAndPOS wap in s.SQ.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.SQ.next) printROOTTraversal(sNext, indent);
            }
            if (s.NP != null)
            {
                Console.Write("NP  " + line);
                foreach (WordAndPOS wap in s.NP.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.NP.next) printROOTTraversal(sNext, indent);
            }
            if (s.VP != null)
            {
                Console.Write("VP  " + line);
                foreach (WordAndPOS wap in s.VP.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.VP.next) printROOTTraversal(sNext, indent);
            }
            if (s.PP != null)
            {
                Console.Write("PP  " + line);
                foreach (WordAndPOS wap in s.PP.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.PP.next) printROOTTraversal(sNext, indent);
            }
            if (s.Ss != null)
            {
                Console.Write("S   " + line);
                foreach (WordAndPOS wap in s.Ss.words) Console.Write(wap.word + " ");
                Console.WriteLine();
                foreach (S sNext in s.Ss.next) printROOTTraversal(sNext, indent);
            }
        }

        //main
        public static ROOT getPOSTree(string sentence)
        {
            //取得句子的詞性標記結果(依行分割)
            string[] LPLines = getLPLines(sentence);

            //取得LPLines每一行的縮排
            List<int> LPLinesIndent = getLPLinesIndent(LPLines);

            //取得LPLines各縮排數有哪些行
            List<List<int>> LPLinesIndentNote = getLPLinesIndentNote(LPLinesIndent);

            //將每一行的LPLines轉成POSUnit(不處理next)
            List<POSUnit> POSUnitList = getPOSUnitList(LPLines);

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
