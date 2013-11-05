#--------------------------------------------------------------------------#
############################### LabelMapper ################################
#--------------------------------------------------------------------------#

This class is an extensible way to convert from one set of domain labels, to
another. The best example is the CRF -- for training and testing purposes,
it requires that sketches be labeled with things like "Wire", "Gate", and
"Label". However, our data is labeled with things like "Wire", "AND", "OR",
and "Label". A quick run through LabelMapper with the appropriate map file
will replace the more detailed labels with more appropriate alternatives.

LabelMapper map files are simple text files where each line contains a
mapping of two strings separated by "=". For example, a line
might say "AND=Gate".

To use the LabelMapper in your code, include the LabelMapper project,
instantiate a LabelMapper object, and either use the translateSketch
function to translate an entire sketch at once, or use the labelMap
dictionary to convert individual objects.
