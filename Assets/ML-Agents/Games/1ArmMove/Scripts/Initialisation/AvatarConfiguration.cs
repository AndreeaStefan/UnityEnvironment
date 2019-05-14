using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace ArmMove
{
    public static class AvatarConfiguration
    {
        private static dynamic _configuration;
        private static readonly string _configPath = Application.streamingAssetsPath + "/specification.json";

        public static dynamic GetConfiguration()
        {
            if (_configuration == null) _configuration = LoadJson();
            return _configuration;
        }
        
        private static dynamic LoadJson()
        {
            Debug.Log($"About to obtain avatar specification from path {_configPath}");
            if (!File.Exists(_configPath)) return null;
            
            var reader = new StreamReader(_configPath);
            string json = reader.ReadToEnd();
            var config = JsonConvert.DeserializeObject<dynamic>(json);
            return config;
        }
    }
}