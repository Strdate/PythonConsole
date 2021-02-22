using IronPython.Runtime.Types;
using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkylinesRemotePython
{
    public static class PythonHelp
    {
        [ThreadStatic]
        private static int toStringRecursionDepth;

        public static string RuntimeToString(object obj)
        {
            string result;
            if(toStringRecursionDepth > 10) {
                return "???";
            }
            try {
                toStringRecursionDepth++;
                Type type = obj.GetType();
                var props = type.GetProperties();
                var builder = new StringBuilder();
                builder.AppendLine(type.Name + " {");

                foreach (var prop in props) {
                    var docAtr = prop.GetCustomAttribute(typeof(DocAttribute), true);
                    var noToStrAtr = prop.GetCustomAttribute(typeof(ToStringIgnoreAttribute), true);
                    if (docAtr != null && noToStrAtr == null) {
                        object value = null;
                        bool error = false;
                        try {
                            value = prop.GetValue(obj);
                        }
                        catch { error = true; }

                        Type valueType = value?.GetType();

                        string valueStr;
                        if(error) {
                            valueStr = "error";
                        } else if(value == null) {
                            valueStr = "null";
                        } else if(valueType.Namespace == "System") {
                            valueStr = value.ToString();
                        } else if(typeof(ISimpleToString).IsAssignableFrom(valueType)) {
                            valueStr = ((ISimpleToString)value).SimpleToString();
                        } else {
                            valueStr = valueType.Name;
                        }

                        string text = "  " + prop.Name + ": " + valueStr;
                        builder.AppendLine(text);
                    }
                }
                builder.Append("}");
                result = builder.ToString();
            } finally {
                toStringRecursionDepth--;
            }
            return result;
        }

        public static string GetHelp(object obj)
        {
            Type type = obj is PythonType pt ? pt.__clrtype__() : obj.GetType();
            
            var classAtr = (DocAttribute)type.GetCustomAttribute(typeof(DocAttribute));
            string classText = classAtr != null ? classAtr.Description + "\n\n" : "";

            var methods = type.GetMethods();
            var methodSet = new SortedDictionary<string,string>();
            foreach(var method in methods) {
                var atrs = method.GetCustomAttributes(typeof(DocAttribute), true);
                if(atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    bool isVoid = method.ReturnType == typeof(void);
                    string text = (type == typeof(GameAPI) ? "game." : "") + method.Name + "(" + FormatParams(method) + ")" + (isVoid ? "" : ": " + method.ReturnType.Name) + " - " + atr.Description;
                    string textPrefix = (method.IsStatic ? "static " : "") + text;
                    methodSet.Add(text, textPrefix);
                }
            }

            var constructors = type.GetConstructors();
            foreach (var ctor in constructors) {
                var atrs = ctor.GetCustomAttributes(typeof(DocAttribute), true);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    string text = ctor.Name + "(" + FormatParams(ctor) + ")" + " - " + atr.Description;
                    methodSet.Add(text, "static " + text);
                }
            }

            var props = type.GetProperties();
            var propSet = new SortedDictionary<string, string>();
            foreach (var prop in props) {
                var atrs = prop.GetCustomAttributes(typeof(DocAttribute), true);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    string text = (type == typeof(GameAPI) ? "game." : "") + prop.Name + ": " + prop.PropertyType.Name + " - " + atr.Description;
                    string textPrefix = (prop.GetGetMethod().IsStatic ? "static " : "") + text;
                    propSet.Add(text, textPrefix);
                }
            }

            return "\n====\nHelp (" + type.Name + ")\n====\n\n" + classText + (methodSet.Count > 0 ? "Methods:\n" + SortedSetToString(methodSet) : "")
                + (propSet.Count > 0 ? "\nProperties:\n" + SortedSetToString(propSet) + "\n": "");
        }

        private static string FormatParams(MethodBase method)
        {
            var paramStrings = method.GetParameters().Select(param => param.ParameterType.Name + (param.IsOptional ? "?" : "") + " " + param.Name + (param.IsOptional && param.DefaultValue != null ? " = " +
                param.DefaultValue : ""));
            return string.Join(", ", paramStrings);
        }

        private static string SortedSetToString(SortedDictionary<string, string> dict)
        {
            StringBuilder strB = new StringBuilder();
            foreach(var pair in dict) {
                strB.AppendLine(pair.Value);
            }
            return strB.ToString();
        }
    }
}
