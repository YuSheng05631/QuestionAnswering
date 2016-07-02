using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestionAnswering
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                string sentence = Console.ReadLine();
                List<string> segmentList;
                List<MainTerm> MTList, truthList;
                Segment.getSegment(sentence, out segmentList, out MTList);
                truthList = Truth.getTruth(MTList);
                
                //print
                Console.WriteLine("==========segmentList==========");
                foreach (string str in segmentList) Console.WriteLine(str);
                Console.WriteLine();
                Console.WriteLine("==========MTList==========");
                foreach (MainTerm mt in MTList)
                {
                    foreach (string str in mt.W) Console.WriteLine("W: " + str);
                    foreach (string str in mt.SQ) Console.WriteLine("SQ: " + str);
                    foreach (string str in mt.PP) Console.WriteLine("PP: " + str);
                    foreach (string str in mt.S) Console.WriteLine("S: " + str);
                    foreach (string str in mt.V) Console.WriteLine("V: " + str);
                    foreach (string str in mt.VBN) Console.WriteLine("VBN: " + str);
                    foreach (string str in mt.O) Console.WriteLine("O: " + str);
                    Console.WriteLine();
                }
                Console.WriteLine("==========truthList==========");
                foreach (MainTerm mt in truthList)
                {
                    foreach (string str in mt.W) Console.WriteLine("W: " + str);
                    foreach (string str in mt.SQ) Console.WriteLine("SQ: " + str);
                    foreach (string str in mt.PP) Console.WriteLine("PP: " + str);
                    foreach (string str in mt.S) Console.WriteLine("S: " + str);
                    foreach (string str in mt.V) Console.WriteLine("V: " + str);
                    foreach (string str in mt.VBN) Console.WriteLine("VBN: " + str);
                    foreach (string str in mt.O) Console.WriteLine("O: " + str);
                    Console.WriteLine();
                }
                Console.WriteLine("========================================\n");
            }
        }
    }
}
