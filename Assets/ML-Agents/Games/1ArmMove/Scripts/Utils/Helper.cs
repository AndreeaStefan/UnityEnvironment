
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


namespace ArmMove
{
    public static class Helper
    {
        public static dynamic LoadJson(string filePath)
        {
            if (File.Exists(filePath))
            {
                var reader = new StreamReader(filePath);
                string json = reader.ReadToEnd();
                var config = JsonConvert.DeserializeObject<dynamic>(json);
                return config;
            }

            return null;

        }
    }
}
