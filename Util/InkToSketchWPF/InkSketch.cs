using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using ConverterXML;
using Sketch;
using Featurefy;
using System.Windows.Ink;
using System.Windows.Input;

namespace InkToSketchWPF
{
    /// <summary>
    /// Binds a Microsoft Ink object and a HMC Sketch object together.
    /// July 2011: We primarily work with instances of its subclass, InkCanvasSketch.
    /// </summary>
    public class InkSketch
    {
        #region Internals


        public System.Windows.Ink.Stroke NullStroke = null;

        protected Sketch.Sketch mSketch;

        protected Featurefy.FeatureSketch mFeatureSketch;

        /// <summary>
        /// Ink Stroke IDs to Sketch Substrokes
        /// </summary>
        protected Dictionary<String, Guid?> ink2sketchStr;

        /// <summary>
        /// Sketch Substrokes to Ink Stroke IDs
        /// </summary>
        protected Dictionary<Guid?, String> sketchStr2ink;

        /// <summary>
        /// Map of Sketch Substroke IDs to actual Substrokes
        /// </summary>
        protected Dictionary<Guid?, Substroke> substrokeIdMap;

        /// <summary>
        /// True iff the InkSketch is currently recording Ink strokes
        /// to the sketch.
        /// </summary>
        protected bool recording;

        /// <summary>
        /// Default units for sketch
        /// </summary>
        public static string DefaultSketchUnits = "pixel";

        /// <summary>
        ///  The Guid string that will be used to add time data to each stroke
        /// </summary>
        protected const string dtGuidString = "03457307-3475-3450-3035-640435034540";

        /// <summary>
        ///  The Guid string that will be used to identify the stroke
        /// </summary>
        protected const string idGuidString = "03457307-3475-3450-3035-640435034542";

        /// <summary>
        /// The Guid for dtGuidString
        /// </summary>
        protected Guid dtGuid;

        /// <summary>
        /// The Guid for idGuidString
        /// </summary>
        protected Guid idGuid;

        #endregion

        #region Ink Event Listening Methods
        /// <summary>
        /// Subscribes InkSketch to StrokeCollection events.
        /// </summary>
        protected virtual void subscribeToInk()
        {
            if (recording)
                return;

            recording = true;
            
        }

        /// <summary>
        /// Unsubscribes InkSketch to StrokeCollection events.
        /// </summary>
        protected virtual void unsubscribeToInk()
        {
            if (!recording)
                return;

            recording = false;
        }

        #endregion

        #region Properties and Getters
        /// <summary>
        /// Gets or sets the Sketch that this InkSketch wraps.
        /// Setting Sketch will clear the Ink and synchronize Ink with Sketch.
        /// </summary>
        public Sketch.Sketch Sketch
        {
            get
            {
                return mFeatureSketch.Sketch;
            }
        }

        /// <summary>
        /// Gets or sets the FeatureSketch that this InkSketch wraps.
        /// </summary>
        public Featurefy.FeatureSketch FeatureSketch
        {
            get
            {
                return mFeatureSketch;
            }
        }

        /// <summary>
        /// Gets the dtIdGuid
        /// </summary>
        public Guid DTGuid
        {
            get { return dtGuid; }
        }


        /// <summary>
        /// Gets the idGuid
        /// </summary>
        public Guid IDGuid
        {
            get { return idGuid; }
        }



        #endregion

    }
}
