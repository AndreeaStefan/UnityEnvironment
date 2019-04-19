using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmMove
{
    public class BodyPartConstrain
    {
        // to identify the corresponding body part
        public string Name;

        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;

        // joint constrains
        public float LowTwistLimit;
        public float HighTwistLimit;
        public float SwingLimit1;
        public float SwingLimit2;

    }
}
