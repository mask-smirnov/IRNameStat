using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NameStat
{
    internal class ReadNameSetsFile
    {
        public readonly string DEFINE = "def";      //определение выборки
        public readonly string ADD = "add";         //добавление файла в выборку
        public readonly string COMPARE = "compare"; //сравнение 2 выборок
        public readonly string ALL = "all";     
        public readonly string COMMENT = "//";      //комментарий    
        public readonly string MINGROUP = "minimal_group_search";   //поиск мин. размера группы, начиная с кот. "Иван" перестает быть наиболее частотным именем 
        public readonly string FREQGROUPS = "freq_groups_calc";     //Расчет групп частотности и вывод их на график
        public readonly string NAMEBYYEARCALC = "name_by_year_calc";    //Расчет частотности имени по годам 
        public readonly string NAMEBYYEAROUT = "name_by_year_output";   //Вывод частотности имени по годам 

        public readonly string MALE = "M"; //используется в команде name_by_year_calc
        public readonly string FEMALE = "F";

        public List<Tuple<string, Gender, int, string>> listOfPeople; //имя, пол, возраст, локализация

        Dictionary<string, NameSet> nameSets = new Dictionary<string, NameSet>(); //выборки людей определенные командами def. Имя, класс NameSet

        NameByYearChart nameByYearChart;
        public void run()
        {
            string[] lines;
            string[] lineItems;

            if (File.Exists(Config.workingFolder + Config.nameSetsFilename))
            {
                lines = File.ReadAllLines(Config.workingFolder + Config.nameSetsFilename);
                foreach (string line in lines)
                {
                    if (line.StartsWith(COMMENT) || line == "")
                        continue;
                    else if (line.StartsWith(DEFINE))
                    {
                        this.defineNameSet(line);
                    }
                    else if (line.StartsWith(ADD))
                    {
                        this.addToNameSet(line);
                    }
                    else if (line.StartsWith(COMPARE))
                    {
                        this.compareSets(line);
                    }
                    else if (line.StartsWith(MINGROUP))
                    {
                        this.minGroupSearch(500, 5);
                    }
                    else if (line.StartsWith(FREQGROUPS))
                    {
                        this.freqGroupsCalculation(line);
                    }
                    else if (line.StartsWith(NAMEBYYEARCALC))
                    {
                        this.nameByYearCalc(line);

                    } else if (line.StartsWith(NAMEBYYEAROUT))
                    {
                        this.nameByYearOutput();
                    }
                    else
                        throw new Exception("Неверный формат строки файла наборов имен");

                    lineItems = line.Split(new char[] { '\t' });

                }
            }
        }
        protected void minGroupSearch(int _groupSizeFrom, int _numOfGroupsToFind)
        //_groupSizeFrom - кол-во человек в группе, с которого мы начинаем поиск, уменьшая кол-во человек до тех пор, пока
        //группы не станут такими маленькими, что в некоторых наиболее частотным именем будет какое-то другое имя
        {
            string mostFreqName = "Иван";

            List<Tuple<string, Gender, int, string>> listOfPeopleInGroup = new List<Tuple<string, Gender, int, string>>();

            //цикл от максимального значения вниз до 1, но по факту выходим из него раньше, когда кол-во
            //групп, в которых наиболее часотное имя не является таковым станет равным определенному значению
            for (int groupSize = _groupSizeFrom; groupSize >= 1; groupSize--)
            {
                int numberOfGroupsWDiffName = 0;
                int numberOfGroupsTotal = 0;
                string mostFrequentNames = "";
                int totalCount = 0;
                int count = 0;

                foreach (var person in listOfPeople.Where(x => (x.Item2 == Gender.M)))
                {
                    count++;
                    totalCount++;

                    listOfPeopleInGroup.Add(person);

                    if (count == groupSize)
                    {
                        count = 0;
                        numberOfGroupsTotal++;

                        NameStatCalc nameStatCalc = new NameStatCalc();
                        nameStatCalc.init();
                        nameStatCalc.addPeople(listOfPeopleInGroup);
                        nameStatCalc.nameFreqCalc();

                        string curMostFreqName = nameStatCalc.getMostFrequentName(Gender.M);

                        if (curMostFreqName != mostFreqName)
                        {
                            numberOfGroupsWDiffName++;
                            if (mostFrequentNames == "")
                                mostFrequentNames = curMostFreqName;
                            else
                                mostFrequentNames = mostFrequentNames+ ", " + curMostFreqName;
                        }
                        //Console.WriteLine("С " + (totalCount - groupSize) + " по " + totalCount + ": " + nameStatCalc.getMostFrequentName(Gender.M));
                        //nameStatCalc.outputFreqGroups(Gender.M);

                        listOfPeopleInGroup.Clear();
                    }
                }
                Console.WriteLine("Количество групп по {0} человек, в которых имя {1} не является самым частотным: {2} из {3} ({4})",
                    groupSize, mostFreqName, numberOfGroupsWDiffName, numberOfGroupsTotal, mostFrequentNames);
                
                if (numberOfGroupsWDiffName >= _numOfGroupsToFind)
                    break;
            }


        }
        protected void defineNameSet(string _line)
        {
            //создать набор имен для сравнения
            //формат: def, имя набора, файл, фильтр по возрасту (min-max), название набора
            string[] words = _line.Split('\t');
            if (words.Length != 5)
                throw new Exception(String.Format("Неверный формат команды def {0}", _line));

            NameSet nameSet = new NameSet();
            nameSet.name        = words[1];
            nameSet.filename    = words[2];
            nameSet.desc        = words[4];

            string[] ages = words[3].Split('-');
            if (ages.Length != 2 ||
                !Int32.TryParse(ages[0], out nameSet.minAge) ||
                !Int32.TryParse(ages[1], out nameSet.maxAge))
                throw new Exception(String.Format("Неверный формат диапазона возрастов в команде def {0}", _line));

            nameSet.listOfPeople = listOfPeople.Where(x => (x.Item4 == nameSet.filename))
                                               .Where(x => (x.Item3 >= nameSet.minAge))
                                               .Where(x => (x.Item3 <= nameSet.maxAge))
                                               .ToList();

            nameSets.Add(nameSet.name, nameSet);

            string output = "Создан набор " + nameSet.desc + " (" + nameSet.listOfPeople.Count + ") ";
            /*
            nameSet.calcFrequency(); 
            int c = 0;
            foreach (var name in nameSet.nameStatCalc.nameFreq.Where(x => (x.Key.Item1 == Gender.M)).OrderByDescending(x => x.Value.Item1))
            {
                c++;
                if (c > 3) break;
                output = output + name.Key.Item2 + " ";
            }*/

            Console.WriteLine(output);
        }
        protected void addToNameSet(string _line)
        //добавить в набор имен еще один файл
        //формат: add, имя набора, файл, фильтр по возрасту (min-max)
        {
            string[] words = _line.Split('\t');
            if (words.Length != 4)
                throw new Exception(String.Format("Неверный формат команды add {0}", _line));

            NameSet nameSet = nameSets[words[1]];
            nameSet.listOfPeople = nameSet.listOfPeople.Concat(listOfPeople.Where(x => (x.Item4 == words[2]))
                                                                            .Where(x => (x.Item3 >= nameSet.minAge))
                                                                            .Where(x => (x.Item3 <= nameSet.maxAge))
                                                                            .ToList()).ToList();

            Console.WriteLine("В набор {0} добавлены данные, суммарный объем {1}", nameSet.name, nameSet.listOfPeople.Count);
        }

        protected void compareSets(string _line)
        {
            //сравнение двух наборов
            //формат: compare, имя набора 1, имя набора 2
            string[] words = _line.Split('\t');
            if (words.Length != 3)
                throw new Exception(String.Format("Неверный формат команды compare ({0})", _line));

            Console.WriteLine("Сравнение наборов '{0}' и '{1}'", nameSets[words[1]].desc, nameSets[words[2]].desc);

            NameSetsComparison nameSetsComparison = new NameSetsComparison(nameSets[words[1]], nameSets[words[2]]);
            nameSetsComparison.run();
        }
        
        protected void freqGroupsCalculation(string _line)
        {
            //подсчет объема в разбивке по группам частотности
            //имена относятся к группам частотности согласно их частотности в суммарной выборке (ее имя передается параметром) 
            //затем для всех определенных def-ом выборок рассчитывается объем каждой группы частотности
            //т.о. разбивка по часотностям не совпадает с той, которая рассчитывается в NameStatCalc исходя из частотностей внутри самой выборки

            string[] words = _line.Split('\t');
            if (words.Length != 2)
                throw new Exception(String.Format("Неверный формат команды freq_groups_calc ({0})", _line));

            FreqGroupsCalculation freqGroupsCalculation = new FreqGroupsCalculation(nameSets[words[1]], nameSets);
            freqGroupsCalculation.run();

        }
        void nameByYearCalc (string _line)
        //расчет статистики имен по годам
        //формат команды: name_by_year_calc, имя набора определенного командой def, год документа, имя, пол, имя, пол, ... имя, пол
        //Пол - M или F
        //кол-во имен не ограничено
        //год нужен для пересчета возрастов в календарный год
        //вывод осуществляется отдельной командой name_by_year_calc - для того, чтобы можно было рассчитать статистику по разным неймсетам, а вывести ее на один график
        {
            string[] words = _line.Split('\t');

            if (words.Length < 5)
                throw new Exception(String.Format("Неверный формат команды name_by_year_calc ({0})", _line));

            string namesetName = words[1];
            int documentYear;
            if (!Int32.TryParse(words[2], out documentYear))
                throw new Exception(String.Format("Неверный формат года в команде name_by_year_calc ({0})", _line));

            HashSet<GenderAndName> names = new HashSet<GenderAndName>();
            for (int i = 3; i < words.Length; i = i + 2)
            {
                string name = words[i];
                string genderStr = words[i + 1];
                Gender gender;

                if (genderStr == MALE) gender = Gender.M;
                else if (genderStr == FEMALE) gender = Gender.F;
                else throw new Exception(String.Format("Неверный формат пола в команде name_by_year_calc ({0})", _line));

                names.Add(new GenderAndName(gender, name));
            }

            if (nameByYearChart == null)
                nameByYearChart = new NameByYearChart();

            nameByYearChart.calc(nameSets[namesetName], documentYear, names);
        }

        void nameByYearOutput()
        //name_by_year_output вывод графика в HTML файл
        {
            nameByYearChart.HTMLFileOutput();
        }
    }
}
