using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public interface IGoogleSerializable
    {
        void Read(System.IO.MemoryStream stream);
        void Write(System.IO.MemoryStream stream);
    }
}
