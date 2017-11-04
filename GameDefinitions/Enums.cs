using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions
{
    public enum FieldOrientation
    {
        Verticaly = 0,
        Horzintaly = 1,
    }
    /// <summary>
    /// Game strategy
    /// </summary>
    public enum DribleState
    {
        Target,
        Horizontal,
        Vertical,
    }
    /// <summary>
    /// our penaltygoaller Strategy
    /// </summary>
    public enum OurPenaltyGoaller
    {
        MoveByOppAngle,
        RandomlyMove,
        Dive,
        DiveByChangeAngle,
    }
    /// <summary>
    /// our kickoff strategy
    /// </summary>
    public enum OurKickOff
    {
        Auto,
        Direct,
        SidePassing,
        BackPassing,
        ChipPass
    }
    /// <summary>
    /// our indirectfreekick strategy
    /// </summary>
    public enum OurIndirectFreeKick
    {
        Auto,
        Normal,
        Heading,
        Compress,
        Heavy,
        NearFree,
        VeryHeavy,
        Double
    }
    public enum PenaltyShoter
    {
        Corner = 0,
        Center = 1
    }
    /// <summary>
    /// our indirectfreekick strategy
    /// </summary>
    public enum OurIndirectFreeKickMiddle
    {
        Auto,
        Normal,
        Heading,
        Compress,
        Heavy,
        NearFree
    }
    /// <summary>
    /// our indirectfreekick strategy
    /// </summary>
    public enum OurIndirectFreeKickOpponent
    {
        Auto,
        Normal,
        Heading,
        Compress,
        Heavy,
        NearFree
    }
    /// <summary>
    /// our normal game strategy
    /// </summary>
    public enum OurNormalGame
    {
        Auto,
        Defence,
        Normal,
        Attack,
        Direct,
        HalfBack
    }
    /// <summary>
    /// mode for calculate kick power
    /// </summary>
    public enum Calculation_Mode
    {
        ChipWithSpinBack = 0,
        ChipWithoutSpinBack = 1,
        DirectWithSpinBack = 2,
        Direct = 3
    }
    /// <summary>
    /// our cornerkicks strategy
    /// </summary>
    public enum OurCornerKick
    {
        Auto,
        Heading,
        Near,
        Normal,
        Compress,
        Heavy
    }
    /// <summary>
    /// ball state
    /// </summary>
    public enum ActiveRoleStates
    {
        FreeBall_WeCanGet_Static,
        FreeBall_WeCanGet_Incoming,
        FreeBall_WeCanGet_Outgoing,
        FreeBall_TheyCanGet,
        InOurCustody,
        InTheirCustody,
        InConflict
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ActiveRoleStates2
    {
        FreeBall_WeCanGet_Static,
        FreeBall_WeCanGet_Incoming,
        FreeBall_WeCanGet_Outgoing,
        FreeBall_TheyCanGet,
        InOurCustody,
        InTheirCustody,
        InConflict
    }
    /// <summary>
    /// 
    /// </summary>
    public enum BallPosition
    {
        FarFromGoal,
        Alarm,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum DrawingObjectType
    {
        Line,
        Circle,
        Rectangle,
        DrawCollection,
        SingleObjectState,
        Text,
        Region,
        Position,
        Region3D,
        Position3D
    }
    /// <summary>
    /// 
    /// </summary>
    public enum SerializedDataType
    {
        GlobalModel,
        Bool,
        Model,
        Balls,
        MergerSetting,
        Techniques,
        Tactics,
        Refree,
        NetworkSetting,
        Customvariables,
        Engines,
        Robots,
        LockupTable,
        BallIndex,
        SendedDataList,
        Position,
        Console,
        RobotCommand,
        SendDevice,
        RecieveMode,
        SingleObjectSate,
        Tactic,
        GameParameters,
        BatteryFlag,
        SenderDevice,
        ComponentsCommand,
        Robot,
        BallStatus,
        MoveRobot,
        AngleError,
        OPMatrix,
        RobotData,
        ActiveSetting,
        Sensore,
        Strategies,
        StrategySchema,
        ControlData,
        RotateParameter,
        PassShootTune,
        FourCamMergerSetting,
        ballPlacementPos
    }
    /// <summary>
    /// 
    /// </summary>
    public enum StreamControlCode
    {
        Begin,
        Continue,
        End,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum WirelessSenderDevice
    {
        AI,
        Visualizer
    }
    /// <summary>
    /// 
    /// </summary>
    public enum ModelRecieveMode
    {

        SSLVision,
        Visualizer,
        Simulator,
        Analizer,
        GrSimulator
    }

    /// <summary>
    /// New OneTouch Mode
    /// </summary>
    public enum OneTouchMode
    {
        Normal,
        Random
    }
}
