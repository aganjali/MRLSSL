using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public static class SpecializedExtensions
    {
        public static SingleObjectState ToAdhamiMatrix(this SingleObjectState source)
        {
            SingleObjectState sos = source.Copy();
            MathMatrix A = new MathMatrix(3, 3);
            A = MathMatrix.IdentityMatrix(3, 3);

            A[0, 0] = 1.4235; A[0, 1] = -0.1002; A[0, 2] = 0.0286;
            A[1, 0] = -0.0648; A[1, 1] = 1.1874; A[1, 2] = -0.0173;
            A[2, 0] = -0.1808; A[2, 1] = -0.0624; A[2, 2] = 1.0378;

            MathMatrix B = new MathMatrix(3, 1);
            B[0, 0] = 0.0044;
            B[1, 0] = 0.0036;
            B[2, 0] = 0.0289;

            MathMatrix Vc = new MathMatrix(3, 1);
            Vc[0, 0] = sos.Speed.X;
            Vc[1, 0] = sos.Speed.Y;
            Vc[2, 0] = (sos.AngularSpeed.HasValue) ? sos.AngularSpeed.Value : 0;
            MathMatrix VR = A * Vc + B;
            double speedX = VR[0, 0];
            double speedY = VR[1, 0];
            double angSpeed = VR[2, 0];

            speedX = Math.Round(speedX, 3);
            speedY = Math.Round(speedY, 3);

            sos.Speed = new MRL.SSL.CommonClasses.MathLibrary.Vector2D(speedX, speedY);
            sos.AngularSpeed = (float)Math.Round(angSpeed, 0);
            return sos;
        }

        public static SingleWirelessCommand ToAdhamiMatrix(this SingleWirelessCommand source,SingleObjectState state)
        {
            SingleWirelessCommand swc = source.Copy();
            //MathMatrix A = new MathMatrix(3, 6);
            //A = MathMatrix.IdentityMatrix(3, 6);

            ////A[0, 0] = 5.2474; A[0, 1] = -0.2614; A[0, 2] = -0.2410; A[0, 3] = -3.3225; A[0, 4] = 0.3612; A[0, 5] = 0.2364;
            ////A[1, 0] = -0.9303; A[1, 1] = 4.1713; A[1, 2] = 0.0487; A[1, 3] = 0.7918; A[1, 4] = -2.9538; A[1, 5] = -0.0554;
            ////A[2, 0] = -9.8004; A[2, 1] = -0.2057; A[2, 2] = 3.2401; A[2, 3] = 7.5002; A[2, 4] = -0.0434; A[2, 5] = -2.3016;


            ////A[0, 0] = 1.5087; A[0, 1] = 0.0406; A[0, 2] = 0.0223; A[0, 3] = 0.3010; A[0, 4] = 0.0571; A[0, 5] = -0.0145;
            ////A[1, 0] = 0.0042; A[1, 1] = 1.1974; A[1, 2] = 0.0009; A[1, 3] = -0.0978; A[1, 4] = -0.0402; A[1, 5] = -0.0107;
            ////A[2, 0] = -0.5755; A[2, 1] = -0.0975; A[2, 2] = 0.9796; A[2, 3] = -1.0638; A[2, 4] = -0.0954; A[2, 5] = -0.2248;


            //A[0, 0] = 1.7972; A[0, 1] = 0.0964; A[0, 2] = 0.0108; A[0, 3] = 0; A[0, 4] = 0; A[0, 5] = 0;
            //A[1, 0] = -0.0906; A[1, 1] = 1.1579; A[1, 2] = -0.0098; A[1, 3] = 0; A[1, 4] = 0; A[1, 5] = 0;
            ////A[2, 0] = -1.6178; A[2, 1] = -0.1953; A[2, 2] = 0.7644; A[2, 3] = 0; A[2, 4] = 0; A[2, 5] = 0;
            //A[2, 0] = -0.2178; A[2, 1] = -0.1953; A[2, 2] = 0.7644; A[2, 3] = 0; A[2, 4] = 0; A[2, 5] = 0;


            /////
            ///// 27 esfand :*
            /////
            ////A[0, 0] = 1.2060; A[0, 1] = -0.0751; A[0, 2] = 0.0579; A[0, 3] = 0.5126; A[0, 4] = -0.0225; A[0, 5] = 0.0320;
            ////A[1, 0] = 0.0304; A[1, 1] = 0.7183; A[1, 2] = 0.0706; A[1, 3] = 0.0324; A[1, 4] = 0.4828; A[1, 5] = -0.0588;
            ////A[2, 0] = -0.4811; A[2, 1] = -0.0611; A[2, 2] = 0.8263; A[2, 3] = -0.3142; A[2, 4] = -0.1098; A[2, 5] = -1.0252;


            ////A[0, 0] = 1.6896; A[0, 1] = -0.1038; A[0, 2] = 0.1062; A[0, 3] = 0; A[0, 4] = 0; A[0, 5] = 0;
            ////A[1, 0] = 0.0570; A[1, 1] = 1.1264; A[1, 2] = 0.0279; A[1, 3] = 0; A[1, 4] = 0; A[1, 5] = 0;
            ////A[2, 0] = -0.6058; A[2, 1] = -0.1262; A[2, 2] = -0.001; A[2, 3] = 0; A[2, 4] = 0; A[2, 5] = 0;



            ////A[0, 0] = 1; A[0, 1] = 0; A[0, 2] = 0; A[0, 3] = 0; A[0, 4] = 0; A[0, 5] = 0;
            ////A[1, 0] = 0; A[1, 1] = 1; A[1, 2] = 0; A[1, 3] = 0; A[1, 4] = 0; A[1, 5] = 0;
            ////A[2, 0] = 0; A[2, 1] = 0; A[2, 2] = 1; A[2, 3] = 0; A[2, 4] = 0; A[2, 5] = 0;

            //MathMatrix B = new MathMatrix(3, 1);
            //B[0, 0] = 0;
            //B[1, 0] = 0;
            //B[2, 0] = 0;
     

            //MathMatrix Vc = new MathMatrix(6, 1);
            //Vc[0, 0] = swc.Vx;
            //Vc[1, 0] = swc.Vy;
            //Vc[2, 0] = swc.W;
            //Vc[3, 0] = state.Speed.X;//
            //Vc[4, 0] = state.Speed.Y;//
            //Vc[5, 0] = state.AngularSpeed.Value;//
            //MathMatrix VR = A * Vc + B;
            //double speedX = VR[0, 0];
            //double speedY = VR[1, 0];
            //double angSpeed = VR[2, 0];

            //speedX = Math.Round(speedX, 3);
            //speedY = Math.Round(speedY, 3);
            //Vector2D linearSpeed = new Vector2D(speedX, speedY);
            //if (linearSpeed.Size > 3)
            //    linearSpeed.NormalizeTo(3);

            //if (Math.Abs(angSpeed) > 5)
            //    angSpeed = Math.Sign(angSpeed) * 5;

            //swc.Vx = linearSpeed.X;
            //swc.Vy = linearSpeed.Y;
            //swc.W = Math.Round(angSpeed, 3);

            MathMatrix A = new MathMatrix(3, 3);
            A[0, 0] = 1.0103; A[0, 1] = 0; A[0, 2] = -0.0025; 
            A[1, 0] = 0; A[1, 1] = 0.9957; A[1, 2] = 0;
            A[2, 0] = -0.2516; A[2, 1] = 0; A[2, 2] = 1.0043;
            
            MathMatrix B = new MathMatrix(3, 1);
            B[0, 0] = swc.Vx;
            B[1, 0] = swc.Vy;
            B[2, 0] = swc.W;

            B = A * B;

            swc.Vx = B[0, 0];
            swc.Vy = B[1, 0];
            swc.W = B[2, 0];

            return swc;
        }

        public static SingleWirelessCommand ToAdhamiMatrix(this SingleWirelessCommand source, SingleObjectState state,bool OP)
        {
            SingleWirelessCommand swc = source.Copy();

            MathMatrix temp = new MathMatrix(3, 1);
            temp[0, 0] = swc.Vx;
            temp[1, 0] = swc.Vy;
            temp[2, 0] = swc.W;

            MathMatrix result = GlobalOptimizationCommands.A_old * temp;

            swc.Vx = result[0, 0];
            swc.Vy = result[1, 0];
            swc.W = result[2, 0];
            if (double.IsNaN(swc.Vx))
            {
            }

            return swc;
        }

        public static SingleWirelessCommand Copy(this SingleWirelessCommand source)
        {
            SingleWirelessCommand swc = new SingleWirelessCommand();
            swc.Vx = source.Vx;
            swc.Vy = source.Vy;
            swc.W = source.W;
            swc._kickPower = source._kickPower;
            swc._kickPowerByte = source._kickPowerByte;
            swc.isChipKick = source.isChipKick;
            swc.isDelayedKick = source.isDelayedKick;
            swc.KickPower = source.KickPower;
            swc.Motor1 = source.Motor1;
            swc.Motor2 = source.Motor2;
            swc.Motor3 = source.Motor3;
            swc.Motor4 = source.Motor4;
            swc.SpinBack = source.SpinBack;
            swc.spinBackward= source.spinBackward;
            swc.statusRequest = source.statusRequest;
            return swc;
        }

        public static SingleObjectState Copy(this SingleObjectState source)
        {
            SingleObjectState sos = new SingleObjectState();
            sos.Acceleration = source.Acceleration;
            sos.Angle = source.Angle;
            sos.AngularSpeed = source.AngularSpeed;
            sos.ChangedInSimulutor = source.ChangedInSimulutor;
            sos.IsShown = source.IsShown;
            sos.Location = source.Location;
            sos.Opacity = source.Opacity;
            sos.ParentState = source.ParentState;
            sos.Speed = source.Speed;
            sos.Type = source.Type;
            return sos;
        }

    }
}
