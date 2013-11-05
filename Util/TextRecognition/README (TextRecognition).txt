TextRecognition:
----------------
NOTE: To use this project, the Microsoft.Ink dll might need to be added to the project that is using this project.

To recognize strokes, there are five methods to use (four of them are overloaded).  recognize has four overloads.  The first takes in a shape and returns a string.  The second takes in a shape and a factiod and returns a string.  There are two factoids available by using TextRecognition.TextRecognition.data, which is used for truth table recognition (O,1,X), and TextRecognition.TextRecognition.label, which is used for label recognition (first character is a letter, and subsequent characters are letters or numbers). The third takes in a shape, a factoid, and a recognition mode and returns a string.  The recognition mode can be changed to coerce with this method by putting in RecognitionModes.Coerce (the previous two default to the lenient recognition mode), which forces the result to be in the factoid provided.  The fourth takes in a shape, a wordlist, and a recognition mode and returns a string.  The wordlist is a list of words that the text recognizer tries to match the shape to, and it can be coerced to this wordlist with the recognition mode.  The last method is recognizeAlternates, which takes in a shape, a wordlist, a recognition mode, and an integer indicating the number of alternate text recognition results desired.  An array of strings is returned with the length equal to the number of alternates.

The factoids can be obtained as described above, or other factiods can be used (look online at msdn to see other factoids).  The wordlist can be loaded using the default wordlist created by using createLabelWordList().  A wordlist can be loaded from a file by loadLabelWordList(loadLabelStringList(filename)).  A list of strings can be saved to file to be used as a wordlist later by using saveLabelWordList(List<string> wordsToLoad) since the words in a wordlist cannot be iterated through.

So, if a user wants to add words to a wordlist, it needs to be added to the wordlist that is currently being used but also to the list of strings that is loaded with loadLabelStringList(filename).  This way, when the expanded word list is going to be saved, the string list already has the all the words that were there before and the words that were added.  Then, the saveLabelWordList can be called on the list of strings.

Files:
------

1) InkRecognition.cs: implements the Microsoft Ink Recognition.  Called from TextRecognition.cs.

2) TextRecognition.cs: recognizes the given strokes as text.  It has overload methods for recognizing with no factoid, with a given factoid, or with a given WordList (can use the default WordList or load from a text file).  The recognition mode can also be selected (i.e. can specify coerce).  Can also save a list of strings to a text file so that it can later be loaded into a WordList.

3) WordList.txt: a text file that can be loaded from to create a WordList and then later saved to.  Intially, contains the same words as the default WordList.