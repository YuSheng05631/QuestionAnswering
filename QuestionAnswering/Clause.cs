using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Clause
    {
        //從POSTree抓取的詞性順序。e.g. NP + VP + NP
        private static List<string> retrievePOSList = new List<string>();
        //設置retrievePOSList(輸入type)
        private static void setRetrievePOSList(int type)
        {
            retrievePOSList = new List<string>();
            switch (type)
            {
                case 0: //一個動詞
                    setRetrievePOSList("NP,VP,NP");
                    break;
                case 1:
                    setRetrievePOSList("NP,NP,VP,NP");
                    break;
                case 2:
                    setRetrievePOSList("NP,VP,NP,NP");
                    break;
                case 3:
                    setRetrievePOSList("NP,NP,VP,NP,NP");
                    break;
                case 4://兩個動詞
                    setRetrievePOSList("NP,VP,VP,NP");
                    break;
                case 5:
                    setRetrievePOSList("NP,NP,VP,VP,NP");
                    break;
                case 6:
                    setRetrievePOSList("NP,VP,VP,NP,NP");
                    break;
                case 7:
                    setRetrievePOSList("NP,NP,VP,VP,NP,NP");
                    break;
            }
        }
        //設置retrievePOSList(輸入字串)
        private static void setRetrievePOSList(string type)
        {
            retrievePOSList = new List<string>();
            string[] cut = type.Split(',');
            foreach (string c in cut) retrievePOSList.Add(c);
        }

        //取得子句(Start)
        public static List<List<PLAndPOS>> getClauseList(ROOT root, int rType)
        {
            //設置retrievePOSList
            setRetrievePOSList(rType);

            //將POSTree轉換成List<PL>
            List<PLAndPOS> PLList = POSTree.getPLList(root);

            //取得子句，每個子句由List<PLAndPOS>組成
            List<List<PLAndPOS>> clauseList = getClauseListTraversal(PLList, 0, 0);

            //去掉沒有retrievePOSList所有詞性的子句
            for (int i = 0; i < clauseList.Count; i++)
            {
                if (clauseList[i].Count != retrievePOSList.Count)
                {
                    clauseList.RemoveAt(i);
                    i--;
                }
            }
            return clauseList;
        }
        //取得子句(Traversal)
        //PLList: 整個句子的PLAndPOS列表
        //plIndex: 接下來從PLList中第幾個索引開始抓取
        //rIndex: 接下來要抓retrievePOSList中第幾個索引的詞性
        private static List<List<PLAndPOS>> getClauseListTraversal(List<PLAndPOS> PLList, int plIndex, int rIndex)
        {
            List<List<PLAndPOS>> clauseList = new List<List<PLAndPOS>>();
            if (rIndex < retrievePOSList.Count) //需要的詞性還沒有取完
            {
                for (int i = plIndex; i < PLList.Count; i++)
                {
                    //如果目前要取的詞性等於這個PL的詞性，而且這個PL有內容
                    if (PLList[i].pos == retrievePOSList[rIndex] && PLList[i].pl.words.Count != 0)
                    {
                        List<List<PLAndPOS>> tempCluaseList = new List<List<PLAndPOS>>();
                        tempCluaseList.AddRange(getClauseListTraversal(PLList, i + 1, rIndex + 1)); //取得從這個PL開始的子句
                        //next之後沒有詞彙了，加上這個PL
                        if (tempCluaseList.Count == 0)
                        {
                            List<PLAndPOS> tempPP = new List<PLAndPOS>();
                            tempPP.Add(PLList[i]);
                            tempCluaseList.Add(tempPP);
                        }
                        //next之後有詞彙，將每個子句的開頭加上這個PL
                        else for (int j = 0; j < tempCluaseList.Count; j++) tempCluaseList[j].Insert(0, PLList[i]);
                        clauseList.AddRange(tempCluaseList);
                    }
                }
            }
            return clauseList;
        }
        
        //印出子句
        public static void printCluaseList(List<List<PLAndPOS>> clauseList)
        {
            foreach (List<PLAndPOS> clause in clauseList)
            {
                foreach (PLAndPOS plpos in clause)
                {
                    foreach (WordAndPOS wap in plpos.pl.words)
                    {
                        Console.Write(wap.word + " ");
                    }
                    Console.Write(", ");
                }
                Console.WriteLine();
            }
        }
    }
}
