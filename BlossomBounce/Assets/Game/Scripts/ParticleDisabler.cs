using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDisabler : MonoBehaviour
{
    private ParticleSystem ps;
    private WaitForSeconds checkDelay;

    public Material childMat;

    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();

        checkDelay = new WaitForSeconds(0.1f);

        childMat.SetColor("_Color", Color.white);

        StartCoroutine(CheckIfAlive());
    }

    private IEnumerator CheckIfAlive()
    {
        while (ps != null)
        {
            yield return checkDelay;

            if (!ps.IsAlive(true))
            {
                gameObject.SetActive(false);

                break;
            }
        }
    }
}