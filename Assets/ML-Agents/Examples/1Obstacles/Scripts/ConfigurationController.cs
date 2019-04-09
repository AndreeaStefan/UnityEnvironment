using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ML_Agents.Examples._1Obstacles.Scripts
{
    public class ConfigurationController
    {
        public Config[] configArray;

        public void InitConfiguration()
        {
            var scaleFactorArea = new float[] {1, 1.25f, 1.5f, 2};
            var scaleFactorPlayer = new float[] {1, 0.25f, 0.5f,  1.25f, 1.5f, 2};
            configArray = new Config[scaleFactorPlayer.Length * scaleFactorArea.Length];
            var i = 0;
            foreach (var t in scaleFactorArea)
            {
                foreach (var k in scaleFactorPlayer)         
                {
                    var config = new Config(t, k);
                    configArray[i] = config;
                    i++;
                }

            }
        }

        public Config GetConfigAt(int i)
        {
            return configArray[i];
        }
    }
}
