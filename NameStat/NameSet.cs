using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameStat
{
    internal class NameSet
    {
        public string name; //идентификатор
        public string desc; //описание в человеко-читаемом виде
        public string filename; //имя файла 
        public int maxAge = 0; //макс возраст включительно
        public int minAge = 0; //мин возраст включительно

        public List<Tuple<string, Gender, int, string>> listOfPeople; //имя, пол, возраст, локализация

        public NameStatCalc nameStatCalc;

        public void calcFrequency()
        {
            nameStatCalc = new NameStatCalc();
            nameStatCalc.init();
            nameStatCalc.addPeople(listOfPeople);
            nameStatCalc.nameFreqCalc();
            nameStatCalc.freqGroupsCalc();

        }
    }
}
