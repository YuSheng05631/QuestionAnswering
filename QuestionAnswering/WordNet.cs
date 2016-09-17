using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace QuestionAnswering
{
    public class WordNetResult
    {
        public int frequencyCounts, lexicalFileNumbers;
        public string lexicalFileInfo;
        public WordNetResult()
        {
            this.frequencyCounts = 0;
            this.lexicalFileInfo = "";
            this.lexicalFileNumbers = 0;
        }
    }
    class WordNet
    {
        //取得網頁原始碼
        private static string getAllWebData(string word)
        {
            string url = @"http://wordnetweb.princeton.edu/perl/webwn?s=" + word + "&o2=1&o4=1&o5=1&o0=&o1=&o3=&o6=&o7=&o8=&o9=";
            string allWebData = "";
            WebClient client = new WebClient();
            using (Stream data = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                {
                    allWebData = reader.ReadToEnd();
                }
            }
            return allWebData;
        }
        //取得出原始碼中每個辭意解釋
        private static List<string> getLiList(string allWebData)
        {
            List<string> liList = new List<string>();
            int first = 0, last = 0;
            first = allWebData.IndexOf("<li>");
            while (first != -1)
            {
                last = allWebData.IndexOf("</li>", first) + 5;
                liList.Add(allWebData.Substring(first, last - first));
                first = allWebData.IndexOf("<li>", last);
            }
            return liList;
        }
        //取得WordNetResultList
        private static List<WordNetResult> getWordNetResultList(List<string> liList)
        {
            List<WordNetResult> wnrList = new List<WordNetResult>();
            foreach (string li in liList)
            {
                WordNetResult wnr = new WordNetResult();
                int first = 0, last = 0;
                //Frequency Counts
                first = li.IndexOf("<li>(");
                if (first != -1)
                {
                    first += 5;
                    last = li.IndexOf(")", first);
                    wnr.frequencyCounts = Convert.ToInt32(li.Substring(first, last - first));
                }
                //Lexical File Info
                first = li.IndexOf("&lt;");
                if (first != -1)
                {
                    first += 4;
                    last = li.IndexOf("&gt;", first);
                    wnr.lexicalFileInfo = li.Substring(first, last - first);
                }
                //Lexical File Numbers
                first = li.IndexOf("[");
                if (first != -1)
                {
                    first += 1;
                    last = li.IndexOf("]", first);
                    wnr.lexicalFileNumbers = Convert.ToInt32(li.Substring(first, last - first));
                }
                wnrList.Add(wnr);
            }
            return wnrList;
        }

        //印出WordNetResultList
        public static void printWordNetResultList(List<WordNetResult> wnrList)
        {
            foreach (WordNetResult wnr in wnrList)
            {
                Console.Write("Frequency Counts: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(wnr.frequencyCounts);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(",  ");

                Console.Write("Lexical File Info: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(wnr.lexicalFileInfo);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(",  ");

                Console.Write("Lexical File Numbers: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(wnr.lexicalFileNumbers);
                Console.ForegroundColor = ConsoleColor.Gray;

            }
        }

        //取得WordNetResultList
        public static List<WordNetResult> getWordNetResultList(string word)
        {
            //取得網頁原始碼
            string allWebData = getAllWebData(word);

            //取得原始碼中每個辭意解釋
            List<string> liList = getLiList(allWebData);

            //取得WordNetResultList
            List<WordNetResult> wnrList = getWordNetResultList(liList);

            return wnrList;
        }
    }
}
