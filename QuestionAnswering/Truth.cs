using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Truth
    {
        //複製MTList
        private static List<MainTerm> copyMTList(List<MainTerm> MTList)
        {
            List<MainTerm> MTListCopy = new List<MainTerm>();
            foreach (MainTerm mt in MTList)
            {
                MainTerm MTTemp = new MainTerm();
                MTTemp.W.AddRange(mt.W);
                MTTemp.SQ.AddRange(mt.SQ);
                MTTemp.S.AddRange(mt.S);
                MTTemp.O.AddRange(mt.O);
                MTTemp.V.AddRange(mt.V);
                MTTemp.VBN.AddRange(mt.VBN);
                MTTemp.PP.AddRange(mt.PP);
                MTListCopy.Add(MTTemp);
            }
            return MTListCopy;
        }
        //取得事實
        public static List<MainTerm> getTruth(List<MainTerm> MTList)
        {
            //將MTList調整成事實句truthList
            List<MainTerm> truthList = copyMTList(MTList);
            List<MainTerm> newTruthList = new List<MainTerm>();
            for (int i = 0; i < truthList.Count; i++)
            {
                //若第一句W有兩個詞以上，轉換為一個新事實
                //ex. Which language... > *ans* is language
                if (i == 0 && truthList[i].W.Count != 0 && truthList[i].W[0].IndexOf(" ") != -1)
                {
                    if (truthList[i].W.Count > 1) Console.WriteLine("Warning1: 第" + (i + 1) + "句有兩個W");
                    int index = truthList[i].W[0].IndexOf(" "); //只取W[0](暫)
                    string cut = truthList[i].W[0].Substring(index + 1);
                    MainTerm tempMT = new MainTerm();
                    tempMT.S.Add("*ans*");
                    tempMT.V.Add("is");
                    tempMT.O.Add(cut);
                    newTruthList.Add(tempMT);
                }
                //若第一句有SQ和V，將O以*ans*標記
                //(有V)ex. What do you mean? > you mean *ans*
                //(無V)ex. What is the name of the cat? > *ans* is name of the cat
                if (i == 0 && truthList[i].SQ.Count != 0 && truthList[i].V.Count != 0)
                {
                    truthList[i].O.Add("*ans*");
                }
                //若第一句有SQ、沒有V，將SQ移到V，將S移到O
                else if (i == 0 && truthList[i].SQ.Count != 0 && truthList[i].V.Count == 0)
                {
                    truthList[i].V.AddRange(truthList[i].SQ);
                    truthList[i].SQ = new List<string>();
                    truthList[i].O.AddRange(truthList[i].S);
                    truthList[i].S = new List<string>();
                }
                //若第一句沒有S，則將S以*ans*標記(檢查有無W)
                //ex. What is your name? > *ans* is your name
                if (i == 0 && truthList[i].S.Count == 0)
                {
                    if (truthList[i].W.Count == 0) Console.WriteLine("Warning2: 第" + (i + 1) + "句沒有W和S");
                    truthList[i].S.Add("*ans*");
                }
                //若其他句沒有S，則將S設為前一句的O，若前一句沒有O，則將S設為前一句的S(檢查有無W)
                else if (truthList[i].S.Count == 0)
                {
                    if (truthList[i].W.Count == 0) Console.WriteLine("Warning3: 第" + (i + 1) + "句沒有W和S");
                    if (truthList[i - 1].O.Count == 0) Console.WriteLine("Warning4: 第" + i + "句沒有O");
                    int ctO = truthList[i - 1].O.Count;
                    int ctS = truthList[i - 1].S.Count;
                    if (ctO != 0) truthList[i].S.Add(truthList[i - 1].O[ctO - 1]);
                    else if (ctS != 0) truthList[i].S.Add(truthList[i - 1].S[ctS - 1]);
                }
                //若其他句有W和S，

                //將有VBN的句子S、O調換位置，將VBN取代V(暫)
                //ex. The cat was adopted by Max. > Max adopted The cat.
                if (truthList[i].VBN.Count != 0)
                {
                    List<string> tempList = new List<string>();
                    foreach (string str in truthList[i].S) tempList.Add(str);
                    truthList[i].S = new List<string>();
                    foreach (string str in truthList[i].O) truthList[i].S.Add(str);
                    truthList[i].O = new List<string>();
                    foreach (string str in tempList) truthList[i].O.Add(str);
                    truthList[i].V = new List<string>();
                    foreach (string str in truthList[i].VBN) truthList[i].V.Add(str);
                    truthList[i].VBN = new List<string>();
                }
            }
            truthList.AddRange(newTruthList);   //加入新事實

            return truthList;
        }
    }
}
