using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraDoorScript
{
    public class CameraOpenDoor : MonoBehaviour
    {
        public float DistanceOpen = 3;
        public GameObject text;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            RaycastHit hit;
            //Debug.DrawRay(transform.position, transform.forward, Color.green);
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen))
                {
                    if (hit.transform.GetComponent<DoorScript.Door>())
                    {
                        //text.SetActive (true);
                        hit.transform.GetComponent<DoorScript.Door>().OpenDoor();
                    }
                    else
                    {
                        //text.SetActive (false);
                    }
                }
                else
                {
                    //text.SetActive (false);
                }
            }
        }
    }
}
