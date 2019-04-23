using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


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

        public static Vector3 getRelativePosition(Transform origin, Vector3 objectPosition)
        {
            var distance = objectPosition - origin.position;
            var relativePosition = Vector3.zero;
            relativePosition.x = Vector3.Dot(distance, origin.right.normalized);
            relativePosition.y = Vector3.Dot(distance, origin.up.normalized);
            relativePosition.z = Vector3.Dot(distance, origin.forward.normalized);

            return relativePosition;
        }
    }
}