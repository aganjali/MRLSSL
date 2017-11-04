using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using Meta.Numerics.Matrices;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.Planning.MotionPlanner
{
    public class AdaptiveTunner
    {
        VelocityCoefCalculaterBase[] vcc = new VelocityCoefCalculaterBase[PIDParameters.PIDModuleCount];
        PID[] pid = new PID[PIDParameters.PIDModuleCount];
        double[] velCoef = new double[PIDParameters.PIDModuleCount];

        public double[] VelCoef
        {
            get { return velCoef; }
        }
        double minCoefTresh = 0.4;
        public double coefResetValue = 0.7;
        public double minDistTresh = 0.08;
        public double minAngleTresh = 0.39;
        public double minVelTresh = 0.5;
        public double resetCoefTresh = 0.39;

        public AdaptiveTunner()
        {
            vcc[0] = new VelocityCoefCalculatorPos(minDistTresh, minVelTresh, coefResetValue);
            vcc[1] = new VelocityCoefCalculatorPos(minDistTresh, minVelTresh, coefResetValue);
            vcc[2] = new VelocityCoefCalculatorAngle(minAngleTresh, minVelTresh, coefResetValue);
            for (int i = 0; i < pid.Length; i++)
                pid[i] = new PID();
        }
        
        public void UpdateCoefs(Position2D p, Vector2D v, double angle, double w, Position2D target, double targetAngle)
        {
            velCoef[0] = Math.Max(minCoefTresh, vcc[0].UpdateVelocityCoefs(p.X, v.X, target.X));
            velCoef[1] = Math.Max(minCoefTresh, vcc[1].UpdateVelocityCoefs(p.Y, v.Y, target.Y));
            velCoef[2] = Math.Max(minCoefTresh, vcc[2].UpdateVelocityCoefs(angle, w, targetAngle));
        }
        public double Tune(double d, int RobotID, PIDType type)
        {
            int idx = (int)type;

            pid[idx].Coef = PIDParameters.Coefs[RobotID, type] / velCoef[idx];

            return -pid[idx].Calculate(d, 0);
        }
        public void Reset(PIDType type)
        {
            pid[(int)type].Reset();
        }
        public void Drawings(PIDType type, int RobotID)
        {
            
            int idx = (int)type;

            CharterData.AddData("err", pid[idx].Err);

            //DrawingObjects.AddObject(new StringDraw(type.ToString() + velCoef[idx], new Position2D(1 + (double)((idx) * 0.25), -(double)(RobotID * 0.3))), RobotID + type.ToString() + "velCoef" + type);
        }
        public void Check4CollisionReset(PIDType type)
        {
            if (VelCoef[(int)type] < resetCoefTresh)
                VelCoef[(int)type] = coefResetValue;
        }
    }

    abstract class VelocityCoefCalculaterBase
    {
        protected int latency = 6, maxFrame2Calc = 30, prediction = 1;
        protected double velocityCoef;

        public double VelocityCoef
        {
            get { return velocityCoef; }
        }
        protected double distTresh, velTresh;

        protected Queue<double> posQ = new Queue<double>();
        protected Queue<double> velQ = new Queue<double>();

        public abstract double UpdateVelocityCoefs(double p, double v, double target);

    }
    class VelocityCoefCalculatorPos : VelocityCoefCalculaterBase
    {
        public VelocityCoefCalculatorPos(double distTresh, double velTresh, double velCoef)
        {
            this.distTresh = distTresh;
            this.velTresh = velTresh;
            this.velocityCoef = velCoef;
        }

        public override double UpdateVelocityCoefs(double p, double v, double target)
        {
            if (Math.Abs(p - target) > distTresh && Math.Abs(v) > velTresh)
            {
                posQ.Enqueue(p);
                velQ.Enqueue(v);

                if (posQ.Count > maxFrame2Calc + prediction)
                    posQ.Dequeue();
                if (velQ.Count > maxFrame2Calc + latency)
                    velQ.Dequeue();

                if (velQ.Count < latency + maxFrame2Calc || posQ.Count < prediction + maxFrame2Calc)
                    return velocityCoef;

                RectangularMatrix xn = new RectangularMatrix(posQ.Skip(prediction).ToArray());
                RectangularMatrix x = new RectangularMatrix(posQ.Take(posQ.Count - prediction).ToArray());
                RectangularMatrix vn = new RectangularMatrix(velQ.Take(velQ.Count - latency).ToArray());
                RectangularMatrix d = StaticVariables.FRAME_RATE * (xn - x);

                RectangularMatrix alfaM = ((SquareMatrix)(vn.Transpose() * vn)).Inverse() * vn.Transpose() * d;

                return velocityCoef = alfaM[0, 0];
            }
            posQ.Clear();
            velQ.Clear();
            return velocityCoef;
        }
    }
    class VelocityCoefCalculatorAngle : VelocityCoefCalculaterBase
    {
        public VelocityCoefCalculatorAngle(double distTresh, double velTresh, double velCoef)
        {
            this.distTresh = distTresh;
            this.velTresh = velTresh;
            this.velocityCoef = velCoef;
        }

        public override double UpdateVelocityCoefs(double p, double v, double target)
        {
            double tmpd = AngleModeInDegree(AngleModeInDegree(p) - AngleModeInDegree(target));

            if (Math.Abs(tmpd) > distTresh && Math.Abs(v) > velTresh)
            {
                posQ.Enqueue(p);
                velQ.Enqueue(v);

                if (posQ.Count > maxFrame2Calc + prediction)
                    posQ.Dequeue();
                if (velQ.Count > maxFrame2Calc + latency)
                    velQ.Dequeue();

                if (velQ.Count < latency + maxFrame2Calc || posQ.Count < prediction + maxFrame2Calc)
                    return velocityCoef;

                double[] xn = posQ.Skip(prediction).ToArray();
                double[] x = posQ.Take(posQ.Count - prediction).ToArray();
                RectangularMatrix vn = new RectangularMatrix(velQ.Take(velQ.Count - latency).ToArray());
                RectangularMatrix d = new RectangularMatrix(maxFrame2Calc, 1);

                for (int i = 0; i < maxFrame2Calc; i++)
                    d[i, 0] = StaticVariables.FRAME_RATE * AngleModeInDegree(xn[i] - x[i]).ToRadian();

                RectangularMatrix alfaM = ((SquareMatrix)(vn.Transpose() * vn)).Inverse() * vn.Transpose() * d;

                velocityCoef = alfaM[0, 0];
                return velocityCoef;
            }
            posQ.Clear();
            velQ.Clear();
            return velocityCoef;
        }
        double AngleModeInDegree(double angle)
        {
            if (angle > 180)
                angle -= 360;
            else if (angle < -180)
                angle += 360;
            return angle;
        }
        double AngleModeInRadian(double angle)
        {
            if (angle > Math.PI)
                angle -= 2 * Math.PI;
            else if (angle < -Math.PI)
                angle += 2 * Math.PI;
            return angle;
        }
    }

    public class PID
    {
        private double _lastError, _integral, _difrential;
        private PIDCoef coef;

        public PIDCoef Coef
        {
            get { return coef; }
            set { coef = value; }
        }

        bool first = true;

        private double err;

        public double Err
        {
            get { return err; }
        }
        public double Calculate(double current, double desierd)
        {
            double error = desierd - current;
            if (first)
                _lastError = error;
            err = error;
            _difrential = (error - _lastError) * StaticVariables.FRAME_RATE;
            _lastError = error;
            _integral *= coef.Lambda;
            _integral += error * StaticVariables.FRAME_PERIOD;
            double output = error * coef.Kp + _integral * coef.Ki + _difrential * coef.Kd;

               // CharterData.AddData("err", -error);
            first = false;
            return output;
        }
        public void Reset()
        {
            _integral = 0;
            first = true;
        }
    }
   
    public class Tunner
    {
        private double IdS, lastE;
        private Position2D _p0, _p1, _q0, _q1, _o0 = new Position2D(10, 0), _o1 = new Position2D(0.00001, 0);

        public Position2D O1
        {
            get { return _o1; }
            set { _o1 = value; }
        }

        public Position2D O0
        {
            get { return _o0; }
            set { _o0 = value; }
        }

        public Position2D Q1
        {
            get { return _q1; }
            set { _q1 = value; }
        }

        public Position2D Q0
        {
            get { return _q0; }
            set { _q0 = value; }
        }

        public Position2D P1
        {
            get { return _p1; }
            set { _p1 = value; }
        }

        public Position2D P0
        {
            get { return _p0; }
            set { _p0 = value; }
        }

        bool first = true;
        double kp = 0;
        double ki = 0;
        double kd = 0; 
        Line lP = new Line();
        Line lI = new Line();
        Line lD = new Line();
        
        public double Tune(double dS, double v0, int i, int RobotID)
        {

            double lambda = 1;

            //lP = new Line(_p0, _p1);
            //lI = new Line(_q0, _q1);
            //lD = new Line(_q0, _q1);

            //kp = lP.CalculateY(v0).Y;
            //ki = lI.CalculateY(v0).Y;
            //kd = lD.CalculateY(v0).Y;

            if (i == 1 || i == 2)
            {
                kp = 9;
                ki = 0;
                kd = 0.07;
            }
            else
            {
                kp = 6;// 7.3;
                ki = 0;//0.03;
                kd =  0.001;
            }

            if (first)
            {
                first = false;
                lastE = dS;
            }
            if (i == 3 && RobotID == 6)
                CharterData.AddData("errW", dS);
            double d = (dS - lastE) / StaticVariables.FRAME_PERIOD;
            IdS = IdS * lambda + (dS * StaticVariables.FRAME_PERIOD);
            lastE = dS;
            if (i == 3 && RobotID == 6)
                CharterData.AddData("err", kp * dS + IdS * ki + d * kd);
            return kp * dS + IdS * ki + d * kd;
        }
        public void Reset()
        {
            IdS = 0;
            first = true;
        }
    }

    public static class PIDParameters
    {
        public const int MaxRobotID = 16, PIDModuleCount = 3;
        static double kp = 7.9, ki = .05, kd = 0.01;
        static PIDCoef[] coefs = new PIDCoef[MaxRobotID * PIDModuleCount];
        public sealed class CoefIndex
        {
            public PIDCoef this[int RobotID, PIDType type]
            {
                get
                {
                    return PIDParameters.coefs[(int)type * MaxRobotID + RobotID];
                }
                set
                {
                    PIDParameters.coefs[(int)type * MaxRobotID + RobotID] = value;
                }
            }
        }

        static CoefIndex indexr;

        public static CoefIndex Coefs
        {
            get { return indexr; }
        }

        static PIDParameters()
        {
            Initilize();
            indexr = new CoefIndex();
        }
        public static void Initilize()
        {
            for (int i = 0; i < MaxRobotID; i++)
                for (int j = 0; j < PIDModuleCount; j++)
                    coefs[j * MaxRobotID + i] = new PIDCoef(kp, ki, kd);
        }
    
    }
    public struct PIDCoef
    {
        double kp, ki, kd, lambda;
        public PIDCoef(double _kp, double _ki, double _kd, double _lambda)
        {
            kp = _kp;
            ki = _ki;
            kd = _kd;
            lambda = _lambda;
        }
        public PIDCoef(double _kp, double _ki, double _kd)
        {
            kp = _kp;
            ki = _ki;
            kd = _kd;
            lambda = 1;
        }
        public double Kp
        {
            get { return kp; }
            set { kp = value; }
        }

        public double Ki
        {
            get { return ki; }
            set { ki = value; }
        }

        public double Kd
        {
            get { return kd; }
            set { kd = value; }
        }

        public double Lambda
        {
            get { return lambda; }
            set { lambda = value; }
        }
        public static PIDCoef operator /(PIDCoef c, double d)
        {
            return new PIDCoef(c.kp / d, c.ki / d, c.kd / d,c.lambda);
        }
        public static PIDCoef operator *(PIDCoef c, double d)
        {
            return new PIDCoef(c.kp * d, c.ki * d, c.kd * d, c.lambda);
        }
        public static PIDCoef operator *(double d, PIDCoef c)
        {
            return new PIDCoef(c.kp * d, c.ki * d, c.kd * d, c.lambda); ;
        }
    }
    public enum PIDType
    {
        X = 0,
        Y = 1,
        W = 2
    }
}
