module NAME(input[31:0] A, input[31:0] B, input[31:0] C, input[31:0] D, input reset, input clock, output[31:0] Y);

wire[31:0] Z;
wire[31:0] X;
wire[15:0] D;
wire[7:0] Den;
wire[31:0] Muxout;

assign Z = A & B;
assign X = C & D;
assign Y = ~(Z | X);
always @(posedge clk, posedge reset)
if(reset)		Q <= 0;
else			Q <= D;
always @(posedge clk, posedge reset)
if(reset)		Qen <= 0;
elseif(en)		Qen <= Den;
always @( * )
case(s)
	2'b00: Muxout <= d0;
	2'b01: Muxout <= d1;
	2'b10: Muxout <= d2;
	2'b11: Muxout <= d3;
endcase
endmodule
