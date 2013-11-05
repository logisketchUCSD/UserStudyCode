#--------------------------------------------------------------------------#
################################ Front Ends ################################
#--------------------------------------------------------------------------#

These are end-user products that let our work interact with the world at
large. At least, that's the hope. Most of these projects depend extensively
on projects in Recognition and Util.

3dpen-v3-kazu-high-res: Outdated C++ code

Circuit Recognizer: A project that takes a recognized sketch and converts this
 	into a circuit by creating the elements and meshes.

CircuitSimulatorUI: An all-inclusive UI for sketching and simulating
	circuits. Has not been updated to use the more recent recognition and
	grouping code, never mind the CRF. Needs an overhaul, which should
	probably include making the rest of the code pass around a single
	FeatureSketch and share it (since all of our new stuff will make
	recalculating features every few functions quickly become prohibitive).

CleanCircuitWindow: A window that can hold the cleaned up version of a circuit's image.
 	This is currently an option in WPFCircuitSimulatorUI.

ErrorCorrection:

ErrorCorrectionInterface:

GateUS:  A tool used during the Gate Structure User Study, GateUS asks the user to perform certain tasks and automatically saves them.

NotesWindow: A window that contains a sketching surface (InkCanvas) and a clear button. For use in 
	WPFCircuitSimulatorUI for user notes while drawing/recognizing circuits.

Power Point Interface:

SelectionManager: A manager that hooks into a SketchPanelWPF to allow editing/error-correction
	of the panel's sketch.  

SketchBasedInterface: Contains the project for HoverCross, a program that experiments
   	with using the hover space above a tablet in selection.

SketchPanel: A reusable Windows Forms Component (and supporting files) for
	building Sketch Recognition GUIs.  Includes a demo Sketch Journal app.

SketchPanelWPF: A resuabel Windows Controls Component for building SketchRecognition GUIs.
	Contains an InkCanvas for drawing as well as an InkSketch, which coordinates a featurified
	version of the sketch (for recognition) with the sketch on the screen.

UserStudyUI: Front end for testing recognition triggers, feedback
	mechanisms, and error rates.  

WizardLabeler: Adapted Labeler for supporting UserStudyUI Wizard of Oz
	Studies.  Uses deprecated code from old Labeler.

WPFCircuitSimulatorUI: An all-inclusive UI for sketching, recognizing, and simulating
	circuits.

XilinxIntegration: files relating to the project's final user interface and
	its eventual integration with Xilinx


