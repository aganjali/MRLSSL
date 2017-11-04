using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StillDesign.PhysX;

namespace Simulator
{
    public abstract class SimulatorBase
    {
        protected SimulatorBase()
            {
                Engine = new Engine();
                Engine.OnUpdate += UpdateEngine;
            }

            private void UpdateEngine(TimeSpan elapsed)
            {
                Update(elapsed);
            }

            protected abstract void Update(TimeSpan elapsed);

            protected void Run()
            {
                Engine.Run();
            }
            protected void Shutdown()
            {
                if (Engine.Core != null && !Engine.Core.IsDisposed)
                    Engine.Core.Dispose();
            }

            protected Engine Engine { get; private set; }
        }
    
}
