using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Question
    {
        //是否為問句(Start)
        public static bool isQuestion(ROOT root)
        {
            bool isQ = false;
            foreach (S s in root.S) isQ = isQ || isQuestionTraversal(s);
            return isQ;
        }
        //是否為問句(Traversal)
        private static bool isQuestionTraversal(S s)
        {
            //其中有一個S擁有WH和SQ即為問句
            bool isQ = false;
            if (s.WH != null && s.SQ != null) isQ = true;   //一檢查到問句就不需要繼續檢查了
            else if (s.Ss != null)   //只檢查Ss
            {
                foreach (S sNext in s.Ss.next) isQ = isQ || isQuestionTraversal(sNext);
            }
            return isQ;
        }

        //取得問句類型(Start)
        public static int getQuestionType(ROOT root)
        {
            //0. 非問句
            //1. SQ連接NP+VP。 e.g. What do you mean?
            //2. SQ連接NP/VP。 e.g. What is your name?
            //3. 命令型問句(第一個S底下有VP無NP)。 e.g. Write in the name of your cat.
            //4. 填空型問句(POSTree包含PRN)。 e.g. Hammurabi belonged to the dynasty of the (B) people.
            int type = 0;
            foreach (S s in root.S) //檢查type 1 or 2
            {
                type = getQuestionTypeTraversal1or2(s);
                if (type != 0) break;
            }
            if (type == 0)  //檢查type 3
            {
                foreach (S s in root.S)
                {
                    type = getQuestionTypeTraversal3(s);
                    if (type != 0) break;
                }
            }
            if (type == 0)  //檢查type 4
            {
                foreach (S s in root.S)
                {
                    type = getQuestionTypeTraversal4(s);
                    if (type != 0) break;
                }
            }
            return type;
        }
        //取得問句類型1 or 2(Traversal)
        private static int getQuestionTypeTraversal1or2(S s)
        {
            int type = 0;
            if (s.WH != null && s.SQ != null && s.SQ.next.Count != 0)   //一檢查到問句就不需要繼續檢查了
            {
                if (s.SQ.next[0].NP != null && s.SQ.next[0].VP != null) type = 1;   //暫定SQ的next只有一個
                else type = 2;
            }
            else if (s.Ss != null)   //只檢查Ss
            {
                foreach (S sNext in s.Ss.next) type = getQuestionTypeTraversal1or2(sNext);
            }
            return type;
        }
        //取得問句類型3(Traversal)
        private static int getQuestionTypeTraversal3(S s)
        {
            int type = 0;
            if (s.VP != null && s.NP == null) type = 3; //一檢查到問句就不需要繼續檢查了
            else if (s.Ss != null)   //只檢查Ss
            {
                foreach (S sNext in s.Ss.next) type = getQuestionTypeTraversal3(sNext);
            }
            return type;
        }
        //取得問句類型4(Traversal)
        private static int getQuestionTypeTraversal4(S s)
        {
            int type = 0;

            return type;
        }

        //將問句轉換成有NNans的陳述句(Start)
        public static ROOT transformQuestion(ROOT root)
        {
            //讀取整個句子的每個詞，重組一個含有NNans新的句子，再使用POSTree.getPOSTree(sentence);
            ROOT rootNew = new ROOT();
            string sentence = "";
            //取得問句類型(Start)
            int type = getQuestionType(root);
            Console.WriteLine("type: " + type);
            if (type == 0) return root; //0. 非問句
            else if (type == 1)         //1. SQ連接NP+VP。 e.g. What do you mean?
            {
                foreach (S s in root.S)
                {
                    string SQWord = getSQWordTraversal(s);
                    sentence += transformQuestionTraversalType1(s, false, SQWord);
                }
            }
            else if (type == 2)         //2. SQ連接NP/VP。 e.g. What is your name?
            {
                sentence = "NNans ";
                foreach (S s in root.S) sentence += transformQuestionTraversalType2(s, false);
            }
            else if (type == 3)         //3. 命令型問句(第一個S底下有VP無NP)。 e.g. Write in the name of your cat.
            {
                sentence = "NNans is ";
                foreach (S s in root.S) sentence += transformQuestionTraversalType3(s, false);
            }
            else if (type == 4)         //4. 填空型問句(POSTree包含PRN)。 e.g. Hammurabi belonged to the dynasty of the (B) people.
            {

            }
            rootNew = POSTree.getPOSTree(sentence);
            return rootNew;
        }
        //將問句轉換成有NNans的陳述句type1(Traversal)
        private static string transformQuestionTraversalType1(S s, bool hasSetNNans, string SQWord)
        {
            //WH、SQ不加入sentence，第一個動詞後加上NNans，加上NNans後將之後的WH、SQ加入sentence
            //進行式：若SQ底下的VP是VBG，將SQ的word加入sentence，位置是SQ底下的NP後
            string sentence = "";
            if (s.WH != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.WH.words) sentence += wap.word + " ";
                foreach (S sNext in s.WH.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.SQ != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.SQ.words) sentence += wap.word + " ";
                foreach (S sNext in s.SQ.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.NP != null)
            {
                foreach (WordAndPOS wap in s.NP.words) sentence += wap.word + " ";
                foreach (S sNext in s.NP.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.VP != null)
            {
                foreach (WordAndPOS wap in s.VP.words)
                {
                    if (!hasSetNNans && (wap.pos == "VBG" || wap.pos == "VBN")) sentence += SQWord + " ";
                    sentence += wap.word + " ";
                }
                if (!hasSetNNans) sentence += "NNans ";
                hasSetNNans = true;
                foreach (S sNext in s.VP.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.PP != null)
            {
                foreach (WordAndPOS wap in s.PP.words) sentence += wap.word + " ";
                foreach (S sNext in s.PP.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.ADJP != null)
            {
                foreach (WordAndPOS wap in s.ADJP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADJP.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.ADVP != null)
            {
                foreach (WordAndPOS wap in s.ADVP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADVP.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            if (s.Ss != null)
            {
                foreach (WordAndPOS wap in s.Ss.words) sentence += wap.word + " ";
                foreach (S sNext in s.Ss.next) sentence += transformQuestionTraversalType1(sNext, hasSetNNans, SQWord);
            }
            return sentence;
        }
        //將問句轉換成有NNans的陳述句type2(Traversal)
        private static string transformQuestionTraversalType2(S s, bool hasSetNNans)
        {
            //sentence開頭加上NNans(在transformQuestion()中加上)，隨後的WH不加入sentence，第二個之後的WH加入sentence
            string sentence = "";
            if (s.WH != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.WH.words) sentence += wap.word + " ";
                else hasSetNNans = true;
                foreach (S sNext in s.WH.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.SQ != null)
            {
                foreach (WordAndPOS wap in s.SQ.words) sentence += wap.word + " ";
                foreach (S sNext in s.SQ.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.NP != null)
            {
                foreach (WordAndPOS wap in s.NP.words) sentence += wap.word + " ";
                foreach (S sNext in s.NP.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.VP != null)
            {
                foreach (WordAndPOS wap in s.VP.words) sentence += wap.word + " ";
                foreach (S sNext in s.VP.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.PP != null)
            {
                foreach (WordAndPOS wap in s.PP.words) sentence += wap.word + " ";
                foreach (S sNext in s.PP.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.ADJP != null)
            {
                foreach (WordAndPOS wap in s.ADJP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADJP.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.ADVP != null)
            {
                foreach (WordAndPOS wap in s.ADVP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADVP.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            if (s.Ss != null)
            {
                foreach (WordAndPOS wap in s.Ss.words) sentence += wap.word + " ";
                foreach (S sNext in s.Ss.next) sentence += transformQuestionTraversalType2(sNext, hasSetNNans);
            }
            return sentence;
        }
        //將問句轉換成有NNans的陳述句type3(Traversal)
        private static string transformQuestionTraversalType3(S s, bool hasSetNNans)
        {
            //從第一個S到包含NP的S之前的內容都不加入sentence
            string sentence = "";
            if (s.WH != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.WH.words) sentence += wap.word + " ";
                foreach (S sNext in s.WH.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.SQ != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.SQ.words) sentence += wap.word + " ";
                foreach (S sNext in s.SQ.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.NP != null)
            {
                hasSetNNans = true;
                foreach (WordAndPOS wap in s.NP.words) sentence += wap.word + " ";
                foreach (S sNext in s.NP.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.VP != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.VP.words) sentence += wap.word + " ";
                foreach (S sNext in s.VP.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.PP != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.PP.words) sentence += wap.word + " ";
                foreach (S sNext in s.PP.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.ADJP != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.ADJP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADJP.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.ADVP != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.ADVP.words) sentence += wap.word + " ";
                foreach (S sNext in s.ADVP.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            if (s.Ss != null)
            {
                if (hasSetNNans) foreach (WordAndPOS wap in s.Ss.words) sentence += wap.word + " ";
                foreach (S sNext in s.Ss.next) sentence += transformQuestionTraversalType3(sNext, hasSetNNans);
            }
            return sentence;
        }
        //取得SQ的Word(Traversal) (transformQuestion用)
        private static string getSQWordTraversal(S s)
        {
            string SQWord = "";
            if (s.WH != null)
            {
                foreach (S sNext in s.WH.next) SQWord += getSQWordTraversal(sNext);
            }
            if (s.SQ != null)
            {
                if (s.SQ.words.Count != 0) SQWord = s.SQ.words[0].word; //找到SQ
                else foreach (S sNext in s.SQ.next) SQWord += getSQWordTraversal(sNext);
            }
            if (s.Ss != null)
            {
                foreach (S sNext in s.Ss.next) SQWord += getSQWordTraversal(sNext);
            }
            return SQWord;
        }
    }
}
