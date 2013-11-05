using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Ink;
using System.Collections;

using System.Windows.Controls.Primitives; 
using System.Windows.Media.Animation;
using System.Threading;

using SketchPanelLib; 
using ConverterXML;
using Domain;

namespace WPFCircuitSimulatorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Front end.
    /// Contains panels, menu item callbacks, button callbacks, etc.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Internals

        /// <summary>
        /// Set to true to print debug statements
        /// </summary>
        private bool debug = false;

        /// <summary>
        /// Bool for drawing style adaptations user study.  Setting to true will record metrics and save sketches upon recognition
        /// </summary>
        private bool UserStudy = false;

        /// <summary>
        /// Bool for if you're working on embed circuits, adds in the left panel button
        /// </summary>
        private bool EmbedEnabled = false;

        #region Panels

        /// <summary>
        /// Panel for drawing the circuit schematic
        /// </summary>
        private SketchPanel circuitPanel;

        /// <summary>
        /// Panel for drawing notes
        /// </summary>
        private InkCanvas notesPanel;

        /// <summary>
        /// Recognizer for practice area
        /// </summary>
        private RecognitionInterfaces.Recognizer practiceRecognizer;

        /// <summary>
        /// Recognition pipeline for recognizing strokes on the fly.
        /// </summary>
        private RecognitionManager.RecognitionPipeline onFlyPipeline;

        /// <summary>
        /// InkCanvasSketch for the practice window.
        /// </summary>
        private InkToSketchWPF.InkCanvasSketch practiceSketch;

        /// <summary>
        /// Notes window with notesPanel
        /// </summary>
        private NotesWindow.MainWindow notesWindow;

        /// <summary>
        /// Window for practicing gate drawing
        /// </summary>
        private PracticeWindow.MainWindow practiceWindow;

        /// <summary>
        /// Window which shows recognition progress.
        /// 
        /// To update, call UpdateProgress.  Please refrain from accessing it otherwise!
        /// </summary>
        private ProgressWindow.MainWindow progressWindow;

        /// <summary>
        /// Clean circuit window with cleanPanel
        /// </summary>
        private CleanCircuitWindow.CleanWindow cleanWindow;

        /// <summary>
        /// Set to true if clean window should be in new window
        /// </summary>
        private bool cleanInNewWindow = false;

        #endregion

        #region Managers and Recognizers

        /// <summary>
        /// Recognizes circuit
        /// </summary>
        private RecognitionManager.RecognitionManager recognitionManager;

        /// <summary>
        /// Used to simulate the circuit after recognition
        /// </summary>
        private SimulationManager.SimulationManager simulationManager;

        /// <summary>
        /// Form for managing project files. Not currently used.
        /// </summary>
        //private ProjectManagerForm projectManagerForm;
        
        /// <summary>
        /// If we have already saved once, the user does not need to go through the 
        /// save window again.
        /// </summary>
        private bool firstSave = true;

        /// <summary>
        /// Indicates if the sketch has been modified since it was last saved.
        /// </summary>
        private bool dirty = false;

        /// <summary>
        /// Constant for zoom in and zoom out
        /// </summary>
        private int ZOOM_AMOUNT = 70;

        /// <summary>
        /// keep track of if the recognition of on the fly is on
        /// </summary>
        private bool onFlyTurnedOn = false;

        /// <summary>
        /// The path of the current sketch
        /// </summary>
        private string currentFilePath;

        /// <summary>
        /// Handles displaying the results on the screen.
        /// </summary>
        private DisplayManager.DisplayManager displayManager;

        /// <summary>
        /// Selection tool so that we actually get the power of the labeler
        /// </summary>
        private SelectionManager.SelectionManager selectionTool;

        /// <summary>
        /// Command Manager for providing Undo/Redo
        /// </summary>
        private CommandManagement.CommandManager commandManager;

        #endregion

        #region Status Bar

        public delegate void doNextStep();

        public enum Status
        {
            /// <summary>
            /// UI is idle
            /// </summary>
            Idle,

            /// <summary>
            /// UI is currently running a recognition process
            /// </summary>
            Recognizing,

            /// <summary>
            /// UI is currently simulating the circuit
            /// </summary>
            Simulating,

            /// <summary>
            /// UI is currently loading a sketch
            /// </summary>
            Loading,
        }

        /// <summary>
        /// The current status of the system. 
        /// Access through CurrentStatus
        /// </summary>
        private Status currentStatus;

        /// <summary>
        /// Use this to access currentStatus.  Sets the status strip label.
        /// </summary>
        private Status CurrentStatus
        {
            get
            {
                return currentStatus;
            }
            set
            {
                currentStatus = value;
                if (currentStatus == Status.Idle)
                    statusStripLabel.Content = "Idle.";
                else if (currentStatus == Status.Recognizing)
                    statusStripLabel.Content = "Recognizing...";
                else if (currentStatus == Status.Simulating)
                    statusStripLabel.Content = "Simulating...";
                else if (currentStatus == Status.Loading)
                    statusStripLabel.Content = "Loading...";
            }
        }

        #endregion

        #region Simulation Modes
        /// <summary>
        /// Indicates when strokes have been changed and it needs to be re-recognized.
        /// </summary>
        private bool needsRerec = true;

        /// <summary>
        /// Indicates when the sketch has been changed and it needs to be re-simulated.
        /// OR when the circuit parser failed? (If you know, change this. Fiona 24-Jun-2011
        /// </summary>
        private bool needsResim = true;

        #endregion

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Constructor, initializes the window
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.StackTrace);
            }

            try
            {
                InitializeEverything();
            }
            catch (Exception e)
            {
                // This message box closes immediately when it is shown.  If you're bored, fix it!
                MessageBoxResult result = MessageBox.Show("Exception " + e);//"The program failed to open. Try running as an administrator.\n\n" +
                    //"If that doesn't work, some files may be missing, and you should try reinstalling the program.", "Oh Noes!", MessageBoxButton.OK);
                //Console.WriteLine("Exception " + e);
                //Close();
            }
        }

        /// <summary>
        /// Initializes everything that the main window uses.  Needed for the constructor and newSketch
        /// </summary>
        private void InitializeEverything()
        {
            // Set up sketch panel
            commandManager = new CommandManagement.CommandManager(new Func<bool>(commandManager_updateEditMenu), new Func<bool>(commandManager_moveUpdate), UserStudy);
            circuitPanel = new SketchPanel(commandManager);
            notesPanel = new InkCanvas();

            // Add panels and configure layout
            circuitDock.Children.Clear();
            circuitDock.Children.Add(circuitPanel);

            circuitPanel.Name = "Circuit";
            circuitPanel.Height = circuitDock.ActualHeight;
            circuitPanel.Width = circuitDock.ActualWidth;

            // Add recognizers
            if (debug) Console.WriteLine("RecognitionManager");
            recognitionManager = new RecognitionManager.RecognitionManager(circuitPanel);
            onFlyPipeline = RecognitionManager.RecognitionPipeline.createOnFlyPipeline();

            // Set up Project File Manager (not currently used)
            //if (debug) Console.WriteLine("ProjectManager");
            //projectManagerForm = new ProjectManagerForm();

            // Set up correction tool
            if (debug) Console.WriteLine("Correction Tools");
            selectionTool = new SelectionManager.SelectionManager(ref commandManager, ref circuitPanel);
            displayManager = new DisplayManager.DisplayManager(ref circuitPanel, FilenameConstants.DefaultCircuitDomainFilePath, endPointChange);

            // Set project name
            this.Title = FilenameConstants.ProgramName + " - New Project";

            // Set up the SimulationManager
            if (debug) Console.WriteLine("SimulationManager");
            simulationManager = new SimulationManager.SimulationManager(ref circuitPanel);

            SubscribeEverything();
            commandManager_updateEditMenu();

            #region Practice Window Stuff
            InkCanvas practiceCanvas = new InkCanvas();
            practiceSketch = new InkToSketchWPF.InkCanvasSketch(practiceCanvas);

            // Set Editing Modes
            practiceCanvas.EditingMode = InkCanvasEditingMode.Ink;
            practiceCanvas.EditingModeInverted = InkCanvasEditingMode.EraseByStroke;

            // Set up recognizer
            practiceRecognizer = RecognitionManager.RecognitionPipeline.createDefaultRecognizer();
            #endregion

            // Set up user study things
            if (UserStudy == true)
            {
                userStudyOption.IsChecked = true;
                UserStudyOn();
            }

            onFlyTurnedOn = (bool)shapesWhileDrawing.IsChecked;

            // Set current status
            CurrentStatus = Status.Idle;
            circuitPanel.EnableDrawing();
        }

        #endregion 

        #region Subscription

        /// <summary>
        /// Handles main window subscription
        /// </summary>
        private void SubscribeEverything()
        {
            // Set up highlighting-of-current-panel feature (We only have one panel, so not used)
            //mainMenu.MouseEnter += new MouseEventHandler(menu_MouseEnter);
            //circuitPanel.InkCanvas.MouseEnter += new MouseEventHandler(sketchArea_MouseEnter);

            // Gesture Recognition
            circuitPanel.InkCanvas.SetEnabledGestures(new ApplicationGesture[]
                { //ApplicationGesture.Triangle,
                //ApplicationGesture.Curlicue,
                ApplicationGesture.ScratchOut,
                ApplicationGesture.Exclamation });
            circuitPanel.InkCanvas.Gesture += new InkCanvasGestureEventHandler(GestureHandler);

            // Configure recognizers
            circuitPanel.ResultReceived += new RecognitionResultReceivedHandler(circuitPanel_ResultReceived);
            circuitPanel.SketchFileLoaded += new SketchFileLoadedHandler(circuitPanel_SketchFileLoaded);

            // Selection tool
            selectionTool.SubscribeToPanel();

            // Make sure menu cut and copy are enabled at the right times
            circuitPanel.InkCanvas.SelectionChanged += new EventHandler(InkCanvas_SelectionChanged);
            commandManager.Clear();
            enableEditing();

            // Set up mode indicator
            circuitPanel.InkCanvas.ActiveEditingModeChanged += new RoutedEventHandler(InkCanvas_EditingModeChanged);
            modeIndicator.Text = "Current Mode: Ink";

            // Subscribe opening and saving (project manager not currently used)
            //projectManagerForm.ProjectSaving += new ProjectSavingHandler(saveProjectFiles);
            //projectManagerForm.ProjectOpening += new ProjectOpeningHandler(openFiles);

            // Set up so we know when we need to re-recognize and re-build the circuit
            selectionTool.InkRerecognized += new SelectionManager.InkRerecognizedEventHandler(InkChanged);
            selectionTool.GroupTogether += new SelectionManager.RegroupShapes(selectionTool_GroupTogether);
            selectionTool.Learn += new SelectionManager.LearningEventHandler(selectionTool_Learn);
            circuitPanel.InkChanged += new InkChangedEventHandler(InkChanged);
            //circuitPanel.drawGates += new InkChangedOnFlyRecHandler(drawGates);


            //circuitPanel.InkSketch.OnFlyRecognition += new InkToSketchWPF.OnFlyRecognitionHandler(circuitPanel_OnFlyRecognition);

            // Change the endpoints when they need to be
            displayManager.EndpointsChanged += new DisplayManager.EndpointsChangedHandler(RecognitionChanged);

            // Set the content of the recognize button based on our simulation mode
            recognizeButton.Content = "Recognize";
            simulationButton.Content = "Simulate";

            recognizeButton.Checked += new RoutedEventHandler(recognizeButton_Click);
            simulationButton.Checked += new RoutedEventHandler(delayedSimButton_Click);
            if (EmbedEnabled)
            {
                embedButton.Visibility = Visibility.Visible;
            }
            else
            {
                embedButton.Visibility = Visibility.Hidden;
            }
            subCircuitSave.Height = 0;
            subCircuitSave.Visibility = Visibility.Hidden;


        }

        /// <summary>
        /// Resets main window by unsubscribing event handlers and clearing command list
        /// </summary>
        private void UnsubscribeEverything()
        {
            // Remove highlighting-of-current-panel feature (We only have one panel, so not used)
            //mainMenu.MouseEnter -= new MouseEventHandler(menu_MouseEnter);
            //circuitPanel.InkCanvas.MouseEnter -= new MouseEventHandler(sketchArea_MouseEnter);

            // Gesture Recognition
            circuitPanel.InkCanvas.Gesture -= new InkCanvasGestureEventHandler(GestureHandler);

            // De-configure recognizers
            circuitPanel.ResultReceived -= new RecognitionResultReceivedHandler(circuitPanel_ResultReceived);
            circuitPanel.SketchFileLoaded -= new SketchFileLoadedHandler(circuitPanel_SketchFileLoaded);

            // Unsubscribe feedback and tools
            displayManager.UnsubscribeFromPanel();
            displayManager.EndpointsChanged -= new DisplayManager.EndpointsChangedHandler(RecognitionChanged);
            selectionTool.UnsubscribeFromPanel();
            simulationManager.UnsubscribeFromPanel();

            // Make sure menu cut and copy are enabled at the right times
            circuitPanel.InkCanvas.SelectionChanged -= new EventHandler(InkCanvas_SelectionChanged);

            // Remove mode indicator event
            circuitPanel.InkCanvas.ActiveEditingModeChanged -= new RoutedEventHandler(InkCanvas_EditingModeChanged);

            // Remove project manager events (project manager not currently used)
            //projectManagerForm.ProjectSaving -= new ProjectSavingHandler(saveProjectFiles);
            //projectManagerForm.ProjectOpening -= new ProjectOpeningHandler(openFiles);

            // Remove ink and recognition change evnts
            selectionTool.InkRerecognized -= new SelectionManager.InkRerecognizedEventHandler(InkChanged);
            selectionTool.GroupTogether -= new SelectionManager.RegroupShapes(selectionTool_GroupTogether);
            selectionTool.Learn -= new SelectionManager.LearningEventHandler(selectionTool_Learn);
            circuitPanel.InkChanged -= new InkChangedEventHandler(InkChanged);
            //circuitPanel.drawGates -= new InkChangedOnFlyRecHandler(drawGates);

            //circuitPanel.InkSketch.OnFlyRecognition -= new InkToSketchWPF.OnFlyRecognitionHandler(circuitPanel_OnFlyRecognition);

            // Unsubscribe simulaion/recognition buttons
            recognizeButton.Checked -= new RoutedEventHandler(recognizeButton_Click);
            simulationButton.Checked -= new RoutedEventHandler(delayedSimButton_Click);
        }

        #endregion

        #region CLEAR

        /// <summary>
        /// Resets the state of the GUI to the "no project loaded" state
        /// </summary>
        public void Clear()
        {
            SimToEdit();

            simulationButton.Visibility = System.Windows.Visibility.Hidden;
            notesPanel.Strokes.Clear();
            displayManager.RemoveFeedbacks();

            circuitPanel.SimpleDeleteAllStrokes();
            commandManager.Clear();
            enableEditing();
            circuitPanel.EnableDrawing();

            // Set project name
            this.Title = FilenameConstants.ProgramName + " - New Project";

            // Reset some variables
            needsRerec = true;
            needsResim = true;
            firstSave = true;
            dirty = false;
        }

        #endregion

        #region Main Window Changing Events

        /// <summary>
        /// When the window is resized, we also resize the sketchPanels.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_SizeChanged(object sender, RoutedEventArgs e)
        {
            circuitPanel.Height = circuitDock.ActualHeight;
            circuitPanel.Width = circuitDock.ActualWidth;
            if (simulationManager.DisplayingClean)
            {
                simulationManager.CleanCircuit.Height = circuitDock.ActualHeight;
                simulationManager.CleanCircuit.Width = circuitDock.ActualWidth;
            }

            selectionTool.RemoveAllWidgets();

            displayManager.RefreshFeedbacks();

            if (simulationManager.Subscribed)
            {
                simulationManager.MakeNewToggles();
                simulationManager.ShowToggles();
            }
        }

        /// <summary>
        /// When the window is minimized or resized, the popups should be closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    if (simulationManager.Subscribed)
                    {
                        simulationManager.MakeNewToggles();
                        simulationManager.ShowToggles();
                    }
                    displayManager.RefreshFeedbacks();
                    break;
                case WindowState.Minimized:
                    if (selectionTool.Subscribed)
                        selectionTool.RemoveAllWidgets();
                    if (simulationManager.Subscribed)
                        simulationManager.HideToggles();
                    displayManager.RemoveFeedbacks();
                    break;
                case WindowState.Normal:
                    if (simulationManager.Subscribed)
                        simulationManager.ShowToggles();
                    displayManager.RefreshFeedbacks();
                    break;
            }
        }

        
        /// <summary>
        /// Changes the mode indicator to reflect the current editing mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_EditingModeChanged(object sender, RoutedEventArgs e)
        {
            InkCanvasEditingMode mode = circuitPanel.InkCanvas.ActiveEditingMode;

            if (mode == InkCanvasEditingMode.InkAndGesture ||
                mode == InkCanvasEditingMode.Ink)
                modeIndicator.Text = "Current Mode: Ink";
            else if (mode == InkCanvasEditingMode.Select ||
                mode == InkCanvasEditingMode.None)
                modeIndicator.Text = "Current Mode: Selection";
            else if (mode == InkCanvasEditingMode.EraseByStroke ||
                mode == InkCanvasEditingMode.EraseByPoint)
                modeIndicator.Text = "Current Mode: Erasing";
            else
                modeIndicator.Text = "Current Mode: Unknown";
        }

        /// <summary>
        /// When the window is deactivated, we want to close all the input/output toggles and take away other popups
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Deactivated(object sender, EventArgs e)
        {
            selectionTool.RemoveAllWidgets();

            if (simulationManager != null && simulationManager.Subscribed)
                simulationManager.HideToggles();

            displayManager.RemoveFeedbacks();
        }

        /// <summary>
        /// When the window is activated, we want to bring back the input/output toggles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainWindow_Activated(object sender, EventArgs e)
        {
            if (simulationManager != null && simulationManager.Subscribed)
                simulationManager.ShowToggles();
            displayManager.RefreshFeedbacks();
        }
        
        #endregion

        #region Callbacks

        #region Project File I/O Callbacks

        /// <summary>
        /// Open the xml file
        /// </summary>
        private void openFiles()
        {
            // Set status
            CurrentStatus = Status.Loading;

            // Check to see if our file is a valid type
            if (!currentFilePath.Contains(FilenameConstants.DefaultXMLExtension))
            {
                MessageBox.Show(this, "The chosen file is not valid.", "Invalid File");
                return;
            }

            Wait();

            // Clear the GUI
            Clear();

            // Load the sketch and recolor it
            circuitPanel.LoadSketch(currentFilePath);

            recognitionManager.ConnectSketch();
            //recognizeCircuit(false);

            this.Title = FilenameConstants.ProgramName + " - " + System.IO.Path.GetFileNameWithoutExtension(currentFilePath);

            firstSave = false;
            dirty = false;

            CurrentStatus = Status.Idle;

            EndWait();
        }

        /// <summary>
        /// Saves all the project files
        /// 
        /// Precondition: all project file paths are valid and exist.
        /// </summary>
        private void saveProjectFiles()
        {
            Wait();

            // Make sure project directory exists.
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(currentFilePath)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(currentFilePath));
            }

            // We are not currently using the project manager
            //String sketchPath = projectManagerForm.CurrentProjectFileBase;

            circuitPanel.SaveSketch(currentFilePath);

            firstSave = false;
            dirty = false;

            this.Title = FilenameConstants.ProgramName + " - " + System.IO.Path.GetFileNameWithoutExtension(currentFilePath);

            EndWait();
        }

        /// <summary>
        /// Exports circuit to .circ format, compatible wih Logisim
        /// </summary>
        /// <param name="fileName"></param>
        private void exportToLogisim(string fileName)
        {
            Wait();

            // Make sure project directory exists.
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
            }

            circuitPanel.ExportSketch(fileName);

            EndWait();
        }

        /// <summary>
        /// Creates a new project using the project file manager
        /// </summary>
        private void menuNewSketch_Click(object sender, RoutedEventArgs e)
        {
            bool saved = true;

            if (dirty)
            {
                MessageBoxResult result = MessageBox.Show(this, "Sketch has not been saved. Do you want to save?",
                    "Warning: Unsaved Sketch", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                    saved = menuSaveSketch(sender, e);
                else if (result == MessageBoxResult.Cancel)
                    return;
            }

            Wait();

            if (saved)
            {
                currentFilePath = null;
                Clear();
            }

            EndWait();
        }

        /// <summary>
        /// Exits the application.  Prompts the user to save the project if the project is currently
        /// unsaved.
        /// </summary>
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (recognitionManager != null)
                recognitionManager.Recognizer.save();
            closeProgressWindow();
            closePractice(null, null);
            closeNotes(null, null);
            if (!checkSave())
                e.Cancel = true;
            else
                System.Environment.Exit(0); // kills all background threads too
        }
        /// <summary>
        /// Handler for the embed button to display the menu for embeding circuits
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void embedButton_Click(object sender, RoutedEventArgs e)
        {
            layer1.Visibility = Visibility.Visible;

            // Adjust z order to ensure the pane is on top:

            Grid.SetZIndex(layer1, 1);
            Canvas.SetZIndex(circuitPanel, 1);

            DoubleAnimation animate = new DoubleAnimation();
            animate.From = 0;
            animate.To = 220;
            animate.Duration = TimeSpan.FromMilliseconds(250);
            ActionPanel.BeginAnimation(Border.WidthProperty, animate);

        }
        private void closeEmbed_Click(object sender, RoutedEventArgs e)
        {
            // Adjust z order to ensure the pane is on top:

            Grid.SetZIndex(layer1, 1);
            Canvas.SetZIndex(circuitPanel, 1);

            DoubleAnimation animate = new DoubleAnimation();
            animate.From = 220;
            animate.To = 0;
            animate.Duration = TimeSpan.FromMilliseconds(250);
            ActionPanel.BeginAnimation(Border.WidthProperty, animate);
        }
        /// <summary>
        /// Asks if the user wants to save their sketch
        /// </summary>
        /// <returns>true if the user makes a choice, false if the user cancels</returns>
        private bool checkSave()
        {
            if (dirty && !UserStudy)
            {
                string message = "Would you like to save your sketch before exiting?";
                string title = "Confirm Application Exit";
                System.Windows.MessageBoxButton buttons = MessageBoxButton.YesNoCancel;
                MessageBoxResult result = MessageBox.Show(this, message, title, buttons);

                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
                else if (result == MessageBoxResult.Yes)
                {
                    menuSaveSketch_Click(null, null);
                }
            }
            return true;
        }

        /// <summary>
        /// Displays a help window with instructions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        private void menuLoadSketch_Click(object sender, RoutedEventArgs e)
        {
            // If the user cancels loading the sketch, abort loading
            if (!checkSave())
                return;

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            openFileDialog.Title = "Load a Sketch";
            openFileDialog.Filter = "MIT XML sketches (*.xml)|*.xml";
            openFileDialog.RestoreDirectory = false;

            if (openFileDialog.ShowDialog() == true)
            {
                if (!System.IO.File.Exists(openFileDialog.FileName))
                {
                    MessageBox.Show(this, "Target file does not exist", "Error");
                }
                else if (!openFileDialog.FileName.Contains(FilenameConstants.DefaultXMLExtension))
                {
                    MessageBox.Show(this, "Filename is not valid. Sketch must be saved as an XML.", "Invalid");
                }
                else
                {
                    currentFilePath = openFileDialog.FileName;

                    openFiles();
                }
            }
        }

        private void menuSaveSketch_Click(object sender, RoutedEventArgs e)
        {
            menuSaveSketch(sender, e);
        }

        private bool menuSaveSketch(object sender, RoutedEventArgs e)
        {
            if (!firstSave)
            {
                saveProjectFiles();
                return true;
            }
            else
                return menuSaveSketchAs(sender, e);
        }

        private void menuSaveSketchAs_Click(object sender, RoutedEventArgs e)
        {
            menuSaveSketchAs(sender, e);
        }

        private void menuSaveSubCircuit_Click(object sender, RoutedEventArgs e)
        {
            //TODO: create saving for subcircuits
            return;
        }

        private bool menuSaveSketchAs(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.Filter = "MIT XML Files (*.xml)|*.xml"; //Alternate filetype, not currently supported "Canonical Example (*.cxtd)|*.cxtd";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = false;

            if (saveFileDialog.ShowDialog() == true)
            {
                if (!saveFileDialog.FileName.Contains(FilenameConstants.DefaultXMLExtension))
                {
                    MessageBox.Show(this, "Filename is not valid. Sketch must be saved as an XML.", "Invalid");
                    return false;
                }
                else
                {
                    currentFilePath = saveFileDialog.FileName;
                    saveProjectFiles();
                    return true;
                }
            }
            return false;
        }

        private void menuExportToLogisim_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            saveFileDialog.Filter = "Logisim Files (*.circ)|*.circ";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = false;

            if (saveFileDialog.ShowDialog() == true)
            {
                if (!saveFileDialog.FileName.Contains(FilenameConstants.DefaultLogisimExtension))
                {
                    MessageBox.Show(this, "Filename is not valid. Sketch must be saved as .circ.", "Invalid");
                    return;
                }
                else
                {
                    exportToLogisim(saveFileDialog.FileName);
                }
            }
        }

        #region Unused Menu Options

        /// <summary>
        /// Opens a project using the project file manager.
        /// </summary>
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            //projectManagerForm.LaunchProjectManager(ProjectManagerForm.ProjectManagerContext.Open);
        }


        /// <summary>
        /// Allows the user to edit the current project files with the project file manager
        /// </summary>
        private void menuRenameProject_Click(object sender, RoutedEventArgs e)
        {
            //projectManagerForm.LaunchProjectManager(ProjectManagerForm.ProjectManagerContext.Edit);
        }


        /// <summary>
        /// Saves the current project using the project file manager
        /// </summary>
        private void menuSave_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (firstSave || !projectManagerForm.ValidateFilepaths(false) ||
                !projectManagerForm.ValidateFilesExist(false))
            {
                bool? result =
                    projectManagerForm.LaunchProjectManager(ProjectManagerForm.ProjectManagerContext.Save);

                if (result != true)
                    return;
            }
            else
            {
                // Write out files
                saveProjectFiles();
            }
         */
        }



        private void menuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            //projectManagerForm.LaunchProjectManager(ProjectManagerForm.ProjectManagerContext.Save);
        }



        private void menuConnect_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog= new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }

            System.Windows.Forms.FolderBrowserDialog targetFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            targetFolderDialog.Description = "Choose the target directory";

            if (targetFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(targetFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.labelData(sourceFolderDialog.SelectedPath, targetFolderDialog.SelectedPath);*/
        }

        private void menuClassify_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }

            System.Windows.Forms.FolderBrowserDialog targetFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            targetFolderDialog.Description = "Choose the target directory";

            if (targetFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(targetFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.classifyData(sourceFolderDialog.SelectedPath, targetFolderDialog.SelectedPath);*/
        }

        private void menuTestGrouper_Click(object sender, RoutedEventArgs e)
        {
            /* System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.testGrouper(sourceFolderDialog.SelectedPath); */
        }

        private void menuTestRecog_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.testRecognizer(sourceFolderDialog.SelectedPath);*/
        }

        private void menuTestClassifier_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.testClassifier(sourceFolderDialog.SelectedPath);*/
        }

        private void menuTestOverall_Click(object sender, RoutedEventArgs e)
        {
            /* System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.testOverallRecognition(sourceFolderDialog.SelectedPath);*/
        }

        private void menuTestRefiner_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.testRefiner(sourceFolderDialog.SelectedPath); */
        }

        private void menuProccess_Click(object sender, RoutedEventArgs e)
        {
            /*System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }

            System.Windows.Forms.FolderBrowserDialog targetFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            targetFolderDialog.Description = "Choose the target directory";

            if (targetFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(targetFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.proccessData(sourceFolderDialog.SelectedPath, targetFolderDialog.SelectedPath); */
        }

        private void menuClean_Click(object sender, EventArgs e)
        {
            /* System.Windows.Forms.FolderBrowserDialog sourceFolderDialog = new System.Windows.Forms.FolderBrowserDialog();

            sourceFolderDialog.Description = "Choose the source directory";

            if (sourceFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.Directory.Exists(sourceFolderDialog.SelectedPath))
                {
                    MessageBox.Show("Error: target folder does not exist");
                }
            }
            recognitionManager.cleanData(sourceFolderDialog.SelectedPath); */
        }

        #endregion

        #endregion

        #region Toolbar Button Callbacks

        /// <summary>
        /// Triggers CircuitParser Recognition on circuit panel
        /// </summary>
        private void recognizeCircuitButton_Click(object sender, RoutedEventArgs e)
        {
            recognizeCircuit(true);
            displayManager.ColorStrokesByType();
        }

        /// <summary>
        /// Triggers label recognition on the panel.
        /// </summary
        private void recognizeInkButton_Click(object sender, RoutedEventArgs e)
        {
            recognizeSketch();
            displayManager.ColorStrokesByType();
        }

        private void refineButton_Click(object sender, RoutedEventArgs e)
        {
            recognitionManager.RefineSketch();
            displayManager.ColorStrokesByType();
        }

        /// <summary>
        /// Trigers single-stroke classification on the panel.
        /// </summary>
        private void classifyButton_Click(object sender, RoutedEventArgs e)
        {
            classifyStrokes();
            displayManager.displayClassification();
        }

        /// <summary>
        /// Triggers the connection recognition on the panel.
        /// </summary>
        private void connectShapesButton_Click(object sender, RoutedEventArgs e)
        {
            connectSketch();
            displayManager.ColorStrokesByType();
        }

        /// <summary>
        /// Triggers stroke grouping on the panel.
        /// </summary>
        private void groupStrokesButton_Click(object sender, RoutedEventArgs e)
        {
            groupStrokes();
            displayManager.displayGroups();
        }

        #region Tools

        /// <summary>
        /// Calls appropriate gesture event
        /// </summary>
        private void GestureHandler(object sender, InkCanvasGestureEventArgs e)
        {

            // recognize the stroke as some gestures, if any
            System.Collections.ObjectModel.ReadOnlyCollection<GestureRecognitionResult> gestureResults = e.GetGestureRecognitionResults();

            // Perform various actions depending on the recognized gesture
            switch (gestureResults[0].ApplicationGesture)
            {
                case ApplicationGesture.Curlicue: // Switch between ink and selection modes
                    Curlicue(sender, e);
                    System.Console.WriteLine("Curlicue");
                    break;
                case ApplicationGesture.ScratchOut: // Delete scratched out strokes
                    Scratchout(sender, e);
                    System.Console.WriteLine("Scratchout");
                    break;
                case ApplicationGesture.Exclamation:   // Draw a triangle on the canvas
                    Exclamation(sender, e);
                    System.Console.WriteLine("Exclamation");
                    break;
                case ApplicationGesture.Triangle:   // Draw a triangle on the canvas
                    System.Console.WriteLine("Triangle");
                    selectionTool.SetEditWidget(new System.Windows.Point(e.Strokes.GetBounds().X, e.Strokes.GetBounds().Y));
                    break;
                default:    // Do nothing if no desired gesture is recognized
                    break;
            }
        }


        /// <summary> 
        /// Handles switching between Ink and Edit mode when a curlicue is drawn
        /// </summary>
        private void Curlicue(object sender, InkCanvasGestureEventArgs e)
        {
            // For now do nothing
        }

        /// <summary>
        /// Handles deleting objects that are scratched out
        /// </summary>
        private void Scratchout(object sender, InkCanvasGestureEventArgs e)
        {
            // Don't know if this works
            // Deletes any strokes that are 50% contained within the scribble's bounding box
            // Probably not ideal method
            Rect rect = e.Strokes.GetBounds();

            StrokeCollection strokesHitTest = circuitPanel.InkCanvas.Strokes.HitTest(rect, 50);
            circuitPanel.InkCanvas.Strokes.Remove(strokesHitTest);
        }

        /// <summary>
        /// Brings up the edit menu when an exclamation mark is drawn
        /// </summary>
        private void Exclamation(object sender, InkCanvasGestureEventArgs e)
        {
            System.Windows.Rect bounds = e.Strokes.GetBounds();
            selectionTool.SetEditWidget(new System.Windows.Point(bounds.X, bounds.Y));
        }

        #endregion

        #endregion

        #region Edit Menu Callbacks

        /// <summary>
        /// If nothing is selected, cut and copy should not be enabled on the edit menu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InkCanvas_SelectionChanged(object sender, EventArgs e)
        {
            if (circuitPanel.InkCanvas.GetSelectedStrokes().Count == 0)
            {
                menuCut.IsEnabled = false;
                menuCopy.IsEnabled = false;
                menuDelete.IsEnabled = false;
            }
            else
            {
                menuCut.IsEnabled = true;
                menuCopy.IsEnabled = true;
                menuDelete.IsEnabled = true;
            }
        }

        /// <summary>
        /// Copies selection of Sketch in focus
        /// </summary>
        private void menuCopy_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.CopyStrokes();

            // Keep selection widget open even after copying
            // selectionTool.RemoveSelectWidget();
        }

        /// <summary>
        /// Cuts selection of Sketch in focus
        /// </summary>
        private void menuCut_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.CutStrokes();

            selectionTool.RemoveSelectWidget();
        }

        /// <summary>
        /// Pastes to selection of Sketch in focus
        /// </summary>
        private void menuPaste_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.PasteStrokes(new System.Windows.Point(-1, -1));

            selectionTool.SetSelectWidget();
        }

        /// <summary>
        /// Deletes selection of Sketch in focus
        /// </summary>
        private void menuDelete_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.DeleteStrokes();

            selectionTool.RemoveSelectWidget();
        }

        /// <summary>
        /// Undoes the last command
        /// </summary>
        private void menuUndo_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.Undo();

            if (currentFocusPanel.InkCanvas.GetSelectedStrokes().Count > 0)
                selectionTool.SetSelectWidget();
        }

        /// <summary>
        /// Redoes the last command
        /// </summary>
        private void menuRedo_Click(object sender, EventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.Redo();

            if (currentFocusPanel.InkCanvas.GetSelectedStrokes().Count > 0)
                selectionTool.SetSelectWidget();
        }

        /// <summary>
        /// Selects all of the strokes
        /// </summary>
        private void menuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.SelectAllStrokes();

            if (currentFocusPanel.InkCanvas.GetSelectedStrokes().Count > 0)
                selectionTool.SetSelectWidget();
        }

        /// <summary>
        /// Deletes all of the panels' strokes
        /// </summary>
        private void menuDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            circuitPanel.DeleteAllStrokes();

            selectionTool.RemoveSelectWidget();
        }

        /// <summary>
        /// Zooms in
        /// </summary>
        private void menuZoomIn_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            InkChanged(false);
            currentFocusPanel.ZoomIn();
         }

        /// <summary>
        /// Zooms out
        /// </summary>
        private void menuZoomOut_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();
            currentFocusPanel.ZoomOut(ZOOM_AMOUNT);
        }

        /// <summary>
        /// Displays a color dialog to change the default ink color
        /// </summary>
        private void menuSelectInkColor_Click(object sender, RoutedEventArgs e)
        {
            SketchPanel currentFocusPanel = getLastActivePanel();

            System.Windows.Forms.ColorDialog colorDlg = new System.Windows.Forms.ColorDialog();
            colorDlg.AnyColor = true;
            colorDlg.ShowHelp = true;

            if (colorDlg.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                Color newColor = new Color();
                newColor.A = colorDlg.Color.A;
                newColor.B = colorDlg.Color.B;
                newColor.G = colorDlg.Color.G;
                newColor.R = colorDlg.Color.R;
                currentFocusPanel.InkCanvas.DefaultDrawingAttributes.Color = newColor;
            }
        }

        /// <summary>
        /// When clicked, takes us out of simulation mode and back into edit mode.
        /// </summary>
        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton editButton = (ToggleButton)sender;


            editButton.IsChecked = false;
            SimToEdit();
        }



        #endregion

        #region View Menu Callbacks

        private void menuShowClassification_Click(object sender, RoutedEventArgs e)
        {
            displayManager.displayClassification();
        }

        private void menuGroups_Click(object sender, RoutedEventArgs e)
        {
            displayManager.displayGroups();
        }

        private void menuLabels_Click(object sender, RoutedEventArgs e)
        {
            displayManager.ColorStrokesByType();
        }

        private void menuAdjacencyMesh_Click(object sender, RoutedEventArgs e)
        {
            displayManager.displayAdjacency();
        }

        private void menuValidity_Click(object sender, RoutedEventArgs e)
        {
            displayManager.displayValidity(recognitionManager.TestValidity());
        }

        private void menuPoints_Click(object sender, RoutedEventArgs e)
        {
            displayManager.showPoints();
        }

        private void menuProcess_Click(object sender, RoutedEventArgs e)
        {
            // We don't have anything here yet...
        }

        #endregion

        #region Feedback Menu Callbacks

        private void labelAfterRec_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.RecognitionTooltips = (bool)labelsAfterRec.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shapesAfterRec_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.GatesOnRec = (bool)shapesAfterRec.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highlightAfterRec_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.ErrorHighlighting = (bool)HighlightAfterRec.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shapesAfterLabel_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.GatesOnLabel = (bool)shapesAfterLabel.IsChecked;
        }


        private void shapesOnHover_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.GatesOnHovering = (bool)shapesOnHover.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shapesWhileDrawing_Click(object sender, RoutedEventArgs e)
        {
            // TODO make this finish loading when turned on
            if ((bool)shapesWhileDrawing.IsChecked && !onFlyTurnedOn)
                circuitPanel.InkSketch.InkCanvas.Strokes.StrokesChanged += new StrokeCollectionChangedEventHandler(circuitPanel_OnFlyRecognition);
            else if (!(bool)shapesWhileDrawing.IsChecked && onFlyTurnedOn)
                circuitPanel.InkSketch.InkCanvas.Strokes.StrokesChanged -= new StrokeCollectionChangedEventHandler(circuitPanel_OnFlyRecognition);
            
            onFlyTurnedOn = (bool)shapesWhileDrawing.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void colorShapes_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.StrokeColoring = (bool)colorShapes.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void highlightEndpoints_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.EndpointHighlighting = (bool)highlightEndpoints.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void meshHighlight_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.MeshHighlighting = (bool)meshHighlight.IsChecked;
        }

        /// <summary>
        /// Called when you check or uncheck this box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tooltipsOn_Click(object sender, RoutedEventArgs e)
        {
            if (displayManager != null)
                displayManager.StylusTooltips = (bool)tooltipsOn.IsChecked;
        }

        /// <summary>
        /// Function for updating the wires when an endpoint is snapped
        /// </summary>
        /// <param name="movedPoint"></param>
        /// <returns></returns>
        public bool endPointChange(Sketch.EndPoint oldLocation, Point newLocation, Stroke attachedStroke, Sketch.Shape shapeAtNewLoc)
        {
            SketchPanelLib.CommandList.EndPointMoveCmd mover = new SketchPanelLib.CommandList.EndPointMoveCmd(circuitPanel.InkSketch, 
                oldLocation, newLocation, attachedStroke, shapeAtNewLoc);
            
            mover.Regroup += new SketchPanelLib.CommandList.EndpointMoveHandlerRegroup(selectionTool_GroupTogether);
            mover.Handle += new SketchPanelLib.CommandList.EndpointMoveHandler(endpointMoveCallback);
            commandManager.ExecuteCommand(mover);
            return true;
        }

        private void endpointMoveCallback(Sketch.Shape shape)
        {
            InkChanged(true);
        }

        #endregion

        #region Recognition on the Fly Callback

        private void circuitPanel_OnFlyRecognition(object sender, StrokeCollectionChangedEventArgs e)
        {
            // Create a pipeline comprising the first three recognition stages and run it
            onFlyPipeline.process(circuitPanel.InkSketch.FeatureSketch);
            drawGates();
        }

        /// <summary>
        /// draw just the things we want for on the fly recognition
        /// </summary>
        private void drawGates()
        {
            if (!(bool)shapesWhileDrawing.IsChecked)
                return;

            dirty = true;

            displayManager.RefreshFeedbacks();


            //draw ghost gates for the last few shapes
            int last = circuitPanel.InkSketch.Sketch.Shapes.Length - 1;
            int index = last;
            Sketch.Shape recentShape;
            circuitPanel.StylusDown += new StylusDownEventHandler(circuitPanel_StylusDown);
            while (index >= 0 && (last - index < 3))
            {
                recentShape = (Sketch.Shape)circuitPanel.InkSketch.Sketch.Shapes.GetValue(index);
                displayManager.DrawFeedback(recentShape);
                index--;
            }
            circuitPanel.InkCanvas.UseCustomCursor = false;
            simulationButton.Visibility = System.Windows.Visibility.Hidden;
        }

        private void circuitPanel_StylusDown(object sender, StylusEventArgs e)
        {
        }

        #endregion

        #region Recognition Button Callbacks

        /// <summary>
        /// Main recognition function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void recognizeButton_Click(object sender, RoutedEventArgs e)
        {

            if (CurrentStatus == Status.Recognizing)
                return;

            // Clear the undo/redo lists and update buttons
            commandManager.ClearLists();
            refreshUndoRedoPaste();
            if (UserStudy && trialNum == 0) SaveUserStudyFile();

            // The user should not be able to edit during recognition
            selectionTool.UnsubscribeFromPanel();
            modeIndicator.Visibility = Visibility.Hidden;

            Wait();
            
            // Clean up old display elements
            displayManager.RemoveFeedbacks();
            if (selectionTool.widgetsShowing)
                selectionTool.HideAllWidgets();
            if (selectionTool.selectionActive)
                selectionTool.RemoveMenu();

            // Check if it is safe to recognize
            if (!recognitionManager.sketchSubstrokesAreConsistent())
            {
                // Try to reload (once)
                Clear();
                circuitPanel.InkSketch.LoadSketch(circuitPanel.InkSketch.Sketch);
                if (!recognitionManager.sketchSubstrokesAreConsistent())
                    throw new Exception("Sketch and/or FeatureSketch are inconsistent.");
            }

            // Actually recognize
            CurrentStatus = Status.Recognizing;
            UpdateProgress("Classifying strokes...", 5);

            recognizeButton.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new doNextStep(classifyStrokes));

            recognizeButton.IsChecked = false;

            if (UserStudy) SaveUserStudyFile();
            EndWait();
        }

        private void selectionTool_GroupTogether(List<Sketch.Shape> shapesToBeGrouped)
        {
            foreach (Sketch.Shape shapeToGroup in shapesToBeGrouped)
            {
                recognitionManager.RerecognizeGroup(shapeToGroup);
                displayManager.ColorStrokesByType(new List<Sketch.Substroke>(shapeToGroup.Substrokes));
            }
            RecognitionChanged();

            displayManager.AlertFeedback(simulationManager.CircuitParser, shapesToBeGrouped);
            circuitPanel.EnableDrawing();
        }

        private void selectionTool_Learn(Sketch.Shape shape)
        {
            recognitionManager.Recognizer.learnFromExample(shape);
        }

        /// <summary>
        /// Event handler called when the simulation button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void delayedSimButton_Click(object sender, RoutedEventArgs e)
        {
            if (simulationManager.Valid)
                EditToSim();
            else // Why was this button even clickable?
                throw new Exception("Simulation button was clickable when circuit wasn't valid!");

            if (sender == simulationButton)
                simulationButton.IsChecked = false;
        }

        #endregion

        #region Rerec and Resim callbacks

        /// <summary>
        /// Handles when ink in the panel has been changed
        /// </summary>
        private void InkChanged(bool reColor)
        {
            dirty = true;

            circuitPanel.InkCanvas.UseCustomCursor = false;

            if (circuitPanel.Sketch.Substrokes.Length == 0)
            {
                NeedsRecognition();
                return;
            }

            foreach (Sketch.Substroke sub in circuitPanel.Sketch.Substrokes)
            {
                if ( sub.Type == new ShapeType() || (sub.ParentShape != null && !sub.ParentShape.AlreadyLabeled))
                {
                    NeedsRecognition();
                    return;
                }
            }

            RecognitionChanged();
        }

        /// <summary>
        /// Handles when ink in the panel has been re-labeled or re-grouped
        /// </summary>
        private void RecognitionChanged()
        {
            needsResim = true;
            needsRerec = false;
            dirty = true;

            recognitionManager.ConnectSketch();
            recognizeCircuit(false);

            displayManager.AlertFeedback(simulationManager.CircuitParser, false);

            circuitPanel.InkCanvas.UseCustomCursor = false;
        }

        /// <summary>
        /// Handles setting the sim button and bools for when the sketch needs recognition
        /// </summary>
        private void NeedsRecognition()
        {
            if (simulationManager.Subscribed)
                simulationManager.HideToggles();

            needsRerec = true;
            needsResim = true;

            displayManager.AlertFeedback(simulationManager.CircuitParser, false);

            simulationButton.Visibility = System.Windows.Visibility.Hidden;
        }

        #endregion

        #region Simulation-Editing Switches
        /// <summary>
        /// Updates the recognition and simulation buttons upon simulation.
        /// </summary>
        private void EditToSim()
        {
            if (simulationManager.CircuitParser.ParseErrors.Count > 0 || simulationManager.Subscribed)
                return;

            CurrentStatus = Status.Simulating;
            modeIndicator.Visibility = System.Windows.Visibility.Hidden;

            // Clear the command stack and update the edit menu
            commandManager.ClearLists();
            refreshUndoRedoPaste();

            simulationManager.SubscribeToPanel();
            selectionTool.UnsubscribeFromPanel();
            displayManager.EndPointHighlightPause();
            displayManager.RemoveFeedbacks();

            // Display our simulation toggles
            cleanToggle.Visibility = System.Windows.Visibility.Visible;
            truthTableToggle.Visibility = System.Windows.Visibility.Visible;

            // Set editing mode
            circuitPanel.DisableDrawing();

            // Disable editing from the edit menu
            this.disableEditing();

            // Convert simulation button to edit button
            recognizeButton.IsEnabled = false;
            simulationButton.Content = "Edit";
            simulationButton.Checked -= new RoutedEventHandler(delayedSimButton_Click);
            simulationButton.Checked += new RoutedEventHandler(editButton_Click);
        }

        /// <summary>
        /// Takes us out of simulation mode and puts us in edit mode
        /// </summary>
        private void SimToEdit()
        {
            // If you're not simulating, why are you here?
            if (!simulationManager.Subscribed) return;

            CurrentStatus = Status.Idle;

            modeIndicator.Visibility = System.Windows.Visibility.Visible;

            // Remove our simulation toggles
            cleanToggle.Visibility = System.Windows.Visibility.Hidden;
            truthTableToggle.Visibility = System.Windows.Visibility.Hidden;
            cleanToggle.IsChecked = false;
            truthTableToggle.IsChecked = false;

            simulationManager.UnsubscribeFromPanel();
            selectionTool.SubscribeToPanel();
            displayManager.EndpointHighlightUnpause();

            // Convert edit button to simulate button
            recognizeButton.IsEnabled = true;
            simulationButton.Content = "Simulate";
            simulationButton.Checked -= new RoutedEventHandler(editButton_Click);
            simulationButton.Checked += new RoutedEventHandler(delayedSimButton_Click);
            if (needsRerec)
                simulationButton.Visibility = System.Windows.Visibility.Hidden;

            // Selectively enable editing
            this.enableEditing();

            circuitPanel.EnableDrawing();
            displayManager.AlertFeedback(simulationManager.CircuitParser, false);
        }

        #endregion

        #endregion

        #region Helper functions

        /// <summary>
        /// Returns the last panel that the user hovered the stylus over.
        /// Currently just returns circuitPanel, no other main panels exist.
        /// </summary>
        /// <returns>The current panel in focus.</returns>
        private SketchPanel getLastActivePanel()
        {
            return circuitPanel;
        }
        
        /// <summary>
        /// Updates feedbacks
        /// </summary>
        /// <param name="result"></param>
        private void circuitPanel_ResultReceived(SketchPanelLib.RecognitionResult result)
        {
            if (CurrentStatus == Status.Recognizing)
            {
                UpdateProgress("Recognition complete.", 100, true);

                // Set status
                CurrentStatus = Status.Idle;
            }
            circuitPanel.Recognized = true;
        }

        /// <summary>
        /// Event for when a sketch is loaded. Looks to see if it has been recognized
        /// </summary>
        private void circuitPanel_SketchFileLoaded()
        {
            recognitionManager.Featuresketch = circuitPanel.InkSketch.FeatureSketch;
            if (circuitPanel.Recognized)
            {
                needsRerec = circuitPanel.Recognized;
                RecognitionChanged();
            }
        }

        /// <summary>
        /// Since MenuItems don't callback till sub items are called, this function is a
        /// helper that updates the Edit Menu values so only sensical things are clickable.
        /// They must have a non void return type to pass.
        /// </summary>
        private bool commandManager_updateEditMenu()
        {
            if (selectionTool.widgetsShowing)
                selectionTool.HideAllWidgets();
            return refreshUndoRedoPaste();
        }

        private bool commandManager_moveUpdate()
        {
            selectionTool.closeAllButSelect();
            return refreshUndoRedoPaste();
        }

        /// <summary>
        /// Selectively enables the Undo, Redo and Paste options
        /// </summary>
        /// <returns></returns>
        private bool refreshUndoRedoPaste()
        {
            menuUndo.IsEnabled = commandManager.UndoValid;
            menuRedo.IsEnabled = commandManager.RedoValid;
            menuPaste.IsEnabled = !commandManager.ClipboardEmpty;
            return true;
        }

        /// <summary>
        /// Disables all the editing commands on the dropdown Edit Menu (since you shouldn't be able to edit in Simulation mode)
        /// </summary>
        /// <returns></returns>
        private bool disableEditing()
        {
            menuCut.IsEnabled = false;
            menuCopy.IsEnabled = false;
            menuPaste.IsEnabled = false;
            menuDelete.IsEnabled = false;
            menuDeleteAll.IsEnabled = false;
            menuSelectAll.IsEnabled = false; 
            menuUndo.IsEnabled = false;
            menuRedo.IsEnabled = false;
            menuPaste.IsEnabled = false;
            return true;
        }

        /// <summary>
        /// Enables delete all/select all on the dropdown Edit Menu (when returning to Edit mode from Simulation mode)
        /// </summary>
        /// <returns></returns>
        private bool enableEditing()
        {
            // Won't be enabled at first
            menuCut.IsEnabled = false;
            menuCopy.IsEnabled = false;
            menuPaste.IsEnabled = false;
            menuDelete.IsEnabled = false;

            // Should always be enabled (except in Simulation mode)
            menuDeleteAll.IsEnabled = true;
            menuSelectAll.IsEnabled = true;
            return refreshUndoRedoPaste();
        }

        /// <summary>
        /// Displays the wait cursor and disallows drawing.
        /// To end this, call EndWait().
        /// </summary>
        private void Wait()
        {
            circuitPanel.InkCanvas.UseCustomCursor = true;
            circuitPanel.InkCanvas.Cursor = Cursors.Wait;
            circuitPanel.DisableDrawing();
        }

        /// <summary>
        /// Returns cursor to normal and reallows drawing.
        /// Generally called after Wait()
        /// </summary>
        private void EndWait()
        {
            displayManager.SubscribeToPanel();
            circuitPanel.InkCanvas.UseCustomCursor = false;
            circuitPanel.EnableDrawing();
        }

        #endregion

        #region Recognition Steps

        /// <summary>
        /// Classifies the sketch's strokes
        /// </summary>
        private void classifyStrokes()
        {
            recognitionManager.ClassifySingleStrokes();

            if (debug)           
                displayManager.ColorStrokesByType();
            circuitPanel.InvalidateVisual();

            // If we're currently trying to do the entire recognition process,
            // continue on to the next step.
            if (CurrentStatus == Status.Recognizing)
            {
                UpdateProgress("Grouping strokes...", 20);

                recognizeButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new doNextStep(groupStrokes));
            }
        }

        /// <summary>
        /// Groups strokes into shapes.
        /// </summary>
        private void groupStrokes()
        {
            recognitionManager.GroupStrokes();

            if (debug)
            {
                displayManager.displayGroups();
                circuitPanel.InvalidateVisual();
            }

            // If we're currently trying to do the entire recognition process,
            // continue on to the next step.
            if (CurrentStatus == Status.Recognizing)
            {
                UpdateProgress("Recognizing ink...", 60);

                recognizeButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new doNextStep(recognizeSketch));
            }
        }

        /// <summary>
        /// Recognizes shapes in the sketch.
        /// </summary>
        private void recognizeSketch()
        {
            recognitionManager.Recognize();

            if (debug)
                displayManager.ColorStrokesByType();

            // No matter what, we'll want to start connecting shapes.
            UpdateProgress("Connecting shapes...", 80);

            recognizeButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new doNextStep(connectSketch));
        }

        /// <summary>
        /// Connects substrokes in the sketch
        /// </summary>
        private void connectSketch()
        {            
            recognitionManager.ConnectSketch();

            // If we're currently trying to do the entire recognition process,
            // continue on to the next step.
            if (CurrentStatus == Status.Recognizing)
            {
                UpdateProgress("Refining recognition...", 90);

                recognizeButton.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.SystemIdle,
                    new doNextStep(refineRecognition));
            }
        }

        /// <summary>
        /// Tries to fix up mistakes in the sketch recognition
        /// </summary>
        private void refineRecognition()
        {
            recognitionManager.RefineSketch();
           
            // If we're currently trying to do the entire recognition process,
            // continue on to the next step.
            if (CurrentStatus == Status.Recognizing)
            {
                UpdateProgress("Constructing circuit...", 95);
                recognizeCircuit(true);
            }

            displayManager.AlertFeedback(simulationManager.CircuitParser, CurrentStatus == Status.Recognizing);
            circuitPanel.Recognized = true;
        }

        /// <summary>
        /// Triggers CircuitParser Recognition on circuit panel
        /// </summary>
        private void recognizeCircuit(bool showMessage)
        {
            recognitionManager.MakeShapeNames();

            // Recognize the circuit if we need to 
            if (!UserStudy && (needsResim || (bool)HighlightAfterRec.IsChecked))
                simulationManager.recognizeCircuit();

            if (CurrentStatus == Status.Recognizing)
                UpdateProgress("Done.", 100, true);

            // Update information
            needsRerec = false;

            if (!UserStudy)
            {
                simulationButton.Visibility = System.Windows.Visibility.Visible;
                needsResim = !simulationManager.Valid;

                // Show the message if we have an invalid circuit
                if (needsResim && showMessage)
                    MessageBox.Show(this, "The recognized circuit has errors. Please fix these errors before hitting 'Simulate' to continue.", "Errors!");
                else if (simulationManager.CircuitParser.ParseErrors.Count > 0)
                {
                    // Disable the simulation button if we have parse errors
                    needsResim = true;

                    if (showMessage)
                    {
                        string text = simulationManager.CircuitParser.ParseErrors[0].Explanation;
                        MessageBox.Show(this, text, "Errors!");
                    }
                }
                // Selectively show simulation button
                simulationButton.IsEnabled = !needsResim;
            }

            if (EmbedEnabled)
            {
                subCircuitSave.Height = 0;
                subCircuitSave.Visibility = Visibility.Hidden;
                if (!needsRerec || !needsResim)
                {
                    subCircuitSave.Height = 25;
                    subCircuitSave.Visibility = System.Windows.Visibility.Visible;
                }
            }
            // Make it so the user can edit again
            selectionTool.SubscribeToPanel();
            modeIndicator.Visibility = Visibility.Visible;

            // Update the display
            displayManager.AlertFeedback(simulationManager.CircuitParser, CurrentStatus == Status.Recognizing);
            CurrentStatus = Status.Idle;
        }

        #endregion

        #region Progress Window

        /// <summary>
        /// Makes and shows the window with the recognition progress bar
        /// </summary>
        private void bringUpProgressWindow()
        {
            progressWindow = new ProgressWindow.MainWindow();
            progressWindow.Show();
        }

        /// <summary>
        /// Closes the progress window
        /// </summary>
        private void closeProgressWindow()
        {
            if (progressWindow != null)
                progressWindow.Close();
            progressWindow = null;
        }

        /// <summary>
        /// Displays the given message and progress in the progress window, closes the window after if closeWindow is set to true.
        /// 
        /// Will bring up the progress window if it is null.
        /// </summary>
        /// <param name="message">The message the progress window should display</param>
        /// <param name="progress">The percentage the progress bar should be at</param>
        /// <param name="closeWindow">Set to true if we should close the window after displaying the message</param>
        private void UpdateProgress(string message, int progress, bool closeWindow = false)
        {
            if (progressWindow == null)
                bringUpProgressWindow();

            progressWindow.setText(message);
            progressWindow.setProgress(progress);

            if (closeWindow)
                closeProgressWindow();
        }

        #endregion

        #region Notes Window

        private void bringUpNotes(object sender, RoutedEventArgs e)
        {
            notesWindow = new NotesWindow.MainWindow(ref notesPanel);
            notesWindow.Show();
            notesWindow.Closed += new EventHandler(notesWindow_Closed);
        }

        private void closeNotes(object sender, RoutedEventArgs e)
        {
            if (notesWindow != null && notesWindow.IsEnabled)
            {
                notesWindow.Close();
                notesWindow.Closed -= new EventHandler(notesWindow_Closed);
            }
        }

        private void notesWindow_Closed(object sender, EventArgs e)
        {
            NoteExpander.IsExpanded = false;
        }

        #endregion

        #region Practice Window

        private void bringUpPractice(object sender, RoutedEventArgs e)
        {
            Wait();

            practiceWindow = new PracticeWindow.MainWindow(ref practiceSketch, ref practiceRecognizer);
            practiceWindow.Show();
            practiceWindow.Closed += new EventHandler(practiceWindow_Closed);

            EndWait();
        }

        private void closePractice(object sender, RoutedEventArgs e)
        {
            if (practiceWindow != null && practiceWindow.IsEnabled)
            {
                practiceWindow.Close();
                practiceWindow.Closed -= new EventHandler(practiceWindow_Closed);
            }
        }

        private void practiceWindow_Closed(object sender, EventArgs e)
        {
            PracticeExpander.IsExpanded = false;
        }

        #endregion

        #region Clean Circuit

        private void cleanToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (!cleanInNewWindow)
            {
                circuitDock.Children.Remove(circuitPanel);
                simulationManager.DisplayCleanCircuit(circuitDock);
            }
            else
            {
                bringUpCleanWindow();
            }
        }

        private void cleanToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!cleanInNewWindow)
            {
                simulationManager.RemoveCleanCircuit(circuitDock);
                circuitDock.Children.Add(circuitPanel);
            }
            else
            {
                closeCleanWindow();
            }
            displayManager.AlertFeedback(simulationManager.CircuitParser, false);
            simulationManager.colorWires();
        }
        
        /// <summary>
        /// Brings up the cleaned circuit window
        /// </summary>
        private void bringUpCleanWindow()
        {
            cleanWindow = new CleanCircuitWindow.CleanWindow();

            simulationManager.DisplayCleanCircuit(cleanWindow.CleanWindowDock);
            cleanWindow.Show();
            cleanWindow.Closed += new EventHandler(cleanWindow_Closed);
        }

        /// <summary>
        /// Closes the cleaned circuit window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeCleanWindow()
        {
            cleanWindow.CleanWindowDock.Children.Clear();
            if (cleanWindow != null && cleanWindow.IsEnabled)
                cleanWindow.Close();
            cleanWindow.Closed -= new EventHandler(cleanWindow_Closed);
        }

        /// <summary>
        /// Event that handles closing the clean window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cleanWindow_Closed(object sender, EventArgs e)
        {
            cleanToggle.IsChecked = false;
        }
        
        #endregion

        #region Truth Table

        private void truthTableToggle_Checked(object sender, RoutedEventArgs e)
        {
            simulationManager.TruthTableClosed += new SimulationManager.TruthTableClosedHandler(TruthTableClosed);
            simulationManager.DisplayTruthTable();
        }

        private void truthTableToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            simulationManager.TruthTableClosed -= new SimulationManager.TruthTableClosedHandler(TruthTableClosed);
            simulationManager.CloseTruthTable();
        }

        private void TruthTableClosed()
        {
            truthTableToggle.IsChecked = false;
        }

        #endregion

        #region User Study

            #region User Study Constants

        /// <summary>
        /// The number of times the user must draw each gate or circuit
        /// </summary>
        private const int NUMITERATIONS = 2;

        /// <summary>
        /// The number of recognized and corrected circuits the user will draw
        /// </summary>
        private const int NUMCIRCUITS = 5;

        /// <summary>
        /// The number of unrecognized circuits the user will draw at the end
        /// </summary>
        private const int NUMFINALCIRCUITS = 2;

            #endregion

            #region User Study internals

        /// <summary>
        /// The "trial number" is the number of times the user has recognized this equation
        /// When it is changed, call trialNumChanged()
        /// </summary>
        private int trialNum = 1;

        /// <summary>
        /// The number of the current user
        /// </summary>
        private int userNum = 1;

        /// <summary>
        /// The current equation
        /// </summary>
        private int eqIndex = 0;

        /// <summary>
        /// The current warmup gate
        /// </summary>
        private int gateIndex = 0;

        /// <summary>
        /// The number of attempts the user has made at this equation or gate
        /// </summary>
        private int curIteration = 1;

        /// <summary>
        /// All the gate names we want the users to practice with
        /// </summary>
        private Dictionary<int, string> gateNames;

        /// <summary>
        /// All the equations, along with their reference number
        /// </summary>
        private Dictionary<int, string> equations;

        /// <summary>
        /// The numbers of the chosen equations
        /// </summary>
        private List<int> chosenEqs;

        /// <summary>
        /// Are we during the warmup phase?
        /// </summary>
        private bool warmup = true;
            #endregion

            #region User Study Initialization

        /// <summary>
        /// Initializes the lists used in the user study
        /// </summary>
        private void initUserStudy()
        {
            // Clear the canvas
            circuitPanel.SimpleDeleteAllStrokes();

            // Initialize lists
            chosenEqs = new List<int>();
            gateNames = new Dictionary<int, string>();
            equations = new Dictionary<int, string>();

            if (recognizeButton != null)
                recognizeButton.Visibility = System.Windows.Visibility.Hidden;

            // Fill in the gateNames list
            int i = 0;
            foreach (ShapeType gate in LogicDomain.Gates)
                if (gate != LogicDomain.FULLADDER && gate != LogicDomain.SUBTRACTOR && 
                    gate != LogicDomain.NOTBUBBLE && gate != LogicDomain.SUBCIRCUIT)
                {
                    gateNames[i] = gate.Name;
                    i += 1;
                }

            // Reset some counters
            warmup = true;
            gateIndex = 0;
            curIteration = 1;
            eqIndex = -1;
            trialNum = 0;
            trialNumChanged();
            UniqueUser();
            UserStudySetIndicators();
            studyProgress.Value = 0;
            progressLabel.Text = (int)studyProgress.Value + "% Completed";


            // I reordered the equations in order from easiest to hardest:
            // For baseline data, the order was  5, 4, 2, 3, 1, 6, 7
            // For the ghost gate condition, the order was 6, 2, 3, 7, 5, 1, 4

            // Baseline Order
            // Rewrote the equations so that they all have 6 gates in them.
            equations[1] = "d = (a NOR b) NAND [(NOT b) OR c]\ne = (b XNOR c) XOR c";
            equations[2] = "d = (b OR c) NOR (b NAND c) \ne = (a XNOR b) AND (NOT c)";
            equations[3] = "d = (NOT a) OR (b XOR c) \ne = [a NAND (b XNOR c)] AND a";
            equations[4] = "d = [a AND (a XOR b)] XNOR (NOT b)\ne = [a NOR b] NAND (b OR c)";
            equations[5] = "d = [a AND (b NOR c)] \ne= (NOT c) NAND [(a XNOR b) XOR c]";
            equations[6] = "d = (a NOR b) XOR (NOT c) \ne = (a NAND b) OR (a AND c)";
            equations[7] = "d = a OR [(NOT a) AND b] \ne = a XNOR [(NOT b) XOR c]";

            // GG Order
            /*equations[2] = "d = a OR (b XOR c) \ne = [a OR (b XOR c)] AND (b XOR c)"; //EQ1
            equations[6] = "d = [a AND (b OR c)] NAND [NOT (b OR c)]"; //EQ2
            equations[5] = "d = a XNOR (NOT b)\ne = [a NOR (NOT b)] NAND (b OR c)"; //EQ3
            equations[1] = "d = (a XOR b) XOR c \ne = (a AND b) OR [c AND (a XOR b)]"; //EQ4
            equations[3] = "d = a OR [(NOT a) AND b] \ne = {a OR [(NOT a) AND b]} XOR (b NAND c)"; //EQ5
            equations[7] = "d = b OR c\ne = (a XNOR b) AND (NOT c)"; //EQ6
            equations[4] = "d = (a NOR b) NAND [(NOT b) OR c]\ne = (b XNOR c)"; //EQ7 */

            // Original Order
            /*equations[1] = "d = a OR (b XOR c) \ne = [a OR (b XOR c)] AND (b XOR c)"; //EQ1
            equations[2] = "d = [a AND (b OR c)] NAND [NOT (b OR c)]"; //EQ2
            equations[3] = "d = a XNOR (NOT b)\ne = [a NOR (NOT b)] NAND (b OR c)"; //EQ3
            equations[4] = "d = (a XOR b) XOR c \ne = (a AND b) OR [c AND (a XOR b)]"; //EQ4
            equations[5] = "d = a OR [(NOT a) AND b] \ne = {a OR [(NOT a) AND b]} XOR (b NAND c)"; //EQ5
            equations[6] = "d = b OR c\ne = (a XNOR b) AND (NOT c)"; //EQ6
            equations[7] = "d = (a NOR b) NAND [(NOT b) OR c]\ne = (b XNOR c)"; //EQ7 */

            // Choose equations to use
            /*Random random = new Random();
            List<int> temp = equations.Keys.ToList();
            while (chosenEqs.Count < NUMCIRCUITS + NUMFINALCIRCUITS && temp.Count != 0)
            {
                int next = random.Next(0, temp.Count);
                chosenEqs.Add(temp[next]);
                temp.RemoveAt(next);
            }*/

            // Choose equations to use
            chosenEqs = equations.Keys.ToList();
        }

            #endregion

            #region Saving Study Files

        /// <summary>
        /// Returns a filename and path for the current user, equation, and trial
        /// </summary>
        /// <returns></returns>
        private string UserStudyFilename()
        {
            // We're saving this data in Data\DrawingStyleStudyData
            string directory = FilenameConstants.DataRoot + @"DrawingStyleStudyData\User" + userNum + @"\";
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            string filename = "";
            if (warmup)
                filename = gateNames[gateIndex] + "_Iter" + curIteration + "_" + trialNum + FilenameConstants.DefaultXMLExtension;
            else
            {
                string gates = "GatesOff";
                string recognized = "Recognized" + trialNum;
                if ((bool)cond1.IsChecked) gates = "GatesOn";
                if (trialNum == 0) recognized = "Unrecognized";

                filename = eqIndex + "_EQ" + chosenEqs[eqIndex] + "_" + gates + "_Iter" + curIteration + "_" + recognized + FilenameConstants.DefaultXMLExtension;
            }
            return directory + filename;
        }

        /// <summary>
        /// Saves the current sketch to the study data folder with a unique filename
        /// </summary>
        private void SaveUserStudyFile()
        {
            // Please don't save if there is nothing in the sketch, or if nothing has changed
            if (circuitPanel.InkCanvas.Strokes.Count == 0 || (commandManager.CommandCounts.Keys.Count == 0 && trialNum != 1)) return;

            Wait();

            // If we're in warmup, label the shape before we save it.
            if (warmup)
            {
                // Make sure that nothing is in a shape or group
                circuitPanel.InkSketch.Sketch.clearShapes();
                circuitPanel.InkSketch.Sketch.RemoveLabelsAndGroups();

                // Add all the strokes to a single shape, label them all as belonging to a gate
                Sketch.Shape shape = new Sketch.Shape(circuitPanel.InkSketch.Sketch.SubstrokesL, Sketch.XmlStructs.XmlShapeAttrs.CreateNew());
                foreach (Sketch.Substroke substroke in shape.Substrokes)
                    substroke.Classification = LogicDomain.GATE_CLASS;
                shape.Type = LogicDomain.getType(gateNames[gateIndex]);

                circuitPanel.InkSketch.Sketch.AddShape(shape);
                recognitionManager.MakeShapeNames();
            }


            // Let's not write over exising data...
            string filename = UserStudyFilename();
            while (System.IO.File.Exists(filename))
            {
                trialNum += 1;

                filename = UserStudyFilename();
            }

            if (commandManager.CommandCounts.Keys.Count != 0)
                SaveUserCommands();
            circuitPanel.SaveSketch(filename);

            // Iterate the trial number
            trialNum += 1;
            trialNumChanged();

            EndWait();
        }

        /// <summary>
        /// Prints the number of times the user has performed various commands to a text file.
        /// </summary>
        private void SaveUserCommands()
        {
            // Create a new file for this user.  If the file already exists, add to it
            string filename = FilenameConstants.DataRoot + @"DrawingStyleStudyData\User" + userNum + @"\Commands.txt";
            System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
            Dictionary<string, int> counts = commandManager.CommandCounts;

            // For now, recording all commands rather than specific ones.
            if (warmup)
                writer.WriteLine("Gate " + gateNames[gateIndex] + ", Iter " + curIteration + ", Trial " + trialNum);
            else
                writer.WriteLine(eqIndex + ") EQ " + chosenEqs[eqIndex] + ", Iter " + curIteration + ", Trial " + trialNum);
            writer.WriteLine("---------------------------------------------------");
            foreach (string command in counts.Keys)
            {
                writer.WriteLine(command + ": " + counts[command].ToString());
            }
            writer.WriteLine();
            writer.Close();
        }

            #endregion

            #region Advancing The Study

        /// <summary>
        /// When we change the trial, clear the command manager counts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trialNumChanged()
        {
            if (commandManager != null)
                commandManager.Clear();
            if (trialNum == 0 && !warmup)
                nextButton.IsEnabled = false;
            else
                nextButton.IsEnabled = true;
        }

        /// <summary>
        /// Save everything form the last user, iterate the user number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newUserButton_Click(object sender, RoutedEventArgs e)
        {
            SaveUserStudyFile();
            initUserStudy();
        }

        /// <summary>
        /// Get a user number that hasn't been used before
        /// </summary>
        /// <returns></returns>
        private void UniqueUser()
        {
            int num = userNum;
            // We're saving this data in Data\DrawingStyleStudyData
            string directory = FilenameConstants.DataRoot + @"DrawingStyleStudyData\User" + num + @"\";
            while (System.IO.Directory.Exists(directory))
            {
                num += 1;
                directory = FilenameConstants.DataRoot + @"DrawingStyleStudyData\User" + num + @"\";
            }
            System.IO.Directory.CreateDirectory(directory);
            userNum = num;

            this.Title = FilenameConstants.ProgramName + " - User " + userNum;
            userIDIndicator.Text = "User ID: " + userNum;
        }

        /// <summary>
        /// Saves the sketch, clears the screen, and displays the next equation or gate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveUserStudyFile();
            circuitPanel.SimpleDeleteAllStrokes();
            circuitPanel.EnableDrawing();
            displayManager.RemoveFeedbacks();

            trialNum = 0;
            trialNumChanged();

            // Are we doing the current gate/circuit again?
            if (curIteration < NUMITERATIONS)
            {
                curIteration += 1;
            }
            else
            {
                curIteration = 1;

                // We're in the gates-only section
                if (warmup)
                {
                    gateIndex += 1;

                    // You have hit the end of the gates-only section!
                    if (gateIndex == gateNames.Count)
                    {
                        MessageBox.Show(this, "You are now done with the warmup section of the study.\nFeel free to take a short break.", "Warmup Over");

                        // Set up for non-warmup tasks
                        warmup = false;
                        recognizeButton.Visibility = System.Windows.Visibility.Visible;
                        nextButton.IsEnabled = false;

                        circuitPanel.EnableDrawing();
                    }
                }

                // Circuits!
                if (!warmup)
                {
                    eqIndex += 1;
                    if (eqIndex == (int)(chosenEqs.Count) / 2)
                        MessageBox.Show(this, "You are halfway done with the circuit portion of the study.\nFeel free to take a short break.", "Halfway done!");
                }
            }

            // Set the progress bar, equation indicator, and everything else
            UserStudySetIndicators();
        }

        /// <summary>
        /// Sets the text of the equation and iteration indicators, as well as the value of the progress bar
        /// </summary>
        private void UserStudySetIndicators()
        {
            // Indicates how many times you have done this same gate/circuit
            if (NUMITERATIONS > 1)
                iterationIndicator.Text = "Trial #" + curIteration;

            // Set the progress bar and equation indicator, depending on where we are.
            // Circuits are counted as three times the progress of individual gates.
            double totalProgress = (gateNames.Count + 3 * chosenEqs.Count) * NUMITERATIONS;
            if (warmup) // Gate section
            {
                equationIndicator.Text = gateNames[gateIndex];
                studyProgress.Value += 1 / totalProgress * 100;
            }
            else if (eqIndex < chosenEqs.Count) // Circuits section
            {
                equationIndicator.Text = equations[chosenEqs[eqIndex]];
                studyProgress.Value += 3 / totalProgress * 100;
            }
            else // End of study
            {
                equationIndicator.Text = "Thank you for participating!";
                studyProgress.Value = 100;
                MessageBox.Show(this, "You are done with the test!\nThank you for participating!", "You're the Best!");
            }
            progressLabel.Text = (int)studyProgress.Value + "% Completed";
        }

            #endregion

            #region Study Conditions

        /// <summary>
        /// The first condition for the study.  Enables Ghost Gates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cond1_Click(object sender, RoutedEventArgs e)
        {
            highlightEndpoints.IsChecked = false;
            meshHighlight.IsChecked = false;
            shapesAfterLabel.IsChecked = true;
            shapesAfterRec.IsChecked = true;
            shapesOnHover.IsChecked = true;
            labelsAfterRec.IsChecked = false;
            shapesWhileDrawing.IsChecked = false;
            colorShapes.IsChecked = true;
            tooltipsOn.IsChecked = false;
            displayManager.EndpointHighlighting = false;
        }

        /// <summary>
        /// The second condition for the study. Disables Ghost Gates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cond2_Click(object sender, RoutedEventArgs e)
        {
            highlightEndpoints.IsChecked = false;
            meshHighlight.IsChecked = false;
            shapesAfterLabel.IsChecked = false;
            shapesAfterRec.IsChecked = false;
            shapesOnHover.IsChecked = false;
            labelsAfterRec.IsChecked = true;
            shapesWhileDrawing.IsChecked = false;
            colorShapes.IsChecked = true;
            tooltipsOn.IsChecked = false;
            displayManager.EndpointHighlighting = false;
        }

            #endregion

            #region User study on/off

        /// <summary>
        /// Sets up the UI for the user study
        /// </summary>
        private void UserStudyOn()
        {
            Clear();
            commandManager.CountCommands = true;
            commandManager.Clear();

            // Make special user study things visible
            headerUserStudyMenu.Visibility = System.Windows.Visibility.Visible;
            nextButton.Visibility = System.Windows.Visibility.Visible;
            equationIndicator.Visibility = System.Windows.Visibility.Visible;
            studyProgress.Visibility = System.Windows.Visibility.Visible;
            progressLabel.Visibility = System.Windows.Visibility.Visible;
            if (NUMITERATIONS > 1) iterationIndicator.Visibility = System.Windows.Visibility.Visible;

            // Make some normal things invisible, or at least disabled (or visible)
            headerFeedbackMenu.IsEnabled = false;
            headerFileMenu.IsEnabled = false;
            headerViewMenu.IsEnabled = false;
            PracticeExpander.Visibility = System.Windows.Visibility.Hidden;
            recognizeButton.Visibility = System.Windows.Visibility.Hidden;
            nextButton.IsEnabled = true;

            // Initialize everything!
            cond1.IsChecked = true;
            initUserStudy();
        }

        /// <summary>
        /// Puts everything back to normal after the user study
        /// </summary>
        private void UserStudyOff()
        {
            Clear();
            commandManager.CountCommands = false;
            commandManager.Clear();

            // Make special user study things invisible
            headerUserStudyMenu.Visibility = System.Windows.Visibility.Hidden;
            nextButton.Visibility = System.Windows.Visibility.Hidden;
            equationIndicator.Visibility = System.Windows.Visibility.Hidden;
            studyProgress.Visibility = System.Windows.Visibility.Hidden;
            progressLabel.Visibility = System.Windows.Visibility.Hidden;
            iterationIndicator.Visibility = System.Windows.Visibility.Hidden;

            // Make some normal things invisible, or at least disabled
            headerFeedbackMenu.IsEnabled = true;
            headerFileMenu.IsEnabled = true;
            headerViewMenu.IsEnabled = true;
            PracticeExpander.Visibility = System.Windows.Visibility.Visible;
            recognizeButton.Visibility = System.Windows.Visibility.Visible;

            // We probably want endpoints and mesh highlighting on...
            meshHighlight.IsChecked = true;
            highlightEndpoints.IsChecked = true;
        }

        /// <summary>
        /// When you check the user study checkbox, call this event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void userStudyOption_Click(object sender, RoutedEventArgs e)
        {
            UserStudy = !UserStudy;
            userStudyOption.IsChecked = UserStudy;
            if (UserStudy)
                UserStudyOn();
            else
                UserStudyOff();
        }

            #endregion

        #endregion
    }
}
