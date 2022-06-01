using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Diagnostics;

namespace NameStat
{
    internal class NamesTreeMapBuilder
    {
        public List<Tuple<string, Gender, int, string>> listOfPeople; //имя, пол, возраст, локализация

        NameStatCalc nameStatCalc;

        protected const string seriesLabel = "##Series##"; //Серия данных

        public void run()
        {
            nameStatCalc = new NameStatCalc();
            nameStatCalc.init();
            nameStatCalc.addPeople(listOfPeople);
            nameStatCalc.nameFreqCalc();

            this.createHTMLFile(Gender.M);
            this.createHTMLFile(Gender.F);
        }
        
        string makeJSONSeries(Gender _gender)
        {
            string ret = "";
            //CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            //cultureInfo.NumberFormat.NumberFormat("N2");

            foreach (KeyValuePair<Tuple<Gender, string>, Tuple<int, decimal>> name 
                     in nameStatCalc.nameFreq.Where(x => (x.Key.Item1 == _gender)).OrderByDescending(x => (x.Value.Item1)))
            {
                if (ret != "")
                    ret = ret + ",\n";

                ret = ret + String.Format("{{text:'{0}', value:{1}}}", name.Key.Item2, name.Value.Item2.ToString("N2", CultureInfo.InvariantCulture));
                //{ text: 'Иван',value: 806}

            }
            return ret;
        }
        public void createHTMLFile(Gender _gender)
        {
            string HTMLText;

            if (File.Exists(Config.workingFolder + Config.TreeMapTemplateFilename))
            {
                HTMLText = File.ReadAllText(Config.workingFolder + Config.TreeMapTemplateFilename);

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

    }
}
