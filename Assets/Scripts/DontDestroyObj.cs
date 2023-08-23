using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyObj : MonoBehaviour
{
    public static DontDestroyObj ddo;
    private void Awake()
    {
        if (ddo != null && ddo != this)
        {
            Destroy(gameObject);
            return;
        }
        ddo = this;
        DontDestroyOnLoad(gameObject);
    }
}
