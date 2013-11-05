using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SymbolRec.Image;
using Sketch;
using Congeal;
using System.IO;

namespace TestCongealing
{
    /// <summary>
    /// Class for testing the congealing libraries.
    /// </summary>
    class TestCongealing
    {
        //Simple data for Jason's comp
        //static string m_dir = @"C:\Users\sketchers\Documents\sketchRepo\Code\Util\TestCongealing\SimpleTestData";
        //Simple data for my comp
        //static string m_dir = @"C:\Users\sketchers\Documents\Trunk\Code\Util\TestCongealing\SimpleTestDataLarge";
        static string m_bmPattern = @"*.bmp";
        static string m_xmlPattern = @"*.non.xml";
        static string m_pattern = m_bmPattern;
        //All users data from the training set:
        //static string m_dir = @"C:\Users\sketchers\Documents\Trunk\Data\Context Symbols\TRAIN\and";
        // static string m_pattern = @"*.non.xml.bmp";
        
        //Oriented data from the training set:
        static string m_dir = @"C:\Users\sketchers\Documents\Trunk\Data\ContextOriented\TRAIN";

        //Devins data for training:
        static string m_dirDevin = @"C:\Users\sketchers\Documents\Trunk\Data\Symbols";

        //Partial symbols:
        static string m_PartialDir = @"C:\Users\sketchers\Documents\Trunk\Data\Partial Context Symbols\nand";


        //For testing:
        static string m_testDir = @"C:\Users\sketchers\Documents\Trunk\Data\Context Symbols\TEST\";
        static string m_testPattern = m_xmlPattern;
        static string m_outputPath = "testResults.txt";


        static int width =  64;
        static int height = 64;

        static string m_NandDir = @"C:\Users\sketchers\Documents\Trunk\Data\ContextOriented\TRAIN\nand";

        static void Main(string[] args)
        {
            List<Designation> designations;

            List<string> dirs = new List<string>(Directory.GetDirectories(m_dir));
            dirs.RemoveAt(0); //remove the svn directory
            //List<string> nand = new List<string>(Directory.GetDirectories(m_dir));


            //blurTesting(dirs);


            
           

            //testWarps();
            //if (true)
            //    return;
            List<Sketch.Sketch> throwAway; //old testing sketches pulled as the last sketch from each dataset.
            designations = doTraining(dirs, out throwAway);

            //foreach (Designation d in designations)
            //{
            //    Designation.saveTraining(d, d.Name + ".des");
            //}


            //designations = testLoading();

            
            //testMasking();



            #region Classify
            //Should turn this into a method

            //TESTING ON DEVINS 
            List<string> dirsClassify = new List<string>(Directory.GetDirectories(m_dirDevin));
            dirsClassify.RemoveAt(0); //remove svn directory

            
            string results = "";
            Classifier c = new Classifier(designations); 
            //each dir corresponds to one type of gate, so dirsClassify[0] is directory full of AND gates.
            foreach (string dir in dirsClassify)
            {
                List<Sketch.Sketch> testSymbols = Util.getSketches(dir, "*.jnt");     
                string label = dir.Substring(dir.LastIndexOf('\\'));
                results += "\n\n" + c.classify(Util.sketchToBitmap(width, height, testSymbols),label);
                
            }
            c.writeResults(m_outputPath, results);
            #endregion

            Console.WriteLine("DONE...");
            Console.ReadKey();

            # region old testing stuff including lots of warps stuff
            //// SET TO 1 so I only deal with the first image.  FIXME!
            //for(int bmpIdx = 0; bmpIdx < 1; ++bmpIdx)
            //{
            //    Bitmap bm = imgs[bmpIdx];
            //    Congeal.ImageTransform it = new Congeal.ImageTransform(bm);
            //    bool[][] bImg = new bool[bm.Height][];

            //    for(int i = 0; i < bm.Height; ++i)
            //    {
            //        bImg[i] = new bool[bm.Width];
            //        for(int j = 0; j < bm.Width; ++j)
            //        {
            //            if (it.isPixelColored(j,i, bm))
            //            {
            //                bImg[i][j] = true;
            //                Console.Write("1 ");
            //            }
            //            else
            //            {
            //                bImg[i][j] = false;
            //                Console.Write("0 ");
            //            }
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine("Normal Image \n\n");

            //    # region Different Warps
            //    //Congeal.Warp w = new Congeal.Warp(); // Identity warp
            //    //Congeal.Warp identity = new Congeal.Warp();
            //    //Console.WriteLine("Distance between identity and itself is {0}", Congeal.Warp.distance(identity, identity));
                
            //    // Possible problem... shifting can push images off the edge of the array... how to we want
            //    //  to deal with this?  Currently we just drop the points... but we don't actually lose track of them
            //    //  because the points are independently tracked outside of a warp
            //    //Congeal.Warp w = new Congeal.Warp(1, 1, 0, 0, 5, 0); // x-shift... shifts down by 5 rows
            //    //Congeal.Warp w = new Congeal.Warp(1, 1, 0, 0, -5, 0); // x-shift... shifts up by 5 rows
            //    //Congeal.Warp w = new Congeal.Warp(1, 1, 0, 0, 0, 5); // y-shift... shifts right by 5 rows
            //    //Congeal.Warp w = new Congeal.Warp(1, 1, 0, 0, 0, -5); // y-shift...shifts left by 5 rows
            //    //Congeal.Warp w = new Congeal.Warp(1, 1, 0, 0, 5, 5); // x & y shift... down and right by 5 each
            //    //Congeal.Warp rightShift = new Congeal.Warp(1, 1, 0, 0, 0, 1);
            //    //Congeal.Warp downShift = new Congeal.Warp(1, 1, 0, 0, 1, 0);
            //    //Console.WriteLine("Distance between right and down shift is {0}", Congeal.Warp.distance(rightShift, downShift));
            //    //Console.WriteLine("Distance between down and right shift is {0}", Congeal.Warp.distance(downShift, rightShift));
                
            //    //Congeal.Warp w = new Congeal.Warp((float).5, 1, 0, 0, 0, 0); // scale down to half the number of rows
            //    // Possible problem... scaling up introduces a bunch of gaps in our image representation... do we want
            //    //  to be interpolating
            //    //Congeal.Warp w = new Congeal.Warp((float)1.5, 1, 0, 0, 0, 0); // scale to 1.5 the number of rows...
            //    //Congeal.Warp scaleDownRows = new Congeal.Warp((float).9, 1, 0, 0, 0, 0);
            //    //Congeal.Warp scaleDownCols = new Congeal.Warp(1,(float).9, 0, 0, 0, 0);
            //    //Console.WriteLine("Distance between scale down rows and cols is {0}", Congeal.Warp.distance(scaleDownCols,scaleDownRows));
            //    //Console.WriteLine("Distance between scale up and down rows is {0}", Congeal.Warp.distance(scaleDownRows.Inverse, scaleDownRows));

            //    // Scaled down to see if it skews correctly, which it seems to (front of gate [more 'right' part] is farther 'down'
            //    //Congeal.Warp w = new Congeal.Warp((float).5, (float).5, (float).5, 0, 0, 0);  // "y-skew"
            //    //Congeal.Warp w = new Congeal.Warp((float).5, (float).5, (float)-.5, (float).25, 15, 0);                

            //    // See if inverses work
            //    //Congeal.Warp v = new Congeal.Warp(1, 1, 0, 0, 5, 5);  
            //    //Congeal.Warp w = v.Inverse;  // Moves up and left by 5 each instead of down and right, inverse seems to work

            //    // Try a 45 degree rotation
            //    //double rad = (45) * (Math.PI / 180);
            //    //Congeal.Warp w = new Congeal.Warp((float)Math.Cos(rad), (float)Math.Cos(rad),
            //    //                                    (float)-Math.Sin(rad), (float)Math.Sin(rad),0,0);

            //    // Try a -45 degree rotation
            //    //double rad = (-45) * (Math.PI / 180);
            //    //Congeal.Warp rotNeg45 = new Congeal.Warp((float)Math.Cos(rad), (float)Math.Cos(rad),
            //    //                                    (float)-Math.Sin(rad), (float)Math.Sin(rad), 0, 0);

            //    // Determinant seems to always be correct 
            //    // which is (x-scale * y-scale) - (x-skew * y-skew)
            //    // Console.WriteLine("Determinant is {0}", w.Det);  

            //    # endregion

            //    # region Warp composition

            //    // Apply the lines below one at a time to test composition, which appears to work.
            //    //Congeal.Warp w = new Congeal.Warp();
            //    //w.append(downShift);
            //    //w.undoAppend();
            //    //w.append(downShift);
            //    //w.append(rightShift);
            //    //w.undoAppend();
            //    //w.undoAppend();  // Does nothing, as expected

            //    Congeal.Warp w = new Congeal.Warp();
            //    //foreach (Congeal.Warp v in Congeal.UnitWarps.warps)
            //    //{
            //    //    w.append(v);
            //    //}

      

            //    # endregion

            //    bool[][] newImg = w.warpImage(bImg);
            //    for (int row = 0; row < newImg.Length; ++row)
            //    {
            //        for (int col = 0; col < newImg[0].Length; ++col)
            //        {
            //            if (newImg[row][col])
            //            {
            //                Console.Write("1 ");
            //            }
            //            else
            //            {
            //                Console.Write("0 ");
            //            }
            //        }
            //        Console.WriteLine();
            //    }
            //    Console.WriteLine("Warped Image\n\n");
            //    Console.ReadKey();
            //}
            #endregion
        
        }

        /// <summary>
        /// Debugging code to make sure that Designation.LoadDesignation(string) works.
        /// Note that we don't serialize or de-serialize the ImageTransforms, since 
        /// 2d Matrix is not serializable.
        /// </summary>
        /// <returns></returns>
        private static List<Designation> testLoading()
        {
            List<Designation> designations = new List<Designation>();

            List<string> files = Util.getFiles(Directory.GetCurrentDirectory(), "*.des");
            foreach (string filename in files)
            {
                Designation d = Designation.LoadDesignation(filename);
                designations.Add(d);
                Console.WriteLine("\n\n Deserializing");
                Console.WriteLine(d.Name);
                d.testLoad();   
            }
            return designations;


        }

        /// <summary>
        /// Debug code to make sure the Gaussian blur classes work as expected.
        /// </summary>
        /// <param name="dirs"></param>
        private static void blurTesting(List<string> dirs)
        {
            Console.WriteLine("Start blur testing");
            string dir = dirs[1];
            List<Sketch.Sketch> sketches = Congeal.Util.getSketches(dir, m_xmlPattern);
            List<Bitmap> bms = Util.sketchToBitmap(64, 64, sketches);
            for (int i = 0; i < bms.Count; i++)
            {
                Console.Write(".");
                bms[i].Save(String.Format("before{0}.bmp",i));
                
                Adrian.PhotoX.Lib.GaussianBlur gb = new Adrian.PhotoX.Lib.GaussianBlur(9);
                Bitmap after = gb.ProcessImage(bms[i]);
                after.Save(String.Format("after{0}.bmp", i));
            }
            Console.WriteLine("Done blue");
        }

        /// <summary>
        /// Debug code to make sure our unit warps work as expected.
        /// </summary>
        private static void testWarps()
        {
            Console.WriteLine("Start testWarps()");
             List<Bitmap> imgs = Congeal.Util.getBitmaps(m_dirDevin, m_bmPattern);
             Bitmap bm = imgs[0];

             ImageTransform it = new ImageTransform(bm);
            //AvgImage ai = new AvgImage(
     

            string output =  @"C:\Users\sketchers\Documents\Trunk\Code\Util\TestCongealing\TestCongealing\bin\Debug\WarpTests\";
            it.writeImage(output + "original.bmp");
           // UnitWarps warps = new UnitWarps(bm.Width, bm.Height);
            UnitWarps warps = new UnitWarps(bm.Width, bm.Height, 16, (float)1.25, (float).8, 30);

            ImageTransform test = new ImageTransform(bm);

            //it.append(warps.warps[2]);
            //it.append(warps.warps[4]);
            //it.writeImage(output + "scaleShear.bmp");


            //test.append(warps.warps[4]);
            //test.append(warps.warps[2]);
            //test.writeImage(output + "shearScale.bmp");

            for (int i = 0; i < warps.warps.Length; i++)
            {
                it.append(warps.warps[i]);
                it.writeImage(output + i + "warp.bmp");
                it.undoAppend();

                it.append(warps.warps[i].Inverse);
                it.writeImage(output + i + "InverseWarp.bmp");
                it.undoAppend();

            }

            Console.WriteLine("End testWarps()");
 
        }

        /// <summary>
        /// Code to test the idea of masking. I hoped to "XOR" an AND gate with an OR gate so that
        /// the areas where they overlapped would drop out, and we would only be left with the pixels
        /// where the two gates were different. This would then give us a mask to help emphasize the
        /// differences between gates.
        /// </summary>
        private static void testMasking()
        {
               string bm1 = @"C:\Users\sketchers\Documents\Trunk\Code\Util\TestCongealing\TestCongealing\bin\Debug\output\nor3.bmp";
               string bm2 = @"C:\Users\sketchers\Documents\Trunk\Code\Util\TestCongealing\TestCongealing\bin\Debug\output\nand3.bmp";
               Util.applyMask(bm1, bm2);
        }

        /// <summary>
        /// Does all the setup, creates a Designation class from a list of gates, and calls Designation.train();
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="testSymbols"></param>
        /// <returns></returns>
        private static List<Designation> doTraining(List<string> dirs, out List<Sketch.Sketch> testSymbols)
        {
            string symbolType;

            List<Designation> designations = new List<Designation>();
            testSymbols = new List<Sketch.Sketch>(dirs.Count);
            
            //For partial gate recog
           // List<Sketch.Sketch> nandGates = Congeal.Util.getSketches(m_NandDir, m_xmlPattern);
           
            foreach (string dir in dirs)
            {
                List<Sketch.Sketch> sketches = Congeal.Util.getSketches(dir, m_xmlPattern);

                

                //The 3rd sketch gets pulled out later as a testing sketch.
                List<Sketch.Sketch> two = new List<Sketch.Sketch>();
                for (int i = 0; i < 7; i++)
                {                   
                    two.Add(sketches[i]);
                    
                    
                }
                //sketches = two;
                //sketches.AddRange(nandGates);




                // List<Sketch.Sketch> sketches = Congeal.Util.getSketches(dir, "*.jnt");
                //List<Bitmap> bitmaps = Util.getBitmaps(dir, m_bmPattern);

                //Get the name of the folder containing that set of training data
                //Which will usually serve as a decent name for the designation.


                symbolType = dir.Substring(dir.LastIndexOf('\\') + 1);              
                //DEBUG
                // write input bitmaps to file
                List<Bitmap> bitmaps = Util.sketchToBitmap(64, 64, sketches);
                for (int i = 0; i < bitmaps.Count; i++)
                {
                    Bitmap bm = new Bitmap(bitmaps[i],128, 128);
                    ImageTransform it = new ImageTransform(bm);
                    it.writeImage(String.Format("inputs\\input_{0}{1}.bmp", symbolType, i));
                    
                }
                
                if (sketches != null && sketches.Count > 0)
                {
                    //Pull out the last sketch for testing
                    Sketch.Sketch s = sketches[sketches.Count - 1];
                    testSymbols.Add(s);
                    sketches.RemoveAt(sketches.Count - 1);

                    Designation d = new Congeal.Designation(width, height, sketches, symbolType);

                    System.Console.WriteLine("training: " + symbolType);
                    d.train();
                    designations.Add(d);
                }



            }
            return designations;
        }

    }
}
