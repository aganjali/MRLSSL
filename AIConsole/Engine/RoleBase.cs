using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;


namespace MRL.SSL.AIConsole.Engine
{
    public abstract class RoleBase
    {
        private int _lastState;
        public int LastState
        {
            get { return _lastState; }
            set { _lastState = value; }
        }
        private int _currentState;
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

        private SkillBase _currentSkill;
        public SkillBase CurrentSkill
        {
            get { return _currentSkill; }
            set { _currentSkill = value; }
        }

        public void ResetState()
        {
            _currentSkill = null;
        }

        protected T GetSkill<T>() where T : SkillBase, new()
        {
            if (CurrentSkill != null && CurrentSkill is T)
                return CurrentSkill as T;
            else
            {
                T t = new T();
                CurrentSkill = t;
                return t;
            }
        }

        public abstract RoleCategory QueryCategory();

        public abstract void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles);

        public abstract double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles);

        public abstract List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles);

        public abstract bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles);

        public override string ToString()
        {
            return GetType().ToString();
        }
    }

    public enum RoleCategory
    {
        Goalie,
        Defender,
        Active,
        Positioner,
        Test
    }

    public enum RoleState
    {
        Running,
        Failed,
        Successed
    }
}
