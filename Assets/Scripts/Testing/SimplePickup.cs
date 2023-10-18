using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimplePickup : MonoBehaviour
{

    [SerializeField]
    private Text count;

    private int package;

    // Start is called before the first frame update
    void Start()
    {
        package = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Pickup")
        {
            package++;
            Destroy(gameObject);

        }
    }
}
