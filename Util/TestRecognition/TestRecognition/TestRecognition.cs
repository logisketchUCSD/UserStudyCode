using System;
using System.Collections.Generic;
using System.Text;
using Recognizers;
using Sketch;
using System.IO;
using SymbolRec.Image;

namespace TestRecognition
{
    class TestRecognition
    {
        #region key filepath vars

        // HACK hardcoded location of this project and the data folder that contains the .amat files
        static string m_noConMatrixPath = @"C:\Users\sketchers\Documents\Trunk\Data\TrainingResults\DevinAnton\";
        static string m_symbolPath = @"C:\Users\sketchers\Documents\Trunk\Data\ContextOriented\TEST\";
        static string m_conMatrixPath = @"C:\Users\sketchers\Documents\Trunk\Data\TrainingResults\ContextOriented\";
        static string m_outPath = @"C:\Users\sketchers\Documents\Trunk\Data\TrainingResults\";
      
        static string m_outImageFileName = @"DevonOrientedIm.csv";
        static string m_outRecFileName = @"DevonOrientedRec.txt";
        static string m_matrixPattern = "*.amat";
        static string m_conSymbolPattern = "*.con.xml";
        static string m_nonSymbolPattern = "*.non.xml";
        static string m_testSymbolPattern = m_nonSymbolPattern;
        #endregion

        static void Main(string[] args)
        {
            compareRecognizer();
            compareImagesByTestFile();
            Console.WriteLine("\n FINISHED \n");
            Console.ReadKey();
        }

        #region Using Image Distance
        /// <summary>
        /// Recommended to use compareByTestFile instead
        /// </summary>
        private static void compareImagesByDefn()
        {
            string results = "";//"Def. File,  Comp. File, Haus. Dist. \n\n";
            string matrixResults = "";
            string symbolResults = "";
            //string conSymbolPath = @"C:\Users\sketchers\Documents\Trunk\Data\Context Symbols\TEST";
            List<string> filenames = getFiles(m_conMatrixPath, m_matrixPattern);
            
            foreach(string fileName in filenames)
            {
                matrixResults += "\n" + compareImagesToDefn(m_conMatrixPath, m_matrixPattern, fileName);
                symbolResults += "\n" + compareImagesToDefn(m_symbolPath, m_testSymbolPattern, fileName);
            }

            results += "MATRIX: \n" + matrixResults;
            results += "\n\n" +"SYMBOL: \n" + symbolResults;

            //remove the first part of long filepaths for better readability
            results = results.Replace(m_symbolPath, "");
            results = results.Replace(m_conMatrixPath, "");

          
            writeLog(results, m_outPath + m_outImageFileName);
        }

        private static void compareImagesByTestFile()
        {
            
            #region context
            string conResults = "";//Def. File,  Comp. File, Haus. Dist. \n\n";
            string matrixResults = "";
            string symbolResults = "";
            List<string> defFileNames = getFiles(m_conMatrixPath, m_matrixPattern);
            matrixResults += "\n" + compareImagesByTestFile(m_conMatrixPath, m_matrixPattern, defFileNames);
            symbolResults += "\n" + compareImagesByTestFile(m_symbolPath, m_testSymbolPattern, defFileNames);

            conResults += "MATRIX: \n" + matrixResults;
            conResults += "\n\n" + "SYMBOL: \n" + symbolResults;

            //remove the first part of long filepaths for better readability
            
            #endregion

            #region nonContext

            string nonResults = "";//"Def. File,  Comp. File, Haus. Dist. \n\n";
            matrixResults = "";
            symbolResults = "";
            
            defFileNames = getFiles(m_noConMatrixPath, m_matrixPattern);
            matrixResults += "\n" + compareImagesByTestFile(m_noConMatrixPath, m_matrixPattern, defFileNames);
            symbolResults += "\n" + compareImagesByTestFile(m_symbolPath, m_testSymbolPattern, defFileNames);

            nonResults += "MATRIX: \n" + matrixResults;
            nonResults += "\n\n" + "SYMBOL: \n" + symbolResults;
            
            
            #endregion


            string results = "CONTEXT \n\n" + conResults + "\nNO CONTEXT\n\n" + nonResults;

            //remove the first part of long filepaths for better readability
            results = results.Replace(m_conMatrixPath, "");
            results = results.Replace(m_symbolPath, "");
            results = results.Replace(m_noConMatrixPath, "");
            results = results.Replace(m_symbolPath, "");

            writeLog(results, m_outPath + m_outImageFileName);

        }

        private static string compareImagesByTestFile(string dir, string pattern, List<string> defFileNames)
        {
            // "-r" = recursively search subdirectories for files that match pattern
            List<string> testFiles = getFiles(dir, pattern);
            
            //Set up column headings
            string output = "Testfile , ";
            foreach (string df in defFileNames)
            {
                output += df + ", ";
            }
            output += "Best Guess, minDist, \n";
           
            foreach (string tf in testFiles)
            {
                Console.WriteLine(tf);
                Image testImage = new Image();
                testImage.LoadImage(tf);
                output += tf + ", ";
                double minDistance = Double.MaxValue;
                string bestGuess = "NONE";
                foreach (string df in defFileNames)
                {
                    Image defImage = new Image(32, 32);
                    defImage.LoadImage(df);
                    Metrics.ImageDistance id = new Metrics.ImageDistance(defImage, testImage);
                    double distance = id.distance(Metrics.ImageDistance.HAUSDORFF);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestGuess = df;
                    }
                    output += distance + ", "; 
                }
                output += bestGuess + ", " + minDistance + ", \n";
            }
            return output;

        }

        private static string compareImagesToDefn(string dir, string pattern, string defFileName)
        {
            // "-r" = recursively search subdirectories for files that match pattern
            List<string> filenames = getFiles(dir, pattern);

            Console.WriteLine("Compare images to:  " + defFileName);
            Image defImage = new Image(32, 32);
            defImage.LoadImage(defFileName);

            string output = "";
            foreach (string fileName in filenames)
            {
                Console.WriteLine(fileName);
                Image testImage = new Image();
                testImage.LoadImage(fileName);
                Metrics.ImageDistance id = new Metrics.ImageDistance(defImage, testImage);
                double distance = id.distance(Metrics.ImageDistance.HAUSDORFF);
               // output += defFileName.Replace(@"C:\Users\sketchers\Documents\Trunk\Data\TrainingResults\Context","") + ", ";
               // output += fileName.Replace(@"C:\Users\sketchers\Documents\Trunk\Data\Context Symbols\TEST","") + ", ";
                output += defFileName + ", ";
                output += fileName + ", ";
               
                output +=  distance + ", \n";
            }
            return output;

        }
        #endregion

        #region using the recognizer
        private static void compareRecognizer()
        {
            GateRecognizer gr;

            Console.WriteLine("Loading files for recognizer");


            gr = new GateRecognizer(m_conMatrixPath + @"\gate.model",
                new string[] { 
                m_conMatrixPath + @"\and.amat", 
                m_conMatrixPath + @"\nand.amat", 
                m_conMatrixPath + @"\nor.amat", 
                m_conMatrixPath + @"\not.amat", 
                m_conMatrixPath + @"\or.amat" }, 32, 32);
            string nonResults = getContextResults(gr);

            
            gr = new GateRecognizer(m_noConMatrixPath + @"\gate.model",
                new string[] { 
                m_noConMatrixPath + @"\and.amat", 
                m_noConMatrixPath + @"\nand.amat", 
                m_noConMatrixPath + @"\nor.amat", 
                m_noConMatrixPath + @"\not.amat", 
                m_noConMatrixPath + @"\or.amat" }, 32, 32);
            string conResults = getNoContextResults(gr);

            writeLog(nonResults + conResults, m_outPath + m_outRecFileName); 
        }


        private static string getContextResults(GateRecognizer gr)
        {
            string results = "\n CONTEXT \n";
            string dir;
            Console.WriteLine("\nAND results:\n");
            results += "\nAND results:\n";
            dir = m_symbolPath + @"and\";
            results += recognizeFiles(dir, m_conSymbolPattern, gr);

            Console.WriteLine("\nNAND results:\n");
            results += "\nNAND results:\n";
            dir = m_symbolPath + @"nand\";
            results += recognizeFiles(dir, m_conSymbolPattern, gr);

            Console.WriteLine("\nNOR results:\n");
            results += "\nNOR results:\n";
            dir = m_symbolPath + @"nor\";
            results += recognizeFiles(dir, m_conSymbolPattern, gr);

            Console.WriteLine("\nNOT results:\n");
            results += "\nNOT results:\n";
            dir = m_symbolPath + @"not\";
            results += recognizeFiles(dir, m_conSymbolPattern, gr);

            Console.WriteLine("\nOR results:\n");
            results += "\nOR results:\n";
            dir = m_symbolPath + @"or\";
            results += recognizeFiles(dir, m_conSymbolPattern, gr);

            //Console.WriteLine("\nXNOR results:\n");
            //results += "\nXNOR results:\n";
            //dir = m_conSymbolPath + @"xnor\";
            //results += recognizeFiles(dir, m_symbolPattern);

            return results;

        }

        private static string getNoContextResults(GateRecognizer gr)
        {
            string dir;
            string results = "\n NO CONTEXT";
            Console.WriteLine("\nAND results:\n");
            results += "\nAND results:\n";
            dir = m_symbolPath + @"and";
            results += recognizeFiles(dir, m_nonSymbolPattern, gr);

            Console.WriteLine("\nNAND results:\n");
            results += "\nNAND results:\n";
            dir = m_symbolPath + @"nand\";
            results += recognizeFiles(dir, m_nonSymbolPattern, gr);

            //Console.WriteLine("\nNONGATE results:\n");
            //dir = m_conSymbolPath + @"nongate\";
            //results += recognizeFiles(dir, m_nonSymbolPattern);

            Console.WriteLine("\nNOR results:\n");
            results += "\nNOR results:\n";
            dir = m_symbolPath + @"nor\";
            results += recognizeFiles(dir, m_nonSymbolPattern, gr);

            Console.WriteLine("\nNOT results:\n");
            results += "\nNOT results:\n";
            dir = m_symbolPath + @"not\";
            results += recognizeFiles(dir, m_nonSymbolPattern, gr);

            Console.WriteLine("\nOR results:\n");
            results += "\nOR results:\n";
            dir = m_symbolPath + @"or\";
            results += recognizeFiles(dir, m_nonSymbolPattern, gr);

            return results;
            
        }

        private static string recognizeFiles(string dir, string pattern, GateRecognizer gr)
        {
           // "-r" = recursively search subdirectories for files that match pattern
            List<string> filenames = getFiles(dir, pattern);

            string output = "";
            if (filenames == null)
            {
                Console.WriteLine("Error, no files found in directory {0} with pattern {1}", dir, pattern);
                return "";
            }
            foreach (string fileName in filenames)
            {
                ConverterXML.ReadXML reader = new ConverterXML.ReadXML(fileName);
                Substroke[] strokes = reader.Sketch.Substrokes;
                Recognizer.Results results = gr.Recognize(strokes);
                output += fileName + "\n\t";
                Console.WriteLine(fileName);
                output += results.ToString() + "\n";
            }
            return output;
        }
        #endregion

        #region util
        private static void writeLog(string text, string path)
        {
            TextWriter tw = new StreamWriter(path);
            // write a line of text (present date/time) to the file
            tw.WriteLine(DateTime.Now);
            // write the rest of the text lines
            tw.Write(text);
            // close the stream
            tw.Close();
        }

        /// <summary>
        /// Gets files in directory dir (and all subdirectories) where the filename matches pattern
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private static List<string> getFiles(string dir, string pattern)
        {
            Files.Files fileClass = new Files.Files(new string[] { dir, pattern, "-r" });
            List<string> filenames = fileClass.GetFiles;
            return filenames;
        }
        #endregion



    }
}
