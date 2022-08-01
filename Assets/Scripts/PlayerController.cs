using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bob
{
    public class PlayerController : MonoBehaviour
    {
        #region private methods

        Vector3 targetVelocity;
        Rigidbody rb;
        BoxCollider coll;

        bool isGrounded; // Cached value
        #endregion

        #region native methods
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            coll = GetComponent<BoxCollider>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // Cache the isGrounded value
            isGrounded = IsGrounded();

            
            // Drag => v = v * ( 1 - drag * dt )

        }

        private void FixedUpdate()
        {
           
            Debug.Log("Velocity:" + rb.velocity.magnitude);    
        }

        #endregion

        #region public methods
       
        /// <summary>
        /// Returns true if the bob hits the ground, otherwise false
        /// </summary>
        /// <returns></returns>
        public bool IsGrounded()
        {
            // Overlap the box collider
            Vector3 center = transform.position + coll.center;
            Vector3 halfExtents = new Vector3(coll.size.x / 2f, coll.size.y / 2f, coll.size.z / 2f);
            LayerMask mask = LayerMask.GetMask(new string[] { Layers.Ground });
            Collider[] ret = Physics.OverlapBox(center, halfExtents, transform.rotation, mask);
            if (ret != null && ret.Length > 0)
                return true;
            else
                return false;
        }
        #endregion
    }

}
