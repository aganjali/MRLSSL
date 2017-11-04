using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Diagnostics;
using Meta.Numerics.Matrices;

namespace MRL.SSL.AIConsole.Merger_and_Tracker
{
    public class KalmanFilter
    {
        /*/////////////////////////////////////////////////////                           
         * Kalman filter Section to fulfill our delay (0.04s)
         * it compound of 2 sections, first initialization
         * section and then corrector.
         */
        ////////////////////////////////////////////////////
        private ObjectType objType;
        private Stopwatch stp = new Stopwatch();
        private RectangularMatrix A;
        private RectangularMatrix H;
        private RectangularMatrix Q;
        private RectangularMatrix R;
        private RectangularMatrix I;
        private RectangularMatrix Y;
        private RectangularMatrix P;
        private RectangularMatrix PP;
        private RectangularMatrix K;
        private RectangularMatrix EState;
        private RectangularMatrix State;
        private double lastT = 0;
        private Position2D lastPos;
        private float LastTheta, LastRaw;
        private int counter = 0;

        private double lastCovergenced = 0;
        private double VarianceMes = 0.6;//0.03;
        private double VarianceAcc = 3000;
        private double MaximumPermissibleError = 0.06 * 0.06 + 0.06 * 0.06;
        private double MinimumPermissibleError = 0.03 * 0.03 + 0.03 * 0.03;
        private double Covergenced = double.MaxValue;

        private bool isRestarted = false;
        public bool IsRestarted
        {
            get { return isRestarted; }
            set { isRestarted = value; }
        }

        public void setBallOrRobot(ObjectType RobotOrBall)
        {
            objType = RobotOrBall;
            initKalman();
        }

        public KalmanFilter()
        {
            stp.Start();
            initKalman();
        }
        ~KalmanFilter()
        {
            stp.Stop();
        }

        private void initKalman()
        {
            double dt = 0.0167;
            counter = 0;

            if (objType == ObjectType.Ball)
            {
                VarianceAcc = 3000;

                A = new RectangularMatrix(6, 6);
                H = new RectangularMatrix(2, 6);
                Q = new RectangularMatrix(6, 6);
                R = new RectangularMatrix(2, 2);
                I = new RectangularMatrix(6, 6);
                Y = new RectangularMatrix(2, 1);

                R[0, 0] = VarianceMes; R[0, 1] = 0.00001;
                R[1, 0] = R[0, 1]; R[1, 1] = R[0, 0];

                Q[0, 0] = 1; Q[1, 1] = 1; Q[2, 2] = 10; Q[3, 3] = 10; Q[4, 4] = 100; Q[5, 5] = 100;
                I[0, 0] = I[1, 1] = I[2, 2] = I[3, 3] = I[4, 4] = I[5, 5] = 1;
                H[0, 0] = 1; H[1, 1] = 1;

                A[0, 0] = 1; A[0, 1] = 0; A[0, 2] = dt; A[0, 3] = 0; A[0, 4] = 0.5 * dt * dt; A[0, 5] = 0;
                A[1, 0] = 0; A[1, 1] = 1; A[1, 2] = 0; A[1, 3] = dt; A[1, 4] = 0; A[1, 5] = 0.5 * dt * dt;
                A[2, 0] = 0; A[2, 1] = 0; A[2, 2] = 1; A[2, 3] = 0; A[2, 4] = dt; A[2, 5] = 0;
                A[3, 0] = 0; A[3, 1] = 0; A[3, 2] = 0; A[3, 3] = 1; A[3, 4] = 0; A[3, 5] = dt;
                A[4, 0] = 0; A[4, 1] = 0; A[4, 2] = 0; A[4, 3] = 0; A[4, 4] = 1; A[4, 5] = 0;
                A[5, 0] = 0; A[5, 1] = 0; A[5, 2] = 0; A[5, 3] = 0; A[5, 4] = 0; A[5, 5] = 1;
                ////////////////////////////////////
                P = new RectangularMatrix(6, 6);
                for (int i = 0; i < P.RowCount; i++)
                    for (int j = 0; j < P.ColumnCount; j++)
                        P[i, j] = 1;
                EState = new RectangularMatrix(6, 1);
                State = new RectangularMatrix(6, 1);
                K = new RectangularMatrix(6, 2);

                EState[0, 0] = lastPos.X;
                EState[1, 0] = lastPos.Y;
                ////////////////////////////////////
            }
            else  //  whenever there is robot
            {
                VarianceAcc = 1300;

                A = new RectangularMatrix(9, 9);
                H = new RectangularMatrix(3, 9);
                Q = new RectangularMatrix(9, 9);
                R = new RectangularMatrix(3, 3);
                I = new RectangularMatrix(9, 9);
                Y = new RectangularMatrix(3, 1);

                R[0, 0] = VarianceMes; R[0, 1] = 0.00001; R[0, 2] = 0.00001;
                R[1, 0] = R[0, 1]; R[1, 1] = VarianceMes; R[1, 2] = 0.00001;
                R[2, 0] = R[0, 2]; R[2, 1] = R[1, 2]; R[2, 2] = VarianceMes;

                Q[0, 0] = 1; Q[1, 1] = 1; Q[2, 2] = 1; Q[3, 3] = 10; Q[4, 4] = 10; Q[5, 5] = 10; Q[6, 6] = 100; Q[7, 7] = 100; Q[8, 8] = 100;
                I[0, 0] = I[1, 1] = I[2, 2] = I[3, 3] = I[4, 4] = I[5, 5] = I[6, 6] = I[7, 7] = I[8, 8] = 1;
                H[0, 0] = 1; H[1, 1] = 1; H[2, 2] = 1;

                A[0, 0] = 1; A[0, 1] = 0; A[0, 2] = 0; A[0, 3] = dt; A[0, 4] = 0; A[0, 5] = 0; A[0, 6] = 0.5 * dt * dt; A[0, 7] = 0; A[0, 8] = 0;
                A[1, 0] = 0; A[1, 1] = 1; A[1, 2] = 0; A[1, 3] = 0; A[1, 4] = dt; A[1, 5] = 0; A[1, 6] = 0; A[1, 7] = 0.5 * dt * dt; A[1, 8] = 0;
                A[2, 0] = 0; A[2, 1] = 0; A[2, 2] = 1; A[2, 3] = 0; A[2, 4] = 0; A[2, 5] = dt; A[2, 6] = 0; A[2, 7] = 0; A[2, 8] = 0.5 * dt * dt;

                A[3, 0] = 0; A[3, 1] = 0; A[3, 2] = 0; A[3, 3] = 1; A[3, 4] = 0; A[3, 5] = 0; A[3, 6] = dt; A[3, 7] = 0; A[3, 8] = 0;
                A[4, 0] = 0; A[4, 1] = 0; A[4, 2] = 0; A[4, 3] = 0; A[4, 4] = 1; A[4, 5] = 0; A[4, 6] = 0; A[4, 7] = dt; A[4, 8] = 0;
                A[5, 0] = 0; A[5, 1] = 0; A[5, 2] = 0; A[5, 3] = 0; A[5, 4] = 0; A[5, 5] = 1; A[5, 6] = 0; A[5, 7] = 0; A[5, 8] = dt;

                A[6, 0] = 0; A[6, 1] = 0; A[6, 2] = 0; A[6, 3] = 0; A[6, 4] = 0; A[6, 5] = 0; A[6, 6] = 1; A[6, 7] = 0; A[6, 8] = 0;
                A[7, 0] = 0; A[7, 1] = 0; A[7, 2] = 0; A[7, 3] = 0; A[7, 4] = 0; A[7, 5] = 0; A[7, 6] = 0; A[7, 7] = 1; A[7, 8] = 0;
                A[8, 0] = 0; A[8, 1] = 0; A[8, 2] = 0; A[8, 3] = 0; A[8, 4] = 0; A[8, 5] = 0; A[8, 6] = 0; A[8, 7] = 0; A[8, 8] = 1;
                ////////////////////////////////////
                P = new RectangularMatrix(9, 9);
                for (int i = 0; i < P.RowCount; i++)
                    for (int j = 0; j < P.ColumnCount; j++)
                        P[i, j] = 1;
                EState = new RectangularMatrix(9, 1);
                State = new RectangularMatrix(9, 1);
                K = new RectangularMatrix(9, 2);

                EState[0, 0] = lastPos.X;
                EState[1, 0] = lastPos.Y;
                ////////////////////////////////////
            }
        }

        public void kalmanFilter(Position2D Pos)
        {
            if (!(Pos.X > GameParameters.OppGoalLeft.X /*- RobotParameters.OurRobotParams.Diameter / 2.0*/ && Pos.X < GameParameters.OurGoalLeft.X /*+ RobotParameters.OurRobotParams.Diameter / 2.0*/ && Pos.Y > GameParameters.OurRightCorner.Y/*- RobotParameters.OurRobotParams.Diameter / 2.0*/ && Pos.Y < GameParameters.OurLeftCorner.Y /*+ RobotParameters.OurRobotParams.Diameter / 2.0*/))
            {
                initKalman();
            }
            
            lastPos = Pos;
            double mesX = 0, mesY = 0;
            double t = stp.ElapsedMilliseconds / 1000.0;
            if (lastT == 0)
                lastT = t;
            double dt = (t - lastT);
            if (dt == 0) dt = 0.0167;
            //dt = 0.0167;
            A[0, 2] = dt; A[0, 4] = 0.5 * dt * dt;
            A[1, 3] = dt; A[1, 5] = 0.5 * dt * dt;
            A[2, 4] = dt; A[3, 5] = dt;

            Q[0, 0] = (0.5 * dt * dt) * (0.5 * dt * dt) * VarianceAcc;
            Q[1, 1] = (0.5 * dt * dt) * (0.5 * dt * dt) * VarianceAcc;
            Q[2, 2] = (dt * dt) * VarianceAcc;
            Q[3, 3] = (dt * dt) * VarianceAcc;
            Q[4, 4] = VarianceAcc;
            Q[5, 5] = VarianceAcc;
            do
            {
                if (double.IsNaN(EState[0, 0]) || double.IsNaN(EState[1, 0]) == true)
                {
                    initKalman();
                }

                mesX = Pos.X;
                mesY = Pos.Y;

                State = A * EState;
                PP = A * P * A.Transpose() + Q;
                K = PP * H.Transpose() * ((SquareMatrix)(H * PP * H.Transpose() + R)).Inverse();

                Y[0, 0] = mesX; Y[1, 0] = mesY;
                EState = State + K * (Y - H * State);
                P = (I - K * H) * PP;

                CalculateVarianceBall();
                lastT = t;
                counter++;
            } while (counter < 20);
        }

        public void kalmanFilter(Position2D Pos, float? Angle)
        {
            if (objType == ObjectType.Ball)
            {
                kalmanFilter(Pos);
                return;
            }
            else
            {
                float rawAngle = (float)(Angle.Value * (Math.PI / 180));
                //Angle = (float)(Angle.Value * (Math.PI / 180));
                float eps = (float)(50 * (Math.PI / 180));
                if ((rawAngle - LastRaw) > (2 * Math.PI - eps))
                {
                    Angle = (float)(((rawAngle - LastRaw) - Math.PI * 2) + LastTheta);
                }
                else if (rawAngle - LastRaw < (-2 * Math.PI + eps))
                {
                    Angle = (float)(((rawAngle - LastRaw) + Math.PI * 2) + LastTheta);
                }
                else
                {
                    Angle = LastTheta + rawAngle - LastRaw;
                }
                //DrawingObjects.AddObject(new StringDraw(Angle.ToString(), Pos+new Vector2D(-0.15,0)), "jkh");

                LastTheta = Angle.Value;
                LastRaw = rawAngle;
                lastPos = Pos;

                double mesX = 0, mesY = 0, mesT = 0;
                double t = stp.ElapsedMilliseconds / 1000.0;
                if (lastT == 0)
                    lastT = t;
                double dt = (t - lastT);
                if (dt == 0) dt = 0.0167;

                A[0, 3] = dt; A[0, 6] = 0.5 * dt * dt;
                A[1, 4] = dt; A[1, 7] = 0.5 * dt * dt;
                A[2, 5] = dt; A[2, 8] = 0.5 * dt * dt;

                A[3, 6] = dt;
                A[4, 7] = dt;
                A[5, 8] = dt;

                Q[0, 0] = (0.5 * dt * dt) * (0.5 * dt * dt) * VarianceAcc;
                Q[1, 1] = (0.5 * dt * dt) * (0.5 * dt * dt) * VarianceAcc;
                Q[2, 2] = (0.5 * dt * dt) * (0.5 * dt * dt) * VarianceAcc;
                Q[3, 3] = (dt * dt) * VarianceAcc;
                Q[4, 4] = (dt * dt) * VarianceAcc;
                Q[5, 5] = (dt * dt) * VarianceAcc;
                Q[6, 6] = VarianceAcc;
                Q[7, 7] = VarianceAcc;
                Q[8, 8] = VarianceAcc;

                do
                {
                    if (double.IsNaN(EState[0, 0]) || double.IsNaN(EState[1, 0]) == true || double.IsNaN(EState[2, 0]))
                    {
                        initKalman();
                    }

                    mesX = Pos.X;
                    mesY = Pos.Y;
                    mesT = Angle.Value;

                    State = A * EState;
                    PP = A * P * A.Transpose() + Q;
                    K = PP * H.Transpose() * ((SquareMatrix)(H * PP * H.Transpose() + R)).Inverse();

                    Y[0, 0] = mesX; Y[1, 0] = mesY; Y[2, 0] = mesT;
                    EState = State + K * (Y - H * State);
                    P = (I - K * H) * PP;

                    CalculateVarianceRobot();
                    lastT = t;
                    counter++;
                } while (counter < 20);
            }
        }

        private void CalculateVarianceBall()
        {
            Covergenced = (EState[0, 0] - lastPos.X) * (EState[0, 0] - lastPos.X) + (EState[1, 0] - lastPos.Y) * (EState[1, 0] - lastPos.Y);
            if (lastCovergenced != 0)
            {
                Covergenced = (Covergenced + lastCovergenced) / 2.0;
            }
            lastCovergenced = Covergenced;
        }

        private void CalculateVarianceRobot()
        {
            Covergenced = (EState[0, 0] - lastPos.X) * (EState[0, 0] - lastPos.X) + (EState[1, 0] - lastPos.Y) * (EState[1, 0] - lastPos.Y) + (EState[2, 0] - LastTheta) * (EState[2, 0] - LastTheta);
            if (lastCovergenced != 0)
            {
                Covergenced = (Covergenced + lastCovergenced) / 2.0;
            }
            lastCovergenced = Covergenced;
        }

        private bool isNullMatrix(RectangularMatrix m)
        {
            
            for (int i = 0; i < m.RowCount; i++)
                for (int j = 0; j < m.ColumnCount; j++)
                    if (m[i, j] != 0)
                        return false;
            return true;
        }

        public bool isImmobile()
        {
            if (EState[2, 0] < 0.09 && EState[3, 0] < 0.09 && EState[4, 0] < 0.9 && EState[5, 0] < 0.9)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetStatus(GameStatus status)
        {
            if (status != GameStatus.Normal)
            {
                MaximumPermissibleError = 0.02 * 0.02 + 0.02 * 0.02;
                MinimumPermissibleError = 0.01 * 0.01 + 0.01 * 0.01;
            }
            else
            {
                MaximumPermissibleError = 0.06 * 0.06 + 0.06 * 0.06;
                MinimumPermissibleError = 0.03 * 0.03 + 0.03 * 0.03;
            }
        }

        public void RestartState()
        {
            this.initKalman();
        }

        public Vector2D getEstimatedAcceleration(double delay)
        {
            if (objType == ObjectType.Ball)
            {
                A[0, 2] = delay; A[0, 4] = 0.5 * delay * delay;
                A[1, 3] = delay; A[1, 5] = 0.5 * delay * delay;
                A[2, 4] = delay; A[3, 5] = delay;

                State = A * EState;

                double EstAX = State[4, 0];
                double EstAY = State[5, 0];

                return new Vector2D(EstAX, EstAY);
            }
            else
            {
                A[0, 3] = delay; A[0, 6] = 0.5 * delay * delay;
                A[1, 4] = delay; A[1, 7] = 0.5 * delay * delay;
                A[2, 5] = delay; A[2, 8] = 0.5 * delay * delay;

                A[3, 6] = delay;
                A[4, 7] = delay;
                A[5, 8] = delay;

                State = A * EState;

                double EstAX = State[6, 0];
                double EstAY = State[7, 0];

                return new Vector2D(EstAX, EstAY);
            }
        }

        public Vector2D getEstimatedVelocity(double delay, params double[] inRate)
        {
            double insurerate = 1;
            if (inRate.Length >= 1)
                insurerate = inRate[0];

            if (objType == ObjectType.Ball)
            {
                if ((Covergenced > MaximumPermissibleError) && (insurerate < 1))
                {
                    return new Vector2D(0, 0);
                }
                else
                {
                    A[0, 2] = delay; A[0, 4] = 0.5 * delay * delay;
                    A[1, 3] = delay; A[1, 5] = 0.5 * delay * delay;
                    A[2, 4] = delay; A[3, 5] = delay;

                    State = A * EState;

                    double EstVX = State[2, 0];
                    double EstVY = State[3, 0];

                    return new Vector2D(EstVX, EstVY);
                }
            }
            else
            {
                A[0, 3] = delay; A[0, 6] = 0.5 * delay * delay;
                A[1, 4] = delay; A[1, 7] = 0.5 * delay * delay;
                A[2, 5] = delay; A[2, 8] = 0.5 * delay * delay;

                A[3, 6] = delay;
                A[4, 7] = delay;
                A[5, 8] = delay;

                State = A * EState;

                double EstVX = State[3, 0];
                double EstVY = State[4, 0];

                return new Vector2D(EstVX, EstVY);
            }
        }

        public Position2D getEstimatedPosition(double delay, params double[] inRate)
        {
            double insurerate = 1;
            if (inRate.Length >= 1)
                insurerate = inRate[0];

            if (counter < 20)
            {
                return lastPos;
            }
            if (objType == ObjectType.Ball)
            {
                if (insurerate <= 0.7)
                {
                    initKalman();
                    return lastPos;
                }
                if ((Covergenced > MaximumPermissibleError) && (insurerate < 1))
                {
                    return lastPos;
                }
                else
                {
                    A[0, 2] = delay; A[0, 4] = 0.5 * delay * delay;
                    A[1, 3] = delay; A[1, 5] = 0.5 * delay * delay;
                    A[2, 4] = delay; A[3, 5] = delay;

                    State = A * EState;

                    double EstX = State[0, 0];
                    double EstY = State[1, 0];

                    return new Position2D(EstX, EstY);
                }
            }
            else
            {
                A[0, 3] = delay; A[0, 6] = 0.5 * delay * delay;
                A[1, 4] = delay; A[1, 7] = 0.5 * delay * delay;
                A[2, 5] = delay; A[2, 8] = 0.5 * delay * delay;

                A[3, 6] = delay;
                A[4, 7] = delay;
                A[5, 8] = delay;

                State = A * EState;

                double EstX = State[0, 0];
                double EstY = State[1, 0];

                return new Position2D(EstX, EstY);
            }
        }

        internal float getEstimatedAngle(double delay)
        {
            A[0, 3] = delay; A[0, 6] = 0.5 * delay * delay;
            A[1, 4] = delay; A[1, 7] = 0.5 * delay * delay;
            A[2, 5] = delay; A[2, 8] = 0.5 * delay * delay;

            A[3, 6] = delay;
            A[4, 7] = delay;
            A[5, 8] = delay;

            State = A * EState;
            float esAngle = (float)(State[2, 0]);
            esAngle = (float)(Math.Atan2(Math.Sin(esAngle), Math.Cos(esAngle)) * 180 / Math.PI);

            return esAngle;
        }
    }
}
