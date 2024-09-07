using KinematicCharacterController.Examples;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckType : NetworkBehaviour
{
    [SerializeField] ExampleCharacterCamera CharacterCamera;
    [SerializeField] List<Renderer> capsuleRend;
    [SerializeField] Material hunterMat;
    [SerializeField] Material morphMat;

    private GameManager gameManager;

    private NetworkVariable<Type> CurrentType = new NetworkVariable<Type>(Type.Morph,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public enum Type
    {
        Hunter,
        Morph
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        UpdateCharacter();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (CurrentType.Value != gameManager.type)
            {
                ChangeCharacterServerRpc(gameManager.type);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HomePageUI.Instance.ChangeType();
            }
        }
    }

    private void UpdateCharacter()
    {
        switch (CurrentType.Value)
        {
            case Type.Hunter:
                CharacterCamera.TargetDistance = 0f;
                for (int i = 0; i < capsuleRend.Count; i++)
                {
                    capsuleRend[i].material = hunterMat;
                }
                break;
            case Type.Morph:
                CharacterCamera.TargetDistance = CharacterCamera.DefaultDistance;
                for (int i = 0; i < capsuleRend.Count; i++)
                {
                    capsuleRend[i].material = morphMat;
                }
                break;
            default:
                Debug.Log("oups no tag");
                break;
        }
    }

    [ServerRpc]
    private void ChangeCharacterServerRpc(Type type)
    {
        CurrentType.Value = type;
        ChangeCharacterClientRpc();
    }

    [ClientRpc]
    private void ChangeCharacterClientRpc()
    {
        UpdateCharacter();
    }
}
