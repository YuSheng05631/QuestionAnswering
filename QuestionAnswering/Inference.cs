using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    public class Inference
    {
        private static string NNans = "";
        
        //兩個root是否相同(Start)
        public static bool isSamePOSTree(ROOT root1, ROOT root2)
        {
            bool isSame = true;
            if (root1.S.Count != root2.S.Count) return false;   //S數量不同
            for (int i = 0; i < root1.S.Count; i++)
            {
                isSame = isSame && isSamePOSTreeTraversal(root1.S[i], root2.S[i]);
            }

            return isSame;
        }
        //兩個root是否相同(Traversal)
        private static bool isSamePOSTreeTraversal(S s1, S s2)
        {
            //檢查當下的S
            bool isSame = isSame = isSameS(s1, s2);
            if (!isSame) return isSame;
            //檢查next的S，經過isSameS(s1, s2)可以確保兩個S的各PL數一樣多
            if (s1.Ss != null && s2.Ss != null)
                for (int i = 0; i < s1.Ss.Count; i++)
                    if (s1.Ss[i].next != null && s2.Ss[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.Ss[i].next, s2.Ss[i].next);
            if (s1.WH != null && s2.WH != null)
                for (int i = 0; i < s1.WH.Count; i++)
                    if (s1.WH[i].next != null && s2.WH[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.WH[i].next, s2.WH[i].next);
            if (s1.SQ != null && s2.SQ != null)
                for (int i = 0; i < s1.SQ.Count; i++)
                    if (s1.SQ[i].next != null && s2.SQ[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.SQ[i].next, s2.SQ[i].next);
            if (s1.NP != null && s2.NP != null)
                for (int i = 0; i < s1.NP.Count; i++)
                    if (s1.NP[i].next != null && s2.NP[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.NP[i].next, s2.NP[i].next);
            if (s1.VP != null && s2.VP != null)
                for (int i = 0; i < s1.VP.Count; i++)
                    if (s1.VP[i].next != null && s2.VP[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.VP[i].next, s2.VP[i].next);
            if (s1.PP != null && s2.PP != null)
                for (int i = 0; i < s1.PP.Count; i++)
                    if (s1.PP[i].next != null && s2.PP[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.PP[i].next, s2.PP[i].next);
            if (s1.ADJP != null && s2.ADJP != null)
                for (int i = 0; i < s1.ADJP.Count; i++)
                    if (s1.ADJP[i].next != null && s2.ADJP[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.ADJP[i].next, s2.ADJP[i].next);
            if (s1.ADVP != null && s2.ADVP != null)
                for (int i = 0; i < s1.ADVP.Count; i++)
                    if (s1.ADVP[i].next != null && s2.ADVP[i].next != null)
                        isSame = isSame && isSamePOSTreeTraversal(s1.ADVP[i].next, s2.ADVP[i].next);
            return isSame;
        }
        //兩個S是否相同(不考慮next的詞彙)
        private static bool isSameS(S s1, S s2)
        {
            if (s1 == null && s2 == null) return true;      //兩個都是null
            if (s1 == null || s2 == null) return false;     //只有一個是null

            if (!isSamePLList(s1.Ss, s2.Ss)) return false;
            if (!isSamePLList(s1.WH, s2.WH)) return false;
            if (!isSamePLList(s1.SQ, s2.SQ)) return false;
            if (!isSamePLList(s1.NP, s2.NP)) return false;
            if (!isSamePLList(s1.VP, s2.VP)) return false;
            if (!isSamePLList(s1.PP, s2.PP)) return false;
            if (!isSamePLList(s1.ADJP, s2.ADJP)) return false;
            if (!isSamePLList(s1.ADVP, s2.ADVP)) return false;
            return true;
        }
        //兩個List<PL>是否相同(不考慮next的詞彙)
        private static bool isSamePLList(List<PL> pl1, List<PL> pl2)
        {
            if (pl1 == null && pl2 == null) return true;    //兩個都是null
            if (pl1 == null || pl2 == null) return false;   //只有一個是null
            if (pl1.Count != pl2.Count) return false;
            for (int i = 0; i < pl1.Count; i++) if (!isSamePL(pl1[i], pl2[i])) return false;
            return true;
        }
        //兩個PL是否相同(不考慮next的詞彙)
        private static bool isSamePL(PL pl1, PL pl2)
        {
            if (pl1.words.Count != pl2.words.Count) return false;   //詞彙數不同
            for (int i = 0; i < pl1.words.Count; i++)
            {
                if (pl1.words[i].word.IndexOf("NNans") == 0)   //找到NNans
                {
                    NNans = pl2.words[i].word;
                    continue;
                }
                if (pl2.words[i].word.IndexOf("NNans") == 0)
                {
                    NNans = pl1.words[i].word;
                    continue;
                }
                if (pl1.words[i].word.ToLower() != pl2.words[i].word.ToLower()) return false;   //詞彙不同
                if (pl1.words[i].pos != pl2.words[i].pos) return false;                         //詞性不同
            }
            return true;
        }

        //取得NNans答案
        public static string getAns(ROOT rootQ, ROOT rootD)
        {
            //假設兩個POSTree除了NNAns以外其他結構均相同，可直接找出NNAns
            Console.WriteLine("isSame: " + isSamePOSTree(rootQ, rootD));
            return NNans;
        }
    }
}
