using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StillDesign.PhysX.MathPrimitives;
using StillDesign.PhysX;
using System.Threading;
using System.Drawing;

namespace Simulator
{
    public class Robot
    {
        private Vector3 _maxSpeed = new Vector3(3.5f, 0, 3.5f);
        private Vector3 _maxAcceleration = new Vector3(0.1f, 0, 0.1f);
        private Vector3 _maxDeceleration = new Vector3(-0.3f, 0, 0.3f);
        private float _robotRadi = 0.09f;
        private float _robotMass = 0.15f;
        private float _wheelRadius = 0.025f;
        private float _wheelOffset = 0.01f;
        public uint ID { get; private set; }
        private Actor _robotActor;
        public Color color { get; private set; }
        public Actor RobotActor
        {
            get { return _robotActor; }
            set { _robotActor = value; }
        } 
        private Model _robotModel;

        //wheel Properties
        private WheelShape LeftFront
        {
            get;
            set;
        }
        private WheelShape LeftRear
        {
            get;
            set;
        }
        private WheelShape RightFront
        {
            get;
            set;
        }
        private WheelShape RightRear
        {
            get;
            set;
        }
        
        public Robot(MRLSimulator simulator, uint ID, float mass,Color color,Actor actor)
        {
            this.ID = ID;
            actor.Name = ID.ToString();
            this.color = color;
            this._robotMass = mass;
            //this._robotModel = model;
            LoadRobotPhysX(simulator, actor);
        }

        private void LoadRobotPhysX(MRLSimulator simulator, Actor actor)
        {
           // actor.SetCenterOfMassOffsetGlobalPose(Matrix.Translation(_globalPose));
            _robotActor = actor;
            _robotActor.BodyFlags.FrozenRotationZ = true;
            _robotActor.BodyFlags.FrozenRotationX = true;
            _robotActor.SetCenterOfMassOffsetLocalPosition(new Vector3(0, 0.05f, 0));//center of Mass 5 cm from center
            WheelShapeFlag wflag = WheelShapeFlag.AxleSpeedOverride;
          
            //Creating Wheels
            #region CreatingWheels
            TireFunctionDescription longtfd = new TireFunctionDescription()
            {
                ExtremumSlip = 0.0001f,
                ExtremumValue = 2f,
                AsymptoteSlip = 0.0002f,
                AsymptoteValue = 0.01f,
                StiffnessFactor = 1000000f
            };


            TireFunctionDescription laterntfd = new TireFunctionDescription()
            {
                ExtremumSlip = 1f,
                ExtremumValue = 0.002f,
                AsymptoteSlip = 2.0f,
                AsymptoteValue = 0.01f,
                StiffnessFactor = 1000000.0f
            };

            WheelShapeDescription leftFrontDesc = new WheelShapeDescription()
            {
                Radius = (float)_wheelRadius,
                SuspensionTravel = 0,
                LocalPosition = new Vector3(((float)(_robotRadi * Math.Sin(ToRadians(81)))), (float)(_wheelRadius - _wheelOffset), ((float)(_robotRadi * Math.Cos(ToRadians(81))))),
                LateralTireForceFunction = laterntfd,
                LongitudalTireForceFunction = longtfd,
                LocalRotation = Matrix.RotationY(ToRadians(-9)),//-9
                Flags = wflag,
                Mass = 0.03f
            };

            WheelShapeDescription rightFrontDesc = new WheelShapeDescription()
            {
                Radius = (float)_wheelRadius,
                SuspensionTravel = 0,
                LocalPosition = new Vector3(((float)(_robotRadi * Math.Sin(ToRadians(-81)))), (float)(_wheelRadius - _wheelOffset), ((float)(_robotRadi * Math.Cos(ToRadians(-81))))),
                LateralTireForceFunction = laterntfd,
                LongitudalTireForceFunction = longtfd,
                LocalRotation = Matrix.RotationY(ToRadians(9)),//9
                Flags = wflag,
                //SkinWidth = 0.1f
                Mass = 0.03f
                //Flags = WheelShapeFlag.InputLaterialSlipVelocity | WheelShapeFlag.InputLongutudalSlipVelocity
            };

            WheelShapeDescription leftRearDesc = new WheelShapeDescription()
            {
                Radius = (float)_wheelRadius,
                SuspensionTravel = 0,
                LocalPosition = new Vector3(((float)(_robotRadi * Math.Sin(ToRadians(135)))), (float)(_wheelRadius - _wheelOffset), ((float)(_robotRadi * Math.Cos(ToRadians(135))))),
                LateralTireForceFunction = laterntfd,
                LongitudalTireForceFunction = longtfd,
                LocalRotation = Matrix.RotationY(ToRadians(45)),//45
                Flags = wflag,
                //SkinWidth = 0.01f,
                Mass = 0.03f
                //Flags = WheelShapeFlag.InputLaterialSlipVelocity | WheelShapeFlag.InputLongutudalSlipVelocity

            };

            WheelShapeDescription rightRearDesc = new WheelShapeDescription()
            {
                Radius = (float)_wheelRadius,
                SuspensionTravel = 0,
                LocalPosition = new Vector3(((float)(_robotRadi * Math.Sin(ToRadians(-135)))), (float)(_wheelRadius - _wheelOffset), ((float)(_robotRadi * Math.Cos(ToRadians(-135))))),
                LateralTireForceFunction = laterntfd,

                LongitudalTireForceFunction = longtfd,
                LocalRotation = Matrix.RotationY(ToRadians(-45)),//-45
                Flags = wflag,
                //SkinWidth = 0.01f,,
                Mass = 0.03f
                //Flags = WheelShapeFlag.InputLaterialSlipVelocity | WheelShapeFlag.InputLongutudalSlipVelocity
            };
            #endregion
            //adding to robot

          //  Monitor.Enter(_robotActor);
          //  Monitor.Enter(this);
            this.LeftFront = _robotActor.CreateShape(leftFrontDesc) as WheelShape;

            this.LeftRear = _robotActor.CreateShape(leftRearDesc) as WheelShape;

            this.RightFront = _robotActor.CreateShape(rightFrontDesc) as WheelShape;

            this.RightRear = _robotActor.CreateShape(rightRearDesc) as WheelShape;
          //  Monitor.Exit(_robotActor);
          //  Monitor.Exit(this);
        }

        public void SetMotorSpeed(Vector3 speed, double AngularVelocity)
        {
            
            double leftFront, leftRear, rightFront, rightRear;
            // speed = new Vector3(0.1f, 0, 0);

            double w1_speed = (-20.947655 * speed.X) + (32.25656 * speed.Z) + (3.1154 * AngularVelocity);
            double w2_speed = (27.1964 * speed.X) + (27.1964 * speed.Z) + (3.1154 * AngularVelocity);
            double w3_speed = (-20.947655 * speed.X) - (32.25656 * speed.Z) + (3.1154 * AngularVelocity);
            double w4_speed = (27.1964 * speed.X) - (27.1964 * speed.Z) + (3.1154 * AngularVelocity);

            leftFront = -w3_speed / 10;
            leftRear = -w4_speed / 10;
            rightFront = w1_speed / 10;
            rightRear = w2_speed / 10;

            this.RightFront.AxleSpeed = (float)rightFront;
            this._robotActor.WakeUp();
             //this.RightFront.Actor.WakeUp();
            this.RightRear.AxleSpeed = (float)rightRear;
            this._robotActor.WakeUp();
            //  this.RightRear.Actor.WakeUp();
            this.LeftFront.AxleSpeed = (float)leftFront;
            this._robotActor.WakeUp();
            //  this.LeftFront.Actor.WakeUp();
            this.LeftRear.AxleSpeed = (float)leftRear;
            this._robotActor.WakeUp();
             
           // this.RobotActor.AddForce(new Vector3(0.003f, 0, 0.003f), ForceMode.VelocityChange);
            //this.RobotActor.WakeUp();
        }

        Vector3 LastRobotSpeed = new Vector3();
        public void SetRobotSpeed(Vector3 Speed, double AngularV)
        {
            float elapsedTime = 0.016f;
            elapsedTime = 0.016f;

            double ax = (Speed.X - LastRobotSpeed.X) / elapsedTime;
            double az = (Speed.Z - LastRobotSpeed.Z) / elapsedTime;


            //limit speed
            if (Math.Abs(Speed.X) > Math.Abs(_maxSpeed.X))
            {
                Speed.X = _maxSpeed.X * Math.Sign(Speed.X);
            }

            if (Math.Abs(Speed.Z) > Math.Abs(_maxSpeed.Z))
            {
                Speed.Z = _maxSpeed.Z * Math.Sign(Speed.Z);
            }

            Vector4 temp = Vector3.Transform(Speed, Matrix.RotationY(ToRadians(-GetAngle(this._robotActor.GlobalOrientationQuat))));
            Speed = new Vector3(temp.X, temp.Y, temp.Z);



            this._robotActor.LinearVelocity = Speed;
            this._robotActor.AngularVelocity = new Vector3(0, (float)AngularV, 0);
            this._robotActor.WakeUp();
            LastRobotSpeed = this._robotActor.LinearVelocity;
        }


        public void SetRobotPosition(Vector3 Position,float Angle)
        {
            _robotActor.GlobalOrientationQuat = Quaternion.RotationAxis(Vector3.UnitY, (float)(Math.PI * Angle) / 180.0f);
            _robotActor.GlobalPosition = Position;
        }

        public Vector3 LimitSpeeds(Vector3 CurrentSpeed)
        {
            float angle = GetAngle(_robotActor.GlobalOrientationQuat);
            return Vector3.Zero;

        }


        private float GetAngle(Quaternion q1)
        {
            ///** assumes q1 is a normalised quaternion */
            double test = q1.X * q1.Y + q1.Z * q1.W;
            double heading = 0, attitude = 0, bank = 0;
            if (test > 0.499)
            { // singularity at north pole
                heading = 2 * Math.Atan2(q1.W, q1.W);
                attitude = Math.PI / 2;
                bank = 0;
                return (float)heading;
            } if (test < -0.499)
            { // singularity at south pole
                heading = -2 * Math.Atan2(q1.X, q1.W);
                attitude = -Math.PI / 2;
                bank = 0;
                return (float)heading;
            } double sqx = q1.X * q1.X; double sqy = q1.Y * q1.Y; double sqz = q1.Z * q1.Z;
            heading = Math.Atan2(2 * q1.Y * q1.W - 2 * q1.X * q1.Z, 1 - 2 * sqy - 2 * sqz);
            attitude = Math.Asin(2 * test);

            heading *= 180 / Math.PI;
            bank *= 180 / Math.PI;
            attitude *= 180 / Math.PI;

            return (float)heading;
        }



        float ToRadians(double deg)
        {
            return (float)((float)deg * Math.PI) / 180f;
        }

    }
}
