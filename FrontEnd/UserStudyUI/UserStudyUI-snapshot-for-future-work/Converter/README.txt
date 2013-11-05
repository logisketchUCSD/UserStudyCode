Converter by Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.
Harvey Mudd College, Claremont, CA 91711.
Sketchers 2006.

The Converter stores that functions that can be used to convert a 
Microsoft Journal file into an XML format developed at MIT.
This process works as follows:

JNT --> XML JNT
First, the JNT file is converted into an XML JNT file, which still encodes the Ink
data in Base64.

XML JNT --> InkObject
The InkObject is then extracted from the XML JNT.

InkObject --> Strokes/Points
The Stroke and Point data is then extracted from the InkObject.

Strokes/Points --> Converter.Strokes/Converter.Points
The Ink formatted Strokes and Points are then put into the MIT XML Stroke/Point
standards.

Converted.Strokes/Converted.Points --> MIT XML
The Converted data is then put into the MIT XML format.

