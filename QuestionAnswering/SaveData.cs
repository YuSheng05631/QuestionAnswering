﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;  //PM> Install-Package Newtonsoft.Json

namespace QuestionAnswering
{
    public class SaveData
    {
        //儲存POSTree(List)
        public static void savePOSTree(List<ROOT> rootList, string fileName)
        {
            using (StreamWriter writer = new StreamWriter("SavePOSTree\\" + fileName + ".txt"))
            {
                //刷新紀錄檔
            }
            using (StreamWriter writer = new StreamWriter("SavePOSTree\\" + fileName + ".txt", true))
            {
                foreach (ROOT root in rootList)
                {
                    string saveData = JsonConvert.SerializeObject(root);
                    writer.WriteLine(saveData);
                }
            }
        }
        //載入POSTree(List)
        public static List<ROOT> loadPOSTree(string fileName)
        {
            string saveData = "";
            List<ROOT> rootList = new List<ROOT>();
            try
            {
                using (StreamReader reader = new StreamReader("SavePOSTree\\" + fileName + ".txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        saveData = reader.ReadLine();
                        rootList.Add(JsonConvert.DeserializeObject<ROOT>(saveData));
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("找不到 " + fileName + ".txt 記錄檔。");
            }
            return rootList;
        }
    }
}