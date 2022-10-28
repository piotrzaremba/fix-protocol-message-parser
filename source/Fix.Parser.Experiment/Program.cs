using System;
using System.Text;
using System.IO;
using QuickFix.DataDictionary;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Fix.Parser.Experiment
{
    class Program
    {
        static void FixToJson(string messageStr, bool humanReadableValues, QuickFix.DataDictionary.DataDictionary sessionDataDictionary, QuickFix.DataDictionary.DataDictionary appDataDictionary)
        {
            try
            {

                    QuickFix.IMessageFactory msgFactory = new QuickFix.DefaultMessageFactory();
                    ParsableMessage msg = new Fix.Parser.Experiment.ParsableMessage();
                    string line = messageStr;
                    string comma = "";

                        line = line.Trim();
                        msg.FromString(line, false, sessionDataDictionary, appDataDictionary, msgFactory);
                        Console.WriteLine(comma + msg.ToJSON(sessionDataDictionary, humanReadableValues));

                        comma = ",";
     



            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }


        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 4)
            {
                System.Console.WriteLine("USAGE");
                System.Console.WriteLine("");
                System.Console.WriteLine("    FixToJson.exe FILE [HUMAN_READABLE_VALUES] [DATA_DICTIONARY]");
                System.Console.WriteLine("");
                System.Console.WriteLine("EXAMPLES");
                System.Console.WriteLine("");
                System.Console.WriteLine("    FixToJson.exe messages.log true ../../spec/fix/FIX50SP2.xml");
                System.Console.WriteLine("    FixToJson.exe messages.log true ../../spec/fix/FIX44.xml");
                System.Console.WriteLine("    FixToJson.exe messages.log false ../../spec/fix/FIX42.xml");
                System.Console.WriteLine("");
                System.Console.WriteLine("NOTE");
                System.Console.WriteLine("");
                System.Console.WriteLine("    Per the FIX JSON Encoding Specification, tags are converted to human-readable form, but values are not.");
                System.Console.WriteLine("    Set the HUMAN_READABLE_VALUES argument to TRUE to override the standard behavior.");
                //System.Environment.Exit(2);
            }

            /*
            string fname = args[0];
            bool humanReadableValues = false;
            QuickFix.DataDictionary.DataDictionary sessionDataDictionary = null;
            QuickFix.DataDictionary.DataDictionary appDataDictionary = null;

            if (args.Length > 1)
            {
                humanReadableValues = bool.Parse(args[1]);
            }

            if (args.Length > 2)
            {
                sessionDataDictionary = new QuickFix.DataDictionary.DataDictionary(args[2]);
                appDataDictionary = sessionDataDictionary;
            }
            */

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

            var _dataDictionary = new DataDictionary("FIX44.xml");

            FixToJson(messageStr, false, _dataDictionary, _dataDictionary);
            Environment.Exit(1);
        }
    }
}