using PharmacyMS.Domain.Rules;
var total=StockRules.LineTotal(3,12.50m);
if(total!=37.50m) throw new Exception("Purchase total incorrect.");
if(StockRules.GrandTotal([total],5m)!=32.50m) throw new Exception("Discount incorrect.");
try {StockRules.EnsureSaleable(2,3,new DateOnly(2030,1,1),new DateOnly(2026,10,17));throw new Exception("Oversell was accepted.");}
catch(InvalidOperationException) { }
try {StockRules.EnsureSaleable(3,1,new DateOnly(2026,10,16),new DateOnly(2026,10,17));throw new Exception("Expired stock was accepted.");}
catch(InvalidOperationException) { }
Console.WriteLine("4 Domain checks passed.");
