using KinematicCharacterController;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
    [Header("Unity objects")]
    [SerializeField] private GameObject meshParent;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private AudioClip shootingNoise;
    [SerializeField] private LayerMask playerLayer;

    [Header("Parameters")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float shootRaycastDistance = 100f;
    [SerializeField] private float morphRaycastDistance = 5f;
    [SerializeField] private float grabRaycastDistance = 5f;
    [SerializeField] private float highlightRaycastDistance = 5f;
    [SerializeField] private float hightlightPropWidth = 100f;
    [SerializeField] private Color hightlightPropColor = Color.red;

    private GameObject propCopyObject;
    private GameObject currentHighlightedObject;
    private Prop currentlyHeldObject;
    private Text propGrabedText;

    private void Start()
    {
        propGrabedText = UIManager.Instance.objectDescription;
    }

    void Update()
    {
        switch (GetComponent<CheckType>().CurrentType.Value)
        {
            case CheckType.Type.Hunter:
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Shoot();
                }
                break;
            case CheckType.Type.Morph:
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Morph();
                }
                break;
            default:
                Debug.Log("Should not happen");
                break;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Grab();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            PlayRandomSound();
        }

        HighlightProps();
    }

    private void Morph()
    {
        Vector3 capsuleEnd = transform.position + Vector3.up * (playerHeight - 0.5f);
        if (Physics.CapsuleCast(transform.position, capsuleEnd, 0.5f, transform.forward,
            out RaycastHit hit, morphRaycastDistance))
        {
            GameObject propObject = GetParentWithProp(hit.transform.gameObject);
            if (propObject)
            {
                ChangeAppearanceAndTransform(propObject);
            }
        }
    }

    private GameObject GetParentWithProp(GameObject obj)
    {
        Transform currentParent = obj.transform;
        while (currentParent != null)
        {
            if (currentParent.GetComponent<Prop>())
            {
                return currentParent.gameObject;
            }

            currentParent = currentParent.parent;
        }

        return null;
    }

    private void ChangeAppearanceAndTransform(GameObject propObject)
    {
        // Destroy the previous transformation
        foreach (Transform child in meshParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Disable Renderer visibility
        ChangeRendererVisibility(meshParent, false);

        // Create a copy of the prop for the transformation
        propCopyObject = Instantiate(propObject);
        DisableColliders(propCopyObject);

        if (propCopyObject.GetComponent<Rigidbody>())
        {
            propCopyObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        propCopyObject.transform.SetParent(meshParent.transform);
        propCopyObject.transform.localScale = propObject.transform.localScale;
        propCopyObject.transform.SetLocalPositionAndRotation(
            new Vector3(0, 0, 0),
            Quaternion.Euler(Vector3.zero)
        );

        // Compute capsule propoerties for the character motor
        Bounds computedBounds = ComputeBounds(propCopyObject);

        float height = computedBounds.size.y;
        float radius = Mathf.Min(Mathf.Min(computedBounds.size.x, computedBounds.size.z) / 2f, height / 2f);

        transform.GetComponent<KinematicCharacterMotor>()
            .SetCapsuleDimensions(radius, height, height / 2f);
    }

    private Bounds ComputeBounds(GameObject propCopy)
    {
        Bounds bounds = new(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        MeshRenderer[] renderers = propCopy.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            Bounds worldBounds = renderer.bounds;
            Vector3 localCenter = propCopy.transform.InverseTransformPoint(worldBounds.center);
            Vector3 localSize = Vector3.Scale(worldBounds.size, renderer.transform.lossyScale);
            Bounds localBounds = new(localCenter, localSize);

            if (!boundsInitialized)
            {
                bounds = localBounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(localBounds);
            }
        }

        return bounds;
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

        audioSource.clip = shootingNoise;
        audioSource.Play();

    }

    private void HighlightProps()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, highlightRaycastDistance, ~playerLayer))
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
    
    public void PlayRandomSound()
    {
        int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
        AudioClip randomClip = audioClips[randomIndex];
        audioSource.clip = randomClip;
        audioSource.Play();
    }
}
