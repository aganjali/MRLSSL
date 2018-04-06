using System;
using System.Collections.Generic;
//using MRL.SSL.AIConsole.Analysers;
using MRL.SSL.GameDefinitions;
using MRL.SSL.GameDefinitions.General_Settings;
using System.Reflection;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Threading;
//using MRL.SSL.AIConsole.HighLevel;
using System.Drawing;
using MRL.SSL.AIConsole.Plays;
//using MRL.SSL.AIConsole.HighLevel.AttackPlans;
using System.Linq;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.Planning.GamePlanner;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Engine
{
    public class GameStrategyEngine : IDisposable
    {
        public PlayBase[] ImplementedPlays;
        public List<StrategyBase> ImplementedStrategies;
        public Dictionary<Type, ActionDescriptionBase> ImplementedActions;
        public Dictionary<Type, RoleBase> ImplementedRoles;
        public PlayBase LastRunningPlay;
        public int EngineID;

        //private HighLevel.GamePlanner _gamePlanner;
        private GamePlannerInfo gameInfo;
        private GamePlannerEngine _newGamePlanner;

        public GamePlannerEngine NewGamePlanner
        {
            get { return _newGamePlanner; }
            set { _newGamePlanner = value; }
        }
        public GamePlannerInfo GameInfo
        {
            get { return gameInfo; }
            set { gameInfo = value; }
        }
        //public HighLevel.GamePlanner GamePlanner
        //{
        //    get { return _gamePlanner; }
        //}

        private GameStatus _status = GameStatus.Halt;

        public GameStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public GameStrategyEngine(int EngineID)
        {
            this.EngineID = EngineID;
            List<PlayBase> pb = new List<PlayBase>();
            Type[] types = System.Reflection.Assembly.GetAssembly(typeof(GameStrategyEngine)).GetTypes();
            ImplementedRoles = new Dictionary<Type, RoleBase>();
            ImplementedActions = new Dictionary<Type, ActionDescriptionBase>();

            ImplementedStrategies = new List<StrategyBase>();
            foreach (Type t in types)
            {
                if (t.IsClass && t.IsSubclassOf(typeof(PlayBase)))
                    pb.Add(t.GetConstructor(new Type[] { }).Invoke(new object[] { }) as PlayBase);
                else if (t.IsClass && t.IsSubclassOf(typeof(RoleBase)))
                    ImplementedRoles.Add(t, t.GetConstructor(BindingFlags.Instance | BindingFlags.Default | BindingFlags.Public, null, Type.EmptyTypes, null).Invoke(new object[] { }) as RoleBase);
                else if (t.IsClass && t.IsSubclassOf(typeof(StrategyBase)))
                    ImplementedStrategies.Add(t.GetConstructor(new Type[] { }).Invoke(new object[] { }) as StrategyBase);
                else if (t.IsClass && t.IsSubclassOf(typeof(ActionDescriptionBase)))
                {
                    
                    ImplementedActions.Add(t, t.GetConstructor(new Type[] { }).Invoke(new object[] { }) as ActionDescriptionBase);
                }
            }

            foreach (var item in ImplementedStrategies)
            {
                item.FillInformation();
                StrategyInfo.Add(new StrategyInfo(item.StrategyName, item.AttendanceSize, item.About));
            }

            ImplementedPlays = pb.ToArray();
            rnd = new Random();
            //     _gamePlanner = new MRL.SSL.AIConsole.HighLevel.GamePlanner(this);
            _newGamePlanner = new GamePlannerEngine();

            gameInfo = new GamePlannerInfo();
        }

        Random rnd;
        int Counter = 0;
        public Dictionary<int, RoleBase> assignedroles;
        bool firstTiemStrategy = true;
        public void PlayGame(WorldModel Model, bool StrategyChanged, bool logViewer)
        {
            _newGamePlanner.Start();

            gameInfo = _newGamePlanner.CalculateInfo(Model, logViewer);
            //if (gameInfo.OppTeam.GoaliID.HasValue)
            //    DrawingObjects.AddObject(new StringDraw("goaliID: " + gameInfo.OppTeam.GoaliID.Value.ToString(), "goaliIDopp", new Position2D(-0.5, -.5)));
            //else
            //    DrawingObjects.AddObject(new StringDraw("goaliID: null", "goaliIDopp", new Position2D(-0.5, -.5)));
            if (StrategyChanged || firstTiemStrategy)
            {
                firstTiemStrategy = false;
                foreach (var item in ImplementedStrategies)
                {
                    item.zone = StrategyInfo.Get(item.StrategyName).Region;
                    item.Enable = StrategyInfo.Get(item.StrategyName).Enabled;
                    item.Probability = StrategyInfo.Get(item.StrategyName).Probability;
                    item.status = StrategyInfo.Get(item.StrategyName).Status;
                }
            }


            #region Drawing
            if (MainGameParameters.Default.Drawing)
            {
                DrawCollection GamePlannerDrawings = new DrawCollection();
                DrawCollection RegionDrawings = new DrawCollection();
                //int? oppFirstOwnerId = gameInfo.OppTeam.BallOwner;
                //int? oppSecondOwnerId = null;
                //int? oppThirdOwnerId = null;
                StringDraw oppFirstStr = new StringDraw(), oppSecStr = new StringDraw(), oppThirdStr = new StringDraw();
                //if (oppFirstOwnerId.HasValue)
                //    oppSecondOwnerId  = _gamePlanner.GetSecondOpponentAttackingRobot(this, Model, oppFirstOwnerId.Value, out oppThirdOwnerId);
                int? oppFirstOwnerId = null;
                int? oppSecondOwnerId = null;
                int? oppThirdOwnerId = null;
                if (gameInfo.OppTeam.Scores.Count > 0)
                    oppFirstOwnerId = gameInfo.OppTeam.Scores.First().Key;
                if (gameInfo.OppTeam.Scores.Count > 1)
                    oppSecondOwnerId = gameInfo.OppTeam.Scores.ElementAt(1).Key;
                if (gameInfo.OppTeam.Scores.Count > 2)
                    oppThirdOwnerId = gameInfo.OppTeam.Scores.ElementAt(2).Key;

                if (oppFirstOwnerId.HasValue && Model.Opponents.ContainsKey(oppFirstOwnerId.Value))
                {
                    oppFirstStr = new StringDraw("1st Attacker", Model.Opponents[oppFirstOwnerId.Value].Location + new Vector2D(0, -0.25));
                    oppFirstStr.TextColor = Color.Chocolate;
                    oppFirstStr.IsShown = false;
                    oppFirstStr.OnTop = true;
                    GamePlannerDrawings.AddObject(oppFirstStr);
                }
                if (oppSecondOwnerId.HasValue && Model.Opponents.ContainsKey(oppSecondOwnerId.Value))
                {
                    oppSecStr = new StringDraw("2nd Attacker", Model.Opponents[oppSecondOwnerId.Value].Location + new Vector2D(0, -0.25));
                    oppSecStr.TextColor = Color.Chocolate;
                    oppSecStr.OnTop = true;
                    oppFirstStr.IsShown = false;
                    GamePlannerDrawings.AddObject(oppSecStr);
                }
                if (oppThirdOwnerId.HasValue && Model.Opponents.ContainsKey(oppThirdOwnerId.Value))
                {
                    oppThirdStr = new StringDraw("3d Attacker", Model.Opponents[oppThirdOwnerId.Value].Location + new Vector2D(0, -0.25));
                    oppThirdStr.TextColor = Color.Chocolate;
                    oppThirdStr.OnTop = true;
                    oppFirstStr.IsShown = false;
                    GamePlannerDrawings.AddObject(oppThirdStr);
                }


                //List<MRL.SSL.AIConsole.HighLevel.VisibleGoalInterval> intervals = _gamePlanner.GetVisibleGoalIntervals(Model.BallState.Location, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null);
                gameInfo.OppGoalIntervals = gameInfo.GetVisibleIntervals(Model, Model.BallState.Location, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null);

                List<Position2D> positions = new List<Position2D>();
                DrawRegion oppRegion = new DrawRegion();
                foreach (MRL.SSL.Planning.GamePlanner.Types.VisibleGoalInterval item in gameInfo.OppGoalIntervals)
                {
                    positions.Add(Model.BallState.Location);
                    positions.Add(new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start));
                    positions.Add(new Position2D(GameParameters.OppGoalCenter.X, item.interval.End));
                }
                oppRegion = new DrawRegion(positions, true, false, Model.OurMarkerISYellow ? Color.Blue : Color.Yellow, Model.OurMarkerISYellow ? Color.DarkBlue : Color.Gold, 0.2f);

                //List<MRL.SSL.AIConsole.HighLevel.VisibleGoalInterval> intervals2 = _gamePlanner.GetVisibleGoalIntervals(Model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null);
                gameInfo.OurGoalIntervals = gameInfo.GetVisibleIntervals(Model, Model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, true, true, null);
                List<Position2D> positions2 = new List<Position2D>();
                DrawRegion ourRegion = new DrawRegion();
                
                foreach (MRL.SSL.Planning.GamePlanner.Types.VisibleGoalInterval item in gameInfo.OurGoalIntervals)
                {
                    positions2.Add(Model.BallState.Location);
                    positions2.Add(new Position2D(GameParameters.OurGoalCenter.X, item.interval.Start));
                    positions2.Add(new Position2D(GameParameters.OurGoalCenter.X, item.interval.End));
                }

                ourRegion = new DrawRegion(positions2, true, false, Model.OurMarkerISYellow ? Color.Yellow : Color.Blue, Model.OurMarkerISYellow ? Color.Gold : Color.DarkBlue, 0.2f);

                ourRegion.Filled = true;
                oppRegion.Filled = true;
                RegionDrawings.AddObject(ourRegion, "ourregion");
                RegionDrawings.AddObject(oppRegion, "oppregion");


                DrawingObjects.AddObject("GamePlanner", GamePlannerDrawings);
                DrawingObjects.AddObject("Regions", RegionDrawings);
            }
            #endregion

            Dictionary<int, CommonDelegate> functions = new Dictionary<int, CommonDelegate>();
            Model.Status = _status;
            bool playChanged = false;
            if (LastRunningPlay == null || !LastRunningPlay.IsFeasiblel(this, Model, LastRunningPlay, ref Model.Status) ||  LastRunningPlay.QueryPlayResult() != PlayResult.InPlay/*|| Model.Status != wmbackup.Status*/)
            {
                List<PlayBase> feasibleplays = new List<PlayBase>();
                foreach (PlayBase p in ImplementedPlays)
                {
                    p.ResetPlay(Model, this);
                    if (p.IsFeasiblel(this, Model, LastRunningPlay, ref Model.Status))
                        feasibleplays.Add(p);
                }
                if (feasibleplays.Count == 0)
                    //TODO Implement enough plays to span the state space, so we'll never see this error.
                    throw new Exception("No Plays are feasible");
                PlayBase selectedplay = feasibleplays[rnd.Next(0, feasibleplays.Count)];
                selectedplay.ResetPlay(Model, this);
                Planner.IsStopBall(false);
                LastRunningPlay = selectedplay;
                playChanged = true;
            }
            _status = Model.Status;
            RobotCommands Commands = new RobotCommands();


            assignedroles = LastRunningPlay.RunPlay(this, Model, playChanged || (++Counter) >= 10, out functions);

            foreach (int key in assignedroles.Keys)
            {
                if (!Model.OurRobots.ContainsKey(key)) continue;
                DrawingObjects.AddObject(new StringDraw(assignedroles[key].ToString().Substring(24), System.Drawing.Color.Black, Model.OurRobots[key].Location + new Vector2D(-0, -0.25), false) { OnTop = true }, key.ToString());
            }

            if (Counter == 10)
                Counter = 0;
            Commands.SequenceNumber = Model.SequenceNumber;

            /// Omid
            Commands.Commands = new Dictionary<int, SingleWirelessCommand>();

            // System.Threading.Tasks.Parallel.ForEach<int>(Model.OurRobots.Keys,
            //   delegate(int RobotID)
            foreach (int RobotID in assignedroles.Keys.ToList())
            {
                //RobotID = keys[i];
                if (functions.ContainsKey(RobotID))
                {
                    assignedroles[RobotID].DetermineNextState(this, Model, RobotID, assignedroles);
                    functions[RobotID](this, Model);
                    //
                }
                // i++;
            }
            //  );
            ///omid
            //return Commands;
        }

        public void Dispose()
        {
            //_gamePlanner.Dispose();
            _newGamePlanner.Dispose();
        }
    }
}
