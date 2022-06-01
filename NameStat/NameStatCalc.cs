using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace NameStat
{
    internal class NameStatCalc
    {
        public List<Tuple<string, Gender, int, string>> listOfPeople; //имя, пол, возраст, локализация
        public Dictionary<Tuple<Gender, string>, Tuple<int, decimal>> nameFreq, nameFreqCopy; //частотность имен - пол, имя -> кол-во, частотность в %%
        public Dictionary<Tuple<Gender, int>, Tuple<string, decimal>> freqGroups; //группы частотности - пол, группа -> имена, сумм. част-ть
        public Dictionary<Tuple<Gender, string>, int> freqGroupMembers; //члены группы частотности. Пол, имя -> группа

        int totalM = 0, totalF = 0; //Общее кол-во людей по полам
        decimal maxFreqM = 0m, maxFreqF = 0m, minFreqM = 0m, minFreqF = 0m; //min и max частотность по полам

        public void nameFreqCalc()
        {
            //Если уже рассчитывалось, выходим. Могло быть вызвано другой командой
            if (nameFreq.Count > 0)
                return;
            
            //Первый цикл - подсчет кол-ва
            foreach (Tuple<string, Gender, int, string> person in listOfPeople)
            {
                Tuple<Gender, string> freqKey = Tuple.Create(person.Item2, person.Item1);

                if (person.Item2 == Gender.M) //подсчет кол-ва людей по полам
                    totalM++;
                else
                    totalF++;

                if (nameFreq.ContainsKey(freqKey))
                    nameFreq[freqKey] = Tuple.Create(nameFreq[freqKey].Item1 + 1, 0m);
                else
                    nameFreq[freqKey] = Tuple.Create(1, 0m);

            }
            //Console.WriteLine(nameFreq.Count);

            //Второй цикл - расчет частотности в %%
            nameFreqCopy = new Dictionary<Tuple<Gender, string>, Tuple<int, decimal>>(nameFreq); //создаем копию, т.к. нельзя изменять dictionary внутри цикла по самому себе
            foreach (var name in nameFreqCopy)
            {
                int curTotal = 0;
                decimal curFreq = 0m;

                if (name.Key.Item1 == Gender.M) //подсчет кол-ва людей по полам
                    curTotal = totalM;
                else
                    curTotal = totalF;

                curFreq = (decimal)(name.Value.Item1 * 100m/ (decimal)curTotal);
                nameFreq[name.Key] = Tuple.Create(name.Value.Item1, curFreq);

                if (name.Key.Item1 == Gender.M) //Подсчет максимальной частотности для организации цикла от максимальной до минимальной частотности
                {
                    if (curFreq > maxFreqM)
                        maxFreqM = curFreq;
                }
                else
                {
                    if (curFreq > maxFreqF)
                        maxFreqF = curFreq;
                }
            }

            minFreqM = totalM == 0m ? 0m : 100m / (decimal)totalM;
            minFreqF = totalF == 0m ? 0m : 100m / (decimal)totalF;

            //Console.WriteLine(nameFreq.ToString());

        }
        public void freqGroupsCalc()
        //второй цикл - расчет групп частотности
        //расчет групп частотности исходя из частотностей внутри самой этой группы
        //!!!! рассчитанные значения могут быть пересчитаны в recalcFreqGroupsByReference(), это делает, напр, команда freq_groups_calc

        {
            freqGroups = new Dictionary<Tuple<Gender, int>, Tuple<string, decimal>>(); //группы частотности - пол, группа -> имена, сумм. част-ть
            //инициализируем freqGroup нулями, т.к. иначе записи с нулями не создадутся сами
            if (minFreqM > 0)
                for (int i = this.freqGroup(minFreqM); i <= this.freqGroup(maxFreqM); i++)
                    freqGroups.Add(Tuple.Create(Gender.M, i), Tuple.Create("", 0m));

            if (minFreqF > 0)
                for (int i = this.freqGroup(minFreqF); i <= this.freqGroup(maxFreqF); i++)
                    freqGroups.Add(Tuple.Create(Gender.F, i), Tuple.Create("", 0m));

            IEnumerable<KeyValuePair<Tuple<Gender, string>, Tuple<int, decimal>>> nameFreqSorted;
            nameFreqSorted = nameFreq.OrderByDescending(x => x.Value.Item2);

            foreach (var nameFreqKV in nameFreqSorted)
            {
                var freqGroupCur = this.freqGroup(nameFreqKV.Value.Item2);

                Tuple<Gender, int> freqGroupsKey = Tuple.Create(nameFreqKV.Key.Item1, freqGroupCur);

                int roundDigits = (nameFreqKV.Value.Item2 > 1.1m) ? 1 : 2; //если процент выше 1.1, округляем до 1 знака, иначе до 2

                string namesString = freqGroups[freqGroupsKey].Item1 == ""
                                        ? nameFreqKV.Key.Item2 + "(" + Math.Round(nameFreqKV.Value.Item2, roundDigits) + ")"
                                        : freqGroups[freqGroupsKey].Item1 + ", " + nameFreqKV.Key.Item2 + "(" + Math.Round(nameFreqKV.Value.Item2, roundDigits) + ")";
                decimal sumFreq = freqGroups[freqGroupsKey].Item2 + nameFreqKV.Value.Item2;

                freqGroups[freqGroupsKey] = Tuple.Create(namesString, sumFreq);

                freqGroupMembers.Add(Tuple.Create(nameFreqKV.Key.Item1, nameFreqKV.Key.Item2), freqGroupCur);
            }

            //!!!! рассчитанные значения могут быть пересчитаны в recalcFreqGroupsByReference(), это делает, напр, команда freq_groups_calc

            //Console.WriteLine("Рассчитано групп по частотности:" + freqGroups.Count);
        }
        public void recalcFreqGroupsByReference(NameSet _nameSetTotal)
        //расчет групп частотности исходя из частотностей общей выборки (передается параметром)
        //подразумевается, что текущая выборка входит в общую
        {
            //подразумевается, что run() уже запускался, nameFreq рассчитан, но maxFreqM,maxFreqF,minFreqM,minFreqF рассчитаны не для той выборки
            freqGroups = new Dictionary<Tuple<Gender, int>, Tuple<string, decimal>>(); //группы частотности - пол, группа -> имена, сумм. част-ть

            _nameSetTotal.calcFrequency();
            //Копируем min и max из референсной группы. Они нужны для организации цикла при выводе
            maxFreqM = _nameSetTotal.nameStatCalc.maxFreqM;
            maxFreqF = _nameSetTotal.nameStatCalc.maxFreqF;
            minFreqM = _nameSetTotal.nameStatCalc.minFreqM;
            minFreqF = _nameSetTotal.nameStatCalc.minFreqF;

            foreach (var freqGroupTotal in _nameSetTotal.nameStatCalc.freqGroups) //переделать на freqGroupMembers
            {
                //создаем частотную группу для текущей выборки
                Tuple<Gender, int> freqGroupsKey = Tuple.Create(freqGroupTotal.Key.Item1, //пол
                                                                freqGroupTotal.Key.Item2); //группа
                freqGroups.Add(freqGroupsKey, Tuple.Create("", 0m));

                foreach (var freqGroupMember in _nameSetTotal.nameStatCalc.freqGroupMembers.Where(x => (x.Value == freqGroupTotal.Key.Item2 &&
                                                                                                        x.Key.Item1 == freqGroupTotal.Key.Item1)))
                {
                    //цикл по группам и именам референса
                    //ищем текущее имя в исследуемой выборке

                    string memberName = freqGroupMember.Key.Item2;
                    Gender memberGender = freqGroupMember.Key.Item1;

                    Tuple<Gender, string> nameFreqKey = Tuple.Create(memberGender, memberName);

                    if (nameFreq.ContainsKey(nameFreqKey))
                    {
                        decimal memberFreq = nameFreq[nameFreqKey].Item2;
                        string freqRounding = (memberFreq > 1.1m ? "0.0" : "0.00");

                        decimal sumFreq = freqGroups[freqGroupsKey].Item2 + memberFreq;
                        string namesListStr = freqGroups[freqGroupsKey].Item1;
                        if (namesListStr != "")
                            namesListStr = namesListStr + ", ";
                        namesListStr += String.Format("{0} ({1})", memberName, memberFreq.ToString(freqRounding));

                        freqGroups[freqGroupsKey] = Tuple.Create(namesListStr, sumFreq);
                    }

                }
                //Console.WriteLine("", freqGroup.);
            }
        }

        public void outputFreqGroups(Gender _gen)
        {
            Console.WriteLine("Частотная группа;Имена и частотность в %%");
            for (int i = (_gen == Gender.M ? this.freqGroup(maxFreqM) : this.freqGroup(maxFreqF));
                    i >= (_gen == Gender.M ? this.freqGroup(minFreqM) : this.freqGroup(minFreqF));
                    i--)
            {
                Console.WriteLine(  String.Format(CultureInfo.InvariantCulture,
                                    "Частотность {0}, суммарно {2:0.0}%;{1}",
                                    this.freqGroupRepresentation(i), 
                                    freqGroups[Tuple.Create(_gen, i)].Item1,
                                    Math.Round(freqGroups[Tuple.Create(_gen, i)].Item2, 1)));
            }
            
        }
        protected int freqGroup(decimal _freq)
        {
            //Перевод частотности в логарифмическую частотную группу
            //Если частотность от 1/2 до 1% - группа 0, если от 1 до 2% - группа 1, и т.д.
            //округление вниз

            return (int) Math.Floor(Math.Log((double)_freq, 2.0));
        }
        protected string freqGroupRepresentation(int _fGroup)
        {
            //строковое представление группы частотности, например "От 1/8 до 1/4%"
            if (_fGroup >= 0)
                return "от " + Math.Round(Math.Pow(2.0, (double)_fGroup)) + " до " + Math.Round(Math.Pow(2.0, (double)_fGroup + 1.0)) + "%";
            else if (_fGroup == -1)
                return "от 1/2 до 1%";
            else
                return "от 1/" + Math.Round(Math.Pow(2.0, Math.Abs(_fGroup))) + " до 1/" + Math.Round(Math.Pow(2.0, Math.Abs(_fGroup + 1.0))) + "%";
        }
        public string getMostFrequentName(Gender _gender)
        {
            IEnumerable<KeyValuePair<Tuple<Gender, string>, Tuple<int, decimal>>> nameFreqSorted;
            nameFreqSorted = nameFreq.OrderByDescending(x => x.Value.Item2);

            foreach (var nameFreqKV in nameFreqSorted)
            {
                if (nameFreqKV.Key.Item1 == _gender)
                    return nameFreqKV.Key.Item2;
            }

            throw new Exception("Ошибка при определении наиболее частотного имени");
        }

            public void addPeople(List<Tuple<string, Gender, int, string>> _listOfPeopleAddition)
        {
            listOfPeople = (List<Tuple<string, Gender, int, string>>)listOfPeople.Concat(_listOfPeopleAddition).ToList();
        }

        public string makeFreqGroupsJSONSerie(Gender _gender, string _desc)
        {
            string ret = String.Format("{{ text: '{0}', values: [\n", _desc);
            bool firstIteration = true;

            foreach (var freqGroup in freqGroups.Where(f => (f.Key.Item1 == _gender)).OrderByDescending(f => f.Key.Item2))
            {
                string groupDesc = this.freqGroupRepresentation(freqGroup.Key.Item2);
                string groupFrequencyStr = freqGroup.Value.Item2.ToString("0.000", CultureInfo.InvariantCulture);

                if (!firstIteration)
                    ret += ",\n";
                firstIteration = false;

                ret += String.Format("['{0}', {1}]", groupDesc, groupFrequencyStr);
            }

            ret += "\n]\n}";

            return ret;
        }
        public void init()
        {
            listOfPeople = new List<Tuple<string, Gender, int, string>>();
            nameFreq = new Dictionary<Tuple<Gender, string>, Tuple<int, decimal>>();
            freqGroupMembers = new Dictionary<Tuple<Gender, string>, int>();
        }
    }
}
