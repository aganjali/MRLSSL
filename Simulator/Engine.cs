using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StillDesign.PhysX;
using StillDesign.PhysX.MathPrimitives;
using System.Diagnostics;

namespace Simulator
{
    public delegate void UpdateEventHandler(TimeSpan elapsed);
    public class Engine
    {
        public event UpdateEventHandler OnUpdate;
        public Scene Scene { get; private set; }
        public Core Core { get; private set; }

        public Engine()
        {
            InitalizePhysics();
        }
        public void InitalizePhysics()
        {

            CoreDescription coreDesc = new CoreDescription();
            UserOutput output = new UserOutput();

            this.Core = new StillDesign.PhysX.Core(coreDesc, output);

            Core core = this.Core;
            core.SetParameter(PhysicsParameter.SkinWidth, 0.001f);
            //    core.SetParameter(PhysicsParameter.BounceThreshold, -4.5F);
            core.SetParameter(PhysicsParameter.VisualizeWorldAxes, true);

            try
            {
                SceneDescription sceneDesc = new SceneDescription()
                {
                    SimulationType = SimulationType.Hardware,
                    Gravity = new Vector3(0.0f, -9.81f, 0.0f),
                    GroundPlaneEnabled = true
                };
                this.Scene = core.CreateScene(sceneDesc);
            }
            catch
            {
                SceneDescription sceneDesc = new SceneDescription()
                {
                    SimulationType = SimulationType.Software,
                    Gravity = new Vector3(0.0f, -9.81f, 0.0f),
                    GroundPlaneEnabled = true
                };
                this.Scene = core.CreateScene(sceneDesc);
            }

            HardwareVersion ver = Core.HardwareVersion;
            SimulationType simType = this.Scene.SimulationType;

            // Connect to the remote debugger if it's there
            core.Foundation.RemoteDebugger.Connect("localhost");
        }

        public void Run()
        {
            //Application.DoEvents();

            Stopwatch timer = new Stopwatch();
            timer.Start();

            while (true)
            {
                // 60fps = 1/60 = 16.67 ms/frame
                if (timer.Elapsed < TimeSpan.FromMilliseconds(1))
                    continue;

                Update(timer.Elapsed);
                timer.Restart();
                // Application.DoEvents();
            }
        }

        private void Update(TimeSpan elapsed)
        {
            // Update Physics
            this.Scene.Simulate((float)elapsed.TotalSeconds);
            //_scene.Simulate( 1.0f / 60.0f );
            this.Scene.FlushStream();
            this.Scene.FetchResults(SimulationStatus.RigidBodyFinished, true);


            if (OnUpdate != null)
                OnUpdate(elapsed);
        }

    }
}
