using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aimAndShoot : MonoBehaviour
{
    public float rayDistance = 1000f; // La distance maximale du raycast
    public Camera playerCamera; // La caméra du joueur
    public LineRenderer lineRenderer; // Le composant LineRenderer pour dessiner le rayon
    public float rayDuration = 0.5f; // La durée pendant laquelle le rayon reste visible

    void Update()
    {
        //AimWithMouse();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Shoot();
        }
    }

    void AimWithMouse()
    {
        // Obtenir la position de la souris en 2D
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = playerCamera.nearClipPlane; // Distance entre la caméra et le plan de vision
        Vector3 worldPosition = playerCamera.ScreenToWorldPoint(mousePosition);

        // Calculer la direction de la souris par rapport au joueur
        Vector3 direction = (worldPosition - transform.position).normalized;

        // Faire tourner l'objet pour qu'il pointe vers la souris
        transform.forward = direction;
    }

    void Shoot()
    {
        // Obtenir la position d'origine du raycast (au niveau du joueur)
        Vector3 rayOrigin = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

        // Obtenir la direction du tir en fonction de la position de la souris sur l'écran
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 rayDirection = new Vector3(ray.direction.x * -1,
            ray.direction.y * 1, ray.direction.z * -1).normalized;

        // Debugging pour voir la direction du ray
        Debug.Log("Direction du ray: " + rayDirection);

        // Lancer le raycast
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            // Dessiner le rayon avec LineRenderer
            StartCoroutine(DrawRay(rayOrigin, hit.point));

            // Vérifier le tag de l'objet touché
            if (hit.collider.CompareTag("Prop"))
            {
                Debug.Log("Aïe raté");
            }
            else if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Touché");
            }
        }
        else
        {
            // Si le raycast ne touche rien, dessiner le rayon à pleine distance
            StartCoroutine(DrawRay(rayOrigin, rayOrigin + rayDirection * rayDistance));
        }
    }


    IEnumerator DrawRay(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(rayDuration);
        lineRenderer.enabled = false;
    }
}
