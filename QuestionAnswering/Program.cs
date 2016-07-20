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
                Question.transformQuestion(root);

                Console.WriteLine("========================================\n");
            }
        }
        static void Main(string[] args)
        {
            userInputSentence();

            string s1 = "Write the name of your cat.";
            ROOT root1 = POSTree.getPOSTree(s1);
            POSTree.printROOT(root1);


            Console.ReadLine();
        }
    }
}
