using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using System.Runtime.InteropServices;
using MRL.SSL.Planning.GPUDirect;

namespace MRL.SSL.Planning.GamePlanner
{
    public partial class BallState : IDisposable
    {

        public BallState()
        {
            AllocateVariables();
        }
        
        float[] hostHeads, hostTails, hostTimeHeads, hostTimeTails;
        int[] hosthisto;
        private void AllocateVariables()
        {
            hostHeads = new float[2 * maxRobotCount * maxLines * 2];
            hostTails = new float[2 * maxRobotCount * maxLines * 2];
            hostTimeHeads = new float[2 * maxRobotCount * maxLines];
            hostTimeTails = new float[2 * maxRobotCount * maxLines];
            hosthisto = new int[2 * maxRobotCount * 2];
        }

        public void GoodBallCatchingLines(List<MRL.SSL.GameDefinitions.WorldModel> model, GVector2D GlobalBallSpeed, ref GamePlannerInfo gpInfo)
        {
            List<float> fstates = new List<float>();
            float[] FBall = new float[4];
            MRL.SSL.GameDefinitions.WorldModel Model = model.Last();
            List<GObjectState> tmpObj = new List<GObjectState>();
            List<int> tmpOurIds = new List<int>(), tmpOppIds = new List<int>();
            int N = Math.Min(maxPoints, (int)((100 * GlobalBallSpeed.Size() * GlobalBallSpeed.Size() / (2 * GPUParams.ballDecel)) + 1));

            int ourRobotsCount = Math.Min(Model.OurRobots.Values.Count, maxRobotCount);
            foreach (var item in Model.OurRobots.Keys.Take(ourRobotsCount))
            {
                tmpObj.Add(Model.OurRobots[item]);
                tmpOurIds.Add(item);
                fstates.Add((float)Model.OurRobots[item].Location.X);
                fstates.Add((float)Model.OurRobots[item].Location.Y);
                fstates.Add((float)Model.OurRobots[item].Speed.X);
                fstates.Add((float)Model.OurRobots[item].Speed.Y);
            }
            int oppRobotsCount = Math.Min(Model.Opponents.Values.Count, maxRobotCount);
            foreach (var item in Model.Opponents.Keys.Take(oppRobotsCount))
            {
                tmpObj.Add(Model.Opponents[item]);
                tmpOppIds.Add(item);
                fstates.Add((float)Model.Opponents[item].Location.X);
                fstates.Add((float)Model.Opponents[item].Location.Y);
                fstates.Add((float)Model.Opponents[item].Speed.X);
                fstates.Add((float)Model.Opponents[item].Speed.Y);
            }
            int RobotCounts = ourRobotsCount + oppRobotsCount;
            FBall[0] = (float)Model.BallState.Location.X;
            FBall[1] = (float)Model.BallState.Location.Y;
            FBall[2] = (float)Model.BallState.Speed.X;
            FBall[3] = (float)Model.BallState.Speed.Y;
            if (RobotCounts > 0)
            {
                GPosition2D[] heads, tails;
                float[] timeHeads, timeTails;
                List<Line> ls;
                Dictionary<int, List<Line>> dicOurLine = new Dictionary<int, List<Line>>(), dicOppLine = new Dictionary<int, List<Line>>();
                Dictionary<int, List<float>> dicOurHT = new Dictionary<int, List<float>>(), dicOurTT = new Dictionary<int, List<float>>(), dicOppHT = new Dictionary<int, List<float>>(), dicOppTT = new Dictionary<int, List<float>>();

                
                GPPlanner.GPlannerBallState(fstates.ToArray(), RobotCounts, N, FBall, hostHeads, hostTails, hostTimeHeads, hostTimeTails, hosthisto);
                for (int i = 0; i < RobotCounts; i++)
                {
                  
                    heads = new GPosition2D[Math.Min(hosthisto[i * 2], maxLines)];
                    timeHeads = new float[Math.Min(hosthisto[i * 2], maxLines)];
                    tails = new GPosition2D[Math.Min(hosthisto[i * 2 + 1], maxLines)];
                    timeTails = new float[Math.Min(hosthisto[i * 2 + 1], maxLines)];
                    int jj = 0;
                    for (int j = 2 * i * maxLines; j < 2 * i * maxLines + 2 * Math.Min(hosthisto[i * 2], maxLines); j+=2)
                    {
                        heads[jj] = new GPosition2D(hostHeads[j], hostHeads[j + 1]);
                        timeHeads[jj] = hostTimeHeads[(int)(j / 2)];
                        jj++;
                    }
                    jj = 0;
                    for (int j = 2 * i * maxLines; j < 2 * i * maxLines + 2 * Math.Min(hosthisto[i * 2 + 1], maxLines); j += 2)
                    {
                        tails[jj] = new GPosition2D(hostTails[j], hostTails[j + 1]);
                        timeTails[jj] = hostTimeTails[(int)(j / 2)];
                        jj++;
                    }

                    Dictionary<int, GPosition2D> tmpH = new Dictionary<int, GPosition2D>(), tmpT = new Dictionary<int, GPosition2D>();
                    int length = Math.Min(heads.Length, tails.Length);
                    for (int j = 0; j < length; j++)
                    {
                        tmpH.Add(j, heads[j]);
                        tmpT.Add(j, tails[j]);
                    }
                    var dicH = tmpH.OrderBy(o => (i < ourRobotsCount) ? o.Value.DistanceFrom(Model.OurRobots[tmpOurIds[i]].Location) : o.Value.DistanceFrom(Model.Opponents[tmpOppIds[i - ourRobotsCount]].Location));
                    var dicT = tmpT.OrderBy(o => (i < ourRobotsCount) ? o.Value.DistanceFrom(Model.OurRobots[tmpOurIds[i]].Location) : o.Value.DistanceFrom(Model.Opponents[tmpOppIds[i - ourRobotsCount]].Location));
                    float[] tmpTH = new float[length], tmpTT = new float[length];
                    for (int k = 0; k < length; k++)
                    {
                        tmpTH[k] = timeHeads[dicH.ElementAt(k).Key];
                        tmpTT[k] = timeTails[dicT.ElementAt(k).Key];
                    }

                    ls = new List<Line>();
                    for (int j = 0; j < length; j++)
                    {
                        Position2D p1 = new Position2D(dicH.ElementAt(j).Value.X, dicH.ElementAt(j).Value.Y), p2 = new Position2D(dicT.ElementAt(j).Value.X, dicT.ElementAt(j).Value.Y);
                        if (p1.DistanceFrom(p2) >= 0.02)
                            ls.Add(new Line(p1, p2));
                        else
                            ls.Add(new Line(p1, p1 + GlobalBallSpeed.GetNormalizeToCopy(0.07f)));
                    }
                    int id = 0;
                    if (i < ourRobotsCount)
                    {
                        id = tmpOurIds[i];
                        dicOurLine[id] = ls;
                        dicOurHT[id] = tmpTH.ToList();
                        dicOurTT[id] = tmpTT.ToList();
                    }
                    else
                    {
                        id = tmpOppIds[i - ourRobotsCount];
                        dicOppLine[id] = ls;
                        dicOppHT[id] = tmpTH.ToList();
                        dicOppTT[id] = tmpTT.ToList();
                    }
                }

                GPTeam ourTeam = new GPTeam(Model.OurMarkerISYellow ? Color.Yellow : Color.Blue), oppTeam = new GPTeam(Model.OurMarkerISYellow ? Color.Blue : Color.Yellow);
                ourTeam.CatchBallLines = dicOurLine;
                ourTeam.TimeHeads = dicOurHT;
                ourTeam.TimeTails = dicOurTT;
                oppTeam.CatchBallLines = dicOppLine;
                oppTeam.TimeHeads = dicOppHT;
                oppTeam.TimeTails = dicOppTT;
                gpInfo.OppTeam = oppTeam;
                gpInfo.OurTeam = ourTeam;

            }
        }

        public void Dispose()
        {
 
        }

    }
}
