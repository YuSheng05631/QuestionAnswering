using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Arrow
    {
        public string relation;
        public Circle tip;
        public Arrow(string relation, Circle tip)
        {
            this.relation = relation;
            this.tip = tip;
        }
    }
    class Circle
    {
        public string id, word, pos;
        public List<Arrow> next, prev;
        public Circle()
        {
            this.id = "";
            this.word = "";
            this.pos = "";
            this.next = new List<Arrow>();
            this.prev = new List<Arrow>();
        }
        public Circle(string id, string word, string pos)
        {
            this.id = id;
            this.word = word;
            this.pos = pos;
            this.next = new List<Arrow>();
            this.prev = new List<Arrow>();
        }
        public void print()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("id: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(this.id);
            Console.Write(", ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("word: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(this.word);
            Console.Write(", ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("pos: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(this.pos);
            Console.Write(", ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("next: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            foreach (Arrow arrow in this.next) Console.Write(arrow.tip.word + ", ");
            Console.Write("], ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("prev: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("[");
            foreach (Arrow arrow in this.prev) Console.Write(arrow.tip.word + ", ");
            Console.WriteLine("], ");
        }
    }
    class SemanticRelationMap
    {
        public Dictionary<string, Circle> idDict;               //{id: circle}
        public Dictionary<string, List<Circle>> headerTable;    //{word: circles}
        private Dictionary<int, List<Circle>> circleDict;       //{pl.index: circles} 每當加入一個句子時更新一次
        private int sn;     //Serial number of sentences.
        public SemanticRelationMap()
        {
            this.idDict = new Dictionary<string, Circle>();
            this.headerTable = new Dictionary<string, List<Circle>>();
        }

        //判斷PL中是否有指定詞性(以開頭判斷)
        private bool hasPOS(PL pl, string pos)
        {
            foreach (WordAndPOS wap in pl.words)
                if (wap.pos.IndexOf(pos) == 0)
                    return true;
            return false;
        }
        //取出PL中所有指定詞性的WordAndPOS(以等於判斷)
        private List<WordAndPOS> getPOSWapListByEqual(PL pl, string pos)
        {
            List<WordAndPOS> wapList = new List<WordAndPOS>();
            foreach (WordAndPOS wap in pl.words)
                if (wap.pos == pos)
                    wapList.Add(wap);
            return wapList;
        }
        //取出PL中所有指定詞性的WordAndPOS(以開頭判斷)
        private List<WordAndPOS> getPOSWapListByHead(PL pl, string pos)
        {
            List<WordAndPOS> wapList = new List<WordAndPOS>();
            foreach (WordAndPOS wap in pl.words)
                if (wap.pos.IndexOf(pos) == 0)
                    wapList.Add(wap);
            return wapList;
        }
        //將wapList的詞彙合併成字串
        private string getWordsString(List<WordAndPOS> wapList)
        {
            string s = "";
            foreach (WordAndPOS wap in wapList)
                s += wap.word + " ";
            return s.TrimEnd();
        }

        //判斷一個詞的詞性屬於NNP、NP、VP、JJ、CD或逗號
        private string getNeedPOS(WordAndPOS wap)
        {
            if (wap.pos == "") return "";
            else if (wap.pos.IndexOf("NNP") == 0) return "NNP";
            else if (wap.pos == "CD") return "CD";

            char c = wap.pos[0];
            if (c == 'N') return "NP";
            else if (c == 'V') return "VP";
            else if (c == 'J') return "JJ";
            else if (c == ',') return ",";
            return "";
        }
        //切割PL成數個詞以供作成Circle
        private List<WordAndPOS> cutPL(PL pl)
        {
            List<WordAndPOS> wapList = new List<WordAndPOS>();
            WordAndPOS tempWAP = new WordAndPOS();
            string nowPos;
            foreach (WordAndPOS wap in pl.words)
            {
                nowPos = getNeedPOS(wap);
                if (nowPos != "")
                {
                    if (tempWAP.pos == "" && nowPos != ",")
                    {
                        tempWAP.word = wap.word;
                        tempWAP.pos = nowPos;
                    }
                    else if (nowPos[0] == 'N' && tempWAP.pos[0] == 'N')   //名詞合併，其餘分開
                    {
                        tempWAP.word += " " + wap.word;
                    }
                    else if (nowPos != ",")
                    {
                        wapList.Add(tempWAP);
                        tempWAP = new WordAndPOS(wap.word, nowPos);
                    }
                    else
                    {
                        if (tempWAP.word != "") wapList.Add(tempWAP);
                        tempWAP = new WordAndPOS(); //逗號不做成Circle(用來分割連詞)
                    }
                }
            }
            if (tempWAP.word != "") wapList.Add(tempWAP);
            return wapList;
        }
        //將句子的所有詞做成Circle(不處理next)，回傳字典{PL編號: PL中的數個Cirlce}
        private Dictionary<int, List<Circle>> generateCircles(List<PL> PLList)
        {
            circleDict = new Dictionary<int, List<Circle>>();
            Circle tempCircle;
            List<WordAndPOS> tempWAPList;
            int ctPL = 0, ctWA = 0;
            foreach (PL pl in PLList)
            {
                if (pl.words.Count != 0)
                {
                    ctWA = 0;
                    tempWAPList = cutPL(pl);    //切割PL成數個詞以供作成Circle
                    circleDict[ctPL] = new List<Circle>();
                    foreach (WordAndPOS wap in tempWAPList)
                    {
                        //做成Circle
                        tempCircle = new Circle(string.Format("{0}-{1}-{2}", this.sn, ctPL, ctWA), wap.word, wap.pos);
                        //加入circleDict
                        circleDict[ctPL].Add(tempCircle);
                        //加入idDict
                        this.idDict[tempCircle.id] = tempCircle;
                        //加入headerTable (需改良)
                        if (tempCircle.pos == "NNP")
                        {
                            if (this.headerTable.ContainsKey(tempCircle.word))
                            {
                                this.headerTable[tempCircle.word].Add(tempCircle);
                            }
                            else
                            {
                                this.headerTable[tempCircle.word] = new List<Circle>() { tempCircle };
                            }
                        }
                        ctWA += 1;
                    }
                }
                ctPL += 1;
            }
            return circleDict;
        }
        //連接兩個Circle
        private void connectTwoCircles(Circle c1, Circle c2, string relation)
        {
            c1.next.Add(new Arrow(relation, c2));
            c2.prev.Add(new Arrow(relation, c1));
        }

        #region Connect circles (rely on POS tree)

        #region wh-Processing
        
        //處理which / whom / that
        private void whichProcessing(PL pointerPL, ref bool stop)
        {
            //當名詞碰上which，代表會是 NP1 + which + NP2 + VP 的結構
            //VP 必須指向 NP1
            Circle circleNP1 = new Circle(), circleVP = new Circle();
            //抓出NP1
            bool stopWh = false;
            PL p = pointerPL;       //現在pointerPL指向的是which
            while (!stopWh)         //scan prev
            {
                while(!stopWh)      //scan brotherY
                {
                    if (circleDict.ContainsKey(p.index) && p != pointerPL)
                        foreach (Circle circle in circleDict[p.index])
                            if (circle.pos[0] == 'N')
                            {
                                circleNP1 = circle;
                                stopWh = true;
                            }
                    if (p.brotherY == null) break;
                    p = p.brotherY;
                }
                if (p.prev == null) break;
                p = p.prev;
            }
            //抓出VP
            stopWh = false;
            p = pointerPL;                          //現在pointerPL指向的是which
            if (p.next.Count > 0) p = p.next[0];    //避免有些POS tree結構不一致
            while (!stopWh)         //scan next
            {
                while (!stopWh)     //scan brotherO
                {
                    if (circleDict.ContainsKey(p.index))
                        foreach (Circle circle in circleDict[p.index])
                            if (circle.pos[0] == 'V')
                            {
                                circleVP = circle;
                                stopWh = true;
                            }
                    if (p.brotherO == null) break;
                    p = p.brotherO;
                }
                if (p.next.Count == 0) break;
                p = p.next[0];
            }
            connectTwoCircles(circleVP, circleNP1, "o");
            stop = true;
        }
        //處理where
        private void whereProcessing(PL pointerPL, ref bool stop)
        {
            //當名詞碰上where，代表會是 NP1 + where + NP2 + VP + NP3 的結構
            //NP3 必須指向 NP1
            Circle circleNP1 = new Circle(), circleNP3 = new Circle();
            //抓出NP1
            bool stopWh = false;
            PL p = pointerPL;       //現在pointerPL指向的是where
            while (!stopWh)         //scan prev
            {
                while (!stopWh)     //scan brotherY
                {
                    if (circleDict.ContainsKey(p.index) && p != pointerPL)
                        foreach (Circle circle in circleDict[p.index])
                            if (circle.pos[0] == 'N')
                            {
                                circleNP1 = circle;
                                stopWh = true;
                            }
                    if (p.brotherY == null) break;
                    p = p.brotherY;
                }
                if (p.prev == null) break;
                p = p.prev;
            }
            //抓出NP3
            bool hasVP = false;
            stopWh = false;
            p = pointerPL;                          //現在pointerPL指向的是where
            if (p.next.Count > 0) p = p.next[0];    //避免有些POS tree結構不一致
            while (!stopWh)         //scan next
            {
                while (!stopWh)     //scan brotherO
                {
                    if (circleDict.ContainsKey(p.index))
                        foreach (Circle circle in circleDict[p.index])
                            if (hasVP && circle.pos[0] == 'N')
                            {
                                circleNP3 = circle;
                                stopWh = true;
                            }
                            else if (circle.pos[0] == 'V') hasVP = true;
                    if (p.brotherO == null) break;
                    p = p.brotherO;
                }
                if (p.next.Count == 0) break;
                p = p.next[0];
            }
            connectTwoCircles(circleNP3, circleNP1, "pp");
            stop = true;
        }
        //處理whose
        private void whoseProcessing(PL pointerPL, ref bool stop)
        {
            //當名詞碰上whose，代表會是 NP1 + whose + NP2 + VP 的結構
            //NP1 必須指向 NP2
            Circle circleNP1 = new Circle(), circleNP2 = new Circle();
            //抓出NP1
            bool stopWh = false;
            PL p = pointerPL;       //現在pointerPL指向的是whose
            while (!stopWh)         //scan prev
            {
                while (!stopWh)     //scan brotherY
                {
                    if (circleDict.ContainsKey(p.index) && p != pointerPL)
                        foreach (Circle circle in circleDict[p.index])
                            if (circle.pos[0] == 'N')
                            {
                                circleNP1 = circle;
                                stopWh = true;
                            }
                    if (p.brotherY == null) break;
                    p = p.brotherY;
                }
                if (p.prev == null) break;
                p = p.prev;
            }
            //抓出NP2
            stopWh = false;
            p = pointerPL;                          //現在pointerPL指向的是whose
            if (p.next.Count > 0) p = p.next[0];    //避免有些POS tree結構不一致
            while (!stopWh)         //scan next
            {
                while (!stopWh)     //scan brotherO
                {
                    if (circleDict.ContainsKey(p.index))
                        foreach (Circle circle in circleDict[p.index])
                            if (circle.pos[0] == 'N')
                            {
                                circleNP2 = circle;
                                stopWh = true;
                            }
                    if (p.brotherO == null) break;
                    p = p.brotherO;
                }
                if (p.next.Count == 0) break;
                p = p.next[0];
            }
            connectTwoCircles(circleNP1, circleNP2, "'s");
            stop = true;
        }

        #endregion

        //初始Parent information
        private void initParentInfo(Circle targetCircle, ref List<string> catchPOS, ref string relation)
        {
            if (targetCircle.pos[0] == 'N') //NP or NNP
            {
                catchPOS.Add("VP");
                relation = "o";
            }
            else if (targetCircle.pos == "VP")
            {
                catchPOS.Add("NP");
                relation = "v";
            }
        }
        //更新Parent information
        private void updateParentInfo(ref Circle targetCircle, PL pointerPL, ref List<string> catchPOS, ref string relation, ref bool stop, bool isFirstPL)
        {
            foreach (WordAndPOS wap in pointerPL.words)
            {
                if (targetCircle.pos[0] == 'N') //NP or NNP
                {
                    //名詞遇到介詞
                    if (wap.pos == "IN" || wap.pos == "TO")
                    {
                        catchPOS.Add("NP");
                        relation = "pp:" + wap.word;
                    }
                    //名詞遇到's
                    else if (wap.word == "'s" && !isFirstPL)
                    {
                        catchPOS = new List<string>() { "NP" };
                        relation = "'s";
                    }
                    //名詞遇到關係代名詞，依不同的代名詞採取不同動作
                    else if (wap.pos[0] == 'W')
                    {
                        if (wap.word == "which" || wap.word == "whom" || wap.word == "that")
                            whichProcessing(pointerPL, ref stop);
                        else if (wap.word == "where") whereProcessing(pointerPL, ref stop);
                        else if (wap.word == "whose") whoseProcessing(pointerPL, ref stop);
                        else if (wap.word == "when" || wap.word == "why") stop = true;
                    }
                    //名詞遇到逗號，判斷是不是同位語
                    else if (wap.pos == ",")
                    {
                        //brotherO中有逗號但沒有連詞的為同位語
                        bool hasComma = false, hasCC = false;
                        PL p = pointerPL.brotherO;
                        while (p != null)
                        {
                            if (p.words.Count > 0)
                            {
                                if (p.words[0].pos == ",") hasComma = true;
                                if (p.words[0].pos == "CC") hasCC = true;
                            }
                            p = p.brotherO;
                        }
                        //是同位語
                        if (hasComma && !hasCC)
                        {
                            Circle circle = new Circle(targetCircle.id + "#", "is", "VP");
                            connectTwoCircles(circle, targetCircle, "o");   //連接兩個Circle
                            this.idDict[circle.id] = circle;
                            targetCircle = circle;
                            catchPOS = new List<string>() { "NP" };
                            relation = "v";
                        }
                    }
                }
            }
        }
        //判斷是否符合詞性並抓取父節點
        private bool catchParent(Circle targetCircle, PL pointerPL, List<string> catchPOS, string relation, bool needPluralParent)
        {
            //符合詞性，且該PL有circle
            if ((catchPOS.Contains(pointerPL.pos) || 
                (pointerPL.words.Count > 0 && 
                    (pointerPL.words[0].pos[0] == 'N' && catchPOS.Contains("NP") || 
                    pointerPL.words[0].pos[0] == 'V' && catchPOS.Contains("VP")))) && 
                circleDict.ContainsKey(pointerPL.index))
            {
                foreach (Circle circle in circleDict[pointerPL.index])
                {
                    if (circle.pos[0] == 'N' && catchPOS.Contains("NP") ||
                        circle.pos[0] == 'V' && catchPOS.Contains("VP"))
                    {
                        connectTwoCircles(circle, targetCircle, relation);  //連接兩個Circle
                        if (needPluralParent)
                            catchPluralParent(targetCircle, pointerPL, catchPOS, relation); //抓取複數父節點
                        return true;
                    }
                }
            }
            return false;
        }
        
        //抓取複數父節點
        private void catchPluralParent(Circle targetCircle, PL pointerPL, List<string> catchPOS, string relation)
        {
            PL p = pointerPL.brotherO;
            bool hasCC = false;
            //先檢查brotherO有CC
            while (p != null)   //scan brotherO
            {
                if (p.words.Count > 0 && p.words[0].pos == "CC") hasCC = true;
                p = p.brotherO;
            }
            p = pointerPL.brotherO;
            while (p != null && hasCC)  //scan brotherO
            {
                if (circleDict.ContainsKey(p.index))
                {
                    foreach (Circle circle in circleDict[p.index])
                    {
                        if (circle.pos[0] == 'N' && catchPOS.Contains("NP"))
                        {
                            connectTwoCircles(circle, targetCircle, relation);      //連接兩個Circle
                        }
                        //父節點是動詞代表targetCircle是受詞，若父節點底下還有別的受詞，那就不抓
                        else if (circle.pos[0] == 'V' && catchPOS.Contains("VP"))
                        {
                            if (p.next.Count == 0 || p.next[0].pos[0] != 'N')
                                connectTwoCircles(circle, targetCircle, relation);  //連接兩個Circle
                        }
                    }
                }
                p = p.brotherO;
            }
            if (pointerPL.next.Count > 0) p = pointerPL.next[0];
            hasCC = false;
            //先檢查brotherO有CC
            while (p != null)   //scan brotherO of next
            {
                if (p.words.Count > 0 && p.words[0].pos == "CC") hasCC = true;
                p = p.brotherO;
            }
            if (pointerPL.next.Count > 0) p = pointerPL.next[0];
            while (p != null && hasCC)  //scan brotherO of next
            {
                if (circleDict.ContainsKey(p.index))
                {
                    foreach (Circle circle in circleDict[p.index])
                    {
                        if (circle.pos[0] == 'N' && catchPOS.Contains("NP"))
                        {
                            connectTwoCircles(circle, targetCircle, relation);      //連接兩個Circle
                        }
                        //父節點是動詞代表targetCircle是受詞，若父節點底下還有別的受詞，那就不抓
                        else if (circle.pos[0] == 'V' && catchPOS.Contains("VP"))
                        {
                            if (p.next.Count == 0 || p.next[0].pos[0] != 'N')
                                connectTwoCircles(circle, targetCircle, relation);  //連接兩個Circle
                        }
                    }
                }
                p = p.brotherO;
            }
        }
        //確認是否需要檢查複數父節點
        private bool checkNeedPluralParent(Circle targetCircle, PL pointerPL)
        {
            if (targetCircle.pos == "JJ" && pointerPL.pos == "NP") return false;
            return true;
        }

        //處理形容詞
        private bool adjProcessing(Circle targetCircle, PL pointerPL)
        {
            if (targetCircle.pos != "JJ") return false;
            //由形容詞的PL向上檢查，若先碰到NP則向下找父節點，若先碰到VP則向上找父節點
            PL p = pointerPL;
            bool hasNP = false, stop = false;
            while (!stop)       //scan prev
            {
                while (!stop && p.pos != "")    //scan brotherY
                {
                    if (p.pos[0] == 'N')
                    {
                        hasNP = true;
                        stop = true;
                    }
                    else if (p.pos[0] == 'V')
                    {
                        stop = true;
                    }
                    if (p.brotherO == null) break;
                    p = p.brotherO;
                }
                if (p.prev == null) break;
                p = p.prev;
            }
            p = pointerPL;
            stop = false;
            if (hasNP)  //向下找父節點
            {
                while (!stop)       //scan next
                {
                    while (!stop)   //scan brotherO
                    {
                        if (circleDict.ContainsKey(p.index))
                            foreach (Circle circle in circleDict[p.index])
                                if (circle.pos[0] == 'N')
                                {
                                    connectTwoCircles(circle, targetCircle, "adj");
                                    stop = true;
                                }
                        if (p.brotherO == null) break;
                        p = p.brotherO;
                    }
                    if (p.next.Count == 0) break;
                    p = p.next[0];
                }
            }
            else        //向上找父節點
            {
                while (!stop)       //scan prev
                {
                    while (!stop)   //scan brotherY
                    {
                        if (circleDict.ContainsKey(p.index))
                            foreach (Circle circle in circleDict[p.index])
                                if (circle.pos[0] == 'N')
                                {
                                    connectTwoCircles(circle, targetCircle, "adj");
                                    stop = true;
                                }
                        if (p.brotherY == null) break;
                        p = p.brotherY;
                    }
                    if (p.prev == null) break;
                    p = p.prev;
                }
            }
            return true;
        }

        //尋找一個Circle的父節點
        private void findParent(Circle targetCircle, List<PL> PLList, int ctPL)
        {
            //先確認是否為形容詞，需要個別處理
            if (adjProcessing(targetCircle, PLList[ctPL])) return;
            //初始Parent information
            List<string> catchPOS = new List<string>();
            string relation = "";
            initParentInfo(targetCircle, ref catchPOS, ref relation);
            //尋找父節點
            PL pointerPL = PLList[ctPL];
            bool stop = false, isParent = false, isFirstPL = true;
            bool needPluralParent = checkNeedPluralParent(targetCircle, pointerPL); //確認是否需要檢查複數父節點
            while (!stop)       //scan prev
            {
                while (!stop)   //scan brotherY
                {
                    updateParentInfo(ref targetCircle, pointerPL, ref catchPOS, ref relation, ref stop, isFirstPL); //更新Parent information
                    if (stop) break;
                    stop = catchParent(targetCircle, pointerPL, catchPOS, relation, needPluralParent);  //判斷是否符合詞性並抓取父節點
                    
                    //符合詞性，但該PL沒有circle，且不是自己的父輩
                    if (!stop && catchPOS.Contains(pointerPL.pos) && pointerPL.next.Count > 0 && !isParent)
                    {
                        PL pointerStore = pointerPL.next[0];
                        while (!stop)       //scan brotherO
                        {
                            while (!stop)   //scan next
                            {
                                stop = catchParent(targetCircle, pointerPL, catchPOS, relation, needPluralParent);  //判斷是否符合詞性並抓取父節點
                                if (pointerPL.next.Count == 0) break;
                                pointerPL = pointerPL.next[0];
                            }
                            if (pointerPL.brotherO == null) break;
                            pointerPL = pointerPL.brotherO;
                        }
                        pointerPL = pointerStore;
                    }

                    if (pointerPL.brotherY == null) break;
                    pointerPL = pointerPL.brotherY;
                    isParent = false;   //pointer指向brother後，就不是自己的父輩
                    isFirstPL = false;
                }
                if (pointerPL.prev == null) break;
                pointerPL = pointerPL.prev;
                isParent = true;
            }
        }
        //連接Circle
        private void connectCircles(List<PL> PLList)
        {
            for (int ctPL = 0; ctPL < PLList.Count; ctPL++)
            {
                if (circleDict.ContainsKey(ctPL))   //這個PL有需要連接的Circle
                {
                    foreach (Circle targetCircle in circleDict[ctPL])
                    {
                        findParent(targetCircle, PLList, ctPL);
                    }
                }
            }
        }

        #endregion

        #region Connect circles (not rely on POS tree)

        //尋找一個Circle的父節點 - 檢查名詞異動
        private void findParent_preCheckNP(WordAndPOS wap, ref Circle targetCircle, ref List<string> catchPOS, ref string relation, ref bool doIncrease, ref int ctPL, ref List<PL> PLList, ref int ctPL_Target, ref bool stop)
        {
            //名詞遇到介詞
            if (wap.pos == "IN" || wap.pos == "TO")
            {
                catchPOS.Add("NP");
                relation = "pp:" + wap.word;
                doIncrease = false;
            }
            //名詞遇到's
            else if (wap.word == "'s")
            {
                catchPOS = new List<string>() { "NP" };
                relation = "'s";
                doIncrease = false;
            }
            //名詞遇到逗號，判斷是and語句或同位語
            //若是and語句，再判斷要不要抓父節點
            //若是同位語，則抓同位語前的名詞
            else if (wap.word == ",")
            {
                //第一個逗號後沒有and，且第二個逗號後是動詞，那就是同位語
                bool stopSearch = false, hasAnd = false, hasNP = false, hasVP = false;
                int i = ctPL + 1;
                //檢查第一個逗號後有沒有and
                while (i < PLList.Count && !stopSearch)
                {
                    foreach (WordAndPOS wapi in PLList[i].words)
                    {
                        stopSearch = true;
                        if (wapi.pos == "CC") hasAnd = true;
                    }
                    i += 1;
                }
                //檢查第二個逗號後有沒有動詞
                stopSearch = false;
                while (i < PLList.Count && !stopSearch)
                {
                    if (PLList[i].words.Count > 0 && PLList[i].words[0].pos == ",")
                    {
                        int j = i + 1;
                        while (j < PLList.Count && !stopSearch)
                        {
                            foreach (WordAndPOS wapj in PLList[j].words)
                            {
                                stopSearch = true;
                                if (wapj.pos[0] == 'V') hasVP = true;
                            }
                            j += 1;
                        }
                    }
                    i += 1;
                }
                if (!stopSearch) hasVP = true;  //找不到第二個逗號
                //是同位語
                if (!hasAnd && hasVP)
                {
                    Circle circle = new Circle(targetCircle.id + "#", "is", "VP");
                    connectTwoCircles(circle, targetCircle, "o");   //連接兩個Circle
                    this.idDict[circle.id] = circle;
                    targetCircle = circle;
                    catchPOS = new List<string>() { "NP" };
                    relation = "v";
                    doIncrease = false;
                }
                //是and語句
                //若逗號後有NP+VP結構，則不抓父節點
                //若沒有則抓取and語句的第一個逗號前的詞彙
                else
                {
                    stopSearch = false;
                    hasNP = false;
                    hasVP = false;
                    i = ctPL + 1;
                    while (i < PLList.Count && !stopSearch)
                    {
                        if (PLList[i].words.Count > 0 && PLList[i].words[0].pos == ",") break;
                        foreach (WordAndPOS wapi in PLList[i].words)
                        {
                            if (!hasNP && wapi.pos[0] == 'N')
                            {
                                hasNP = true;
                                break;
                            }
                            else if (hasNP && wapi.pos[0] == 'V')   //NP緊接著VP
                            {
                                hasVP = true;
                                stopSearch = true;
                            }
                            else if (hasNP) //NP後不是VP
                            {
                                stopSearch = true;
                            }
                        }
                        i += 1;
                    }
                    //有NP+VP結構，不抓父節點
                    if (hasNP && hasVP) stop = true;
                    //沒有則抓取and語句的第一個逗號前的詞彙
                    else
                    {
                        //往前找到and語句的第一個逗號
                        i = ctPL;
                        int lastComma = 0;
                        while (i >= 0 && !stopSearch)
                        {
                            if (PLList[i].words.Count > 0 && PLList[i].words[0].pos == ",")
                            {
                                lastComma = i;
                            }

                            i -= 1;
                        }
                    }
                }
            }
        }
        //尋找一個Circle的父節點 - 檢查動詞異動
        private void findParent_preCheckVP(WordAndPOS wap, ref int ctPL, ref List<PL> PLList, ref bool andLock, ref bool appositiveLock)
        {
            //動詞遇到逗號，判斷是and語句或同位語
            //若是and語句，則跳過and前的名詞，直到遇到另一個動詞
            //若是同位語，則抓同位語前的名詞
            if (wap.pos == "," && !andLock && !appositiveLock)
            {
                //往前找另一個逗號，若逗號後的第一個詞也是動詞，或找不到逗號，那就是and語句
                bool findComma = false;
                int i = ctPL - 1;
                while (i >= 0 && !findComma)
                {
                    if (PLList[i].words.Count > 0 && PLList[i].words[0].pos == ",") //找到另一個逗號
                    {
                        int j = i + 1;
                        while (j < ctPL && !findComma)
                        {
                            foreach (WordAndPOS wapj in PLList[j].words)
                            {
                                //e.g. Max keeps a dog, a cat, and a bird, adopts a mouse, a fish, and a rabbit, and has a pig, a snake, and a sheep.
                                if (wapj.pos[0] == 'V' || wapj.pos == "CC") //是and語句
                                {
                                    andLock = true;
                                    findComma = true;
                                    break;
                                }
                                else if (wapj.pos[0] == 'N')    //是同位語
                                {
                                    appositiveLock = true;
                                    findComma = true;
                                    break;
                                }
                            }
                            j += 1;
                        }
                    }
                    i -= 1;
                }
                if (!findComma && i == -1) andLock = true;  //到最後沒有找到另一個逗號
            }
            else if (andLock && wap.pos.Length > 0 && wap.pos[0] == 'V') andLock = false;
            else if (appositiveLock && wap.pos == ",") appositiveLock = false;
        }
        //尋找一個Circle的父節點 - 檢查形容詞異動
        private void findParent_preCheckJJ(ref Circle targetCircle, ref string relation, ref List<PL> PLList, ref int ctPL_Target, ref bool stop)
        {
            //N1 + V + JJ 型，將JJ的parent指向V的parent
            //N1 + V + N2 + JJ 型，將JJ的parent指向N2
            if (PLList[ctPL_Target].pos == "ADJP")
            {
                int i = ctPL_Target - 1;
                while (i >= 0 && !stop)
                {
                    if (circleDict.ContainsKey(i))
                    {
                        foreach (Circle circle in circleDict[i])
                        {
                            if (circle.pos == "NP" || circle.pos == "NNP")
                            {
                                connectTwoCircles(circle, targetCircle, relation);
                                stop = true;
                                break;
                            }
                            else if (circle.pos == "VP" && circle.prev.Count > 0)
                            {
                                connectTwoCircles(circle.prev[circle.prev.Count - 1].tip, targetCircle, relation);
                                stop = true;
                                break;
                            }
                        }
                    }
                    i -= 1;
                }
            }
        }

        //尋找一個Circle的父節點
        private void findParentT(Circle targetCircle, List<string> catchPOS, string relation, bool doIncrease, int ctPL, List<PL> PLList)
        {
            int ctPL_Target = ctPL;
            bool stop = false, andLock = false, appositiveLock = false;
            while (!stop && 0 <= ctPL && ctPL < PLList.Count)
            {
                //檢查PL有沒有需要但不是Circle的詞(pp、and、which)，再更新catchPOS、relation、doIncrease
                foreach (WordAndPOS wap in PLList[ctPL].words)
                {
                    if (ctPL_Target == ctPL) break;

                    //檢查名詞異動
                    if (targetCircle.pos == "NP" || targetCircle.pos == "NNP")
                        findParent_preCheckNP(wap, ref targetCircle, ref catchPOS, ref relation, ref doIncrease, ref ctPL, ref PLList, ref ctPL_Target, ref stop);

                    //檢查動詞異動
                    else if (targetCircle.pos == "VP")
                        findParent_preCheckVP(wap, ref ctPL, ref PLList, ref andLock, ref appositiveLock);

                    //檢查形容詞異動
                    else if (targetCircle.pos == "JJ")
                        findParent_preCheckJJ(ref targetCircle, ref relation, ref PLList, ref ctPL_Target, ref stop);
                }
                //確認並抓取父節點
                if (circleDict.ContainsKey(ctPL) && !stop && !andLock && !appositiveLock)
                {
                    foreach (Circle circle in circleDict[ctPL])
                    {
                        if ((catchPOS.Contains(circle.pos) || circle.pos == "NNP" && catchPOS.Contains("NP")) && targetCircle != circle)
                        {
                            connectTwoCircles(circle, targetCircle, relation);  //連接兩個Circle
                            stop = true;
                        }
                        //已抓到父節點
                        //處理複數父節點問題
                        if (stop && targetCircle.pos != "JJ")
                        {
                            int i = ctPL - 1;
                            if (PLList[i].words.Count > 0 && PLList[i].words[0].pos == "CC")
                            {
                                while (i >= 0 && (PLList[i].pos == "" || PLList[i].pos.Length > 0 && PLList[i].pos[0] == circle.pos[0]))
                                //while (i >= 0 && !(PLList[i].pos != "" && PLList[i].words.Count == 0))
                                {
                                    if (circleDict.ContainsKey(i))
                                        foreach (Circle circleCC in circleDict[i])
                                            if (circleCC.pos.Length > 0 && circle.pos[0] == circleCC.pos[0])   //必須與第一個父節點同詞性
                                                connectTwoCircles(circleCC, targetCircle, relation);  //連接兩個Circle
                                    i -= 1;
                                }
                            }
                            break;
                        }
                    }
                }
                if (!doIncrease) ctPL -= 1;
                else ctPL += 1;
            }
        }
        //連接Circle
        private void connectCirclesT(List<PL> PLList, Dictionary<int, List<Circle>> circleDict)
        {
            List<string> catchPOS = new List<string>();
            string relation = "";
            bool doIncrease = false;
            for (int ctPL = 0; ctPL < PLList.Count; ctPL++)
            {
                if (circleDict.ContainsKey(ctPL))   //這個PL有需要連接的Circle
                {
                    foreach (Circle targetCircle in circleDict[ctPL])
                    {
                        //決定初始catchPOS、relation、doIncrease
                        if (targetCircle.pos == "NP" || targetCircle.pos == "NNP")
                        {
                            catchPOS = new List<string>() { "VP" };
                            relation = "o";
                            doIncrease = false;
                        }
                        else if (targetCircle.pos == "VP")
                        {
                            catchPOS = new List<string>() { "NP" };
                            relation = "v";
                            doIncrease = false;
                        }
                        else if (targetCircle.pos == "JJ")
                        {
                            catchPOS = new List<string>() { "NP" };
                            relation = "adj";
                            doIncrease = true;
                        }
                        //尋找一個Circle的父節點
                        findParentT(targetCircle, catchPOS, relation, doIncrease, ctPL, PLList);
                    }
                }
            }
        }

        #endregion

        public void add(List<PL> PLList)
        {
            //將句子的所有詞做成Circle(不處理next)，回傳字典{PL編號: PL中的數個Cirlce}
            generateCircles(PLList);

            //連接Circle
            connectCircles(PLList);

            this.sn += 1;
        }
    }
}
