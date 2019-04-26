using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


namespace ArmMove
{
    public static class Helper
    {

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