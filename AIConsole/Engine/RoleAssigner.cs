using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Engine
{
    public class RoleAssigner
    {
        public static int? AssignRole(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> PreviouslyAssignedRoles, Dictionary<int, RoleBase> CurrentlyAssignedRoles, FeasibilityCalculator FC, double Margin, Type RoleType)
        {
            //foreach (int item in PreviouslyAssignedRoles)
            //{
            //    if(
            //}
            RoleBase RoleToBeAssigned = RoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            int? RobotID = DetermineRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, FC, Margin, RoleType, RoleToBeAssigned);
            if (RobotID.HasValue)
            {
                if (PreviouslyAssignedRoles != null && PreviouslyAssignedRoles.ContainsKey(RobotID.Value) && PreviouslyAssignedRoles[RobotID.Value].GetType() == RoleType)
                    CurrentlyAssignedRoles[RobotID.Value] = PreviouslyAssignedRoles[RobotID.Value];
                else
                    CurrentlyAssignedRoles[RobotID.Value] = RoleToBeAssigned;
            }
	
			return RobotID;
        }

		public static int? DetermineRole(GameStrategyEngine engine, WorldModel Model, Dictionary<int, RoleBase> PreviouslyAssignedRoles, Dictionary<int, RoleBase> CurrentlyAssignedRoles, FeasibilityCalculator FC, double Margin, Type RoleType, RoleBase RoleToBeAssigned)
        {
            int? bestRobotID = null;
			double? bestfeasibility = double.MinValue;
			Dictionary<int,double> feasibilities = new Dictionary<int,double>();
            foreach (int RobotID in Model.OurRobots.Keys)
            {
                if (CurrentlyAssignedRoles.ContainsKey(RobotID))
                {
                    feasibilities[RobotID] = float.MaxValue;
                    continue;
                }
                double feas = FC.Invoke(engine, Model, RobotID, RoleToBeAssigned, CurrentlyAssignedRoles);
                feasibilities[RobotID] = feas;
                if (feas > bestfeasibility)
                {
                    bestfeasibility = feas;
                    bestRobotID = RobotID;
                }
            }
            if (!bestRobotID.HasValue)
                return null;
            if (PreviouslyAssignedRoles == null)
                return bestRobotID;
            int? LastRoleOwner = null;
			foreach (int RobotID in PreviouslyAssignedRoles.Keys)
                if (PreviouslyAssignedRoles.ContainsKey(RobotID) && PreviouslyAssignedRoles.ContainsKey(RobotID) && PreviouslyAssignedRoles[RobotID].GetType() == RoleType)
                {
                    LastRoleOwner = RobotID;
                    break;
                }
            if (!LastRoleOwner.HasValue || CurrentlyAssignedRoles.ContainsKey(LastRoleOwner.Value))
                return bestRobotID;
            if (feasibilities.ContainsKey(LastRoleOwner.Value) && feasibilities[LastRoleOwner.Value] + Margin > bestfeasibility)
                return LastRoleOwner;
            else
                return bestRobotID;
        }

        public static void DetermineTwoDefenders(WorldModel Model, RoleBase[] PreviouslyAssignedRoles, RoleBase[] CurrentlyAssignedRoles, float Margin, Type LeftRoleType, Type RightRoleType)
        {
			//RoleBase LeftRoleToBeAssigned = LeftRoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
			//RoleBase RightRoleToBeAssigned = RightRoleType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;

			//int? LN = null, RN = null;
			//double LND = double.MaxValue, RND = double.MaxValue;
			//for (int i = 0; i < CurrentlyAssignedRoles.Length; i++)
			//{
			//    if (CurrentlyAssignedRoles[i] != null)
			//        continue;
			//    double temp = LeftRoleToBeAssigned.RunRole(Model, i, CurrentlyAssignedRoles).Speed.Size;
			//    if (temp < LND)
			//    {
			//        LND = temp;
			//        LN = i;
			//    }
			//    temp = RightRoleToBeAssigned.RunRole(Model, i, CurrentlyAssignedRoles).Speed.Size;
			//    if (temp < RND)
			//    {
			//        RND = temp;
			//        RN = i;
			//    }
			//}
			//if (LN != RN)
			//{
			//    if (LN.HasValue)
			//    {
			//        if (PreviouslyAssignedRoles != null && PreviouslyAssignedRoles.Length > LN.Value && PreviouslyAssignedRoles[LN.Value].GetType() == LeftRoleType)
			//            CurrentlyAssignedRoles[LN.Value] = PreviouslyAssignedRoles[LN.Value];
			//        else
			//            CurrentlyAssignedRoles[LN.Value] = LeftRoleToBeAssigned;
			//    }
			//    if (RN.HasValue)
			//    {
			//        if (PreviouslyAssignedRoles != null && PreviouslyAssignedRoles.Length > RN.Value && PreviouslyAssignedRoles[RN.Value].GetType() == RightRoleType)
			//            CurrentlyAssignedRoles[RN.Value] = PreviouslyAssignedRoles[RN.Value];
			//        else
			//            CurrentlyAssignedRoles[RN.Value] = RightRoleToBeAssigned;
			//    }
			//    return;
			//}

			//int? Nearest = null, SecondNearest = null;
			//double NDist = double.MaxValue, SNDist = double.MaxValue;
			//for (int i = 0; i < CurrentlyAssignedRoles.Length; i++)
			//{
			//    if (CurrentlyAssignedRoles[i] != null)
			//        continue;
			//    Vector2D P1 = LeftRoleToBeAssigned.RunRole(Model, i, CurrentlyAssignedRoles).Speed;
			//    Vector2D P2 = RightRoleToBeAssigned.RunRole(Model, i, CurrentlyAssignedRoles).Speed;
			//    double d = (P1 + P2).Size / 2;
			//    if (d < NDist)
			//    {
			//        SNDist = NDist;
			//        SecondNearest = Nearest;
			//        NDist = d;
			//        Nearest = i;
			//    }
			//    else if (d < SNDist)
			//    {
			//        SNDist = d;
			//        SecondNearest = i;
			//    }
			//}
			//if (!Nearest.HasValue)
			//    return;
			//else if (!SecondNearest.HasValue)
			//{
			//    CurrentlyAssignedRoles[Nearest.Value] = LeftRoleToBeAssigned;
			//    return;
			//}
			//else
			//{
			//    if (Model.OurRobots[Nearest.Value].Location.Y > Model.OurRobots[SecondNearest.Value].Location.Y)
			//    {
			//        CurrentlyAssignedRoles[Nearest.Value] = LeftRoleToBeAssigned;
			//        CurrentlyAssignedRoles[SecondNearest.Value] = RightRoleToBeAssigned;
			//    }
			//    else
			//    {
			//        CurrentlyAssignedRoles[Nearest.Value] = RightRoleToBeAssigned;
			//        CurrentlyAssignedRoles[SecondNearest.Value] = LeftRoleToBeAssigned;
			//    }
			//}
			
        }

    }

    public delegate double FeasibilityCalculator(GameStrategyEngine engine, WorldModel Model, int RobotID, RoleBase RoleToBeAssigned, Dictionary<int, RoleBase> CurrentlyAssignedRoles);
}
