#--------------------------------------------------------------------------#
########################### DecisionTreeFeatures ###########################
#--------------------------------------------------------------------------#

This project wraps the C4.5 decision tree file format. The DTF class wraps
the format proper, aht the CRFFeaturesToDTF class is an executable that
writes the current CRF features to a DTF-format file. It is -extremely-
ad-hoc, and will require several lines of code added for each new feature
added to the CRF.

Usage:
	CRFFeaturesToDTF.exe [-F] LabelMapperFile Directory

The LabelMapperFile is a LabelMapper domain file, as described in the readme
for LabelMapper. The Directory is a directory of files to read in and
featurefy. If the -F flag is passed, the sketches will be autofragmented;
otherwise, they will not.
