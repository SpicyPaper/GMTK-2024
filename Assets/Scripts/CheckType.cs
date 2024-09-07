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
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootingNoise;

    public NetworkVariable<Type> CurrentType = new(Type.Morph);

    public enum Type
    {
        Hunter,
        Morph
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentType.OnValueChanged += OnTypeChanged;
        UpdateCharacter();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                HomePageUI.Instance.ChangeType();
            }
        }
    }
    private void OnDestroy()
    {
        // Unsubscribe to avoid potential memory leaks
        CurrentType.OnValueChanged -= OnTypeChanged;
    }

    private void UpdateCharacter()
    {
        Debug.Log(IsOwner);
        Debug.Log(CurrentType.Value);
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

    private void OnTypeChanged(Type oldValue, Type newValue)
    {
        UpdateCharacter();
    }

    [ServerRpc]
    private void ChangeCharacterServerRpc(Type type)
    {
        CurrentType.Value = type;
        //ChangeCharacterClientRpc();
    }

    //[ClientRpc]
    //private void ChangeCharacterClientRpc()
    //{
    //    UpdateCharacter();
    //}

    public void ChangeType(Type type)
    {
        if (IsOwner)
        {
            ChangeCharacterServerRpc(type);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SoundShootServerRpc()
    {
        SoundShootClientRpc();
    }

    [ClientRpc]
    public void SoundShootClientRpc()
    {
        audioSource.clip = shootingNoise;
        audioSource.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SoundRandomSoundServerRpc(int randomIndex)
    {
        SoundRandomSoundClientRpc(randomIndex);
    }

    [ClientRpc]
    public void SoundRandomSoundClientRpc(int randomIndex)
    {
        audioSource.clip = GetComponent<Player>().audioClips[randomIndex];
        audioSource.Play();
    }
}
