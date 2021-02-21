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
    public static class Help
    {
        public static string GetHelp(object obj)
        {
            Type type = obj is PythonType pt ? pt.__clrtype__() : obj.GetType();
            
            var classAtr = (DocAttribute)type.GetCustomAttribute(typeof(DocAttribute));
            string classText = classAtr != null ? classAtr.Description + "\n" : "";

            var methods = type.GetMethods();
            var methodSet = new SortedDictionary<string,string>();
            foreach(var method in methods) {
                var atrs = method.GetCustomAttributes(typeof(DocAttribute), false);
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
                var atrs = ctor.GetCustomAttributes(typeof(DocAttribute), false);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    string text = ctor.Name + "(" + FormatParams(ctor) + ")" + " - " + atr.Description;
                    methodSet.Add(text, "static " + text);
                }
            }

            var props = type.GetProperties();
            var propSet = new SortedDictionary<string, string>();
            foreach (var prop in props) {
                var atrs = prop.GetCustomAttributes(typeof(DocAttribute), false);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    string text = (type == typeof(GameAPI) ? "game." : "") + prop.Name + ": " + prop.PropertyType.Name + " - " + atr.Description;
                    string textPrefix = (prop.GetGetMethod().IsStatic ? "static " : "") + text;
                    propSet.Add(text, textPrefix);
                }
            }

            return "\n====\nHelp (" + type.Name + ")\n" + classText + "====\n" + (methodSet.Count > 0 ? "Methods:\n" + SortedSetToString(methodSet) : "")
                + (propSet.Count > 0 ? "Properties:\n" + SortedSetToString(propSet) : "");
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
