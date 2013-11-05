Andrew Danowitz, Sketchers 2007

1) Description:
This is a modified version of the xilinx-tasks.tcl file that comes installed with Xilinx.  It contains the sketch_draw function used to call the circuit-simulator ui from the Xilinx TCL shell.  

2) Installation:
	1.  Replace the default xilinx-tasks.tcl script in the bin\script folder in the Xilinx install 	                     directory with the modified script.  
	2.  Replace {c:\circuitsimulatorui\bin\debug\circuitsimulatorui} in the set ui_path command 	                   (line 17) with the full directory path of the sketchers program (in brackets)
	     Note:  There can be no spaces in the filepath