using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassTriggerSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<BoxCollider>().size = new Vector3 (10 - gameObject.transform.localScale.z * 1.5f, 3, 10 - gameObject.transform.localScale.x * 1.5f);
    }

}
