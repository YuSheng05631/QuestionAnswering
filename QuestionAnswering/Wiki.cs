using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace QuestionAnswering
{
    //wiki的Infobox
    public class Infobox
    {
        public List<string> infobox;    //若使用getWikiType方法，可能會取得許多詞
        public string birthDate, deathDate, parents, father, mother, children;
        public Infobox()
        {
            infobox = new List<string>();
            birthDate = "";
            deathDate = "";
            parents = "";
            father = "";
            mother = "";
            children = "";
        }
        //是否為人
        public bool isHuman()
        {
            if (infobox.IndexOf("person") != -1 || infobox.IndexOf("people") != -1) return true;
            if (birthDate != "" || deathDate != "" || parents != "" ||
                father != "" || mother != "" || children != "") return true;
            return false;
        }
        //是否為單數
        public bool isSingular()
        {
            if (infobox.IndexOf("people") != -1) return false;
            return true;
        }
    }
    class Wiki
    {
        //取得網頁原始碼
        private static string getAllWebData(string url)
        {
            WebClient client = new WebClient();
            string allWebData = "";
            using (Stream data = client.OpenRead(url))
            {
                using (StreamReader reader = new StreamReader(data, Encoding.GetEncoding("UTF-8")))
                {
                    allWebData = reader.ReadToEnd();
                }
            }
            return allWebData;
        }
        //搜尋Google取得wiki連結
        private static string googleWiki(string keyword)
        {
            string url = @"http://www.google.com.tw/search?hl=zh-TW&q=" + keyword;
            string allWebData = getAllWebData(url); //取得網頁原始碼
            MatchCollection matches = Regex.Matches(allWebData, @"<h3 class=""r""><a.+?href=[\""'](?<href>.+?)[\""'].+?></h3>");

            //擷取出所有連結
            List<string> resultUrl = new List<string>();
            foreach (Match m in matches)
            {
                string cut = m.Value.ToString();
                int first = 0, last = 0;
                first = cut.IndexOf("<a href=\"/url?q=") + 16;
                if (first != -1 + 16)
                {
                    last = cut.IndexOf("&", first);
                    cut = cut.Substring(first, last - first);
                    resultUrl.Add(cut);
                }
            }

            //挑選出wiki連結
            List<string> wikiUrl = new List<string>();
            foreach (string strUrl in resultUrl)
            {
                if (strUrl.Contains("https://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Replace("%25", "%");              //%25在URL Encoding就等於%，搜尋時需先替換
                    wikiUrl.Add(url);
                }
                else if (strUrl.Contains("http://en.wikipedia.org"))
                {
                    if (strUrl.Contains("List_of_")) continue;  //濾掉此類連結
                    url = strUrl;
                    url = url.Insert(24, "wiki/");              //搜尋到的是http而不是https，需修改網址
                    url = url.Replace("%3Ftitle%3D", "");       //去掉%3Ftitle%3D
                    wikiUrl.Add(url);
                }
            }

            //優先選擇wiki標題和keywords完全相符的url
            keyword = keyword.ToLower();
            foreach (string strUrl in wikiUrl)
            {
                string wikiTitle = getWikiTitle(strUrl);     //取得wiki的標題
                wikiTitle = wikiTitle.Replace("_", " ").ToLower();
                if (wikiTitle == keyword) return strUrl;
            }

            if (wikiUrl.Count != 0) return wikiUrl[0];
            else return "";
        }
        //去掉指定標籤內容
        private static string removeLabel(string sentence, string left, string right)
        {
            int first = 0;
            int last = 0;
            while (first != -1 && last != -1)
            {
                first = sentence.IndexOf(left);
                last = sentence.IndexOf(right);
                if (first != -1 && last != -1)
                {
                    if (first < last) //避免因編碼問題而發生錯誤
                    {
                        sentence = sentence.Remove(first, last - first + 1);
                    }
                    else
                    {
                        Console.WriteLine("*因編碼問題，去除" + left + "標籤" + right + "時發生錯誤：" + sentence + "\n");
                        break;
                    }
                }
            }
            return sentence;
        }
        //(無使用)檢查wiki頁面是否存在
        private static bool existPage(string url, string keyword)
        {
            string allWebData = getAllWebData(url);
            if (allWebData.IndexOf("title=\"Category: Disambiguation pages") != -1) return false;
            if (allWebData.IndexOf(keyword + "</a>\" does not exist.") != -1) return false;
            return true;
        }

        #region WikiPLArticle
        //取得wiki上<p>標籤的所有句子
        private static List<string> getWikiData(string url)
        {
            List<string> wikiData = new List<string>();
            string allWebData = getAllWebData(url); //取得網頁原始碼

            //取得所有<p>標籤的句子
            int first = 0, last = 0;
            string cut = "";
            while (first != -1 && last != -1)
            {
                first = allWebData.IndexOf("<p>", last);
                if (first != -1)
                {
                    last = allWebData.IndexOf("</p>", first);
                    cut = allWebData.Substring(first + 3, last - first - 3);
                    cut = removeLabel(cut, "<", ">");   //去掉<標籤>
                    cut = removeLabel(cut, "[", "]");   //去掉<標籤>
                    cut = removeLabel(cut, "(", ")");   //去掉(標籤)
                    if (cut.IndexOf(".") == -1)         //根本不是句子
                    {
                        first = 0;
                        continue;
                    }
                    //以句點分隔段落
                    first = 0;
                    last = 0;
                    while (first != -1 && last != -1)
                    {
                        first = 0;
                        last = cut.IndexOf(". ");
                        if (first != -1 && last != -1)  //此段落擁有兩個句子以上
                        {
                            wikiData.Add(cut.Substring(first, last - first + 1));   //List加進此段落的第一個句子
                            cut = cut.Remove(first, last - first + 2);              //cut刪除此段落的第一個句子
                        }
                        else     //已經是單一個句子
                        {
                            if (cut.Length > 1) wikiData.Add(cut);  //長度小於1的字串不需要加進List
                        }
                    }
                }
            }
            return wikiData;
        }
        //取得wiki的標題
        private static string getWikiTitle(string url)
        {
            int first = url.IndexOf("/wiki/") + 6;
            string cut = url.Substring(first);
            return cut;
        }
        //取得wiki所有句子的PLList，並儲存下來
        private static List<List<PL>> getWikiPLArticle(List<string> wikiData, string wikiTitle)
        {
            List<List<PL>> PLARticle = Sentence.getPLArticle(wikiData);
            SaveData.savePLArticle(PLARticle, wikiTitle);   //儲存PLArticle
            return PLARticle;
        }
        //取得wiki所有句子的PLList，並儲存下來
        public static List<List<PL>> getWikiPLArticle(string keyword)
        {
            string url = googleWiki(keyword);   //搜尋Google取得wiki連結
            if (url == "")      //若搜尋不到wiki，將關鍵字後面加上" wiki"再搜尋一次
            {
                url = googleWiki(keyword + " wiki");
                if (url == "")  //還是找不到
                {
                    Console.WriteLine(keyword + " 找不到wiki。");
                    return new List<List<PL>>();
                }
            }

            string wikiTitle = getWikiTitle(url);   //取得wiki的標題
            List<List<PL>> PLArticle = SaveData.loadPLArticle(wikiTitle);   //載入PLArticle
            if (PLArticle.Count == 0)   //若沒有記錄檔
            {
                List<string> wikiData = getWikiData(url);           //取得wiki上<p>標籤的所有句子
                PLArticle = getWikiPLArticle(wikiData, wikiTitle);  //取得wiki所有句子的PLList，並儲存下來
            }
            return PLArticle;
        }
        #endregion
        
        #region Infobox
        //取得wiki的Infobox
        //wikiTitle: 用以判斷單複數。e.g. Tigris可以找到wiki頁面，代表是單數
        public static Infobox getInfobox(string keyword, out string wikiTitle)
        {
            wikiTitle = "";

            //載入Infobox
            if (isInNoInfobox(keyword))
            {
                //Console.WriteLine(word + " 已記錄在 ((NoInfobox.txt 檔裡。");
                return null;
            }
            Infobox infobox = SaveData.loadInfobox(out wikiTitle, keyword);
            if (infobox != null) return infobox;

            //搜尋Google取得wiki連結
            string url = googleWiki(keyword);
            if (url == "")      //若搜尋不到wiki，將關鍵字後面加上" wiki"再搜尋一次
            {
                url = googleWiki(keyword + " wiki");
                if (url == "")  //還是找不到
                {
                    Console.WriteLine("找不到wiki。");
                    return null;
                }
            }

            //取得wiki的標題
            wikiTitle = getWikiTitle(url);

            //組合Edit頁面網址
            url = "https://en.wikipedia.org/w/index.php?title=" + wikiTitle + "&action=edit";

            //取得網頁原始碼
            string allWebData = getAllWebData(url);

            //取得Infobox區段
            string infoboxSec = getInfoboxSec(allWebData);

            //從infoboxSec取得Infobox資訊
            url = "https://en.wikipedia.org/wiki/" + wikiTitle;
            infobox = getInfoboxFromSec(infoboxSec, url);

            //儲存Infobox
            if (infobox != null) SaveData.saveInfobox(infobox, wikiTitle, keyword);
            else SaveData.saveNoInfobox(keyword);

            //印出Infobox
            //printInfobox(infobox);

            return infobox;
        }
        //是否在NoInfobox檔案裡
        private static bool isInNoInfobox(string word)
        {
            List<string> words = SaveData.loadNoInfobox();
            int index = words.IndexOf(word);
            if (index != -1) return true;
            else return false;
        }
        //取得Infobox區段
        private static string getInfoboxSec(string allWebData)
        {
            //取出Infobox區段
            int firstInfobox = allWebData.IndexOf("{{Infobox");
            if (firstInfobox == -1) firstInfobox = allWebData.IndexOf("{{\nInfobox");
            if (firstInfobox == -1) return null;
            //找到結尾
            int first = allWebData.IndexOf("{{", firstInfobox + 2);
            int last = allWebData.IndexOf("}}", firstInfobox + 2);
            while (first < last && first != -1)
            {
                first = allWebData.IndexOf("{{", first + 2);
                last = allWebData.IndexOf("}}", last + 2);
            }
            return allWebData.Substring(firstInfobox, last - firstInfobox + 2);
        }
        //從infoboxSec取得Infobox資訊
        private static Infobox getInfoboxFromSec(string infoboxSec, string url)
        {
            Infobox infobox = new Infobox();
            if (infoboxSec == null) //若頁面裡沒有infobox，用wikiType代替
            {
                infobox.infobox = getWikiType(url);
                if (infobox.infobox == null) return null;   //也沒有wikiType
                else return infobox;
            }
            else
            {
                infobox.infobox.AddRange(getWikiType(url)); //infobox + wikiType
            }
            infoboxSec = infoboxSec.ToLower();
            infobox.infobox.Add(getInfoboxItem(infoboxSec, "infobox"));
            infobox.birthDate = getInfoboxItem(infoboxSec, "birth_date");
            infobox.deathDate = getInfoboxItem(infoboxSec, "death_date");
            infobox.parents = getInfoboxItem(infoboxSec, "parents");
            infobox.father = getInfoboxItem(infoboxSec, "father");
            infobox.mother = getInfoboxItem(infoboxSec, "mother");
            infobox.children = getInfoboxItem(infoboxSec, "children");
            return infobox;
        }
        //從infoboxSec取得某項資訊
        private static string getInfoboxItem(string infoboxSec, string item)
        {
            int first = 0, last = 0;
            first = infoboxSec.IndexOf(item);
            if (first != -1)
            {
                first = infoboxSec.IndexOf(" ", first + 2) + 1;
                last = infoboxSec.IndexOf("\n", first);
                return infoboxSec.Substring(first, last - first);
            }
            else return "";
        }
        //印出Infobox
        public static void printInfobox(Infobox infobox)
        {
            Console.Write("infobox: ");
            foreach (string s in infobox.infobox) Console.Write(s + ", ");
            Console.WriteLine();
            Console.WriteLine("birthDate: " + infobox.birthDate);
            Console.WriteLine("deathDate: " + infobox.deathDate);
            Console.WriteLine("parents: " + infobox.parents);
            Console.WriteLine("father: " + infobox.father);
            Console.WriteLine("mother: " + infobox.mother);
            Console.WriteLine("children: " + infobox.children);
        }
        #endregion

        #region Category
        //取得wiki或Category頁面的所有Category
        private static List<string> getCategoryList(string allWebData)
        {
            List<string> categoryList = new List<string>();
            int first = 0, last = 0, end = 0;
            first = allWebData.IndexOf("title=\"Help:Category\">Categories</a>:");
            if (first != -1)
            {
                end = allWebData.IndexOf("</ul>", first);   //目錄區塊的結尾位置
                first = allWebData.IndexOf("/wiki/Category:", first);
                while (first != -1)
                {
                    if (first > end) break;     //到達結尾
                    first += "/wiki/Category:".Length;
                    last = allWebData.IndexOf("\" ", first);
                    categoryList.Add(allWebData.Substring(first, last - first));
                    first = allWebData.IndexOf("/wiki/Category:", last);
                }
            }
            return categoryList;
        }
        //取得Category頁面的Subcategory
        private static List<string> getSubcategoryList(string allWebData)
        {
            List<string> subcategoryList = new List<string>();
            int first = 0, last = 0;
            first = allWebData.IndexOf("►");
            while (first != -1)
            {
                first = allWebData.IndexOf("/wiki/Category:", first);
                first += "/wiki/Category:".Length;
                last = allWebData.IndexOf("\">", first);
                subcategoryList.Add(allWebData.Substring(first, last - first));
                first = allWebData.IndexOf("►", last);
            }
            return subcategoryList;
        }
        //根據wikiTitle從categoryList中選擇最相關的目錄
        private static string getMostRelevantCategory(List<string> categoryList, string wikiTitle)
        {
            foreach (string category in categoryList)
                if (category.IndexOf(wikiTitle) != -1)   //目錄包含了Title
                    return category;
            if (categoryList.Count != 0) return categoryList[0];    //回傳第一個
            else return "";
        }

        //取得由下到上的目錄路線
        private static List<string> getCategoryRoute(string keyword)
        {
            string url = googleWiki(keyword);       //搜尋Google取得wiki連結
            string allWebData = getAllWebData(url); //取得網頁原始碼
            string wikiTitle = getWikiTitle(url);   //取得wiki的標題

            //取得wiki或Category頁面的所有Category
            List<string> categoryList = getCategoryList(allWebData);

            //根據wikiTitle從categoryList中選擇最相關的目錄
            string mrCategory = getMostRelevantCategory(categoryList, wikiTitle);

            //向上搜尋
            List<string> categoryRoute = new List<string>();
            while (categoryList.Count != 0)
            {
                if (mrCategory != "")   //只有第一次需要選擇最相關的目錄
                {
                    url = "https://en.wikipedia.org/wiki/Category:" + mrCategory;
                    categoryRoute.Add(mrCategory);
                    mrCategory = "";
                }
                else
                {
                    url = "https://en.wikipedia.org/wiki/Category:" + categoryList[0];  //選擇第一個
                    categoryRoute.Add(categoryList[0]);
                }
                allWebData = getAllWebData(url);    //取得網頁原始碼
                categoryList = getCategoryList(allWebData);

                if (categoryRoute.Count > 100)
                {
                    Console.WriteLine("categoryRoute 可能有迴圈。");
                    break;
                }
            }
            return categoryRoute;
        }
        //取得由下到上的目錄路線，並找到兩個目錄路線的交叉點
        public static void getCross(string keyword1, string keyword2, out List<string> cr1, out List<string> cr2, out int ci1, out int ci2)
        {
            cr1 = Wiki.getCategoryRoute(keyword1);  //取得由下到上的目錄路線
            cr2 = Wiki.getCategoryRoute(keyword2);  //取得由下到上的目錄路線
            ci1 = -1;
            ci2 = -1;
            for (int i = 0; i < cr1.Count; i++)
            {
                int index = cr2.IndexOf(cr1[i]);
                if (index != -1)
                {
                    ci1 = i;
                    ci2 = index;
                    break;
                }
            }
        }

        //取得所有目錄路線
        private static List<List<string>> categoryRouteList = new List<List<string>>();
        private static List<string> categoryVisitedList = new List<string>();
        //取得categoryRouteList(Start)
        public static List<List<string>> getCategoryRouteList(string keyword)
        {
            categoryRouteList = new List<List<string>>();
            categoryVisitedList = new List<string>();
            string url = googleWiki(keyword);       //搜尋Google取得wiki連結
            string allWebData = getAllWebData(url); //取得網頁原始碼

            //取得wiki或Category頁面的所有Category
            List<string> categoryList = getCategoryList(allWebData);

            categoryRouteList.Add(new List<string> { "__Head__" });
            getCategoryRouteListTraversal(0, categoryList);
            return categoryRouteList;
        }
        //取得categoryRouteList(Traversal)
        private static int getCategoryRouteListTraversal(int nowIndex, List<string> categoryList)
        {
            //重複增加現在的路線
            for (int i = 0; i < categoryList.Count - 1; i++)    //若有2個分支，就重複增加1條路線
            {
                //製作要重複的路線(避免指向同個物件)
                List<string> tempList = new List<string>();
                for (int j = 0; j < categoryRouteList[nowIndex].Count; j++)
                    tempList.Add(categoryRouteList[nowIndex][j]);
                categoryRouteList.Insert(nowIndex + 1, tempList);
            }

            //將重複的路線填上各個分支目錄
            for (int i = 0; i < categoryList.Count; i++)
            {
                if (categoryVisitedList.IndexOf(categoryList[i]) == -1) //避免迴圈
                {
                    categoryRouteList[nowIndex + i].Add(categoryList[i]);
                    categoryVisitedList.Add(categoryList[i]);
                }
                else
                {
                    categoryRouteList.RemoveAt(nowIndex + i);   //刪掉重複路線
                    categoryList.RemoveAt(i);                   //刪掉分支目錄
                    i--;
                }
            }

            //Loop
            int offset = 0;
            for (int i = 0; i < categoryList.Count; i++)
            {
                //取得下一層的目錄List
                string url = "https://en.wikipedia.org/wiki/Category:" + categoryList[i];
                string allWebData = getAllWebData(url);
                List<string> nextCategoryList = getCategoryList(allWebData);

                if (nextCategoryList.Count > 0)
                    offset += getCategoryRouteListTraversal(nowIndex + i + offset, nextCategoryList);
            }
            offset += categoryList.Count - 1;
            if (offset < 0) offset = 0;
            return offset;
        }
        #endregion

        #region WikiType(由getInfoboxFromSec呼叫)
        //判斷是否為Be動詞
        private static bool isBeV(string word)
        {
            List<string> beVList = new List<string>() { "is", "are", "was", "were" };
            if (beVList.IndexOf(word) != -1) return true;
            return false;
        }
        //取得類型
        public static List<string> getWikiType(List<PL> PLList)
        {
            //取得S. + be + O. 句型的O. (取得Wiki第一句中所描述的該頁面屬於什麼)
            List<string> wikiTypeList = new List<string>();
            //抓取的詞性順序。e.g. NP + VP + NP
            List<string> retrievePOSList = new List<string>() { "NP", "VP", "NP" }; //phrase level
            int rIndex = 0;
            foreach (PL pl in PLList)
            {
                if (pl.pos == retrievePOSList[rIndex])
                {
                    if (rIndex == 0) rIndex += 1;   //找到主詞
                    else if (rIndex == 1)           //找到動詞
                    {
                        foreach (WordAndPOS wap in pl.words)
                        {
                            if (isBeV(wap.word))    //判斷是否為Be動詞
                            {
                                rIndex += 1;
                                break;
                            }
                        }
                    }
                    else if (pl.words.Count != 0)   //找到受詞
                    {
                        string nouns = "";
                        foreach (WordAndPOS wap in pl.words)    //取出名詞
                            if (wap.pos != "NNP" && wap.pos[0] == 'N')
                                nouns += wap.word + " ";
                        nouns = nouns.Trim();
                        if (nouns != "") wikiTypeList.Add(nouns);
                    }
                }
                else if (rIndex == 2 && pl.words.Count != 0 && pl.words[0].word == ",") //找到受詞後，遇到逗號就停止
                    return wikiTypeList;
            }
            if (wikiTypeList.Count != 0) return wikiTypeList;
            else return null;
        }
        //取得類型
        private static List<string> getWikiType(string url)
        {
            List<string> wikiData = getWikiData(url);   //取得wiki上<p>標籤的所有句子
            //取得第一句的WikiType(有些Wiki擷取出來的第一句不是完整句子，故使用此迴圈)
            foreach (string data in wikiData)
            {
                List<List<PL>> PLArticle = Sentence.getPLArticle(data);
                return getWikiType(PLArticle[0]);
            }
            return null;
        }
        #endregion
    }
}
