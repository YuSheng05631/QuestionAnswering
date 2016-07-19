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

            /*string s1 = "What were the people of the Mitanni Kingdom called?";
            string s2 = "The people of the Mitanni Kingdom were called Hurrian.";
            string s3 = "Write in the name of one city-state involved.";

            List<ROOT> rootList = new List<ROOT>();
            rootList.Add(POSTree.getPOSTree(s1));
            rootList.Add(POSTree.getPOSTree(s2));
            rootList.Add(POSTree.getPOSTree(s3));

            SaveData.savePOSTree(rootList, "test");*/

            List<ROOT> rootList = SaveData.loadPOSTree("test");
            foreach (ROOT root in rootList)
            {
                POSTree.printROOT(root);
            }

            Console.ReadLine();
        }
    }
}
