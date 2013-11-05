using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Utilities;

namespace ReadMySQLoutput
{
    class Program
    {
        enum ReadState { TotalInstances, NumStrokeCounts, NumTouchupCounts, StrokeOrderCounts };
        enum TaskState { Isolated, Copy, Synthesize, Ensemble };

        static void Main(string[] args)
        {
            string inFile = "C:\\Documents and Settings\\eric\\My Documents\\Research\\Current\\Desktop\\StrokeOrderData\\outIndUsers.txt";
            string outFile = "C:\\Documents and Settings\\eric\\My Documents\\Research\\Current\\Desktop\\StrokeOrderData\\tableIndUsers.txt";
            string out2File = "C:\\Documents and Settings\\eric\\My Documents\\Research\\Current\\Desktop\\StrokeOrderData\\tableIndUsers_Console.txt";

            GetResults(inFile, outFile, out2File);
            
            
            /*
            string shapeName = "NA";
            Dictionary<string, int> numTotal = new Dictionary<string, int>();
            Dictionary<string, Dictionary<int, int>> numStrokes = GetNumStrokesDictionary();
            Dictionary<string, Dictionary<int, int>> numTouchUps = GetNumTouchUpsDictionary();
            Dictionary<string, Dictionary<string, int>> numOrders = new Dictionary<string, Dictionary<string, int>>();


            

            ReadState state = ReadState.TotalInstances;
            StreamReader reader = new StreamReader(inFile);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Check for header
                if (line.Contains("COUNT(*)"))
                {
                    state = GetState(line);
                    continue;
                }

                switch (state)
                {
                    case ReadState.TotalInstances:
                        ReadTotalInstances(line, ref numTotal, ref shapeName);
                        break;
                    case ReadState.NumStrokeCounts:
                        ReadNumStrokes(line, ref numStrokes);
                        break;
                    case ReadState.NumTouchupCounts:
                        ReadNumTouchUps(line, ref numTouchUps);
                        break;
                    case ReadState.StrokeOrderCounts:
                        ReadStrokeOrders(line, ref numOrders);
                        break;
                    default:
                        break;
                }
            }

            reader.Close();

            int total = 0;
            foreach (int subtotal in numTotal.Values)
                total += subtotal;

            if (!numTotal.ContainsKey("Isolated"))
                numTotal.Add("Isolated", 0);
            if (!numOrders.ContainsKey("Isolated"))
                numOrders.Add("Isolated", new Dictionary<string, int>());

            foreach (string order in numOrders["Ensemble"].Keys)
            {
                if (!numOrders["Isolated"].ContainsKey(order))
                    numOrders["Isolated"].Add(order, 0);

                if (!numOrders["Copy"].ContainsKey(order))
                    numOrders["Copy"].Add(order, 0);

                if (!numOrders["Synthesize"].ContainsKey(order))
                    numOrders["Synthesize"].Add(order, 0);
            }


            StreamWriter writer = new StreamWriter(outFile);

            //writer.WriteLine("\tAll");
            
            writer.WriteLine(shapeName + "\tEnsemble\tIsolated\tCopy\tSynthesize");
            
            writer.WriteLine("Num Instances Total\t" + 
                total + "\t" + 
                numTotal["Isolated"] + "\t" +
                numTotal["Copy"] + "\t" +
                numTotal["Synthesize"]);

            for (int num = 1; num < 10; num++)
            {
                writer.WriteLine("Num w/ " + num + " Stroke(s)\t" +
                    numStrokes["Ensemble"][num] + "\t" +
                    numStrokes["Isolated"][num] + "\t" +
                    numStrokes["Copy"][num] + "\t" +
                    numStrokes["Synthesize"][num]);
            }

            for (int num = 1; num < 5; num++)
            {
                writer.WriteLine("Num w/ " + num + " TouchUp(s)\t" +
                    numTouchUps["Ensemble"][num] + "\t" +
                    numTouchUps["Isolated"][num] + "\t" +
                    numTouchUps["Copy"][num] + "\t" +
                    numTouchUps["Synthesize"][num]);
            }

            foreach (string order in numOrders["Ensemble"].Keys)
            {
                writer.WriteLine(order + "\t" +
                    numOrders["Ensemble"][order] + "\t" +
                    numOrders["Isolated"][order] + "\t" +
                    numOrders["Copy"][order] + "\t" +
                    numOrders["Synthesize"][order]);
            }

            writer.Close();
            */
        }

        private static void GetResults(string inFile, string outFile, string out2File)
        {
            StreamWriter writer2 = new StreamWriter(out2File);

            List<string> tasknames = new List<string>();
            tasknames.Add("Ensemble");
            tasknames.Add("Isolated");
            tasknames.Add("Copy");
            tasknames.Add("Synthesize");
            tasknames.Add("CScomb");

            List<string> shapenames = new List<string>();
            shapenames.Add("AND");
            shapenames.Add("OR");
            shapenames.Add("NOT");
            shapenames.Add("NAND");
            shapenames.Add("NOR");
            shapenames.Add("XOR");
            shapenames.Add("NOTBUBBLE");
            shapenames.Add("LabelBox");

            // user --> shape --> task --> Stroke order counts
            Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> data = new Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, int>>>>();
            // user --> shape --> [delta_Iso-Copy, delta_Iso-Synth, delta_Copy-Synth, delta_Iso-Combo]
            Dictionary<int, Dictionary<string, double[]>> deltas = new Dictionary<int, Dictionary<string, double[]>>();
            // user --> shape --> [totals_Iso-Copy, totals_Iso-Synth, totals_Copy-Synth, totals_Iso-Combo]
            Dictionary<int, Dictionary<string, int[]>> totals = new Dictionary<int, Dictionary<string, int[]>>();

            Dictionary<int, Dictionary<string, double[]>> deltasO1 = new Dictionary<int, Dictionary<string, double[]>>();
            Dictionary<int, Dictionary<string, double[]>> deltasO2 = new Dictionary<int, Dictionary<string, double[]>>();
            Dictionary<int, Dictionary<string, double[]>> deltasO3 = new Dictionary<int, Dictionary<string, double[]>>();
            Dictionary<int, Dictionary<string, double[][]>> allPercentages = new Dictionary<int, Dictionary<string, double[][]>>();

            for (int i = 0; i <= 24; i++)
            {
                Dictionary<string, Dictionary<string, Dictionary<string, int>>> shapes = new Dictionary<string, Dictionary<string, Dictionary<string, int>>>();
                Dictionary<string, double[]> shapeDeltas = new Dictionary<string, double[]>();
                Dictionary<string, double[]> shapeDeltas1 = new Dictionary<string, double[]>();
                Dictionary<string, double[]> shapeDeltas2 = new Dictionary<string, double[]>();
                Dictionary<string, double[]> shapeDeltas3 = new Dictionary<string, double[]>();
                Dictionary<string, int[]> shapeCounts = new Dictionary<string, int[]>();
                Dictionary<string, double[][]> perc = new Dictionary<string, double[][]>();

                foreach (string shape in shapenames)
                {
                    shapeDeltas.Add(shape, new double[4]);
                    shapeDeltas1.Add(shape, new double[4]);
                    shapeDeltas2.Add(shape, new double[4]);
                    shapeDeltas3.Add(shape, new double[4]);
                    shapeCounts.Add(shape, new int[4]);
                    perc.Add(shape, new double[3][]);

                    Dictionary<string, Dictionary<string, int>> tasks = new Dictionary<string, Dictionary<string, int>>();
                    foreach (string task in tasknames)
                        tasks.Add(task, new Dictionary<string, int>());

                    shapes.Add(shape, tasks);
                }

                data.Add(i, shapes);
                deltas.Add(i, shapeDeltas);
                totals.Add(i, shapeCounts);
                deltasO1.Add(i, shapeDeltas1);
                deltasO2.Add(i, shapeDeltas2);
                deltasO3.Add(i, shapeDeltas3);
                allPercentages.Add(i, perc);
            }

            

            // Read input file and fill data
            StreamReader reader = new StreamReader(inFile);
            
            List<string> headers = new List<string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("COUNT(*)"))
                    headers = ReadHeader(line);
                else if (line.Trim() == "")
                    continue;
                else
                    ReadResult(line, headers, shapenames, ref data);
            }

            reader.Close();


            foreach (int user in data.Keys)
            {
                Dictionary<string, Dictionary<string, Dictionary<string, int>>> perUserData = data[user];

                foreach (string shape in perUserData.Keys)
                {
                    Dictionary<string, Dictionary<string, int>> perShapeData = perUserData[shape];
                    Dictionary<string, int> ensemble = Sort(perShapeData["Ensemble"]);
                    Dictionary<string, int> isolated = Sort(perShapeData["Isolated"]);
                    Dictionary<string, int> copy = Sort(perShapeData["Copy"]);
                    Dictionary<string, int> synthesize = Sort(perShapeData["Synthesize"]);
                    Dictionary<string, int> csComb = Sort(perShapeData["CScomb"]);

                    // n = num top orders to look at
                    int n = ensemble.Count;
                    List<string> topOrders = new List<string>(ensemble.Keys);
                    int[][] counts = new int[n][];
                    for (int i = 0; i < n; i++)
                    {
                        counts[i] = new int[4];
                        if (isolated.ContainsKey(topOrders[i]))
                            counts[i][0] = isolated[topOrders[i]];
                        else
                            counts[i][0] = 0;

                        if (copy.ContainsKey(topOrders[i]))
                            counts[i][1] = copy[topOrders[i]];
                        else
                            counts[i][1] = 0;

                        if (synthesize.ContainsKey(topOrders[i]))
                            counts[i][2] = synthesize[topOrders[i]];
                        else
                            counts[i][2] = 0;

                        if (csComb.ContainsKey(topOrders[i]))
                            counts[i][3] = csComb[topOrders[i]];
                        else
                            counts[i][3] = 0;
                    }

                    totals[user][shape] = GetTotals(counts);

                    double[][] percentages = GetPercentages(counts);

                    for (int i = 0; i < 3; i++)
                    {
                        if (percentages.Length > i)
                            allPercentages[user][shape][i] = percentages[i];
                        else
                            allPercentages[user][shape][i] = new double[] { 0.0, 0.0, 0.0, 0.0 };
                    }

                    

                    deltas[user][shape] = GetShapeDeltas(percentages);
                    deltasO1[user][shape] = GetShapeDeltas(percentages, 0);
                    deltasO2[user][shape] = GetShapeDeltas(percentages, 1);
                    deltasO3[user][shape] = GetShapeDeltas(percentages, 2);

                    PrintPercentages(percentages, deltas[user][shape], shape, user, writer2);
                }
            }

            #region Compute Aggregates
            Dictionary<int, Dictionary<string, double[]>> aggAvg = new Dictionary<int, Dictionary<string, double[]>>();
            Dictionary<int, Dictionary<string, double[]>> aggMedian = new Dictionary<int, Dictionary<string, double[]>>();
            Dictionary<int, Dictionary<string, double[]>> aggStdDev = new Dictionary<int, Dictionary<string, double[]>>();
            
            int minTotal = 5;

            for (int i = 0; i < 4; i++)
            {
                aggAvg.Add(i, new Dictionary<string, double[]>());
                aggMedian.Add(i, new Dictionary<string, double[]>());
                aggStdDev.Add(i, new Dictionary<string, double[]>());

                foreach (string shape in shapenames)
                {
                    List<double> listIC = new List<double>();
                    List<double> listIS = new List<double>();
                    List<double> listCS = new List<double>();
                    List<double> listComb = new List<double>();

                    foreach (int user in deltas.Keys)
                    {
                        if (user == 0) continue;

                        Dictionary<int, Dictionary<string, double[]>> d = null;
                        if (i == 0)
                            d = deltas;
                        else if (i == 1)
                            d = deltasO1;
                        else if (i == 2)
                            d = deltasO2;
                        else if (i == 3)
                            d = deltasO3;

                        double current0 = d[user][shape][0];
                        if (!double.IsNaN(current0) && totals[user][shape][0] > minTotal)
                            listIC.Add(current0);
                        double current1 = d[user][shape][1];
                        if (!double.IsNaN(current1) && totals[user][shape][0] > minTotal)
                            listIS.Add(current1);
                        double current2 = d[user][shape][2];
                        if (!double.IsNaN(current2) && totals[user][shape][0] > minTotal)
                            listCS.Add(current2);
                        double current3 = d[user][shape][3];
                        if (!double.IsNaN(current3) && totals[user][shape][0] > 2 * minTotal)
                            listComb.Add(current3);
                    }

                    double[] avgs = new double[] { 
                    Compute.Mean(listIC.ToArray()), 
                    Compute.Mean(listIS.ToArray()), 
                    Compute.Mean(listCS.ToArray()),
                    Compute.Mean(listComb.ToArray()) };
                    double[] medians = new double[] { 
                    Compute.Median(listIC.ToArray()), 
                    Compute.Median(listIS.ToArray()), 
                    Compute.Median(listCS.ToArray()),
                    Compute.Median(listComb.ToArray()) };
                    double[] stdDevs = new double[] { 
                    Compute.StandardDeviation(listIC.ToArray()), 
                    Compute.StandardDeviation(listIS.ToArray()), 
                    Compute.StandardDeviation(listCS.ToArray()),
                    Compute.StandardDeviation(listComb.ToArray()) };

                    aggAvg[i].Add(shape, avgs);
                    aggMedian[i].Add(shape, medians);
                    aggStdDev[i].Add(shape, stdDevs);
                }
            }
            #endregion

            #region Write output
            StreamWriter writer = new StreamWriter(outFile);

            for (int i = 0; i < 4; i++)
            {
                writer.WriteLine("Aggregates\tIso-Copy\t\t\tIso-Synth\t\t\tCopy-Synth\t\t\tIso-Combo");
                writer.WriteLine("Shape\tMedian\tAverage\tStdDev\tMedian\tAverage\tStdDev\tMedian\tAverage\tStdDev\tMedian\tAverage\tStdDev");
                foreach (string shape in shapenames)
                {
                    writer.Write(shape + "\t");
                    writer.Write(aggMedian[i][shape][0] + "\t");
                    writer.Write(aggAvg[i][shape][0] + "\t");
                    writer.Write(aggStdDev[i][shape][0] + "\t");
                    writer.Write(aggMedian[i][shape][1] + "\t");
                    writer.Write(aggAvg[i][shape][1] + "\t");
                    writer.Write(aggStdDev[i][shape][1] + "\t");
                    writer.Write(aggMedian[i][shape][2] + "\t");
                    writer.Write(aggAvg[i][shape][2] + "\t");
                    writer.Write(aggStdDev[i][shape][2] + "\t");
                    writer.Write(aggMedian[i][shape][3] + "\t");
                    writer.Write(aggAvg[i][shape][3] + "\t");
                    writer.Write(aggStdDev[i][shape][3] + "\t");
                    writer.WriteLine();
                }

                writer.WriteLine();
                writer.WriteLine("Shape\tUser\tIso-Copy\tIso-Synth\tCopy-Synth\tIso-Combo");
                foreach (string shape in shapenames)
                {
                    foreach (int user in deltas.Keys)
                    {
                        Dictionary<int, Dictionary<string, double[]>> d = null;
                        if (i == 0)
                            d = deltas;
                        else if (i == 1)
                            d = deltasO1;
                        else if (i == 2)
                            d = deltasO2;
                        else if (i == 3)
                            d = deltasO3;

                        writer.Write(shape + "\t");
                        writer.Write(user + "\t");
                        writer.Write(d[user][shape][0] + "\t");
                        writer.Write(d[user][shape][1] + "\t");
                        writer.Write(d[user][shape][2] + "\t");
                        writer.Write(d[user][shape][3] + "\t");

                        if (i == 0)
                        {
                            writer.Write("\t");
                            writer.Write(totals[user][shape][0] + "\t");
                            writer.Write(totals[user][shape][1] + "\t");
                            writer.Write(totals[user][shape][2] + "\t");
                            writer.Write(totals[user][shape][3] + "\t");
                        }
                        writer.WriteLine();
                    }
                }

                writer.WriteLine(); writer.WriteLine(); writer.WriteLine(); writer.WriteLine(); writer.WriteLine();
            }

            foreach (string shape in shapenames)
            {
                writer.WriteLine(shape + " gates\tIsolated\t\t\tCopy\t\t\tSynthesize\t\t\tCombo");
                writer.WriteLine("User #\tOrder1\tOrder2\tOrder3\tOrder1\tOrder2\tOrder3\tOrder1\tOrder2\tOrder3\tOrder1\tOrder2\tOrder3");
                for (int i = 1; i <= 24; i++)
                {
                    writer.Write("User " + i.ToString() + "\t");
                    for (int j = 0; j < 4; j++)
                        for (int k = 0; k < 3; k++)
                            writer.Write(allPercentages[i][shape][k][j].ToString() + "\t");

                    writer.WriteLine();
                }
            }

            writer.Close();
            writer2.Close();
            #endregion
        }

        private static double[] GetShapeDeltas(double[][] percentages, int p)
        {
            double[] deltas = new double[4];

            if (percentages.Length > p)
            {
                double[] row = percentages[p];
                // Iso-Copy delta
                deltas[0] = Math.Abs(row[0] - row[1]);
                // Iso-Synth delta
                deltas[1] = Math.Abs(row[0] - row[2]);
                // Copy-Synth delta
                deltas[2] = Math.Abs(row[1] - row[2]);
                // Iso-Combo (Combo = Copy + Synth) delta
                deltas[3] = Math.Abs(row[0] - row[3]);

                for (int i = 0; i < deltas.Length; i++)
                    deltas[i] /= 2;
            }
            else
                for (int i = 0; i < deltas.Length; i++)
                    deltas[i] = 0.0;
            

            return deltas;
        }

        private static int[] GetTotals(int[][] counts)
        {
            int[] all = new int[4];

            for (int j = 0; j < 4; j++)
            {
                int total = 0;
                for (int i = 0; i < counts.Length; i++)
                    total += counts[i][j];

                all[j] = total;
            }

            return all;
        }

        private static double[] GetShapeDeltas(double[][] percentages)
        {
            double[] deltas = new double[4];

            foreach (double[] row in percentages)
            {
                // Iso-Copy delta
                deltas[0] += Math.Abs(row[0] - row[1]);
                // Iso-Synth delta
                deltas[1] += Math.Abs(row[0] - row[2]);
                // Copy-Synth delta
                deltas[2] += Math.Abs(row[1] - row[2]);
                // Iso-Combo (Combo = Copy + Synth) delta
                deltas[3] += Math.Abs(row[0] - row[3]);
            }
            for (int i = 0; i < deltas.Length; i++)
                deltas[i] /= 2;

            return deltas;
        }

        private static void PrintPercentages(double[][] percentages, double[] deltas, string shape, int user, StreamWriter writer2)
        {
            writer2.WriteLine("User: " + user);
            writer2.WriteLine("Shape: " + shape);
            writer2.WriteLine("Order\tIso\tCopy\tSynth\tCombo");            

            for (int i = 0; i < percentages.Length; i++)
            {
                writer2.Write("O_" + i.ToString() + "\t");
                writer2.Write(percentages[i][0].ToString("#0.00") + "\t");
                writer2.Write(percentages[i][1].ToString("#0.00") + "\t");
                writer2.Write(percentages[i][2].ToString("#0.00") + "\t");
                writer2.Write(percentages[i][3].ToString("#0.00") + "\t");
                writer2.WriteLine();
            }

            writer2.WriteLine("  Deltas:");
            writer2.WriteLine("    Iso-Copy =\t" + deltas[0].ToString("#0.00"));
            writer2.WriteLine("    Iso-Synth =\t" + deltas[1].ToString("#0.00"));
            writer2.WriteLine("    Copy-Synth =\t" + deltas[2].ToString("#0.00"));
            writer2.WriteLine("    Iso-Combo =\t" + deltas[3].ToString("#0.00"));
        }

        private static double[][] GetPercentages(int[][] counts)
        {
            double[][] p = new double[counts.Length][];
            for (int i = 0; i < p.Length; i++)
                p[i] = new double[4];

            for (int j = 0; j < 4; j++)
            {
                int total = 0;
                for (int i = 0; i < counts.Length; i++)
                    total += counts[i][j];

                //if (total == 0) total++;

                for (int i = 0; i < counts.Length; i++)
                    p[i][j] = (double)counts[i][j] / total;
            }

            return p;
        }

        private static Dictionary<string, int> Sort(Dictionary<string, int> dictionary)
        {
            Dictionary<string, int> sorted = new Dictionary<string, int>(dictionary.Count);
            List<int> counts = new List<int>(dictionary.Values);
            counts.Sort();
            counts.Reverse();
            List<int> nums = new List<int>(dictionary.Values);
            List<string> orders = new List<string>(dictionary.Keys);

            foreach (int count in counts)
            {
                int index = nums.IndexOf(count);
                string order = orders[index];
                sorted.Add(order, count);
                nums.RemoveAt(index);
                orders.RemoveAt(index);
            }

            return sorted;
        }

        private static List<string> ReadHeader(string line)
        {
            return new List<string>(line.Split("\t".ToCharArray()));
        }

        private static void ReadResult(string line, List<string> headers, List<string> validShapes,
            ref Dictionary<int, Dictionary<string, Dictionary<string, Dictionary<string, int>>>> data)
        {
            string[] splits = line.Split("\t".ToCharArray());

            int countIndex = headers.IndexOf("COUNT(*)");
            int count;
            bool goodCount = int.TryParse(splits[countIndex], out count);
            if (!goodCount) throw new Exception("Unable to parse count: " + splits[countIndex]);

            int shapenameIndex = headers.IndexOf("shapename");
            string shapename = splits[shapenameIndex];

            int strokeorderIndex = headers.IndexOf("strokeorder");
            string strokeorder = splits[strokeorderIndex];

            int usernameIndex = headers.IndexOf("username");
            int username;
            if (usernameIndex >= 0)
            {
                bool goodUsername = int.TryParse(splits[usernameIndex], out username);
                if (!goodUsername) throw new Exception("Unable to parse username: " + splits[usernameIndex]);
            }
            else
                username = 0;

            string task = "Ensemble";
            int taskIndex = headers.IndexOf("task");
            if (taskIndex >= 0)
                task = splits[taskIndex];

            if (validShapes.Contains(shapename))
            {
                data[username][shapename][task].Add(strokeorder, count);
                if (task == "Copy" || task == "Synthesize")
                {
                    if (!data[username][shapename]["CScomb"].ContainsKey(strokeorder))
                        data[username][shapename]["CScomb"].Add(strokeorder, 0);

                    data[username][shapename]["CScomb"][strokeorder] += count;
                }
            }
        }

        private static void ReadStrokeOrders(string line, ref Dictionary<string, Dictionary<string, int>> numOrders)
        {
            string[] splits = line.Split("\t".ToCharArray());

            int count;
            bool goodCount = int.TryParse(splits[0], out count);
            if (!goodCount) return;

            string shapeName = splits[1];

            int orderIndex = 2;
            string task = "Ensemble";
            if (splits[2] == "Isolated" || splits[2] == "Copy" || splits[2] == "Synthesize")
            {
                task = splits[2];
                orderIndex = 3;
            }


            string order = splits[orderIndex];

            if (!numOrders.ContainsKey(task))
                numOrders.Add(task, new Dictionary<string, int>());


            if (numOrders[task].ContainsKey(order))
                numOrders[task][order] = count;
            else
                numOrders[task].Add(order, count);
        }

        private static void ReadNumTouchUps(string line, ref Dictionary<string, Dictionary<int, int>> numTouchups)
        {
            string[] splits = line.Split("\t".ToCharArray());

            int count;
            bool goodCount = int.TryParse(splits[0], out count);
            if (!goodCount) return;

            string shapeName = splits[1];

            string task = "Ensemble";
            int num;
            bool goodNum;
            bool ensemble = int.TryParse(splits[2], out num);
            if (!ensemble)
            {
                task = splits[2];
                goodNum = int.TryParse(splits[3], out num);
            }
            else
                goodNum = true;

            if (!numTouchups.ContainsKey(task))
                numTouchups.Add(task, new Dictionary<int, int>());


            if (numTouchups[task].ContainsKey(num))
                numTouchups[task][num] = count;
            else
                numTouchups[task].Add(num, count);
        }

        private static void ReadNumStrokes(string line, ref Dictionary<string, Dictionary<int, int>> numStrokes)
        {
            string[] splits = line.Split("\t".ToCharArray());

            int count;
            bool goodCount = int.TryParse(splits[0], out count);
            if (!goodCount) return;

            string shapeName = splits[1];

            string task = "Ensemble";
            int num;
            bool goodNum;
            bool ensemble = int.TryParse(splits[2], out num);
            if (!ensemble)
            {
                task = splits[2];
                goodNum = int.TryParse(splits[3], out num);
            }
            else
                goodNum = true;

            if (!numStrokes.ContainsKey(task))
                numStrokes.Add(task, new Dictionary<int, int>());


            if (numStrokes[task].ContainsKey(num))
                numStrokes[task][num] = count;
            else
                numStrokes[task].Add(num, count);
        }

        private static void ReadTotalInstances(string line, ref Dictionary<string, int> numTotal, ref string shapeName)
        {
            string[] splits = line.Split("\t".ToCharArray());
            
            int count;
            bool goodCount = int.TryParse(splits[0], out count);
            if (!goodCount) return;

            shapeName = splits[1];

            string task = splits[2];
            if (numTotal.ContainsKey(task))
                numTotal[task] = count;
            else
                numTotal.Add(task, count);
        }

        private static Dictionary<string, Dictionary<int, int>> GetNumTouchUpsDictionary()
        {
            Dictionary<string, Dictionary<int, int>> dic = new Dictionary<string, Dictionary<int, int>>();

            string[] tasks = new string[] { "Ensemble", "Isolated", "Copy", "Synthesize" };

            foreach (string task in tasks)
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                for (int i = 0; i < 5; i++)
                    counts.Add(i, 0);

                dic.Add(task, counts);
            }

            return dic;
        }

        private static Dictionary<string, Dictionary<int, int>> GetNumStrokesDictionary()
        {
            Dictionary<string, Dictionary<int, int>> dic = new Dictionary<string, Dictionary<int, int>>();

            string[] tasks = new string[] { "Ensemble", "Isolated", "Copy", "Synthesize" };

            foreach (string task in tasks)
            {
                Dictionary<int, int> counts = new Dictionary<int, int>();
                for (int i = 0; i < 10; i++)
                    counts.Add(i, 0);

                dic.Add(task, counts);
            }

            return dic;
        }

        private static ReadState GetState(string line)
        {
            if (line == "COUNT(*)	shapename	task")
                return ReadState.TotalInstances;
            else if (line == "COUNT(*)	shapename	numstrokes" ||
                line == "COUNT(*)	shapename	task	numstrokes")
                return ReadState.NumStrokeCounts;
            else if (line == "COUNT(*)	shapename	numtouchups" ||
                line == "COUNT(*)	shapename	task	numtouchups")
                return ReadState.NumTouchupCounts;
            else if (line == "COUNT(*)	shapename	strokeorder" ||
                line == "COUNT(*)	shapename	task	strokeorder")
                return ReadState.StrokeOrderCounts;

            throw new Exception("Invalid Header");
        }
    }
}
