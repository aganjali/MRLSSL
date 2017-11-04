using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwarmDefence
{
    class SpecificRandomGenerator
    {
        Random rndGenP = new Random();
       /// <summary>
       /// Random Position generator for goalie
       /// </summary>
       /// <returns></returns>
        public SwarmDefence.ParticleSwarm.Position2DPSO GoalieRandomGenerator()
        {
            double radiusP = rndGenP.NextDouble();
            double angleP = rndGenP.Next(90, 270);
            angleP = (angleP * Math.PI) / 180;
            SwarmDefence.ParticleSwarm.Vector2DPSO randomvectorP = FromAngleSize(angleP, radiusP);
            SwarmDefence.ParticleSwarm.Position2DPSO randomPosP =new ParticleSwarm.Position2DPSO( GoalCenterP.X + randomvectorP.X ,GoalCenterP.Y + randomvectorP.Y ) ;
            
            return randomPosP ;
        }
        public SwarmDefence.ParticleSwarm.Vector2DPSO FromAngleSize(double Angle, double Size)
        {
            //Vector2D p = new Vector2D(Size * Math.Cos(Angle), Size * Math.Sin(Angle)); 
            return new SwarmDefence.ParticleSwarm.Vector2DPSO(Size * Math.Cos(Angle), Size * Math.Sin(Angle));
        }

        /// <summary>
        /// Random position generator for Defenders
        /// </summary>
        /// <returns></returns>
        public SwarmDefence.ParticleSwarm.Position2DPSO DefenderRandomGenerator()
        {
            double radiusP = rndGenP.NextDouble()+1;
            double angleP = rndGenP.Next(90, 270);
            angleP = (angleP * Math.PI) / 180;

            SwarmDefence.ParticleSwarm.Vector2DPSO randomvectorP =FromAngleSize(angleP, radiusP);
            SwarmDefence.ParticleSwarm.Position2DPSO randomPosP =new ParticleSwarm.Position2DPSO( GoalCenterP.X + randomvectorP.X ,GoalCenterP.Y + randomvectorP.Y ) ;
            if (randomPosP.X > 4.045 ||Math.Sqrt(((randomPosP.X -GoalCenterP.X )*(randomPosP.X -GoalCenterP.X))+((randomPosP.Y -GoalCenterP.Y)*(randomPosP.Y -GoalCenterP.Y))) <1)
                radiusP = 0;
            return randomPosP;
        }
        SwarmDefence.ParticleSwarm.Position2DPSO GoalCenterP = new SwarmDefence.ParticleSwarm.Position2DPSO(4.045,0);
        

    }
}
