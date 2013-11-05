using System;

using Sketch;
using Converter;


namespace UserStudyUI
{
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Recognizer Framework
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Recognizer Framework

	/// <summary>
	/// Recognition events are handled by event subscribers through this
	/// delegate function.  The Recognizer will push events to 
	/// subscribers whenever recognition results are ready.  Allows a
	/// front end to be "pushed" with recognition results.
	/// <seealso cref="http://msdn2.microsoft.com/en-us/library/system.eventargs.aspx"/>
	/// </summary>
	public delegate void RecognitionResultsEventHandler(object source, RecognizerEventArgs re);


	/// <summary>
	/// A Recognizer performs recognition on sketches.  The
	/// recognizer receives sketch data from the front end,
	/// reocgnizes that sketch data, and then returns
	/// labeled results (and possibly other data) to the 
	/// front end.  This base Recognizer provides a 
	/// framework for building Recognizers that actually
	/// process sketch data.
	/// 
	/// Microsoft's documentation on event handling, as well
	/// as a helpful Fire Alarm example, is here:
	/// http://msdn2.microsoft.com/en-us/library/system.eventargs.aspx
	/// </summary>
	interface Recognizer
	{
		/// <summary>
		/// Recognition event that this class triggers.  Event subscribers should
		/// add their event handler callbacks to this member.
		/// <seealso cref="http://msdn2.microsoft.com/en-us/library/system.eventargs.aspx"/>
		/// </summary>
		event RecognitionResultsEventHandler RecognitionEvent;
		
		/// <summary>
		/// Triggers the recognition process.  Since a front end can trigger the
		/// recognizer both excplicitly (e.g., when the user activates
		/// a recognition trigger) and implicitly (e.g., after every stroke is drawn or
		/// other relevant ink events), a front end may call this method in various
		/// contexts (note the userTriggered flag).  The recognizer should
		/// report results of recognizing this data to any (and all) 
		/// RecognitionResultsEventHandlers subscribed to it (e.g., the front end). 
		/// </summary>
		/// <param name="sketch">A Sketch containing strokes to recognize</param>
		/// <param name="userTriggered">A flag that is true iff the user triggered recognition</param>
		void recognize(Sketch.Sketch sketch, bool userTriggered);

		/// <summary>
		/// Retrieves the latest set of recognition results from the Recognizer.  Allows
		/// a front end to "pull" recognition results.
		/// </summary>
		/// <returns></returns>
		RecognizerEventArgs getRecognitionResults();
	}


	/// <summary>
	/// A recognition event class.  The Recognizer
	/// passes recognition events to event subscribers
	/// in order to transfer labeling results and other
	/// data.
	/// 
	/// <seealso cref="http://msdn2.microsoft.com/en-us/library/system.eventargs.aspx"/>
	/// </summary>
	public class RecognizerEventArgs: EventArgs 
	{
		/// <summary>
		/// The recognized Sketch(es).  These sketches
		/// may contain several strokes or even the
		/// entire user drawn sketch, but each
		/// sketch must contain labelling results 
		/// (if any) from the recognizer.
		/// </summary>
		public Sketch.Sketch[] recognitionResults = new Sketch.Sketch[100];

		/// <summary>
		/// Time at which the recognition results were 
		/// completed.
		/// </summary>
		public DateTime eventTime;

		/// <summary>
		/// True iff these recognition results 
		/// are due to the user triggering recognition.
		/// </summary>
		public bool userTriggered;
	}

	#endregion

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Sample (Dummy) Recognizer
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Sample (Dummy) Recognizer

	/// <summary>
	/// Sample recognizer that simply returns a fixed pre-labeled sketch
	/// whenever the user triggers recognition.  The sketch returned is 
	/// a sample chosen from the E85 data set.  Uses the sketch in the file
	/// pointed at by the UIConstant DummyRecognitionFilepath. 
	/// <see cref="UIConstants.DummyRecognitionFilepath"/>
	/// </summary>
	public class DummyRecognizer : Recognizer 
	{
		private RecognizerEventArgs dummyRecognitionResults;

		public event RecognitionResultsEventHandler RecognitionEvent;

		public DummyRecognizer() {
			// Read in the sketch
			Sketch.Sketch dummySketch = (new ReadXML(UIConstants.DummyRecognitionFilepath)).Sketch;
			
			// Construct the recognition results to return
			dummyRecognitionResults = new RecognizerEventArgs();
			dummyRecognitionResults.recognitionResults[0] = dummySketch;
			dummyRecognitionResults.eventTime = DateTime.Now;
			dummyRecognitionResults.userTriggered = true;
		}

		/// <summary>
		/// When the user manually triggers recognition, send a pre-labeled
		/// sketch back to subscribers of this recognizer (e.g., the front end). 
		/// </summary>
		void Recognizer.recognize(Sketch.Sketch sketch, bool userTriggered)
		{	
			if (userTriggered)
				RecognitionEvent(this, dummyRecognitionResults);
		}

		/// <summary>
		/// Returns pre-labeled sketch as a recognition result.
		/// </summary>
		/// <returns>The dummy recognition result</returns>
		RecognizerEventArgs Recognizer.getRecognitionResults()
		{
			return dummyRecognitionResults;
		}
	}


	#endregion

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Sample Wire Label Recognizer
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Sample Wire Label Recognizer

	/// <summary>
	/// Sample recognizer that labels all strokes it sees as a wire.    
	/// </summary>
	public class WireLabelRecognizer : Recognizer 
	{
		private RecognizerEventArgs lastLabeledSketchArgs;

		public event RecognitionResultsEventHandler RecognitionEvent;

		public WireLabelRecognizer() 
		{
			
		}

		/// <summary>
		/// When the user manually triggers recognition, label everything
		/// in the sketch as a wire. 
		/// </summary>
		void Recognizer.recognize(Sketch.Sketch sketch, bool userTriggered)
		{	
			// Only operate on user triggered recognition events
			if (!userTriggered)
				return;

			foreach (Sketch.Substroke substroke in sketch.Substrokes) 
			{
				sketch.AddLabel(substroke, UIConstants.WireLabel, 1.0);
			}

			lastLabeledSketchArgs = new RecognizerEventArgs();
			lastLabeledSketchArgs.recognitionResults[0] = sketch;
			lastLabeledSketchArgs.eventTime = DateTime.Now;
			lastLabeledSketchArgs.userTriggered = true;

			RecognitionEvent(this, lastLabeledSketchArgs);
		}

		/// <summary>
		/// Returns the last labeled sketch.
		/// </summary>
		/// <returns>The recognition result</returns>
		RecognizerEventArgs Recognizer.getRecognitionResults()
		{
			return lastLabeledSketchArgs;
		}
	}

	#endregion

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Wizard Recognizer
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	#region Wizard Recognizer

	#endregion

}
