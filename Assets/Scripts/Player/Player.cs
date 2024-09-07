using KinematicCharacterController;
using System;
using System.Runtime.CompilerServices;
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
        if (GameManager.Instance.IsAlive)
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

            float mouseDelta = Input.mouseScrollDelta.y;
            if (mouseDelta != 0)
            {
                RotateProp(mouseDelta);
            }

            HighlightProps();
        }
    }

    private void Morph()
    {
        if (currentHighlightedObject)
        {
            // Destroy the previous transformation
            foreach (Transform child in meshParent.transform)
            {
                Destroy(child.gameObject);
            }

            // Disable Renderer visibility
            ChangeRendererVisibility(meshParent, false);

            RemoveHighlight();

            // Create a copy of the prop for the transformation
            propCopyObject = Instantiate(currentHighlightedObject);

            AddHighlight(currentHighlightedObject);

            DisableColliders(propCopyObject);

            if (propCopyObject.GetComponent<Rigidbody>())
            {
                propCopyObject.GetComponent<Rigidbody>().isKinematic = true;
            }

            propCopyObject.transform.SetParent(meshParent.transform);
            propCopyObject.transform.localScale = currentHighlightedObject.transform.localScale;
            propCopyObject.transform.SetLocalPositionAndRotation(
                new Vector3(0, 0, 0),
                Quaternion.Euler(Vector3.zero)
            );

            // Compute capsule propoerties for the character motor
            if (propCopyObject.TryGetComponent<Prop>(out var prop))
            {
                Bounds computedBounds = prop.ComputeBounds();

                float height = computedBounds.size.y;
                float radius = Mathf.Min(Mathf.Min(computedBounds.size.x, computedBounds.size.z) / 2f, height / 2f);

                transform.GetComponent<KinematicCharacterMotor>()
                    .SetCapsuleDimensions(radius, height, height / 2f);
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
        if (currentHighlightedObject && currentlyHeldObject == null)
        {
            if (currentHighlightedObject.TryGetComponent<Prop>(out var prop))
            {
                prop.Grab(objectGrabPoint);
                currentlyHeldObject = prop;
            }
        } 
        else if (currentlyHeldObject != null)
        {
            currentlyHeldObject.Drop();
            currentlyHeldObject = null;
        }
    }

    private void RotateProp(float mouseDelta)
    {
        if (currentlyHeldObject != null && currentlyHeldObject.TryGetComponent<Prop>(out var prop))
        {
            prop.Rotate(mouseDelta);
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
        Vector3 rayOrigin = new(transform.position.x, transform.position.y + 1.43f, transform.position.z);

        if (Physics.Raycast(rayOrigin, ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject.GetComponent<Prop>())
            {
                GetComponent<PlayerInteraction>().TakeDamageServerRpc(20);
            }
            else if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerInteraction>().TakeDamageServerRpc(20);
            }
        }

        Debug.DrawRay(rayOrigin, ray * shootRaycastDistance, Color.red, 2f);

        GetComponent<CheckType>().SoundShootServerRpc();

    }

    private void HighlightProps()
    {
        GameObject target = null;

        // Check for props in overlaping sphere
        Collider[] overlapHits = Physics.OverlapSphere(transform.position, 2f, ~playerLayer);
        foreach (Collider overlapHit in overlapHits)
        {
            GameObject hitObject = GetParentWithProp(overlapHit.transform.gameObject);
            if (hitObject && hitObject.TryGetComponent<Prop>(out _))
            {
                target = hitObject;
            }
        }

        if (target == null)
        {
            if (Physics.SphereCast(transform.position, 2f, transform.forward, out RaycastHit hit2, highlightRaycastDistance, ~playerLayer))
            {
                GameObject hitObject = GetParentWithProp(hit2.transform.gameObject);
                if (hitObject && hitObject.TryGetComponent<Prop>(out _))
                {
                    target = hitObject;
                }
            }
        }

        if (target && target.TryGetComponent<Prop>(out var prop))
        {
            if (target != currentHighlightedObject)
            {
                RemoveHighlight();
                AddHighlight(target);
                currentHighlightedObject = target;
                propGrabedText.text = target.name + " [ Size: " + prop.GetSizeCategory() + "(" + prop.GetSize() + ") ]";
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
            if (currentHighlightedObject.TryGetComponent<Outline>(out var outline))
            {
                DestroyImmediate(outline);
            }
        }
    }

    public void PlayRandomSound()
    {
        int randomIndex = UnityEngine.Random.Range(0, audioClips.Length);
        AudioClip randomClip = audioClips[randomIndex];
        audioSource.clip = randomClip;
        audioSource.Play();

        GetComponent<CheckType>().SoundRandomSound(randomClip);
    }
}
