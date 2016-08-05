using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveonik.Stemmers;

namespace QuestionAnswering
{
    class Stem
    {
        //取得Stem結果
        public static string getStem(string word)
        {
            IStemmer stemmer = new EnglishStemmer();
            return stemmer.Stem(word);
        }
    }
}
