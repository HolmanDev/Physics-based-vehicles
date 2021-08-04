using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float timer;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(timer);
        Destroy(gameObject);
    }
}
