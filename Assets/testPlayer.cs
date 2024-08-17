using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class testPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (!IsOwner) return;

        Vector3 moveDir = new Vector3(0, 0, 0);


        if (Input.GetKey(KeyCode.W)) moveDir.z += +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z += -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x += -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += +1f;

        float moveSpeed = 3f;

        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
