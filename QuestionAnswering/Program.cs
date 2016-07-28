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
            //userInputSentence();

            string word = "babylonia";
            List<string> synonymList, antonymList;
            Thesaurus.getThesaurus(word, out synonymList, out antonymList);


            Console.WriteLine("synonymList: " + synonymList.Count);
            foreach (string str in synonymList) Console.Write(str + ", ");
            Console.WriteLine("\n\nantonymList: " + antonymList.Count);
            foreach (string str in antonymList) Console.Write(str + ", ");

            Console.ReadLine();
        }
    }
}
