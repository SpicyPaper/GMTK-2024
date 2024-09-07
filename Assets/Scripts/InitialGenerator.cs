using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitialGenerator : MonoBehaviour
{
    public GameObject dontdestroyonload;
    public GameObject NetworkPrefab;

    private void Awake()
    {
        if (!DoneOnce.IsCreated)
        {
            Instantiate(NetworkPrefab);
            GameObject t = Instantiate(dontdestroyonload);
            DontDestroyOnLoad(t);
        }
    }
    private void Start()
    {
        if (!DoneOnce.IsCreated)
        {
            DoneOnce.IsCreated = true;
        }
    }
}
