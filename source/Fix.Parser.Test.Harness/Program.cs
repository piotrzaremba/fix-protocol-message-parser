using System.Diagnostics;
using Fix.QuickFix.Parser;
using QuickFix;
using QuickFix.DataDictionary;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Fix.Parser.Test.Harness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var tcr = new TradeCaptureReport();

                var noClearingInstructions1 = new TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup();
                noClearingInstructions1.ClearingInstruction = new ClearingInstruction(1);
                var noClearingInstructions2 = new TradeCaptureReport.NoSidesGroup.NoClearingInstructionsGroup();
                noClearingInstructions2.ClearingInstruction = new ClearingInstruction(2);

                var sidesGrp1 = new TradeCaptureReport.NoSidesGroup();
                sidesGrp1.Account = new Account("Silvio");
                sidesGrp1.OrderID = new OrderID("09011900");
                sidesGrp1.Side = new Side(Side.BUY);
                sidesGrp1.AddGroup(noClearingInstructions1);
                sidesGrp1.AddGroup(noClearingInstructions2);

                var sidesGrp2 = new TradeCaptureReport.NoSidesGroup();
                sidesGrp2.Account = new Account("Sven");
                sidesGrp2.OrderID = new OrderID("2000");
                sidesGrp2.Side = new Side(Side.BUY);
                sidesGrp2.AddGroup(noClearingInstructions2);

                tcr.AddGroup(sidesGrp1);
                tcr.AddGroup(sidesGrp2);

                var messageStr = tcr.ToString();

                var fixParser = new Parser("./FIX44.xml");
                var json = fixParser.Parse(messageStr);
                Console.WriteLine(json);

                var dataDictionary = new DataDictionary("./FIX44.xml");
                
                IMessageFactory msgFactory = new DefaultMessageFactory();
                var message = new ParsableMessage();
                message.FromString(messageStr, true, dataDictionary, dataDictionary, msgFactory);
                Console.WriteLine(message.ToJson(dataDictionary, false));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}