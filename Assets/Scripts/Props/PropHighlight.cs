using UnityEngine;
using UnityEngine.UI;

public class PropHighlight : MonoBehaviour
{
    public Camera playerCamera;
    public float raycastDistance = 100f;
    public float outlineWidth = 100f;
    public Color outlineColor = Color.red;
    private Text text;

    private GameObject currentHighlightedObject;
    void Start()
    {
        text = UIManager.Instance.objectDescription;
    }
    void Update()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, raycastDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            if (hitObject.TryGetComponent<Prop>(out var prop))
            {
                if (hitObject != currentHighlightedObject)
                {
                    RemoveHighlight();
                    AddHighlight(hitObject);
                    currentHighlightedObject = hitObject;
                    text.text = hitObject.name + " [ Size: " + prop.GetSizeCategory() + "(" + prop.GetSize() + ") ]";
                }
            }
            else
            {
                RemoveHighlight();
                currentHighlightedObject = null;
                text.text = "";
            }
        }
        else
        {
            RemoveHighlight();
            currentHighlightedObject = null;
            text.text = "";
        }
    }

    void AddHighlight(GameObject obj)
    {
        if (obj != null)
        {
            if (obj.GetComponent<Outline>() == null)
            {
                var outline = obj.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAll;
                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
            }
        }
    }

    void RemoveHighlight()
    {
        if (currentHighlightedObject != null)
        {
            Outline outline = currentHighlightedObject.GetComponent<Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }
        }
    }
}
