Andrew Danowitz

For PinList Documentation, scroll down

Pin:
-----------
This is a class library, so it needs to be called from another program.  To use this, create a new Pin object.  There are a number of Pin constructors that require various combinations of:  pin name, pin value, PinPolarity, expected pin value or a sketch.shape shape.  

Pin Methods:
-----------------

val2str():  Returns the pin value char[] as a string

expected2str():  Returns the pin expected value char[] as a string

findIndex(List<Pin> pins):  Finds the zero-based index of the pin's location inside a list of pins.  If the pin is not a member of the list, the returned value will be larger than (List.count-1)

Equals(Pin pin):  Equals overload method for Pin Class.  Determines pin equality based on pin name, polarity and bus size

printend(String filepath, Sting filename):  Prints the line "endmodule" at the end of the file specified by filename

Externally Available Values:
---------------------------------
PinName (gettable, settable):  Returns String pin name

PinNames (gettable):  Returns String[] of alternate pin name text recognition results

PinVal (gettable, settable):  Returns char[] containing pin value

Expected (gettable, settable):  Returns char[] containing expected pin value

Polarity (gettable):  Returns PinPolarity of current pin

bussize (gettable):  Returns int containing bus size of current pin

Shape (gettable):  Returns Sketch.Shape containing hand-drawn pin shape

Known Bugs/Issues:
-------------------------
None

Likely Modifications:
--------------------------------
Equals(Pin pin):  To increase or alter parameters used to compare pin equalities, alter the arguments 	in the class if statement
	
	Example:  Modify Pin equality to not depend on pin polarity

	Default Code:
	-----------------
	if (this.pinNames[0] == pin.pinNames[0] && this.bussize == pin.bussize && 				this.polarity.Equals(pin.polarity))
               	 return true;

	Modified Code:
	------------------
	if (this.pinNames[0] == pin.pinNames[0] && this.bussize == pin.bussize)
               	return true;

PinPolarity:  To modify pin polarities, simply add or remove options in the PinPolarity enum

	Example:  Add inOut as pin polarity

	Default Code:
	----------------
	public enum PinPolarity
    	{
        		Input,
        		Ouput,
       		 Wire
    	}

	Modified Code:
	------------------
	public enum PinPolarity
    	{
        		Input,
        		Ouput,
        		Wire,
		inOut
    	}

-------------------------------------------------------------------------------------------------------------------------

PinList:
-----------
This is a class library, so it needs to be called from another program.  To use this, create a new PinList object.  PinList is a wrapper class designed specifically to extend the functionality of List<Pin>.  The constructor takes either no arguments of a list of Pins.

PinList Methods:
--------------------
Contains (Pin pin):  Checks PinList for an instance of pin.  Pin equality determined using Pin.equals method

Add (Pin pin):  Adds pin instance to list.  Returns false if pin instance is null

AddRange(List<Pin> addPins):  Adds list of pins to PinList

removePin(Pin pin):  Removes pin instance from PinList.  Retruns false if pin instance is not a member 	of PinList.  Pin equality determined using Pin.equals method

clk_var():  Searches PinList for a pin that represents a clock input to a circuit.  If no clock pin is found, 	a pin with the name "auto_clock" is automatically generated.  Clock pin is returned. 
	Current supported clock pin names:  "clk," "Clk," "CLK," "clock," "Clock," "CLOCK"

clk_rm(Pin clkVar):  Attempts to removes clock variable from pin list and returns true if successful.  	Differs from remove in that it checks whether the clockVar is "auto_clk," indicating that no 	clock has existed in pin list

RemoveAt(int i):  Removes pin at zero-based index i

Externally Available Values:
---------------------------------
Pins (gettable):  Returns List<Pin> contained in PinList structure

Count (gettable):  Returns number of pins stored in PinList

Known Bugs/Issues:
-------------------------
None

Likely Modifications:
------------------------
clk_var():  To add or remove recognized "clock" pin names, add or remove if/else if statements from the foreach statement
