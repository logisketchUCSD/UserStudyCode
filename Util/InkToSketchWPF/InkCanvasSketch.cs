using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Ink;
using System.Windows.Media;
using System.Windows.Ink;
using System.Windows.Controls;
using System.Windows.Input;
using Sketch;
using Featurefy;
using ConverterXML;

namespace InkToSketchWPF
{
    public delegate void OnFlyRecognitionHandler();


    /// <summary>
    /// Binds a Microsoft InkCanvas object and a HMC Sketch object together.
    /// </summary>
    public class InkCanvasSketch : InkSketch
    {
        #region Internals

        /// <summary>
        /// The InkCanvas to wrap
        /// </summary>;
        private InkCanvas mInkCanvas;

        // This is the standard sample rate according to http://msdn.microsoft.com/msdnmag/issues/03/10/TabletPC/default.aspx
        private const float SAMPLE_RATE = 133.0f;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.  Creates an empty FeatureSketch and Sketch.
        /// </summary>
        /// <param name="inkPic">The InkCanvas with which to synchronize</param>
        public InkCanvasSketch(InkCanvas inkCanvas)
            : this(inkCanvas, FeatureSketch.MakeFeatureSketch(new Sketch.Sketch()))
        {
        }

        /// <summary>
        /// Constructor for testing (where we pass in an initialized featureSketch)
        /// </summary>
        /// <param name="inkCanvas"></param>
        /// <param name="featureSketch"></param>
        public InkCanvasSketch(InkCanvas inkCanvas, FeatureSketch featureSketch)
        {
            mInkCanvas = inkCanvas;
            mFeatureSketch = featureSketch;
            mSketch = featureSketch.Sketch;

            // Comply with XML format requirements
            dtGuid = new Guid(dtGuidString);
            idGuid = new Guid(idGuidString);
            Sketch.Units = DefaultSketchUnits;

            // Intialize other fields
            ink2sketchStr = new Dictionary<String, Guid?>();
            sketchStr2ink = new Dictionary<Guid?, String>();
            substrokeIdMap = new Dictionary<Guid?, Substroke>();

            subscribeToInk();
        }

        #endregion

        #region InkCanvas Event Listening Methods

        /// <summary>
        /// Clears out the internal data structures for loading
        /// </summary>
        public void Clear()
        {
            mSketch = new Sketch.Sketch();
            mFeatureSketch = FeatureSketch.MakeFeatureSketch(mSketch);

            ink2sketchStr.Clear();
            sketchStr2ink.Clear();
            substrokeIdMap.Clear();
        }

        /// <summary>
        /// Enables the synchronization of Ink to Sketch and subscribes to InkPicture events.
        /// </summary>
        protected override void subscribeToInk()
        {
            base.subscribeToInk();
        }    

        /// <summary>
        /// Disables the synchronization of Ink to Sketch and unsubscribes to InkCanvas events.
        /// </summary>
        protected override void unsubscribeToInk()
        {
            base.unsubscribeToInk();
        }

        #endregion

        #region Change strokes

        /// <summary>
        /// Refreshes the underlying data structures corresponding to the inkStroke.
        /// </summary>
        public void UpdateInkStrokes(System.Windows.Ink.StrokeCollection inkStrokes)
        {
            foreach (System.Windows.Ink.Stroke inkStroke in inkStrokes)
                UpdateInkStroke(inkStroke);
        }

        /// <summary>
        /// Refreshes the underlying data structures corresponding to the inkStroke after the
        /// inkStroke has been modified (moved or resized)
        /// </summary>
        /// <param name="inkStroke">The Ink Stroke to modify</param>
        public void UpdateInkStroke(System.Windows.Ink.Stroke inkStroke)
        {
            // Delete the old stroke but save its shape for the new Stroke
            Sketch.Shape oldShape = GetSketchSubstrokeByInk(inkStroke).ParentShape;
            DeleteStroke(inkStroke);
            
            // Add the new stroke to the Sketch and put it in the old Shape
            AddStroke(inkStroke);
            Sketch.Substroke newSubstroke = GetSketchSubstrokeByInk(inkStroke);
            if (oldShape != null)
            {
                oldShape.AddSubstroke(newSubstroke);
                if (!Sketch.ShapesL.Contains(oldShape))
                    Sketch.AddShape(oldShape);
            }
        }

        /// <summary>
        /// Adds a collection of Strokes to the Sketch and FeatureSketch.
        /// </summary>
        public void AddStrokes(StrokeCollection inkStrokes)
        {
            foreach (System.Windows.Ink.Stroke iStroke in inkStrokes)
                AddStroke(iStroke);
        }


        /// <summary>
        /// Adds a Stroke to our internal data structures - the Sketch and FeatureSketch
        /// </summary>
        /// Precondition: The InkStroke is not in the InkCanvas's collection of strokes.
        /// Postcondition: The InkStroke has been added to the InkCanvas, Sketch's list of 
        /// Strokes, Substrokes, and FeatureSketch. 
        /// <param name="inkStroke"></param>
        public void AddStroke(System.Windows.Ink.Stroke inkStroke)
        {
            if (mInkCanvas.Strokes.Contains(inkStroke))
                throw new Exception("adding a stroke to the InkCanvasSketch, but it was already there!");
            // Format the stroke
            inkStroke.AddPropertyData(dtGuid, (ulong)DateTime.Now.ToFileTime());
            string strokeId = Guid.NewGuid().ToString();
            inkStroke.AddPropertyData(idGuid, strokeId);

            // Update the dictionaries
            Sketch.Stroke sketchStroke = new Sketch.Stroke(inkStroke, dtGuid, SAMPLE_RATE);
            ink2sketchStr.Add(strokeId, sketchStroke.Substrokes[0].XmlAttrs.Id);
            sketchStr2ink.Add(sketchStroke.Substrokes[0].Id, strokeId);
            substrokeIdMap.Add(sketchStroke.Substrokes[0].Id, sketchStroke.Substrokes[0]);

            // Add it to the relevant data structures (InkCanvas last.)
            mFeatureSketch.AddStroke(sketchStroke);
            mInkCanvas.Strokes.Add(inkStroke);
        }

        /// <summary>
        /// Deletes a collection of Strokes and updates the Sketch.
        /// </summary>
        /// <param name="iStroke">The Stroke to delete</param>
        public void DeleteStrokes(System.Windows.Ink.StrokeCollection inkStrokes)
        {
            foreach (System.Windows.Ink.Stroke iStroke in inkStrokes)
                DeleteStroke(iStroke);
        }
        /// <summary>
        /// Deletes a Stroke from our internal data structures - the Sketch and FeatureSketch
        /// </summary>
        /// Precondition: The InkStroke is in the InkCanvas's collection of strokes.
        /// Postcondition: The InkStroke has been removed to the Sketch's list of 
        /// Strokes, Substrokes, and FeatureSketch. 
        /// <param name="inkStroke"></param>
        public void DeleteStroke(System.Windows.Ink.Stroke inkStroke)
        {
            // Update mappings
            Substroke substroke = substrokeIdMap[ink2sketchStr[(String)inkStroke.GetPropertyData(idGuid)]];
            ink2sketchStr.Remove((String)inkStroke.GetPropertyData(idGuid));
            sketchStr2ink.Remove(substroke.Id);
            substrokeIdMap.Remove(substroke.Id);

            // Remove it from the relevant data structures (InkCanvas last)
            mFeatureSketch.RemoveSubstroke(substroke);
            mInkCanvas.Strokes.Remove(inkStroke);
        }

        #endregion

        #region Loading/Saving
        /// <summary>
        /// Load a Stroke with a corresponding substroke. 
        /// Precondition: Substroke is in the Sketch/FeatureSketch.
        /// Postcondition: The stroke's in our bookkeeping data structures.
        /// </summary>
        /// <param name="inkStroke">The Stroke to add</param>
        public void AddInkStrokeWithSubstroke(System.Windows.Ink.Stroke inkStroke, Sketch.Substroke substroke)
        {
            // Format the inkStroke
            inkStroke.AddPropertyData(dtGuid, (ulong)DateTime.Now.ToFileTime());
            String strokeId = System.Guid.NewGuid().ToString();
            inkStroke.AddPropertyData(idGuid, strokeId);

            // Add it to the data structures (InkCanvas last)
            ink2sketchStr.Add(strokeId, substroke.XmlAttrs.Id);
            sketchStr2ink.Add(substroke.XmlAttrs.Id, strokeId);
            substrokeIdMap.Add(substroke.XmlAttrs.Id, substroke);
            mInkCanvas.Strokes.Add(inkStroke);
        }

        /// <summary>
        /// Loads a Sketch from a file and synchronizes StrokeCollection.  Clears the current contents
        /// of this InkSketch.
        /// </summary>
        /// <param name="filename">The path of the file to load</param>
        public StrokeCollection LoadSketch(string filepath)
        {
            Clear();
            if (System.IO.File.Exists(filepath) && System.IO.Path.GetExtension(filepath.ToLower()) == ".xml")
                return LoadSketch((new ReadXML(filepath)).Sketch);
            else
                return null;
        }

        /// <summary>
        /// Loads a Sketch into the InkSketch
        /// </summary>
        /// <param name="sketch">The sketch to load</param>
        /// Precondition: The InkSketch is clean (all dictionaries cleared etc)
        /// Postcondition: The InkSketch has the proper data members
        public StrokeCollection LoadSketch(Sketch.Sketch sketch)
        {
            unsubscribeToInk();

            foreach (Shape shape in sketch.Shapes)
            {
                if (shape.Type != new Domain.ShapeType())
                {
                    shape.AlreadyGrouped = true;
                    shape.AlreadyLabeled = true;
                }
            }

            mSketch = sketch;
            mFeatureSketch = FeatureSketch.MakeFeatureSketch(sketch);

            subscribeToInk();

            return CreateInkStrokesFromSketch(); ;
        }

        /// <summary>
        /// Returns a list of ink strokes from the existing sub strokes
        /// </summary>
        /// Precondition: Sketch has been loaded into the inkSketch
        public StrokeCollection CreateInkStrokesFromSketch()
        {
            StrokeCollection strokeColl = new StrokeCollection();

            foreach (Substroke sub in mSketch.Substrokes)
            {
                StylusPointCollection points = new StylusPointCollection();

                foreach (Sketch.Point point in sub.Points)
                {
                    StylusPoint styPoint = new StylusPoint(point.X, point.Y);
                    if (point.Pressure > 1)
                        styPoint.PressureFactor = 1;
                    else if (point.Pressure < 0)
                        styPoint.PressureFactor = 0;
                    else
                        styPoint.PressureFactor = point.Pressure;
                    points.Add(styPoint);
                }

                System.Windows.Ink.Stroke stroke = new System.Windows.Ink.Stroke(points);

                stroke.DrawingAttributes.FitToCurve = true;
                stroke.DrawingAttributes.IgnorePressure = false;

                System.Windows.Media.Color color = new System.Windows.Media.Color();
                color = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
                if (sub.XmlAttrs.Color != null) stroke.DrawingAttributes.Color = color;
                if (sub.XmlAttrs.PenWidth != null) stroke.DrawingAttributes.Width = Convert.ToSingle(sub.XmlAttrs.PenWidth);
                if (sub.XmlAttrs.PenHeight != null) stroke.DrawingAttributes.Height = Convert.ToSingle(sub.XmlAttrs.PenHeight);
                if (sub.XmlAttrs.PenTip != null)
                {
                    if (sub.XmlAttrs.PenTip.Equals("Ball")) stroke.DrawingAttributes.StylusTip = StylusTip.Ellipse;
                    else if (sub.XmlAttrs.PenTip.Equals("Rectangle")) stroke.DrawingAttributes.StylusTip = StylusTip.Rectangle;
                }

                strokeColl.Add(stroke);
                AddInkStrokeWithSubstroke(stroke, sub);
            }
            return strokeColl;
        }

        /// <summary>
        /// Saves this Sketch to the specified file path.
        /// </summary>
        /// <param name="filename">The path of the file to write</param>
        public void SaveSketch(string filename, CircuitSimLib.Circuit circuit)
        {
            SaveToXML xmlHolder = new SaveToXML(Sketch, circuit);
            xmlHolder.WriteXML(filename);
        }

        /// <summary>
        /// Saves this Sketch to the specified file path.
        /// </summary>
        /// <param name="filename">The path of the file to write</param>
        public void SaveSketch(string filename)
        {
            SaveSketch(filename, null);
        }

        #endregion

        #region Testing
        /// <summary>
        /// Checks whether the sizes of the dictionaries are the same. Returns -1 if not.
        /// </summary>
        /// <returns></returns>
        public int areDictionarySizesConsistent()
        {
            bool equalSize = (ink2sketchStr.Count == sketchStr2ink.Count) && (ink2sketchStr.Count== substrokeIdMap.Count);
            if (equalSize)
                return ink2sketchStr.Count;
            return -1;
        }

        #endregion

        #region Stroke getters
        /// <summary>
        /// Returns a the sketch substroke corresponding to the given
        /// Ink stroke ID.  Returns null if no corresponding 
        /// Sketch stroke exists.
        /// </summary>
        /// <param name="id">The Id of the Ink stroke</param>
        /// <returns>The corresponding Sketch substroke</returns>
        public Substroke GetSketchSubstrokeByInkId(String id)
        {
            Guid? subId = null;
            ink2sketchStr.TryGetValue(id, out subId);

            if (subId == null)
                return null;

            Substroke sub = null;
            substrokeIdMap.TryGetValue(subId, out sub);
            return sub;
        }

        /// <summary>
        /// Returns a the sketch substroke corresponding to the given
        /// Ink stroke.  Returns null if no corresponding 
        /// Sketch stroke exists.
        /// </summary>
        /// <param name="id">The Ink stroke</param>
        /// <returns>The corresponding Sketch substroke</returns>
        public Substroke GetSketchSubstrokeByInk(System.Windows.Ink.Stroke stroke)
        {
            String id = (String)stroke.GetPropertyData(idGuid);
            return GetSketchSubstrokeByInkId(id);
        }


        /// <summary>
        /// Returns the stroke corresponding to the given
        /// Sketch substroke Id.  Returns null if no corresponding
        /// Ink stroke exists.
        /// </summary>
        /// <param name="substr">The Id of the Substroke to look up</param>
        /// <returns>The corresponding Ink stroke</returns>
        public String GetInkStrokeBySubstrokeId(Guid? substrId)
        {
            String index;
            sketchStr2ink.TryGetValue(substrId, out index);
            return index;
        }

        /// <summary>
        /// Returns the stroke corresponding to the given
        /// Sketch substroke.  Returns null if no corresponding
        /// Ink stroke exists.
        /// </summary>
        /// <param name="substr">The Substroke to look up</param>
        /// <returns>The corresponding Ink stroke</returns>
        public System.Windows.Ink.Stroke GetInkStrokeBySubstroke(Substroke substroke)
        {
            Guid? substrId = substroke.XmlAttrs.Id;
            String index;
            sketchStr2ink.TryGetValue(substrId, out index);
            return GetInkStrokeById(index);
        }

        /// <summary>
        /// Returns the stroke corresponding to the given
        /// Sketch substroke.  Returns -1 if no corresponding stroke exists.
        /// </summary>
        /// <param name="substr">The Substroke to look up</param>
        /// <returns>The corresponding Ink stroke</returns>
        public String GetInkStrokeIDBySubstroke(Substroke substroke)
        {
            return GetInkStrokeIdBySubstrokeId(substroke.XmlAttrs.Id);
        }

        /// <summary>
        /// Returns the Guid of the stroke corresponding to the given
        /// Sketch substroke Guid.  Returns -1 (an invalid Guid) if no 
        /// corresponding stroke exists.
        /// </summary>
        /// <param name="substr">The Guid of the substroke to look up</param>
        /// <returns>The Guid of the corresponding stroke; 
        /// returns NullInkStrokeId if no such Guid exists</returns>
        public String GetInkStrokeIdBySubstrokeId(Guid? substrId)
        {
            if (!sketchStr2ink.ContainsKey(substrId))
                return null;
            else
                return sketchStr2ink[substrId];
        }

        /// <summary>
        /// Returns the substroke that corresponds to the given
        /// substroke Id
        /// </summary>
        /// <param name="substrId">The Guid of the substroke to look up</param>
        /// <returns>The corresponding substroke or null if no such substroke exists</returns>
        public Substroke GetSubstrokeById(Guid? substrId)
        {
            if (!substrokeIdMap.ContainsKey(substrId))
                return null;
            else
                return substrokeIdMap[substrId];
        }


        
        public System.Windows.Ink.Stroke GetInkStrokeById(String inkId)
        {
            foreach (System.Windows.Ink.Stroke stroke in mInkCanvas.Strokes)
            {
                if ((String)stroke.GetPropertyData(idGuid) == inkId)
                    return stroke;
            }
            return null;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the InkCanvas that this InkPictureSketch wraps.
        /// Setting the InkPicture will delete all strokes in this InkPictureSketch's
        /// Sketch.
        /// </summary>
        public InkCanvas InkCanvas
        {
            get
            {
                return mInkCanvas;
            }

            set
            {
                unsubscribeToInk();
                mInkCanvas = value;
                subscribeToInk();
            }
        }
        #endregion

    }
}
