using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floorchecker : MonoBehaviour
{
    [SerializeField]
    private GameObject playerBody;

    //Can be accessed to check whether the vehicle is on the ground
    private bool grounded;
    public bool Grounded { get { return grounded; } }

    private void Update()
    {
        grounded = CheckGrounded(playerBody);
    }

    private bool CheckGrounded(GameObject body)
    {
        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, Vector3.down, out hit))
        {
            if (hit.distance > 1)
            {
                return false;
            }
        }

        return true;
    }
}
