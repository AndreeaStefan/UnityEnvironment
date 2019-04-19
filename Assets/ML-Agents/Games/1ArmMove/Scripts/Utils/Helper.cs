
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace ArmMove
{
    public static class Helper
    {
        public static Dictionary<string, BodyPartConstrain> LoadJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string json = reader.ReadToEnd();
                Dictionary<string, BodyPartConstrain> config = JsonConvert.DeserializeObject<Dictionary<string, BodyPartConstrain>>(json);
                return config;
            }

            return null;

        }
    }
}
