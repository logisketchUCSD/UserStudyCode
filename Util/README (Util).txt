#--------------------------------------------------------------------------#
################################### UTIL ###################################
#--------------------------------------------------------------------------#

This directory holds various utility classes, some of which are libraries
used by Recognition and FrontEnd, while others are testing, training, and
analysis libraries.

BayesianRecognizer: Empty, for now.

CircuitSimLib: Library for a graph-based circuit that contains CircuitElements
	and connections. Also contains truth table values.

Classifier: A UI based on the Labeler for testing classification and
	clustering. Currently only used by the team at UCR.

CommandManagement: The frame work for command input in the labeler and
	SketchPanel. A wrapper that can be used to provide undo and redo
	functionality to arbitrary commands

ConverterDRS: DRS file-type handling. Currently read-only.

ConverterJnt: Microsoft Windows Journal file-type handling. Currently
	read-only.

ConverterXML: MIT XML file-type handling. Read/write

DataAnalysis: Some very old code that extracts the following statistics from
	labeled sketches:
	   * Number of gates/wires drawn with non-consecutive strokes
	   * Order of text relative to diagram pieces
	   * Order of labels vs. rest of diagram
	   * Order of gates vs. wires that connect to them
	   * Timing between strokes in the same object vs. different objects
	   * Differences between participants in any of the above
	   * Number of strokes per each gate, wire, and text item
	   * Length of strokes in each gate, wire, and text item

DecisionTreeFeatures: Console application for parsing our features
	(primarily those of the CRF) into the C4.5 decision tree file format.
	This was used in Summer 2008 to choose an ideal featureset in
	conjunction with UC Riverside.

DisplayManager: Manages color display of strokes, circuit feedback mechanisms, and 
	displaying the label of a recognized shape.

DomainInfo: Creates the domain info, including colors and labels, of a domain
	by reading in a domain file.

EditMenu: Menu for all edit/correction actions.

Files: Contains utility subroutines for manipulating files and filetypes.
	See the README in the subdirectory for more information.

GateStudy: Computes statistics on timing and other information for collected
	data and writes them to a CSV file. Written by the mysterious rgordon.

GaussianBlur: A fast implementation of a Gaussian Blur implemented from
	the article "Fast Gaussian Blur Algorithm in C#" found at "Adrian's Tech
	Blog" -- http://www.cnblogs.com/Dah/archive/2007/03/30/694527.html

GenerateSubsets: Get all subsets of a parameterized array, and optionally
	call a function on them. Not used by anything, but could be useful at
	some point in the future.

InkExplorer: A sample application provided with the Microsoft Tablet PC
	Platform SDK for analyzing stroke data. Notable in that it is painfully
	slow.

InkRecognition: A sample application provided with the Microsoft Tablet PC
	Platform SDK which performs text recognition using the Microsoft.Ink
	recognizer

InkToSketch: Used to convert tablet ink from Microsoft.Ink to Sketch.Sketch.
	I'm not sure why this is necessary any more, since the Labeler can read
	JNT files and save them as XML files, and hand-label them along the way.

InkToSketchWPF: Used to convert tablet ink from Windows.Ink to Sketch.Sketch.
	Keeps the InkCanvas for display and the featurified sketch for recognition
	in sync.

Labeler: The primary UI used to label and view sketches. For much more
	information, see the REAPlease note that
   autofragmentation will clear any hand-fragmentation that you have
   previously performed.DME file in the Labeler\ directory.

LabelMapper: Used to translate between shape labels in the domain, and
	simplified labels for use with the CRF or equivalent. For more
	information, see the README file in the LabelMapper\ directory.

MathNet.Iridium-2008.4.14.425: The Iridium math library for C#. Includes
	good matrix support, probability distributions, statistical functions,
	and many other useful math things. Note: this is an import of an
	external open-source project, but must be manually updated when new
	releases are made. To update, go to mathnet.opensourcedotnet.info and
	download the latest copy. You will have to update all references and
	solution files to point to the new copy. 

	If you need math functionality, it's in this project.

Metrics: A set of classes currently used for image distance

NeighborhoodMap: Contains several classes:
   * LinkedPoint maps a point to a substroke that it came from
   * NeighborhoodMap is a graph structure describing which substrokes are
     close to which other substrokes
   * Neighborhood: A testing/executable file with hardcoded paths.

Network: Some miscellaneous TCP/IP code that is no longer used by anything.
	Seems to implement a chat server running on port 1234 of machine
	134.173.43.9

PairedList: A list class that stores two items for each entry, and can sort
	by either of them. Quite messy code. 

Pin: Data structure used to store circuit labels and values

Set: Implements set functionality in C# (i.e., only holding one copy of each
	element). Includes several different implementations (ListSet, HashSet,
	etc.)

SimulationManager: Handles the interface of simulation, including the truth table,
	clean circuit, and circuit value toggles. 

Sketch: Holds strokes, substrokes, and point data about a sketch. Arguably
	the most important class in this project.

SketchCorrectness: compares two labeled sketches to determine how correct
	they are compared to each other

SketchStats: computes stats about a sketch (specifically a truth table) and 
	prints them

statistic: Compute some statistics. Seems highly sketchy, code-wise. Could
	probably be replaced by something from MathNet.Iridium.

SwitchLabels: A deprecated precursor to LabelMapper which is hardcoded to
	convert the labels "AND", "OR", "NOT", "NAND", "NOR", "XOR", and "XNOR" to
	"Gate" in sketch files. Almost certainly no longer needed. Interesting
	note: the author apparently cannot spell "argument".

SwitchTypeName: switches type and name fields in the labeled xml file. Not
	needed any more, but required because some files were created with
	switched fields, and were not compatible with anything.

TestCircuitRec: used to test CircuitRec

TestCongealing: Ad-hoc testing code for the congealing image recognizer (See
	the Recognition\Congeal directory for more information). Largely
	superceded by the integration of Congeal into the Recognizers framework
	and TestRig's SymbolStage.

TestConverter: initially, this was used to test Converter, but now it can
	test a lot of other stuff too

TestRig: The main test framework for this project. Includes modules for
	testing single-stroke labeling, grouping, shape recognition, and
	fragmentation. Designed to be extensible and fast. For more information,
	see the TestRig node on twiki.

TestRecogntition: Wrapper to test the Microsoft.Ink text recognizer.

TrainingDataPreprocessor: Preprocesses XML files into binary files used to
	train the Congealer (and, theoretically, any other recognizer that
	wanted to implement the required APIs). Vastly reduces training time.
	For more information, see the README file in the
	TrainingDataPreprocessor\ directory

ThresholdEval: Seems to have been designed to find thresholds for the CRF.
	Highly mediocre code. For more information, see the ThresholdEval node
	on twiki.

VerilogWriter: writes recognized circuit to Verilog

Weka: contains WekaWrap and related files. Used by the StrokeClassifier and
	the StrokeGrouper to make decisions. 
