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
                ROOT root = POSTree.getPOSTree(sentence);
                if (Question.getQuestionType(root) != 0) root = Question.transformQuestion(root);
                POSTree.printROOT(root);
                Console.WriteLine("========================================\n");
            }
        }
        static void userInputPrintCluaseList()
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                ROOT root = POSTree.getPOSTree(sentence);
                if (Question.getQuestionType(root) != 0) root = Question.transformQuestion(root);
                List<List<PLAndPOS>> clauseList = Clause.getClauseList(root, 6);
                Clause.printCluaseList(clauseList);
                Console.WriteLine("========================================\n");
            }
        }

        static void Main(string[] args)
        {
            //userInputPrintROOT();
            userInputPrintCluaseList();

            Console.ReadLine();
        }
    }
}
