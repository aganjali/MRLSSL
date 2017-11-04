using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public class StrategyModel
    {
        string name;
        string description;
        int attendance;
        private List<StrategyStates> states;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public int Attendance
        {
            get { return attendance; }
            set { attendance = value; }
        }

        public List<StrategyStates> States
        {
            get { return states; }
            set { states = value; }
        }

        public StrategyModel()
        {
            name = "new Strategy";
            description = "";
            attendance = 6;
            states = new List<StrategyStates>();
        }
    }
    public class StrategyStates
    {
        int id;
        int robotID;
        Dictionary<Type, object> tasks;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int RobotID
        {
            get { return robotID; }
            set { robotID = value; }
        }

        public Dictionary<Type, object> Tasks
        {
            get { return tasks; }
            set { tasks = value; }
        }

        public StrategyStates()
        {
            id = 0;
            robotID = 0;
            tasks = new Dictionary<Type, object>();
        }
    }
}
