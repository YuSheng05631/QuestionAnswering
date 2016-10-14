using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Clause
    {
        //判斷兩個詞是否同根
        public static bool isDerivative(string w1, string w2)
        {
            //先經過Stem
            w1 = Stem.getStem(w1);
            w2 = Stem.getStem(w2);
            //找出最大連續相同數。 e.g. happy & happiness: 4，apple & axxxe: 1
            int maxSame = -1;
            for (int i = 0; i < w1.Length; i++)
            {
                for (int j = 0; j < w2.Length; j++)
                {
                    int ct = 0;
                    if (w1[i] == w2[j]) ct += 1;
                    while (true)
                    {
                        if (i + ct >= w1.Length || j + ct >= w2.Length) break;
                        if (w1[i + ct] != w2[j + ct]) break;
                        ct += 1;
                    }
                    if (ct != 0 && maxSame < ct) maxSame = ct;
                }
            }
            if (w1.Length - maxSame <= 1 && w2.Length - maxSame <= 1) return true;
            return false;
        }
        //判斷是否為需要的詞性
        private static bool isNeedPOS(WordAndPOS wap)
        {
            if (wap.pos[0] == 'N' || wap.pos[0] == 'V' || wap.pos.IndexOf("JJ") == 0) return true;
            else return false;
        }
        //判斷兩個WordAndPOS是否關聯
        private static bool isRelevant(WordAndPOS wap1, WordAndPOS wap2)
        {
            if (Stem.getStem(wap1.word) == Stem.getStem(wap2.word)) return true;    //詞相同
            else if (wap1.word.IndexOf("NNans") == 0 && wap2.pos[0] == 'N') return true;
            else if (wap1.pos[0] == wap2.pos[0])    //詞性相同
            {
                return Thesaurus.hasAntonym(wap1.word, wap2.word);  //判斷是否為同義詞
            }
            else    //詞性不同
            {
                return isDerivative(wap1.word, wap2.word);  //判斷兩個詞是否同根
            }
        }
        public static void match(List<PL> PLList1, List<PL> PLList2)
        {
            List<WordAndPOS> matchList1 = new List<WordAndPOS>();
            List<WordAndPOS> matchList2 = new List<WordAndPOS>();
            int pointer = 0;
            foreach (PL pl in PLList1)
            {
                foreach (WordAndPOS wap1 in pl.words)
                {
                    if (isNeedPOS(wap1))    //判斷是否為需要的詞性
                    {
                        for (int i = pointer; i < PLList2.Count; i++)
                        {
                            bool isR = false;
                            foreach (WordAndPOS wap2 in PLList2[i].words)
                            {
                                if (matchList2.IndexOf(wap2) != -1) continue;   //wap2沒有加入過
                                if (isNeedPOS(wap2))    //判斷是否為需要的詞性
                                {
                                    isR = isRelevant(wap1, wap2);   //判斷兩個WordAndPOS是否關聯
                                    if (isR)
                                    {
                                        matchList1.Add(wap1);
                                        matchList2.Add(wap2);
                                        pointer = i;
                                        break;
                                    }
                                }
                            }
                            if (isR) break;
                        }
                    }
                }
            }

            foreach (WordAndPOS wap in matchList1) Console.Write(wap.word + ", ");
            Console.WriteLine();
            foreach (WordAndPOS wap in matchList2) Console.Write(wap.word + ", ");
            Console.WriteLine();
        }
    }
}
