using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RecognitionManager;
using RecognitionInterfaces;
using Refiner;
using Domain;

namespace TestRig
{
    class PipelineStage : ProcessStage
    {

        private List<RecognitionPipeline> _pipelines;
        private Dictionary<RecognitionPipeline, Utils.Table> _tables;
        private Dictionary<RecognitionPipeline, Utils.ConfusionMatrix<string>> _classificationConfusion;
        private Dictionary<RecognitionPipeline, Utils.ConfusionMatrix<ShapeType>> _recognitionConfusion;
        private Dictionary<RecognitionPipeline, double> _cls;
        private Dictionary<RecognitionPipeline, double> _grp;
        private Dictionary<RecognitionPipeline, double> _rec;
        private int _numTests;

        public PipelineStage()
        {
            name = "Pipeline";
            shortname = "pip";
            outputFiletype = ".csv"; // comma-separated values; readable by Excel

            _numTests = 0;
            _pipelines = new List<RecognitionPipeline>();
            _tables = new Dictionary<RecognitionPipeline, Utils.Table>();
            _classificationConfusion = new Dictionary<RecognitionPipeline, Utils.ConfusionMatrix<string>>();
            _recognitionConfusion = new Dictionary<RecognitionPipeline, Utils.ConfusionMatrix<ShapeType>>();
            _cls = new Dictionary<RecognitionPipeline, double>();
            _grp = new Dictionary<RecognitionPipeline, double>();
            _rec = new Dictionary<RecognitionPipeline, double>();
        }

        private static IRecognitionStep getStep(string name)
        {
            Console.WriteLine("Loading stage: " + name);
            switch (name)
            {
                case "cls": return RecognitionPipeline.createDefaultClassifier();
                case "grp": return RecognitionPipeline.createDefaultGrouper();
                case "rec": return RecognitionPipeline.createDefaultRecognizer();
                case "con": return RecognitionPipeline.createDefaultConnector();
                case "ref": return RecognitionPipeline.createDefaultRefiner(RecognitionPipeline.createDefaultConnector(), RecognitionPipeline.createDefaultRecognizer());
                case "ref_ctx": return new ContextRefiner(ContextDomain.CircuitDomain.GetInstance(), RecognitionPipeline.createDefaultRecognizer());
                case "ref_cctx": return new CarefulContextRefiner(ContextDomain.CircuitDomain.GetInstance(), RecognitionPipeline.createDefaultRecognizer());
                case "ref_search": return new SearchRefiner(ContextDomain.CircuitDomain.GetInstance());
            }
            return null;
        }

        private void prepForPipeline(RecognitionPipeline pipeline)
        {
            _pipelines.Add(pipeline);
            Utils.Table table = new Utils.Table(new string[] { "File", "Classification Quality", "Grouping Quality", "Recognition Quality" });
            _tables.Add(pipeline, table);
            _cls.Add(pipeline, 0);
            _grp.Add(pipeline, 0);
            _rec.Add(pipeline, 0);
        }

        public override void processArgs(string[] args)
        {
            RecognitionPipeline pipeline = new RecognitionPipeline();

            foreach (string arg in args)
            {
                if (arg == "|")
                {
                    prepForPipeline(pipeline);
                    pipeline = new RecognitionPipeline();
                    continue;
                }

                IRecognitionStep step = getStep(arg);
                if (step != null)
                    pipeline.addStep(step);
                else
                    Console.WriteLine("WARNING: Unused argment for pipeline stage: " + arg);
            }

            prepForPipeline(pipeline);
        }

        public override void start()
        {
            foreach (RecognitionPipeline pipeline in _pipelines)
            {
                _classificationConfusion.Add(pipeline, new Utils.ConfusionMatrix<string>());
                _recognitionConfusion.Add(pipeline, new Utils.ConfusionMatrix<ShapeType>());
            }
        }

        public override void run(Sketch.Sketch original, string filename)
        {
            _numTests++;

            foreach (RecognitionPipeline pipeline in _pipelines)
            {
                Sketch.Sketch sketch = original.Clone();
                sketch.RemoveLabelsAndGroups();
                pipeline.process(sketch);

                Utils.SketchComparer comparer = new Utils.SketchComparer(original, sketch);

                double cls = comparer.ClassificationQuality * 100;
                double grp = comparer.GroupingQuality * 100;
                double rec = comparer.RecognitionQuality * 100;

                Console.WriteLine("--------- Results ---------");
                Console.WriteLine("    Classification: " + cls + "%");
                Console.WriteLine("    Grouping:       " + grp + "%");
                Console.WriteLine("    Recognition:    " + rec + "%");

                _tables[pipeline].addResult(new string[] { 
                    filename, 
                    "" + cls, 
                    "" + grp, 
                    "" + rec 
                });

                _classificationConfusion[pipeline].AddResults(comparer.ClassificationConfusion);
                _recognitionConfusion[pipeline].AddResults(comparer.RecognitionConfusion);

                _cls[pipeline] += cls;
                _grp[pipeline] += grp;
                _rec[pipeline] += rec;
            }
        }

        public override void writeToFile(TextWriter handle, string path)
        {
            Utils.Table overall = new Utils.Table(new string[] { "Pipeline", "Classification Quality", "Grouping Quality", "Recognition Quality" });
            handle.WriteLine("Overall results");
            int pipelineID = 0;
            foreach (RecognitionPipeline pipeline in _pipelines)
            {
                pipelineID++;
                overall.addResult(new string[] {
                    "" + pipelineID,
                    "" + _cls[pipeline] / _numTests,
                    "" + _grp[pipeline] / _numTests,
                    "" + _rec[pipeline] / _numTests
                });
            }
            overall.writeCSV(handle);
            handle.WriteLine();

            pipelineID = 0;
            foreach (RecognitionPipeline pipeline in _pipelines)
            {
                pipelineID++;
                handle.WriteLine("Pipeline " + pipelineID);
                _classificationConfusion[pipeline].writeCSV(handle);
                _recognitionConfusion[pipeline].writeCSV(handle);
                _tables[pipeline].writeCSV(handle);
                handle.WriteLine();
            }
        }

    }
}
