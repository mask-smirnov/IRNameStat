using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace NameStat
{
    internal class IRAnalysis_DataConversion : IRAnalysis
    {
        public int peopleCountM, peopleCountF; 

        //список людей - имя, пол, возраст с округлением вниз, локализация (идентификатор прихода равный имени файла)
        public List<Tuple<string, Gender, int, string>> listOfPeople;  
        public bool resultOK;

        public override void run()
        {
            peopleCountM = 0;
            peopleCountF = 0;
            listOfPeople = new List<Tuple<string, Gender, int, string>>();
            resultOK = false;

            base.run();
            Console.WriteLine("Всего найдено м.п.: " + peopleCountM);
            Console.WriteLine("Всего найдено ж.п.: " + peopleCountF);
            resultOK = true;
        }
        public override void processLine(string _line)
        {
            string alphaOnlyLine;
            string[] wordsOfALine;
            string ageStr;
            string genderStr = "";
            string baseName = "";
            int namesFound = 0;
            Gender genderFromNumeration;

            if (lineCount == 1877) {    } //debug point

            alphaOnlyLine = this.replaceNonAlpha(_line);

            ageStr = getGenderAge(_line);
            genderFromNumeration = getGenderFromNumeration(_line);

            if (gender == Gender.M && genderFromNumeration == Gender.F ||
                gender == Gender.F && genderFromNumeration == Gender.M)
            {
                Console.WriteLine("Некорректно указан пол в строке " + lineCount + " (" +_line + ")");
                gender = Gender.undefined; //чтобы дальше не шла обработка
            }

            if (gender != Gender.undefined) //gender устанавливается в getGenderAge()
            {
                genderStr = (gender == Gender.M) ? "М" : "Ж";

                wordsOfALine = alphaOnlyLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string word in wordsOfALine)
                {
                    if (namesDict.ContainsKey(word))
                    {
                        baseName = namesDict[word];
                        namesFound++;
                    }
                }
            }

            if (namesFound == 1)
            {
                if (gender == Gender.M)
                    peopleCountM++;
                else
                    peopleCountF++;

                if (gender == Gender.M && baseName == "Евил") //breakpoint
                {
                    int a = 0; a++;
                }
                
                //Console.WriteLine(baseName + ", " + ageStr + " (" + genderStr + ") " + ((gender == Gender.M) ? peopleCountM : peopleCountF));
                listOfPeople.Add(new Tuple<string, Gender, int, string>
                                       (baseName,
                                        gender,
                                        this.ageStr2Int(ageStr),
                                        curFilename));
            }
            else if (namesFound == 2)
            {
                //проверяем на кейс "Александра жена Евдокия Иванова", когда мужское имя в род. падеже совпадает с женским. Все такие имена внесены в файл омонимов
                wordsOfALine = alphaOnlyLine.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                string firstName = "";
                foreach (string word in wordsOfALine)
                {
                    if (namesDict.ContainsKey(word))
                    {
                        if (firstName == "")
                        {
                            firstName = word;

                            if (homonymsDict.Contains(word))
                                continue; //если первое слов из 2 есть в словаре омонимов, то это наш случай. Пропускаем слово и ищем следующее
                            else
                            {
                                //Слово есть в словаре, но его нет в словаре омонимов. Не наш случай
                                //Console.WriteLine("Найдено " + namesFound + " имен (" + _line + ")");
                                break;
                            }
                        }
                        else
                        {
                            //Нашлось второе слово, поступаем с ним так же, как с единичным
                            if (gender == Gender.M)
                                peopleCountM++;
                            else
                                peopleCountF++;

                            baseName = namesDict[word];
                            //Console.WriteLine("Найдено 2 имени, {0} и {1}, выбрано второе", firstName, word);
                            //Console.WriteLine(baseName + ", " + ageStr + " (" + genderStr + ") " + ((gender == Gender.M) ? peopleCountM : peopleCountF));

                            listOfPeople.Add(new Tuple<string, Gender, int, string>
                                                   (baseName,
                                                    gender,
                                                    this.ageStr2Int(ageStr),
                                                    curFilename));

                        }
                    }
                }
            }
            else if (namesFound != 1)
            {
                //Console.WriteLine("Найдено " + namesFound + " имен (" + _line + ")");
            }
        }
        public override void processLineManuallyResolved()
        {
            Tuple<string, Gender, string> record = manuallyResolved[Tuple.Create(curFilename, lineCount)];
            string baseName;

            if (record.Item2 == Gender.M)
                peopleCountM++;
            else if (record.Item2 == Gender.F)
                peopleCountF++;
            else
                return;

            if (namesDict.ContainsKey(record.Item1))
            {
                baseName = namesDict[record.Item1];
            }
            else
                throw new Exception(String.Format("Имя {0} не найдено в словаре имен (обработка разобранного вручную)", record.Item1));

            //Console.WriteLine(baseName + "(manually resolved) " + ((record.Item2 == Gender.M) ? peopleCountM : peopleCountF));
            listOfPeople.Add(new Tuple<string, Gender, int, string>
                                           (baseName,
                                            record.Item2,
                                            this.ageStr2Int(record.Item3),
                                            curFilename));
        }
        private int ageStr2Int(string _ageStr)
        {
            int ret;

            //если возраст содержит буквы, то это "5 мес", "1 нед", "полугода" итп, округляем вниз до 0 лет
            if (Regex.IsMatch(_ageStr, "\\p{L}+"))
                return 0;
            else if (Regex.IsMatch(_ageStr, "^[0-9]+$"))
            {
                if (Int32.TryParse(_ageStr, out ret))
                    return ret;
            }

            throw new Exception("Ошибка преобразования возраста в число: (" + _ageStr + ")");
        }

        public static IRAnalysis_DataConversion construct(  DataFilesKeeper _dataFilesKeeper)
        {
            IRAnalysis_DataConversion iRAnalysis = new IRAnalysis_DataConversion();

            iRAnalysis.init(_dataFilesKeeper);
            return iRAnalysis;
        }

    }
}
