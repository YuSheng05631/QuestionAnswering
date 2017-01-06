using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Preprocessing
    {
        #region transformPassive
        //被動語句轉換
        public List<PL> transformPassive(List<PL> PLList)
        {
            //條件: VBN + by (中間無N)
            bool hasVBN = false, hasBy = false;
            int sIndex = 0, vIndex = 0, vIndent = 0, byIndex = 0, oIndex = 0;
            //尋找被動語句
            for (int i = 0; i < PLList.Count; i++)
            {
                if (!hasVBN && PLList[i].pos == "VP")
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        if (wap.pos == "VBN") hasVBN = true;    //找到VBN
                }
                else if (hasVBN && PLList[i].pos == "NP")       //找到by之前遇到名詞，不算是被動語句
                {
                    hasVBN = false;
                }
                else if (hasVBN && PLList[i].pos == "PP")
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        if (wap.word == "by")
                        {
                            hasBy = true;     //找到by
                            byIndex = i;
                        }
                }
                if (hasVBN && hasBy) break;
            }
            //找到被動語句
            if (hasVBN && hasBy)
            {
                //從by開始向前找到第一個VP
                for (int j = byIndex - 1; j >= 0; j--)
                {
                    if (PLList[j].pos != "VP" && PLList[j].pos != "ADVP")   //副詞例外
                    {
                        vIndex = j + 1;
                        vIndent = PLList[j + 1].indent;
                        break;
                    }
                }
                //向前找到與第一個VP同縮排的PL
                for (int j = vIndex - 1; j >= 0; j--)
                {
                    if (PLList[j].indent < vIndent)
                    {
                        sIndex = j + 1;
                        break;
                    }
                }
                //向後找到與第一個VP縮排還小的PL
                for (int j = vIndex + 1; j < PLList.Count; j++)
                {
                    if (PLList[j].indent <= vIndent)
                    {
                        oIndex = j - 1;
                        break;
                    }
                }
                PLList = exchangePassive(PLList, sIndex, vIndex, byIndex, oIndex);  //主詞受詞交換
                PLList = transformPassive(PLList);  //可能不只有一個被動語句
            }
            return PLList;
        }
        //主詞受詞交換: (sIndex ~ vIndex - 1) ←→ (byIndex + 1 ~ oIndex)
        private List<PL> exchangePassive(List<PL> PLList, int sIndex, int vIndex, int byIndex, int oIndex)
        {
            string newSentence = "";
            //原主詞前
            for (int i = 0; i <= sIndex - 1; i++)
                foreach (WordAndPOS wap in PLList[i].words)
                    newSentence += wap.word + " ";
            //原受詞
            for (int i = byIndex + 1; i <= oIndex; i++)
                foreach (WordAndPOS wap in PLList[i].words)
                    newSentence += wap.word + " ";
            //原動詞
            for (int i = vIndex; i <= byIndex - 1; i++)
                foreach (WordAndPOS wap in PLList[i].words)
                    if (!isBe(wap.word))
                        newSentence += wap.word + " ";
            //原主詞
            for (int i = sIndex; i <= vIndex - 1; i++)
                foreach (WordAndPOS wap in PLList[i].words)
                    newSentence += wap.word + " ";
            //原受詞後
            for (int i = oIndex + 1; i < PLList.Count; i++)
                foreach (WordAndPOS wap in PLList[i].words)
                    newSentence += wap.word + " ";

            Sentence sen = new Sentence();
            return sen.getPLArticle(newSentence.Trim())[0];
        }
        //判斷是否為be動詞
        private bool isBe(string word)
        {
            List<string> beList = new List<string>() { "am", "is", "are", "was", "were", "be", "been" };
            if (beList.IndexOf(word) != -1) return true;
            else return false;
        }
        #endregion

        #region transformOf
        //of名詞轉換
        public List<PL> transformOf(List<PL> PLList)
        {
            //找出所有of的index
            List<int> ofIndexList = new List<int>();
            for (int i = 0; i < PLList.Count; i++)
            {
                foreach (WordAndPOS wap in PLList[i].words)
                    if (wap.word == "of" && PLList[i - 1].pos == "NP" && PLList[i + 1].pos == "NP")
                        ofIndexList.Add(i);
            }
            if (ofIndexList.Count == 0) return PLList;  //沒有of
            //前後名詞交換
            string newSentence = "";
            for (int i = 0; i < PLList.Count; i++)
            {
                if (ofIndexList.IndexOf(i + 1) != -1)   //是of前面的名詞
                {
                    foreach (WordAndPOS wap in PLList[i + 2].words)
                        newSentence += wap.word + " ";
                    foreach (WordAndPOS wap in PLList[i].words)
                        newSentence += wap.word + " ";
                    i += 2;
                }
                else
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        newSentence += wap.word + " ";
                }
            }
            Sentence sen = new Sentence();
            return sen.getPLArticle(newSentence)[0];
        }
        #endregion

        #region transformWH
        //where語句轉換
        //原句擴增
        public List<PL> transformWhere(List<PL> PLList)
        {
            //條件: where + S (中間無其他詞性)
            List<int> whereIndexList = new List<int>();
            bool hasWhere = false;
            //找出所有where的index
            for (int i = 0; i < PLList.Count; i++)
            {
                if (!hasWhere && PLList[i].pos.Length > 0 && PLList[i].pos[0] == 'W')
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        if (wap.word == "where") hasWhere = true;   //找到where
                }
                else if (hasWhere && PLList[i].pos == "S")  //找到S
                {
                    whereIndexList.Add(i - 1);
                    hasWhere = false;
                }
                else if (hasWhere && PLList[i].pos != "S")  //where之後的詞性不是S
                {
                    hasWhere = false;
                }
            }
            if (whereIndexList.Count == 0) return PLList;   //沒有where
            //組成新句子
            string newSentence = "";
            for (int i = 0; i < PLList.Count; i++)
            {
                if (whereIndexList.IndexOf(i) != -1)
                {
                    int nIndex = 0, lIndex = 0;
                    //向前找到第一個NP
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (PLList[j].pos == "NP" && PLList[j].words.Count > 0 /*&& PLList[j].indent <= PLList[i].indent*/)  //可濾掉同謂語
                        {
                            nIndex = j;
                            break;
                        }
                    }
                    //向後找到與where縮排還小的PL
                    for (int j = i + 1; j < PLList.Count; j++)
                    {
                        if (PLList[j].indent < PLList[i].indent)
                        {
                            lIndex = j - 1;
                            break;
                        }
                    }
                    //組成新句子
                    for (int j = i + 1; j <= lIndex; j++)
                        foreach (WordAndPOS wap in PLList[j].words)
                            newSentence += wap.word + " ";
                    newSentence += "in ";
                    foreach (WordAndPOS wap in PLList[nIndex].words)
                        newSentence += wap.word + " ";
                    i = lIndex;
                }
                else
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        newSentence += wap.word + " ";
                }
            }
            Sentence sen = new Sentence();
            return sen.getPLArticle(newSentence.Trim())[0];
        }
        //產生子句
        /*public List<List<PL>> transformWhere(List<PL> PLList)
        {
            //條件: where + S (中間無其他詞性)
            List<int> whereIndexList = new List<int>();
            bool hasWhere = false;
            //找出所有where的index
            for (int i = 0; i < PLList.Count; i++)
            {
                if (!hasWhere && PLList[i].pos.Length > 0 && PLList[i].pos[0] == 'W')
                {
                    foreach (WordAndPOS wap in PLList[i].words)
                        if (wap.word == "where") hasWhere = true;   //找到where
                }
                else if (hasWhere && PLList[i].pos == "S")  //找到S
                {
                    whereIndexList.Add(i - 1);
                    hasWhere = false;
                }
                else if (hasWhere && PLList[i].pos != "S")  //where之後的詞性不是S
                {
                    hasWhere = false;
                }
            }
            if (whereIndexList.Count == 0) return new List<List<PL>>(); //沒有where
            //產生where子句
            List<List<PL>> clauses = new List<List<PL>>();
            foreach (int whereIndex in whereIndexList)
            {
                clauses.Add(generateWhereClause(PLList, whereIndex));
            }
            return clauses;
        }*/
        //產生where子句
        /*private List<PL> generateWhereClause(List<PL> PLList, int whereIndex)
        {
            int nIndex = 0, lIndex = 0;
            //向前找到第一個NP
            for (int i = whereIndex - 1; i >= 0; i--)
            {
                if (PLList[i].pos == "NP")
                {
                    nIndex = i;
                    break;
                }
            }
            //向後找到與where縮排還小的PL
            for (int i = whereIndex + 1; i < PLList.Count; i++)
            {
                if (PLList[i].indent < PLList[whereIndex].indent)
                {
                    lIndex = i - 1;
                    break;
                }
            }
            //組合新字串
            string newSentence = "";
            for (int i = whereIndex + 1; i <= lIndex; i++)
            {
                foreach (WordAndPOS wap in PLList[i].words)
                    if (wap.word != "where")
                        newSentence += wap.word + " ";
            }
            newSentence += "in ";
            for (int i = nIndex; i <= whereIndex - 1; i++)
            {
                foreach (WordAndPOS wap in PLList[i].words)
                    if (wap.word != "where")
                        newSentence += wap.word + " ";
            }
            return Sentence.getPLArticle(newSentence.Trim())[0];
        }*/
        #endregion

        #region transformAppositive
        //同位語轉換
        public List<PL> transformAppositive(List<PL> PLList)
        {


            return null;
        }
        #endregion

        //進行所有的前處理
        public List<PL> preprocessing(List<PL> PLList)
        {
            if (PLList == null) return PLList;
            PLList = transformPassive(PLList);
            PLList = transformOf(PLList);
            PLList = transformWhere(PLList);
            return PLList;
        }
        public List<List<PL>> preprocessing(List<List<PL>> PLArticle)
        {
            for (int i = 0; i < PLArticle.Count; i++)
            {
                if (PLArticle[i] == null) continue;
                PLArticle[i] = preprocessing(PLArticle[i]);
            }
            return PLArticle;
        }
    }
}
