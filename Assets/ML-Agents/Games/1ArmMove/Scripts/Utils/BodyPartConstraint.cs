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

        public readonly float ScaleX;
        public readonly float ScaleY;
        public readonly float ScaleZ;

        // joint constrains
        public readonly float LowAngularXLimit ;
        public readonly float HighAngularXLimit ;
        public readonly float AngularYLimit ;
        public readonly float AngularZLimit ;


        public readonly bool XRotationLocked;
        public readonly bool YRotationLocked;
        public readonly bool ZRotationLocked;

        public BodyPartConstraint(dynamic source)
        {
            Name = source["Name"];
            ScaleX = source["ScaleX"];
            ScaleY = source["ScaleY"];
            ScaleZ = source["ScaleZ"];
            XRotationLocked = source["XRotationLocked"];
            YRotationLocked = source["YRotationLocked"];
            ZRotationLocked = source["ZRotationLocked"];
            LowAngularXLimit = source["LowAngularXLimit"];
            HighAngularXLimit = source["HighAngularXLimit"];
            AngularYLimit = source["AngularYLimit"];
            AngularZLimit = source["AngularZLimit"];
        }
       
    }
}
