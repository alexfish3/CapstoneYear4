using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TriggerRespawn : MonoBehaviour
{
    //Variables
    Respawn respawn;




    private void OnTriggerEnter(Collider player)
    {
        respawn.RespawnPlayer();

        
    }

}
