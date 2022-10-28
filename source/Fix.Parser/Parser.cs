using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickFix;
using QuickFix.DataDictionary;
using QuickFix.Fields;
using QuickFix.FIX44;

namespace Fix.Parser
{
    public class Parser
    {
        private static StringBuilder _stringBuilder;
        private readonly DataDictionary _dataDictionary;

        public Parser(string dataDictionaryFilename)
        {
            _dataDictionary = new DataDictionary(dataDictionaryFilename);
        }

        public string Parse(string messageStr)
        {
            _stringBuilder = new StringBuilder();
            try
            {
                IMessageFactory defaultMsgFactory = new MessageFactory();

                var message = new QuickFix.Message();
                message.FromString(messageStr, true, _dataDictionary, _dataDictionary, defaultMsgFactory);

                var msgType = message.Header.GetField(new StringField(35));
                var fieldsByTag = _dataDictionary.FieldsByTag;
                var ddGrps = _dataDictionary.GetMapForMessage(msgType.Obj).Groups;

                ConvertToJson(_dataDictionary, msgType, fieldsByTag, ddGrps, message);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return _stringBuilder.ToString();
        }

        private static void ConvertToJson(DataDictionary dataDictionary, StringField msgType, IDictionary<int, DDField> fieldsByTag, Dictionary<int, DDGrp> ddGrps, QuickFix.Message message)
        {
            var rootNode = new JObject();

            var headerNode = new JObject();
            ConvertFieldMapToJson(dataDictionary, msgType, fieldsByTag, ddGrps, message.Header, headerNode);
            rootNode.Add("header", headerNode);

            var bodyNode = new JObject();
            ConvertFieldMapToJson(dataDictionary, msgType, fieldsByTag, ddGrps, message, bodyNode);
            rootNode.Add("body", bodyNode);

            var trailerNode = new JObject();
            ConvertFieldMapToJson(dataDictionary, msgType, fieldsByTag, ddGrps, message.Trailer, trailerNode);
            rootNode.Add("trailer", trailerNode);

            var str = rootNode.ToString(Formatting.Indented);
            _stringBuilder.AppendLine(str);
        }

        private static void ConvertFieldMapToJson(DataDictionary dataDictionary, StringField msgType, IDictionary<int, DDField> fieldsByTag, Dictionary<int, DDGrp> ddGrps, FieldMap fieldMap, JObject node)
        {
            var fieldIterator = fieldMap.GetEnumerator();
            while (fieldIterator.MoveNext())
            {
                var pair = (KeyValuePair<int, IField>)fieldIterator.Current;
                var field = pair.Value;

                var isPartOfGroupFlag = false;
                foreach (var ddItem in ddGrps)
                {
                    var ddGrp = ddItem.Value;

                    isPartOfGroupFlag |= CheckIsInGroup(dataDictionary, msgType, field, ddGrp);
                }

                if (isPartOfGroupFlag) continue;

                if (ddGrps.ContainsKey(field.Tag)) continue; // Is Group # Tag

                var ddField = fieldsByTag.FirstOrDefault(x => x.Value.Tag == field.Tag);
                if (ddField.Value == null) continue;

                node.Add(new JProperty($"{ddField.Value.Name}{pair.Key}", pair.Value.ToString()));
            }

            var groupTags = fieldMap.GetGroupTags();
            foreach (var groupTag in groupTags)
            {
                var ddField = fieldsByTag.FirstOrDefault(x => x.Value.Tag == groupTag);
                if (ddField.Value == null) continue;

                var ddItem = ddGrps.FirstOrDefault(x => x.Key == groupTag);

                var group = new Group(groupTag, 0);
                var jArray = new JArray();
                node.Add(new JProperty($"{ddField.Value.Name}{groupTag}", jArray));

                var groupCount = fieldMap.GetInt(groupTag);
                for (var i = 1; i <= groupCount; i++)
                {
                    fieldMap.GetGroup(i, group);
                    var jObject = new JObject();

                    var subDdGrps = GetDdGrps(ddGrps, groupTag);

                    ConvertFieldMapToJson(dataDictionary, msgType, fieldsByTag, subDdGrps, group, jObject);
                    jArray.Add(jObject);
                }
            }
        }

        public static Dictionary<int, DDGrp> GetDdGrps(Dictionary<int, DDGrp> ddGrps, int tag)
        {
            var dictionary = ddGrps.FirstOrDefault(x => x.Key == tag);

            return dictionary.Value.Groups;
        }

        private static bool CheckIsInGroup(DataDictionary dataDictionary, StringField msgType, IField field, DDGrp ddGrp)
        {
            try
            {
                dataDictionary.CheckIsInGroup(field, ddGrp, msgType.Obj);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}