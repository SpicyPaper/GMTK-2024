using Unity.Netcode;
using UnityEngine;

public class Prop : NetworkBehaviour
{
    public enum SizeCategory { UNDEFINED, XS, S, M, L, XL, XXL }

    private Transform cameraTransform;
    private Transform rootPlayerTransform;
    private Rigidbody objectRigidbody;
    private float effectivePickupDistance;
    private Quaternion initRot;

    private float rotationSpeed = 1000f;
    private float rotationAngle = 0f;

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
        ChangeOwnerServerRpc();
        this.rootPlayerTransform = rootPlayerTransform;
        initRot = rootPlayerTransform.localRotation;

        objectRigidbody.useGravity = false;
        objectRigidbody.velocity = Vector3.zero;

        objectRigidbody.constraints = RigidbodyConstraints.FreezeRotation;

        effectivePickupDistance = (objectRigidbody.position - rootPlayerTransform.position).magnitude;
    }

    public void Drop()
    {
        rootPlayerTransform = null;
        objectRigidbody.useGravity = true;

        objectRigidbody.constraints = RigidbodyConstraints.None;
    }

    public void Rotate(float mouseDelta)
    {
        rotationAngle += mouseDelta * rotationSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (rootPlayerTransform != null)
        {
            // Calculate the target position for the object
            Vector3 targetPos = rootPlayerTransform.position +
                rootPlayerTransform.forward * effectivePickupDistance + Vector3.up * 0.1f;

            // Maintain the object's initial rotation relative to the player
            Quaternion relativeRotation = rootPlayerTransform.rotation * Quaternion.Inverse(initRot);
            Quaternion offsetRotation = Quaternion.Euler(0, rotationAngle, 0);
            objectRigidbody.transform.rotation = relativeRotation * offsetRotation * initRot;

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
                effectivePickupDistance + 2f)
            {
                Drop();
            }
        }
    }

    public float GetSize()
    {
        Vector3 bounds = ComputeBounds().size;
        return bounds.x * bounds.y * bounds.z;
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

    public Bounds ComputeBounds()
    {
        Bounds bounds = new(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;

        MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in renderers)
        {
            Bounds worldBounds = renderer.bounds;
            Vector3 localCenter = gameObject.transform.InverseTransformPoint(worldBounds.center);
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

    [ServerRpc(RequireOwnership = false)]
    public void ChangeOwnerServerRpc()
    {
        GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
    }
}
