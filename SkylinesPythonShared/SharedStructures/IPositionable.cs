using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.API
{
    [Doc("Any object that has the 'position' attribute")]
    public interface IPositionable
    {
        [Doc("Object position")]
        Vector position { get; }
    }
}
