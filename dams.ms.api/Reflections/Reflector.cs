using System;
using System.Reflection;

namespace Dams.ms.auth.Reflections
{
    public class Reflector<T>
    {
        internal static FieldInfo[] GetFieldInfos()
        {
            Type type = typeof(T); // Get type pointer
            FieldInfo[] fields = type.GetFields(); // Obtain all fields
            return fields;
        }

        internal static PropertyInfo[] GetPropertyInfos()
        {
            Type type = typeof(T); // Get type pointer
            PropertyInfo[] properties = type.GetProperties(); //Obtain all properties
            return properties;
        }
    }

    public class PlainReflector
    {
        internal static FieldInfo[] GetFieldInfosFromType(Type type)
        {
            FieldInfo[] fields = type.GetFields(); // Obtain all fields
            return fields;
        }

        internal static PropertyInfo[] GetPropertyInfosFromType(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(); //Obtain all properties
            return properties;
        }
    }
}
