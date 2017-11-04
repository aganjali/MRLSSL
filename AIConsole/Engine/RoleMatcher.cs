using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.AIConsole.Engine
{
    public class RoleMatcher
    {
        public Dictionary<int, RoleBase> MatchRoles(GameStrategyEngine engine, WorldModel model, List<int> robotIDs, List<RoleInfo> rolesToBeAssigned, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Dictionary<int, RoleBase> newRoles = new Dictionary<int, RoleBase>();

            lock (this)
            {
                count = Math.Min(robotIDs.Count, rolesToBeAssigned.Count);
                costs = new double[count, count];
                //
                double[,] rolesSwitch = new double[count, count];
                Dictionary<int, RoleBase> rolesNotAssignedInSwitchMode = previouslyAssignedRoles.ToDictionary(p => p.Key, p => p.Value);
                Dictionary<int, bool> RobotsIDsNotAssignedInSwitch = robotIDs.ToDictionary(p => p, v => false);
                if (previouslyAssignedRoles.Count > 0)
                {
                    
                    for (int i = 0; i < count; i++)
                    {
                        RoleInfo roleInRow = rolesToBeAssigned[i];

                        if (previouslyAssignedRoles.Any(p => p.Value.GetType() == roleInRow.Role.GetType()))
                        {
                            
                            int robotId = previouslyAssignedRoles.Where(p => p.Value.GetType() == roleInRow.Role.GetType()).First().Key;
                            
                            int IndexOfRobotID = robotIDs.IndexOf(robotId);

                            
                            if (IndexOfRobotID != -1 && IndexOfRobotID< count)
                            {
                                rolesNotAssignedInSwitchMode.Remove(robotId);
                                RobotsIDsNotAssignedInSwitch[robotId] = true;

                                List<RoleBase> switches = roleInRow.Role.SwichToRole(engine, model, robotId, previouslyAssignedRoles);
                                if (switches.Count > 0)
                                {
                                    for (int j = 0; j < count; j++)
                                    {
                                        RoleInfo roleInColumn = rolesToBeAssigned[j];
                                        if (!switches.Any(p => p.GetType() == roleInColumn.Role.GetType()))
                                        {
                                            rolesSwitch[IndexOfRobotID, j] = 1000;
                                        }
                                        else
                                        {
                                            rolesSwitch[IndexOfRobotID, j] = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < count; j++)
                                    {
                                        rolesSwitch[IndexOfRobotID, j] = 1000;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    rolesSwitch[i, j] = 0;
                                }
                            }
                        }

                    }


                    foreach (var item in rolesNotAssignedInSwitchMode.Keys)
                    {
                        int IndexOfRobotID = robotIDs.IndexOf(item);
                        if (IndexOfRobotID != -1 && IndexOfRobotID < count)
                        {
                            RobotsIDsNotAssignedInSwitch[item] = true;

                            List<RoleBase> switches = rolesNotAssignedInSwitchMode[item].SwichToRole(engine, model, item, previouslyAssignedRoles);
                            if (switches.Count > 0)
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    RoleInfo roleInColumn = rolesToBeAssigned[j];
                                    if (!switches.Any(p => p.GetType() == roleInColumn.Role.GetType()))
                                    {
                                        rolesSwitch[IndexOfRobotID, j] = 1000;
                                    }
                                    else
                                    {
                                        rolesSwitch[IndexOfRobotID, j] = 0;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < count; j++)
                                {
                                    rolesSwitch[IndexOfRobotID, j] = 1000;
                                }
                            }
                        }
                        else
                        {
                            //foreach (var item2 in robotIDs)
                            for (int i = 0; i < count; i++)
                            {
                                int item2 = robotIDs[i];

                                if (RobotsIDsNotAssignedInSwitch[item2] == false)
                                {
                                    for (int j = 0; j < count; j++)
                                    {
                                        int ii = robotIDs.IndexOf(item2);
                                        rolesSwitch[ii, j] = 0;
                                    }
                                    RobotsIDsNotAssignedInSwitch[item2] = true;
                                }
                            }
                        }
                    }
                    //foreach (var item in robotIDs)
                    for (int i = 0; i < count; i++)
                    {
                        int item = robotIDs[i];
                        if (RobotsIDsNotAssignedInSwitch[item] == false)
                        {
                            for (int j = 0; j < count; j++)
                            {
                                int ii = robotIDs.IndexOf(item);
                                rolesSwitch[ii, j] = 0;
                            }
                            RobotsIDsNotAssignedInSwitch[item] = true;
                        }
                    }
                }
                
                //

				//VisualizerData.ClearMomentlyLists();

                for (int i = 0; i < count; i++)
                {
                    int robotID = robotIDs[i];
					//VisualizerData.AddMomently(robotID.ToString(),System.Drawing.Color.Black,1f, new Position2D(i / 2f + 1, -2.0));
                   // VisualizerData.AddMomently(rolesToBeAssigned[i].Role.ToString().Substring(23), System.Drawing.Color.Black, 1f, new Position2D(0.5, i / 2f - 2));
                    for (int j = 0; j < count; j++)
                    {
                        //if (rolesSwitch[i, j] != double.MaxValue)
                        {
                            RoleInfo roleInfo = rolesToBeAssigned[j];
                            RoleBase prev = null;
                            bool hadRole = previouslyAssignedRoles.TryGetValue(robotID, out prev);
                            costs[i, j] = (roleInfo.Role.CalculateCost(engine, model, robotID, previouslyAssignedRoles) + ((hadRole && prev.GetType() == roleInfo.Role.GetType()) ? -roleInfo.Margin : 0)) * roleInfo.Weight + rolesSwitch[i, j];
                            //costs[i, j] = costs[i, j] * Math.Abs(costs[i, j]);
                        }
                      //  VisualizerData.AddMomently(costs[i, j].ToString("f5"), System.Drawing.Color.Black, 1f, new Position2D(i / 2f + 1, j / 2f - 2));
                    }
                }

                bestMatch = new Dictionary<int, int>();
                bestMatchCost = double.MaxValue;
                remainingRoleIndices.Clear();
                for (int i = 0; i < count; i++)
                    remainingRoleIndices.Add(i);
                currentList = new int[count];
                currentCost = 0;
                findBestMatch(count - 1);

                for (int i = 0; i < count; i++)
                {
                    int robotID = robotIDs[i/*bestMatch[i]*/];
                    RoleBase last, newrole;
                    if (previouslyAssignedRoles.TryGetValue(robotID, out last) && last == rolesToBeAssigned[bestMatch[i]/*i*/].Role)
                        newrole = last;
                    else
                    {
                        newrole = rolesToBeAssigned[/*i*/bestMatch[i]].Role;
                        newrole.ResetState();
                    }
                    newRoles.Add(robotID, newrole);
                    if (newrole.QueryCategory() == RoleCategory.Goalie)
                        model.GoalieID = robotID;
                }
            }
            return newRoles;
        }

        double[,] costs;
        int count;
        List<int> remainingRoleIndices = new List<int>();
        Dictionary<int, int> bestMatch = null;
        double bestMatchCost;
        int[] currentList;
        double currentCost;
        void findBestMatch(int index)
        {
			for (int i = 0; i < remainingRoleIndices.Count; i++)
			{
				currentList[index] = remainingRoleIndices[i];
				currentCost += costs[index, currentList[index]];
				if (index == 0)
				{
					if (currentCost < bestMatchCost)
					{
						bestMatchCost = currentCost;
						bestMatch.Clear();
						for (int j = 0; j < count; j++)
							bestMatch.Add(j, currentList[j]);
					}
				}
				else
				{
					remainingRoleIndices.RemoveAt(i);
					findBestMatch(index - 1);
					remainingRoleIndices.Insert(i, currentList[index]);
				}
				currentCost -= costs[index, currentList[index]];
			}
			////int c = count;
			////double minVal = double.MaxValue,minI,minJ;
			//////for (int k = 0; k < c; k++)
			//////{
			//////    for (int i = 0; i < count; i++)
			//////    {
			//////        for (int j = 0; j < count; j++)
			//////        {
			//////            if (costs[i, j] < minVal)
			//////            {
			//////                minI = i;
			//////                minJ = j;
			//////                minVal = costs[i, j];
			//////            }
			//////        }
			//////    }

			//////}
			//for (int i = 0; i < count; i++)
			//{
			//    bestMatch.Add(i, currentList[i]);
			//}
            
        }


    }

    public struct RoleInfo
    {
        public RoleBase Role;
        public double Weight;
        public double Margin;
        public RoleInfo(RoleBase role, double weight, double margin)
        {
            Role = role;
            Weight = weight;
            Margin = margin;
        }
    }

}
