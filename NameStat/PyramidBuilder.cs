using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace NameStat
{
    //Строит поло-возрастную пирамиду
    internal class PyramidBuilder
    {
        public List<Tuple<string, Gender, int, string>> listOfPeople; //список людей - имя, пол, возраст с округлением вниз, локализация 
        public Dictionary<Tuple<Gender, int>, int> pyramid; //пирамида - пол, возраст -> кол-во людей

        public string labelsString;
        public string serie1String;
        public string serie2String;

        protected const string labelsLabel = "##Labels##"; //заголовки по оси Y
        protected const string serie1Label = "##Serie1##"; //серия М
        protected const string serie2Label = "##Serie2##"; //серия Ж
        protected const string legend1Label = "##Legend1##"; //легенда М
        protected const string legend2Label = "##Legend2##"; //легенда Ж
        protected const string titleLabel = "##Title##";  //Заголовок диаграммы

        DataFilesKeeper dataFilesKeeper;
        public int maxAge;

        int sumAgeM, sumAgeF; //Суммы возрастов для подсчета среднего возраста
        int peopleCountM, peopleCountF; //кол-во
        public void run()
        {
            maxAge = 0;
            sumAgeM = 0;
            sumAgeF = 0;
            peopleCountM = 0;
            peopleCountF = 0;

            foreach (Tuple<string, Gender, int, string> person in listOfPeople)
            {
                Tuple<Gender, int> pyramidKey = Tuple.Create(person.Item2, person.Item3);
                if (pyramid.ContainsKey(pyramidKey))
                {
                    pyramid[pyramidKey] = pyramid[pyramidKey] + 1;
                }
                else
                {
                    pyramid[pyramidKey] = 1;
                }

                if (person.Item3 > maxAge)
                    maxAge = person.Item3;

                if (person.Item2 == Gender.M)
                {
                    sumAgeM += person.Item3;
                    peopleCountM++;
                } 
                else
                {
                    sumAgeF += person.Item3;
                    peopleCountF++;
                }
            }

            for (int i = 0; i <= maxAge; i++)
            {
                labelsString += i.ToString();

                Tuple<Gender, int> key = Tuple.Create(Gender.M, i);
                if (pyramid.ContainsKey(key))
                    serie1String += pyramid[key];
                else
                    serie1String += "0";

                key = Tuple.Create(Gender.F, i);
                if (pyramid.ContainsKey(key))
                    serie2String += pyramid[key];
                else
                    serie2String += "0";

                if (i < maxAge)
                {
                    labelsString += ", ";
                    serie1String += ", ";
                    serie2String += ", ";
                }
            }
            Console.WriteLine(pyramid.Count);

            this.createHTMLFile();
        }

        public void init(DataFilesKeeper _dataFilesKeeper)
        {
            dataFilesKeeper = _dataFilesKeeper;
            listOfPeople = new List<Tuple<string, Gender, int, string>>();
            pyramid = new Dictionary<Tuple<Gender, int>, int>();
        }
        public void addPeople(List<Tuple<string, Gender, int, string>> _listOfPeopleAddition)
        {
            listOfPeople = (List<Tuple<string, Gender, int, string>>) listOfPeople.Concat(_listOfPeopleAddition).ToList();
        }
        public void createHTMLFile()
        {
            string HTMLText;

            if (File.Exists(Config.workingFolder + Config.PyramidTemplateFilename))
            {
                HTMLText = File.ReadAllText(Config.workingFolder + Config.PyramidTemplateFilename);

                HTMLText = HTMLText.Replace(labelsLabel, labelsString);
                HTMLText = HTMLText.Replace(serie1Label, serie1String);
                HTMLText = HTMLText.Replace(serie2Label, serie2String);
                HTMLText = HTMLText.Replace(titleLabel, this.diagramHeader());
                HTMLText = HTMLText.Replace(legend1Label, String.Format("Мужчины. Ср. возраст {0:0.0}", peopleCountM == 0 ? 0m : (decimal)sumAgeM / (decimal)peopleCountM));
                HTMLText = HTMLText.Replace(legend2Label, String.Format("Женщины. Ср. возраст {0:0.0}", peopleCountF == 0 ? 0m : (decimal)sumAgeF / (decimal)peopleCountF));

                string hTMLFilename = Path.GetTempFileName();
                File.Move(hTMLFilename, hTMLFilename + ".html");
                hTMLFilename = hTMLFilename + ".html";

                File.WriteAllText(hTMLFilename, HTMLText, Encoding.UTF8);

                Console.WriteLine("Создан файл " + hTMLFilename);
                
                Process.Start(hTMLFilename);
            }
        }

        string diagramHeader()
        {
            return String.Format("Поло-возрастная пирамида (n={0})",
                                 //dataFilesKeeper.descriptionStr(),
                                 listOfPeople.Count);
        }
    }
    
}
