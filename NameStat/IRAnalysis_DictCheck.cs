using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace NameStat
{
    class IRAnalysis_DictCheck : IRAnalysis
    {
        Dictionary<Tuple<string, Gender>, int> dictHash; //словарь документа

        public new void run()
        {
            dictHash = new Dictionary<Tuple<string, Gender>, int> { };

            base.run();
            this.dictCheck();    
        }
        public override void processLine(string _line)
        {
            string alphaOnlyLine;
            string[] wordsOfALine;
            string ageStr;

            Tuple<string, Gender> dictKey;

            alphaOnlyLine = this.replaceNonAlpha(_line);
            //Console.WriteLine(alphaOnlyLine);
            ageStr = this.getGenderAge(_line);


            wordsOfALine = alphaOnlyLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in wordsOfALine)
            {
                dictKey = Tuple.Create(word, gender);
                if (dictHash.ContainsKey(dictKey))
                {
                    dictHash[dictKey] = dictHash[dictKey] + 1;
                }
                else
                {
                    dictHash.Add(dictKey, 1);
                }
            }

        }
        protected void dictCheck()
        {
            Console.WriteLine("Слова не найденные в словарях, начало:");

            foreach (var word in dictHash.Keys)
            {
                if (!wordsDict.Contains(word.Item1, StringComparer.CurrentCultureIgnoreCase) &&      //Нет ни в словаре слов
                    !namesDict.ContainsKey(word.Item1) &&      //ни в словаре имен
                    !patronimicDict.ContainsKey(word.Item1))        //ни в словаре отчеств
                {
                    if (word.Item1.EndsWith("а") && //ни в словаре отчеств с -а на конце
                        patronimicDict.ContainsKey(word.Item1.Substring(0, word.Item1.Length - 1)))
                    {
                        //Console.WriteLine("skipped " + word.Item1.Substring(0, word.Item1.Length - 1));
                        continue;
                    }
                    Console.WriteLine(word.Item1);
                }
            }
            Console.WriteLine("Слова не найденные в словарях, конец");
        }
        protected void dictOuput()
        {
            long maxCount = 0;
            long i;

            foreach (long WordsCount in dictHash.Values)
                if (WordsCount > maxCount)
                    maxCount = WordsCount;

            for (i = maxCount; i > 0; i--)
            {
                foreach (var word in dictHash.Keys)
                {
                    if (dictHash[word] == i)
                        Console.WriteLine(word.Item1 + " (" + word.Item2 + ") " + dictHash[word]);
                }
            }
        }
        public static void constructAndRun( DataFilesKeeper _dataFilesKeeper)
        {
            IRAnalysis_DictCheck iRAnalysis = new IRAnalysis_DictCheck();

            iRAnalysis.init(_dataFilesKeeper);
            iRAnalysis.run();
        }
    }
}
