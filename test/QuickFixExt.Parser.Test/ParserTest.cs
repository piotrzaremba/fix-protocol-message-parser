using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using QuickFix;
using QuickFix.DataDictionary;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace QuickFixExt.Parser.Test
{
    public class UnitTest1
    {

        [Fact]
        public void WithSimpleMessageFromObject()
        {
            // Arrange
            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":33,\"MsgType35\":\"D\",\"OrderQty38\":100,\"Price44\":1.00,\"Side54\":\"1\",\"Symbol55\":\"AAPL\"}";

            var nos = new NewOrderSingle();

            nos.SetField(new Side('1'));
            nos.SetField(new OrderQty(100));
            nos.SetField(new Symbol("AAPL"));
            nos.SetField(new Price(1.00m));

            // Act
            var dataDictionary = new DataDictionary("./FIX44.xml");

            IMessageFactory msgFactory = new DefaultMessageFactory();
            var message = new ParsableMessage();
            message.FromString(nos.ToString(), true, dataDictionary, dataDictionary, msgFactory);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithComplexMessageFromObject()
        {
            // Arrange
            const string expected =
                "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":96,\"MsgType35\":\"AE\",\"LastPx31\":1.24,\"NoSides552\":[{\"Account1\":\"Silvio\",\"OrderID37\":\"09011900\",\"Side54\":\"1\",\"NoClearingInstructions576\":[{\"ClearingInstruction577\":1},{\"ClearingInstruction577\":2}]},{\"Account1\":\"Sven\",\"OrderID37\":\"2000\",\"Side54\":\"1\",\"NoClearingInstructions576\":[{\"ClearingInstruction577\":2}]}]}";

            var tcr = new TradeCaptureReport();

            tcr.SetField(new LastPx(1.24m));

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

            // Act
            var dataDictionary = new DataDictionary("./FIX44.xml");

            IMessageFactory msgFactory = new DefaultMessageFactory();
            var message = new ParsableMessage();
            message.FromString(tcr.ToString(), true, dataDictionary, dataDictionary, msgFactory);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithSimpleMessage()
        {
            // Arrange
            var messageStr = "8=FIX.4.4\u00019=33\u000135=D\u000138=100\u000144=1.00\u000154=1\u000155=AAPL\u000110=037\u0001";

            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":33,\"MsgType35\":\"D\",\"OrderQty38\":100,\"Price44\":1.00,\"Side54\":\"1\",\"Symbol55\":\"AAPL\"}";

            // Act
            var dataDictionary = new DataDictionary("./FIX44.xml");
            var message = new ParsableMessage(messageStr);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithSimpleMessageBlankPrice()
        {
            // Arrange
            var messageStr = "8=FIX.4.4\u00019=122\u000135=G\u000149=SALI\u000156=SNEX\u000134=8617\u000152=20221123-13:32:37\u000141=1572\u000111=1574\u000121=3\u000155=MSFT\u000154=1\u000160=20221123-13:32:37\u000140=1\u000138=1575\u000144=\u000110=192\u0001";

            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":122,\"MsgSeqNum34\":8617,\"MsgType35\":\"G\",\"SenderCompID49\":\"SALI\",\"SendingTime52\":\"20221123-13:32:37\",\"TargetCompID56\":\"SNEX\",\"ClOrdID11\":\"1574\",\"HandlInst21\":\"3\",\"OrderQty38\":1575,\"OrdType40\":\"1\",\"OrigClOrdID41\":\"1572\",\"Price44\":,\"Side54\":\"1\",\"Symbol55\":\"MSFT\",\"TransactTime60\":\"20221123-13:32:37\"}";

            // Act
            var dataDictionary = new DataDictionary("./FIX44.xml");
            var message = new ParsableMessage(messageStr);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithSimpleMessageAlphaPrice()
        {
            // Arrange
            var messageStr = "8=FIX.4.4\u00019=125\u000135=G\u000149=SALI\u000156=SNEX\u000134=8617\u000152=20221123-13:32:37\u000141=1572\u000111=1574\u000121=3\u000155=MSFT\u000154=1\u000160=20221123-13:32:37\u000140=1\u000138=1575\u000144=abc\u000110=233\u0001";

            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":125,\"MsgSeqNum34\":8617,\"MsgType35\":\"G\",\"SenderCompID49\":\"SALI\",\"SendingTime52\":\"20221123-13:32:37\",\"TargetCompID56\":\"SNEX\",\"ClOrdID11\":\"1574\",\"HandlInst21\":\"3\",\"OrderQty38\":1575,\"OrdType40\":\"1\",\"OrigClOrdID41\":\"1572\",\"Price44\":abc,\"Side54\":\"1\",\"Symbol55\":\"MSFT\",\"TransactTime60\":\"20221123-13:32:37\"}";

            // Act
            var dataDictionary = new DataDictionary("./FIX44.xml");
            var message = new ParsableMessage(messageStr);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithSimpleMessageBlankPriceFromString()
        {
            // Arrange
            var messageStr = "8=FIX.4.4\u00019=122\u000135=G\u000149=SALI\u000156=SNEX\u000134=8617\u000152=20221123-13:32:37\u000141=1572\u000111=1574\u000121=3\u000155=MSFT\u000154=1\u000160=20221123-13:32:37\u000140=1\u000138=1575\u000144=\u000110=192\u0001";

            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":122,\"MsgSeqNum34\":8617,\"MsgType35\":\"G\",\"SenderCompID49\":\"SALI\",\"SendingTime52\":\"20221123-13:32:37\",\"TargetCompID56\":\"SNEX\",\"ClOrdID11\":\"1574\",\"HandlInst21\":\"3\",\"OrderQty38\":1575,\"OrdType40\":\"1\",\"OrigClOrdID41\":\"1572\",\"Price44\":,\"Side54\":\"1\",\"Symbol55\":\"MSFT\",\"TransactTime60\":\"20221123-13:32:37\"}";

            // Act
            IMessageFactory msgFactory = new DefaultMessageFactory();
            var dataDictionary = new DataDictionary("./FIX44.xml");
            var message = new ParsableMessage();
            message.FromString(messageStr, true, dataDictionary, dataDictionary, msgFactory);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }

        [Fact]
        public void WithSimpleMessageAlphaPriceFromString()
        {
            // Arrange
            var messageStr = "8=FIX.4.4\u00019=125\u000135=G\u000149=SALI\u000156=SNEX\u000134=8617\u000152=20221123-13:32:37\u000141=1572\u000111=1574\u000121=3\u000155=MSFT\u000154=1\u000160=20221123-13:32:37\u000140=1\u000138=1575\u000144=abc\u000110=233\u0001";

            var expected = "{\"BeginString8\":\"FIX.4.4\",\"BodyLength9\":125,\"MsgSeqNum34\":8617,\"MsgType35\":\"G\",\"SenderCompID49\":\"SALI\",\"SendingTime52\":\"20221123-13:32:37\",\"TargetCompID56\":\"SNEX\",\"ClOrdID11\":\"1574\",\"HandlInst21\":\"3\",\"OrderQty38\":1575,\"OrdType40\":\"1\",\"OrigClOrdID41\":\"1572\",\"Price44\":abc,\"Side54\":\"1\",\"Symbol55\":\"MSFT\",\"TransactTime60\":\"20221123-13:32:37\"}";

            // Act
            IMessageFactory msgFactory = new DefaultMessageFactory();
            var dataDictionary = new DataDictionary("./FIX44.xml");
            var message = new ParsableMessage();
            message.FromString(messageStr, true, dataDictionary, dataDictionary, msgFactory);

            // Assert
            Assert.Equal(expected, message.ToJson(dataDictionary, false));
        }
    }
}