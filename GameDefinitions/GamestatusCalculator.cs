using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Calculate the Status from refree
    /// </summary>
    public class GameStatusCalculator
    {
        /// <summary>
        /// Calculate the char that comes from the Ref Box
        /// </summary>
        /// <param name="LastGameStatus">Status from last Refree Command</param>
        /// <param name="p">charecter from ref Box</param>
        /// <param name="OurTeamIsYellow">Check if Our Color is Yellow</param>
        /// <returns>Status of Game</returns>
        public static GameStatus CalculateGameStatus(GameStatus LastGameStatus, char p, bool OurTeamIsYellow)
        {
            switch (p)
            {
                case 'V':
                case 'v':
                    return GameStatus.ComeHere;
                case 'H':
                    return GameStatus.Halt;
                case 'S':
                    return GameStatus.Stop;
                case ' ':
                    {
                        switch (LastGameStatus)
                        {
                            case GameStatus.KickOff_Opponent_Waiting:
                                return GameStatus.KickOff_Opponent_Go;
                            case GameStatus.KickOff_OurTeam_Waiting:
                                return GameStatus.KickOff_OurTeam_Go;
                            case GameStatus.Penalty_Opponent_Waiting:
                                return GameStatus.Penalty_Opponent_Go;
                            case GameStatus.Penalty_OurTeam_Waiting:
                                return GameStatus.Penalty_OurTeam_Go;
                            default:
                                return GameStatus.Normal;
                        }
                    }
                case 's':
                    {
                        return GameStatus.Normal;
                    }
                case 'k':
                case 'K':
                    return ((p == 'k') ^ OurTeamIsYellow) ? GameStatus.KickOff_Opponent_Waiting : GameStatus.KickOff_OurTeam_Waiting;
                case 'p':
                case 'P':
                    return ((p == 'p') ^ OurTeamIsYellow) ? GameStatus.Penalty_Opponent_Waiting : GameStatus.Penalty_OurTeam_Waiting;
                case 'f':
                case 'F':
                    return ((p == 'f') ^ OurTeamIsYellow) ? GameStatus.DirectFreeKick_Opponent : GameStatus.DirectFreeKick_OurTeam;
                case 'i':
                case 'I':
                    return ((p == 'i') ^ OurTeamIsYellow) ? GameStatus.IndirectFreeKick_Opponent : GameStatus.IndirectFreeKick_OurTeam;
                case 't':
                case 'T':
                    //return ((p == 't') ^ OurTeamIsYellow) ? GameStatus.Halt : GameStatus.Halt;
                    return ((p == 't') ^ OurTeamIsYellow) ? GameStatus.TestDefend : GameStatus.TestOffend;
                case 'A':
                case 'a':
                    return GameStatus.ComponetsTest;
                case 'M':
                case 'm':
                    return GameStatus.MoveRobot;
                case 'z':
                case 'Z':
                    return GameStatus.MatrixCalculator;
                case 'X':
                case 'x':
                    return GameStatus.PassShootTune;
                case 'B':
                case 'b':
                    return ((p == 'b') ^ OurTeamIsYellow) ? GameStatus.BallPlace_Opponent : GameStatus.BallPlace_OurTeam;    
                default:
                    return LastGameStatus;
            }
        }
    }
}