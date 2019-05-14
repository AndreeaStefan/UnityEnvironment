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

        public static Vector3 getNormalisedRelativePosition(Transform origin, Vector3 objectPosition, Vector3 overrideMax)
        {
            var current = getRelativePosition(origin, objectPosition);
            var originScale = origin.localScale;
            return getNormalised(originScale, current, overrideMax);
        }

        public static Vector3 getNormalised(Vector3 originScale, Vector3 current, Vector3 overrideMax)
        {
            var max = new Vector3(  (int)overrideMax.x == 0? originScale.x : overrideMax.x,
                (int)overrideMax.y == 0? originScale.y : overrideMax.y,
                (int)overrideMax.z == 0? originScale.z : overrideMax.z);            
            
            return new Vector3(current.x/max.x, current.y / max.y, current.z / max.z);
        }
        
    }
}