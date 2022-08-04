using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVectors : MonoBehaviour
{
    [SerializeField]
    Vector3 v;

    // Start is called before the first frame update
    void Start()
    {
        Log();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Log();
    }

    void Log()
    {
        Debug.Log("V:" + v);
        Debug.Log("TranformPoint:" + transform.TransformPoint(v));
        Debug.Log("TranformDirection:" + transform.TransformDirection(v));
        Debug.Log("InverseTransformPoint:" + transform.InverseTransformPoint(v));
        Debug.Log("InverseTransformDirection:" + transform.InverseTransformDirection(v));
    }
}
