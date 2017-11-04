using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    /// <summary>
    /// Status of Game from Refree
    /// </summary>
    public enum GameStatus
    {
        Normal,
        Stop,
        Halt,
        KickOff_OurTeam_Waiting,
        KickOff_OurTeam_Go,
        KickOff_Opponent_Waiting,
        KickOff_Opponent_Go,
        DirectFreeKick_OurTeam,
        DirectFreeKick_Opponent,
        IndirectFreeKick_OurTeam,
        IndirectFreeKick_Opponent,
        CornerKick_OurTeam,
        CornerKick_Opponnent,
        Penalty_OurTeam_Waiting,
        Penalty_OurTeam_Go,
        Penalty_Opponent_Waiting,
        Penalty_Opponent_Go,
        /// <summary>
        /// Test Status for Defending
        /// </summary>
        TestDefend,
        /// <summary>
        /// Test Status for Ofending
        /// </summary>
        TestOffend,
        ComponetsTest,
        ComeHere,
        MoveRobot,
        MatrixCalculator,
        PassShootTune, 
        BallPlace_OurTeam,
        BallPlace_Opponent
    }
}
