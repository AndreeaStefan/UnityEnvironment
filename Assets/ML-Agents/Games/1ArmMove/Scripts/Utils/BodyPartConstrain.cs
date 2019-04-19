namespace Games._1ArmMove.Scripts.Utils
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
