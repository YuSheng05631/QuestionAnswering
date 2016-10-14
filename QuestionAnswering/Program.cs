using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace QuestionAnswering
{
    class Program
    {
        static void printPLArticle()
        {
            while (true)
            {
                string sentence = Console.ReadLine().Trim();
                if (sentence == "") continue;
                List<List<PL>> PLArticle = Sentence.getPLArticle(sentence);
                foreach (List<PL> PLList in PLArticle)
                {
                    Sentence.printPLList(PLList);
                    Console.WriteLine();
                }
                Console.WriteLine("========================================\n");
            }
        }
        static void printHasSynonymOrAntonym()
        {
            while (true)
            {
                string w1 = Console.ReadLine().Trim();
                if (w1 == "") continue;
                string w2 = Console.ReadLine().Trim();
                if (w2 == "") continue;
                Console.WriteLine("Stem: " + Stem.getStem(w1) + ", " + Stem.getStem(w2));
                Console.WriteLine("hasSynonym: " + Thesaurus.hasSynonym(w1, w2));
                Console.WriteLine("hasAntonym: " + Thesaurus.hasAntonym(w1, w2));
                Console.WriteLine("========================================\n");
            }
        }
        static void printIsDerivative()
        {
            while (true)
            {
                string w1 = Console.ReadLine().Trim();
                if (w1 == "") continue;
                string w2 = Console.ReadLine().Trim();
                if (w2 == "") continue;
                Console.WriteLine("Stem: " + Stem.getStem(w1) + ", " + Stem.getStem(w2));
                Console.WriteLine("isDerivative: " + Clause.isDerivative(w1, w2));
                Console.WriteLine("========================================\n");
            }
        }
        static void Main(string[] args)
        {
            //printPLArticle();
            //printHasSynonymOrAntonym();
            //printIsDerivative();
            //SaveData.deleteAllSaveData();
            /*
            string s = "The annual average rainfall in Southern Mesopotamia was less than 200 mm, which is the minimum amount of annual rainfall required for rainfed agriculture. Nevertheless, from around 3500 BC, the(A) people built a great urban civilization which would prosper for more than 1, 000 years on that land.";
            List<List<PL>> PLArticle = Sentence.getPLArticle(s);
            Anaphora.transformAnaphora(PLArticle);
            */
            
            List<string> l = new List<string>();
            l.Add("Nevertheless, from around 3500 BC, the (A) people built a great urban civilization which would prosper for more than 1,000 years on Southern Mesopotamia.");
            l.Add("Sumer was the first urban civilization in the historical region of southern Mesopotamia, modern-day southern Iraq, during the Chalcolithic and Early Bronze ages, and arguably the first civilization in the world.");
            List<List<PL>> PLArticle = Sentence.getPLArticle(l);
            PLArticle[0] = Question.transformQuestion(PLArticle[0]);
            Sentence.printPLList(PLArticle[0]);
            Sentence.printPLList(PLArticle[1]);
            Clause.match(PLArticle[0], PLArticle[1]);
            

            /*
            string s = "Write in the name of the cat that has a pink eye.";
            List<List<PL>> PLArticle = Sentence.getPLArticle(s);
            Sentence.printPLList(Question.transformQuestion(PLArticle[0]));
            */

            Console.WriteLine("====================End====================");
            Console.ReadLine();
        }
    }
}
