using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace NameStat
{
    internal class IRAnalysis
    {
        DataFilesKeeper dataFilesKeeper;
        internal string curFilename;

        protected HashSet<string>               wordsDict; //словарь слов
        protected Dictionary<string, string>    namesDict; //словарь имен
        protected Dictionary<string, string>    patronimicDict; //словарь отчеств
        protected HashSet<string>               homonymsDict; //словарь омонимов

        //разобранное вручную. Имя файла, номер строки начиная с 1 -> имя, пол, возраст строкой
        protected Dictionary<Tuple<string, int>, Tuple<string, Gender, string>> manuallyResolved; 

        protected Gender gender;

        protected int lineCount;

        public virtual void run()
        {
            string[] lines;

            this.readDictionaries();
            this.readManualResolutionFile();

            foreach (DataFile iRFile in dataFilesKeeper.dataFiles)
            {
                if (File.Exists(Config.workingFolder + iRFile.FileName))
                {
                    lineCount = 0;
                    curFilename = iRFile.FileName;

                    lines = File.ReadAllLines(Config.workingFolder + curFilename);
                    foreach (string line in lines)
                    {
                        lineCount++;
                        //if (lineCount > 200) break; //test mode

                        if (manuallyResolved.ContainsKey(Tuple.Create(curFilename, lineCount)))
                            this.processLineManuallyResolved();
                        else
                            this.processLine(this.changeSymbols(line));
                    }
                }
                else
                    throw new Exception("Не найден файл данных");
            }
        }
        protected void readManualResolutionFile()
        {
            string[] lines;

            manuallyResolved = new Dictionary<Tuple<string, int>, Tuple<string, Gender, string>> {};

            if (File.Exists(Config.workingFolder + Config.manuallyResolvedFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.manuallyResolvedFilename);
                foreach (string line in lines)
                {
                    Gender MRgender;
                    string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length == 5)
                    {
                        //формат файла: имя файла, строка, имя, пол, возраст
                        //Пол м.б. M, F или undefined, если нужно пропустить строку
                        if (dataFilesKeeper.containsFilename(words[0])) //если файл есть в списке обрабатываемых файлов
                        {
                            if (words[3] == "M") MRgender = Gender.M;
                            else if (words[3] == "F") MRgender = Gender.F;
                            else MRgender = Gender.undefined; //для пропуска строки

                            int lineNum;
                            if (Int32.TryParse(words[1], out lineNum))
                            {
                                manuallyResolved.Add(new Tuple<string, int>(words[0], lineNum), 
                                                     new Tuple<string, Gender, string>(words[2], MRgender, words[4]));
                            }
                        }
                    }
                    else
                        throw new Exception("Manually resolved file format error");
                }
            }
            Console.WriteLine("Загружено обработанных вручную исключений: " + manuallyResolved.Count);
        }
        string changeSymbols(string _line)
        {
            string ret;

            //Заменяем Ё на E, т.к. оно неправильно обрабатывается split-ом
            ret = _line.Replace('ё', 'е').Replace('Ё', 'Е');

            //Убираем ½, чтобы не усложнять регулярные выражения. В ИР Черных Ручьев 1860 есть возраста вплоть до 14½
            ret = ret.Replace("9½", "9");
            ret = ret.Replace("8½", "8");
            ret = ret.Replace("7½", "7");
            ret = ret.Replace("6½", "6");
            ret = ret.Replace("5½", "5");
            ret = ret.Replace("4½", "4");
            ret = ret.Replace("3½", "3");
            ret = ret.Replace("2½", "2");
            ret = ret.Replace("1½", "1");
            ret = ret.Replace("0½", "0"); //это 10½
            ret = ret.Replace('½', '0'); //а это просто ½

            return ret;
        }
        protected Gender getGenderFromNumeration(string _line)
        {
            Match lineBeginningMatch;

            lineBeginningMatch = Regex.Match(_line, "^[0-9\\.]*\\s+[0-9\\.]+\\s+\\p{Pd}+\\s+[\\p{L}\\(\\)]+");

            if (lineBeginningMatch.Success)
                return Gender.M;

            lineBeginningMatch = Regex.Match(_line, "^[0-9\\.]*\\s+\\p{Pd}+\\s+[0-9\\.]+\\s+[\\p{L}\\(\\)]+");
            

            if (lineBeginningMatch.Success)
                return Gender.F;

            return Gender.undefined;
        }
        protected string getGenderAge(string _line)
        {
            Match ageMatch;

            //возраст мужчины - буквы, возможно отчество в скобках, возможно, точка после отчества
            //потом пробелы и сразу за пробелами возраст
            //После возраста не идет "-го", иначе это фраза "2-го брака", "1-го брака", и не идет "брака" (без -го)
            ageMatch = Regex.Match(_line, @"\p{L}+\s*(?:\(\p{L}+\))*\.?\s*([0-9]+(?!\sбр|\p{Pd}го)\s?(мес|нед)?)");

            if (ageMatch.Success)
            {
                gender = Gender.M;
                Group g = ageMatch.Groups[1];
                return g.Value.Trim();
            }

            //возраст женщины - буквы, возможно отчество в скобках, возможно, точка после отчества
            //потом пробелы, тире, пробелы, возраст
            //После возраста не идет "-го", иначе это фраза "2-го брака", "1-го брака", и не мдет "брака" (без -го)
            ageMatch = Regex.Match(_line, @"\p{L}+\s*(?:\(\p{L}+\))*\.?\s*\p{Pd}+\s+([0-9]+(?!\sбр|\p{Pd}го)\s?(мес|нед)?)");

            if (ageMatch.Success)
            {
                gender = Gender.F;
                Group g = ageMatch.Groups[1];
                return g.Value.Trim();
            }

            gender = Gender.undefined;
            return "";
        }
        protected string replaceNonAlpha(string _line)
        {
            _line = Regex.Replace(_line, "[^а-яА-Яa-zA-Z]+", " ");
            return _line;
        }
        public virtual void processLine(string _line) { }
        public virtual void processLineManuallyResolved() { }
        private void readDictionaries()
        {
            DictReader dictReader;

            dictReader = DictReader.construct();
            dictReader.run();

            wordsDict       = dictReader.getWordsDict();
            namesDict       = dictReader.getNamesDict();
            patronimicDict  = dictReader.getPatronimicDict();
            homonymsDict    = dictReader.getHomonymsDict();
        }
        public void init(DataFilesKeeper _dataFilesKeeper)
        {
            dataFilesKeeper         = _dataFilesKeeper;

            manuallyResolved = new Dictionary<Tuple<string, int>, Tuple<string, Gender, string>>();
        }
    }
}
