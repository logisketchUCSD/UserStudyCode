using System;
using System.Drawing;

using Microsoft.Ink;

namespace UserStudyUI
{
	/// <summary>
	/// Stores constant parameters for the User Interface
	/// </summary>
	public class UIConstants 
	{
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Default Ink Appearance Parameters
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Default Ink Appearance Parameters

		/// <summary>
		/// The default color for Ink.
		/// </summary>
		public static Color DefaultInkColor = Color.Black;

		/// <summary>
		/// The sketch annotation drawing color.
		/// </summary>
		public static Color AnnotationColor = Color.Silver;

		/// <summary>
		/// Wire label value and color for wire recognizer/feedback.  The wire recognizer applies this label to (almost) all
		/// strokes in a sketch.
		/// </summary>
		public const string WireLabel = "Wire";
		public static Color WireColor = Color.Blue;

		/// <summary>
		/// Error label value and color for wire recognizer/feedback.  The wire recognizer applies this label to a small
		/// percentage of strokes.
		/// </summary>
		public const string ErrorLabel = "Error";
		public static Color ErrorColor = Color.Red;

		/// <summary>
		/// When loading a new sketch, the sketch is scaled to 
		/// the current size of the Form.  This factor
		/// is the number of pixels to offset the scale by
		/// in order to pad the sketch such that scrollbars
		/// do not appear when the sketch is loaded.
		/// </summary>
		public const int scaleOffset = 10;

		/// <summary>
		/// Scaling factors for sketch zoom in/out controls.
		/// </summary>
		public const float zoomInFactor = 1.1F;
		public const float zoomOutFactor = 0.9F;

		#endregion

		
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Recognition Trigger Parameters
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Recognition Trigger Parameters

		/// <summary>
		/// Duration of pause for Pause recognition trigger
		/// in milliseconds.
		/// </summary>
		public const int PauseTriggerWait = 4000;

		/// <summary>
		/// Path to the pre-labelled sketch used by the 
		/// Sample (Dummy) recognizer.
		/// 
		/// This path should work for anybody who runs the UI
		/// through Visual Studio's Debug feature.  The standard 
		/// location for the sample sketch is in the project's
		/// main directory, which is two directories back from the 
		/// Debug directory.  
		/// </summary>
		public const string DummyRecognitionFilepath = "..\\..\\sampleSketch.labeled.xml";
	
		/// <summary>
		/// Threshold for gesture recognition confidence.  Any  
		/// gestures below this confidence level will be ignored.
		/// </summary>
		public static Microsoft.Ink.RecognitionConfidence GestureConfidenceThreshhold = Microsoft.Ink.RecognitionConfidence.Intermediate;

		/// <summary>
		/// Simulated error rate for the wire recognizer.  Approximately WireLabelerErrorRate * 100%
		/// of the strokes recognizer will receive the "error" label.
		/// </summary>
		public const double ErrorLabelErrorRate = 0.1;

		/// <summary>
		/// The maximum amount of time that can elapse between a 'check' and a 'tap' gesture
		/// in order for Check-Tap recognition to be fired.  Time is represented in Ticks (or
		/// hundreds of nanoseconds); divide this constant by 100,000,000 to get the time threshold in 
		/// seconds.
		/// </summary>
		public const long CheckTapGestureThreshold = 150000000L;

		/// <summary>
		/// Threshold for lasso-tap selection.  This parameter represents the percentage of a stroke's
		/// points that must be within the user-drawn lasso selection area in order for that stroke
		/// to be included in the selection.
		/// </summary>
		public const float LassoTapSelectionThreshold = 50.0F;

		#endregion


		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Feedback Mechanism Parameters
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		#region Feedback Mechanism Parameters

		/// <summary>
		/// Path to the domain file used by the 
		/// domain aware feedback mechanisms.
		/// 
		/// This path should work for anybody who runs the UI
		/// through Visual Studio's Debug feature.  The standard 
		/// location for the sample sketch is in the project's
		/// main directory, which is two directories back from the 
		/// Debug directory.  
		/// </summary>
		public const string ColorDomainFilepath = "..\\..\\LogicGatesColorDomain.txt";

		/// <summary>
		/// Delay before tooltips are shown when using the Coloring Feedback Mechanism
		/// </summary>
		public const int ColorFeedbackMechanismTooltipDelay = 100;

		/// <summary>
		/// Maximum distance (in ink space) that the mouse pointer must be 
		/// away from a stroke in order to display a tool tip.
		/// </summary>
		public const int ColorFeedbackMechanismTooltipDistanceThreshold = 250;

		/// <summary>
		/// The color for wires when using the text feedback mechanism.
		/// </summary>
		public static Color TextFeedbackMechanismWireColor = Color.DarkBlue;

		/// <summary>
		/// The color for symbols when using the text feedback mechanism.
		/// </summary>
		public static Color TextFeedbackMechanismSymbolColor = Color.DarkGreen;

		#endregion

	}

	


	
}
