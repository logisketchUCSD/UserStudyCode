TestCircuitRec:
---------------
This is a Console Application that can either be run from the command line, can be complied and run, or can be run from the executable.  This is used to test CircuitRec and to more easily debug it than by using the GUI.  Enter a filename where prompted.  The address of the file is relative, so if the data gets moved or the folders are moved, it will not work.  The addressing is: ../../../../../Recognition/IOTrain/Test Data/" + input + ".switched.xml.  So, it expects something of the form 0128_1.1.1 or 0128_1.1.1.edit.  (.edit files have wires grouped so that they have more than one endpoint, and others have it grouped so that wires should only have two endpoints).  If the addressing needs to be changed, it can be edited in the Input Select region.  Below that is the domain file that is loaded.  Right now it is digital_domain.txt, which should be put in the proper directory when TestCircuitRec is built.  Next, the wordlist is loaded from WordList.txt, which should also be put in the proper directory when the project is built.  The filenames/paths can be changed to load in any domain or wordlist.  After, the recognition is run, the errors are displayed, and the ZedGraph form is shown with some of the results of the recognition (the form is mainly used to see the endpoints and the console output is used for debugging purposes that cannot be as easily seen with the GUI).  After closing the form, hit any but "n" to enter a different file to be tested.  "n" will exit the application.

Files:
------

1) Form1.cs: The ZedGraph form that is used to display the sketch as a collection of points.

2) TestCircuitRec.cs: Console application where the name of a sketch is given, then the recognition is run, and the results are displayed in the ZedGraph form.  Requires XML files to be in the IOTrain/TestData folder and to have .switched.xml extension. (only need to put in the file name without the .switched.xml).

3) digital_domain.txt: Domain file for the digital domain.

4) fulltrain467_13: Describes the 13 feature neural net used as a default to determine if a Wire is an input, output, or internal wire.