InkToSketch:
-----------
This project contains a class library intended to provide a mapping between Microsoft Ink and Sketchers Sketch 
functionality.  For example, the class library contains an InkSketch class that binds an Ink instance to a
Sketch instance, translating Ink stroke adds and deletes to Sketch stroke adds and deletes.  InkSketch also 
builds dictionaries mapping Substroke Ids to Ink Stroke Ids; many GUI-oriented tasks rely on these mappings 
to produce visual transformations. The purpose of this project is to provide a library that supports 
Ink-Sketch interoperability (especially in the context of developing MS Ink GUIs).


Files:
----- 
InkSketch.cs -- Main library class for binding Ink to Sketch.  Binds a generic Microsoft Ink object to a 
sketch object.  See class comments for details on functionality.

InkPictureSketch.cs -- Library class for binding an InkPicture to a Sketch.  Links additional functionality 
between Ink and Sketch using additional events that InkPicture provides.  This library class is also the 
current (7/07) main class that GUI projects like SketchPanel and CircuitSimulatorUI use.  

ReadInk.cs -- Utility class for reading in an Ink object and creating a matching sketch instance.  


Maintenance:
-----------

The most likely addition to this library would be an InkOverlaySketch class that links InkOverlay events
to equivalent Sketch actions.  An InkOverlaySketch class would likely look a lot like InkPictureSketch, but
InkOverlay provides different event names than InkPicture does.  

Bugs:
-----

No known bugs.  Some of the constructors in InkSketch have not been thoroughly tested but should work as written.
