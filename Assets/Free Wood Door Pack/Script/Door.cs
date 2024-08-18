using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]
    public class Door : NetworkBehaviour
    {
        public NetworkVariable<bool> Open = new NetworkVariable<bool>(false);

        public float smooth = 1.0f;
        float DoorOpenAngle = -90.0f;
        float DoorCloseAngle = 0.0f;
        public AudioSource asource;
        public AudioClip openDoor, closeDoor;

        // Use this for initialization
        void Start()
        {
            asource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Open.Value)
            {
                var target = Quaternion.Euler(0, DoorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);
            }
            else
            {
                var target1 = Quaternion.Euler(0, DoorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);
            }
        }

        public void OpenDoor()
        {
            Interact();
        }

        private void DoOpen()
        {
            asource.clip = Open.Value ? openDoor : closeDoor;
            asource.Play();
        }

        // Method to interact with the door (called by the player)
        public void Interact()
        {
            ToggleDoorStateServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void ToggleDoorStateServerRpc()
        {
            Open.Value = !Open.Value;

            // Synchronize the door's state with all clients
            ToggleDoorStateClientRpc();
        }

        [ClientRpc]
        private void ToggleDoorStateClientRpc()
        {
            DoOpen();
        }
    }
}