using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Runtime.Remoting.Messaging;

using Sketch;

namespace SketchPanelLib
{
    #region Framework for Recognizer Interface

    /// <summary>
    /// Subscribers to recognition results (e.g., the panels in the GUI) handle
    /// result events through this delegate.  The Recognizer will push events to 
    /// subscribers whenever recognition results are ready.  This delegate allows a
    /// recognizer thread to "push" the front end with recognition results.
    /// <seealso cref="UserStudyUI.RecognitionResultsEventHandler"/>
    /// </summary>
    /// <param name="source">The recognizer that sent this event</param>
    /// <param name="rr">The recognition result</param>
    public delegate void RecognitionResultsEventHandler(SketchRecognizer source, RecognitionResult result);


    /// <summary>
    /// Subscribers to 
    /// triggers (e.g., the circuit recognizer) handle
    /// trigger events through this delegate.  The Front End event publisher (e.g. a 
    /// SketchPanel <see cref="SketchPanel"/>) will push events to subscribers whenever 
    /// the user triggers recognition.  This delegate allows the front end thread to 
    /// "push" events to the recognizer thread.
    /// </summary>
    /// <param name="source">The panel that sent this event</param>
    /// <param name="ra">The recognizer arguments</param>
    public delegate void RecognitionTriggerEventHandler(SketchPanel source, RecognitionArgs ra);


    /// <summary>
    /// Basic recognition results class.
    /// </summary>
    public class RecognitionResult
    {
        /// <summary>
        /// The recognized (labeled) sketch
        /// </summary>
        public Sketch.Sketch Sketch;

        /// <summary>
        /// True iff the user triggered recognition.
        /// </summary>
        public bool UserTriggered = false;

        /// <summary>
        /// Any status message, useful for setting the
        /// status bar after results have been returned
        /// </summary>
        public string status;
    }

    /// <summary>
    /// Basic recognition arguments class.  
    /// </summary>
    public class RecognitionArgs
    {
        /// <summary>
        /// The Sketch to be recognized.
        /// </summary>
        public Sketch.Sketch Sketch;

        /// <summary>
        /// True iff the user triggered recognition.
        /// </summary>
        public bool UserTriggered;
    }

    /// <summary>
    /// A SketchRecognizer performs recognition on sketches.  The
    /// Recognizer receives sketch data from the front end,
    /// Reocgnizes that sketch data, and then returns
    /// labeled results (and possibly other data) to the 
    /// front end.  This framework supports both Recognizers that run
    /// only when recognition is triggered and Recognizers that constantly
    /// run in the background (e.g., after every stroke is drawn).  
    /// 
    /// This recogntion framework starts recognition in a separate thread.
    /// Recognizers publish recognition results to SketchPanels (send results to 
    /// the panel's parent thread).
    /// 
    /// Asyncrhonous Event-passing Model: 
    /// SkechPanel has event of type
    ///    RecognitionTriggerEventHandler
    /// and publishes to this event whenever recognition is triggered.
    /// 
    /// SketchRecognizer has event of type
    ///    RecognitionResultsEventHandler
    /// and publishes to this event whenever recognition results are available.
    /// 
    /// Because the GUI (and Windows Forms in general) must run in one thread,
    /// this class serves to give the Recognizer its own thread and join 
    /// Recognition Results with the GUI's thread.  Subclassers must only 
    /// implement the Recognize() method in order to utilize this functionality.
    /// </summary>
    public abstract class SketchRecognizer
    {
        /// <summary>
        /// Internal delegate for starting recognition thread
        /// </summary>
        /// <param name="args">Arguments to recognizer</param>
        /// <returns>Results from recognizer</returns>
        protected delegate RecognitionResult FireRecognition(RecognitionArgs args);

        /// <summary>
        /// FIXME
        /// List of panels subscribed to this recognizer
        /// </summary>
        protected SketchPanel parentPanel;

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SketchRecognizer()
        {
            
        }

        /// <summary>
        /// Constructor.  Subscribes to the given
        /// SketchPanel. 
        /// </summary>
        /// <param name="panel">The SketchPanel to which we want to subscribe.</param>
        public SketchRecognizer(SketchPanel panel)
        {
            SubscribeToPanel(panel);
        }

        /// <summary>
        /// Subscribes this recognizer to the given panel's trigger events.
        /// </summary>
        /// <param name="panel">Given panel to which we want to subscribe</param>
        public void SubscribeToPanel(SketchPanel panel)
        {
            panel.RecognitionTriggered += new RecognitionTriggerEventHandler(panel_RecognitionTriggered);
            parentPanel = panel;
        }

        /// <summary>
        /// Unsubscribes this recognizer from the current panel
        /// </summary>
        public void UnsubscribeFromPanel()
        {
            parentPanel.RecognitionTriggered -= new RecognitionTriggerEventHandler(panel_RecognitionTriggered);
        }

        /// <summary>
        /// Handles recogntion trigger events.  Forwards recognition arguments to the real recognizer.
        /// </summary>
        /// <param name="source">The source panel of this trigger event</param>
        /// <param name="ra">The recognition arguments to forward (e.g., the sketch to be recognized)</param>
        private void panel_RecognitionTriggered(SketchPanel source, RecognitionArgs ra)
        {
            FireRecognition frDelegate = new FireRecognition(this.Recognize);
            frDelegate.BeginInvoke(ra, this.processRecognitionResult, null);
        }

        /// <summary>
        /// Asynchronously handles recognition results and forwards them to subscribers (e.g. SketchPanels)
        /// </summary>
        /// <param name="receipt">a receipt thrown once the result is available; the 
        /// recognition result can be retrieved from this receipt</param>
        private void processRecognitionResult(IAsyncResult receipt)
        {
            // read result
            AsyncResult asResult = (AsyncResult)receipt;
            FireRecognition frDelegate = (FireRecognition)asResult.AsyncDelegate;
            RecognitionResult recogResult = frDelegate.EndInvoke(receipt);
            
            // publish result to panel
            RecognitionResultsEventHandler rrDelegate = new RecognitionResultsEventHandler(parentPanel.SetRecognitionResult);
            parentPanel.Invoke(rrDelegate, new object[] { this, recogResult });
        }

        /// <summary>
        /// Performs recognition on a given set of recognition arguments (e.g. a sketch).  
        /// Subclassers of this class must implement this method; this is where actual
        /// recognition code should go.  This method will run in its own thread;
        /// a new thread is started every time recognition is triggered.
        /// </summary>
        /// <param name="args">Arguments to the recognizer</param>
        /// <returns>The result of recognition</returns>
        public abstract RecognitionResult Recognize(RecognitionArgs args);
    }

    #endregion

    #region Sample Recognizer

    /// <summary>
    /// Dummy recognizer for testing.  Waits 3 seconds and then sends
    /// a blank result.
    /// </summary>
    public class TimeoutRecognizer : SketchRecognizer
    {
        /// <summary>
        /// Waits 3 seconds and then sends a blank recognition result.
        /// </summary>
        /// <param name="args">The Recognition arguments</param>
        /// <returns>A blank RecognitionResult</returns>
        public override RecognitionResult Recognize(RecognitionArgs args)
        {
            Console.WriteLine("Timeout Recognizer thread started, sleeping 3 seconds.");
            Thread.Sleep(3000);
            RecognitionResult result = new RecognitionResult();
            result.UserTriggered = true;

            return result;
        }
    }

    public class GateRecognizer : SketchRecognizer
    {
        private Recognizers.GateRecognizer classify;

        public GateRecognizer()
        {
            classify = new Recognizers.GateRecognizer(
                "data/gate.model", 
                new string[] { 
                    "data/and.amat", 
                    "data/nand.amat", 
                    "data/nor.amat", 
                    "data/not.amat", 
                    "data/or.amat" }, 
                32, 32);

        }

        public override RecognitionResult Recognize(RecognitionArgs args)
        {            
            Substroke[] substrokes = args.Sketch.Substrokes;
            RecognitionResult result = new RecognitionResult();
            result.UserTriggered = true;
            result.Sketch = args.Sketch;

            Recognizers.Recognizer.Results results = classify.Recognize(substrokes);
            result.status = results.ToString();

            System.Drawing.Color c = classify.ToColor(results);        
                        
            foreach (Substroke s in substrokes)
                s.XmlAttrs.Color = c.ToArgb();        

            return result;
        }
    }

    public class PartialGateRecognizer : SketchRecognizer
    {
        private Recognizers.PartialGateRecognizer classify;

        public PartialGateRecognizer()
        {
            classify = new Recognizers.PartialGateRecognizer(
                "data/partial.model",
                new string[] { 
                    "data/backline.amat", 
                    "data/backarc.amat", 
                    "data/frontarc.amat", 
                    "data/bubble.amat" },
                32, 32);

        }

        public override RecognitionResult Recognize(RecognitionArgs args)
        {
            Substroke[] substrokes = args.Sketch.Substrokes;
            RecognitionResult result = new RecognitionResult();
            result.UserTriggered = true;
            result.Sketch = args.Sketch;

            Recognizers.Recognizer.Results results = classify.Recognize(substrokes);
            result.status = results.ToString();

            System.Drawing.Color c = classify.ToColor(results);

            foreach (Substroke s in substrokes)
                s.XmlAttrs.Color = c.ToArgb();

            return result;
        }
    }

    #endregion
}
