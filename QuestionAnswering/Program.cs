using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Program
    {
        static void userInputPrintROOT()
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                List<ROOT> rootList = POSTree.getROOTList(sentence);
                for (int i = 0; i < rootList.Count; i++)
                {
                    if (Question.getQuestionType(rootList[i]) != 0)
                        rootList[i] = Question.transformQuestion(rootList[i]);
                    POSTree.printROOT(rootList[i]);
                }
                Console.WriteLine("========================================\n");
            }
        }
        static void userInputPrintAnaphora()
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                List<ROOT> rootList = POSTree.getROOTList(sentence);
                Anaphora.transformAnaphora(rootList);
                for (int i = 0; i < rootList.Count; i++)
                {
                    POSTree.printROOT(rootList[i]);
                }
                Console.WriteLine("========================================\n");
            }
        }
        static void userInputPrintCluaseList(int rType)
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                List<ROOT> rootList = POSTree.getROOTList(sentence);
                for (int i = 0; i < rootList.Count; i++)
                {
                    if (Question.getQuestionType(rootList[i]) != 0)
                        rootList[i] = Question.transformQuestion(rootList[i]);
                    List<List<PLAndPOS>> clauseList = Clause.getClauseList(rootList[i], rType);
                    Clause.printCluaseList(clauseList);
                }
                Console.WriteLine("========================================\n");
            }
        }
        static void userInputPrintStem()
        {
            while (true)
            {
                string w = Console.ReadLine().Trim();
                if (w == "") continue;
                Console.WriteLine("stem: " + Stem.getStem(w) + "\n");
            }
        }
        static void userInputPrintHasSynonymAndAntonym()
        {
            while (true)
            {
                string w1 = Console.ReadLine().Trim();
                if (w1 == "") continue;
                string w2 = Console.ReadLine().Trim();
                if (w2 == "") continue;
                Console.WriteLine("stem: " + Stem.getStem(w1) + ", " + Stem.getStem(w2));
                Console.WriteLine("hasSynonym: " + Thesaurus.hasSynonym(w1, w2));
                Console.WriteLine("hasAntonym: " + Thesaurus.hasAntonym(w1, w2));
                Console.WriteLine();
            }
        }
        static void userInputPrintWordNet()
        {
            while (true)
            {
                string w = Console.ReadLine().Trim();
                if (w == "") continue;
                WordNet.printWordNetResultList(WordNet.getWordNetResultList(w));
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            //userInputPrintROOT();
            //userInputPrintAnaphora();
            //userInputPrintCluaseList(0);
            //userInputPrintStem();
            //userInputPrintHasSynonymAndAntonym();
            //userInputPrintWordNet();

            List<string> cr1, cr2;
            int ci1, ci2;
            Wiki.getCross("kingdom", "civilization", out cr1, out cr2, out ci1, out ci2);

            foreach (string str in cr1) Console.Write(str + " > ");
            Console.WriteLine();
            foreach (string str in cr2) Console.Write(str + " > ");
            Console.WriteLine();
            
            Console.Write(ci1);
            if (ci1 != -1) Console.WriteLine(", " + cr1[ci1]);
            Console.WriteLine();
            Console.Write(ci2);
            if (ci2 != -1) Console.WriteLine(", " + cr2[ci2]);
            Console.WriteLine();


            /*string url = "https://en.wikipedia.org/wiki/Category:Mind";
            List<string> categoryList = Wiki.getCategoryList(url);
            foreach (string str in categoryList) Console.WriteLine(str);*/

            //List<ROOT> rootList = Wiki.getWikiPOSTree("Edison");
            /*List<ROOT> rootList = Wiki.getWikiPOSTree("Einstein");
            List<ROOT> rootListPart = new List<ROOT>();
            for (int i = 11; i <= 20; i++) rootListPart.Add(rootList[i]);
            Anaphora.transformAnaphora(rootListPart);*/
            //for (int i = 0; i < rootListPart.Count; i++) POSTree.printROOT(rootListPart[i]);

            Console.WriteLine("====================End====================");
            Console.ReadLine();
        }
    }
}
