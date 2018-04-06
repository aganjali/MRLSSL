using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using System.Threading;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.Planning.GamePlanner.Types
{
    public class GamePlannerInfo
    {
        GPTeam ourTeam, oppTeam;
        GPosition2D passPoint;
        Position2D topleft;
        Vector2D size;
        Regions region;
        FlatRectangle R;
        double xStep, yStep;
        float[,] directGoodness, chipGoodness;

        static double minPassOneTouchSpeed = 3, maxPassOneTouchSpeed = 6, minPassCatchSpeed = 4,
            maxPassCatchSpeed = 5, minPassDist = 1, maxPassDist = 5.2, minChipCatchCoef = 0.2,
            maxChipCatchCoef = 0.6, minChipOneTouchCoef = 0.5, maxChipOneTouchCoef = 0.55;
        public double[] DriblePower = { 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80, 80 };
        public float[,] ChipGoodness
        {
            get { return chipGoodness; }
            set { chipGoodness = value; }
        }

        public float[,] DirectGoodness
        {
            get { return directGoodness; }
            set { directGoodness = value; }
        }
        bool chipIsSutaible;
        float passPointScore;
        List<VisibleGoalInterval> ourGoalIntervals, oppGoalIntervals;
        bool Debug = false;

        public List<VisibleGoalInterval> OppGoalIntervals
        {
            get { return oppGoalIntervals; }
            set { oppGoalIntervals = value; }
        }

        public List<VisibleGoalInterval> OurGoalIntervals
        {
            get { return ourGoalIntervals; }
            set { ourGoalIntervals = value; }
        }
        public bool ChipIsSutaible
        {
            get { return chipIsSutaible; }
            set { chipIsSutaible = value; }
        }
        public GPosition2D PassPoint
        {
            get { return passPoint; }
            set { passPoint = value; }
        }
        public float PassPointScore
        {
            get { return passPointScore; }
            set { passPointScore = value; }
        }
        public GamePlannerInfo()
        {
            ourTeam = new GPTeam();
            oppTeam = new GPTeam();
            passPoint = new GPosition2D();
            ourGoalIntervals = new List<VisibleGoalInterval>();
            oppGoalIntervals = new List<VisibleGoalInterval>();
            chipIsSutaible = false;
            passPointScore = 0;
            topleft = new GPosition2D(0, 0.8f * (float)MRL.SSL.GameDefinitions.GameParameters.OppLeftCorner.Y);
            size = new GVector2D(0.8f * (float)MRL.SSL.GameDefinitions.GameParameters.OppGoalCenter.X, 0.8f * (float)(MRL.SSL.GameDefinitions.GameParameters.OppRightCorner.Y - MRL.SSL.GameDefinitions.GameParameters.OppLeftCorner.Y));
            R = new FlatRectangle(topleft, size);
            region = new Regions();
        }
        public GPTeam OppTeam
        {
            get { return oppTeam; }
            set { oppTeam = value; }
        }

        public GPTeam OurTeam
        {
            get { return ourTeam; }
            set { ourTeam = value; }
        }
        public GPTeam this[Color col]
        {
            get
            {
                if (ourTeam.Color.ToArgb() == col.ToArgb())
                    return ourTeam;
                else if (oppTeam.Color.ToArgb() == col.ToArgb())
                    return oppTeam;
                else
                    return null;
            }
            set
            {
                if (ourTeam.Color.ToArgb() == col.ToArgb())
                    ourTeam = value;
                else if (oppTeam.Color.ToArgb() == col.ToArgb())
                    oppTeam = value;
            }
        }
        public void CalculateMaxScoreInGrid(out double BestScore, out Position2D BestPassPoint, out bool isChipSutaible, FlatRectangle Grid)
        {
            DrawingObjects.AddObject(Grid);
            DrawingObjects.AddObject(R, "Rec");
            DrawingObjects.AddObject(R.TopLeft);
            double directKickFavor = 1.8;
            Position2D bestDir = new Position2D();
            Position2D bestChip = new Position2D();
            double MaxDirectGoodness = 0;
            double MaxChipGoodness = 0;
            isChipSutaible = false;
            BestPassPoint = new Position2D();
            BestScore = 0;
            if (directGoodness == null || chipGoodness == null || Grid == null)
                return;
            MaxDirectGoodness = double.MinValue;
            MaxChipGoodness = double.MinValue;

            xStep = R.Width / (double)(DirectGoodness.GetLength(0) - 1);
            yStep = R.Height / (double)(DirectGoodness.GetLength(1) - 1);

            double dx = Grid.TopLeft.X - R.TopLeft.X;
            double dy = Grid.TopLeft.Y - R.TopLeft.Y;
            int xGridCount = (int)Math.Abs(Grid.Width / xStep);
            int yGridCount = (int)Math.Abs(Grid.Height / yStep);
            int xi = (int)(dx / xStep);
            int yi = (int)(dy / yStep);
            for (int i = 0; i < xGridCount; i++)
            {
                for (int j = 0; j < yGridCount; j++)
                {
                    if (MaxDirectGoodness < directGoodness[xi + i, yi + j])
                    {
                        MaxDirectGoodness = directGoodness[xi + i, yi + j];
                        bestDir = new Position2D(topleft.X + (xi + i) * xStep, topleft.Y + (yi + j) * yStep);
                    }
                    if (MaxChipGoodness < chipGoodness[xi + i, yi + j])
                    {
                        MaxChipGoodness = chipGoodness[xi + i, yi + j];
                        bestChip = new Position2D(topleft.X + (xi + i) * xStep, topleft.Y + (yi + j) * yStep);
                    }
                }
            }
            if (MaxChipGoodness > directKickFavor * MaxDirectGoodness)
            {
                isChipSutaible = true;
                BestScore = MaxChipGoodness;
                BestPassPoint = bestChip;
            }
            else
            {
                isChipSutaible = false;
                BestScore = MaxDirectGoodness;
                BestPassPoint = bestDir;
            }
        }
        public List<VisibleGoalInterval> GetVisibleIntervals(WorldModel Model, Position2D From, Position2D Start, Position2D End, bool useOpp, bool useOur, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {
            return region.GetVisibleIntervals(Model, From, Start, End, useOpp, useOur, OpponentIDToExclude, robotIDsToExclude);
        }
        public Position2D? GetAGoodTargetPointInGoal(WorldModel Model, Position2D? LastSelectedTargetPoint, Position2D FromLocation, out double goodness, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, params int[] robotIDsToExclude)
        {
            return region.GetAGoodTargetPointInGoal(Model, LastSelectedTargetPoint, FromLocation, out goodness, GoalStart, GoalEnd, UseOpponents, UseOurRobots, OpponentIDToExclude, robotIDsToExclude);
        }
        public Position2D? GetAGoodTargetPointInGoal(WorldModel Model, Position2D? LastSelectedTargetPoint, Position2D FromLocation, out double goodness, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, int? OpponentIDToExclude, List<Position2D?> Pos2Exclude)
        {
            return region.GetAGoodTargetPointInGoal(Model, LastSelectedTargetPoint, FromLocation, out goodness, GoalStart, GoalEnd, UseOpponents, UseOurRobots, OpponentIDToExclude, Pos2Exclude);
        }

        public double CalculateKickSpeed(WorldModel Model, int RobotID, Position2D From, Position2D To, bool isChip, bool onetouch)
        {
            double dist = Math.Min(Math.Max(From.DistanceFrom(To), minPassDist), maxPassDist);
            if (isChip)
            {
                double coef = (dist - minPassDist) / (maxPassDist - minPassDist);
                if (onetouch)
                {
                    coef = coef * (maxChipOneTouchCoef - minChipOneTouchCoef) + minChipOneTouchCoef;
                    return StaticVariables.ChipCoef[RobotID] * dist * coef;
                }
                else
                {

                    coef = coef * (maxChipCatchCoef - minChipCatchCoef) + minChipCatchCoef;
                    return StaticVariables.ChipCoef[RobotID] * dist * coef;
                }
            }
            else
            {
                dist = (dist - minPassDist) / (maxPassDist - minPassDist);
                if (onetouch)
                    return StaticVariables.DirectCoef[RobotID] * (dist * (maxPassOneTouchSpeed - minPassOneTouchSpeed) + minPassOneTouchSpeed);
                else
                    return StaticVariables.DirectCoef[RobotID] * (dist * (maxPassCatchSpeed - minPassCatchSpeed) + minPassCatchSpeed);
            }
        }
        //public double CalculateKickSpeed(WorldModel Model, int RobotID, Position2D From, Position2D To, bool isChip, bool onetouch, out double kickSpeed)
        //{
        //    double dist = Math.Min(Math.Max(From.DistanceFrom(To), minPassDist), maxPassDist);
        //    double res = 0;
        //    kickSpeed = 0;
        //    if (isChip)
        //    {
        //        double coef = (dist - minPassDist) / (maxPassDist - minPassDist);
        //        if (onetouch)
        //        {
        //            coef = coef * (maxChipOneTouchCoef - minChipOneTouchCoef) + minChipOneTouchCoef;
        //            res =  dist * coef;
        //            kickSpeed = ChipCoef[RobotID] * res;
        //        }
        //        else
        //        {

        //            coef = coef * (maxChipCatchCoef - minChipCatchCoef) + minChipCatchCoef;
        //            res = dist * coef;
        //            kickSpeed = ChipCoef[RobotID] * res;
        //        }
        //    }
        //    else
        //    {
        //        dist = (dist - minPassDist) / (maxPassDist - minPassDist);
        //        if (onetouch)
        //        {
        //            res = (dist * (maxPassOneTouchSpeed - minPassOneTouchSpeed) + minPassOneTouchSpeed);
        //            kickSpeed = DirectCoef[RobotID] * res;
        //        }
        //        else
        //        {
        //            res = (dist * (maxPassCatchSpeed - minPassCatchSpeed) + minPassCatchSpeed);
        //            kickSpeed = DirectCoef[RobotID] * res;
        //        }
        //    }
        //    return res;
        //}
        private List<int> OrderPoints(WorldModel Model, Position2D Target, List<SearchPos> Searchposes, int n, double ka, double ke, double kd, double kx, double ky, Position2D topLeft, double w, double h, double step, bool isChip)
        {

            List<double> or = new List<double>();
            int i = 0;
            Dictionary<int, double> dic = new Dictionary<int, double>();
            double xOffset = (isChip) ? 0 : 0.2;
            foreach (var item in Searchposes)
            {
                double e = 1 - (item.point + 6.0) / 7.0;
                double d = (item.Location.DistanceFrom(Target) - 1) / 4.0;
                double x = (Math.Abs((Model.BallState.Location.X - xOffset) - item.Location.X) + 0) / h;
                double y = 1 - (Math.Abs(item.Location.Y - Model.BallState.Location.Y) - (Math.Abs(topLeft.Y - Model.BallState.Location.Y) - w)) / w;
                double av = 1 - (item.angleView / 0.7);
                dic[i++] = (ke * e + kd * d + kx * x + ky * y + ka * av) / (ke + kd + kx + ky + ka);
            }
            dic = dic.OrderBy(o => o.Value).ToDictionary(kk => kk.Key, v => v.Value);
            return dic.Keys.ToList();
        }
        public bool BestPassPoint(WorldModel Model, Position2D Target, Position2D topLeft, double width, double heigth, int Rows, int column, ref bool isChip, ref Position2D bestPoint/*, ref bool chipIsBetter*/)
        {

            List<Position2D> CenterofRegions = CenterRegion(topLeft, width, heigth, Rows, column);
            List<SearchPos> Searchposes = new List<SearchPos>();

            foreach (var item in CenterofRegions)
            {
                DrawingObjects.AddObject(item);
            }
            int i = 0;
            do
            {
                if (i == 1)
                    isChip = true;
                foreach (var item in CenterofRegions)
                {
                    if (GameParameters.IsInField(item, 0.3) && !Meetobstacle(Model, Target, item, isChip))
                    {
                        SearchPos pointp = new SearchPos();
                        pointp.Location = item;
                        pointp.point = gaussian(Model, item);
                        var tmpInter = GetVisibleIntervals(Model, item, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, OppTeam.GoaliID);
                        double anglview = 0;
                        foreach (var item2 in tmpInter)
                        {
                            if ((Math.Sign(item2.interval.Start) != 0 && Math.Sign(item2.interval.End) != 0) && Math.Sign(item2.interval.Start) != Math.Sign(Math.Sign(item2.interval.End)))
                            {
                                anglview = item2.ViasibleWidth;
                                break;
                            }
                        }
                        pointp.angleView = anglview;
                        Searchposes.Add(pointp);
                    }
                }
                i++;
            } while (!isChip && Searchposes.Count == 0);

            List<int> bestIdxes = OrderPoints(Model, Target, Searchposes, 60, 2, 10, 0, 3, 4.5, topLeft, width, heigth, column, isChip);

            if (bestIdxes.Count > 0)
            {
                bestPoint = Searchposes[bestIdxes.First()].Location;
            }
            else
                bestPoint = CenterofRegions.First();
            return (bestIdxes.Count > 0);
        }
        //public Position2D BestPassPointInFront(WorldModel Model, Position2D shootTarget, Position2D topLeft, double width, double height, int Rows, int column)
        //{
        //    List<Position2D> CenterofRegions = Centerregion(topLeft, width, height, Rows, column);
        //    List<SearchPos> Searchposes = new List<SearchPos>();
        //    Vector2D vr = GameParameters.OppGoalRight - Model.BallState.Location;
        //    Vector2D vl = GameParameters.OppGoalLeft - Model.BallState.Location;
        //    foreach (var item in CenterofRegions)
        //    {
        //        Vector2D v = item - Model.BallState.Location;
        //        if(
        //    }
        //}
        private double gaussian(WorldModel Model, Position2D Input)
        {
            double coeff = 0.08;
            double point = 0;
            if (Model.Opponents != null)
            {
                foreach (var item in Model.Opponents)
                {
                    if (item.Value.Location.DistanceFrom(Input) < .9)
                    {
                        double pow = (-((Input.X - item.Value.Location.X) * (Input.X - item.Value.Location.X)) - ((Input.Y - item.Value.Location.Y) * (Input.Y - item.Value.Location.Y)));
                        pow /= coeff;
                        double gaussians = Math.Pow(Math.E, pow);
                        point += gaussians;
                    }
                }

                point = 1 - point;
            }
            return point;
        }
        private bool Meetobstacle(WorldModel Model, Position2D target, Position2D SelectPoint, bool isChip/*, ref bool chipIsBetter*/)
        {

            Obstacles obs = new Obstacles(Model);
            List<int> oppIds = new List<int>();
            if (oppTeam.GoaliID.HasValue)
            {
                oppIds.Add(oppTeam.GoaliID.Value);
            }
            else
                oppIds = null;
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), oppIds);
            bool b = (!isChip && obs.Meet(Model.BallState, new SingleObjectState(SelectPoint, Vector2D.Zero, null), 0.1)); ;
            //  chipIsBetter |= b;
            return (b) ||
                obs.Meet(new SingleObjectState(SelectPoint, Vector2D.Zero, null), new SingleObjectState(target, Vector2D.Zero, null), 0.04);
        }
        private bool Meetobstacle(WorldModel Model, Position2D From, Position2D target, Position2D SelectPoint, bool isChip, List<int> ourExcludeIds)
        {

            Obstacles obs = new Obstacles(Model);
            List<int> oppIds = new List<int>();
            if (oppTeam.GoaliID.HasValue)
            {
                oppIds.Add(oppTeam.GoaliID.Value);
            }
            else
                oppIds = null;
            obs.AddObstacle(1, 0, 0, 0, ourExcludeIds, oppIds);
            bool b = (!isChip && obs.Meet(new SingleObjectState(From, Vector2D.Zero, 0), new SingleObjectState(SelectPoint, Vector2D.Zero, null), 0.022));
            //  chipIsBetter |= b;
            return (b) ||
                obs.Meet(new SingleObjectState(SelectPoint, Vector2D.Zero, null), new SingleObjectState(target, Vector2D.Zero, null), 0.04);
        }
        private struct SearchPos
        {
            public Position2D Location;
            public double point;
            public double angleView;
        }
        private List<Position2D> CenterRegion(Position2D TopLeft, double width, double heigth, int Rows, int Column)
        {
            double WidthStep = width / Rows;
            double HeightStep = heigth / Column;
            double firstY = HeightStep / 2;
            double firstX = WidthStep / 2;
            List<Position2D> CenterPoints = new List<Position2D>();
            Position2D FirstCenter = TopLeft.Extend(-firstX, firstY);
            CenterPoints.Add(FirstCenter);
            for (int i = 0; i < Column; i++)
            {
                if (i > 0)
                {
                    FirstCenter = FirstCenter.Extend(0, HeightStep);
                    CenterPoints.Add(FirstCenter);
                    //DrawingObjects.AddObject("centerppscore" + FirstCenter.toString(), FirstCenter);
                }

                for (int j = 1; j < Rows; j++)
                {
                    Position2D NextCenter = FirstCenter.Extend(-j * WidthStep, 0);
                    CenterPoints.Add(NextCenter);
                    //    DrawingObjects.AddObject("centerppscore" + NextCenter.toString(), NextCenter);
                }


            }


            return CenterPoints;
        }
        public List<Position2D> BestPassPoint(WorldModel Model, Position2D From, Position2D Target, Position2D topLeft, List<int> ourExcludeIDs, int? passer, double passSpeed, double shootSpeed, double width, double heigth, int Rows, int column)
        {
            List<Position2D> CenterofRegions = CenterRegion(topLeft, width, heigth, Rows, column);
            Position2D otPos = new Position2D(), crPos = new Position2D();
            if (Debug)
            {
                foreach (var item in CenterofRegions)
                {
                    DrawingObjects.AddObject(item);
                }
            }
            Vector2D ballPVec, ballPPrepVec;
            Line ballPLine = new Line(), pTargetLine = new Line();
            double accel = 5, speed = 3;
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
            double maxScoreCr = double.MinValue, maxScoreOt = double.MinValue;
            double[] scoresOt = new double[CenterofRegions.Count], scoresCr = new double[CenterofRegions.Count];
            double minPassD = 0.0;
            int index = -1;
            double dist, DistFromBorder;
            foreach (var item in CenterofRegions)
            {
                index++;
                if (!GameParameters.IsInField(item, -0.1) || GameParameters.IsInDangerousZone(item, true, 0.1, out dist, out DistFromBorder))
                {
                    continue; ;
                } if (Meetobstacle(Model, From, Target, item, false, ourExcludeIDs))
                {
                    continue; ;
                }
                if (item.DistanceFrom(From) < minPassD)
                    continue;
                ballPLine = new Line(From, item);
                pTargetLine = new Line(item, Target);
                ballPVec = From - item;
                ballPPrepVec = Vector2D.FromAngleSize(ballPVec.AngleInRadians + Math.PI / 2, 1);
                double tP = ballPVec.Size / passSpeed;
                double s = tP * accel;
                double t2 = 0, t1 = tP;
                if (s > speed)
                {
                    t2 = t1 - speed / accel;
                    t1 = speed / accel;
                }
                double d = 0.5 * accel * t1 * t1 + speed * t2;
                double d1 = d, d2 = d;
                ballPPrepVec.NormalizeTo(d);
                int idx;
                if (obs.Meet(new SingleObjectState(item, Vector2D.Zero, 0), new SingleObjectState(item + ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d1 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(item) - RobotParameters.OurRobotParams.Diameter);
                }
                if (obs.Meet(new SingleObjectState(item, Vector2D.Zero, 0), new SingleObjectState(item - ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d2 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(item) - RobotParameters.OurRobotParams.Diameter);
                }
                d = d1 + d2;
                double a = Math.Abs(Vector2D.AngleBetweenInRadians(((item + ballPPrepVec.GetNormalizeToCopy(d1)) - From),
                    ((item - ballPPrepVec.GetNormalizeToCopy(d2)) - From)));
                double c = Math.Abs(Vector2D.AngleBetweenInRadians(Target - item, From - item));
                var intervals = GetVisibleIntervals(Model, item, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                float max = float.MinValue;
                int maxIdx = -1;
                for (int i = 0; i < intervals.Count; i++)
                {
                    if (intervals[i].ViasibleWidth > max)
                    {
                        max = intervals[i].ViasibleWidth;
                        maxIdx = i;
                    }
                }
                double b = 0;
                if (maxIdx != -1)
                {
                    b = Math.Abs(Vector2D.AngleBetweenInRadians(new Position2D(GameParameters.OppGoalCenter.X, intervals[maxIdx].interval.Start) - item,
                        new Position2D(GameParameters.OppGoalCenter.X, intervals[maxIdx].interval.End) - item));
                }
                double tS = item.DistanceFrom(Target) / shootSpeed;
                double t = tS + tP;
                double kc = CalculateKc(c);
                double scoreOt = kc * Math.Min(a, b) / Math.Sqrt(t);
                double scoreCr = Math.Min(a, b) / Math.Sqrt(t);
                if (scoreOt > maxScoreOt)
                {
                    maxScoreOt = scoreOt;
                    otPos = item;
                }
                if (scoreCr > maxScoreCr)
                {
                    maxScoreCr = scoreCr;
                    crPos = item;
                }
                scoresOt[index] = scoreOt;
                scoresCr[index] = scoreCr;

            }

            index = 0;
            if (Debug)
            {
                foreach (var item in CenterofRegions)
                {
                    if (scoresOt[index] > 0)
                    {
                        int brightness = (int)(scoresCr[index] / maxScoreOt) * 255;
                        //DrawingObjects.AddObject(new Circle(item, 0.2,new Pen(  Color.FromArgb(brightness,brightness,brightness),0.01f),true,1,true));
                        DrawingObjects.AddObject(new StringDraw(Math.Round(scoresOt[index], 2).ToString(), "scoreot" + item, Color.Black, item, true));
                        //DrawingObjects.AddObject(new StringDraw("cr: " + Math.Round(scoresCr[index], 2).ToString(), "scorecr" + item, item + new Vector2D(0.1, 0.1)));
                    }
                    index++;
                }
            }
            return new List<Position2D>() { otPos, crPos };
        }

        private double CalculateKc(double c)
        {
            if (c < 0 || c >= (70.0).ToRadian())
                return 0;
            if (c >= Math.PI / 4)
                return 1;
            return c / (Math.PI / 4);
        }



        public double CalculatePassCost(WorldModel Model, int RobotID, int? nullable)
        {
            throw new NotImplementedException();
        }
        public List<PassPointData> CalculatePassScore(WorldModel Model, int passerID, int? shooterID, Position2D topLeft, double passSpeed, double shootSpeed, double width, double heigth, int Rows, int column)
        {
            Obstacles obs = new Obstacles(Model);
            SingleObjectState ball = Model.BallState;
            const double accel = 4;
            const double speed = 2.5;
            List<Position2D> CenterofRegions = CenterRegion(topLeft, width, heigth, Rows, column);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
            double dist, DistFromBorder;
            int index = -1;
            Position2D otPos = new Position2D(), crPos = new Position2D();
            double maxScoreCr = double.MinValue, maxScoreOt = double.MinValue;
            double[] scoresOt = new double[CenterofRegions.Count], scoresCr = new double[CenterofRegions.Count];
            foreach (var passTarget in CenterofRegions)
            {
                index++;
                if (!GameParameters.IsInField(passTarget, -0.1) || GameParameters.IsInDangerousZone(passTarget, true, 0, out dist, out DistFromBorder))
                {
                    continue;
                }
                List<Position2D> intervalPos = IntervalSelect(GetVisibleIntervals(Model, passTarget, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null, null));

                Position2D shootTarget = Position2D.Interpolate(intervalPos[0], intervalPos[1], 0.5);

                //---------------------------------------------------------------------------------------------------
                Vector2D ballPVec = ball.Location - passTarget;
                Vector2D ballPPrepVec = Vector2D.FromAngleSize(ballPVec.AngleInRadians + Math.PI / 2, 1);
                double tP = ballPVec.Size / passSpeed;
                double time = tP + ((shootTarget - passTarget).Size / shootSpeed);
                double s = tP * accel;
                double t2 = 0, t1 = tP;
                if (s > speed)
                {
                    t2 = t1 - speed / accel;
                    t1 = speed / accel;
                }
                double d = 0.5 * accel * t1 * t1 + speed * t2;
                double d1 = d, d2 = d;
                ballPPrepVec.NormalizeTo(d);
                int idx;
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget + ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d1 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget - ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d2 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                d = d1 + d2;


                //-------------------------------------------------------------------------------------------------

                Position2D d1Pos = passTarget + ballPPrepVec.GetNormalizeToCopy(d1);
                Position2D d2Pos = passTarget - ballPPrepVec.GetNormalizeToCopy(d2);
                double a = Math.Abs(Vector2D.AngleBetweenInRadians(((passTarget + ballPPrepVec.GetNormalizeToCopy(d1)) - ball.Location),
                    ((passTarget - ballPPrepVec.GetNormalizeToCopy(d2)) - ball.Location)));
                double b = Math.Abs(Vector2D.AngleBetweenInRadians(intervalPos[0] - passTarget, intervalPos[1] - passTarget));
                double c = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - passTarget, ballPVec));
                double Kc = 1;
                if (c < 45)
                    Kc = (Kc / 45) * c;
                else if (c < 90)
                    Kc = 1;
                else
                    Kc = 0;

                //_______________________________________________________________________________________________________
                double oppMinDis = double.MaxValue;
                double oppMaxDis = double.MinValue;
                List<SingleObjectState> opps = new List<SingleObjectState>();
                foreach (var item in Model.Opponents)
                {
                    opps.Add(item.Value);
                }
                foreach (var item in opps)
                {
                    if (passTarget.DistanceFrom(item.Location) < oppMinDis)
                    {
                        oppMinDis = passTarget.DistanceFrom(item.Location);
                    }
                    if (passTarget.DistanceFrom(item.Location) > oppMaxDis)
                    {
                        oppMaxDis = passTarget.DistanceFrom(item.Location);
                    }
                }

                double e = 1;
                e = (e / oppMaxDis) * oppMinDis;
                double f = 1;
                if (shooterID.HasValue && Model.OurRobots.ContainsKey(shooterID.Value))
                    f = 1 - ((f / 10) * Math.Min(10, Model.OurRobots[shooterID.Value].Location.DistanceFrom(passTarget)));

                double h = 1;
                if (Math.Abs(passTarget.Y) > 0.92 * Math.Abs(GameParameters.OurLeftCorner.Y))
                    h = 0.8;
                double i = (Math.Max(Math.Min(passTarget.DistanceFrom(Model.BallState.Location), 3), 0.7) - 0.7) / 2.3;
                double scoreOt = Kc * a * b * e * f * h * i / (time);
                double scoreCr = a * b * e * f * h * i / (time);

                if (scoreOt > maxScoreOt)
                {
                    maxScoreOt = scoreOt;
                    otPos = passTarget;
                }
                if (scoreCr > maxScoreCr)
                {
                    maxScoreCr = scoreCr;
                    crPos = passTarget;
                }
                scoresOt[index] = scoreOt;
                scoresCr[index] = scoreCr;
            }
            return new List<PassPointData>() { new PassPointData(otPos, maxScoreOt, PassType.OT), new PassPointData(crPos, maxScoreCr, PassType.Catch) };
        }
        public List<PassPointData> CalculateAttackerPassScore(WorldModel Model, int passerID, int? shooterID, Position2D attackerPos, Position2D topLeft, double passSpeed, double shootSpeed, double width, double heigth, int Rows, int column)
        {
            Obstacles obs = new Obstacles(Model);
            SingleObjectState ball = Model.BallState;
            const double accel = 4;
            const double speed = 2.5;
            List<Position2D> CenterofRegions = CenterRegion(topLeft, width, heigth, Rows, column);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
            double dist, DistFromBorder;
            int index = -1;
            Position2D otPos = new Position2D(), crPos = new Position2D();
            double maxScoreCr = double.MinValue, maxScoreOt = double.MinValue;
            double[] scoresOt = new double[CenterofRegions.Count], scoresCr = new double[CenterofRegions.Count];
            foreach (var passTarget in CenterofRegions)
            {
                index++;
                if (!GameParameters.IsInField(passTarget, -0.1) || GameParameters.IsInDangerousZone(passTarget, true, 0, out dist, out DistFromBorder))
                {
                    continue;
                }
                List<Position2D> intervalPos = IntervalSelect(GetVisibleIntervals(Model, passTarget, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null, null));

                Position2D shootTarget = Position2D.Interpolate(intervalPos[0], intervalPos[1], 0.5);

                //---------------------------------------------------------------------------------------------------
                Vector2D ballPVec = ball.Location - passTarget;
                Vector2D ballPPrepVec = Vector2D.FromAngleSize(ballPVec.AngleInRadians + Math.PI / 2, 1);
                double tP = ballPVec.Size / passSpeed;
                double time = tP + ((shootTarget - passTarget).Size / shootSpeed);
                double s = tP * accel;
                double t2 = 0, t1 = tP;
                if (s > speed)
                {
                    t2 = t1 - speed / accel;
                    t1 = speed / accel;
                }
                double d = 0.5 * accel * t1 * t1 + speed * t2;
                double d1 = d, d2 = d;
                ballPPrepVec.NormalizeTo(d);
                int idx;
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget + ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d1 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget - ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d2 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                d = d1 + d2;


                //-------------------------------------------------------------------------------------------------

                Position2D d1Pos = passTarget + ballPPrepVec.GetNormalizeToCopy(d1);
                Position2D d2Pos = passTarget - ballPPrepVec.GetNormalizeToCopy(d2);
                double a = Math.Abs(Vector2D.AngleBetweenInRadians(((passTarget + ballPPrepVec.GetNormalizeToCopy(d1)) - ball.Location),
                    ((passTarget - ballPPrepVec.GetNormalizeToCopy(d2)) - ball.Location)));
                double b = Math.Abs(Vector2D.AngleBetweenInRadians(intervalPos[0] - passTarget, intervalPos[1] - passTarget));
                double c = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - passTarget, ballPVec));
                double Kc = 1;
                if (c < 45)
                    Kc = (Kc / 45) * c;
                else if (c < 90)
                    Kc = 1;
                else
                    Kc = 0;

                //_______________________________________________________________________________________________________
                double oppMinDis = double.MaxValue;
                double oppMaxDis = double.MinValue;
                List<SingleObjectState> opps = new List<SingleObjectState>();
                foreach (var item in Model.Opponents)
                {
                    opps.Add(item.Value);
                }
                foreach (var item in opps)
                {
                    if (passTarget.DistanceFrom(item.Location) < oppMinDis)
                    {
                        oppMinDis = passTarget.DistanceFrom(item.Location);
                    }
                    if (passTarget.DistanceFrom(item.Location) > oppMaxDis)
                    {
                        oppMaxDis = passTarget.DistanceFrom(item.Location);
                    }
                }

                double e = 1;
                e = (e / oppMaxDis) * oppMinDis;
                double f = 1;
                if (shooterID.HasValue && Model.OurRobots.ContainsKey(shooterID.Value))
                    f = 1 - ((f / 10) * Math.Min(10, Model.OurRobots[shooterID.Value].Location.DistanceFrom(passTarget)));

                double h = 1;
                if (Math.Abs(passTarget.Y) > 0.92 * Math.Abs(GameParameters.OurLeftCorner.Y))
                    h = 0.8;
                double i = (Math.Max(Math.Min(passTarget.DistanceFrom(Model.BallState.Location), 3), 0.7) - 0.7) / 2.3;
                double attackerScore = attackerPos.DistanceFrom(passTarget) / 50;
                double scoreOt = Kc * a * b * e * f * h * i / (time);
                double scoreCr = a * b * e * f * h * i / (time);
                scoreOt *= attackerScore;
                scoreCr *= attackerScore;
                if (scoreOt > maxScoreOt)
                {
                    maxScoreOt = scoreOt;
                    otPos = passTarget;
                }
                if (scoreCr > maxScoreCr)
                {
                    maxScoreCr = scoreCr;
                    crPos = passTarget;
                }
                scoresOt[index] = scoreOt;
                scoresCr[index] = scoreCr;
            }
            return new List<PassPointData>() { new PassPointData(otPos, maxScoreOt, PassType.OT), new PassPointData(crPos, maxScoreCr, PassType.Catch) };
        }
        public PassPointData CalculatePassPos(WorldModel Model, int passerID, int? shooterID, Position2D topLeft, double passSpeed, double shootSpeed, double width, double heigth, int Rows, int column)
        {
            Obstacles obs = new Obstacles(Model);
            SingleObjectState ball = Model.BallState;
            const double accel = 4;
            const double speed = 2.5;
            List<Position2D> CenterofRegions = CenterRegion(topLeft, width, heigth, Rows, column);
            obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
            double dist, DistFromBorder;
            int index = -1;
            Position2D otPos = new Position2D(), crPos = new Position2D();
            double maxScoreCr = double.MinValue, maxScoreOt = double.MinValue;
            double[] scoresOt = new double[CenterofRegions.Count], scoresCr = new double[CenterofRegions.Count];
            foreach (var passTarget in CenterofRegions)
            {
                //DrawingObjects.AddObject(passTarget, passTarget.toString());
                index++;
                if (!GameParameters.IsInField(passTarget, -0.1) || GameParameters.IsInDangerousZone(passTarget, true, 0, out dist, out DistFromBorder))
                {
                    continue;
                }
                List<Position2D> intervalPos = IntervalSelect(GetVisibleIntervals(Model, passTarget, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null, null));

                Position2D shootTarget = Position2D.Interpolate(intervalPos[0], intervalPos[1], 0.5);

                //---------------------------------------------------------------------------------------------------
                Vector2D ballPVec = ball.Location - passTarget;
                Vector2D ballPPrepVec = Vector2D.FromAngleSize(ballPVec.AngleInRadians + Math.PI / 2, 1);
                double tP = ballPVec.Size / passSpeed;
                double time = tP + ((shootTarget - passTarget).Size / shootSpeed);
                double s = tP * accel;
                double t2 = 0, t1 = tP;
                if (s > speed)
                {
                    t2 = t1 - speed / accel;
                    t1 = speed / accel;
                }
                double d = 0.5 * accel * t1 * t1 + speed * t2;
                double d1 = d, d2 = d;
                ballPPrepVec.NormalizeTo(d);
                int idx;
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget + ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d1 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                if (obs.Meet(new SingleObjectState(passTarget, Vector2D.Zero, 0), new SingleObjectState(passTarget - ballPPrepVec, Vector2D.Zero, 0),
                    RobotParameters.OurRobotParams.Diameter / 2, out idx))
                {
                    d2 = Math.Max(0, obs.ObstaclesList[idx].State.Location.DistanceFrom(passTarget) - RobotParameters.OurRobotParams.Diameter);
                }
                d = d1 + d2;


                //-------------------------------------------------------------------------------------------------

                Position2D d1Pos = passTarget + ballPPrepVec.GetNormalizeToCopy(d1);
                Position2D d2Pos = passTarget - ballPPrepVec.GetNormalizeToCopy(d2);
                double a = Math.Abs(Vector2D.AngleBetweenInRadians(((passTarget + ballPPrepVec.GetNormalizeToCopy(d1)) - ball.Location),
                    ((passTarget - ballPPrepVec.GetNormalizeToCopy(d2)) - ball.Location)));
                double b = Math.Abs(Vector2D.AngleBetweenInRadians(intervalPos[0] - passTarget, intervalPos[1] - passTarget));
                double c = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - passTarget, ballPVec));
                double Kc = 1;
                if (c < 45)
                    Kc = (Kc / 45) * c;
                else if (c < 90)
                    Kc = 1;
                else
                    Kc = 0;

                //_______________________________________________________________________________________________________
                double oppMinDis = double.MaxValue;
                double oppMaxDis = double.MinValue;
                List<SingleObjectState> opps = new List<SingleObjectState>();
                foreach (var item in Model.Opponents)
                {
                    opps.Add(item.Value);
                }
                foreach (var item in opps)
                {
                    if (passTarget.DistanceFrom(item.Location) < oppMinDis)
                    {
                        oppMinDis = passTarget.DistanceFrom(item.Location);
                    }
                    if (passTarget.DistanceFrom(item.Location) > oppMaxDis)
                    {
                        oppMaxDis = passTarget.DistanceFrom(item.Location);
                    }
                }

                double e = 1;
                e = (e / oppMaxDis) * oppMinDis;
                double f = 1;
                if (shooterID.HasValue && Model.OurRobots.ContainsKey(shooterID.Value))
                    f = 1 - ((f / 10) * Math.Min(10, Model.OurRobots[shooterID.Value].Location.DistanceFrom(passTarget)));

                double h = 1;
                if (Math.Abs(passTarget.Y) > 0.92 * Math.Abs(GameParameters.OurLeftCorner.Y))
                    h = 0.8;
                double i = (Math.Max(Math.Min(passTarget.DistanceFrom(Model.BallState.Location), 3), 0.7) - 0.7) / 2.3;
                double scoreOt = Kc * a * b * e * f * h * i / (time);
                double scoreCr = a * b * e * f * h * i / (time);

                if (scoreOt > maxScoreOt)
                {
                    maxScoreOt = scoreOt;
                    otPos = passTarget;
                }
                if (scoreCr > maxScoreCr)
                {
                    maxScoreCr = scoreCr;
                    crPos = passTarget;
                }
                scoresOt[index] = scoreOt;
                scoresCr[index] = scoreCr;
            }
            return new PassPointData(crPos, maxScoreCr, PassType.Catch);
        }

        public List<Position2D> IntervalSelect(List<VisibleGoalInterval> intervals)
        {
            double maxWidth = 0;
            List<Position2D> ret = new List<Position2D>();
            int idx = -1;
            for (int i = 0; i < intervals.Count; i++)
            {
                var item = intervals[i];
                if (Math.Abs(item.interval.Start - item.interval.End) > maxWidth)
                {
                    maxWidth = Math.Abs(item.interval.Start - item.interval.End);
                    idx = i;
                }
            }
            ret.Add(new Position2D(GameParameters.OppGoalCenter.X, (idx > -1) ? intervals[idx].interval.Start : 0));
            ret.Add(new Position2D(GameParameters.OppGoalCenter.X, (idx > -1) ? intervals[idx].interval.End : 0));
            return ret;
        }
        public PassPointData CalculateDribbleScore(WorldModel Model, int RobotID, Position2D From, Position2D Target, int? opp2Ex)
        {
            int count = 5;
            double step = Math.PI / (count + 1);
            double angle = Math.PI / 2, length = 1;
            double[] scores = new double[count];
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
            PassPointData minP = new PassPointData(Position2D.Zero, double.MinValue, PassType.Drible);
            for (int i = 0; i < count; i++)
            {
                angle += step;
                angle = GameParameters.AngleModeR(angle);
                Vector2D v = Vector2D.FromAngleSize(angle, length);
                Position2D p = From + v;
                double a = obs.Meet(new SingleObjectState(From, Vector2D.Zero, 0), new SingleObjectState(p, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi) ? 0.5 : 1;
                double b = GameParameters.IsInField(p + v.GetNormalizeToCopy(1.5), 0) ? 1 : -1;
                double minDist = double.MaxValue;
                foreach (var item in Model.Opponents)
                {
                    if (opp2Ex.HasValue && item.Key == opp2Ex.Value)
                        continue;
                    double d = item.Value.Location.DistanceFrom(p);
                    if (d < minDist)
                        minDist = d;
                }
                if (minDist < RobotParameters.OurRobotParams.Diameter + 0.1)
                    minDist = 0;
                var intervals = GetVisibleIntervals(Model, p, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                double c = 0.1;
                double maxW = double.MinValue;
                int idx = -1;
                for (int j = 0; j < intervals.Count; j++)
                {
                    var item = intervals[j];
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxW)
                    {
                        idx = j;
                        maxW = Math.Abs(item.interval.Start - item.interval.End);
                    }
                }
                if (idx > -1)
                {
                    c = Math.Min(Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y), Math.Abs(intervals[idx].interval.Start - intervals[idx].interval.End));
                    c = c / Math.Abs(GameParameters.OppGoalRight.Y - GameParameters.OppGoalLeft.Y);
                    c += 0.1;
                    c = Math.Min(c, 1);
                }
                minDist = Math.Min(3.0, minDist) / 3.0;
                double s = a * b * minDist * c;
                if (s > minP.score)
                {
                    minP.score = s;
                    minP.pos = p;
                }
            }

            return minP;
        }
    }
    public enum PassType
    {
        OT,
        Catch,
        Drible
    }
    public struct PassPointData
    {
        public PassType type;

        public double score;
        public Position2D pos;
        //public PassPointData()
        //{
        //    type = PassType.OT;
        //    pos = Position2D.Zero;
        //    score = 0;
        //}
        public PassPointData(Position2D p, double s, PassType t)
        {
            type = t;
            pos = p;
            score = s;
        }
    }
    [Flags]
    public enum MarkingType
    {
        //None = 1,
        //ToBall = 2,
        //ToTarget = 4,
        //Near = 8
        Open2Direct = 1,
        Open2Chip = 2,
        Blocked = 4,
        Near = 8
    }
}
