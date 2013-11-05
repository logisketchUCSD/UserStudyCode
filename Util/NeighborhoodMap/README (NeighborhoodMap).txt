NeighborhoodMap:
----------------
This is a class library, so another program needs to call this.  However, there is a Main function that can be used to test the class if the project is changed to a Console Application.

To use the class library:
Create a new object of the Neighborhood class.  The constructor takes a labeled or unlabeled sketch.  After, call the nonstatic method CreateGraph.  CreateGraph takes in a ClosenessMethod, which is either EUCLIDIAN, BLOCK, or TIME.  The thresholds for considreing something close can be changed by changing the constant fields in the internals region at the top of the code file.  More closeness measures can be added by adding another name to the enum ClosenessMeasure at the top of Neighborhood.cs and adding corresponding field to PointDistance.cs in Sketch.  Also, a method to compute that distance has to be added to PointDistance.cs, as well as a threshold for the distance measure with the rest of the thresholds.  A dictionary mapping the Substroke GUID to neighboring substrokes is stored in the Graph field.

Files:
------

1) Neighborhood.cs: Creates a mapping of substrokes to other substrokes that are near each other based on some distance metric (i.e. Euclidian distance, Block distance, or Time).  Uses a dictionary to map the GUID of a substroke to a list of substrokes.  The thresholds for the distance metrics can be adjusted at the top of the file in the Internal region.