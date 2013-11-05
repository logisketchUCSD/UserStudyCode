using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;


using Featurefy;
using SketchPanelLib;
using Sketch;
using CommandManagement;

namespace SelectionManager
{
    /// <summary>
    /// Delegate for handling when ink is re-labeled or re-grouped.
    /// </summary>
    public delegate void InkRerecognizedEventHandler(bool reColor);

    /// <summary>
    /// Handles learning from a corrected shape
    /// </summary>
    /// <param name="shape"></param>
    public delegate void LearningEventHandler(Sketch.Shape shape);

    /// <summary>
    /// Handles regrouping the given shapes
    /// </summary>
    /// <param name="shapes"></param>
    public delegate void RegroupShapes(List<Sketch.Shape> shapes);

    public class SelectionManager
    {

        #region Internals

        private bool debug = false;

        protected CommandManagement.CommandManager commandManager;    // Handles commands so we can redo/undo... not currently used.

        protected bool subscribed = false;              // Makes sure we do not over/under subscribe

        protected SketchPanel sketchPanel;      // SketchPanel with current drawing

        protected System.Windows.Forms.Timer hoverTimer;    // Timer for hovering

        protected const int HOVER_INTERVAL = 200;       // Time that indicates hovering

        protected const double HOVER_RADIUS = 5;           // Distance that you can move and still be 'hovering'

        protected const double MAX_LABEL_DIST = 75;

        protected System.Windows.Point hoverPoint;      // Current position of mouse in hover space

        protected System.Windows.Point startPoint;      // Starting position of hovering

        public bool widgetsShowing;                  // Whether or not the widgets are there

        public bool selectionActive;                      // If a widget is being used, ignore events

        private bool labelActive;

        private bool editActive;                      // If a widget is being used, ignore events

        private bool labelWidgetHit;

        private bool editWidgetHit;

        private const double widgetRadius = 30;             // Radius from mouse point to widgets

        private const double wiggleRoom = 10;                // Margin of error for leaving widget area

        private const double widgetHeight = 20;

        private const double labelLength = 9;

        private const double selectionLength = 6;

        private const double editLength = 5;

        private const double textSize = 8;

        // Widgets

        private Popup selectionPopup;

        private Button selectionButton;

        private Popup labelPopup;

        private Button labelButton;

        private StrokeCollection labelStrokes;

        private string prevName;

        private Popup editPopup;

        private Button editButton;

        // Widget Helpers

        public EditMenu.EditMenu editMenu;     // Menu that appears upon selection

        private SelectorTemplate selector;      // Handles adding and removing strokes from selection

        // Events
        public event InkRerecognizedEventHandler InkRerecognized;

        public event LearningEventHandler Learn;

        public event RegroupShapes GroupTogether;

        #endregion

        #region Constructor

        public SelectionManager(ref CommandManagement.CommandManager commandManager, ref SketchPanel SP)
        {
            // Set the CommandManager, DomainInfo and SketchPanel
            this.commandManager = commandManager;
            this.sketchPanel = SP;

            // Widget helpers
            this.selector = new DragBoxSelect(ref sketchPanel);
            this.editMenu = new EditMenu.EditMenu(ref this.sketchPanel, this.commandManager);
        }

        #endregion

        #region Initializers & Subscription

        private void Initialize()
        {
            sketchPanel.InkCanvas.DefaultDrawingAttributes.FitToCurve = true;

            this.hoverTimer = new System.Windows.Forms.Timer();
            this.hoverTimer.Interval = HOVER_INTERVAL;
            this.hoverTimer.Tick += new EventHandler(hoverTimer_Tick);

            MakeSelectWidget();
            MakeEditWidget();
            MakeLabelWidget();
        }

        /// <summary>
        /// Subscribes to SketchPanel.  Subscibe only when tool is selected.
        /// <see cref="SketchPanelLib.SketchPanelListener.SubscribeToPanel()"/>
        /// </summary>
        public virtual void SubscribeToPanel()
        {
            if (subscribed) return;
            subscribed = true;

            // Hook into SketchPanel selection Events
            sketchPanel.InkCanvas.SelectionChanged += new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.SelectionMoved += new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.SelectionResized += new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.StrokeCollected += new InkCanvasStrokeCollectedEventHandler(InkCanvas_StrokeCollected);

            // Stylus events
            sketchPanel.InkCanvas.StylusDown += new System.Windows.Input.StylusDownEventHandler(inkCanvas_StylusDown);
            sketchPanel.InkCanvas.StylusUp += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusUp);
            sketchPanel.InkCanvas.StylusInAirMove += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusInAirMove);
            sketchPanel.InkCanvas.StylusOutOfRange += new System.Windows.Input.StylusEventHandler(inkCanvas_StylusOutOfRange);
            sketchPanel.InkCanvas.StylusButtonDown += new StylusButtonEventHandler(InkCanvas_StylusButtonDown);
            sketchPanel.InkCanvas.StylusButtonUp += new StylusButtonEventHandler(InkCanvas_StylusButtonUp);


            this.editMenu.InkRerecognized += new EditMenu.InkRerecognizedEventHandler(editMenu_InkRerecognized);
            this.editMenu.regroup += new EditMenu.RegroupEventHandler(Regroup);
            this.editMenu.LearnFromCorrection += new EditMenu.LearningEventHandler(editMenu_LearnFromCorrection);

            // Create the timer, widgets, and components, set Selection variables
            Initialize();

            // Make sure we are in no widget mode
            selectionActive = false;
            editActive = false;
            labelActive = false;

            // Add InkCanvas children
            sketchPanel.InkCanvas.Children.Add(selectionPopup);
            sketchPanel.InkCanvas.Children.Add(editPopup);
            sketchPanel.InkCanvas.Children.Add(labelPopup);
        }

        /// <summary>
        /// Unsubscribes from SketchPanel
        /// <see cref="SketchPanelLib.SketchPanelListener.UnSubscribeToPanel()"/>
        /// </summary>
        public virtual void UnsubscribeFromPanel()
        {
            if (!subscribed || sketchPanel == null)
                return;
            subscribed = false;

            // Release SketchPanel selection Events
            sketchPanel.InkCanvas.SelectionChanged -= new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.SelectionMoved -= new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.SelectionResized -= new EventHandler(InkCanvas_SelectionChanged);
            sketchPanel.InkCanvas.StrokeCollected -= new InkCanvasStrokeCollectedEventHandler(InkCanvas_StrokeCollected);

            // Mouse and stylus events
            sketchPanel.InkCanvas.StylusDown -= new System.Windows.Input.StylusDownEventHandler(inkCanvas_StylusDown);
            sketchPanel.InkCanvas.StylusUp -= new System.Windows.Input.StylusEventHandler(inkCanvas_StylusUp);
            sketchPanel.InkCanvas.StylusInAirMove -= new System.Windows.Input.StylusEventHandler(inkCanvas_StylusInAirMove);
            sketchPanel.InkCanvas.StylusOutOfRange -= new System.Windows.Input.StylusEventHandler(inkCanvas_StylusOutOfRange);
            sketchPanel.InkCanvas.StylusButtonDown -= new StylusButtonEventHandler(InkCanvas_StylusButtonDown);
            sketchPanel.InkCanvas.StylusButtonUp -= new StylusButtonEventHandler(InkCanvas_StylusButtonUp);

            editMenu.InkRerecognized -= new EditMenu.InkRerecognizedEventHandler(editMenu_InkRerecognized);
            editMenu.regroup -= new EditMenu.RegroupEventHandler(Regroup);
            editMenu.LearnFromCorrection -= new EditMenu.LearningEventHandler(editMenu_LearnFromCorrection);

            // Remove InkCanvas children
            sketchPanel.InkCanvas.Children.Remove(selectionPopup);
            sketchPanel.InkCanvas.Children.Remove(editPopup);
            sketchPanel.InkCanvas.Children.Remove(labelPopup);
            //sketchPanel.InkCanvas.Children.Remove(editMenu.labelMenu);

            editMenu.removeMenu();

            if (editMenu.labelMenuIsOpen)
                this.editMenu.closeLabelMenu();

            // Clear selection and unsubscribe from the panel
            if (selector.subscribed)           
                selector.UnsubscribeFromPanel();

            sketchPanel.EnableDrawing();

            selectionActive = false;
        }

        #endregion

        #region Menu Items

        //Displays the context menu when called from outside explicitly
        public virtual void DisplayContextMenu(int x, int y)
        {
            editMenu.displayContextMenu(new System.Windows.Point(x, y));
        }


        //Removes context menu when called from outside explicitly
        public virtual void RemoveMenu()
        {
            editMenu.removeMenu();
        }

        #endregion

        #region Events

        /// <summary>
        /// If a stroke is accidentally collected and we are in a widget mode, we should delete it
        /// </summary>
        private void InkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            if (labelActive || editActive || selectionActive)
            {
                sketchPanel.InkSketch.DeleteStroke(e.Stroke);
                labelActive = false;
                commandManager.ClearLists();
            }
        }

        /// <summary>
        /// Moves EditMenu to appropriate location and shows it when necessary.
        /// <see cref="Labeler.LabelerPanel.sketchInk_SelectionChanged()"/>
        /// </summary>
        private void InkCanvas_SelectionChanged(object sender, EventArgs e)
        {
            if (debug) System.Console.WriteLine("Selection Changed "+ sketchPanel.InkCanvas.GetSelectedStrokes().Count);
            if (selector.Selection.Count > 0)
            {
                if (labelActive)
                    return;

                editMenu.removeMenu();

                editMenu.displayContextMenu(sketchPanel.InkCanvas.GetSelectionBounds().BottomRight);
            }
            else
            {
                editMenu.closeLabelMenu();
                editMenu.removeMenu();
                //selector.UnsubscribeFromPanel();
                selectionActive = false;
                labelActive = false;
                editActive = false;
            }
        }

        /// <summary>
        /// Occurs when the stylus button is pressed down - sets you into selection mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            if (e.StylusButton.Guid != StylusPointProperties.BarrelButton.Id)
                return;
            if (editActive)
                RemoveEditWidget();
            if (labelActive)
                RemoveLabelWidget();
            if (!selectionActive)
                SetSelectWidget();
        }

        /// <summary>
        /// Occurs when the stylus button is released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            if (e.StylusButton.Guid != StylusPointProperties.BarrelButton.Id)
                return;
            if (selector.Selection.Count == 0)
                RemoveSelectWidget();
        }

        /// <summary>
        /// Occurs when the stylus is pressed down on the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_StylusDown(object sender, StylusDownEventArgs e)
        {
            if (debug) Console.WriteLine("Stylus Down " + sketchPanel.InkCanvas.GetSelectedStrokes().Count);

            if (stylusOverWidget(e.GetPosition(sketchPanel.InkCanvas)))
                return;

            this.hoverTimer.Stop();
            if (widgetsShowing)
                HideAllWidgets();

            if (selectionActive && !editMenu.labelMenuIsOpen)
                this.selector.InkCanvas_StylusDown(sender, e);

            // Make sure we the wait cursor is turned off on the pen down
            sketchPanel.InkCanvas.UseCustomCursor = false;
        }

        /// <summary>
        /// Occurs when the stylus comes up from the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_StylusUp(object sender, StylusEventArgs e)
        {
            // Temporary hack :/
            bool justClicked = stylusOverWidget(e.GetPosition(sketchPanel.InkCanvas));
            if (widgetsShowing && justClicked)
            {
                HandleWidgetClick(e.GetPosition(sketchPanel.InkCanvas));
                return;
            }

            if (selector.makingSelection)
                selector.InkCanvas_StylusUp(sender, e);

            if (editActive && (selector.Selection.Count > 0 || !editMenu.Visible))
            {
                editActive = false;
                if (!selectionActive)
                    SetSelectWidget();
            }
            else if (selector.Selection.Count != 0 && !selectionActive && !labelActive)
                SetSelectWidget();
            else if (editActive && selector.Selection.Count == 0 && editWidgetHit)
                RemoveEditWidget();
            else if (editActive)
            {
                editWidgetHit = true;
                return;
            }
            else if (labelActive && ((!this.editMenu.labelRectangle.Contains(e.GetPosition(sketchPanel.InkCanvas))
                     && labelWidgetHit) | !this.editMenu.labelMenuIsOpen))
                RemoveLabelWidget();

            else if (labelActive)
            {
                labelWidgetHit = true;
                return;
            }
            else if (selector.Selection.Count == 0 && !justClicked)
                RemoveSelectWidget();
            else if (selectionActive)
                return;

            hoverPoint = e.GetPosition(sketchPanel.InkCanvas);
            if (hoverTimer.Enabled == false)
            {
                startPoint = e.GetPosition(sketchPanel.InkCanvas);
                hoverTimer.Start();
            }
        }

        /// <summary>
        /// Occurs when the stylus is moving in the hover space
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_StylusInAirMove(object sender, StylusEventArgs e)
        {
            if (selectionActive || editActive || labelActive)
                return;

            else if (widgetsShowing)
            {
                System.Windows.Point movePoint = e.GetPosition(sketchPanel.InkCanvas);

                double distance = Math.Sqrt(Math.Pow((movePoint.X - startPoint.X), 2) + Math.Pow((movePoint.Y - startPoint.Y), 2));

                if (distance > widgetRadius + wiggleRoom && !stylusOverWidget(movePoint))
                    HideAllWidgets();
            }

            hoverPoint = e.GetPosition(sketchPanel.InkCanvas);
            if (!hoverTimer.Enabled)
            {
                startPoint = e.GetPosition(sketchPanel.InkCanvas);
                hoverTimer.Start();
            }
        }

        /// <summary>
        /// Occurs when the stylus leaves the canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inkCanvas_StylusOutOfRange(object sender, StylusEventArgs e)
        {
            // System.Console.WriteLine("Stylus Out of Range " + sketchPanel.InkCanvas.GetSelectedStrokes().Count);
            if (selectionActive || labelActive || editActive)
                return;
            hoverTimer.Stop();
            if (widgetsShowing && !stylusOverWidget(e.GetPosition(sketchPanel.InkCanvas)))
                HideAllWidgets();
        }

        /// <summary>
        /// Occurs when the stylus has been hovering- brings up the widgets if appropriate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void hoverTimer_Tick(object sender, EventArgs e)
        {
            if (selectionActive || labelActive || editActive)
                return;
            hoverTimer.Stop();
            double distance = Math.Sqrt(Math.Pow((startPoint.X - hoverPoint.X), 2) + Math.Pow((startPoint.Y - hoverPoint.Y), 2));
            if (distance <= HOVER_RADIUS & !widgetsShowing)
                showWidgetsAtLocation(hoverPoint.X, hoverPoint.Y);
        }

        private void editMenu_InkRerecognized()
        {
            InkRerecognized(true);
        }

        private void editMenu_LearnFromCorrection(Sketch.Shape shape)
        {
            Learn(shape);
        }

        #endregion

        #region Widgets

        #region Selection Widget

        /// <summary>
        /// Constructs a new selection widget from scratch.
        /// </summary>
        private void MakeSelectWidget()
        {
            // Set Selection Widget properties
            selectionPopup = new Popup();
            selectionPopup.IsOpen = false;
            selectionPopup.Visibility = System.Windows.Visibility.Visible;
            selectionPopup.PlacementTarget = sketchPanel.InkCanvas;
            selectionPopup.Placement = PlacementMode.RelativePoint;
            selectionPopup.Height = widgetHeight;
            selectionPopup.Width = selectionLength * textSize;
            selectionPopup.AllowsTransparency = true;
            selectionButton = new Button();
            selectionButton.Content = "Select";
            selectionPopup.Child = selectionButton;
        }

        /// <summary>
        /// Brings up the selectionpopup at the specified point
        /// </summary>
        private void ShowSelectWidget(double x, double y)
        {
            if (selectionPopup.IsOpen) return;

            selectionPopup.HorizontalOffset = x - widgetHeight;
            selectionPopup.VerticalOffset = y;
            selectionPopup.IsOpen = true;
            SubscribeSelectWidget();
        }

        /// <summary>
        /// Hides and unsubscribes the selection widget
        /// </summary>
        private void HideSelectWidget()
        {
            if (!selectionPopup.IsOpen) return;

            selectionPopup.IsOpen = false;
            selectionPopup.HorizontalOffset = double.NegativeInfinity;
            selectionPopup.VerticalOffset = double.NegativeInfinity;
            UnsubscribeSelectWidget();
        }

        /// <summary>
        /// Activates selection mode through the selection widget
        /// </summary>
        /// <param name="point"></param>
        public void SetSelectWidget()
        {
            selectionActive = true;

            selector.SubscribeToPanel();

            HideAllWidgets();

            labelActive = false;
            editActive = false;
        }

        /// <summary>
        /// Deactivates selection mode through the selection widget
        /// </summary>
        /// <param name="point"></param>
        public void RemoveSelectWidget()
        {
            selectionActive = false;
            RemoveMenu();
            selector.UnsubscribeFromPanel();
            sketchPanel.EnableDrawing();
        }

        #region Selection Widget Events

        /// <summary>
        /// Sets all selection widget events
        /// </summary>
        private void SubscribeSelectWidget()
        {
            selectionPopup.StylusUp += new StylusEventHandler(selectButton_Click);
        }

        /// <summary>
        /// Removes all selection widget events
        /// </summary>
        private void UnsubscribeSelectWidget()
        {
            selectionPopup.StylusUp -= new StylusEventHandler(selectButton_Click);
        }

        /// <summary>
        /// Called when the selection widget is hit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Protect Against Double Clicking
            if (editActive || labelActive || selectionActive)
                return;
            ClearSelection();
            SetSelectWidget();
        }
        #endregion

        #endregion

        #region Edit Widget
        /// <summary>
        /// Constructs a new edit widget from scratch.
        /// </summary>
        private void MakeEditWidget()
        {
            // Set Edit Widget properties
            editPopup = new Popup();
            editPopup.IsOpen = false;
            editPopup.Visibility = System.Windows.Visibility.Visible;
            editPopup.PlacementTarget = sketchPanel.InkCanvas;
            editPopup.Placement = PlacementMode.RelativePoint;
            editPopup.Height = widgetHeight;
            editPopup.Width = textSize * editLength;
            editPopup.AllowsTransparency = true;
            editButton = new Button();
            editButton.Content = "Tools";
            editPopup.Child = editButton;
        }

        /// <summary>
        /// Brings up the labelpopup at the specified point
        /// </summary>
        private void ShowEditWidget(double x, double y)
        {
            editWidgetHit = false;
            editPopup.HorizontalOffset = x + widgetHeight / 2;
            editPopup.VerticalOffset = y - widgetRadius - widgetHeight;
            editPopup.IsOpen = true;
            SubscribeEditWidget();
        }

        private void HideEditWidget()
        {
            editPopup.IsOpen = false;
            editPopup.HorizontalOffset = double.NegativeInfinity;
            editPopup.VerticalOffset = double.NegativeInfinity;
            UnsubscribeEditWidget();
        }

        /// <summary>
        /// Activates edit mode through the edit widget
        /// </summary>
        /// <param name="point"></param>
        public void SetEditWidget(System.Windows.Point point)
        {
            editActive = true;
            sketchPanel.DisableDrawing();
            HideAllWidgets();
            this.editMenu.displayContextMenu(point);
        }

        /// <summary>
        /// Deactivates edit mode through the edit widget
        /// </summary>
        /// <param name="point"></param>
        public void RemoveEditWidget()
        {
            editActive = false;
            RemoveMenu();
            sketchPanel.EnableDrawing();
        }

        #region Edit Widget Events
        /// <summary>
        /// Sets all label widget events
        /// </summary>
        private void SubscribeEditWidget()
        {
            editPopup.StylusDown += new StylusDownEventHandler(editButton_Click);
        }

        private void UnsubscribeEditWidget()
        {
            editPopup.StylusDown -= new StylusDownEventHandler(editButton_Click);
        }

        private void editButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Protect Against Double Clicking
            if (editActive || labelActive || selectionActive)
                return;
            SetEditWidget(new System.Windows.Point(editPopup.HorizontalOffset, editPopup.VerticalOffset));
        }
        #endregion

        #endregion

        #region Label Widget
        /// <summary>
        /// Constructs a new label widget from scratch.
        /// </summary>
        private void MakeLabelWidget()
        {
            // Set Label Widget properties
            labelPopup = new Popup();
            labelPopup.IsOpen = false;
            labelPopup.PlacementTarget = sketchPanel.InkCanvas;
            labelPopup.Placement = PlacementMode.RelativePoint;
            labelPopup.Height = widgetHeight;
            labelPopup.AllowsTransparency = true;
            labelButton = new Button();
            labelPopup.Width = labelLength * textSize;
            labelPopup.Height = widgetHeight;
            prevName = new Domain.ShapeType().Name;
            labelButton.Content = prevName;
            labelPopup.Child = labelButton;
        }

        /// <summary>
        /// Brings up the labelpopup at the specified point
        /// </summary>
        private void ShowLabelWidget(double x, double y)
        {
            // Default label information
            prevName = new Domain.ShapeType().Name;
            labelButton.Content = prevName;

            // Display the name of the nearby shape on the label widget
            Shape parent = sketchPanel.InkSketch.GetSketchSubstrokeByInk(labelStrokes[0]).ParentShape;
            if (labelStrokes != null && parent != null)
            {
                labelButton.Content = parent.Type.Name;
                prevName = parent.Type.Name;
            }

            labelWidgetHit = false;
            labelPopup.HorizontalOffset = x + widgetRadius + widgetHeight + labelPopup.Width / 2;
            labelPopup.VerticalOffset = y;
            labelPopup.IsOpen = true;
            SubscribeLabelWidget();
        }

        private void HideLabelWidget()
        {
            labelPopup.IsOpen = false;
            labelPopup.HorizontalOffset = double.NegativeInfinity;
            labelPopup.VerticalOffset = double.NegativeInfinity;
            UnsubscribeLabelWidget();
        }

        /// <summary>
        /// Activates label mode through the label widget
        /// </summary>
        public void SetLabelWidget(System.Windows.Point point)
        {
            labelWidgetHit = false;
            labelActive = true;
            HideAllWidgets();

            if (labelStrokes == null)
                throw new Exception("Label button was showing when there was no strokes nearby!");

            // Select nearby strokes
            sketchPanel.InkCanvas.Select(labelStrokes);

            if (sketchPanel.InkCanvas.GetSelectedStrokes().Count == 0)
                labelActive = false;
            else
                editMenu.DisplayLabelMenu(point);
        }

        /// <summary>
        /// Deactivates label mode through the label widget
        /// </summary>
        public void RemoveLabelWidget()
        {
            labelWidgetHit = false;
            labelActive = false;
            editMenu.closeLabelMenu();
            RemoveSelectWidget(); // Unsubscribe the selector
            sketchPanel.EnableDrawing();
        }

        #region Label Widget Events
        /// <summary>
        /// Sets all label widget events
        /// </summary>
        private void SubscribeLabelWidget()
        {
            labelButton.StylusEnter += new StylusEventHandler(labelButton_StylusOverChanged);
            labelButton.StylusLeave += new StylusEventHandler(labelButton_StylusOverChanged);
            //labelPopup.StylusDown += new StylusDownEventHandler(labelButton_Click);
            //labelPopup.StylusUp += new StylusEventHandler(labelButton_Click);
            //labelButton.Click += new System.Windows.RoutedEventHandler(labelButton_Click);
            labelButton.StylusUp += new StylusEventHandler(labelButton_Click);
            //labelPopup.PreviewStylusUp += new StylusEventHandler(labelButton_Click);
        }

        /// <summary>
        /// Removes all label widget events
        /// </summary>
        private void UnsubscribeLabelWidget()
        {
            labelButton.StylusEnter -= new StylusEventHandler(labelButton_StylusOverChanged);
            labelButton.StylusLeave -= new StylusEventHandler(labelButton_StylusOverChanged);
            //labelPopup.StylusDown -= new StylusDownEventHandler(labelButton_Click);
            //labelPopup.StylusUp -= new StylusEventHandler(labelButton_Click);
            //labelButton.Click -= new System.Windows.RoutedEventHandler(labelButton_Click);
            labelButton.StylusUp -= new StylusEventHandler(labelButton_Click);
            //labelPopup.PreviewStylusUp -= new StylusEventHandler(labelButton_Click);
        }

        /// <summary>
        /// Changes the label-button content when the stylus enters or leaves its airspace.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelButton_StylusOverChanged(object sender, StylusEventArgs e)
        {
            if (labelButton.IsStylusOver)
                labelButton.Content = "Re-Label";
            else
                labelButton.Content = prevName;
        }

        private void labelButton_Click(object sender, EventArgs e)
        {
            // Protect Against Double Clicking
            if (editActive || labelActive || selectionActive)
                return;
            SetLabelWidget(new System.Windows.Point(labelPopup.HorizontalOffset - (widgetRadius + widgetHeight + labelPopup.Width / 2), labelPopup.VerticalOffset));
        }
        #endregion

        #endregion

        #region Widget Helpers

        /// <summary>
        /// Hides and unsubscribes all widgets
        /// </summary>
        public void HideAllWidgets()
        {
            if (!widgetsShowing) return;
            
            // Close widgets
            widgetsShowing = false;

            // Remove events
            HideSelectWidget();
            HideLabelWidget();
            HideEditWidget();
        }

        /// <summary>
        /// Deavtivates all widget modes, removes all actual widgets
        /// </summary>
        public void RemoveAllWidgets()
        {
            HideAllWidgets();

            RemoveSelectWidget();
            RemoveLabelWidget();
            RemoveEditWidget();
        }

        /// <summary>
        /// Closes all widgets, but leaves selection active
        /// </summary>
        public void closeAllButSelect()
        {
            HideLabelWidget();
            HideEditWidget();
            HideSelectWidget();

            labelActive = false;
            editActive = false;

            editMenu.removeMenu();
            editMenu.closeLabelMenu();
            if (sketchPanel.InkCanvas.GetSelectedStrokes().Count > 0)
                editMenu.displayContextMenu(sketchPanel.InkCanvas.GetSelectionBounds().BottomRight);
        }

        /// <summary>
        /// Returns the widget that the stylus is over.
        /// </summary>
        /// <param name="stylusPos"></param>
        /// <returns></returns>
        private bool stylusOverWidget(System.Windows.Point stylusPos)
        {
            bool value = false;
            System.Windows.Rect selectionRangeRect = new System.Windows.Rect(selectionPopup.HorizontalOffset - 5 - selectionPopup.Width, 
                selectionPopup.VerticalOffset - 5, selectionPopup.Width + 10, selectionPopup.Height + 10);
            System.Windows.Rect labelRangeRect = new System.Windows.Rect(labelPopup.HorizontalOffset - 5 - labelPopup.Width, 
                labelPopup.VerticalOffset - 5, labelPopup.Width + 10, labelPopup.Height + 10);
            System.Windows.Rect editRangeRect = new System.Windows.Rect(editPopup.HorizontalOffset - 5 - editPopup.Width, 
                editPopup.VerticalOffset - 5, editPopup.Width + 10, editPopup.Height + 10);

            if (selectionActive)
                value = false;
            else if (selectionRangeRect.Contains(stylusPos))
                value = true;
            else if (labelRangeRect.Contains(stylusPos))
                value = true;
            else if (editRangeRect.Contains(stylusPos))
                value = true;

            return value;
        }

        /// <summary>
        /// Clicks the widget under this point
        /// </summary>
        /// <param name="stylusPos"></param>
        private void HandleWidgetClick(System.Windows.Point stylusPos)
        {
            System.Windows.Rect selectionRangeRect = new System.Windows.Rect(selectionPopup.HorizontalOffset - 5 - selectionPopup.Width,
                   selectionPopup.VerticalOffset - 5, selectionPopup.Width + 10, selectionPopup.Height + 10);
            System.Windows.Rect labelRangeRect = new System.Windows.Rect(labelPopup.HorizontalOffset - 5 - labelPopup.Width,
                labelPopup.VerticalOffset - 5, labelPopup.Width + 10, labelPopup.Height + 10);
            System.Windows.Rect editRangeRect = new System.Windows.Rect(editPopup.HorizontalOffset - 5 - editPopup.Width,
                editPopup.VerticalOffset - 5, editPopup.Width + 10, editPopup.Height + 10);

            if (selectionRangeRect.Contains(stylusPos))
                selectButton_Click(null, null);
            else if (labelRangeRect.Contains(stylusPos))
                labelButton_Click(null, null);
            else if (editRangeRect.Contains(stylusPos))
                editButton_Click(null, null);
        }

        /// <summary>
        /// Displays the widget for a given point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void showWidgetsAtLocation(double x, double y)
        {
            widgetsShowing = true;

            // If needed find the nearest shape
            labelStrokes = null;
            if (sketchPanel.Recognized)
            {
                labelStrokes = closestStrokes(new System.Windows.Point(x, y));
            }

            // Open Widgets
            if (labelStrokes != null)
                ShowLabelWidget(x, y);
            ShowEditWidget(x, y);
            ShowSelectWidget(x, y);
        }

        /// <summary>
        /// Find the closest shape to a position so you can do things such as select
        /// the shape nearest to the hoverpoint and get the label of the shape for 
        /// the label hover widget.
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private StrokeCollection closestStrokes(System.Windows.Point pos)
        {
            StrokeCollection foundStrokes = new StrokeCollection();

            Substroke substroke = sketchPanel.Sketch.substrokeAtPoint(pos.X, pos.Y, 100);
            if (substroke != null && substroke.ParentShape != null)
                foreach (Substroke sub in substroke.ParentShape.Substrokes)
                    foundStrokes.Add(sketchPanel.InkSketch.GetInkStrokeBySubstroke(sub));
            else if (substroke != null)
                foundStrokes.Add(sketchPanel.InkSketch.GetInkStrokeBySubstroke(substroke));

            if (foundStrokes.Count == 0)
                return null;
            return foundStrokes;

            #region Old Inplementations
            /* 
             * This version takes the strokes from a hit test and iterates through them.
             * It finds the distance from the position to the center of the stroke bounds
             * and keeps track of the closest. The closest stroke has the parent shape
             * that is the closest Shape. Linear on number of strokes.
             */
            // the side effect required in select is to be able to select things with no
            //parent shapes, which is what noParentSubStrokes does

            /*
            noParentSubStrokes = new StrokeCollection();
            StrokeCollection hitStrokes = sketchPanel.InkCanvas.Strokes.HitTest(pos, 50.0);
            if (hitStrokes.Count == 0)
                return shape;
            else
            {
                // Find the closest stroke in the hits
                System.Windows.Ink.Stroke bestStroke = hitStrokes[0];
                double distance = System.Double.MaxValue;
                Shape inCaseNoParent = new Shape();
                foreach (System.Windows.Ink.Stroke stroke in hitStrokes)
                {
                    Sketch.Substroke substroke = sketchPanel.InkSketch.GetSketchSubstrokeByInk(stroke);
                    if (substroke.ParentShape == null)
                        noParentSubStrokes.Add(sketchPanel.InkSketch.GetInkStrokeBySubstroke(substroke));
                    else
                    {
                        System.Windows.Rect strokeBounds = stroke.GetBounds();
                        double strokeDist = Math.Sqrt(Math.Pow(((strokeBounds.X + strokeBounds.Width / 2) - hoverPoint.X), 2)
                                            + Math.Pow(((strokeBounds.Y + strokeBounds.Width / 2) - hoverPoint.Y), 2));
                        if (strokeDist < distance)
                        {
                            bestStroke = stroke;
                            distance = strokeDist;
                        }
                    }
                }

                Sketch.Substroke bestSubStroke = sketchPanel.InkSketch.GetSketchSubstrokeByInk(bestStroke);
                if (bestSubStroke.ParentShape != null)
                    shape = bestSubStroke.ParentShape;               

            }
            return shape;
            */
            /* 
             * This version takes the shapes from the Sketch and iterates through them
             * It finds the distance from the position to the center of the shape bounds
             * (with a little wiggle room so that you can be close to the shape but not inside,
             * this is motivated by the tight bounding boxes can let you be very close to a stroke
             * but not in the shape) and keeps track of the closest shape. Linear on number of shapes
             */
            /*Shape mostCentered = null;

            double bestDistToCenter = double.MaxValue;

            foreach (Shape shape in sketchPanel.Sketch.Shapes)
            {
                System.Windows.Rect boundingBox = new System.Windows.Rect(new System.Windows.Point((float)shape.Bounds.X - 10, (float)shape.Bounds.Y - 10),
                                                                new System.Windows.Point((float)shape.Bounds.BottomRight.X + 10, (float)shape.Bounds.BottomRight.Y + 10));
                if (boundingBox.Contains(pos))
                {
                    Point center = new Point((float)(boundingBox.X + boundingBox.Width / 2), (float)(boundingBox.Y + boundingBox.Height / 2));
                    double currDistToCenter = Math.Sqrt(Math.Pow(center.X - pos.X, 2) + Math.Pow(center.Y - addPoint.Y, 2));
                    if (currDistToCenter < bestDistToCenter)
                    {
                        bestDistToCenter = currDistToCenter;
                        mostCentered = shape;
                    }

                }
            }
            return mostCentered;*/
            #endregion
        }
        #endregion

        #endregion

        #region Helper Functions

        /// <summary>
        /// Makes sure that current selection contains no strokes.
        /// </summary>
        public void ClearSelection()
        {
            sketchPanel.InkCanvas.Select(new StrokeCollection());
        }

        #endregion

        #region Regrouping Events

        private void Regroup(List<Sketch.Shape> substrokes)
        {
            if (GroupTogether != null)
                GroupTogether(substrokes);
        }

        #endregion

        #region Getters and Setters

        public bool Subscribed
        {
            get
            {
                return subscribed;
            }
        }

        #endregion
    }
}
