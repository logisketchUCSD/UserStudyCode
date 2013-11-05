`timescale 1ns / 1ps 
//////////////////////////////////////////////////////////////////////////////////
// Company: 
// Engineer: 
// 
// Create Date:    10:18:06 05/22/2007 
// Design Name: 
// Module Name:    led 
// Project Name: 
// Target Devices: 
// Tool versions: 
// Description: 
//
// Dependencies: 
//
// Revision:   
// Revision 0.01 - File Created
// Additional Comments: 
//
//////////////////////////////////////////////////////////////////////////////////
module led(clk, s, led); 
    input clk;
    input [3:0] s;
    output [7:0] led;

assign led[0] = s[0]; 
assign led[1] = ~s[0];
assign led[2] = s[1];
assign led[3] = ~s[1];
assign led[4] = s[2];
assign led[5] = ~s[2];
assign led[6] = s[2] & s[3];
assign led[7] = clk;

endmodule
