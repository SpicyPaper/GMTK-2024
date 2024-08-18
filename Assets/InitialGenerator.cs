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
        else
        {
            IterateOverSceneGameObjects();
        }
    }

    private void IterateOverSceneGameObjects()
    {
        // Get the active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Get all root GameObjects in the active scene
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            // Perform action on each root GameObject
            Debug.Log("Root GameObject in Scene: " + rootObject.name);

            // Optionally, iterate over all children of the root GameObject
            IterateGameObjectHierarchy(rootObject.transform);
        }
    }

    private void IterateGameObjectHierarchy(Transform parent)
    {
        // Perform action on the parent GameObject
        if (parent.GetComponent<NetworkObject>() != null)
        {
            parent.GetComponent<NetworkObject>().Spawn();
        }

        // Recursively iterate over each child
        foreach (Transform child in parent)
        {
            IterateGameObjectHierarchy(child);
        }
    }
}
