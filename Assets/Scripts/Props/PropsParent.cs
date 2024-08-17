using UnityEngine;

public class PropsParent : MonoBehaviour
{
    void Start()
    {
        AddScriptToChildren(gameObject);
    }

    void AddScriptToChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            AddScriptRecursively(child.gameObject);
        }
    }

    void AddScriptRecursively(GameObject obj)
    {
        AddScriptToGameObject(obj);
        foreach (Transform child in obj.transform)
        {
            AddScriptRecursively(child.gameObject);
        }
    }

    void AddScriptToGameObject(GameObject obj)
    {
        if (obj.GetComponent<Prop>() == null)
        {
            obj.AddComponent<Prop>();
        }
    }
}
