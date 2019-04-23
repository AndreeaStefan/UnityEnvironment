using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmMove
{
    public class BodyPartConstraint
    {
        // to identify the corresponding body part
        public string Name;

        public float ScaleX;
        public float ScaleY;
        public float ScaleZ;

        // joint constrains
        public float LowTwistLimit ;
        public float HighTwistLimit ;
        public float SwingLimit1 ;
        public float SwingLimit2 ;

        BodyPartConstraint(){}
        
        public BodyPartConstraint(dynamic source)
        {
            Name = source["Name"];
            ScaleX = source["ScaleX"];
            ScaleY = source["ScaleY"];
            ScaleZ = source["ScaleZ"];
            LowTwistLimit = source["LowTwistLimit"];
            HighTwistLimit = source["HighTwistLimit"];
            SwingLimit1 = source["SwingLimit1"];
            SwingLimit2 = source["SwingLimit2"];
        }

        public static BodyPartConstraint GetDefault()
        {
            return new BodyPartConstraint
            {
                Name = "default",
                ScaleX = 1,
                ScaleY = 1,
                ScaleZ = 1,
                LowTwistLimit = -20,
                HighTwistLimit = 70,
                SwingLimit1 = 40,
                SwingLimit2 = 40
            };
        }
       
    }
}
