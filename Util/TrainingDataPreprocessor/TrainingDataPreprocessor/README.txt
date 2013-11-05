#--------------------------------------------------------------------------#
######################### TrainingDataPreprocessor #########################
#--------------------------------------------------------------------------#

This project really is two separate projects, but I only made one because
I'm lazy. TrainingData is a class for storing and manipulating sets of
training data for image-based gate recognizers (it is used currently only in
the Congealer, but that could change), where as Preprocessor is a
command-line application for creating these files.

This is a runtime optimization for training (particularly for the
congealer). It turns out that going through a few hundred XML files, finding
all of the shapes, pulling them out, and rasterizing them is an intensive
process. It can take upwards of 20 minutes sometimes, which is annoying if
you have many runs to do. This program does all of that for you and puts it
into a TrainingData objects which you can then serialize and retrieve data
from later.

Preprocessor contains a command-line application to extract the data, and
TrainingData contains the actual serialized object itself. To get data out
of a serialized TrainingData object, use the static ReadFromFile method to
deserialize it. You can then use the Gates accessor to get a set of gate
types, or the Images accessor to get a List of GateImages (a struct which
contains both a Shape and its rasterized bitmap).

In general, you will want to have your build options set to build a Library.
However, if you want create a new preprocessed data file, you will need to
change your build options to build as an executable.

#----------------------------- Future Work --------------------------------#

The most important thing that needs to happen here is breaking this into two
different projects so that we don't have all of this mess with multiple
build targets. Otherwise, well, the program is insanely parallelizable, so
if somebody wanted to rewrite it to be multi-threaded, that'd be neat. I'd
personally use a ThreadPool, as I did in the CongealRecognizer (see the
Recognition\Recognizers\CongealRecognizer.cs file for more information).

Also, the way bubbles are handled is extremely hacky. We currently have some
data labeled NOTBUBBLE and some labeled BUBBLE, so I just do a replace of
NOTBUBBLE/NOT_BUBBLE into BUBBLE. Somebody should just go write an awk or
perl script to fix all of the files.
