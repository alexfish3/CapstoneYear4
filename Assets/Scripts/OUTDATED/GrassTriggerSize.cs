using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTriggerSize : MonoBehaviour
{
    //Variables
    private float grassTriggerScale = 1.8f; //Bigger number means smaller hitbox


    void Start()
    {
        //gameObject.GetComponent<BoxCollider>().size = new Vector3 (10 - gameObject.transform.localScale.z * grassTriggerScale, 3, 10 - gameObject.transform.localScale.x * grassTriggerScale);
    }

}
