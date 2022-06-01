using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NameStat
{
    //Reads dictionaries of words and names
    internal class DictReader
    {
        HashSet<string> wordsDict; //словарь слов
        Dictionary<string, string> namesDict; //словарь имен
        Dictionary<string, string> patronimicDict; //словарь отчеств
        HashSet<string> homonymsDict; //словарь омонимов

        public void run()
        {
            this.readWordsDict();
            this.readPatronimicDict();
            this.readNamesDict();
            this.readHomonymsDict();
        }
        private void readWordsDict()
        {
            string[] lines;

            wordsDict = new HashSet<string>();

            if (File.Exists(Config.workingFolder + Config.dictWordsFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.dictWordsFilename);
                foreach (string line in lines)
                {
                    wordsDict.Add(line.Trim());
                    //Console.WriteLine("words: " + line.Trim());
                }

                Console.WriteLine("Загружен словарь слов объемом " + wordsDict.Count + " единиц.");
            }
            else
                throw new Exception("Не найден файл словаря слов");
        }
        private void readNamesDict()
        {
            string[] lines;
            string[] lineItems;
            //string[] secondItemSplit;

            namesDict = new Dictionary<string, string>();

            if (File.Exists(Config.workingFolder + Config.dictNamesFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.dictNamesFilename);
                foreach (string line in lines)
                {
                    lineItems = line.Split(new char[] {'\t'});
                    if (lineItems[1] == "")
                        throw new Exception("В словаре имен не найдено базовое имя");
                    else
                    {
                        //secondItemSplit = lineItems[1].Split(new char[] {' '});
                        if (!namesDict.ContainsKey(lineItems[0].Trim()))
                            //!patronimicDict.ContainsKey(lineItems[0].Trim())) //Отчество может содержатся в словаре имен, такие записи не загружаем
                            namesDict.Add(lineItems[0].Trim(), lineItems[1].Trim());
                        //Console.WriteLine("names: " + lineItems[0].Trim() + " -> " + secondItemSplit[0].Trim());
                    }
                }
                Console.WriteLine("Загружен словарь имен объемом " + namesDict.Count + " единиц.");
            }
            else
                throw new Exception("Не найден файл словаря имен");
        }

        private void readPatronimicDict()
        {
            string[] lines;
            string[] lineItems;

            patronimicDict = new Dictionary<string, string>();

            if (File.Exists(Config.workingFolder + Config.dictPatronimicFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.dictPatronimicFilename);
                foreach (string line in lines)
                {
                    lineItems = line.Split(new char[] { '\t' });
                    if (lineItems[1] == "")
                        throw new Exception("В словаре отчеств не найдено базовое имя");
                    else
                    {
                        if (!patronimicDict.ContainsKey(lineItems[0].Trim()))
                        {
                            patronimicDict.Add(lineItems[0].Trim(), lineItems[1].Trim());
                            //Console.WriteLine("patronimic: " + lineItems[0].Trim() + " -> " + lineItems[1].Trim());
                        }
                    }
                }
                Console.WriteLine("Загружен словарь отчеств объемом " + patronimicDict.Count + " единиц.");
            }
            else
                throw new Exception("Не найден файл словаря отчеств");
        }
        private void readHomonymsDict()
        {
            string[] lines;

            homonymsDict = new HashSet<string>();

            if (File.Exists(Config.workingFolder + Config.dictHomonymsFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.dictHomonymsFilename);
                foreach (string line in lines)
                {
                    homonymsDict.Add(line.Trim());
                }

                Console.WriteLine("Загружен словарь омонимов объемом " + homonymsDict.Count + " единиц.");
            }
            else
                throw new Exception("Не найден файл словаря омонимов");
        }
        public HashSet<string> getWordsDict()
        {
            return wordsDict;
        }
        public Dictionary<string, string> getNamesDict()
        {
            return namesDict;
        }

        public Dictionary<string, string> getPatronimicDict()
        {
            return patronimicDict;
        }
        public HashSet<string> getHomonymsDict()
        {
            return homonymsDict;
        }

        public static DictReader construct()
        {
            DictReader reader = new DictReader();   

            return reader;
        }
    }
}
