using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    public class Inference
    {
        //兩個S是否相同(不考慮next的詞彙)
        private static bool isSameS(S s1, S s2)
        {
            bool isSame = true; //只要其中一個PL不同，整個S都不同
            isSame = isSame && isSamePL(s1.Ss, s2.Ss);
            isSame = isSame && isSamePL(s1.WH, s2.WH);
            isSame = isSame && isSamePL(s1.SQ, s2.SQ);
            isSame = isSame && isSamePL(s1.NP, s2.NP);
            isSame = isSame && isSamePL(s1.VP, s2.VP);
            isSame = isSame && isSamePL(s1.PP, s2.PP);
            return isSame;
        }
        //兩個PL是否相同(不考慮next的詞彙)
        private static bool isSamePL(PL pl1, PL pl2)
        {
            if (pl1 == null && pl2 == null) return true;            //兩個都是null
            if (pl1 == null || pl2 == null) return false;           //只有一個是null
            if (pl1.next.Count != pl2.next.Count) return false;     //next數不同
            if (pl1.words.Count != pl2.words.Count) return false;   //詞彙數不同
            for (int i = 0; i < pl1.words.Count; i++)
            {
                if (pl1.words[i].word != pl2.words[i].word) return false;   //詞彙不同
                if (pl1.words[i].pos != pl2.words[i].pos) return false;     //詞性不同
            }
            return true;
        }
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
            //檢查next的S，經過isSameS(s1, s2)可以確保兩個S的next數一樣多
            if (s1.Ss != null && s2.Ss != null)
                if (s1.Ss.next != null && s2.Ss.next != null)
                    for (int i = 0; i < s1.Ss.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.Ss.next[i], s2.Ss.next[i]);
            if (s1.WH != null && s2.WH != null)
                if (s1.WH.next != null && s2.WH.next != null)
                    for (int i = 0; i < s1.WH.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.WH.next[i], s2.WH.next[i]);
            if (s1.SQ != null && s2.SQ != null)
                if (s1.SQ.next != null && s2.SQ.next != null)
                    for (int i = 0; i < s1.SQ.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.SQ.next[i], s2.SQ.next[i]);
            if (s1.NP != null && s2.NP != null)
                if (s1.NP.next != null && s2.NP.next != null)
                    for (int i = 0; i < s1.NP.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.NP.next[i], s2.NP.next[i]);
            if (s1.VP != null && s2.VP != null)
                if (s1.VP.next != null && s2.VP.next != null)
                    for (int i = 0; i < s1.VP.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.VP.next[i], s2.VP.next[i]);
            if (s1.PP != null && s2.PP != null)
                if (s1.PP.next != null && s2.PP.next != null)
                    for (int i = 0; i < s1.PP.next.Count; i++)
                        isSame = isSame && isSamePOSTreeTraversal(s1.PP.next[i], s2.PP.next[i]);
            return isSame;
        }


        //取得NNans答案
        public static string getAns(ROOT rootQ, ROOT rootD)
        {
            string ans = "";
            


            return ans;
        }
    }
}
