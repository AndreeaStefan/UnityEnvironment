using System.Collections.Generic;
using UnityEngine;

namespace ArmMove
{
    public class Finder
    {
        public static List<Transform> ChildrenWithTag(Transform parent, string tag)
        {
            var result = new List<Transform>();
            
            if(parent.childCount == 0) return result;

            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag)) result.Add(child);
                result.AddRange(ChildrenWithTag(child,tag));
            }

            return result;
        }
    }
}