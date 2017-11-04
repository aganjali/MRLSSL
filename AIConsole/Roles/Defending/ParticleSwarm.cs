using SwarmDefence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MRL.SSL.GameDefinitions;

namespace SwarmDefence
{
    class ParticleSwarm
    {

        //number of particles we need
        public static int ParticleNumber = 10;
        //iteration number we need
        public static int IterationNumber = 100;
        //Personal Learning Coeff
        public static double PLCoeff =.9;
        //Collective Learning Coeff
        public static double CLCoeff = 1.2;
        //Inertia coeff
        public static double Inertiacoeff = 1;
        double omega = 1;


        public static Particle GlobalBest;

        Position2DPSO bestgoaliePS = new Position2DPSO();
        Position2DPSO goaliePS = new Position2DPSO();
        Position2DPSO bestDefender1PS = new Position2DPSO();
        Position2DPSO defender1PS = new Position2DPSO();
        Position2DPSO bestDefender2PS = new Position2DPSO();
        Position2DPSO defender2PS = new Position2DPSO();
        Vector2DPSO zero = new Vector2DPSO(0, 0);
        HiPerfTimer hp1 = new HiPerfTimer();
        List<double> times = new List<double>();
        Vector2DPSO Zero = new Vector2DPSO(0, 0);
        public Particle Run(Position2DPSO ballpos, Position2DPSO lastGoalie, Position2DPSO LastRobot1, Position2DPSO LastRobot2)
        {
            Position2DPSO goalie = new Position2DPSO();
            Position2DPSO defender1 = new Position2DPSO();
            Position2DPSO defender2 = new Position2DPSO();
            Random rand = new Random();
            resetElements();
            //ObjectiveFunction objectiveFunction = new ObjectiveFunction();

            Particle[] particles = new Particle[ParticleNumber];
            Particle[] personalBests = new Particle[ParticleNumber];

            SpecificRandomGenerator randomgenerator = new SpecificRandomGenerator();

            for (int j = 0; j < ParticleNumber; j++)
            {
                goalie = randomgenerator.GoalieRandomGenerator();
                defender1 = randomgenerator.DefenderRandomGenerator();
                defender2 = randomgenerator.DefenderRandomGenerator();
                double fvals = Temporal(ballpos, goalie, defender1, defender2, lastGoalie, LastRobot1, LastRobot2);
                particles[j] = (new Particle(goalie.X, goalie.Y, defender1.X, defender1.Y, defender2.X, defender2.Y, Zero, Zero, Zero, fvals, j));
                personalBests[j] = (new Particle(goalie.X, goalie.Y, defender1.X, defender1.Y, defender2.X, defender2.Y, Zero, Zero, Zero, fvals, j));
            }
            Vector2DPSO zero = new Vector2DPSO(0, 0);
            //double fval = objectiveFunction.Temporal(new Position2D(3.5, 0), new Position2D(2.5, .1), new Position2D(2.5, -.1));
            GlobalBest = new Particle(0, 0, 0, 0, 0, 0, zero, zero, zero, 10000, 1000);// //it must doesn't be a lambda
            for (int k = 0; k < ParticleNumber; k++)
            {
                if (particles[k].fvalP < GlobalBest.fvalP)
                    GlobalBest = particles[k];
            }
            //hp1.Start();
            for (int k = 0; k < IterationNumber; k++)
            {
                Inertiacoeff = Inertiacoeff * 1;
                omega = Inertiacoeff * omega;
                for (int s = 0; s < ParticleNumber; s++)
                {
                    bestgoaliePS = new Position2DPSO(personalBests[s].xGoalkeeper, personalBests[s].yGoalKeeper);
                    goaliePS = new Position2DPSO(particles[s].xGoalkeeper, particles[s].yGoalKeeper);
                    bestDefender1PS = new Position2DPSO(personalBests[s].xDefender1, personalBests[s].yDefender1);
                    defender1PS = new Position2DPSO(particles[s].xDefender1, personalBests[s].yDefender1);
                    bestDefender2PS = new Position2DPSO(personalBests[s].xDefender2, personalBests[s].yDefender2);
                    defender2PS = new Position2DPSO(particles[s].xDefender2, personalBests[s].yDefender2);

                    double firstRandom = rand.NextDouble();
                    double secondRandom = rand.NextDouble();
                    Vector2DPSO goalieVelocityTemp = new Vector2DPSO((omega * particles[s].goalieVelocity.X) + (firstRandom * PLCoeff * (bestgoaliePS.X - goaliePS.X)) + (secondRandom * CLCoeff * (GlobalBest.xGoalkeeper - goaliePS.X)), (omega * particles[s].goalieVelocity.Y) + (firstRandom * PLCoeff * (bestgoaliePS.Y - goaliePS.Y)) + (secondRandom * CLCoeff * (GlobalBest.yGoalKeeper - goaliePS.Y)));
                    firstRandom = rand.NextDouble();
                    secondRandom = rand.NextDouble();
                    Vector2DPSO defender1VelocityTemp = new Vector2DPSO((omega * particles[s].defender1Velocity.X) + (firstRandom * PLCoeff * (bestDefender1PS.X - defender1PS.X)) + (secondRandom * CLCoeff * (GlobalBest.xDefender1 - defender1PS.X)), (omega * particles[s].defender1Velocity.Y) + (firstRandom * PLCoeff * (bestDefender1PS.Y - defender1PS.Y)) + (secondRandom * CLCoeff * (GlobalBest.yDefender1 - defender1PS.Y)));
                    firstRandom = rand.NextDouble();
                    secondRandom = rand.NextDouble();
                    Vector2DPSO defender2VelocityTemp = new Vector2DPSO((omega * particles[s].defender2velocity.X) + (firstRandom * PLCoeff * (bestDefender2PS.X - defender1PS.X)) + (secondRandom * CLCoeff * (GlobalBest.xDefender2 - defender2PS.X)), (omega * particles[s].defender2velocity.Y) + (firstRandom * PLCoeff * (bestDefender2PS.Y - defender1PS.Y)) + (secondRandom * CLCoeff * (GlobalBest.yDefender2 - defender2PS.Y)));

                    Position2DPSO goaliePositionTemp = new Position2DPSO(goaliePS.X + goalieVelocityTemp.X, goaliePS.Y + goalieVelocityTemp.Y);
                    Position2DPSO defender1PositionTemp = new Position2DPSO(defender1PS.X + defender1VelocityTemp.X, defender1PS.Y + defender1VelocityTemp.Y);
                    Position2DPSO defender2PositionTemp = new Position2DPSO(defender2PS.X + defender2VelocityTemp.X, defender2PS.Y + defender2VelocityTemp.Y);

                    bool defender1IsInRegion = true;
                    bool defender2IsInRegion = true;
                    bool overlapDefense = false;
                    bool DefenseCorrect = DefenderCheckConstraint(defender1PositionTemp, defender2PositionTemp, ref defender1IsInRegion, ref defender2IsInRegion, ref overlapDefense);
                    bool golaieCorrect = GoalieCheckConstraint(goaliePositionTemp);

                    if (!DefenseCorrect)
                    {
                        if (!defender1IsInRegion)
                        {
                            defender1PositionTemp = randomgenerator.DefenderRandomGenerator();
                            defender1VelocityTemp = Zero;
                        }
                        if (!defender2IsInRegion)
                        {
                            defender2PositionTemp = randomgenerator.DefenderRandomGenerator();
                            defender2VelocityTemp = Zero;
                        }
                    }
                    if (!golaieCorrect)
                    {
                        goaliePositionTemp = randomgenerator.GoalieRandomGenerator();
                        goalieVelocityTemp = Zero;
                    }
                    resetElements();
                    double newCost = Temporal(ballpos, goaliePositionTemp, defender1PositionTemp, defender2PositionTemp, lastGoalie, LastRobot1, LastRobot2);
                    particles[s] = new Particle(goaliePositionTemp.X, goaliePositionTemp.Y, defender1PositionTemp.X, defender1PositionTemp.Y, defender2PositionTemp.X, defender2PositionTemp.Y, goalieVelocityTemp, defender1VelocityTemp, defender2VelocityTemp, newCost, s);
                    if (newCost < personalBests[s].fvalP)
                    {
                        personalBests[s] = new Particle(goaliePositionTemp.X, goaliePositionTemp.Y, defender1PositionTemp.X, defender1PositionTemp.Y, defender2PositionTemp.X, defender2PositionTemp.Y, goalieVelocityTemp, defender1VelocityTemp, defender2VelocityTemp, newCost, s);
                    }
                    if (newCost < GlobalBest.fvalP)
                    {
                        GlobalBest = new Particle(goaliePositionTemp.X, goaliePositionTemp.Y, defender1PositionTemp.X, defender1PositionTemp.Y, defender2PositionTemp.X, defender2PositionTemp.Y, goalieVelocityTemp, defender1VelocityTemp, defender2VelocityTemp, newCost, s);
                    }
                }
                if (GlobalBest.fvalP < .1)
                {
                    break;
                }
            }
            //hp1.Stop();
            //draw.textBox((hp1.Duration * 1000).ToString() + "\n");
            return GlobalBest;

        }
        #region Particle
        public struct Particle
        {
            public double xGoalkeeper;
            public double yGoalKeeper;
            public double xDefender1;
            public double yDefender1;
            public double xDefender2;
            public double yDefender2;
            public Vector2DPSO goalieVelocity;
            public Vector2DPSO defender1Velocity;
            public Vector2DPSO defender2velocity;
            public double fvalP;
            public int particleIDP;

            public Particle(double x1, double y1, double x2, double y2, double x3, double y3, Vector2DPSO v1, Vector2DPSO v2, Vector2DPSO v3, double fval, int particleID)
            {
                this.xGoalkeeper = x1;
                this.yGoalKeeper = y1;
                this.xDefender1 = x2;
                this.yDefender1 = y2;
                this.xDefender2 = x3;
                this.yDefender2 = y3;
                this.defender1Velocity = v2;
                this.defender2velocity = v3;
                this.goalieVelocity = v1;
                this.fvalP = fval;
                this.particleIDP = particleID;
            }
            public string ToString()
            {
                return "Goalie X Dimention: " + this.xGoalkeeper + "\n" + " Goalie Y Dimention: " + this.yGoalKeeper + "\n" + " Defender 1 X Dimension: " + this.xDefender1 + "\n" + " Defender 1 Y Dimension: " + this.yDefender1 + "\n" + " Defender 2 X Dimension: " + this.xDefender2 + "\n" + " Defender 2 Y Dimension: " + this.yDefender2 + "\n" + " fval: " + this.fvalP + "\n";

            }
        }
        #endregion
        #region Position2D
        public struct Position2DPSO
        {
            public double X;
            public double Y;
            public Position2DPSO(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }
            //public static Position2D operator +(Position2D s1, Vector2D s2)
            //{
            //    return new Position2D(s1.X + s2.X, s1.Y + s2.Y);
            //}

            //public static Vector2D operator -(Position2D s1, Position2D s2)
            //{
            //    return new Vector2D(s1.X - s2.X, s1.Y - s2.Y);
            //}
            //public double DistanceFrom(Position2D SecondPosition)
            //{
            //    return Math.Sqrt(((this.X - SecondPosition.X) * (this.X - SecondPosition.X)) + ((this.Y - SecondPosition.Y) * (this.Y - SecondPosition.Y)));
            //}
        }
        #endregion
        #region Vector2D
        public struct Vector2DPSO
        {
            public double X;
            public double Y;
            public Vector2DPSO(double x, double y)
            {
                this.X = x;
                this.Y = y;
            }

            //public static Vector2D operator *(double s1, Vector2D s2)
            //{
            //    return new Vector2D(s1 * s2.X, s1 * s2.Y);
            //}
            //public static Vector2D operator +(Vector2D s1, Vector2D s2)
            //{
            //    return new Vector2D(s1.X + s2.X, s1.Y + s2.Y);
            //}
            //public static Vector2D operator *(Vector2D s2, double s1)
            //{
            //    return new Vector2D(s1 * s2.X, s1 * s2.Y);
            //}
            //public static Vector2D FromAngleSize(double Angle, double Size)
            //{
            //    //Position2D p = new Position2D(Size * Math.Cos(Angle), Size * Math.Sin(Angle)); 
            //    return new Vector2D(Size * Math.Cos(Angle), Size * Math.Sin(Angle));
            //}
            //public Vector2D GetNormnalizedCopy()
            //{
            //    double size = Math.Sqrt((this.X * this.X) + (this.Y * this.Y));
            //    return new Vector2D(this.X / size, this.Y / size);
            //}
        }
        #endregion
        /// <summary>
        /// Constraint Functions
        /// </summary>
        #region constraint
        double goalieMargin = 0;
        double defenderMargin = 0;
        public bool GoalieCheckConstraint(Position2DPSO goaliePosition)
        {
            if (isindangerzone(goalieMargin, goaliePosition) && goaliePosition.X <= 4.045)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        static int counter = 0;
        public bool DefenderCheckConstraint(Position2DPSO Defender1Position, Position2DPSO Defender2Position, ref bool defender1IsInRegion, ref bool defender2IsInRegion, ref bool itHasOverlap)
        {
            counter++;
            //draw.textBox(counter.ToString()+"\n");
            defender1IsInRegion = IsInRegion(defenderMargin, Defender1Position);
            defender2IsInRegion = IsInRegion(defenderMargin, Defender2Position);
            itHasOverlap = RobotOverLap(Defender1Position, Defender2Position);
            if (defender1IsInRegion && defender2IsInRegion && !itHasOverlap)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// determine overlap between Robotss
        /// </summary>
        /// <param name="defender1">Defender 1 Position</param>
        /// <param name="defender2">Defender 2 Position</param>
        /// <returns></returns>
        public bool RobotOverLap(Position2DPSO defender1, Position2DPSO defender2)
        {
            if (Math.Sqrt(((defender1.X - defender2.X) * (defender1.X - defender2.X)) + ((defender1.Y - defender2.Y) * (defender1.Y - defender2.Y))) < .25)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        Position2DPSO GoalCenter = new Position2DPSO(4.045, 0);

        /// <summary>
        /// its the question Defender Robot is in Region Or not ....
        /// </summary>
        /// <param name="margin">Margin</param>
        /// <param name="DefenderPosition">Defender Position</param>
        /// <returns></returns>
        private bool IsInRegion(double margin, Position2DPSO DefenderPosition)
        {

            if (Math.Sqrt(((DefenderPosition.X - GoalCenter.X) * (DefenderPosition.X - GoalCenter.X)) + ((DefenderPosition.Y - GoalCenter.Y) * (DefenderPosition.Y - GoalCenter.Y))) < 2 + margin && Math.Sqrt(((DefenderPosition.X - GoalCenter.X) * (DefenderPosition.X - GoalCenter.X)) + ((DefenderPosition.Y - GoalCenter.Y) * (DefenderPosition.Y - GoalCenter.Y))) > 1 && DefenderPosition.X < 4.045 && DefenderPosition.Y < 3.025 && DefenderPosition.Y > -3.025)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check is goalie in dengerzone or not
        /// </summary>
        /// <param name="margin">bigger danger zone quantity</param>
        /// <param name="goaliePosition">position of goalie</param>
        /// <returns></returns>
        private bool isindangerzone(double margin, Position2DPSO goaliePosition)
        {
            if (Math.Sqrt(((goaliePosition.X - GoalCenter.X) * (goaliePosition.X - GoalCenter.X)) + ((goaliePosition.Y - GoalCenter.Y) * (goaliePosition.Y - GoalCenter.Y))) < 1 + margin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void resetElements()
        {
            leftMostPointM = 0;
            rightLeftmostPointM = 0;
            leftMiddlePointM = 0;
            rightLeftMiddlePintM = 0;
            leftLessPointM = 0;
            rightLeftLessPointM = 0;
            goalieM = new Position2DPSO(3.8, 0);
            defender1M = new Position2DPSO(2.8, -0.15);
            defender2M = new Position2DPSO(2.8, 0.15);
            errorpM = 0;
            robotRadiM = .09;
            goalCenterM = new Position2DPSO(4.045, 0);
            leftOnGoalM = new Position2DPSO(4.05, .025);
            rightOnGoalM = new Position2DPSO(4.05, -0.25);
            SumM = 0;
            sumcoeffM = 0;

            point0M = new Position2DPSO();
            pointR1M = new Position2DPSO();
            pointR2M = new Position2DPSO();
            pointR3M = new Position2DPSO();
            pointR4M = new Position2DPSO();
            pointR5M = new Position2DPSO();
            pointL1M = new Position2DPSO();
            pointL2M = new Position2DPSO();
            pointL3M = new Position2DPSO();
            pointL4M = new Position2DPSO();
            pointL5M = new Position2DPSO();

            xxcoeffM = 0;
            yycoeffM = 0;
            distM = 0;
            firstcoeffM = 0;
            secondcoeffM = 0;
            thirdCoeffM = 0;
            xLeftD1M = 0;
            xRightD1M = 0;
            xxcoeff2M = 0;
            yycoeff2M = 0;
            distrobot2ToBallM = 0;
            firstcoeff2M = 0;
            thirdCoeff2M = 0;

            xLeftD2M = 0;
            xRightD2M = 0;

            xxcoeffgM = 0;
            yycoeffgM = 0;
            distGoalieToBallM = 0;
            firstcoeffgM = 0;
            thirdCoeffgM = 0;

            RightAM = 0;
            LeftAM = 0;
            RightBM = 0;
            LeftBM = 0;
            RightCM = 0;
            LeftCM = 0;

            distanceFromRobotM = 0;
            intersectXM = 0;
            intersectYM = 0;
            goalCenterYPositionM = -0.50;
        }

        #region Objective
        double leftMostPointM = 0;
        double rightLeftmostPointM = 0;
        double leftMiddlePointM = 0;
        double rightLeftMiddlePintM = 0;
        double leftLessPointM = 0;
        double rightLeftLessPointM = 0;
        Position2DPSO goalieM = new Position2DPSO(3.8, 0);
        Position2DPSO defender1M = new Position2DPSO(2.8, -0.15);
        Position2DPSO defender2M = new Position2DPSO(2.8, 0.15);
        double errorpM = 0;
        double robotRadiM = .09;
        Position2DPSO goalCenterM = new Position2DPSO(4.045, 0);
        Position2DPSO leftOnGoalM = new Position2DPSO(4.05, .025);
        Position2DPSO rightOnGoalM = new Position2DPSO(4.05, -0.25);
        double SumM = 0;
        double sumcoeffM = 0;

        Position2DPSO point0M = new Position2DPSO();
        Position2DPSO pointR1M = new Position2DPSO();
        Position2DPSO pointR2M = new Position2DPSO();
        Position2DPSO pointR3M = new Position2DPSO();
        Position2DPSO pointR4M = new Position2DPSO();
        Position2DPSO pointR5M = new Position2DPSO();
        Position2DPSO pointL1M = new Position2DPSO();
        Position2DPSO pointL2M = new Position2DPSO();
        Position2DPSO pointL3M = new Position2DPSO();
        Position2DPSO pointL4M = new Position2DPSO();
        Position2DPSO pointL5M = new Position2DPSO();

        double xxcoeffM = 0;
        double yycoeffM = 0;
        double distM = 0;
        double firstcoeffM = 0;
        double secondcoeffM = 0;
        double thirdCoeffM = 0;
        double xLeftD1M = 0;
        double xRightD1M = 0;
        double xxcoeff2M = 0;
        double yycoeff2M = 0;
        double distrobot2ToBallM = 0;
        double firstcoeff2M = 0;
        double thirdCoeff2M = 0;

        double xLeftD2M = 0;
        double xRightD2M = 0;

        double xxcoeffgM = 0;
        double yycoeffgM = 0;
        double distGoalieToBallM = 0;
        double firstcoeffgM = 0;
        double thirdCoeffgM = 0;

        double RightAM = 0;
        double LeftAM = 0;
        double RightBM = 0;
        double LeftBM = 0;
        double RightCM = 0;
        double LeftCM = 0;

        double distanceFromRobotM = 0;
        double intersectXM = 0;
        double intersectYM = 0;
        double goalCenterYPositionM = -0.50;

        HiPerfTimer hptimerM = new HiPerfTimer();
        List<double> timelistM = new List<double>();

        //DrawingClass drawM = new DrawingClass();

        public double MinDistLineBall(Position2DPSO ballPosition, Position2DPSO goalie, Position2DPSO defender1, Position2DPSO defender2)
        {
            Position2DPSO positionGolieM = goalie;
            Position2DPSO positionDef1M = defender1;
            Position2DPSO positionDef2M = defender2;
            double xRobotGoalieM = positionGolieM.X;
            double yRobotGoalieM = positionGolieM.Y;
            double xRobotDef1M = positionDef1M.X;
            double yRobotDef1M = positionDef1M.Y;
            double xRobotDef2M = positionDef2M.X;
            double yRobotDef2M = positionDef2M.Y;

            SumM = 0;
            double slopeBallCenterM = (goalCenterM.Y - ballPosition.Y) / (goalCenterM.X - ballPosition.X);

            double Slop2M = -1 / slopeBallCenterM;
            double xRightM = (((ballPosition.Y + 1) - ballPosition.Y) / Slop2M) + ballPosition.X;
            double xLeftM = (((ballPosition.Y - 1) - ballPosition.Y) / Slop2M) + ballPosition.X;
            Position2DPSO posRightM = new Position2DPSO(xRightM, ballPosition.Y + 1);
            Position2DPSO posLeftM = new Position2DPSO(xLeftM, ballPosition.Y - 1);
            Vector2DPSO vec_RightM = new ParticleSwarm.Vector2DPSO(posRightM.X - ballPosition.X, posRightM.Y - ballPosition.Y);
            Vector2DPSO vec_LeftM = new ParticleSwarm.Vector2DPSO(posLeftM.X - ballPosition.X, posLeftM.Y - ballPosition.Y);
            double sizeM = Math.Sqrt((vec_RightM.X * vec_RightM.X) + (vec_RightM.Y * vec_RightM.Y));
            Vector2DPSO norm_Vec_RightM = new ParticleSwarm.Vector2DPSO(vec_RightM.X / sizeM, vec_RightM.Y / sizeM);
            sizeM = Math.Sqrt((vec_LeftM.X * vec_LeftM.X) + (vec_LeftM.Y * vec_LeftM.Y));
            Vector2DPSO norm_Vec_LeftM = new ParticleSwarm.Vector2DPSO(vec_LeftM.X / sizeM, vec_LeftM.Y / sizeM);
            point0M = ballPosition;
            pointR1M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_RightM.X * .1), ballPosition.Y + (norm_Vec_RightM.Y * .1));
            pointR2M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_RightM.X * .2), ballPosition.Y + (norm_Vec_RightM.Y * .2));
            pointR3M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_RightM.X * .3), ballPosition.Y + (norm_Vec_RightM.Y * .3));
            pointR4M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_RightM.X * .4), ballPosition.Y + (norm_Vec_RightM.Y * .4));
            pointR5M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_RightM.X * .5), ballPosition.Y + (norm_Vec_RightM.Y * .5));
            pointL1M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_LeftM.X * .1), ballPosition.Y + (norm_Vec_LeftM.Y * .1));
            pointL2M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_LeftM.X * .2), ballPosition.Y + (norm_Vec_LeftM.Y * .2));
            pointL3M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_LeftM.X * .3), ballPosition.Y + (norm_Vec_LeftM.Y * .3));
            pointL4M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_LeftM.X * .4), ballPosition.Y + (norm_Vec_LeftM.Y * .4));
            pointL5M = new ParticleSwarm.Position2DPSO(ballPosition.X + (norm_Vec_LeftM.X * .5), ballPosition.Y + (norm_Vec_LeftM.Y * .5));
            Position2DPSO[] ballPointsM = new Position2DPSO[11] { new Position2DPSO(point0M.X, point0M.Y), new Position2DPSO(pointR1M.X, pointR1M.Y), new Position2DPSO(pointR2M.X, pointR2M.Y), new Position2DPSO(pointR3M.X, pointR3M.Y), new Position2DPSO(pointR4M.X, pointR4M.Y), new Position2DPSO(pointR5M.X, pointR5M.Y), new Position2DPSO(pointL1M.X, pointL1M.Y), new Position2DPSO(pointL2M.X, pointL2M.Y), new Position2DPSO(pointL3M.X, pointL3M.Y), new Position2DPSO(pointL4M.X, pointL4M.Y), new Position2DPSO(pointL5M.X, pointL5M.Y) };

            sumcoeffM = 0;
            for (int j = 1; j <= 5; j++)
            {
                double s = .5 - ((j - 1) * .1);
                sumcoeffM = sumcoeffM + (Math.Exp((-(s * s)) / .5));
            }

            sumcoeffM = (sumcoeffM * 2) + 1;

            for (int i = 0; i < 10; i++)
            {
                double dM = Math.Sqrt(Math.Pow((ballPointsM[i].X - point0M.X), 2) + Math.Pow((ballPointsM[i].Y - point0M.Y), 2));
                double guassianFuncXM = Math.Exp((-Math.Pow(dM, 2)) / .5);

                double signyballM = 1;
                double xBallM = ballPointsM[i].X;
                double yBallM = ballPointsM[i].Y;

                if (yBallM != 0)
                    signyballM = yBallM / Math.Abs(yBallM);
                else
                    signyballM = 1;

                xRobotDef1M = positionDef1M.X;
                yRobotDef1M = positionDef1M.Y;
                xRobotDef2M = positionDef2M.X;
                yRobotDef2M = positionDef2M.Y;
                robotRadiM = 0.09;


                xxcoeffM = (xBallM - xRobotDef1M);
                yycoeffM = (yBallM - yRobotDef1M);
                distM = Math.Sqrt(((xxcoeffM * xxcoeffM) + (yycoeffM * yycoeffM)));



                firstcoeffM = (signyballM * Math.Asin(robotRadiM / distM));
                secondcoeffM = (4.05 - xBallM);
                thirdCoeffM = Math.Asin((yRobotDef1M - yBallM) / distM);

                xLeftD1M = secondcoeffM * Math.Tan(thirdCoeffM + firstcoeffM);
                xRightD1M = secondcoeffM * Math.Tan(thirdCoeffM - firstcoeffM);//firstcoeff yek - dar sign darad
                xLeftD1M = yBallM + (signyballM * xLeftD1M);
                xRightD1M = yBallM + (signyballM * xRightD1M);



                xxcoeff2M = (xBallM - xRobotDef2M);
                yycoeff2M = (yBallM - yRobotDef2M);
                distrobot2ToBallM = Math.Sqrt(((xxcoeff2M * xxcoeff2M) + (yycoeff2M * yycoeff2M)));
                //double distrobot2ToBall = Math.Sqrt((((xBall - xRobotDef2) * (xBall - xRobotDef2)) + ((yBall - yRobotDef2) * (yBall - yRobotDef2))));
                firstcoeff2M = (signyballM * Math.Asin(robotRadiM / distrobot2ToBallM));
                thirdCoeff2M = Math.Asin((yRobotDef2M - yBallM) / distrobot2ToBallM);

                xLeftD2M = secondcoeffM * Math.Tan(thirdCoeff2M + firstcoeff2M);
                xRightD2M = secondcoeffM * Math.Tan(thirdCoeff2M - firstcoeff2M);
                xLeftD2M = yBallM + ((signyballM) * (xLeftD2M));
                xRightD2M = yBallM + ((signyballM) * (xRightD2M));


                xxcoeffgM = (xBallM - xRobotGoalieM);
                yycoeffgM = (yBallM - yRobotGoalieM);
                distGoalieToBallM = Math.Sqrt((xxcoeffgM * xxcoeffgM) + (yycoeffgM * yycoeffgM));
                //  double distGoalieToBall = Math.Sqrt((((xBall - xRobotGoalie) * (xBall - xRobotGoalie)) + ((yBall - yRobotGoalie) * (yBall - yRobotGoalie))));

                firstcoeffgM = (signyballM * Math.Asin(robotRadiM / distGoalieToBallM));
                thirdCoeffgM = Math.Asin((yRobotGoalieM - yBallM) / distGoalieToBallM);

                double xLeftGoalieM = secondcoeffM * Math.Tan(thirdCoeffgM + firstcoeffgM);
                double xRightGoalieM = secondcoeffM * Math.Tan(thirdCoeffgM - firstcoeffgM);
                xLeftGoalieM = yBallM + ((signyballM) * (xLeftGoalieM));
                xRightGoalieM = yBallM + ((signyballM) * (xRightGoalieM));

                RightAM = xLeftD1M + .5;
                LeftAM = xRightD1M + .5;
                RightBM = xLeftD2M + .5;
                LeftBM = xRightD2M + .5;
                RightCM = xLeftGoalieM + .5;
                LeftCM = xRightGoalieM + .5;

                LeftAM = Math.Max(Math.Min(LeftAM, 1), 0);
                RightAM = Math.Max(Math.Min(RightAM, 1), 0);
                LeftBM = Math.Max(Math.Min(LeftBM, 1), 0);
                RightBM = Math.Max(Math.Min(RightBM, 1), 0);
                LeftCM = Math.Max(Math.Min(LeftCM, 1), 0);
                RightCM = Math.Max(Math.Min(RightCM, 1), 0);

                if (LeftBM == 0)
                    LeftBM = LeftBM + 0.001;

                if (LeftCM == 0)
                    LeftCM = LeftCM + 0.002;


                if (LeftAM < LeftBM && LeftAM < LeftCM)
                {
                    leftMostPointM = LeftAM;
                    rightLeftmostPointM = RightAM;
                    if (LeftBM < LeftCM)
                    {
                        leftMiddlePointM = LeftBM;
                        rightLeftMiddlePintM = RightBM;
                        leftLessPointM = LeftCM;
                        rightLeftLessPointM = RightCM;
                    }
                    else
                    {
                        leftMiddlePointM = LeftCM;
                        rightLeftMiddlePintM = RightCM;
                        leftLessPointM = LeftBM;
                        rightLeftLessPointM = RightBM;
                    }
                }
                else if (LeftBM < LeftAM && LeftBM < LeftCM)
                {
                    leftMostPointM = LeftBM;
                    rightLeftmostPointM = RightBM;
                    if (LeftAM < LeftCM)
                    {
                        leftMiddlePointM = LeftAM;
                        rightLeftMiddlePintM = RightAM;
                        leftLessPointM = LeftCM;
                        rightLeftLessPointM = RightCM;
                    }
                    else
                    {
                        leftMiddlePointM = LeftCM;
                        rightLeftMiddlePintM = RightCM;
                        leftLessPointM = LeftAM;
                        rightLeftLessPointM = RightAM;
                    }
                }
                else if (LeftCM < LeftAM && LeftCM < LeftBM)
                {
                    leftMostPointM = LeftCM;
                    rightLeftmostPointM = RightCM;
                    if (LeftAM < LeftBM)
                    {
                        leftMiddlePointM = LeftAM;
                        rightLeftMiddlePintM = RightAM;
                        leftLessPointM = LeftBM;
                        rightLeftLessPointM = RightBM;
                    }
                    else
                    {
                        leftMiddlePointM = LeftBM;
                        rightLeftMiddlePintM = RightBM;
                        leftLessPointM = LeftAM;
                        rightLeftLessPointM = RightAM;
                    }
                }

                LeftAM = leftMostPointM;
                RightAM = rightLeftmostPointM;
                if (RightAM <= LeftAM)
                    RightAM = LeftAM + .001;

                LeftBM = leftMiddlePointM;
                RightBM = rightLeftMiddlePintM + .001;
                if (RightBM <= LeftBM)
                    RightBM = LeftBM + .001;

                LeftCM = leftLessPointM;
                RightCM = rightLeftLessPointM + .001;
                if (RightCM <= LeftCM)
                    RightCM = LeftCM + .001;

                double totalM = Math.Abs(RightAM - LeftAM) + Math.Abs(RightBM - LeftBM) + Math.Abs(RightCM - LeftCM);
                double percentM = 0;

                bool RightALeftBM = RightAM <= LeftBM;
                bool LeftBRightBM = LeftBM <= RightBM;
                bool RightBLeftCM = RightBM <= LeftCM;
                bool LeftCRightCM = LeftCM <= RightCM;
                bool LeftBRightAM = LeftBM <= RightAM;
                bool RightARightBM = RightAM <= RightBM;
                bool LeftBLeftCM = LeftBM <= LeftCM;
                bool LeftCLeftAM = LeftCM <= RightAM;
                bool RightBRightCM = RightBM <= RightCM;
                bool LeftCRightBM = LeftCM <= RightBM;
                bool RightCRightBM = RightCM <= RightBM;
                bool RightARightCM = RightAM <= RightCM;
                ////////////////--_F----
                bool flag_F1M = RightALeftBM && LeftBRightBM && RightBLeftCM && LeftCRightCM;
                bool flag_F2M = LeftBRightAM && RightARightBM && RightBLeftCM && LeftCRightCM;
                bool flag_F3M = LeftBLeftCM && LeftCLeftAM && RightALeftBM && RightBRightCM;
                bool flag_F4M = LeftBLeftCM && LeftCRightBM && RightBRightCM && (RightCM <= RightAM);
                bool flag_F5M = LeftBLeftCM && LeftCRightBM && (RightBM <= RightAM) && RightARightCM;
                bool flag_F6M = LeftBLeftCM && LeftCRightCM && RightCRightBM && (RightBM <= RightAM);
                bool flag_F7M = LeftBLeftCM && LeftCLeftAM && RightARightCM && RightCRightBM;
                bool flag_F8M = RightALeftBM && LeftBLeftCM && LeftCRightBM && RightBRightCM;
                bool flag_F9M = LeftBLeftCM && LeftCRightCM && (RightCM <= RightAM) && RightARightBM;
                ////////////////--_C----
                bool flag_C1M = LeftBRightBM && RightBLeftCM && LeftCRightCM && (RightCM <= RightAM);
                bool flag_C2M = LeftBRightBM && RightBLeftCM && LeftCLeftAM && RightARightCM;
                bool flag_C3M = LeftBRightAM && (RightAM <= LeftCM) && LeftCRightBM && RightBRightCM;
                //////////////--_G----
                bool flag_G1M = LeftBRightBM && (RightBM <= RightAM) && (RightAM <= LeftCM) && LeftCRightCM;
                /////////////-_A----
                bool flag_AM = RightALeftBM && LeftBLeftCM && LeftCRightCM && RightCRightBM;
                //////////////--_I----
                bool flag_M = LeftBRightAM && (RightAM <= LeftCM) && LeftCRightCM && RightCRightBM;

                bool flag_FM = flag_F1M || flag_F2M || flag_F2M || flag_F3M || flag_F4M || flag_F5M || flag_F6M || flag_F7M || flag_F8M || flag_F9M;
                bool flag_CM = flag_C1M || flag_C2M || flag_C3M;
                bool flag_GM = flag_G1M;
                if (flag_FM)
                    percentM = totalM - ((Math.Min(RightCM, Math.Max(LeftBM, RightAM)) - Math.Min(LeftCM, RightBM)) + (RightBM - LeftBM));
                else if (flag_AM)
                    percentM = totalM - ((Math.Min(RightCM, Math.Max(LeftBM, RightAM)) - Math.Min(LeftCM, RightBM)) + (RightCM - LeftBM));
                else if (flag_M)
                    percentM = totalM - (Math.Min(RightCM, Math.Max(LeftBM, RightAM)) - LeftCM + (RightCM - LeftBM));
                else if (flag_CM)
                    percentM = totalM - ((Math.Min(RightCM, Math.Max(LeftBM, RightAM)) - LeftCM) + (RightBM - LeftBM));
                else if (flag_GM)
                    percentM = totalM - ((Math.Min(RightCM, RightBM) - LeftBM));
                else
                    errorpM = errorpM + 1;

                double JM = (100 - (percentM * 100)) / 100;
                SumM = (JM * (guassianFuncXM / sumcoeffM)) + SumM;
            }
            ////////// Distance Cost--------------------------------
            double DistDef1M = Math.Sqrt(Math.Pow((xRobotDef1M - goalCenterM.X), 2) + Math.Pow((yRobotDef1M - goalCenterM.Y), 2));
            double DistDef2M = Math.Sqrt(Math.Pow((xRobotDef2M - goalCenterM.X), 2) + Math.Pow((yRobotDef2M - goalCenterM.Y), 2));
            double DistGoalieM = Math.Sqrt(Math.Pow((xRobotGoalieM - goalCenterM.X), 2) + Math.Pow((yRobotGoalieM - goalCenterM.Y), 2));
            double TotalDistanceM = DistDef1M + DistDef2M + DistGoalieM;
            SumM = (SumM + TotalDistanceM);
            return SumM;
        }
        /// <summary>
        /// temporal or time horizon goal convergence created with this target 
        /// that if we cant convergence all of goal we can do it with maximum 
        /// confidence for arrive to ball after kick in minimum of time
        /// 
        /// </summary>
        /// <param name="ballPosition">Ball Position</param>
        /// <param name="goalie">Goalie Position</param>
        /// <param name="defender1">Defender 1 Position </param>
        /// <param name="defender2">Defender 2 Position</param>
        /// <returns>Cost of Inputs (integerated distance of all robot to lines</returns>
        public double Temporal(Position2DPSO ballPosition, Position2DPSO goalie, Position2DPSO defender1, Position2DPSO defender2, Position2DPSO LastPosGoal, Position2DPSO LastPosDef1, Position2DPSO LastPosDef2)
        {
            //coeff of Distance equation
            double firstcoeffM = ballPosition.X - 4.045;
            //I consider 50 point on goal 
            for (int i = 0; i < 50; i++)
            {
                //coeff of distance Calculation
                double secondCoeffM = ballPosition.Y - goalCenterYPositionM;
                double thirdCoeffM = (ballPosition.X * goalCenterYPositionM) - (ballPosition.Y * 4.045);
                double forthCoeffM = Math.Sqrt((ballPosition.Y - goalCenterYPositionM) * (ballPosition.Y - goalCenterYPositionM) + ((ballPosition.X - 4.045) * (ballPosition.X - 4.045)));

                //line generating
                double lineSlopeM = secondCoeffM / firstcoeffM;
                double cM = (lineSlopeM * 4.045) + goalCenterYPositionM;
                double accoeffM = lineSlopeM * cM;
                double bccoeffM = -cM;
                double abcoeffM = -lineSlopeM;
                double a2coeffM = lineSlopeM * lineSlopeM;
                double a2b2M = a2coeffM + 1;

                //CalCulate Distance of each Robot To Specific Calculated Line 
                double distanceRobot1M = Math.Abs((secondCoeffM * defender1.X) - (firstcoeffM * defender1.Y) + thirdCoeffM) / forthCoeffM;
                double distanceRobot2M = Math.Abs((secondCoeffM * defender2.X) - (firstcoeffM * defender2.Y) + thirdCoeffM) / forthCoeffM;
                double distanceGoalieM = Math.Abs((secondCoeffM * goalie.X) - (firstcoeffM * goalie.Y) + thirdCoeffM) / forthCoeffM;

                //Compare of distances for select nearest Robot To Line
                if (distanceGoalieM < distanceRobot1M && distanceGoalieM < distanceRobot2M)
                {
                    //X Coordinate of Intersection
                    intersectXM = (goalie.X - (abcoeffM * goalie.Y) - accoeffM) / a2b2M;
                    //Y Coordinate of Intersection
                    intersectYM = ((-abcoeffM * goalie.X) + (a2coeffM * goalie.Y) - bccoeffM) / a2b2M;
                    // Distance Of InterSection To Robot
                    distanceFromRobotM = distanceGoalieM;
                }
                else if (distanceRobot1M < distanceRobot2M && distanceRobot1M < distanceGoalieM)
                {
                    //X Coordinate of Intersection
                    intersectXM = ((defender1.X) - (abcoeffM * defender1.Y) - accoeffM) / a2b2M;
                    //Y Coordinate of Intersection
                    intersectYM = ((-(abcoeffM) * defender1.X) + (a2coeffM * defender1.Y) - bccoeffM) / a2b2M;
                    // Distance of Intersection to Robot
                    distanceFromRobotM = distanceRobot1M;
                }
                else if (distanceRobot2M < distanceRobot1M && distanceRobot2M < distanceGoalieM)
                {
                    //X Coordinate of Intersection
                    intersectXM = ((defender2.X) - (abcoeffM * defender2.Y) - accoeffM) / a2b2M;
                    //Y Coordinate of Intersection
                    intersectYM = ((-(abcoeffM) * defender2.X) + (a2coeffM * defender2.Y) - bccoeffM) / a2b2M;
                    //Distance Robot To Intersection
                    distanceFromRobotM = distanceRobot2M;
                }
                double distanceFromBallM = Math.Sqrt(((intersectYM - ballPosition.Y) * (intersectYM - ballPosition.Y)) + ((intersectXM - ballPosition.X) * (intersectXM - ballPosition.X)));
                //Actual Robot Radius
                double RobotRadiusNormal = .09;

                //Distance Affect on Robot Radius it means if ball is far we can consider 
                //the radius of Robot is more than 0.09 Cm and we can increase the radius
                //of Robot
                //kinematic formulation of ball arrive time i consider velocity of ball 8 m/s
                double ballArriveTime = (Math.Sqrt(((ballPosition.X - GoalCenter.X) * (ballPosition.X - GoalCenter.X)) + ((ballPosition.Y - GoalCenter.Y) * (ballPosition.Y - GoalCenter.Y))) - 2) / 8;

                //kinematic formulation of ball and Robot Move with 2 m/s^2 acceleration 
                double CalculatedRobotRadius = .5 * .5 * 2 * (ballArriveTime * ballArriveTime) + .09;
                if (distanceFromRobotM < Math.Max(RobotRadiusNormal, CalculatedRobotRadius))
                    distanceFromRobotM = 0;

                double ratioM = distanceFromRobotM / distanceFromBallM;

                SumM = SumM + (ratioM);


                goalCenterYPositionM = goalCenterYPositionM + .02;
            }
            double DistanceGoalie = Math.Sqrt(((LastPosGoal.X - goalie.X) * (LastPosGoal.X - goalie.X) + (LastPosGoal.Y - goalie.Y) * (LastPosGoal.Y - goalie.Y)));
            double DistanceDef1 = Math.Sqrt(((LastPosGoal.X - defender1.X) * (LastPosGoal.X - defender1.X) + (LastPosGoal.Y - defender1.Y) * (LastPosGoal.Y - defender1.Y)));
            double DistanceDef2 = Math.Sqrt(((LastPosGoal.X - defender2.X) * (LastPosGoal.X - defender2.X) + (LastPosGoal.Y - defender2.Y) * (LastPosGoal.Y - defender2.Y)));
            double DistanceCostCoeff = .4;
            double distanceCost  = (DistanceDef1 + DistanceDef2 + DistanceGoalie);
            return SumM + distanceCost;
        }

        public float Temporal(Position2DPSO ballPosition, Position2DPSO goalie, Position2DPSO defender1, Position2DPSO defender2, bool floatComputing)
        {
            for (int i = 0; i < 50; i++)
            {
                float firstcoeffM = (float)(ballPosition.X - 4.045);
                float secondCoeffM = (float)(ballPosition.Y - (float)goalCenterYPositionM);
                float thirdCoeffM = (float)((ballPosition.X * goalCenterYPositionM) - (ballPosition.Y * 4.045));
                float forthCoeffM = (float)(Math.Sqrt(((ballPosition.Y - goalCenterYPositionM) * (ballPosition.Y - goalCenterYPositionM)) + ((ballPosition.X - 4.045) * (ballPosition.X - 4.045))));

                float lineSlopeM = (secondCoeffM) / (firstcoeffM);
                float aM = lineSlopeM;
                float bM = -1;
                float cM = (float)((lineSlopeM * 4.045) + goalCenterYPositionM);
                float accoeffM = (aM * cM);
                float bccoeffM = (bM * cM);
                float abcoeffM = aM * bM;
                float b2coeffM = bM * bM;
                float a2coeffM = aM * aM;
                float a2b2M = a2coeffM + b2coeffM;

                float distanceRobot1M = (float)Math.Abs(((secondCoeffM) * defender1.X) - ((firstcoeffM) * defender1.Y) + thirdCoeffM) / forthCoeffM;
                float distanceRobot2M = (float)Math.Abs(((secondCoeffM) * defender2.X) - ((firstcoeffM) * defender2.Y) + thirdCoeffM) / forthCoeffM;
                float distanceGoalieM = (float)Math.Abs(((secondCoeffM) * goalie.X) - ((firstcoeffM) * goalie.Y) + thirdCoeffM) / forthCoeffM;


                if (distanceRobot1M < distanceRobot2M && distanceRobot1M < distanceGoalieM)
                {
                    intersectXM = ((b2coeffM * defender1.X) - (abcoeffM * defender1.Y) - accoeffM) / a2b2M;
                    intersectYM = ((-abcoeffM * defender1.X) + (a2coeffM * defender1.Y) - bccoeffM) / a2b2M;
                    distanceFromRobotM = distanceRobot1M;
                }
                else if (distanceRobot2M <= distanceRobot1M && distanceRobot2M < distanceGoalieM)
                {
                    intersectXM = ((b2coeffM * defender2.X) - (abcoeffM * defender2.Y) - accoeffM) / a2b2M;
                    intersectYM = ((-abcoeffM * defender2.X) + (a2coeffM * defender2.Y) - bccoeffM) / a2b2M;
                    distanceFromRobotM = distanceRobot2M;
                }
                else if (distanceGoalieM < distanceRobot1M && distanceGoalieM < distanceRobot2M)
                {
                    intersectXM = ((b2coeffM * goalie.X) - (abcoeffM * goalie.Y) - accoeffM) / a2b2M;
                    intersectYM = ((-abcoeffM * goalie.X) + (a2coeffM * goalie.Y) - bccoeffM) / a2b2M;
                    distanceFromRobotM = distanceGoalieM;
                }
                double distanceFromBall = Math.Sqrt(((intersectYM - ballPosition.Y) * (intersectYM - ballPosition.Y)) + ((intersectXM - ballPosition.X) * (intersectXM - ballPosition.X)));
                if (distanceFromRobotM < .09)
                    distanceFromRobotM = 0;

                double ratio = distanceFromRobotM / distanceFromBall;

                SumM = SumM + (ratio);
                goalCenterYPositionM = goalCenterYPositionM + .02;
            }
            return (float)SumM;
        }

        #endregion

    }
}
