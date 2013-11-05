SketchPanel:
------------

The SketchPanel project functions as both a library and example program that provides a basis for building 
Ink-enabled GUIs using the Sketchers Sketch framework (e.g., the Sketch, ReadXML, and ReadJNT projects).  
The SketchPanel library conists of a main SketchPanel class that binds a Windows Forms Panel, InkPicture, and 
Sketch together, as well as Recognizer and Subscriber frameworks for building classes that interoperate with
SketchPanel.  The SketchPanel example program, called SketchJournal, is a vanilla, Windows-Journal-like 
drawing application that records Ink and runs an example recognizer on drawn symbols.  

SketchPanel is similar to LabelerPanel; see the Labeler project for more information.

Files:
------

SketchPanel Framwork:
SketchPanel.cs -- main framework class
SketchPanelSubscriber.cs -- base class for building modules that listen to SketchPanel events (e.g. Feedback
Mechanisms or editing tools)
RecognizerInterface.cs -- base class for building Recognizers that process the Sketch embedded in a 
SketchPanel.  Supports running a recognizer in a separate thread.

SketchJournal Example:
SketchJournal.cs and SketchJournal.Designer.Cs -- Main form and program for SketchJournal example program.  
data -- directory of symbol data files for example recognizer
images -- sample background images for SketchJournal program



Known Bugs:
-----------
There are many scrolling-related bugs; some have to do with how the Ink framework handles scroll events.  
Furthermore, the drawing area currently only grows as a sketch gets larger; if the user erases strokes then 
the sketch area does NOT currently shrink (this resetting scrollbars).  To change this behavior, see 
SketchPanel.cs.