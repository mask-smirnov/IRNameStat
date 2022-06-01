using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NameStat
{
    public enum Gender { M, F, undefined } //Пол
    enum DiffType { abs, rel } //абсолютное или относительное изменение 

    public struct NameAndSet //имя + название неймсета (для хранения рядов частотности имени по годам)
    {
        public string name;
        public Gender gender;
        public string nameSetName;

        public NameAndSet(string _name, Gender _gender, string _nameSetName)
        {
            name = _name;
            gender = _gender;
            nameSetName = _nameSetName;    
        }
    }
    public struct GenderAndName
    {
        public string name;
        public Gender gender;

        public GenderAndName (Gender _gender, string _name)
        {
            name = _name;
            gender = _gender;
        }
    }

    public static class Config
    {
        public static readonly string PyramidTemplateFilename   = @"template_PyramidChart.html";
        public static readonly string TreeMapTemplateFilename   = @"template_TreeMapChart.html";
        public static readonly string LinesChartTemplateFilename= @"template_LinesChart.html";
        public static readonly string listOfFilesFilename       = @"listOfFiles.txt";
        public static readonly string nameSetsFilename          = @"nameSets.txt";

        public static readonly string dictWordsFilename         = @"dictWords.txt";
        public static readonly string dictNamesFilename         = @"dictNames.txt";
        public static readonly string dictPatronimicFilename    = @"dictPatronimic.txt";
        public static readonly string dictHomonymsFilename      = @"dictHomonyms.txt";

        public static readonly string manuallyResolvedFilename = @"manuallyResolved.txt";
        public static string workingFolder; //папка, из которой читаются все файлы, заканчивается на /

        public static readonly decimal namesFreqThreshold = 0.5m; //порог в %%, значения частотности ниже которого не идут в статистику относительных изменений частотности имени

    }
    

    internal class Program
    {
        IRAnalysis_DataConversion   iRAnalysis_DataConversion;
        DataFilesKeeper             dataFilesKeeper;

        public void run()
        {
            ConsoleKeyInfo keyPressed;

            dataFilesKeeper = new DataFilesKeeper();
            dataFilesKeeper.loadFile();

            Console.WriteLine("Press D for dictionary check, I for IR analysis");
            keyPressed = Console.ReadKey(true);
            switch (keyPressed.KeyChar)
            {
                case 'd':
                    IRAnalysis_DictCheck.constructAndRun(dataFilesKeeper);
                    break;
                case 'i':
                    iRAnalysis_DataConversion = IRAnalysis_DataConversion.construct(dataFilesKeeper);
                    iRAnalysis_DataConversion.run();
                    break;
                default:
                    Console.WriteLine("No action selected");
                    Console.WriteLine(keyPressed.KeyChar);
                    break;
            }

            if (iRAnalysis_DataConversion != null && iRAnalysis_DataConversion.resultOK)
            {
                Console.WriteLine("Press P for pyramid builder, N for names frequency calculation, A for name freq analysis, T for names tree map, other key to exit");
                keyPressed = Console.ReadKey(true);
                switch (keyPressed.KeyChar)
                {
                    case 'p':
                        PyramidBuilder pyramidBuilder = new PyramidBuilder();
                        pyramidBuilder.init(dataFilesKeeper);
                        pyramidBuilder.addPeople(iRAnalysis_DataConversion.listOfPeople);
                        pyramidBuilder.run();
                        break;
                    case 'n':
                        NameStatCalc nameStatCalc = new NameStatCalc();
                        nameStatCalc.init();
                        nameStatCalc.addPeople(iRAnalysis_DataConversion.listOfPeople);
                        nameStatCalc.nameFreqCalc();
                        nameStatCalc.freqGroupsCalc();
                        nameStatCalc.outputFreqGroups(Gender.M);
                        nameStatCalc.outputFreqGroups(Gender.F);
                        break;
                    case 'a':
                        ReadNameSetsFile readNameSetsFile = new ReadNameSetsFile();
                        readNameSetsFile.listOfPeople = iRAnalysis_DataConversion.listOfPeople;
                        readNameSetsFile.run();
                        break;
                    case 't':
                        NamesTreeMapBuilder namesTreeMapBuilder = new NamesTreeMapBuilder();
                        namesTreeMapBuilder.listOfPeople = iRAnalysis_DataConversion.listOfPeople;
                        namesTreeMapBuilder.run();
                        break;
                    default:
                        Console.WriteLine("No action selected");
                        Console.WriteLine(keyPressed.KeyChar);
                        break;
                }
            }
            keyPressed = Console.ReadKey(true);

        }
        static void Main(string[] args)
        {
            Program program = new Program();

            if (args.Length != 1)
                throw new Exception("Путь к рабочей папке не передан в первом параметре в командной строке");
            else
                Config.workingFolder = args[0];
            
            program.run();
        }
    }
}