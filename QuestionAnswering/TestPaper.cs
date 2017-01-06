using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace QuestionAnswering
{
    public class Instruction
    {
        public string text, refU;   //參考
        public int refD;            //data編號
        public List<string> ansList;
        public Instruction()
        {
            this.text = "";
            this.refU = "";
            this.refD = 0;
            this.ansList = new List<string>();
        }
    }
    public class Paper
    {
        public List<string> dataList;
        public List<Instruction> instructionList;
        public Paper()
        {
            this.dataList = new List<string>();
            this.instructionList = new List<Instruction>();
        }
    }
    class TestPaper
    {
        //讀取檔案
        private string getAllData(string file)
        {
            string allData = "";
            using (StreamReader reader = new StreamReader(file, true))
            {
                allData = reader.ReadToEnd();
            }
            return allData;
        }
        //轉換填空標籤 e.g. (A) → <B1>
        private string transformSlot(string data)
        {
            int first = 0, last = 0;
            string cut = "";
            first = data.IndexOf("<blank ");
            while (first != -1)
            {
                //找到標籤代號
                first = data.IndexOf("B", first);
                last = data.IndexOf("\"", first);
                cut = data.Substring(first, last - first);
                //找到填空標籤區段
                first = data.LastIndexOf("<blank ", first);
                last = data.IndexOf("</blank>", first) + "</blank>".Length;
                //組合新字串(順便將標籤除掉，雖然之後會由一個function負責做，但為了不多一道手續就在這邊除掉了)
                data = data.Remove(first, last - first);
                data = data.Insert(first, "<" + cut + ">");
                first = data.IndexOf("<blank ", first);
            }
            return data;
        }
        //轉換底線標籤 e.g. (1) → <U1>
        private string transformUtext(string data)
        {
            int first = 0, last = 0;
            string cut = "";
            first = data.IndexOf("<uText ");
            while (first != -1)
            {
                //找到標籤代號
                first = data.IndexOf("U", first);
                last = data.IndexOf("\"", first);
                cut = data.Substring(first, last - first);
                //找到填空標籤區段
                first = data.LastIndexOf("<uText ", first);
                last = data.IndexOf("</uText>", first) + "</uText>".Length;
                //組合新字串(尚未除掉標籤)
                data = data.Insert(last, "</" + cut + ">");
                data = data.Insert(first, "<" + cut + ">");
                first = data.IndexOf("<uText ", last);
            }
            return data;
        }
        //轉換引用標籤 e.g. <ref target="B1"> → <B1>
        private string transformRefInData(string data)
        {
            int first = 0, last = 0;
            string cut = "";
            first = data.IndexOf("<ref ");
            while (first != -1)
            {
                //找到標籤代號
                first = data.IndexOf("\"", first) + 1;
                last = data.IndexOf("\"", first);
                cut = data.Substring(first, last - first);
                //找到填空標籤區段
                first = data.LastIndexOf("<ref ", first);
                last = data.IndexOf("</ref>", first) + "</ref>".Length;
                //組合新字串(順便將標籤除掉，雖然之後會由一個function負責做，但為了不多一道手續就在這邊除掉了)
                data = data.Remove(first, last - first);
                data = data.Insert(first, "<" + cut + ">");
                first = data.IndexOf("<ref ", first);
            }
            return data;
        }
        //去掉Label標籤(包括涵蓋內容)
        private string removeLabelSec(string data)
        {
            int first = 0, last = 0;
            first = data.IndexOf("<label>");
            while (first != -1)
            {
                last = data.IndexOf("</label>", first) + "</label>".Length;
                data = data.Remove(first, last - first);
                first = data.IndexOf("<label>", first);
            }
            return data;
        }
        //去掉標籤(除了填空和底線標籤)
        private string removeLabel(string data)
        {
            int first = 0, last = 0;
            first = data.IndexOf("<");
            while (first != -1)
            {
                last = data.IndexOf(">", first) + 1;
                if (data[first + 1] != 'B' && data[first + 1] != 'U' && data[first + 2] != 'U')
                {
                    data = data.Remove(first, last - first);
                    first = data.IndexOf("<", first);
                }
                else
                {
                    first = data.IndexOf("<", last);
                }
            }
            return data;
        }

        //找到instruction的refU
        private string getRefU(string allData, int iFirst)
        {
            int first = 0, last = 0, i = 0, ct = 0;
            first = allData.LastIndexOf("<ref", iFirst);
            //若ref跟instruction之間有超過1個"\n"就不納入
            string cut = allData.Substring(first, iFirst - first);
            i = cut.IndexOf("\n");
            while (i != -1)
            {
                i = cut.IndexOf("\n", i + 1);
                ct += 1;
            }
            if (ct == 1)
            {
                first = allData.IndexOf("\"", first) + 1;
                last = allData.IndexOf("\"", first);
                return "<" + allData.Substring(first, last - first) + ">";
            }
            return "";
        }
        //找到instruction的refD
        private int getRefD(List<string> dataList, string refU)
        {
            for (int i = 0; i < dataList.Count; i++)
                if (dataList[i].IndexOf(refU) != -1)
                    return i;
            return -1;
        }
        //轉換參考標籤(Instruction用)
        private string transformRef(string data)
        {
            int first = 0, last = 0;
            string cut = "";
            first = data.IndexOf("<ref ");
            while (first != -1)
            {
                //找到標籤代號
                first = data.IndexOf("\"", first) + 1;
                last = data.IndexOf("\"", first);
                cut = data.Substring(first, last - first);
                //找到填空標籤區段
                first = data.LastIndexOf("<ref ", first);
                last = data.IndexOf("</ref>", first) + "</ref>".Length;
                //組合新字串(順便將標籤除掉，雖然之後會由一個function負責做，但為了不多一道手續就在這邊除掉了)
                data = data.Remove(first, last - first);
                data = data.Insert(first, "<" + cut + ">");
                first = data.IndexOf("<ref ", first);
            }
            return data;
        }

        //取得dataList
        private List<string> getDataList(string allData)
        {
            List<string> dataList = new List<string>();
            int first = 0, last = 0;
            Regex rgx = new Regex(@" {2,}");
            first = allData.IndexOf("<data ");
            while (first != -1)
            {
                last = allData.IndexOf("</data>", first);
                string data = allData.Substring(first, last - first);
                data = transformSlot(data);     //轉換填空標籤
                data = transformUtext(data);    //轉換底線標籤
                data = transformRefInData(data);//轉換引用標籤
                data = removeLabelSec(data);    //去掉Label標籤(包括涵蓋內容)
                data = removeLabel(data);       //去掉標籤(除了填空和底線標籤)
                data = rgx.Replace(data, "");   //去掉多餘空白
                dataList.Add(data.Trim());
                first = allData.IndexOf("<data ", last);
            }
            return dataList;
        }
        //取得instructionList
        private List<Instruction> getInstructionList(string allData, List<string> dataList)
        {
            List<Instruction> instructionList = new List<Instruction>();
            int first = 0, last = 0;
            first = allData.IndexOf("<instruction>");
            Regex rgx = new Regex(@" {2,}");
            while (first != -1)
            {
                //如果<instruction>跟上一個<label>之間只有一個"\n"，代表是大題組的instruction，便不納入
                int ct = 0;
                int i = allData.LastIndexOf("<label>[", first);
                string s = allData.Substring(i, first - i);
                i = s.IndexOf("\n");
                while (i != -1)
                {
                    i = s.IndexOf("\n", i + 1);
                    ct += 1;
                }
                last = allData.IndexOf("</instruction>", first);
                if (ct != 1)
                {
                    Instruction tempI = new Instruction();
                    tempI.refU = getRefU(allData, first);       //找到instruction的refU
                    tempI.refD = getRefD(dataList, tempI.refU); //找到instruction的refD

                    string data = allData.Substring(first, last - first);
                    data = transformSlot(data);     //轉換填空標籤
                    data = transformUtext(data);    //轉換底線標籤
                    data = transformRef(data);      //轉換參考標籤
                    data = removeLabelSec(data);    //去掉Label標籤(包括涵蓋內容)
                    data = removeLabel(data);       //去掉標籤(除了填空和底線標籤)
                    data = rgx.Replace(data, "");   //去掉多餘空白
                    tempI.text = data.Trim();

                    instructionList.Add(tempI);
                }
                first = allData.IndexOf("<instruction>", last);
            }
            return instructionList;
        }
        //將填空題加進instructionList
        private List<Instruction> insertBlankIntoInstructionList(List<Instruction> instructionList, List<string> dataList)
        {
            int first = 0, last = 0;
            List<string> hasInserted = new List<string>();  //紀錄加過的標籤，防止重複
            for (int i = 0; i < dataList.Count; i++)
            {
                //找出所有填空標籤，並做成List<Instruction>
                List<Instruction> tempIList = new List<Instruction>();
                first = dataList[i].IndexOf("<B");
                while (first != -1)
                {
                    last = dataList[i].IndexOf(">", first) + 1;
                    string cut = dataList[i].Substring(first, last - first);
                    if (hasInserted.IndexOf(cut) == -1)
                    {
                        Instruction tempI = new Instruction();
                        tempI.text = cut;
                        tempI.refD = i;
                        tempIList.Add(tempI);
                        hasInserted.Add(cut);
                    }
                    first = dataList[i].IndexOf("<B", last);
                }
                //找出insert index
                for (int j = 0; j < instructionList.Count; j++)
                {
                    if (instructionList[j].refD >= i)
                    {
                        instructionList.InsertRange(j, tempIList);
                        break;
                    }
                }
            }
            return instructionList;
        }
        //取得ansList存入instructionList中
        private List<Instruction> getAnsList(string allData, List<Instruction> instructionList)
        {
            //取得所有答案
            List<List<string>> ansList = new List<List<string>>();
            int aFirst = 0, aLast = 0, eFirst = 0, eLast = 0;
            aFirst = allData.IndexOf("<answer_set ");
            while (aFirst != -1)
            {
                List<string> tempList = new List<string>();
                aLast = allData.IndexOf("</answer_set>", aFirst);
                eFirst = allData.IndexOf("<expression>", aFirst);
                while (eFirst != -1 && eFirst < aLast)
                {
                    eFirst += "<expression>".Length;
                    eLast = allData.IndexOf("</expression>", eFirst);
                    string cut = allData.Substring(eFirst, eLast - eFirst);
                    if (cut != "") tempList.Add(cut);
                    eFirst = allData.IndexOf("<expression>", eLast);
                }
                if (tempList.Count != 0) ansList.Add(tempList);
                aFirst = allData.IndexOf("<answer_set ", aLast);
            }
            //將答案各自存入Instruction中
            for (int i = 0; i < instructionList.Count; i++)
            {
                instructionList[i].ansList = ansList[i];
            }
            return instructionList;
        }
        
        //取得paper
        public Paper getPaper(string fileQ, string fileA)
        {
            Paper paper = new Paper();
            string allDataQ = getAllData(fileQ);
            string allDataA = getAllData(fileA);
            paper.dataList = getDataList(allDataQ);
            paper.instructionList = getInstructionList(allDataQ, paper.dataList);
            paper.instructionList = insertBlankIntoInstructionList(paper.instructionList, paper.dataList);
            paper.instructionList = getAnsList(allDataA, paper.instructionList);
            return paper;
        }
        //取得paper(沒有答案)
        public Paper getPaper(string fileQ)
        {
            Paper paper = new Paper();
            string allDataQ = getAllData(fileQ);
            paper.dataList = getDataList(allDataQ);
            paper.instructionList = getInstructionList(allDataQ, paper.dataList);
            paper.instructionList = insertBlankIntoInstructionList(paper.instructionList, paper.dataList);
            return paper;
        }
        //儲存paper方便閱讀
        public void savePaper(Paper paper, string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                int ct = 0;
                foreach (string data in paper.dataList)
                {
                    writer.WriteLine("================ data " + ct + " ================\n");
                    writer.WriteLine(data);
                    writer.WriteLine("\n========================================\n");
                    writer.WriteLine();
                    ct += 1;
                }
                foreach (Instruction ins in paper.instructionList)
                {
                    writer.WriteLine("refD: " + ins.refD);
                    writer.WriteLine("refU: " + ins.refU);
                    writer.WriteLine("text: " + ins.text);
                    foreach (string ans in ins.ansList) writer.WriteLine("ans: " + ans);
                    writer.WriteLine();
                }
            }
        }
        //讀取paper
        public Paper loadPaper(string fileName)
        {
            Paper paper = new Paper();
            string allData = getAllData(fileName);
            int first = 0, last = 0;
            //dataList
            first = allData.IndexOf("================ data ");
            while (first != -1)
            {
                first = allData.IndexOf("\n", first);
                last = allData.IndexOf("========================================", first);
                paper.dataList.Add(allData.Substring(first, last - first).Trim());
                first = allData.IndexOf("================ data ", first);
            }
            //instructionList
            first = allData.IndexOf("refD: ");
            while (first != -1)
            {
                Instruction tempIns = new Instruction();
                //refD
                first += "refD: ".Length;
                last = allData.IndexOf("\n", first);
                tempIns.refD = Convert.ToInt32(allData.Substring(first, last - first));
                //refU
                first = allData.IndexOf("refU: ", first) + "refU: ".Length;
                last = allData.IndexOf("\n", first);
                tempIns.refU = allData.Substring(first, last - first);
                //text
                first = allData.IndexOf("text: ", first) + "text: ".Length;
                last = allData.IndexOf("\n", first);
                tempIns.text = allData.Substring(first, last - first);
                //ans
                List<string> tempAnsList = new List<string>();
                first = allData.IndexOf("ans: ", first) + "ans: ".Length;
                last = allData.IndexOf("\n", first);
                tempAnsList.Add(allData.Substring(first, last - first));
                while (last + 5 < allData.Length && allData.Substring(last + 1, 5) == "ans: ")
                {
                    first = allData.IndexOf("ans: ", first) + "ans: ".Length;
                    last = allData.IndexOf("\n", first);
                    tempAnsList.Add(allData.Substring(first, last - first));
                }
                tempIns.ansList = tempAnsList;
                paper.instructionList.Add(tempIns);
                first = allData.IndexOf("refD: ", first);
            }
            return paper;
        }
    }
}
