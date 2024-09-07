using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Unity.Netcode;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : NetworkBehaviour
    {
        public ExampleCharacterController Character;
        public ExampleCharacterCamera CharacterCamera;
        public List<MonoBehaviour> gameOjs;
        public AudioListener audioListener;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        public bool isLocked = false;
        private Vector3 initialPosition;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
            
            if (IsOwner)
            {
                GameManager.Instance.SetPlayerCamera(CharacterCamera.Camera);
                if (!IsHost)
                {
                    HomePageUI.Instance.ChooseType();
                }
            }
            else
            {
                foreach (MonoBehaviour item in gameOjs)
                {
                    item.enabled = false;
                }
                audioListener.enabled = false;
            }

            initialPosition = transform.position; // Store the initial position to avoid drift
        }

        private void Update()
        {
            if (!IsOwner)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                //Cursor.lockState = CursorLockMode.Locked;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                Lock();
            }

            if (!isLocked)
            { 
                HandleCharacterInput();
            }
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
            }

            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
            float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                // lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            //float scrollInput = -Input.GetAxis(MouseScrollInput);
            float scrollInput = 0f;
#if UNITY_WEBGL
        scrollInput = 0f;
#endif

            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector);

            // Handle toggling zoom level
            //if (Input.GetMouseButtonDown(1))
            //{
            //    CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            //}
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
            characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
            characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
            characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }

        public void Lock()
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            if (isLocked)
            {
                rb.constraints = RigidbodyConstraints.None;

                CharacterController characterController = GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = true;
                }

                isLocked = false;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;

                transform.position = initialPosition;

                // TODO the player keeps its speed and we don wan dat

                CharacterController characterController = GetComponent<CharacterController>();
                if (characterController != null)
                {
                    characterController.enabled = false;
                }

                // Set the character's position and make it visible again
                var ecc = gameObject.GetComponent<ExampleCharacterController>();
                var state = ecc.Motor.GetState();
                state.BaseVelocity = Vector3.zero;
                state.AttachedRigidbodyVelocity = Vector3.zero;
                ecc.Motor.ApplyState(state);

                isLocked = true;
            }
        }
    }
}