using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickUpDrop : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private LayerMask pickUpLayerMask;


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
                Debug.Log("before pickup");
                float pickUpDistance = 5f;
                Debug.DrawRay(playerTransform.position, playerTransform.forward * pickUpDistance, Color.red, 1f);

                // TODO check if we need to move the rayasting so that le player could grap stuff at other height
                if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit raycastHit, pickUpDistance, pickUpLayerMask))
                {                    
                    Debug.Log("starting to pickup");
                    if (raycastHit.transform.TryGetComponent(out ObjectGrabbable objectGrababble))
                    {
                        Debug.Log(raycastHit.transform);
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
