using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesPythonShared
{
    public class DocAttribute : System.Attribute
    {
        public string Description { get; private set; }

        public DocAttribute(string description)
        {
            this.Description = description;
        }
    }

    public class SingletonAttribute : System.Attribute
    {
        public string Variable { get; private set; }

        public SingletonAttribute(string variable)
        {
            this.Variable = variable;
        }
    }

    public class ToStringIgnoreAttribute : System.Attribute
    {
    }
}
