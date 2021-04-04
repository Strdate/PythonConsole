using IronPython.Runtime.Types;
using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkylinesRemotePython
{
    public static class PythonHelp
    {
        [ThreadStatic]
        public static bool NoCache = false;

        [ThreadStatic]
        private static int toStringRecursionDepth;

        public static string RuntimeToString(object obj)
        {
            if(obj == null) {
                return "null";
            }
            string result;
            if(toStringRecursionDepth > 10) {
                return "???";
            }
            try {
                toStringRecursionDepth++;
                NoCache = true;
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

                        string text = Repeat("  ",toStringRecursionDepth) + prop.Name + ": " + valueStr;
                        builder.AppendLine(text);
                    }
                }
                builder.Append(Repeat("  ", toStringRecursionDepth - 1) + "}");
                result = builder.ToString();
            } finally {
                toStringRecursionDepth--;
                NoCache = false;
            }
            return result;
        }

        public static string GetHelp(object obj, bool markdown = false)
        {
            Type type = obj is PythonType pt ? pt.__clrtype__() : (obj is Type tp ? tp : obj.GetType());
            
            var classAtr = (DocAttribute)type.GetCustomAttribute(typeof(DocAttribute));
            string classText = classAtr != null ? classAtr.Description : null;

            var singletonAtr = (SingletonAttribute)type.GetCustomAttribute(typeof(SingletonAttribute));
            string namePrefix = singletonAtr != null ? singletonAtr.Variable + "." : "";

            var methods = type.GetMethods();
            var methodSet = new SortedDictionary<string,string>();
            foreach(var method in methods) {
                var atrs = method.GetCustomAttributes(typeof(DocAttribute), true);
                if(atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    bool isVoid = method.ReturnType == typeof(void);
                    string text;
                    string textWithPrefix;
                    if(markdown) {
                        text = "`" + namePrefix + method.Name + "(" + FormatParams(method) + ")`" + (isVoid ? "" : ": " + FormatClassLink(method.ReturnType, markdown)) + " - " + atr.Description;
                        textWithPrefix = "* " + (method.IsStatic ? "static " : "") + text;
                    } else {
                        text = namePrefix + method.Name + "(" + FormatParams(method) + ")" + (isVoid ? "" : ": " + method.ReturnType.Name) + " - " + atr.Description;
                        textWithPrefix = (method.IsStatic ? "static " : "") + text;
                    }
                    methodSet.Add(text, textWithPrefix);
                }
            }

            var constructors = type.GetConstructors();
            foreach (var ctor in constructors) {
                var atrs = ctor.GetCustomAttributes(typeof(DocAttribute), true);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    string text;
                    string textWithPrefix;
                    if (markdown) {
                        text = "`" + namePrefix + ctor.Name + "(" + FormatParams(ctor) + ")`" + " - " + atr.Description;
                        textWithPrefix = "* static " + text;
                    } else {
                        text = namePrefix + ctor.Name + "(" + FormatParams(ctor) + ")" + " - " + atr.Description;
                        textWithPrefix = "static " + text;
                    }
                    methodSet.Add(text, textWithPrefix);
                }
            }

            var props = type.GetProperties();
            var propSet = new SortedDictionary<string, string>();
            foreach (var prop in props) {
                var atrs = prop.GetCustomAttributes(typeof(DocAttribute), true);
                if (atrs.Length > 0) {
                    DocAttribute atr = atrs[0] as DocAttribute;
                    bool hasSetter = prop.GetSetMethod() != null;
                    string text;
                    string textWithPrefix;
                    if (markdown) {
                        text = "`" + namePrefix + prop.Name + "`: " + FormatClassLink(prop.PropertyType, markdown) + " " + (hasSetter ? "(get/set) " : "") + "- " + atr.Description;
                        textWithPrefix = "* " + (prop.GetGetMethod().IsStatic ? "static " : "") + text;
                    } else {
                        text = namePrefix + prop.Name + ": " + prop.PropertyType.Name + " " + (hasSetter ? "(get/set) " : "") + "- " + atr.Description;
                        textWithPrefix = (prop.GetGetMethod().IsStatic ? "static " : "") + text;
                    }
                    propSet.Add(text, textWithPrefix);
                }
            }

            return FormatClassHeader(type.Name, classText, markdown) + (methodSet.Count > 0 ? FormatH4("Methods:", markdown) + SortedSetToString(methodSet) : "")
                + (propSet.Count > 0 ? FormatH4("Properties:",markdown) + SortedSetToString(propSet) : "");
        }

        private static string FormatClassLink(Type type, bool markdown)
        {
            if(markdown) {
                if(type.Namespace == "SkylinesRemotePython.API" || type.Namespace == "SkylinesPythonShared.API") {
                    return $"[`{type.Name}`](#Class-{type.Name})";
                } else {
                    return $"`{type.Name}`";
                }
            } else {
                return type.Name;
            }
        }

        private static string FormatClassHeader(string className, string classDescription, bool markdown)
        {
            if(markdown) {
                return $"## Class {className}\n\n{(classDescription != null ? classDescription + "\n\n" : "")}";
            } else {
                return $"\n====\nHelp - {className}\n====\n\n{(classDescription != null ? classDescription + "\n\n" : "")}";
            }
        }

        private static string FormatH4(string header, bool markdown)
        {
            if(markdown) {
                return $"#### {header}\n\n";
            } else {
                return $"{header}\n";
            }
        }

        public static string PrintList(IEnumerable collection)
        {
            return "[" + string.Join(", ", Enumerable.ToArray<object>(collection.Cast<object>())) + "]\n" ;
        }

        public static string DumpDoc(bool markdown = false)
        {
            return DumpDocInAssembly(Assembly.GetExecutingAssembly(), markdown) + DumpDocInAssembly(Assembly.Load("SkylinesPythonShared"), markdown);
        }

        private static string DumpDocInAssembly(Assembly assembly, bool markdown = false)
        {
            StringBuilder b = new StringBuilder();
            foreach (Type type in assembly.GetTypes()) {
                if (type.GetCustomAttribute(typeof(DocAttribute), true) != null) {
                    b.AppendLine(GetHelp(type, markdown));
                }
            }
            return b.ToString();
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
            return strB.ToString() + "\n";
        }

        private static string Repeat(string value, int count)
        {
            return new StringBuilder(value.Length * count).Insert(0, value, count).ToString();
        }
    }
}
