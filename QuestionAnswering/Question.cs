using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Question
    {
        //取得問句類型
        public int getQuestionType(List<PL> PLList)
        {
            //0. 非問句
            //1. SQ連接NP+VP。 e.g. What do you mean?
            //2. SQ連接NP/VP。 e.g. What is your name?
            //3. 命令型問句(第一個S底下有VP無NP)。 e.g. Write in the name of your cat.
            //4. 填空型問句(已在getPLArticle轉換標籤)。 e.g. Hammurabi belonged to the dynasty of the (B) people.
            int type = 0;
            type = getQuestionType4(PLList);                    //檢查type 4
            if (type == 0) type = getQuestionType1or2(PLList);  //檢查type 1 or 2
            if (type == 0) type = getQuestionType3(PLList);     //檢查type 3
            return type;
        }
        //取得問句類型1or2
        private int getQuestionType1or2(List<PL> PLList)
        {
            foreach (PL pl in PLList)
            {
                if (pl.pos != "ROOT" && pl.pos[0] != 'S') break;
                bool hasWH = false, hasSQ = false, hasNP = false, hasVP = false;
                foreach (PL next in pl.next)
                {
                    if (next.pos.IndexOf("WH") == 0) hasWH = true;
                    if (next.pos == "SQ")
                    {
                        hasSQ = true;
                        foreach (PL SQnext in next.next)
                        {
                            if (SQnext.pos == "NP") hasNP = true;
                            if (SQnext.pos == "VP") hasVP = true;
                        }
                    }
                }
                if (hasWH && hasSQ)
                {
                    if (hasNP && hasVP) return 1;
                    else return 2;
                }
            }
            return 0;
        }
        //取得問句類型3
        private int getQuestionType3(List<PL> PLList)
        {
            foreach (PL pl in PLList)
            {
                if (pl.pos != "ROOT" && pl.pos[0] != 'S') break;
                bool hasNP = false, hasVP = false;
                foreach (PL next in pl.next)
                {
                    if (next.pos == "NP") hasNP = true;
                    if (next.pos == "VP") hasVP = true;
                }
                if (!hasNP && hasVP) return 3;
            }
            return 0;
        }
        //取得問句類型4
        private int getQuestionType4(List<PL> PLList)
        {
            foreach (PL pl in PLList)
                foreach (WordAndPOS wap in pl.words)
                    if (wap.word.IndexOf("<B") == 0)
                        return 4;
            return 0;
        }

        //將問句轉換成有NNans的陳述句
        public List<PL> transformQuestion(List<PL> PLList)
        {
            //讀取整個句子的每個詞，重組一個含有NNans新的句子，再使用Sentence.getPLArticle()
            string sentence = "";
            int type = getQuestionType(PLList);         //取得問句類型
            if (type == 0 || type == 4) return PLList;  //0. 非問句 or 4. 填空型問句
            else if (type == 1) //1. SQ連接NP+VP。 e.g. What do you mean?
            {
                string SQWord = getSQWordTraversal(PLList); //取得SQ的Word
                sentence = transformQuestionType1(PLList, SQWord);
            }
            else if (type == 2) //2. SQ連接NP/VP。 e.g. What is your name?
            {
                sentence = "NNans " + transformQuestionType2(PLList);
            }
            else if (type == 3) //3. 命令型問句。 e.g. Write in the name of your cat.
            {
                sentence = "NNans is " + transformQuestionType3(PLList);
            }
            Sentence sen = new Sentence();
            List<List<PL>> PLArticle = sen.getPLArticle(sentence);
            return PLArticle[0];
        }
        //將問句轉換成有NNans的陳述句type1
        private string transformQuestionType1(List<PL> PLList, string SQWord)
        {
            //WH、SQ不加入sentence，第一個動詞後加上NNans，加上NNans後將之後的WH、SQ加入sentence
            //進行式：若SQ底下的VP是VBG，將SQ的word加入sentence，位置是SQ底下的NP後
            string sentence = "";
            bool hasSetNNans = false;
            foreach (PL pl in PLList)
            {
                if (pl.pos.IndexOf("WH") == 0 || pl.pos == "SQ")
                {
                    if (hasSetNNans)
                        foreach (WordAndPOS wap in pl.words)
                            sentence += wap.word + " ";
                }
                else if (pl.pos == "VP")
                {
                    foreach (WordAndPOS wap in pl.words)
                    {
                        if (!hasSetNNans && (wap.pos == "VBG" || wap.pos == "VBN")) sentence += SQWord + " ";
                        sentence += wap.word + " ";
                    }
                    if (!hasSetNNans) sentence += "NNans ";
                    hasSetNNans = true;
                }
                else
                {
                    foreach (WordAndPOS wap in pl.words)
                        sentence += wap.word + " ";
                }
            }
            return sentence;
        }
        //將問句轉換成有NNans的陳述句type2
        private string transformQuestionType2(List<PL> PLList)
        {
            //sentence開頭加上NNans(在transformQuestion()中加上)，隨後的WH不加入sentence，第二個之後的WH加入sentence
            string sentence = "";
            bool hasSetNNans = false;
            foreach (PL pl in PLList)
            {
                if (pl.pos.IndexOf("WH") == 0)
                {
                    if (hasSetNNans) foreach (WordAndPOS wap in pl.words) sentence += wap.word + " ";
                    else hasSetNNans = true;
                }
                else
                {
                    foreach (WordAndPOS wap in pl.words) sentence += wap.word + " ";
                }
            }
            return sentence;
        }
        //將問句轉換成有NNans的陳述句type3
        private string transformQuestionType3(List<PL> PLList)
        {
            //從第一個S到包含NP的S之前的內容都不加入sentence
            string sentence = "";
            bool hasSetNNans = false;
            foreach (PL pl in PLList)
            {
                if (pl.pos == "NP") hasSetNNans = true;
                if (hasSetNNans) foreach (WordAndPOS wap in pl.words) sentence += wap.word + " ";
            }
            return sentence;
        }
        //取得SQ的Word (transformQuestion用)
        private string getSQWordTraversal(List<PL> PLList)
        {
            string SQWord = "";
            foreach (PL pl in PLList)
                if (pl.pos == "SQ")
                    foreach (WordAndPOS wap in pl.words)
                        SQWord += wap.word + " ";
            return SQWord.Trim();
        }
    }
}
