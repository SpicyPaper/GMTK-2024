using KinematicCharacterController.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckType : MonoBehaviour
{
    [SerializeField] ExampleCharacterCamera CharacterCamera;
    private HomePageUI.Type currentType;
    private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        currentType = gameManager.type;

        UpdateCameraDistance();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentType != gameManager.type)
        {
            currentType = gameManager.type;
            UpdateCameraDistance();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HomePageUI.Instance.ChangeType();
        }
    }

    private void UpdateCameraDistance()
    {
        switch (currentType)
        {
            case HomePageUI.Type.Hunter:
                CharacterCamera.TargetDistance = 0f;
                break;
            case HomePageUI.Type.Morph:
                CharacterCamera.TargetDistance = CharacterCamera.DefaultDistance;
                break;
            default:
                Debug.Log("oups no tag");
                break;
        }
    }
}
