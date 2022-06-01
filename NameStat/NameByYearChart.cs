using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace NameStat
{
    internal class NameByYearChart
    {
        Dictionary<NameAndSet, Dictionary<int, decimal>> nameByYearSeries; //статистика частотности имен по годам. Имя+нэймсет -> (год -> частотность)

        protected const string seriesLabel = "##Series##"; //Серии данных
        protected const int ageLimit = 50;
        protected const int roundUpYears = 1;

        public NameByYearChart()
        {
            nameByYearSeries = new Dictionary<NameAndSet, Dictionary<int, decimal>>();
        }

        public void calc(NameSet _nameset, int _documentYear, HashSet<GenderAndName> _names)
        {
            //находим максимальный возраст по всем именам
            int maxOfMaxAge = 0;

            foreach (GenderAndName name in _names)
            {
                int maxAge = _nameset.listOfPeople.Where(n => n.Item1 == name.name).Select(n => n.Item3).Max();

                if (maxAge > maxOfMaxAge)
                    maxOfMaxAge = maxAge;

                nameByYearSeries.Add(new NameAndSet(name.name, name.gender, _nameset.name), new Dictionary<int, decimal>());
            }
            if (maxOfMaxAge > ageLimit)
                maxOfMaxAge = ageLimit;

            for (int i = 0; i < maxOfMaxAge; i++)
            {
                //создаем NameStatCalc с данными на интересующий нас год-возраст
                //NameStatCalc можно создавать без привязки к NameSet
                NameStatCalc nameStatCalc = new NameStatCalc();
                nameStatCalc.init();
                nameStatCalc.listOfPeople = _nameset.listOfPeople.Where(n => n.Item3 == i).ToList();
                nameStatCalc.nameFreqCalc();

                foreach (GenderAndName name in _names)
                {
                    Dictionary<int, decimal> curDict = nameByYearSeries[new NameAndSet(name.name, name.gender, _nameset.name)];

                    Tuple<Gender, string> nameFreqKey = Tuple.Create(name.gender, name.name);

                    int curDictKey = (int)(Math.Floor((decimal)(_documentYear - i)/roundUpYears) * roundUpYears);

                    if (nameStatCalc.nameFreq.ContainsKey(nameFreqKey))
                    {
                        
                        decimal freqValue = nameStatCalc.nameFreq[nameFreqKey].Item2;

                        if (curDict.ContainsKey(curDictKey))
                            curDict[curDictKey] = curDict[curDictKey] + freqValue;
                        else
                            curDict.Add(curDictKey, freqValue);
                    }
                    else
                        if (!curDict.ContainsKey(curDictKey))
                        curDict.Add(curDictKey, 0);
                }
            }
            Console.WriteLine("Рассчитано {0} рядов", nameByYearSeries.Count);
        }

        string makeJSONSeries()
        {
            string ret = "";

            foreach (KeyValuePair<NameAndSet, Dictionary<int, decimal>> nameByYearSerie in nameByYearSeries)
            {
                Dictionary<int, decimal> serie = nameByYearSerie.Value;
                NameAndSet key = nameByYearSerie.Key;

                if (ret != "") ret += ",\n";

                ret += String.Format("{{ text: '{0} {1}', values: [\n", key.nameSetName, key.name);
                bool firstIteration = true;
                foreach (KeyValuePair<int, decimal> point in serie)
                {
                    if (!firstIteration)
                        ret += ",\n";
                    firstIteration = false;

                    string strFreqValue = point.Value.ToString("0.000", CultureInfo.InvariantCulture);

                    ret += String.Format("['{0}', {1}]", point.Key, strFreqValue);
                }

                ret += "\n]\n}";
            }

            return ret;
        }

        public void HTMLFileOutput()
        {
            string HTMLText;

            if (File.Exists(Config.workingFolder + Config.LinesChartTemplateFilename))
            {
                HTMLText = File.ReadAllText(Config.workingFolder + Config.LinesChartTemplateFilename);

                HTMLText = HTMLText.Replace(seriesLabel, this.makeJSONSeries());

                //HTMLText = HTMLText.Replace(titleLabel, this.diagramHeader());

                string hTMLFilename = Path.GetTempFileName();
                File.Move(hTMLFilename, hTMLFilename + ".html");
                hTMLFilename = hTMLFilename + ".html";

                File.WriteAllText(hTMLFilename, HTMLText, Encoding.UTF8);

                Console.WriteLine("Создан файл " + hTMLFilename);

                Process.Start(hTMLFilename);
            }
        }

    }
}
