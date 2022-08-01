using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bob
{
    public class PlayerControllerNOTUSED : MonoBehaviour
    {
        #region private methods

        Vector3 targetVelocity;
        Rigidbody rb;
        BoxCollider coll;

        
        bool isGrounded; // Cached value
        float ySpeed = 0; // The vertical speed applied when not grounded

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
                ySpeed = 0;

                // To compute the velocity we need to project the gravity along the slope
                Vector3 groundNormal = GetGroundNormal(); // The ground normal 

                // Project all the vectors we need on the ground plane
                Vector3 fwdOnGround = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
                Vector3 rgtOnGround = Vector3.ProjectOnPlane(transform.right, groundNormal).normalized;

                // We split the gravity acceleration into two different components along the fwd and right axis
                Vector3 fAcc = Vector3.Project(Physics.gravity, fwdOnGround);
                Vector3 rAcc = Vector3.Project(Physics.gravity, rgtOnGround);
                //fAcc = rAcc = Vector3.zero;

                Debug.Log("Ground.Normal:" + groundNormal);
                Debug.Log("fAcc:" + fAcc);

                targetVelocity += (fAcc + rAcc) * Time.deltaTime;
                
            }
            else
            {
                // Fall down
                ySpeed += Physics.gravity.y * Time.deltaTime;

            }
            // Drag => v = v * ( 1 - drag * dt )
            


        }

        private void FixedUpdate()
        {


            // ySpeed is only applied if the bob is not grounded
            rb.velocity = targetVelocity + Vector3.up * ySpeed;
            
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
