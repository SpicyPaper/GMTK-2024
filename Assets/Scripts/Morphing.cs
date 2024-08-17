using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Morphing : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask propLayerMask;
    [SerializeField] private GameObject initialGameObject;
    [SerializeField] private GameObject meshParent;
    [SerializeField] private float playerHeight = 2f;

    private bool isMorphed = false;
    private GameObject newInst;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isMorphed)
            {
                float raycastDistance = 5f;
                Vector3 capsuleStart = playerTransform.position;
                Vector3 capsuleEnd = playerTransform.position + Vector3.up * (playerHeight - 0.5f);

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, 0.5f, playerTransform.forward,
                    out RaycastHit hit, raycastDistance, propLayerMask))
                {
                    if (hit.collider.CompareTag("Prop"))
                    {
                        ChangeAppearanceAndTransform(hit.collider.gameObject);
                        isMorphed = true;
                    }
                }
            }
            else
            {
                ChangeAppearanceAndUntransform(this.gameObject);
                isMorphed = false;
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
        playerTransform.GetComponent<KinematicCharacterMotor>()
            .SetCapsuleDimensions(newCollider.size.y/4, newCollider.size.y, newCollider.size.y / 4);
        Debug.Log(newCollider.size);

        newInst.transform.SetParent(meshParent.transform);
        newInst.transform.localPosition =
            new Vector3(0, -newCollider.center.y/2, 0);
        newInst.transform.localScale = propObject.transform.localScale;
    }

    private void ChangeAppearanceAndUntransform(GameObject propObject)
    {
        ChangeRendererVisibility(meshParent, true);
        meshParent.GetComponent<KinematicCharacterMotor>()
            .SetCapsuleDimensions(0.5f, 2, 1);

        Destroy(newInst);
    }

    private void DisableColliders(GameObject target)
    {
        // Get all colliders in the GameObject and its children
        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        // Loop through each collider and disable it
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }

    private void ChangeRendererVisibility(GameObject target, bool enabled)
    {
        // Get all colliders in the GameObject and its children
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        // Loop through each collider and disable it
        foreach (Renderer col in renderers)
        {
            col.enabled = enabled;
        }
    }
}
