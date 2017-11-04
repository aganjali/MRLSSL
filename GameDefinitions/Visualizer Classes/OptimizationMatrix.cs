using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRL.SSL.GameDefinitions.Visualizer_Classes
{
    public class OptimizationMatrix
    {

        public OptimizationMatrix(MathMatrix e, MathMatrix d)
        {
            _e = e;
            _d = d;
        }
        public OptimizationMatrix()
        {
        }
        private MathMatrix _e;
        public MathMatrix E
        {
            get { return _e; }
            set 
            {
                if (_e == null)
                    _e = MathMatrix.IdentityMatrix(3, 4);
                _e = value; 
            }
        }

        private MathMatrix _d;
        public MathMatrix D
        {
            get { return _d; }
            set
            {
                if (_d == null)
                    _d = MathMatrix.IdentityMatrix(4, 1);
                _d = value;
            }
        }

        public void Identitymatrix()
        {
            _e = MathMatrix.IdentityMatrix(3, 4);
            _d = MathMatrix.IdentityMatrix(4, 1);
        }

    }
}
