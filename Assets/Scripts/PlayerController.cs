using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bob
{
    public class PlayerController : MonoBehaviour
    {
        #region properties
        public Vector3 TargetVelocity
        {
            get { return targetVelocity; }
        }
        #endregion

        #region private methods

        Vector3 targetVelocity;
        CharacterController cc;
        
        
        bool isGrounded; // Cached value
        float ySpeed = 0; // The vertical speed applied when not grounded

        [SerializeField]
        float drag = 0.5f;

        [SerializeField]
        float directionChangeSpeed = 5;

        [SerializeField]
        float rotationSpeed = 80;

        /// <summary>
        /// X: lateral friction
        /// Y: not used
        /// Z: frontal friction
        /// </summary>
        [SerializeField]
        Vector3 friction;

        [SerializeField]
        float brakeForceMax = .5f;

        float leftBrake, rightBrake = 0; // 0: released; 1: maximum braking force

        [SerializeField]
        Transform head;

        [SerializeField]
        Transform seat;
        
        #endregion

        #region native methods
        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // isGrounded value is cached 
            isGrounded = IsGrounded();

            //
            // Check input
            //
            CheckInput();
                       

            //
            // Move 
            //
            Move();

            //
            // Align to ground
            //
            AlignToGround();

            //
            // Roll ( when grounded )
            //
            Roll();
            
        }


        #endregion

        #region public methods
       
        /// <summary>
        /// Returns true if the bob hits the ground, otherwise false
        /// </summary>
        /// <returns></returns>
        public bool IsGrounded()
        {
            Vector3 point0 = transform.position + cc.center + Vector3.up * ( cc.height / 2f - cc.radius);
            Vector3 point1 = transform.position + cc.center + Vector3.up * (cc.radius - cc.height / 2f);
            LayerMask mask = LayerMask.GetMask(new string[] { Layers.Ground });
            Collider[] colls = Physics.OverlapCapsule(point0, point1, cc.radius + cc.skinWidth, mask);
            if (colls == null || colls.Length == 0)
                return false;
            else 
                return true;
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

        void Move()
        {
            if (isGrounded)
            {
                ySpeed = 0;

                // Get the ground plane ( given by its normal )
                Vector3 groundNormal = GetGroundNormal(); // The ground normal 

                // Project the bob fwd and rgt axis on the ground plane
                Vector3 fwdOnGround = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
                Vector3 rgtOnGround = Vector3.ProjectOnPlane(transform.right, groundNormal).normalized;

                // Get the acceleration along the bob forward axis: this acceleration depends on the 
                // gravity projected on the slope
                Vector3 fAcc = Vector3.Project(Physics.gravity, fwdOnGround);
                // The computed right acceleration should be zero unless we want the bob to slightly
                // slip on the strongest slopes
                Vector3 rAcc = Vector3.zero; // Vector3.Project(Physics.gravity, rgtOnGround);

                // Now we adjust the target velocity depending on the direction we are moving along the slope.
                // We adjust the direction first: the new direction is given by the bob fwd projected on 
                // the ground
                // Adjust the target velocity after jumping
                targetVelocity = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
                Vector3 newDirection = Vector3.MoveTowards(targetVelocity.normalized, fwdOnGround, directionChangeSpeed * Time.deltaTime);
                // Now we adjust the speed which slightly move to zero 
                float decelFactor = Vector3.Dot(targetVelocity.normalized, transform.right) * friction.x;
                // Adding braking factor
                decelFactor += (leftBrake + rightBrake) * brakeForceMax;
                decelFactor = Mathf.Abs(decelFactor);
                // Compute new magnitude
                float newMagnitude = Mathf.MoveTowards(targetVelocity.magnitude, 0, decelFactor * Time.deltaTime);
                // Adjust the target velocity
                targetVelocity = newMagnitude * newDirection;

                Debug.Log("Ground.Normal:" + groundNormal);
                Debug.Log("fAcc:" + fAcc);
                // Apply acceleration
                targetVelocity += (fAcc + rAcc) * Time.deltaTime;

            }
            else
            {
                // Fall speed
                ySpeed += Physics.gravity.y * Time.deltaTime;

            }
            // Drag => v = v * ( 1 - drag * dt )
            float dragDelta = 1 - drag * Time.deltaTime;
            targetVelocity *= dragDelta;
            ySpeed *= dragDelta;

            cc.Move(targetVelocity * Time.deltaTime + Vector3.up * ySpeed * Time.deltaTime);
        }

        void Roll()
        {
            if (!isGrounded)
                return;
        }

        void AlignToGround()
        {
            if (!isGrounded)
                return;

            Vector3 groundNormal = GetGroundNormal();
            
            float pitchAngle = Vector3.SignedAngle(transform.up, groundNormal, transform.right);
            transform.Rotate(Vector3.right, pitchAngle, Space.Self);
            float rollAngle = Vector3.SignedAngle(transform.up, groundNormal, transform.forward);
            transform.Rotate(Vector3.forward, rollAngle, Space.Self);
        }

        void CheckInput()
        {
#if UNITY_EDITOR
            /******************************* ONLY FOR TEST ******************************/
            if (isGrounded)
            {
                // Test input check

                float lBrake = 0;
                float rBrake = 0;
                leftBrake = rightBrake = 0;
                if (Input.GetKey(KeyCode.A))
                    lBrake = 1;
                if (Input.GetKey(KeyCode.D))
                    rBrake = 1;

                if (lBrake != 0)
                    leftBrake = 1;

                if (rBrake != 0)
                    rightBrake = 1;

                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * (rBrake - lBrake));
            }
#endif
        }
        #endregion
    }

}
