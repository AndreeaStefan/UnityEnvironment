using System.IO;
using Newtonsoft.Json;


namespace Assets.ML_Agents.Examples._1ArmMove.Scripts
{
    public static class Helper
    {
        public static ConfigSpecification LoadJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string json = reader.ReadToEnd();
                ConfigSpecification config = JsonConvert.DeserializeObject<ConfigSpecification>(json);
                return config;
            }

            return null;

        }
    }
}
