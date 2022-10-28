using System.Diagnostics;
using System.Text;
using QuickFix;
using QuickFix.DataDictionary;
using QuickFix.Fields;

namespace Fix.Parser.Experiment
{
    public class ParsableMessage : Message
    {
        public ParsableMessage() {}

        public ParsableMessage(string msgstr) : this(msgstr, true){}
        
        public ParsableMessage(string msgstr, bool validate) : base(msgstr, null, null, validate) { }
        
        public ParsableMessage(string msgstr, DataDictionary dataDictionary, bool validate) : base(msgstr, dataDictionary, validate) { }


        public ParsableMessage(string msgstr, DataDictionary sessionDataDictionary, DataDictionary appDD, bool validate) : base(msgstr, sessionDataDictionary, appDD, validate) { }

        public ParsableMessage(Message src) : base(src) { }


        private static StringBuilder FieldMapToJSON(StringBuilder sb, DataDictionary dd, FieldMap fields, bool humanReadableValues)
        {
            IList<int> numInGroupTagList = fields.GetGroupTags();
            IList<IField> numInGroupFieldList = new List<IField>();
            var valueDescription = string.Empty;

            // Non-Group Fields
            foreach (var field in fields)
            {
                if (dd.FieldsByTag[field.Value.Tag].Name == "NoSides")
                {
                    Debug.WriteLine("");
                }

                if (QuickFix.Fields.CheckSum.TAG == field.Value.Tag)
                    continue; // FIX JSON Encoding does not include CheckSum

                if (numInGroupTagList.Contains(field.Value.Tag))
                {
                    numInGroupFieldList.Add(field.Value);
                    continue; // Groups will be handled below
                }

                if ((dd != null) && (dd.FieldsByTag.ContainsKey(field.Value.Tag)))
                {
                    sb.Append("\"" + dd.FieldsByTag[field.Value.Tag].Name + field.Value.Tag + "\":");
                    if (humanReadableValues)
                    {
                        if (dd.FieldsByTag[field.Value.Tag].EnumDict.TryGetValue(field.Value.ToString(), out valueDescription))
                        {
                            sb.Append("\"" + valueDescription + "\",");
                        }
                        else
                        {
                            sb.Append("\"" + field.Value + "\",");
                        }
                    }
                    else
                    {
                        sb.Append("\"" + field.Value + "\",");
                    }
                }
                else
                {
                    sb.Append("\"" + field.Value.Tag + "\":");
                    sb.Append("\"" + field.Value + "\",");
                }
            }

            // Group Fields
            foreach (var numInGroupField in numInGroupFieldList)
            {
                // The name of the NumInGroup field is the key of the JSON list containing the Group items
                if ((dd != null) && (dd.FieldsByTag.ContainsKey(numInGroupField.Tag)))
                    sb.Append("\"" + dd.FieldsByTag[numInGroupField.Tag].Name + dd.FieldsByTag[numInGroupField.Tag].Tag + "\":[");
                else
                    sb.Append("\"" + numInGroupField.Tag + "\":[");

                // Populate the JSON list with the Group items
                for (var counter = 1; counter <= fields.GroupCount(numInGroupField.Tag); counter++)
                {
                    sb.Append("{");
                    FieldMapToJSON(sb, dd, fields.GetGroup(counter, numInGroupField.Tag), humanReadableValues);
                    sb.Append("},");
                }

                // Remove trailing comma
                if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                    sb.Remove(sb.Length - 1, 1);

                sb.Append("],");
            }
            // Remove trailing comma
            if (sb.Length > 0 && sb[sb.Length - 1] == ',')
                sb.Remove(sb.Length - 1, 1);

            return sb;
        }


        /// <summary>
        /// Get a representation of the message as a string in FIX JSON Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        /// </summary>
        /// <returns>a JSON string</returns>
        public string ToJSON()
        {

            return ToJSON(this.dataDictionary_, false);
        }


        /// <summary>
        /// Get a representation of the message as a string in FIX JSON Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        ///
        /// Per the FIX JSON Encoding spec, tags are converted to human-readable form, but values are not.
        /// If you want human-readable values, set humanReadableValues to true.
        /// </summary>
        /// <returns>a JSON string</returns>
        public string ToJSON(bool humanReadableValues)
        {
            return ToJSON(this.dataDictionary_, humanReadableValues);
        }

        /// <summary>
        /// Get a representation of the message as a string in FIX JSON Encoding.
        /// See: https://github.com/FIXTradingCommunity/fix-json-encoding-spec
        ///
        /// Per the FIX JSON Encoding spec, tags are converted to human-readable form, but values are not.
        /// If you want human-readable values, set humanReadableValues to true.
        /// </summary>
        /// <returns>a JSON string</returns>
        public string ToJSON(DataDictionary dd, bool humanReadableValues)
        {
            var sb = new StringBuilder().Append("{").Append("\"Header\":{");
            FieldMapToJSON(sb, dd, Header, humanReadableValues).Append("},\"Body\":{");
            FieldMapToJSON(sb, dd, this, humanReadableValues).Append("},\"Trailer\":{");
            FieldMapToJSON(sb, dd, Trailer, humanReadableValues).Append("}}");
            return sb.ToString();
        }
    }
}