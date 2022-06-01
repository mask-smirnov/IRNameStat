using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NameStat
{
    internal class NameSetsComparison
    {
        NameSet nsStudied, nsReference; //исследуемый образец и референс для сравнения

        Dictionary<Tuple<Gender, string>, Tuple<decimal, decimal>> diff; //пол, имя - > абс. изменение, отн. изменение
        //абс. изменение - на сколько изменился процент частотности (+, если вырос в исследуемом образце)
        //отн. изменение - во сколько раз изменился процент частотности (больше 1, если вырос в исследуемом образце)

        HashSet<Tuple<Gender, string>> newNames; //новые имена (кот. нет в реф. выборке). Пол, имя

        public Dictionary<Gender, decimal> distance; //расстояние между наборами по манхэттенской метрике, отдельно по полам. Считается по частям в 2 методах, calcDiff() и calcDistance()

        public NameSetsComparison(NameSet _nameSetStudied, NameSet _nameSetReference)
        {
            nsStudied = _nameSetStudied;
            nsReference = _nameSetReference;

            distance = new Dictionary<Gender, decimal>();
            distance.Add(Gender.M, 0m);
            distance.Add(Gender.F, 0m);

            diff = new Dictionary<Tuple<Gender, string>, Tuple<decimal, decimal>>();
            newNames = new HashSet<Tuple<Gender, string>>();
        }
        public void run()
        {
            nsStudied.calcFrequency();
            nsReference.calcFrequency();

            this.calcDiff();
            this.calcDistance();
            this.outputDistance();
            this.output(10);
        }

        void calcDiff()
        {
            foreach (var name in nsStudied.nameStatCalc.nameFreq)
            {
                decimal referenceFreq = 0m;
                decimal relDiff = 0m;

                if (nsReference.nameStatCalc.nameFreq.ContainsKey(name.Key)) //в референсной выборке может не быть этого имени
                {
                    referenceFreq = nsReference.nameStatCalc.nameFreq[name.Key].Item2;
                    relDiff = name.Value.Item2 / referenceFreq;

                }
                else
                    newNames.Add(name.Key);

                decimal absDiff = name.Value.Item2 - referenceFreq;

                diff.Add(name.Key, Tuple.Create(absDiff, relDiff));

                distance[name.Key.Item1] = distance[name.Key.Item1] + Math.Abs(absDiff);
            }
        }
        public void output(int _limit = 10)
        {
            Console.WriteLine("Top-{0} отличий частотности мужских имен, абсолютно (разность между процентами частотности)", _limit);
            this.outputGenderType(Gender.M, DiffType.abs, _limit);
            Console.WriteLine("Top-{0} отличий частотности мужских имен, относительно (увеличение процента в разах)", _limit);
            this.outputGenderType(Gender.M, DiffType.rel, _limit);
            Console.WriteLine("Top-{0} отличий частотности женских имен, абсолютно (разность между процентами частотности)", _limit);
            this.outputGenderType(Gender.F, DiffType.abs, _limit);
            Console.WriteLine("Top-{0} отличий частотности женских имен, относительно (увеличение процента в разах)", _limit);
            this.outputGenderType(Gender.F, DiffType.rel, _limit);
        }
        protected void outputGenderType(Gender _gender, DiffType _diffType, int _limit)
        {
            string ret;
            int c = 0;
            ;
            foreach (var difference in diff.Where(d => (d.Key.Item1 == _gender))
                                           .OrderByDescending(d => (_diffType == DiffType.abs) ? 
                                           Math.Abs(d.Value.Item1) : //при подсчете абсолютных изменений берем и плюс, и минус в одну кучу
                                           d.Value.Item2 > 1 ? d.Value.Item2 : 1 / d.Value.Item2)) //для относительных то же самое
            {
                decimal refFrequency = nsReference.nameStatCalc.nameFreq[difference.Key].Item2;
                decimal curFrequency   = nsStudied.nameStatCalc.nameFreq[difference.Key].Item2;

                string decimalFormat = (curFrequency > 1.1m) ? "0.0" : "0.00";

                if (_diffType == DiffType.rel &&
                    refFrequency < Config.namesFreqThreshold &&
                    curFrequency < Config.namesFreqThreshold)
                    continue; //Если частотность в обеих выборках ниже пороговой, не берем ее в top по относительному изменению
                //Если частотность была ниже пороговой, а стала выше (или наоборот), то берем

                c++;
                if (c > _limit) break;

                ret = difference.Key.Item2 + ": ";

                if (_diffType == DiffType.abs)
                    ret = ret + (difference.Value.Item1 > 0 ? "+" : "") + difference.Value.Item1.ToString(decimalFormat) + "%";
                else
                    ret = ret + difference.Value.Item2.ToString(decimalFormat) + " раза";

                ret = ret + " (" + curFrequency.ToString(decimalFormat) + "%)";

                Console.WriteLine(ret);
            }
        }
        
        void calcDistance()
        //подсчет расстояния между двумя наборами по манхэттенской метрике (сумма модулей разностей между частотностями каждого имени
        //подразумевается, что запускался calcDiff() - в нем расчет по именам, которые входят в текущий набор
        //здесь только расчет по именам, которые есть в референсном наборе, но отсутствуют в текущем
        {
            foreach (var name in nsReference.nameStatCalc.nameFreq)
            {
                if (!nsStudied.nameStatCalc.nameFreq.ContainsKey(name.Key))
                {
                    distance[name.Key.Item1] = distance[name.Key.Item1] + name.Value.Item2;
                }
            }
        }
        public void outputDistance()
        {
            Console.WriteLine("Манхэттенское расстояние между мужскими именниками {0}, женскими {1}, суммарно {2}",
                distance[Gender.M].ToString("0.0"), 
                distance[Gender.F].ToString("0.0"),
                (distance[Gender.M] + distance[Gender.F]).ToString("0.0"));
        }
    }
}
