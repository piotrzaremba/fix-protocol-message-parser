using System.Text;
using QuickFix;
using QuickFix.DataDictionary;
using QuickFix.Fields;

namespace Fix.QuickFix.Parser
{
    public class ParsableMessage : Message
    {
        public ParsableMessage() {}

        public ParsableMessage(string msgstr) : this(msgstr, true){}
        
        public ParsableMessage(string msgstr, bool validate) : base(msgstr, null, null, validate) { }
        
        public ParsableMessage(string msgstr, DataDictionary dataDictionary, bool validate) : base(msgstr, dataDictionary, validate) { }


        public ParsableMessage(string msgstr, DataDictionary sessionDataDictionary, DataDictionary appDD, bool validate) : base(msgstr, sessionDataDictionary, appDD, validate) { }

        public ParsableMessage(Message src) : base(src) { }

        private static StringBuilder FieldMapToJson(StringBuilder sb, DataDictionary dd, FieldMap fields, bool humanReadableValues)
        {
            IList<int> numInGroupTagList = fields.GetGroupTags();
            IList<IField> numInGroupFieldList = new List<IField>();

            // Non-Group Fields
            foreach (var field in fields)
            {
                if (global::QuickFix.Fields.CheckSum.TAG == field.Value.Tag) 
                {
                    // FIX Json Encoding does not include CheckSum
                    continue;
                }

                if (numInGroupTagList.Contains(field.Value.Tag))
                {
                    // Groups will be handled below.
                    numInGroupFieldList.Add(field.Value);
                    continue; 
                }

                if ((dd != null) && (dd.FieldsByTag.ContainsKey(field.Value.Tag)))
                {
                    sb.Append($"\"{dd.FieldsByTag[field.Value.Tag].Name}{field.Value.Tag}\":");
                    if (humanReadableValues)
                    {
                        if (dd.FieldsByTag[field.Value.Tag].EnumDict.TryGetValue(field.Value.ToString(), out var valueDescription))
                        {
                            sb.Append($"\"{valueDescription}\",");
                        }
                        else
                        {
                            sb.Append($"\"{field.Value}\",");
                        }
                    }
                    else
                    {
                        sb.Append($"\"{field.Value}\",");
                    }
                }
                else
                {
                    sb.Append($"\"{field.Value.Tag}\":");
                    sb.Append($"\"{field.Value}\",");
                }
            }

            // Group Fields
            foreach (var numInGroupField in numInGroupFieldList)
            {
                // The name of the NumInGroup field is the key of the Json list containing the Group items.
                if ((dd != null) && (dd.FieldsByTag.ContainsKey(numInGroupField.Tag)))
                { sb.Append($"\"{dd.FieldsByTag[numInGroupField.Tag].Name}{dd.FieldsByTag[numInGroupField.Tag].Tag}\":[");}
                else
                {     sb.Append($"\"{numInGroupField.Tag}\":[");}

                // Populate the Json list with the Group items.
                for (var counter = 1; counter <= fields.GroupCount(numInGroupField.Tag); counter++)
                {
                    sb.Append("{");
                    FieldMapToJson(sb, dd, fields.GetGroup(counter, numInGroupField.Tag), humanReadableValues);
                    sb.Append("},");
                }

                // Remove trailing comma.
                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                { sb.Remove(sb.Length - 1, 1);}

                sb.Append("],");
            }
            // Remove trailing comma.
            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
            { sb.Remove(sb.Length - 1, 1);}

            return sb;
        }


        /// <summary>
        /// Get a representation of the message as a string in FIX Json Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        /// </summary>
        /// <returns>a Json string</returns>
        public string ToJson()
        {
            return ToJson(this.dataDictionary_, false);
        }


        /// <summary>
        /// Get a representation of the message as a string in FIX Json Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        ///
        /// Per the FIX Json Encoding spec, tags are converted to human-readable form, but values are not.
        /// If you want human-readable values, set humanReadableValues to true.
        /// </summary>
        /// <returns>a Json string</returns>
        public string ToJson(bool humanReadableValues)
        {
            return ToJson(this.dataDictionary_, humanReadableValues);
        }

        /// <summary>
        /// Get a representation of the message as a string in FIX Json Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        ///
        /// Per the FIX Json Encoding spec, tags are converted to human-readable form, but values are not.
        /// If you want human-readable values, set humanReadableValues to true.
        /// </summary>
        /// <returns>a Json string</returns>
        public string ToJson(DataDictionary dd, bool humanReadableValues)
        {
            var sb = new StringBuilder().Append(@"{").Append($"\"Header\":{{");
            FieldMapToJson(sb, dd, Header, humanReadableValues).Append($"}},\"Body\":{{");
            FieldMapToJson(sb, dd, this, humanReadableValues).Append($"}},\"Trailer\":{{");
            FieldMapToJson(sb, dd, Trailer, humanReadableValues).Append($"}}}}");
            return sb.ToString();
        }
    }
}