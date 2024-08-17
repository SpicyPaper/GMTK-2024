using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour
{
    private Transform objectGrabPointTransform;
    private Rigidbody objectRigidbody;
    private float distanceBeforeDrop = 5f;
    private float effectivePickupDistance;

    private void Awake()
    {
        objectRigidbody = GetComponent<Rigidbody>();
        objectRigidbody.useGravity = true;
    }

    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;

        objectRigidbody.useGravity = false;
        objectRigidbody.velocity = Vector3.zero;

        objectRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        effectivePickupDistance = Vector3.Distance(objectRigidbody.position, objectGrabPointTransform.position);
    }

    public void Drop()
    {
        objectGrabPointTransform = null;
        objectRigidbody.useGravity = true;

        objectRigidbody.constraints = RigidbodyConstraints.None;
    }

    private void FixedUpdate()
    {
        // TODO Do something with the effectivePickupDistance to keep the distance with which the object has been picked up and keep the distance between object and grab point as this distance
        if (objectGrabPointTransform != null)
        {
            Vector3 directionToGrabPoint = (objectGrabPointTransform.position - transform.position).normalized;

            float moveSpeed = 5000f;
            objectRigidbody.AddForce(directionToGrabPoint * moveSpeed * Time.deltaTime);

            float maxSpeed = 20f;
            if (objectRigidbody.velocity.magnitude > maxSpeed)
            {
                objectRigidbody.velocity = objectRigidbody.velocity.normalized * maxSpeed;
            }

            if (Vector3.Distance(objectRigidbody.position, objectGrabPointTransform.position) > distanceBeforeDrop)
            {
                Drop();
            }
        }
    }
}
