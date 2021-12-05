using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DumbJsonParser
{
    class JsonWriter
    {
        private readonly TypeManager typeManager;
        private readonly object sourceObj;
        private readonly StringBuilder b = new StringBuilder();

        internal JsonWriter(TypeManager typeManager, object obj)
        {
            this.typeManager = typeManager;
            this.sourceObj = obj;
        }

        internal string Write()
        {
            WriteJsonType(sourceObj, null, sourceObj.GetType());
            return b.ToString();
        }

        private void WriteJsonType(object obj, string propName = null, Type implicitType = null)
        {
            Type t = obj.GetType();
            TypeHandlingRecord handleType = typeManager.GetTypeHandlingRecord(t);

            if (handleType.handleType == HandleType.List) {
                WriteList(obj, propName, t, handleType.genArgument);
            } else {
                if (propName != null) {
                    b.Append($"\"{propName}\":");
                }

                if (handleType.handleType == HandleType.StructuredObject) {
                    WriteJsonObject(obj, t, t == implicitType);
                } else if (handleType.handleType == HandleType.ToStringEscaped) {
                    EscapeStr(obj.ToString());
                } else if (handleType.handleType == HandleType.ToStringNaked) {
                    EscapeStr(obj.ToString());
                } else if (handleType.handleType == HandleType.ToStringLower) {
                    b.Append($"{obj.ToString().ToLower()}");
                } else if (handleType.handleType == HandleType.DecimalNum) {
                    b.Append($"{obj.ToString().Replace(',', '.')}");
                }
            }


        }

        private void WriteList(object obj, string propName, Type type, Type genericArg)
        {
            b.Append($"\"__type_{propName}\": \"{genericArg.Name}\",");
            System.Collections.IEnumerable list = obj as System.Collections.IEnumerable;
            b.Append($"\"{propName}\": [");
            bool isFirst = true;
            foreach (var item in list) {
                if (!isFirst) {
                    b.Append(",");
                } else {
                    isFirst = false;
                }
                WriteJsonType(item, null, genericArg);
            }
            b.Append("]");
        }

        private void WriteJsonObject(object obj, Type type, bool implicitType)
        {
            b.Append("{");
            bool isFirst = true;
            if (!implicitType) {
                isFirst = false;
                b.Append($"\"__type\": \"{type.Name}\"");
            }
            foreach (var prop in typeManager.GetProperties(type.Name)) {
                object val = prop.GetValue(obj, null);
                if (val != null) {
                    if (!isFirst) {
                        b.Append(",");
                    } else {
                        isFirst = false;
                    }
                    WriteJsonType(val, prop.Name);
                }

            }
            b.Append("}");
        }

        private void EscapeStr(string s)
        {
            b.Append('"');
            if (s == null || s.Length == 0) {
                b.Append('"');
                return;
            }
            char c;
            int i;

            for (i = 0; i < s.Length; i += 1) {
                c = s[i];
                switch (c) {
                    case '\\':
                    case '"':
                        b.Append('\\');
                        b.Append(c);
                        break;
                    case '\n':
                        b.Append("\\n");
                        break;
                    default:
                        b.Append(c);
                        break;
                }
            }
            b.Append('"');
        }
    }
}
