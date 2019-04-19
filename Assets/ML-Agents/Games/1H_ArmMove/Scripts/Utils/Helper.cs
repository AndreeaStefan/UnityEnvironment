using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Games._1H_ArmMove.Scripts.Utils
{
    public static class Helper
    {
        public static List<BodyPartConstrain> LoadJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string json = reader.ReadToEnd();
                List<BodyPartConstrain> config = JsonConvert.DeserializeObject<List<BodyPartConstrain>>(json);
                return config;
            }

            return null;

        }
    }
}
