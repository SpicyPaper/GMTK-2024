using UnityEngine;

public class Prop : MonoBehaviour
{
    public enum SizeCategory { UNDEFINED, XS, S, M, L, XL, XXL }

    private Transform cameraTransform;
    private Transform rootPlayerTransform;
    private Rigidbody objectRigidbody;
    private Vector3 effectivePickupDistance;
    private Quaternion initRot;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        objectRigidbody = gameObject.GetComponent<Rigidbody>();
        if (!objectRigidbody)
        {
            objectRigidbody = gameObject.AddComponent<Rigidbody>();
        }
        objectRigidbody.useGravity = true;
    }

    public void Grab(Transform rootPlayerTransform)
    {
        this.rootPlayerTransform = rootPlayerTransform;
        initRot = rootPlayerTransform.localRotation;

        objectRigidbody.useGravity = false;
        objectRigidbody.velocity = Vector3.zero;

        objectRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        effectivePickupDistance = objectRigidbody.position -
            rootPlayerTransform.position;
    }

    public void Drop()
    {
        rootPlayerTransform = null;
        objectRigidbody.useGravity = true;

        objectRigidbody.constraints = RigidbodyConstraints.None;
    }

    private void FixedUpdate()
    {
        if (rootPlayerTransform != null)
        {
            // Calculate the target position for the object
            Vector3 targetPos = rootPlayerTransform.position +
                cameraTransform.forward * effectivePickupDistance.magnitude + Vector3.up * 2;

            // Maintain the object's initial rotation relative to the player
            Quaternion relativeRotation = rootPlayerTransform.rotation * Quaternion.Inverse(initRot);
            objectRigidbody.transform.rotation = relativeRotation * initRot;

            // Stop any existing velocity
            objectRigidbody.velocity = Vector3.zero;

            // Move the object towards the target position
            if ((transform.position - targetPos).magnitude < 0.2f)
            {
                transform.position = targetPos;
            }
            else
            {
                Vector3 directionToGrabPoint = targetPos - transform.position;

                float moveSpeed = 50000f;
                objectRigidbody.AddForce(moveSpeed * directionToGrabPoint * Time.deltaTime);
            }

            // Drop the object if it exceeds a certain distance from the player
            if (Vector3.Distance(objectRigidbody.position, rootPlayerTransform.position) >
                effectivePickupDistance.magnitude + 2f)
            {
                Drop();
            }
        }
    }

    public float GetSize()
    {
        if (gameObject.TryGetComponent<Renderer>(out var renderer))
        {
            Vector3 boundsSize = renderer.bounds.size;
            return boundsSize.x * boundsSize.y * boundsSize.z;
        }

        return 0;
    }

    public SizeCategory GetSizeCategory()
    {
        float size = GetSize();
        if (size > 0)
        {
            return GetCategoryFromSize(size);
        }

        return SizeCategory.UNDEFINED;
    }

    private SizeCategory GetCategoryFromSize(float size)
    {
        if (size <= 0.125f) return SizeCategory.XS;
        else if (size <= 1f) return SizeCategory.S;
        else if (size <= 8f) return SizeCategory.M;
        else if (size <= 27f) return SizeCategory.L;
        else if (size <= 64f) return SizeCategory.XL;
        else return SizeCategory.XXL;
    }
}
