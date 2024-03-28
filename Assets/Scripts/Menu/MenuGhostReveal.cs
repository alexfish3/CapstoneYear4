using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuGhostReveal : MonoBehaviour
{
    [SerializeField] float timeToStart;
    [SerializeField] float speed;
    [SerializeField] Animator animator;
    [SerializeField] Image menuGhostRevealObject;
    [SerializeField] Material menuGhostRevealMaterialReference;
    Material menuGhostRevealMaterial;

    // Start is called before the first frame update
    void Start()
    {
        menuGhostRevealMaterial = new Material(menuGhostRevealMaterialReference);
        menuGhostRevealObject.material = menuGhostRevealMaterial;
        menuGhostRevealMaterial.SetFloat("_Cutoff", 0);

        menuGhostRevealObject.enabled = true;

        StartCoroutine(WaitToStart());
    }

    private void Reveal()
    {
        StartCoroutine(RevealScreen());
    }

    private IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(timeToStart);
        animator.enabled = true;
    }

    private IEnumerator RevealScreen()
    {
        for (float i = 0; i < 100; i++)
        {
            float num = (1 / 100f) * (i + 1);
            menuGhostRevealMaterial.SetFloat("_Cutoff", num);
            yield return new WaitForSeconds(speed/ 100);
        }
    }



}
