using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Program
    {
        static void userInputSentence()
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                ROOT root = POSTree.getPOSTree(sentence);
                if (Question.isQuestion(root)) root = Question.transformQuestion(root);
                POSTree.printROOT(root);

                Console.WriteLine("========================================\n");
            }
        }

        static void Main(string[] args)
        {
            userInputSentence();

            Console.ReadLine();
        }
    }
}
