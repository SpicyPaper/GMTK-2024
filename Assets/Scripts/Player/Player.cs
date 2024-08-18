using KinematicCharacterController;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Unity objects")]
    [SerializeField] private GameObject meshParent;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Parameters")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float shootRaycastDistance = 100f;
    [SerializeField] private float morphRaycastDistance = 5f;
    [SerializeField] private float grabRaycastDistance = 5f;
    [SerializeField] private float highlightRaycastDistance = 5f;
    [SerializeField] private float hightlightPropWidth = 100f;
    [SerializeField] private Color hightlightPropColor = Color.red;

    private GameObject newInst;
    private GameObject currentHighlightedObject;
    private Prop currentlyHeldObject;
    private Text propGrabedText;

    private void Start()
    {
        propGrabedText = UIManager.Instance.objectDescription;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Morph();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Grab();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Shoot();
        }

        HighlightProps();
    }

    private void Morph()
    {
        Vector3 capsuleEnd = transform.position + Vector3.up * (playerHeight - 0.5f);
        if (Physics.CapsuleCast(transform.position, capsuleEnd, 0.5f, transform.forward,
            out RaycastHit hit, morphRaycastDistance))
        {
            if (hit.collider.gameObject.GetComponent<Prop>())
            {
                ChangeAppearanceAndTransform(hit.collider.gameObject);
            }
        }
    }

    private void ChangeAppearanceAndTransform(GameObject propObject)
    {
        ChangeRendererVisibility(meshParent, false);
        newInst = Instantiate(propObject);
        DisableColliders(newInst);

        if (newInst.GetComponent<Rigidbody>())
        {
            newInst.GetComponent<Rigidbody>().isKinematic = true;
        }

        BoxCollider newCollider = newInst.GetComponent<BoxCollider>();
        transform.GetComponent<KinematicCharacterMotor>()
            .SetCapsuleDimensions(newCollider.size.y / 4, newCollider.size.y, newCollider.size.y / 4);

        newInst.transform.SetParent(meshParent.transform);
        newInst.transform.SetLocalPositionAndRotation(
            new Vector3(0, -newCollider.center.y / 2, 0), 
            Quaternion.Euler(Vector3.zero)
        );
        newInst.transform.localScale = propObject.transform.localScale;
    }

    private void DisableColliders(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    private void ChangeRendererVisibility(GameObject target, bool enabled)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        foreach (Renderer col in renderers)
        {
            col.enabled = enabled;
        }
    }

    private void Grab()
    {
        if (currentlyHeldObject == null)
        {
            Vector3 capsuleEnd = transform.position + Vector3.up * (playerHeight - 0.5f);
            Debug.DrawRay(transform.position, transform.forward * grabRaycastDistance,
                Color.red, 1f);

            if (Physics.CapsuleCast(transform.position, capsuleEnd, 0.5f, transform.forward,
                out RaycastHit raycastHit, grabRaycastDistance))
            {
                if (raycastHit.transform.TryGetComponent(out Prop prop))
                {
                    prop.Grab(objectGrabPoint);
                    currentlyHeldObject = prop;
                }
            }
        }
        else
        {
            currentlyHeldObject.Drop();
            currentlyHeldObject = null;
        }
    }

    private void Shoot()
    {
        if (playerCamera == null)
        {
            Debug.LogError("Camera for shoot raycast not assigned");
            return;
        }

        Vector3 ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;
        Vector3 rayOrigin = new(transform.position.x, transform.position.y + 1.43f,transform.position.z);

        if (Physics.Raycast(rayOrigin, ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject.GetComponent<Prop>())
            {
                Debug.Log("Aïe raté");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerInteraction>().TakeDamageServerRpc(20);
            }
        }

        Debug.DrawRay(rayOrigin, ray * shootRaycastDistance, Color.red, 2f);
    }

    private void HighlightProps()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, highlightRaycastDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.TryGetComponent<Prop>(out var prop))
            {
                if (hitObject != currentHighlightedObject)
                {
                    RemoveHighlight();
                    AddHighlight(hitObject);
                    currentHighlightedObject = hitObject;
                    propGrabedText.text = hitObject.name + " [ Size: " + prop.GetSizeCategory() + "(" + prop.GetSize() + ") ]";
                }
            }
            else
            {
                RemoveHighlight();
                currentHighlightedObject = null;
                propGrabedText.text = "";
            }
        }
        else
        {
            RemoveHighlight();
            currentHighlightedObject = null;
            propGrabedText.text = "";
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
                outline.OutlineColor = hightlightPropColor;
                outline.OutlineWidth = hightlightPropWidth;
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
