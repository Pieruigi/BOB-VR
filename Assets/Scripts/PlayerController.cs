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
        float overturnAngle = 0;
        float pitch, roll;
        
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

        [SerializeField]
        float brakeLength = 0.5f;

        [SerializeField]
        Transform leftBrakePivot;

        [SerializeField]
        Transform rightBrakePivot;

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
            // Roll and pitch
            //
            RollAndPitch();

                
        }


        #endregion

        #region public methods
       
        /// <summary>
        /// Returns true if the bob hits the ground, otherwise false
        /// </summary>
        /// <returns></returns>
        public bool IsGrounded()
        {
            //Vector3 point0 = transform.position + cc.center + Vector3.up * ( cc.height / 2f - cc.radius);
            //Vector3 point1 = transform.position + cc.center + Vector3.up * (cc.radius - cc.height / 2f);
            //LayerMask mask = LayerMask.GetMask(new string[] { Layers.Ground });
            //Collider[] colls = Physics.OverlapCapsule(point0, point1, cc.radius + 2*cc.skinWidth, mask);
            //if (colls == null || colls.Length == 0)
            //    return false;
            //else 
            //    return true;

            Vector3 center = transform.position + cc.center;
            //Debug.Log("Center:" + center);
            LayerMask mask = LayerMask.GetMask(new string[] { Layers.Ground });
            Collider[] colls = Physics.OverlapSphere(center, cc.radius + 2*cc.skinWidth, mask);
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
                //
                // Apply brakes
                //
                // Raycast from both left and right brakes
                RaycastHit hitInfo;
                float leftBrakeRatio = 0;
                float rightBrakeRatio = 0;
                if(Physics.Raycast(new Ray(leftBrakePivot.position, -leftBrakePivot.forward), out hitInfo, brakeLength, LayerMask.GetMask(new string[] { Layers.Ground })))
                {
                    float dist = Vector3.Distance(leftBrakePivot.position, hitInfo.point);
                    dist = brakeLength - dist;
                    leftBrakeRatio = dist / brakeLength;
                }
                if (Physics.Raycast(new Ray(rightBrakePivot.position, -rightBrakePivot.forward), out hitInfo, brakeLength, LayerMask.GetMask(new string[] { Layers.Ground })))
                {
                    float dist = Vector3.Distance(rightBrakePivot.position, hitInfo.point);
                    dist = brakeLength - dist;
                    rightBrakeRatio = dist / brakeLength;
                }
                // Rotate accordingly to the braking force
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime * (rightBrakeRatio - leftBrakeRatio));

                // 
                // Move the bob
                //
                ySpeed = 0; // Reset fall speed when grounded

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
                //decelFactor += (leftBrake + rightBrake) * brakeForceMax;
                decelFactor += ( leftBrakeRatio + rightBrakeRatio ) * brakeForceMax;
                decelFactor = Mathf.Abs(decelFactor);
                // Compute new magnitude
                float newMagnitude = Mathf.MoveTowards(targetVelocity.magnitude, 0, decelFactor * Time.deltaTime);
                // Adjust the target velocity
                targetVelocity = newMagnitude * newDirection;

               
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



        void RollAndPitch()
        {


            if (!isGrounded)
            {
                // Use head to do move center of mass
            }
            else
            {

                //
                // Adjust rotation depending on the ground
                //
                Vector3 groundNormal = GetGroundNormal();
                float speed = 40;
                //Vector3 targetNormal = groundNormal;
                // Compute pitch angle
                pitch = Vector3.SignedAngle(transform.up, groundNormal, transform.right) + transform.eulerAngles.x;
                roll = Vector3.SignedAngle(transform.up, groundNormal, transform.forward) + transform.eulerAngles.z;
                pitch = Mathf.MoveTowardsAngle(transform.eulerAngles.x, pitch, speed * Time.deltaTime);
                roll = Mathf.MoveTowardsAngle(transform.eulerAngles.z, roll, speed * Time.deltaTime);

                // Compute roll angle

                //targetAngle = Quaternion.Euler(0, 0, 20) * targetAngle;
                Vector3 eulers = transform.eulerAngles;
                eulers.x = pitch;
                eulers.z = roll;
                //eulers.x = Mathf.MoveTowardsAngle(transform.eulerAngles.x, pitch, speed * Time.deltaTime);
                //eulers.z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, roll, speed * Time.deltaTime);
                //transform.eulerAngles = eulers;


                //
                // Check for overturn
                //
                //Vector3 centerOfMass = GetCenterOfMass() - transform.position; // From point to vector
                //centerOfMass = transform.InverseTransformDirection(centerOfMass); // Local coordinates
                Vector3 centerOfMass = transform.InverseTransformPoint(GetCenterOfMass());
                centerOfMass.z = 0; // We only need right coordinates
                //bool isToTheRight = centerOfMass.x > 0; // True if the center of mass falls to the right
                centerOfMass = transform.TransformPoint(centerOfMass);
                // Where the center of mass falls on the horizontal plane
                Vector3 comFall = Vector3.ProjectOnPlane(centerOfMass - transform.position, Vector3.up);
                //float halfSize = cc.radius;
                Vector3 rFall = Vector3.ProjectOnPlane(transform.right * cc.radius, Vector3.up);
                //Vector3 lRgt = Vector3.ProjectOnPlane(transform.left * cc.radius, Vector3.up);
                float massFactor = 1f;

                Debug.Log("ComFall:" + comFall);
                Debug.Log("ComFall.Magnitude:" + comFall.magnitude);
                Debug.Log("rFall:" + rFall);
                Debug.Log("rFall.Magnitude:" + rFall.magnitude);
                //Debug.Log("IsToTheRight:" + isToTheRight);
                Vector3 tmp = comFall - rFall;
                // Positive angle when the center of mass falls within the bob base, otherwise is negative
                float rRotForce = -massFactor * tmp.magnitude * Vector3.Dot(tmp.normalized, rFall.normalized);
                Debug.Log("rRotForce:" + rRotForce);
                //tmp = comFall + rFall;
                // Negative force when the center of mass falls within the bob base, otherwise is positive
                float lRotForce = rRotForce - massFactor;// -massFactor * tmp.magnitude * Vector3.Dot(tmp.normalized, rFall.normalized);
                //rRotForce *= massFactor;
                //lRotForce *= massFactor;
                Debug.Log("lRotForce:" + lRotForce);
                
                // Compute the reaction force of the center of mass: if the center of mass projectes along the
                // UP plane falls inside the bob then the force puts the bob down, otherwise the force applies to
                // overturn by itslef )
                // Project the center of mass on the horizontal plane


                //Debug.Log("CenterOfMass.ProjectOnUP:" + comOnHorizontalPlane);



                // Adjust the roll angle depending on the lateral speed
                Vector3 rightProj = Vector3.ProjectOnPlane(transform.right, groundNormal);
               
                Vector3 velProj = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
                float sign = -Vector3.Dot(velProj.normalized, rightProj.normalized);
                // Project the center of mass on the ground plane
                Vector3 com = Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up);
                
                com = Vector3.Project(com, rightProj);
                
                // Sign < 0 means we are moving our head against the velocity ( the slope in theory ) to avoid overturn
                float signCom = Vector3.Dot(velProj.normalized, com.normalized);
                //Debug.Log("signCom:" + signCom);

                // We must check the position of the head to determine the overturn angle
                float baseHalfSize = cc.radius;
                float headDist = com.magnitude; // The distance the head falls in the UP plane 
                float headDir = Mathf.Sign(signCom); // <0 means against the slope to avoid overturn
                
                // The threshold depends on the position of the head: the more the head is against the slope
                // the higher is the threshold
                if(headDir > 0 && headDist > baseHalfSize)
                //if (Mathf.Abs(sign) > (overturnDotThreshold - headDir * headDist / baseHalfSize * 0.1f))
                {
                    float overturnFactor = 20;
                    float targetAngle = overturnAngle + sign * overturnFactor;
                    overturnAngle = Mathf.MoveTowardsAngle(overturnAngle, targetAngle, 20 * Time.deltaTime);
                }
                else
                {
                    overturnAngle = Mathf.MoveTowardsAngle(overturnAngle, 0, 40 * Time.deltaTime);
                }
               
                //targetNormal = Quaternion.AngleAxis(overturnAngle, transform.forward) * groundNormal;

                

              

                
               
            }    

            
        }

        void RollAndPitch_bkp()
        {


            if (!isGrounded)
            {
                // Use head to do move center of mass
            }
            else
            {
                Vector3 groundNormal = GetGroundNormal();
                float speed = 40;
                Vector3 targetNormal = groundNormal;
                // Adjust the roll angle depending on the lateral speed
                Vector3 rightProj = Vector3.ProjectOnPlane(transform.right, groundNormal);

                Vector3 velProj = Vector3.ProjectOnPlane(targetVelocity, groundNormal);
                float sign = -Vector3.Dot(velProj.normalized, rightProj.normalized);
                // Project the center of mass on the ground plane
                Vector3 com = Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up);

                com = Vector3.Project(com, rightProj);

                // Sign < 0 means we are moving our head against the velocity ( the slope in theory ) to avoid overturn
                float signCom = Vector3.Dot(velProj.normalized, com.normalized);
                //Debug.Log("signCom:" + signCom);

                // We must check the position of the head to determine the overturn angle
                float baseHalfSize = cc.radius;
                float headDist = com.magnitude; // The distance the head falls in the UP plane 
                float headDir = Mathf.Sign(signCom); // <0 means against the slope to avoid overturn
                Debug.Log("HeadFallDist:" + headDist);
                // The threshold depends on the position of the head: the more the head is against the slope
                // the higher is the threshold
                if (headDir > 0 && headDist > baseHalfSize)
                //if (Mathf.Abs(sign) > (overturnDotThreshold - headDir * headDist / baseHalfSize * 0.1f))
                {
                    float overturnFactor = 20;
                    float targetAngle = overturnAngle + sign * overturnFactor;
                    overturnAngle = Mathf.MoveTowardsAngle(overturnAngle, targetAngle, 20 * Time.deltaTime);
                }
                else
                {
                    overturnAngle = Mathf.MoveTowardsAngle(overturnAngle, 0, 40 * Time.deltaTime);
                }

                targetNormal = Quaternion.AngleAxis(overturnAngle, transform.forward) * groundNormal;

                // Compute pitch angle
                float pitchAngle = Vector3.SignedAngle(transform.up, targetNormal, transform.right);
                pitchAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.x, transform.eulerAngles.x + pitchAngle, speed * Time.deltaTime);

                // Compute roll angle

                //targetAngle = Quaternion.Euler(0, 0, 20) * targetAngle;
                float rollAngle = Vector3.SignedAngle(transform.up, targetNormal, transform.forward);

                rollAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, transform.eulerAngles.z + rollAngle, speed * Time.deltaTime);


                Vector3 eulers = transform.eulerAngles;
                eulers.x = pitchAngle;
                eulers.z = rollAngle;
                transform.eulerAngles = eulers;

                //transform.Rotate(Vector3.forward, 20, Space.Self);



            }


        }

        
        /// <summary>
        /// Returns the center of mass in world coordinates
        /// </summary>
        /// <returns></returns>
        Vector3 GetCenterOfMass()
        {
            return transform.position + (head.position - transform.position) * 0.5f;
        }

        void CheckInput()
        {
#if UNITY_EDITOR
            /******************************* ONLY FOR TEST ******************************/
            if (isGrounded)
            {
                // Test input check

                
                //leftBrake = rightBrake = 0;
                float speed = 1;
                if (Input.GetKey(KeyCode.A))
                    leftBrake = Mathf.Min(1, leftBrake + speed * Time.deltaTime);
                else
                    leftBrake = Mathf.Max(0, leftBrake - speed * Time.deltaTime);
                
                if (Input.GetKey(KeyCode.D))
                    rightBrake = Mathf.Min(1, rightBrake + speed * Time.deltaTime);
                else
                    rightBrake = Mathf.Max(0, rightBrake - speed * Time.deltaTime);


                // Move brakes handles
                leftBrakePivot.localEulerAngles = new Vector3(leftBrake * -90, 0, 0);
                rightBrakePivot.localEulerAngles = new Vector3(rightBrake * -90, 0, 0);
            }
#endif
        }
        #endregion
    }

}
