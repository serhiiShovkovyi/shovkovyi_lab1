using System;
using System.IO;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace Lab1
{
    class Program
    {
        private static string directoryPath = @"../../../../../../../../Desktop/";
        static void Main(string[] args)
        {
            DirectoryInfo directorySelected = new DirectoryInfo(directoryPath);
            //calculateEnthropy("wasteLand.docx");
            //calculateEnthropy("news.docx";
            //calculateEnthropy("spirit.docx");

            fileToBase("wasteLand.docx");
            fileToBase("news.docx");
            fileToBase("spirit.docx");

            calculateEnthropy("wasteLand.docx.b64");
            calculateEnthropy("news.docx.b64");
            calculateEnthropy("spirit.docx.b64");


            CompressToZip(directorySelected);
            CompressToGzip(directorySelected);

            DirectoryInfo di = new DirectoryInfo(directoryPath);
            FileInfo[] fiArr = di.GetFiles();
            Console.WriteLine("The directory {0} contains the following files:", di.Name);
            foreach (FileInfo f in fiArr)
            {
                if (!(f.Name.EndsWith(".docx") || (f.Name.EndsWith(".b64")))) continue;
                Console.WriteLine("The size of {0} is {1} bytes.", f.Name, f.Length);
            }

        }
        private static void CompressToGzip(DirectoryInfo directorySelected)
        {
            foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            {
                if (!fileToCompress.Name.EndsWith(".docx")) continue;
                using (FileStream originalFileStream = fileToCompress.OpenRead())
                {
                    if ((File.GetAttributes(fileToCompress.FullName) &
                       FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                    {
                        using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                        {
                            using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                               CompressionMode.Compress))
                            {
                                originalFileStream.CopyTo(compressionStream);
                            }
                        }
                        FileInfo info = new FileInfo(directoryPath + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz");
                        Console.WriteLine($"Compressed {fileToCompress.Name} from {fileToCompress.Length.ToString()} to {info.Length.ToString()} bytes.");
                    }
                }
            }
        }

        private static void CompressToZip(DirectoryInfo directorySelected)
        {
            foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            {
                string zipPath = directoryPath + fileToCompress.Name + ".zip"; 
                using (FileStream fs = new FileStream(zipPath,FileMode.Create))
                {
                }
                using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                    {
                        archive.CreateEntryFromFile(directoryPath + fileToCompress.Name, fileToCompress.Name);
                    }
            }
        }

        static void calculateEnthropy (string fileName)
        {
            Console.WriteLine("-------Analyzing file {0}", fileName);
            string path = directoryPath + fileName;

            Dictionary<char, int> symbolsDict = new Dictionary<char, int>();
            Dictionary<char, double> frequencyOccurrenceOfChars = new Dictionary<char, double>();

            int totalNumbOfChars = 0;

            try
            {
                char symbol;
                int symbolInInt;
                double oneSymbolFrequency = 0;
                double entropy = 0;
                int symbolsCount = 0;

                using (StreamReader sr = new StreamReader(path))
                {
                    while ((symbolInInt = sr.Read()) != -1)
                    {
                        symbol = (char)symbolInInt;
                        if (!symbolsDict.ContainsKey(symbol))
                            symbolsDict.Add(symbol, 1);
                        else
                        {
                            symbolsDict[symbol] += 1;
                        }
                    }
                }
                foreach (KeyValuePair<char, int> kvp in symbolsDict)
                {
                    totalNumbOfChars += kvp.Value;
                    symbolsCount += 1;
                }
                Console.WriteLine("Symbols count {0}", symbolsCount);
                foreach (KeyValuePair<char, int> kvp in symbolsDict)
                {
                    oneSymbolFrequency = (double)kvp.Value / totalNumbOfChars;
                    frequencyOccurrenceOfChars.Add(kvp.Key, oneSymbolFrequency);
                    entropy -= oneSymbolFrequency * (Math.Log(oneSymbolFrequency) / Math.Log(2));
                }
                //display freq
                foreach (KeyValuePair<char, double> kvp in frequencyOccurrenceOfChars)
                {
                    Console.WriteLine("Char {0} found {1} times, frequency {2}", kvp.Key,symbolsDict[kvp.Key], kvp.Value);
                }
                Console.WriteLine("Average entropy of file {0}: {1} b", fileName, entropy);
                Console.WriteLine("Amount of information (calculated by entropy): {0} b", entropy * symbolsCount);


            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        static void fileToBase(string fileName)
        {
            string path = directoryPath + fileName;
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.UTF8))
            {
                using (StreamWriter sw = new StreamWriter(path + ".b64"))
                {
                    string content = sr.ReadToEnd();
                    sw.Write(Base64Encode(content));
                }
            }
        }

        private static string Base64Encode(string s)
        {
            string bits = "";
            foreach (var character in s)
            {
                bits += Convert.ToString(character, 2).PadLeft(8, '0');
            }

            string base64 = "";

            const byte threeOctets = 24;
            int octetsTaken = 0;
            while (octetsTaken < bits.Length)
            {
                //Bypass elements in a sequence and then returns the remaining elements.
                var currentOctects = bits.Skip(octetsTaken).Take(threeOctets).ToList();

                const byte sixBits = 6;
                int hextetsTaken = 0;
                while (hextetsTaken < currentOctects.Count())
                {
                    IEnumerable<char> chunks = currentOctects.Skip(hextetsTaken).Take(sixBits);
                    hextetsTaken += sixBits;

                    string chunk = new string(chunks.ToArray());
                    string bitString = chunk.Aggregate("", (current, currentBit) => current + currentBit);

                    if (bitString.Length < 6)
                    {
                        bitString = bitString.PadRight(6, '0');
                    }
                    var singleInt = Convert.ToInt32(bitString, 2);

                    base64 += Base64Letters[singleInt];
                }

                octetsTaken += threeOctets;
            }

            // Pad with = for however many octects we have left
            for (var i = 0; i < (bits.Length % 3); i++)
            {
                base64 += "=";
            }

            return base64;
        }

        private static readonly char[] Base64Letters = new[]
         {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T'
         , 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n'
         , 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4' , '5', '6', '7'
         , '8', '9', '+', '/'};
    }
}
