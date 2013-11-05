using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Sketch;

namespace GateStudy
{

    public class GateStudier
    {
        

        public static void Main(string[] args)
        {
            string labeled_dir = "";
            
            // repeated gate drawings
            List<Sketch> ands = readIn(Directory.GetFiles(labeled_dir,"*_AND*"));
            List<Sketch> ors = readIn(Directory.GetFiles(labeled_dir,"*_OR*"));
            List<Sketch> nots = readIn(Directory.GetFiles(labeled_dir,"*_NOT*"));
            List<Sketch> nors = readIn(Directory.GetFiels(labeled_dir,"*_NOR*"));
            List<Sketch> nands = readIn(Directory.GetFiles(labeled_dir,"*_NAND*"));
            List<Sketch> xors = readIn(Directory.GetFiles(labeled_dir,"*_XOR*"));
            List<Shape> repeat_ands = extract(ands,"and");
            List<Shape> repeat_ors = extract(ors,"or");
            List<Shape> repeat_nots = extract(nots,"not");
            List<Shape> repeat_nors = extract(nors,"nor");
            List<Shape> repeat_nands = extract(nands,"nand");
            List<Shape> repeat_xors = extract(xors,"xor");


            // copied circuits
            List<Sketch> copies = readIn(Directory.GetFiles(labeled_dir,"*_COPY*"));
            List<Shape> copies_ands = extract(copies,"and");
            List<Shape> copies_ors = extract(copies,"or");
            List<Shape> copies_nots = extract(copies,"not");
            List<Shape> copies_nors = extract(copies,"nor");
            List<Shape> copies_nands = extract(copies,"nand");
            List<Shape> copies_xors = extract(copies,"xor");


            // synthesized circuits
            List<Sketch> eqs = readIn(Directory.GetFiles(labeled_dir,"*_EQ*"));
            List<Shape> eqs_ands = extract(eqs,"and");
            List<Shape> eqs_ors = extract(eqs,"or");
            List<Shape> eqs_nots = extract(eqs,"not");
            List<Shape> eqs_nors = extract(eqs,"nor");
            List<Shape> eqs_nands = extract(eqs,"nand");
            List<Shape> eqs_xors = extract(eqs,"xor");



        }

        private static List<Sketch> readIn(string[] files)
        {
            List<Sketch> res = new List<Sketch>();
            foreach (string filename in files)
            {
                ReadXML rxml = new ReadXML(filename);
                res.Add(rxml.Sketch);
            }
            return res;
        }

        private static List<Shape> extract(List<Sketch> sketches, string pattern)
        {
            List<Shape> res = new List<Shape>();

            foreach (Sketch s in sketches)
            {
                foreach (Shape ss in s.ShapesL)
                {
                    if (ss.LabelL == pattern) res.Add(ss);
                }
            }

            return res;
        }

    }

}
