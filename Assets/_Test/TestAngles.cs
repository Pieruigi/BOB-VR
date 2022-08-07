using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAngles : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 eulers = transform.eulerAngles;
        if(Input.GetKey(KeyCode.X))
        {
            eulers.x += 15 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            eulers.x -= 15 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            eulers.z += 15 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            eulers.z -= 15 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Y))
        {
            eulers.y += 15 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.H))
        {
            eulers.y -= 15 * Time.deltaTime;
        }
        transform.eulerAngles = eulers;
    }
}
