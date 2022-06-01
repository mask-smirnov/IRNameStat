using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace NameStat
{
    internal class FreqGroupsCalculation
    {
        NameSet nameSetTotal; //общая выборка
        Dictionary<string, NameSet> nameSets; //выборки, для которых мы считаем разбивку по частотным группам

        protected const string seriesLabel = "##Series##"; //Серии данных

        public FreqGroupsCalculation(NameSet _nameSetTotal, Dictionary<string, NameSet> _nameSets)
        {
            nameSets = _nameSets;
            nameSetTotal = _nameSetTotal;
        }
        public void run()
        {
            foreach (NameSet nameSet in nameSets.Values)
            {
                if (nameSet.name == nameSetTotal.name)
                    continue; //пропускаем общую выборку 


                nameSet.calcFrequency();
                nameSet.nameStatCalc.nameFreqCalc();
                nameSet.nameStatCalc.recalcFreqGroupsByReference(nameSetTotal);

                Console.WriteLine("Выборка '{0}'", nameSet.desc);
                nameSet.nameStatCalc.outputFreqGroups(Gender.M);
                nameSet.nameStatCalc.outputFreqGroups(Gender.F);
            }

            this.HTMLFileOutput(Gender.M);
            this.HTMLFileOutput(Gender.F);
        }

        void HTMLFileOutput(Gender _gender)
        {
            string HTMLText;

            if (File.Exists(Config.workingFolder + Config.LinesChartTemplateFilename))
            {
                HTMLText = File.ReadAllText(Config.workingFolder + Config.LinesChartTemplateFilename);

                HTMLText = HTMLText.Replace(seriesLabel, this.makeJSONSeries(_gender));

                //HTMLText = HTMLText.Replace(titleLabel, this.diagramHeader());

                string hTMLFilename = Path.GetTempFileName();
                File.Move(hTMLFilename, hTMLFilename + ".html");
                hTMLFilename = hTMLFilename + ".html";

                File.WriteAllText(hTMLFilename, HTMLText, Encoding.UTF8);

                Console.WriteLine("Создан файл " + hTMLFilename);

                Process.Start(hTMLFilename);
            }
        }

        string makeJSONSeries(Gender _gender)
        {
            string ret = "";

            foreach (NameSet nameSet in nameSets.Values)
            {
                if (nameSet.name == nameSetTotal.name)
                    continue; //пропускаем общую выборку 

                if (ret != "") ret += ",\n";

                ret += nameSet.nameStatCalc.makeFreqGroupsJSONSerie(_gender, nameSet.desc);

            }

            return ret;

        }
    }
}
