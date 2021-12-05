using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DumbJsonParser
{
    class JsonReader
    {
        private readonly TypeManager typeManager;

        private readonly string text;

        private int p;

        internal JsonReader(string text, TypeManager typeManager)
        {
            this.text = text;
            this.typeManager = typeManager;
        }

        private char ReadNext()
        {
            char c = text[p];
            p++;
            return c;
        }

        internal object Read(string implicitType = null)
        {
            //try {
            return ReadJsonObject(implicitType);
            //} catch(Exception e) {
            //    throw new Exception($"Error at character {p}: {e.Message}", e);
            //}
        }

        private object ReadJsonObject(string typeName = null)
        {
            string arrayType = null;
            bool firstPass = true;

            Expect('{');
            ParsedObject obj = null;
            while (true) {
                bool specialField = false;
                if (text[p] == '}') {
                    p++;
                    if (firstPass) {
                        if (typeName != null) {
                            obj = typeManager.InstantiateObj(typeName);
                            break;
                        } else {
                            throw new Exception("First field in object must be '__type'");
                        }
                    }
                    break;
                }
                string fieldName = ReadString();
                Expect(':');
                object val = ReadJsonType(arrayType);
                if (firstPass) {
                    if (fieldName == "__type") {
                        typeName = (string)val;
                        specialField = true;
                    }
                    if (typeName != null) {
                        obj = typeManager.InstantiateObj(typeName);
                    } else {
                        throw new Exception("First field in object must be '__type'");
                    }
                }
                if (fieldName.StartsWith("__type_")) {
                    arrayType = (string)val;
                    specialField = true;
                }
                if (!specialField) {
                    obj.AssignProperty(fieldName, val);
                }
                firstPass = false;
                SkipComma();
            }
            return obj.GetObject();
        }

        private object ReadJsonType(string implicitType = null)
        {
            SkipWS();
            char nchar = text[p];
            if (nchar == '{') {
                return ReadJsonObject(implicitType);
            } else if (nchar == '[') {
                if (implicitType != null) {
                    return ReadArray(implicitType);
                } else {
                    throw new Exception("Array type was not specified");
                }
            } else if (nchar == '"') {
                return ReadString();
            } else if (Char.IsNumber(nchar)) {
                return ReadNumber();
            } else if (nchar == 't' || nchar == 'f') {
                return ReadBool();
            } else {
                throw new Exception("Expected JSON type");
            }
        }

        private object ReadArray(string type)
        {
            object list = typeManager.InstantiateList(type, out MethodInfo addMethod);
            Expect('[');
            while (true) {
                SkipWS();
                char nchar = text[p];
                if (nchar == ']') {
                    p++;
                    return list;
                }
                addMethod.Invoke(list, new object[] { ReadJsonType(type) });
                SkipComma();
            }
        }

        private object ReadNumber()
        {
            StringBuilder builder = new StringBuilder();
            bool isInteger = true;
            while (true) {
                char cur = text[p];
                if (Char.IsNumber(cur)) {
                    builder.Append(cur);
                    p++;
                } else if (cur == '.') {
                    builder.Append(cur);
                    p++;
                    isInteger = false;
                } else {
                    if (isInteger) {
                        return int.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                    } else {
                        return double.Parse(builder.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        private object ReadBool()
        {
            char cur = text[p];
            if (cur == 't') {
                if (text[p + 1] != 'r' || text[p + 2] != 'u' || text[p + 3] != 'e') {
                    throw new Exception("Expected 'true'");
                } else {
                    p += 4;
                    return true;
                }
            } else {
                if (text[p] != 'f' || text[p + 1] != 'a' || text[p + 2] != 'l' || text[p + 3] != 's' || text[p + 4] != 'e') {
                    throw new Exception("Expected 'false'");
                } else {
                    p += 5;
                    return false;
                }
            }
        }

        private string ReadString()
        {
            bool esc = false;
            StringBuilder builder = new StringBuilder();
            Expect('"');
            while (true) {
                char cur = ReadNext();
                if (esc) {
                    if (cur == '\\' || cur == '"') {
                        builder.Append(cur);
                    } else if (cur == 'n') {
                        builder.Append('\n');
                    } else {
                        throw new Exception("Found unescaped backslash");
                    }
                    esc = false;
                } else {
                    if (cur == '\\') {
                        esc = true;
                        continue;
                    } else if (cur == '"') {
                        return builder.ToString();
                    } else {
                        builder.Append(cur);
                    }
                }
            }
        }

        private char Expect(params char[] chars)
        {
            while (true) {
                char cur = ReadNext();
                if (Char.IsWhiteSpace(cur)) {
                    continue;
                }
                foreach (char el in chars) {
                    if (el == cur) {
                        return el;
                    }
                }
                throw new Exception($"Expected {CharArrayToString(chars)} but got {cur}");
            }
        }

        private void SkipWS()
        {
            while (true) {
                char cur = text[p];
                if (Char.IsWhiteSpace(cur)) {
                    p++;
                    continue;
                }
                return;
            }
        }

        private void SkipComma()
        {
            SkipWS();
            char chr = text[p];
            char comma = ',';
            if (chr == comma) {
                p++;
            }
        }

        private static string CharArrayToString(char[] chars)
        {
            string res = "";
            for (int i = 0; i < chars.Length; i++) {
                if (i == 0) {
                    res = "'" + chars[i].ToString() + "'";
                } else {
                    res = res + " or '" + chars[i].ToString() + "'";
                }
            }
            return res;
        }
    }
}
