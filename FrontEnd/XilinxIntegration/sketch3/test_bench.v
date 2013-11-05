module testbench ();

reg clk, reset;

reg a, b, c, yexpected;

wire y;

reg [31:0] vectornum, errors;

reg [3:0] testvectors [10000:0];

// instantiate device under test
led dut (a, b, c, y);

// generate clock
always
   begin
      clk  1; #5; clk  0; #5;
   end

// at start of test, load vectors
// and pulse reset
initial
   begin
      $readmemb (“example.tv”, testvectors);
      vectornum  0; errors  0;
      reset  1; #27; reset  0;
   end

// apply test vectors on rising edge of clk
always @ (posedge clk)
   begin
      #1; {a, b, c, yexpected} 
         testvectors[vectornum];
   end

// check results on falling edge of clk
always @ (negedge clk)
   if (~reset) begin // skip during reset