#--------------------------------------------------------------------------#
################################## Labeler #################################
#--------------------------------------------------------------------------#

This is the primary UI for converting and labeling sketches.

Features:
   * Reads MIT XML, Windows Journal, and DRS files
   * Outputs MIT XML files
   * Supports run-time domain selection. Domain files are plain-text files
     which support colorization and label selection, and are documented in the
     Domains directory
   * Multiple labeling-display modes (detailed below)
   * Real-time stroke resampling
   * Variable zoom levels
   * Single-stroke feature information display (detailed below)
   * Sketch feature summary display (detailed below)
   * Integration with the auto-fragmenter
   * Custom fragmentation panel
   * One-level undo and redo support.

Usage:

   Your first step should be to load a domain file. These are .txt files
   located in the Domains directory which describe what sort of file you'll
   be working with (i.e., digital circuits). The domain file format is
   specified in the README located in that directory. To load a domain,
   either click on "File->Load Domain", or the convenient "Load Domain"
   toolbar button, and navigate to the domain file.

   You will now want to open a sketch. As mentioned above, the Labeler can
   read MIT XML, Microsoft Windows Journal, and DRS files. Click
   "File->Open" or the "Open Sketch" toolbar button to open a sketch.

   Once you have a sketch open, you can select substrokes either by drawing
   a lasso around them, or simply clicking on the substrokes. To clear your
   selection, click on whitespace. To label strokes, click on the "LABEL"
   button that appears when strokes are selected and check off the labels
   that you want to apply to that stroke or group of strokes. If you select
   multiple strokes when applying a label to them, they will be grouped and
   labeled (this is the preferred method for labeling single shapes).

   If you wish to label only part of a stroke, you should fragment that
   stroke. Clicking the "Auto Fragment" button will apply the
   auto-fragmenter to the entire sketch at once. Please note that
   autofragmentation will clear any hand-fragmentation that you have
   previously performed.If these results are not satisfactory, you may
   select a stroke and click the "Frag Stroke" toolbar button to open a
   dialog box for stroke fragmentation. To fragment a stroke, draw a line
   over the point where you want fragmentation. To commit the changes, click
   the "Done" button in the Fragment Stroke dialog box.

Display Modes:

   Training and testing will often be problematic if multiple labels are
   assigned to a single shape, or if shapes are broken into multiple groups.
   To reduce these errors, the Labeler supports multiple Labeling Display
   modes. To toggle through them, click the "Labeling Mode" toolbar button,
   or to navigate directly to one, use the "Labeling Mode" menu item.

   In the first labeling mode, "Normal", colors are assigned from the Domain
   file for each label. 

   In the "Multiple" labeling mode, colors are assigned based on how many
   labels a stroke has. Strokes with no labels are colored Red, strokes with
   one label are Yellow, and strokes with more than one label are Green.

   In the "Gate" labeling mode, each cluster that corresponds to a different
   Gate type (currently hard-coded into Sketch) is displayed in a different
   color. This is most useful for checking for mis-grouped gates (for
   example, NOT gates with the bubble grouped separately from the buffer).

   Finally, the "Non-Gate" labeling mode acts exactly like the "Gate" labeling
   mode, but for all strokes that are not of the gate type.

Feature Display

   The labeler supports inline feature display of both single-stroke
   features (via FeatureStroke) and multi-stroke features (via
   FeatureSketch). To view single-stroke features, click on a stroke, then
   click "View->Stroke Information". To view sketch features, click
   "View->Sketch Information Summary".

Known Bugs

   * Selection of single strokes is sometimes difficult when viewing
     sketches whose units are in pixel space (as opposed to himetric space).
     The current recommended workaround is to use the lasso selection method
     instead of the click selection method
   * Viewing stroke and sketch features can be expensive for very large
     sketches. Optimizations are currently under way, but for now we can
	 only advise that you not attempt to view feature data for sketches of
	 over 2 MiB.
   * The display may sometimes flicker, particularly when resizing the
     sketch. This is an unfortunate side effect of the hackish way that
	 double-buffering is done in pre-WPF code.

#--------------------------------------------------------------------------#

Code Notes

   The heart of the Labeler is the LabelerPanel class, which is a custom
   control used both here and in most of our other display projects. It
   serves to bind the Microsoft.Ink data to our custom Sketch class and
   display the data to the user.

   The single-stroke information is actually displayed via a different
   project (StrokeInfoForm), which is shared with the SketchPanel input
   utility. Multi-stroke information is displayed via a dialog defined in
   this project, SketchSummary. Both allow for one-line addition of features
   to the display, presuming that said features are already calculated in
   either Featurefy.FeatureSketch or Featurefy.FeatureStroke.
