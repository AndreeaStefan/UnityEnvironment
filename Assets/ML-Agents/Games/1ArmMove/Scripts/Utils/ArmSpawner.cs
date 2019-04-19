using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmSpawner : MonoBehaviour
{

    public GameObject prefab;

    public GameObject connectedObject; 

    // Start is called before the first frame update
    public void AddArm()
    {
       var position = new Vector3(0,0,0);
       Instantiate(prefab, position, Quaternion.identity);
    }

   
}
