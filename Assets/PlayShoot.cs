using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayShoot : NetworkBehaviour
{
    public Camera cam;  // Assign your camera in the Inspector
    public float maxDistance = 100f;  // Maximum distance for the raycast
    public LayerMask hitLayers;  // Layers to be hit by the raycast

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        // Check if the 'Q' key is pressed
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Ensure the camera is assigned
            if (cam == null)
            {
                Debug.LogError("Camera not assigned in CameraRaycast script.");
                return;
            }

            // Create a ray from the camera's center
            Vector3 ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)).direction;

            Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 1.43f,
                transform.position.z);

            // Variable to store information about what the raycast hit
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(rayOrigin, ray, out hit, 100f))
            {
                // Log the name of the object hit by the raycast
                Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

                // You can add additional logic here, such as interacting with the hit object
                if (hit.collider.CompareTag("Prop"))
                {
                    Debug.Log("Aïe raté");
                }
                else if (hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerInteraction>().TakeDamageServerRpc(20);
                }
            }
            else
            {
                // No hit, you can handle it here if needed
                Debug.Log("Raycast did not hit anything.");
            }

            // Optional: Draw the ray in the scene view for debugging purposes
            Debug.DrawRay(rayOrigin, ray * maxDistance, Color.red, 2f);
        }
    }
}
