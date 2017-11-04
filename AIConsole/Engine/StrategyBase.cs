using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;


namespace MRL.SSL.AIConsole.Engine
{
    public abstract class StrategyBase : ICloneable
    {
        protected Dictionary<int, SingleObjectState> Attendance;
        public Dictionary<int, RoleBase> PreviouslyAssignedRoles = new Dictionary<int, RoleBase>();
        public Dictionary<int, CommonDelegate> Functions;
        public int AttendanceSize;
        public double Score;
        public string About = "This is a strategy";
        public int InitialState = -1;
        public int FinalState = -1;
        public int TrapState = -1;
        public string StrategyName = "";
        public bool Enable = false;
        public double Probability = 0;
        public List<Vector2D> zone;
        public List<GameStatus> status;
        public bool UseInMiddle = false;
        public bool UseOnlyInMiddle = false;
        private int _currentState = 0;
        public int CurrentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }
        private RoleState _roleStatus;
        public RoleState RoleStatus
        {
            get { return _roleStatus; }
            set { _roleStatus = value; }
        }
        public bool isInint = false;


        public abstract void ResetState();


        protected RoleMatcher RoleMatcher = new RoleMatcher();
        public abstract void InitializeStates(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance);
        public abstract void FillInformation();


        public abstract bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, ref GameStatus Status);

        public abstract void DetermineNextState(GameStrategyEngine engine, WorldModel Model);
        public abstract Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions);

        public override string ToString()
        {
            return GetType().ToString();
        }

        protected Dictionary<int, SingleObjectState> CurrentAttendance = new Dictionary<int, SingleObjectState>();

        public Dictionary<int, RoleBase> Run(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance, out Dictionary<int, CommonDelegate> Functions)
        {
            CurrentAttendance = attendance.ToDictionary(o => o.Key, p => p.Value);
            if (!isInint)
            {
                PreviouslyAssignedRoles.Clear();
                isInint = true;
                this.InitializeStates(engine, Model, attendance);
            }

            this.DetermineNextState(engine, Model);
            return this.RunStrategy(engine, Model, out Functions);
        }

        protected T GetRole<T>(int robotID) where T : RoleBase, new()
        {
            if (!PreviouslyAssignedRoles.ContainsKey(robotID))
                PreviouslyAssignedRoles.Add(robotID, typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase);
            else if (!(PreviouslyAssignedRoles[robotID] is T))
                PreviouslyAssignedRoles[robotID] = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            return (T)PreviouslyAssignedRoles[robotID];
        }

        //protected void Sort(out Dictionary<int, SingleObjectState> RobotsInStrategy)
        //{
        //    if (RobotsInStrategy != null && RobotsInStrategy.Count > 0)
        //    {
        //        RobotsInStrateg
        //    }
        //}

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
