using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnpackFarc
{
    class Program
    {

        static List<int> fileOffsets = new List<int>();
        static List<byte> fileDataArray = new List<byte>();
        static List<string> fileNameList = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Border Break FARC lame packer by nzgamer41");

            if (!ValidateArgs(args))
                return;

            buildFile(args[0], args[1]);
        }

        private static bool ValidateArgs(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: PackFarc.exe <folder you want to compress> <output filename>");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets number of files in a directory and returns it as a byte array
        /// </summary>
        /// <param name="fileDir">directory where the files are</param>
        /// <returns>byte[]</returns>
        private static byte[] fileCount(string fileDir)
        {
            string[] files = Directory.GetFiles(fileDir);
            byte[] num = BitConverter.GetBytes(files.Length);
            return num;
        }

        private static void buildFile(string fileDir, string outputFile)
        {
            byte[] header = buildHeader(fileDir);
            byte[] fileNames = buildFileNames(fileDir);
            byte[] fileData = fileDataGen(fileDir);

            //first 16 bytes are the initial header stuff
            int arrayBytes = 18;

            //time to work out how big this mf gonna be
            foreach (int i in fileOffsets)
            {
                arrayBytes += 16;
            }

            arrayBytes += fileNames.Length;
            arrayBytes += fileData.Length;

            byte[] finalFile = new byte[arrayBytes];

            int byteCounter = 0;
            foreach (byte b in header)
            {
                finalFile[byteCounter] = b;
                byteCounter++;
            }

            //skipping last 4 bytes of array since we're gonna have to set that later
            byteCounter = 16;
            int fileNameOffset = (byteCounter + (16 * fileOffsets.Count));
            int realOffset = (fileNames.Length + (fileOffsets.Count * 16) + 16 + 2);
            for (int k = 0; k < fileOffsets.Count; k++)
            {
                byte[] conv = BitConverter.GetBytes(fileNameOffset);

                foreach (byte b in conv)
                {
                    finalFile[byteCounter] = b;
                    byteCounter++;
                }

                //offset is size, ignore wording

                

                byte[] offset = BitConverter.GetBytes(realOffset);

                foreach (byte bb in offset)
                {
                    finalFile[byteCounter] = bb;
                    byteCounter++;
                }

                int size = fileOffsets[k];
                byte[] sizeBytes = BitConverter.GetBytes(size);
                foreach (byte sb in sizeBytes)
                {
                    finalFile[byteCounter] = sb;
                    byteCounter++;
                }

                //DO IT AGAIN

                foreach (byte sb in sizeBytes)
                {
                    finalFile[byteCounter] = sb;
                    byteCounter++;
                }

                realOffset += fileOffsets[k];
                fileNameOffset += fileNameList[k].Length + 1;
            }

            //ok we should be good to write filenames now
            foreach (byte b in fileNames)
            {
                finalFile[byteCounter] = b;
                byteCounter++;
            }

            byteCounter += 2;

            //we're gonna set those 4 bytes from earlier!
            int firstFile = byteCounter - 16;
            byte[] firstFileBytes = BitConverter.GetBytes(firstFile);
            int temp = 12;
            foreach (byte ff in firstFileBytes)
            {
                finalFile[temp] = ff;
                temp++;
            }

            //offsets ew


            //ok writing file data now!
            foreach (byte fb in fileData)
            {
                finalFile[byteCounter] = fb;
                byteCounter++;
            }

            //File should be finished now, time to try fix up stuff


            File.WriteAllBytes(outputFile, finalFile);
            Console.WriteLine("Successfully wrote out file: " + outputFile + " to current directory.");

        }

        private static byte[] buildFileNames(string fileDir)
        {
            List<byte> fileNames = new List<byte>();
            string[] files = Directory.GetFiles(fileDir);
            foreach (string file in files)
            {
                string fileWithoutDir = Path.GetFileName(file);
                char[] fileName = fileWithoutDir.ToCharArray();
                foreach (char c in fileName)
                {
                    fileNames.Add((byte) c);
                }

                fileNames.Add(0x00);
                fileNameList.Add(fileWithoutDir);
            }

            return fileNames.ToArray();
        }

        private static byte[] buildHeader(string fileDir)
        {
            byte[] tempArray = new byte[12];
            tempArray[0] = (byte) 'F';
            tempArray[1] = (byte) 'A';
            tempArray[2] = (byte) 'R';
            tempArray[3] = (byte) 'C';
            byte[] tempCount = fileCount(fileDir);
            int i = 4;
            foreach (byte b in tempCount)
            {
                tempArray[i] = b;
                i++;
            }

            tempArray[8] = 0x10;
            tempArray[9] = 0x00;
            tempArray[10] = 0x00;
            tempArray[11] = 0x00;
            return tempArray;
        }

        private static byte[] fileDataGen(string fileDir)
        {
            string[] files = Directory.GetFiles(fileDir);


            foreach (string file in files)
            {
                byte[] fileRead = File.ReadAllBytes(file);
                foreach (byte b in fileRead)
                {
                    fileDataArray.Add(b);
                }

                fileOffsets.Add(fileRead.Length);
            }

            byte[] dataToReturn = fileDataArray.ToArray();
            return dataToReturn;
        }
    }
}
