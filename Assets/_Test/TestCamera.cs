using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bob.Test
{
    public class TestCamera : MonoBehaviour
    {
        [SerializeField]
        Transform target;

        [SerializeField]
        Vector3 offset;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void LateUpdate()
        {
            transform.position = target.position + offset;

            transform.LookAt(target);
        }
    }

}
