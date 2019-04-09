using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ML_Agents.Examples._1Obstacles.Scripts
{
    public class Config
    {
        public float ScaleArea;
        public float ScalePlayer;

        public Config(float scaleArea, float scalePlayer)
        {
            ScaleArea = scaleArea;
            ScalePlayer = scalePlayer;

        }
    }
}
