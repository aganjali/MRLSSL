using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using System.IO;
using Meta.Numerics.Matrices;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Representig a Object and its properties in small size world
    /// </summary>
    public class SingleObjectState:ICloneable
    {
        /// <summary>
        /// this variable uses in path planning for making KD tree
        /// </summary>
        public SingleObjectState ParentState;
        public bool Sensore;
        public int BatteryLife;
        public int SequenceNumber;
        public bool IsShown = true;
        public ObjectType Type;
        public Position2D Location;
        public Vector2D Speed;
        public Vector2D Acceleration;
        public bool? ChangedInSimulutor;
        public double Opacity = 1;
        public double btimestamp;
        public RectangularMatrix BVar;
        /// <summary>
        ///-Pi <= Angle <= Pi 
        /// </summary>
        public float? Angle;
        public float? AngularSpeed;
        public double TimeOfCapture;
        /// <summary>
        /// Construct a copy object of Sibgle Object
        /// </summary>
        /// <param name="From">Target for copy</param>
        public SingleObjectState(SingleObjectState From)
        {
            this.Type = From.Type;
            this.Location = From.Location;
            this.Speed = From.Speed;
            this.Acceleration = From.Acceleration;
            this.Angle = From.Angle;
            this.AngularSpeed = From.AngularSpeed;
        }
        /// <summary>
        /// Set Opacity of Single Object State
        /// </summary>
        /// <param name="From"></param>
        /// <param name="opacity"></param>
        public SingleObjectState(SingleObjectState From ,double opacityVal)
        {
            this.Opacity = opacityVal;
            this.Type = From.Type;
            this.Location = From.Location;
            this.Speed = From.Speed;
            this.Acceleration = From.Acceleration;
            this.Angle = From.Angle;
            this.AngularSpeed = From.AngularSpeed;
        }

        public SingleObjectState()
        {
            //Acceleration = new Vector2D();
        }
      
        /// <summary>
        /// Construct from its properties
        /// </summary>
        /// <param name="Type">OurRobot,Opponent,Ball</param>
        /// <param name="Location">Position2D of Robot</param>
        /// <param name="Speed">Vector2D of Speed</param>
        /// <param name="Acceleration">Vector2D of Acceleration</param>
        /// <param name="Angle">Angle of Object</param>
        /// <param name="AngularSpeed">Speed about its center</param>
        public SingleObjectState(ObjectType Type, Position2D Location, Vector2D Speed, Vector2D Acceleration, float? Angle, float? AngularSpeed)
        {
            this.Type = Type;
            this.Location = Location;
            this.Speed = Speed;
            this.Acceleration = Acceleration;
            this.Angle = Angle;
            this.AngularSpeed = AngularSpeed;
        }

        public SingleObjectState(Position2D Location, Vector2D Speed, float? Angle)
        {
            this.Location = Location;
            this.Speed = Speed;
            this.Angle = Angle;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    /// <summary>
    /// Type of objects
    /// </summary>
    public enum ObjectType
    {
        Ball,
        OurRobot,
        Opponent
    }

}

