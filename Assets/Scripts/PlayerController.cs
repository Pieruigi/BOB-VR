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

        // Movement fields
        /// <summary>
        /// Velocity applied along bob local axis
        /// X: along right axis
        /// Y: along up axis ( not used )
        /// Z: along forward axis
        /// </summary>
        Vector3 localVelocity; // The bob velocity computed along its right and forward axis

        /// <summary>
        /// X: lateral friction
        /// Y: not used
        /// Z: frontal friction
        /// </summary>
        [SerializeField]
        Vector3 friction;
        
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


            if (isGrounded)
            {
                // Compute velocity
                // We need the component of the gravity along the slope
                Vector3 groundNormal = GetGroundNormal();

            }
                        // Drag => v = v * ( 1 - drag * dt )


            // Apply friction to local velocity
            ApplyFriction();
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

        #region private methods
        void ApplyFriction()
        {
            // No friction if you are flying
            if (!isGrounded)
                return;

            // Frontal friction
            localVelocity.z -= friction.z * Time.deltaTime;
            // Lateral friction+
            localVelocity.x -= friction.x * Time.deltaTime;
        }

        Vector3 GetGroundNormal()
        {
            Vector3 ret = Vector3.zero;
            LayerMask mask = LayerMask.GetMask(new string[] { Layers.Ground });
            RaycastHit hitInfo;
            if(Physics.Raycast(transform.position, Vector3.down, out hitInfo, 10, mask))
            {
                ret = hitInfo.normal;
            }

            return ret;
        }
        #endregion
    }

}
