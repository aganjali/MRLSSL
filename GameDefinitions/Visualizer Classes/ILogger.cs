using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    interface ILogger
    {
        void Serialize(out MemoryStream ms);
        ILogger Deserialize(MemoryStream ms);
    }
}
