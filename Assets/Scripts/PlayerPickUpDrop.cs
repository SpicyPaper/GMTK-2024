using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUpDrop : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private LayerMask pickUpLayerMask;
    [SerializeField] private float playerHeight = 2f;


    private ObjectGrabbable currentlyHeldObject;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (currentlyHeldObject == null)
            {
                float pickUpDistance = 5f;

                Vector3 capsuleStart = playerTransform.position;
                Vector3 capsuleEnd = playerTransform.position + Vector3.up * (playerHeight - 0.5f);

                Debug.DrawRay(playerTransform.position, playerTransform.forward * pickUpDistance, Color.red, 1f);

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, 0.5f, playerTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask))
                {                    
                    if (raycastHit.transform.TryGetComponent(out ObjectGrabbable objectGrababble))
                    {
                        objectGrababble.Grab(objectGrabPoint);
                        currentlyHeldObject = objectGrababble;
                    }
                }
            }
            else
            {
                currentlyHeldObject.Drop();
                currentlyHeldObject = null;
            }
        }
    }
}
