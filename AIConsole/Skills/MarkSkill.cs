using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills 
{
    class MarkSkill : SkillBase
    {
        public static Position2D ourDangerZoneRightCorner = GameParameters.OurGoalCenter.Extend(-GameParameters.DefenceAreaHeight, -GameParameters.DefenceAreaWidth/2);
        public static Position2D ourDangerZoneLeftCorner = GameParameters.OurGoalCenter.Extend(-GameParameters.DefenceAreaHeight, GameParameters.DefenceAreaWidth/2);
        Line dangerZoneTop = new Line(ourDangerZoneRightCorner, ourDangerZoneLeftCorner);
       // Line dangerZoneRight = new Line(ourDangerZoneRightCorner, GameParameters.OurGoalRight + 0.60);
        //Line dangerZoneLeft = new Line(ourDangerZoneRightCorner, GameParameters.OurGoalLeft);

        Line dangerZoneRight = new Line(GameParameters.OurGoalLeft.Extend(-1.20, 0.60 ), GameParameters.OurGoalLeft.Extend(0, 0.60 ));

        Line dangerZoneLeft = new Line(GameParameters.OurGoalRight.Extend(-1.20, -0.6 ), GameParameters.OurGoalRight.Extend(0, -0.60 ));

        public Position2D OnDangerZoneMark(int robotId, WorldModel model, Position2D oppToMark , double margin = 0.12)
        {
            Position2D target = new Position2D();
            Line oppToGoal = new Line(oppToMark, GameParameters.OurGoalCenter);
            
            if (GameParameters.SegmentIntersect(oppToGoal, dangerZoneTop).HasValue)
            {

                target = GameParameters.SegmentIntersect(oppToGoal, dangerZoneTop).Value;

            }
            else if(GameParameters.SegmentIntersect(oppToGoal, dangerZoneRight).HasValue)
            {
                target = GameParameters.SegmentIntersect(oppToGoal, dangerZoneRight).Value;
            }
            else if (GameParameters.SegmentIntersect(oppToGoal, dangerZoneLeft).HasValue)
            {
                target = GameParameters.SegmentIntersect(oppToGoal, dangerZoneLeft).Value;
            }
            else
            {
                target = GameParameters.OurGoalCenter.Extend(GameParameters.DefenceAreaHeight + 0.10 , 0);
            }
            Vector2D v = (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(margin);
            DrawingObjects.AddObject(oppToGoal);
            DrawingObjects.AddObject(dangerZoneLeft);
            DrawingObjects.AddObject(dangerZoneRight);
            target = target + v;
            DrawingObjects.AddObject(new Circle(target, 0.1, new Pen(Color.Red, 0.01f)));
            return target;
        }
        

    }
    enum MarkType
    {
        OnDangerZone,
        Linear

    }
}
