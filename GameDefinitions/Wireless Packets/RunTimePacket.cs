using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class RunTimePacket : WirelessPacket
    {
        private Position2D _velocity;

        public Position2D Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }
        
        public override byte[] Serialize()
        {
            
            throw new NotImplementedException();
        }

        public override WirelessPacket Deserialize()
        {
            return new RunTimePacket();
        }
    }
}
