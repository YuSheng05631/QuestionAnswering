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

                ROOT root = POSTree.getPOSTree(sentence);
                Question.transformQuestion(root);

                Console.WriteLine("========================================\n");
            }
        }
        static void Main(string[] args)
        {
            //userInputSentence();

            string s1 = "What you have to do is to leave.";
            string s2 = "What you have to do is to stay.";
            ROOT root1 = POSTree.getPOSTree(s1);
            ROOT root2 = POSTree.getPOSTree(s2);
            Console.WriteLine(Inference.isSamePOSTree(root1, root2));

            Console.ReadLine();
        }
    }
}
