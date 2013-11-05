using Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Utilities
{
    public enum PlatformUsed { TabletPC, Wacom };
    public enum SymbolCompleteness { Complete, Partial, HasExtra, Combo };
    public enum DrawingTask { Synthesize, Repeat, Copy };
    public enum GatePart { BackLine, FrontArc, BackArc, Bubble, TopArc, 
        BottomArc, Triangle, TopLine, BottomLine, GreaterThan, Context };
    public enum Gate { AND, OR, NAND, NOR, NOT, NOTBUBBLE, XOR, XNOR, Unknown };
    public enum CircuitClass { Gate, Wire, Label };

    public static class General
    {

        /// <summary>
        /// We believe this to be the standard sample rate.
        /// </summary>
        public static float SAMPLE_RATE = 133.0f;
        public static Guid theTimeGuid = new Guid(10, 11, 12, 10, 0, 0, 0, 0, 0, 0, 0);

        #region Softmax Stuff / File reading

        public static Dictionary<string, double[]> GetAvgsForSoftMax(string avgsFilename)
        {
            Dictionary<string, double[]> avgs = new Dictionary<string, double[]>();
            try
            {
                System.IO.StreamReader reader = new System.IO.StreamReader(avgsFilename);

                // Get the feature names from the first line
                string line = reader.ReadLine();
                string[] features = line.Split(",".ToCharArray());

                line = reader.ReadLine();
                string[] averageValuesStr = line.Split(",".ToCharArray());

                line = reader.ReadLine();
                string[] stdevValuesStr = line.Split(",".ToCharArray());

                for (int i = 0; i < features.Length; i++)
                {
                    double avg, stdev;
                    bool succesAvg = Double.TryParse(averageValuesStr[i], out avg);
                    bool succesStdev = Double.TryParse(stdevValuesStr[i], out stdev);

                    if (succesAvg && succesStdev)
                        avgs.Add(features[i], new double[2] { avg, stdev });

                }

                reader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("General GetAvgsForSoftMax: " + e.Message);
                MessageBox.Show(e.Message + " ## It is recommended that your restart the program.");
            }
            return avgs;
        }

        #endregion

    }
}
