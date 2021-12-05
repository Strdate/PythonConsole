using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DumbJsonParser
{
    class TypeManager
    {
        private static Dictionary<Type, TypeHandlingRecord> typeHandlingRecords = new Dictionary<Type, TypeHandlingRecord>();

        private static Dictionary<string, Type> types = new Dictionary<string, Type>();

        private static Dictionary<string, ListTypeRecord> listTypes = new Dictionary<string, ListTypeRecord>();

        internal static Dictionary<string, Dictionary<string,PropertyInfo>> properties = new Dictionary<string, Dictionary<string, PropertyInfo>>();

        internal ParsedObject InstantiateObj(string name)
        {
            Type t = TypeByName(name);
            return new ParsedObject(Activator.CreateInstance(t), name);
        }

        internal object InstantiateList(string name, out MethodInfo addMethod)
        {
            ListTypeRecord record = ListTypeByName(name);
            addMethod = record.addMethod;
            return Activator.CreateInstance(record.type);
        }

        internal TypeHandlingRecord GetTypeHandlingRecord(Type type)
        {
            if(!typeHandlingRecords.TryGetValue(type, out TypeHandlingRecord record)) {
                if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                    record.handleType = HandleType.List;
                    record.genArgument = type.GetGenericArguments()[0];
                } else if(type == typeof(string)) {
                    record.handleType = HandleType.ToStringEscaped;
                } else if (type == typeof(int)) {
                    record.handleType = HandleType.ToStringNaked;
                } else if (type == typeof(double)) {
                    record.handleType = HandleType.DecimalNum;
                } else if (type == typeof(bool)) {
                    record.handleType = HandleType.ToStringLower;
                } else {
                    record.handleType = HandleType.StructuredObject;
                }
                typeHandlingRecords[type] = record;
            }
            return record;
        }

        private static ListTypeRecord ListTypeByName(string name)
        {
            if (!listTypes.TryGetValue(name, out ListTypeRecord record)) {
                Type t = typeof(List<>);
                Type[] typeArgs = { TypeByName(name) };
                t = t.MakeGenericType(typeArgs);
                MethodInfo addMethod = t.GetMethod("Add");
                record.type = t;
                record.addMethod = addMethod;
                listTypes[name] = record;
            }
            return record;
        }

        private static Type TypeByName(string name)
        {
            if(!types.TryGetValue(name, out Type type)) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
                    foreach (var t in assembly.GetTypes()) {
                        if (t.Name == name || t.FullName == name) {
                            if(t.GetCustomAttributes(
                                typeof(SerializableAttribute), true
                                ).FirstOrDefault() == null) {
                                throw new Exception($"Class {t.Name} is not serializable");
                            }
                            InitProperties(t, name);
                            types[name] = t;
                            return t;
                        }
                    }
                }
                throw new Exception($"Failed to find type '{name}'");
            }
            return type;
        }

        private static void InitProperties(Type type, string typeName)
        {
            var dict = new Dictionary<string, PropertyInfo>();
            foreach(var prop in type.GetProperties()) {
                Type propType = prop.GetType();
                if(prop.GetCustomAttributes(
                    typeof(NonSerializedAttribute), true
                    ).FirstOrDefault() == null
                    /*&& (!propType.IsPrimitive || (propType == typeof(int) || propType == typeof(double)))*/) {
                    dict.Add(prop.Name, prop);
                }
            }
            properties.Add(typeName, dict);
        }

        internal static PropertyInfo GetProperty(string typeName, string propertyName)
        {
            if (!properties.TryGetValue(typeName, out Dictionary<string, PropertyInfo> propDict)) {
                TypeByName(typeName);
                propDict = properties[typeName];
            }
            return propDict[propertyName];
        }

        internal Dictionary<string, PropertyInfo>.ValueCollection GetProperties(string typeName)
        {
            TypeByName(typeName);
            return properties[typeName].Values;
        }
    }

    class ParsedObject
    {
        private object inner;
        private string typeString;

        internal ParsedObject(object inner, string typeString)
        {
            this.inner = inner;
            this.typeString = typeString;
        }

        internal void AssignProperty(string name, object obj)
        {
            PropertyInfo prop = TypeManager.GetProperty(typeString, name);
            if(prop != null) {
                prop.SetValue(inner, obj, null);
            }
        }

        internal object GetObject() => inner;
    }

    struct ListTypeRecord
    {
        internal Type type;
        internal MethodInfo addMethod;
    }

    struct TypeHandlingRecord
    {
        internal HandleType handleType;
        internal Type genArgument;
    }

    enum HandleType
    {
        ToStringEscaped,
        ToStringNaked,
        ToStringLower,
        DecimalNum,
        List,
        StructuredObject
    }
}
