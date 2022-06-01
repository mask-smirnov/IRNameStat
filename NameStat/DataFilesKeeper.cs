using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NameStat
{
    public class DataFile
    {
        internal string FileName;
        internal string Description;
    }
    public class DataFilesKeeper
    {
        public List<DataFile> dataFiles;

        internal bool containsFilename(string _filename)
        {
            foreach (DataFile file in dataFiles)
                if (file.FileName == _filename)
                    return true;

            return false;
        }
        public string descriptionStr()
        {
            string ret = "";

            foreach (DataFile file in dataFiles)
            {
                if (ret == "")
                    ret = file.Description;
                else
                    ret = ret + ", " + file.Description;
            }
            return ret;
        }
        internal void loadFile()
        {
            string[] lines;
            string[] words;

            if (File.Exists(Config.workingFolder + Config.listOfFilesFilename))
            {
                dataFiles = new List<DataFile>();

                lines = File.ReadAllLines(Config.workingFolder + Config.listOfFilesFilename);
                foreach (string line in lines)
                {
                    if (line.StartsWith("//")) //можно комментировать строки файла
                        continue;

                    words = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (words.Length > 0)
                    {
                        DataFile dataFile = new DataFile();
                        dataFile.FileName = words[0];
                        dataFile.Description = words[1];

                        dataFiles.Add(dataFile);

                        Console.WriteLine("Файл к обработке: {0} ({1})", dataFile.FileName, dataFile.Description);
                    }
                        else
                            throw new Exception("Некорректный формат файла списка файлов к обработке");
                }
            }
            else
                throw new Exception("Не найден файл списка файлов к обработке");
                
        }
    }
}
