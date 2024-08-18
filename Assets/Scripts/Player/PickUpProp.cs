using UnityEngine;

public class PickUpProp : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private float playerHeight = 2f;

    private Prop currentlyHeldObject;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if (currentlyHeldObject == null)
            {
                float pickUpDistance = 5f;

                Vector3 capsuleStart = playerTransform.position;
                Vector3 capsuleEnd = playerTransform.position + Vector3.up * (playerHeight - 0.5f);

                Debug.DrawRay(playerTransform.position, playerTransform.forward * pickUpDistance,
                    Color.red, 1f);

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, 0.5f, playerTransform.forward,
                    out RaycastHit raycastHit, pickUpDistance))
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
    }
}
