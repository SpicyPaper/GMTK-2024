using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Morphing : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask propLayerMask;
    [SerializeField] private GameObject initialGameObject;
    [SerializeField] private float playerHeight = 2f;

    private bool isMorphed = false;

    private Renderer initialRenderer;
    private Collider initialCollider;
    private Vector3 localScale;

    // Start is called before the first frame update
    void Start()
    {
        initialRenderer = initialGameObject.GetComponent<Renderer>();
        initialCollider = initialGameObject.GetComponent<Collider>();
        localScale = initialGameObject.transform.localScale;

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

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, 0.5f, playerTransform.forward, out RaycastHit hit, raycastDistance, propLayerMask))
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
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        GameObject ninst = Instantiate(propObject);

        ninst.transform.SetParent(transform);
        ninst.transform.localPosition = Vector3.zero;
        ninst.transform.localScale = propObject.transform.localScale;
    }

    private void ChangeAppearanceAndUntransform(GameObject propObject)
    {

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
