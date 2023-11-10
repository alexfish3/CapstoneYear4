using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnGravestone : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitToKill());
    }

    private IEnumerator WaitToKill()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
