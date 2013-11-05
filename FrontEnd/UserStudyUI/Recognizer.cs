using System;
using System.IO;
using System.Xml;
using System.Threading;
using System.Collections;

using Sketch;
using ConverterXML;
using ConverterJnt;
using Labeler;


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
		/// recognizer both explicitly (e.g., when the user activates
		/// a recognition trigger) and implicitly (e.g., after every stroke is drawn or
		/// other relevant ink events), a front end may call this method in various
		/// contexts (note the userTriggered flag).  The recognizer should
		/// report results from recognizing this data to any (and all) 
		/// RecognitionResultsEventHandlers subscribed to it (e.g., the front end and 
		/// possible other classes). 
		/// </summary>
		/// <param name="sketch">A Sketch containing strokes to recognize</param>
		/// <param name="selectedStrokes">The strokes that the user has specifically 
		/// selected for recognition, if any</param>
		/// <param name="userTriggered">A flag that is true iff the user triggered recognition</param>
		void recognize(Sketch.Sketch sketch, Sketch.Stroke[] selectedStrokes, bool userTriggered);

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
	public class RecognizerEventArgs : EventArgs 
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
		void Recognizer.recognize(Sketch.Sketch sketch, Sketch.Stroke[] selectedStrokes, bool userTriggered)
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

		private Random randobj = new Random();

		public event RecognitionResultsEventHandler RecognitionEvent;

		public WireLabelRecognizer() 
		{
			
		}

		/// <summary>
		/// When the user manually triggers recognition, label everything
		/// in the sketch as a wire. 
		/// </summary>
		void Recognizer.recognize(Sketch.Sketch sketch, Sketch.Stroke[] selectedStrokes, bool userTriggered)
		{	
			// Only operate on user triggered recognition events
			if (!userTriggered)
				return;

			lastLabeledSketchArgs = new RecognizerEventArgs();
			
			foreach (Sketch.Stroke selectedStroke in selectedStrokes) 
			{
				if (selectedStroke == null)
				{
					// The recognition triggerer left empty elements in 
					// the selected strokes array.  Here we just ignore
					// the lack of a stroke.
					continue;
				}
				
				
				// Get the corresponding stroke in the sketch to make sure we label
				// the right substrokes
				Sketch.Stroke sketchStroke = sketch.GetStroke((System.Guid) selectedStroke.XmlAttrs.Id);

				if (sketchStroke == null)
				{
					// The selected stroke is not in the sketch.
					// In some scenarios, we might want to throw an exception,
					// but in this case we just skip the stroke
					continue;
				}

				// Now label the substrokes
				Sketch.Substroke[] selectedSubstrokes = sketchStroke.Substrokes;
				foreach (Sketch.Substroke selectedSubstroke in selectedSubstrokes)
				{
					if (selectedSubstroke.Labels.Length > 0)
						continue;

					// Apply wire label or error label using simulated error rate
					if (randobj.NextDouble() < UIConstants.ErrorLabelErrorRate)
					{
						sketch.AddLabel(selectedSubstroke, UIConstants.ErrorLabel, 1.0);
					} 
					else 
					{
						sketch.AddLabel(selectedSubstroke, UIConstants.WireLabel, 1.0);
					}
				}
			}

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

	/// <summary>
	/// Delegate for receiving results from the networked Wizard labeler.  
	/// </summary>
	public delegate void WizardResultReceivedEventHandler(string xmlStr);

	/// <summary>
	/// Enumeration for the current error type and rate simulated
	/// </summary>
	public enum WizardError {OFF, E85OMIT, E85FALSEPOSITIVE};

	public class WizardRecognizer : Recognizer 
	{
		
		public event RecognitionResultsEventHandler RecognitionEvent;

		/// <summary>
		/// Current type of error simulated
		/// </summary>
		private WizardError currentError = WizardError.OFF;
		
		/// <summary>
		/// The most recent recognition result generated
		/// </summary>
		private RecognizerEventArgs currentRecognitionResult;

		/// <summary>
		/// The most current sketch received from the Wizard Labeler
		/// </summary>
		private Sketch.Sketch currentSketch;

		/// <summary>
		/// Stores the time when this recognizer last received a 
		/// sketch from the Wizard Labeler
		/// </summary>
		private DateTime lastSketchReceived;

		/// <summary>
		/// Stores the time when this recognizer last sent a 
		/// sketch to the Wizard Labeler
		/// </summary>
		private DateTime lastSketchSent = DateTime.Now;

		/// <summary>
		/// The minimum amount of time (in DateTime Ticks) that must elapse 
		/// between sending updated sketches to the Wizard labeler.  (When 
		/// the user triggers recognition, the sketch is sent to the Wizard 
		/// Labeler regardless of this threshold.)  Divide this number by
		/// 10000 to read it in milliseconds.
		/// </summary>
		public const long WizardMaxUpdateThreshold = 10000000L;

		/// <summary>
		/// True iff the recognizer is waiting for the wizard to 
		/// finish labeling a sketch that the user wants
		/// recognized (recognition trigger has been fired).
		/// </summary>
		private bool waitingOnWizard;

		/// <summary>
		/// SocketClient for recieving data from networked WizardLabler.  This
		/// recognizer hooks into an event on the SocketClient to recieve labeled
		/// sketches whenever the Wizard sends them.
		/// <see cref="DefaultNamespace.SocketClient#ResultReceived()"/>
		/// </summary>
		private DefaultNamespace.SocketClient socketClient;

		/// <summary>
		/// Stores the domain of the symbols that this recognizer recognizes.
		/// <see cref="FeedbackLayer.DomainAwareFeedbackMechanism"/>
		/// </summary>
		private DomainInfo domain;

		/// <summary>
		/// Stores a list of shapes (by ID) that the recognizer did not tag as erroneous.
		/// Helps ensure that the user does not see random errors pop up
		/// between receiving recognition results.
		/// </summary>
		private ArrayList previousCorrectShapes = new ArrayList();


		public WizardRecognizer() 
		{
			this.socketClient = new DefaultNamespace.SocketClient();
			this.socketClient.SketchRecievedEvent += new WizardResultReceivedEventHandler(socketClient_SketchRecievedEvent);
			this.socketClient.startSocketClient();
			this.domain = DomainAwareFeedbackMechanism.loadDomain();
		}

		/// <summary>
		/// Sends the sketch to the networked Wizard Labeler.
		/// </summary>
		/// <param name="sketch">The sketch to send</param>
		/// <param name="selectedStrokes">Selection of strokes to recognize (ignored for now)</param>
		/// <param name="userTriggered">True iff the user triggered recognition</param>
		void Recognizer.recognize(Sketch.Sketch sketch, Sketch.Stroke[] selectedStrokes, bool userTriggered)
		{
			// For fast sketchers, XML (un)marshalling causes a bottleneck. 
			// Here we limit the frequency with which the Wizard is updated
			// in order to avoid crashes due to this bottleneck.
			if (!userTriggered && (DateTime.Now.Ticks - lastSketchSent.Ticks) <= WizardMaxUpdateThreshold)
			{
				Console.WriteLine("Fast sketcher! Skipping Wizard Labeler Update.  Duration: " + (DateTime.Now.Ticks - lastSketchSent.Ticks));
				return;
			}

			Console.WriteLine("Firing send.  Duration: " + (DateTime.Now.Ticks - lastSketchSent.Ticks));
				
			
			// Create XML from the sketch
			MakeXML makeXml = new MakeXML(sketch);
			
			// If the user triggered recognition, then
			// internally remember this event so that
			// the recognizer will fire a recognition
			// event as soon as a result is received.
			this.waitingOnWizard = userTriggered;

			// Give XML to the SocketClient for transmittal
			string xmlStr = makeXml.getXMLstr();
			DefaultNamespace.SocketClient.SocketClientSendXml sendDelegate = 
				new DefaultNamespace.SocketClient.SocketClientSendXml(socketClient.SendXml);
			sendDelegate.BeginInvoke(xmlStr, userTriggered, null, null);

			//TextWriter errorWriter = new StreamWriter("errorSketch.xml");
			//errorWriter.Write(xmlStr);

			// Record when sketch was sent
			lastSketchSent = DateTime.Now;
		}

		/// <summary>
		/// Callback that handles results from the Wizard Labeler.  The SocketClient
		/// forwards each full sketch received to the recognizer.  This method 
		/// unmarshalls the received sketch and stores it internally; a 
		/// recognition event is fired only when the user has antecedently
		/// triggered recognition.
		/// </summary>
		/// <param name="xmlStr">The XML string that the Wizard Labeler sent.</param>
		private void socketClient_SketchRecievedEvent(string xmlStr)
		{
			// Read the XML string and fetch the sketch
			XmlTextReader xmlReader = new XmlTextReader(new StringReader(xmlStr));
			xmlReader.WhitespaceHandling = WhitespaceHandling.None;
			ReadXML readXml = new ReadXML(xmlReader);
			this.currentSketch = readXml.Sketch;
			this.lastSketchReceived = DateTime.Now;

			// Apply error to current sketch before constructing
			// a recognition result
			this.applyErrorToCurrentRecognitionResult();

			// Construct a recognition result
			this.currentRecognitionResult = new RecognizerEventArgs();
			this.currentRecognitionResult.eventTime = DateTime.Now;
			this.currentRecognitionResult.recognitionResults[0] = this.currentSketch;
			this.currentRecognitionResult.userTriggered = waitingOnWizard;

			// If we were waiting for the Wizard to finish labeling a sketch,
			// then fire a recognition event.  In the future, we might
			// 'fade in' recognition results as they are received.
			if (waitingOnWizard)
			{
				waitingOnWizard = false;
				RecognitionEvent(this, this.currentRecognitionResult);
			}
		}

		/// <summary>
		/// Gets the most recent recognition results.
		/// </summary>
		/// <returns></returns>
		public RecognizerEventArgs getRecognitionResults()
		{
			return currentRecognitionResult;
		}

		/// <summary>
		/// Shows and hides the GUI Form that controls
		/// the SocketClient.
		/// </summary>
		/// <param name="toggle">shows the SocketClient GUI Form if this parameter is true; hides
		/// the Form otherwise</param>
		public void showSocketClientGui(bool toggle)
		{
			this.socketClient.Visible = toggle;
		}

		/// <summary>
		/// Hook for reconnecting the Socket Client in the event of an error.
		/// </summary>
		public void reconnectSocketClient()
		{
			this.socketClient.restartSocketClient();
		}

		/// <summary>
		/// Applies appropriate simulated error to recognition result
		/// </summary>
		private void applyErrorToCurrentRecognitionResult()
		{
			if (currentError == WizardError.OFF)
			{
				previousCorrectShapes = new ArrayList(this.currentSketch.Shapes);
				return;
			}
			else if (currentError == WizardError.E85OMIT)
			{
				ArrayList selectedShapes = this.selectShapesFromCurrentRecognitionResult(0.15);
				foreach (Sketch.Shape shape in this.currentSketch.Shapes)
				{
					if (selectedShapes.Contains(shape))
					{
						this.currentSketch.RemoveLabel(shape);
					}
					else
					{
						previousCorrectShapes.Add(shape.XmlAttrs.Id);
					}
				}
			}
			else if (currentError == WizardError.E85FALSEPOSITIVE)
			{
				ArrayList selectedShapes = this.selectShapesFromCurrentRecognitionResult(0.15);
				Random randobj = new Random();
				foreach (Sketch.Shape shape in this.currentSketch.Shapes)
				{
					if (selectedShapes.Contains(shape))
					{
						// Remove correct label
						this.currentSketch.RemoveLabel(shape);
					
						// Pick new label
						ArrayList labels = domain.getLabels();
						labels.Remove(shape.XmlAttrs.Name);
						object[] labelsArr = labels.ToArray();
						int newLabelIndex = (int) Math.Floor(randobj.NextDouble() * labelsArr.Length);
					
						// Set incorrect label
						this.currentSketch.AddLabel(new System.Collections.Generic.List<Substroke>(shape.Substrokes), (string) labelsArr[newLabelIndex], 0.0);
					}
					else
					{
						previousCorrectShapes.Add(shape.XmlAttrs.Id);
					}
				}
			}
		}

		/// <summary>
		/// Randomly selects a given percent of shapes from the sketch.
		/// Only selects shapes that the user has not seen as being
		/// correctly recognized.
		/// </summary>
		/// <param name="percent">The percent of shapes to select</param>
		/// <returns>An ArrayList of the shapes selected</returns>
		private ArrayList selectShapesFromCurrentRecognitionResult(double percent)
		{
			ArrayList possibleShapes;
			if (previousCorrectShapes.Count == 0)
			{
				 possibleShapes = new ArrayList(this.currentSketch.Shapes);
			}
			else
			{
				possibleShapes = new ArrayList();
				foreach (Shape possibleShape in this.currentSketch.Shapes)
				{
					if (!previousCorrectShapes.Contains(possibleShape.XmlAttrs.Id))
					{
						possibleShapes.Add(possibleShape);
					}
				}
			} 
			
			int numShapes = possibleShapes.Count;
			int numShapesToSelect = (int) Math.Floor(percent * numShapes);
			Random randobj = new Random();
			
			Console.WriteLine("Selecting these many shapes for simulated error: " + numShapesToSelect);
			
			object[] possibleShapesArr = possibleShapes.ToArray();
			ArrayList selectedShapes = new ArrayList();
			int currentShape;
			while (selectedShapes.Count < numShapesToSelect)
			{
				currentShape = (int) Math.Floor(randobj.NextDouble() * possibleShapesArr.Length);
				if (!selectedShapes.Contains(possibleShapesArr[currentShape]))
				{
					selectedShapes.Add(possibleShapesArr[currentShape]);
				}
			}

			return selectedShapes;
		}

		/// <summary>
		/// Getter and setter for simulated error type
		/// </summary>
		public WizardError CurrentSimulatedError
		{
			get
			{
				return currentError;
			}

			set
			{
				currentError = value;
			}
		}
	}

	#endregion

}
