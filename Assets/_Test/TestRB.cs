using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRB : MonoBehaviour
{
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Debug.Log("Speed:" + rb.velocity.magnitude);
    }
}
