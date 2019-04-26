using System.IO;
using Newtonsoft.Json;

namespace ArmMove
{
    public static class AvatarConfiguration
    {
        private static dynamic _configuration;
        private static readonly string _configPath = "Assets/config/specification.json";

        public static dynamic GetConfiguration()
        {
            if (_configuration == null) _configuration = LoadJson();
            return _configuration;
        }
        
        private static dynamic LoadJson()
        {
            if (!File.Exists(_configPath)) return null;
            
            var reader = new StreamReader(_configPath);
            string json = reader.ReadToEnd();
            var config = JsonConvert.DeserializeObject<dynamic>(json);
            return config;
        }
    }
}