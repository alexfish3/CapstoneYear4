using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingLantern : MonoBehaviour
{
    [SerializeField] float randomOffset;
    [SerializeField] Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        randomOffset = Random.Range(0f, 3f);
        StartCoroutine(StartAnimation());
    }
    

    IEnumerator StartAnimation()
    {
        yield return new WaitForSeconds(randomOffset);
        animator.enabled = true;
    }
}
