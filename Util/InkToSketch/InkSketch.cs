using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using ConverterJnt;
using ConverterXML;
using Sketch;
using Featurefy;

namespace msInkToHMCSketch
{
    public delegate void StrokeAddedEventHandler(Stroke str);
    public delegate void StrokeRemovedEventHandler(Substroke sub);
    public delegate void SketchLoadedEventHandler();

    /// <summary>
    /// Binds a Microsoft Ink object and a HMC Sketch object together.
    /// Dynamically updates the two data structures on the following events
    /// and use cases:
    ///    * Ink add stroke --> Sketch add stroke
    ///    * Ink delete stroke --> Sketch delete stroke
    ///    * Sketch load --> Reflect Sketch in Ink
    ///    * Ink load --> Reflect Ink in Sketch
    ///    * Ink transform --> Refelect transform in Sketch (supported but not
    ///                                                      hooked into Ink by default,
    ///                                                      see InkpictureSketch.)
    /// Also support several other Client functions, such as fetching a 
    /// Sketch Substroke by Ink stroke Id.
    /// 
    /// Example:
    /// 
    /// Microsoft.Ink mInk = new Microsoft.Ink();
    /// InkSketch inkSketch = new InkSketch(mInk); // inkSketch is now synchronized with mInk
    /// // Add strokes to mInk through Ink.AddStroke event
    /// return inkSketch.Sketch; // do something with the resulting Sketch
    /// </summary>
    public class InkSketch
    {
        #region Internals
        public event StrokeAddedEventHandler StrokeAdded;
        public event StrokeRemovedEventHandler StrokeRemoved;
        public event SketchLoadedEventHandler SketchLoaded;

        public const int NullInkStrokeId = -1;

        /// <summary>
        /// The FeatureSketch associated with this InkSketch
        /// </summary>
        protected FeatureSketch mFeatureSketch;

        /// <summary>
        /// The Ink assocated with this InkSketch
        /// </summary>
        protected Microsoft.Ink.Ink mInk;

        /// <summary>
        /// Ink Stroke IDs to Sketch Substrokes
        /// </summary>
        protected Dictionary<int, Guid?> ink2sketchStr;

        /// <summary>
        /// Sketch Substrokes to Ink Stroke IDs
        /// </summary>
        protected Dictionary<Guid?, int> sketchStr2ink;

        /// <summary>
        /// Map of Sketch Substroke IDs to actual Substrokes
        /// </summary>
        protected Dictionary<Guid?, Substroke> substrokeIdMap;

        /// <summary>
        /// ReadJnt instance for converting ink strokes
        /// to sketch strokes
        /// </summary>
        protected ReadJnt mReadJnt;

        /// <summary>
        /// True iff the InkSketch is currently recording Ink strokes
        /// to the sketch.
        /// </summary>
        protected bool recording;

        /// <summary>
        /// Default units for sketch
        /// </summary>
        public static string DefaultSketchUnits = "himetric";

        #endregion


        #region Constructors and initialization

        /// <summary>
        /// Default Constructor.  Creates and empty Ink
        /// and Sketch.
        /// </summary>
        public InkSketch()
        {
            init(null, null);
        }

        /// <summary>
        /// Constructor.  Creates an empty Ink object and synchronizes it
        /// with the contents of the given Sketch.
        /// </summary>
        /// <param name="sketch">The Sketch with which to synchronize</param>
        public InkSketch(FeatureSketch featureSketch)
        {
            init(null, featureSketch);
        }

        /// <summary>
        /// Constructor.  Creates an empty Sketch and synchronizes it 
        /// with the contents of the given Ink.
        /// </summary>
        /// <param name="ink">The Ink with which to synchronize</param>
        public InkSketch(Microsoft.Ink.Ink ink)
        {
            init(ink, null);
        }

        /// <summary>
        /// Constructor helper.  If both arguments are null, an empty
        /// instance of this class is created.  If Ink or Sketch
        /// is null, then one object is synchronized to the other
        /// (e.g. if ink is null, an empty Ink will be created and
        /// synchronized with Sketch).  If neither argument is null, then
        /// Sketch will be synchronized with Ink.  Subscribes to Ink events
        /// after any synchronization.
        /// </summary>
        /// <param name="ink">The Ink with which to (possibly) synchronize</param>
        /// <param name="sketch">The Sketch with which to (possibly) synchronize</param>
        protected virtual void init(Microsoft.Ink.Ink ink, FeatureSketch featureSketch)
        {
            // Instantiate new Ink and/or Sketch where necessary
            if (ink == null)
            {
                mInk = new Microsoft.Ink.Ink();
            }
            else
            {
                mInk = ink;
            }

            if (featureSketch == null)
            {
                mFeatureSketch = new FeatureSketch();
                Sketch.XmlAttrs.Id = System.Guid.NewGuid();
                Sketch.XmlAttrs.Units = DefaultSketchUnits;
            }
            else
            {
                mFeatureSketch = featureSketch;
            }

            // Intialize other fields
            mReadJnt = new ReadJnt();
            ink2sketchStr = new Dictionary<int, Guid?>();
            sketchStr2ink = new Dictionary<Guid?, int>();
            substrokeIdMap = new Dictionary<Guid?, Substroke>();

            // Synchronize Ink or Sketch as necessary
            if (ink == null && featureSketch != null)
            {
                LoadFeatureSketch(featureSketch);
            }
            else // (ink != null && featureSketch == null)
            {
                LoadInk(ink);
            }

            subscribeToInk();
        }

        /// <summary>
        /// Clears the contents of this InkSketch.  Deletes
        /// all Ink and Sketch strokes and maintains 
        /// synchronization between Ink and Sketch.
        /// </summary>
        public virtual void Clear()
        {
            mFeatureSketch = new FeatureSketch();
            Sketch.XmlAttrs.Id = System.Guid.NewGuid();
            Sketch.XmlAttrs.Units = DefaultSketchUnits;

            mInk = new Microsoft.Ink.Ink();

            if (ink2sketchStr != null) ink2sketchStr.Clear();
            if (sketchStr2ink != null) sketchStr2ink.Clear();
            if (substrokeIdMap != null) substrokeIdMap.Clear();
        }

        #endregion


        #region Ink Event Listening Methods
        /// <summary>
        /// Subscribes InkSketch to Ink events.
        /// </summary>
        protected virtual void subscribeToInk()
        {
            if (recording)
                return;

            recording = true;

            // Only Adds and Deletes are available on default Ink object.
            mInk.InkAdded += new Microsoft.Ink.StrokesEventHandler(mInk_InkAdded);
            mInk.InkDeleted += new Microsoft.Ink.StrokesEventHandler(mInk_InkDeleted);
        }

        /// <summary>
        /// Unsubscribes InkSketch to Ink events.
        /// </summary>
        protected virtual void unsubscribeToInk()
        {
            if (!recording)
                return;

            recording = false;

            mInk.InkAdded -= new Microsoft.Ink.StrokesEventHandler(mInk_InkAdded);
            mInk.InkDeleted -= new Microsoft.Ink.StrokesEventHandler(mInk_InkDeleted);
        }

        /// <summary>
        /// Forwards Ink deletes to Sketch deletes
        /// </summary>
        protected void mInk_InkDeleted(object sender, Microsoft.Ink.StrokesEventArgs e)
        {
            foreach (int id in e.StrokeIds)
            {
                DeleteInkStroke(id);
            }
        }

        /// <summary>
        /// Forwards Ink adds to Sketch adds
        /// </summary>
        protected void mInk_InkAdded(object sender, Microsoft.Ink.StrokesEventArgs e)
        {
            foreach (int id in e.StrokeIds)
            {
                AddInkStroke(id);
            }
        }

        #endregion


        #region Modify Ink (synchronize Sketch to Ink changes)

        /// <summary>
        /// Adds an Ink Stroke and updates the Sketch.  NOTE: Does NOT check
        /// to ensure that the given Ink stroke is contained within the 
        /// Ink object associated with this InkSketch.
        /// </summary>
        /// <param name="iStroke">The Ink Stroke to add</param>
        public void AddInkStroke(Microsoft.Ink.Stroke iStroke)
        {
            Sketch.Stroke sStroke = mReadJnt.InkStroke2SketchStroke(iStroke);
            ink2sketchStr.Add(iStroke.Id, sStroke.Substrokes[0].XmlAttrs.Id); // Converted stroke will always have
                                                                              // exactly one substroke.
            sketchStr2ink.Add(sStroke.Substrokes[0].XmlAttrs.Id, iStroke.Id);
            substrokeIdMap.Add(sStroke.Substrokes[0].XmlAttrs.Id, sStroke.Substrokes[0]);
            Sketch.AddStroke(sStroke);
            if(StrokeAdded != null)
                StrokeAdded(sStroke);
        }

        /// <summary>
        /// Adds an Ink stroke contained within the assocated
        /// Ink object and and updates the Sketch.  Checks to ensure
        /// that the given Ink stroke is contained within Ink.
        /// </summary>
        /// <param name="id">The id of the Ink Stroke to add</param>
        public void AddInkStroke(int id)
        {
            if (ink2sketchStr.ContainsKey(id))
                return;

            // Try to fetch the corresponding Ink stroke
            Microsoft.Ink.Stroke iStroke;
            try
            {
                using (Microsoft.Ink.Strokes strokes = mInk.Strokes)
                {
                    if (strokes.Count == id)
                        iStroke = strokes[id - 1];
                    else
                        iStroke = strokes[strokes.Count - 1];
                }
            }
            catch (ArgumentException)
            {
                return;
                // We should never in practice get here,
                // but if we do, it means the Ink stroke 
                // referenced is missing from mInk
                // !!!!!!!!! Eric 6-14-2009: 
                // You will get here anytime a stroke has
                // been deleted. Added a little catch that may
                // or may not work (check if strokes.Count == id)
            }

            AddInkStroke(iStroke);
        }

        /// <summary>
        /// Deletes an Ink Stroke and updates the Sketch.
        /// </summary>
        /// <param name="strokeId">The ID of the Ink Stroke to delete</param>
        public void DeleteInkStroke(int strokeId)
        {
            if (!ink2sketchStr.ContainsKey(strokeId))
            {
                System.Console.WriteLine("___No Stroke to delete___");
                return;
                //throw new Exception("No stroke to erase");
            }

            if (!substrokeIdMap.ContainsKey(ink2sketchStr[strokeId]))
                return;

            Substroke sStroke = substrokeIdMap[ink2sketchStr[strokeId]];
            Sketch.RemoveSubstroke(sStroke);
            
            // Update mapping
            ink2sketchStr.Remove(strokeId);
            sketchStr2ink.Remove(sStroke.XmlAttrs.Id);
            substrokeIdMap.Remove(sStroke.XmlAttrs.Id);
            if(StrokeRemoved != null)
                StrokeRemoved(sStroke);
        }

        /// <summary>
        /// Deletes an Ink Stroke and updates the Sketch.
        /// </summary>
        /// <param name="iStroke">The Ink Stroke to delete</param>
        public void DeleteInkStroke(Microsoft.Ink.Stroke iStroke)
        {
            DeleteInkStroke(iStroke.Id);
        }

        /// <summary>
        /// Transforms (e.g. moves or resizes) an Ink Stroke in the Sketch.  
        /// Converts the given Ink Stroke into a Sketch stroke and then 
        /// updates the Sketch to point to the new Ink Stroke instead of the old one.
        /// </summary>
        /// <param name="iStroke">The Ink Stroke to modify</param>
        public void TransformInkStroke(Microsoft.Ink.Stroke iStroke)
        {
            if (!ink2sketchStr.ContainsKey(iStroke.Id))
                return;

            if (!substrokeIdMap.ContainsKey(ink2sketchStr[iStroke.Id]))
                return;

            Sketch.Stroke newSStroke = mReadJnt.InkStroke2SketchStroke(iStroke);
            Substroke oldSStroke = substrokeIdMap[ink2sketchStr[iStroke.Id]];

            // FIXME need to masquerade Substroke Ids.  Here
            // we abitrarily synchronize the Ids of the first
            // substroke in each stroke.  We do this because
            // ReadJnt will always return a stroke with one
            // substroke.  In some use cases (e.g. fragmenting),
            // matching Ids like this might cause problems. 
            //
            // Eric Peterson: June 30, 2009: When loading an old
            // xml sketch, we need to preserve the time data for 
            // each point.
            newSStroke.Substrokes[0].XmlAttrs.Id = oldSStroke.XmlAttrs.Id;
            for (int i = 0; i < oldSStroke.Points.Length; i++)
                newSStroke.Substrokes[0].Points[i].Time = oldSStroke.Points[i].Time;

            // Update mapping between strokes
            ink2sketchStr.Remove(iStroke.Id);
            ink2sketchStr.Add(iStroke.Id, newSStroke.Substrokes[0].XmlAttrs.Id);
            sketchStr2ink.Remove(oldSStroke.XmlAttrs.Id);
            sketchStr2ink.Add(newSStroke.Substrokes[0].XmlAttrs.Id, iStroke.Id);
            substrokeIdMap.Remove(newSStroke.Substrokes[0].XmlAttrs.Id);
            substrokeIdMap.Add(newSStroke.Substrokes[0].XmlAttrs.Id, newSStroke.Substrokes[0]);

            // Update pointers in Sketch
            Sketch.Substroke newSSubstroke = newSStroke.Substrokes[0];
            foreach (Sketch.Shape shape in oldSStroke.ParentShapes)
            {
                shape.AddSubstroke(newSSubstroke);
            }

            Sketch.RemoveSubstroke(oldSStroke);
            Sketch.AddStroke(newSStroke);
        }

        /// <summary>
        /// Loads an Ink instance and synchronizes Sketch
        /// </summary>
        /// <param name="ink">The Ink to load</param>
        public void LoadInk(Microsoft.Ink.Ink ink)
        {
            unsubscribeToInk();

            this.Clear();

            mInk = ink;

            // Load strokes
            using (Microsoft.Ink.Strokes iStrokes = ink.Strokes)
            {
                foreach (Microsoft.Ink.Stroke iStroke in iStrokes)
                {
                    AddInkStroke(iStroke);
                }
            }

            subscribeToInk();
        }

        #endregion


        #region Modify Sketch (syncrhonize Ink to Sketch changes)

        /// <summary>
        /// Loads a Sketch from a file and synchronizes Ink.  Clears the current contents
        /// of this InkSketch.
        /// </summary>
        /// <param name="filename">The path of the file to load</param>
        public void LoadSketch(string filepath)
        {
            if (!System.IO.File.Exists(filepath))
                return;

            string extension = System.IO.Path.GetExtension(filepath.ToLower());
            if (extension.Equals(".jnt"))
            {
                LoadSketch((new ReadJnt(filepath)).Sketch);
            }
            else if (extension.Equals(".xml"))
            {
                LoadSketch((new ReadXML(filepath)).Sketch);
            }
        }

        /// <summary>
        /// Loads a Sketch and synchronizes Ink.  Clears the current contents
        /// of this InkSketch.
        /// </summary>
        /// <param name="sketch">The sketch to load</param>
        public void LoadSketch(Sketch.Sketch sketch)
        {
            FeatureSketch featureSketch = new FeatureSketch(ref sketch);
            LoadFeatureSketch(featureSketch);
        }

        /// <summary>
        /// Loads a Sketch and synchronizes Ink.  Clears the current contents
        /// of this InkSketch.
        /// </summary>
        /// <param name="sketch">The sketch to load</param>
        public void LoadFeatureSketch(FeatureSketch featureSketch)
        {
            unsubscribeToInk();

            this.Clear();

            mFeatureSketch = featureSketch;

            #region Create Collection - Load Ink Properties

            #region X

            Microsoft.Ink.TabletPropertyMetrics xMet = new Microsoft.Ink.TabletPropertyMetrics();
            xMet.Minimum = 0;
            xMet.Maximum = 24780;
            xMet.Resolution = 3003.63647f; //TABLET PEN ONLY 100.0f for mouse i think
            xMet.Units = Microsoft.Ink.TabletPropertyMetricUnit.Inches;

            System.Guid xGuid = Microsoft.Ink.PacketProperty.X;

            Microsoft.Ink.TabletPropertyDescription xDesc = new Microsoft.Ink.TabletPropertyDescription(xGuid, xMet);

            #endregion

            #region Y

            Microsoft.Ink.TabletPropertyMetrics yMet = new Microsoft.Ink.TabletPropertyMetrics();
            yMet.Minimum = 0;
            yMet.Maximum = 18630;
            yMet.Resolution = 3010.66577f; //TABLET PEN ONLY 100.0f for mouse i think
            yMet.Units = Microsoft.Ink.TabletPropertyMetricUnit.Inches;

            System.Guid yGuid = Microsoft.Ink.PacketProperty.Y;

            Microsoft.Ink.TabletPropertyDescription yDesc = new Microsoft.Ink.TabletPropertyDescription(yGuid, yMet);

            #endregion

            #region Pressure

            // Get max pressure in Sketch
            // Different input devices have different ranges of pressures
            int max = 0;
            foreach (Sketch.Point pt in Sketch.Points)
                max = Math.Max(max, pt.Pressure);

            Microsoft.Ink.TabletPropertyMetrics pMet = new Microsoft.Ink.TabletPropertyMetrics();
            pMet.Minimum = 0;
            if (max > 1024)
                pMet.Maximum = 32767;
            else if (max > 256)
                pMet.Maximum = 1023;
            else
                pMet.Maximum = 255;
            pMet.Resolution = float.PositiveInfinity;
            pMet.Units = Microsoft.Ink.TabletPropertyMetricUnit.Default;

            System.Guid pGuid = Microsoft.Ink.PacketProperty.NormalPressure;

            Microsoft.Ink.TabletPropertyDescription pDesc = new Microsoft.Ink.TabletPropertyDescription(pGuid, pMet);

            #endregion

            Microsoft.Ink.TabletPropertyDescriptionCollection coll = new Microsoft.Ink.TabletPropertyDescriptionCollection();
            coll.Add(xDesc);
            coll.Add(yDesc);
            coll.Add(pDesc);

            #endregion

            // Load Sketch strokes into Ink
            List<int> points = new List<int>();
            foreach (Substroke sub in Sketch.Substrokes)
            {
                points.Clear();
                foreach (Sketch.Point point in sub.Points)
                {
                    points.Add((int)point.X);
                    points.Add((int)point.Y);
                    points.Add((int)point.Pressure);
                }

                Microsoft.Ink.Stroke stroke = mInk.CreateStroke(points.ToArray(), coll);
                ink2sketchStr.Add(stroke.Id, sub.XmlAttrs.Id);
                sketchStr2ink.Add(sub.XmlAttrs.Id, stroke.Id);
                substrokeIdMap.Add(sub.XmlAttrs.Id, sub);

                stroke.DrawingAttributes.AntiAliased = true;
                stroke.DrawingAttributes.FitToCurve = true;
                stroke.DrawingAttributes.IgnorePressure = false;


                //see ReadJnt.cs for opposite transformation 
                /*
                if (sub.XmlAttrs.Time != null)
                {
                    stroke.ExtendedProperties.Add(Microsoft.Ink.StrokeProperty.TimeID,
                        (long)((ulong)sub.XmlAttrs.Time * 10000 + 116444736000000000));
                }
               */

                if (sub.XmlAttrs.Color != null) stroke.DrawingAttributes.Color = Color.FromArgb((int)sub.XmlAttrs.Color);
                if (sub.XmlAttrs.PenWidth != null) stroke.DrawingAttributes.Width = Convert.ToSingle(sub.XmlAttrs.PenWidth);
                if (sub.XmlAttrs.PenHeight != null) stroke.DrawingAttributes.Height = Convert.ToSingle(sub.XmlAttrs.PenHeight);
                if (sub.XmlAttrs.PenTip != null)
                {
                    if (sub.XmlAttrs.PenTip.Equals("Ball")) stroke.DrawingAttributes.PenTip = Microsoft.Ink.PenTip.Ball;
                    else if (sub.XmlAttrs.PenTip.Equals("Rectangle")) stroke.DrawingAttributes.PenTip = Microsoft.Ink.PenTip.Rectangle;
                }
                if (sub.XmlAttrs.Raster != null)
                {
                    if (sub.XmlAttrs.Raster.Equals("Black"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.Black;
                    else if (sub.XmlAttrs.Raster.Equals("CopyPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.CopyPen;
                    else if (sub.XmlAttrs.Raster.Equals("MakePenNot"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MakePenNot;
                    else if (sub.XmlAttrs.Raster.Equals("MaskNotPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MaskNotPen;
                    else if (sub.XmlAttrs.Raster.Equals("MaskPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MaskPen;
                    else if (sub.XmlAttrs.Raster.Equals("MergeNotPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MergeNotPen;
                    else if (sub.XmlAttrs.Raster.Equals("MergePen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MergePen;
                    else if (sub.XmlAttrs.Raster.Equals("MergePenNot"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.MergePenNot;
                    else if (sub.XmlAttrs.Raster.Equals("NoOperation"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.NoOperation;
                    else if (sub.XmlAttrs.Raster.Equals("Not"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.Not;
                    else if (sub.XmlAttrs.Raster.Equals("NotCopyPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.NotCopyPen;
                    else if (sub.XmlAttrs.Raster.Equals("NotMaskPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.NotMaskPen;
                    else if (sub.XmlAttrs.Raster.Equals("NetMergePen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.NotMergePen;
                    else if (sub.XmlAttrs.Raster.Equals("NotXOrPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.NotXOrPen;
                    else if (sub.XmlAttrs.Raster.Equals("White"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.White;
                    else if (sub.XmlAttrs.Raster.Equals("XOrPen"))
                        stroke.DrawingAttributes.RasterOperation = Microsoft.Ink.RasterOperation.XOrPen;
                }
            }


            subscribeToInk();
            if(SketchLoaded != null)
                SketchLoaded();
        }

        /// <summary>
        /// Saves this Sketch to the specified file path.
        /// </summary>
        /// <param name="filename">The path of the file to write</param>
        public void SaveSketch(string filename)
        {
            MakeXML xmlHolder = new MakeXML(Sketch);
            xmlHolder.WriteXML(filename);
        }

        /// <summary>
        /// Returns the XML representation of this sketch.
        /// </summary>
        /// <returns>The XML for this sketch</returns>
        public string GetSketchXML()
        {
            MakeXML xmlHolder = new MakeXML(Sketch);
            return xmlHolder.getXMLstr();
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
                return this.mFeatureSketch.Sketch;
            }

            set
            {
                unsubscribeToInk();
                LoadSketch(value);
                subscribeToInk();
            }
        }

        /// <summary>
        /// Gets or sets the Ink that this InkSketch wraps.
        /// Setting Ink will clear the Sketch and synchronize Sketch with Ink.
        /// </summary>
        public Microsoft.Ink.Ink Ink
        {
            get
            {
                return mInk;
            }

            set
            {
                unsubscribeToInk();
                LoadInk(value);
                subscribeToInk();
            }
        }

        /// <summary>
        /// Gets or sets the recording status of this InkSketch.  
        /// Setting this property will enable (true) or disable (false) 
        /// the synchronization of Ink changes to Sketch changes.  
        /// </summary>
        public bool Recording
        {
            get
            {
                return recording;
            }

            set
            {
                if (value)
                {
                    if (!recording)
                    {
                        subscribeToInk();
                    }
                }
                else
                {
                    if (recording)
                    {
                        unsubscribeToInk();
                    }
                }
            }
        }

        /// <summary>
        /// Returns a the sketch substroke corresponding to the given
        /// Ink stroke ID.  Returns null if no corresponding 
        /// Sketch stroke exists.
        /// </summary>
        /// <param name="id">The Id of the Ink stroke</param>
        /// <returns>The corresponding Sketch substroke</returns>
        public Substroke GetSketchSubstrokeByInkId(int id)
        {
            if (!ink2sketchStr.ContainsKey(id))
                return null;

            if (!substrokeIdMap.ContainsKey(ink2sketchStr[id]))
                return null;

            return substrokeIdMap[ink2sketchStr[id]];
        }

        /// <summary>
        /// Returns the Ink stroke corresponding to the given
        /// Sketch substroke Id.  Returns null if no corresponding
        /// Ink stroke exists.
        /// </summary>
        /// <param name="substr">The Id of the Substroke to look up</param>
        /// <returns>The corresponding Ink stroke</returns>
        public Microsoft.Ink.Stroke GetInkStrokeBySubstrokeId(Guid? substrId)
        {
            if (!sketchStr2ink.ContainsKey(substrId))
                return null;

            Microsoft.Ink.Stroke iStroke;
            using (Microsoft.Ink.Strokes strokes = mInk.Strokes)
            {
                int index = sketchStr2ink[substrId];

                try
                {
                    iStroke = strokes[index - 1];
                }
                catch (ArgumentException)
                {
                    return null;
                    // We should never in practice get here,
                    // but if we do, it means an Ink stroke 
                    // we reference is missing from mInk
                }
            }

            return iStroke;
        }

        /// <summary>
        /// Returns the Id of the Ink stroke corresponding to the given
        /// Sketch substroke Id.  Returns -1 (an invalid Ink Id) if no 
        /// corresponding Ink stroke exists.
        /// </summary>
        /// <param name="substr">The Id of the substroke to look up</param>
        /// <returns>The Id of the corresponding Ink stroke; 
        /// returns NullInkStrokeId if no such Id exists</returns>
        public int GetInkStrokeIdBySubstrokeId(Guid? substrId)
        {
            if (!sketchStr2ink.ContainsKey(substrId))
                return NullInkStrokeId;
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

        public Microsoft.Ink.Stroke GetInkStrokeById(int inkId)
        {
            foreach (Microsoft.Ink.Stroke stroke in mInk.Strokes)
            {
                if (stroke.Id == inkId)
                    return stroke;
            }

            return null;
        }

        public Dictionary<Guid?, int> SketchStr2ink
        {
            get
            {
                return sketchStr2ink;
            }
        }

        public Dictionary<int, Guid?> Ink2sketchStr
        {
            get
            {
                return ink2sketchStr;
            }
        }
        #endregion

    }
}
