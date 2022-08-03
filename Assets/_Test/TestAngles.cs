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
        Vector3 v = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Debug.Log("V:" + v);
    }
}
