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

                //Debug.Log("SignedPitch:" + Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.up, Vector3.right), Vector3.ProjectOnPlane(groundNormal, Vector3.right), Vector3.right));
                float groundPitch = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.up, transform.right), Vector3.ProjectOnPlane(groundNormal, transform.right), transform.right);
                
                groundPitch += transform.eulerAngles.x;
                //Debug.Log("SignedRoll:" + Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.up, Vector3.forward), Vector3.ProjectOnPlane(groundNormal, Vector3.forward), Vector3.forward));
                float groundRoll = Vector3.SignedAngle(Vector3.ProjectOnPlane(transform.up, transform.forward), Vector3.ProjectOnPlane(groundNormal, transform.forward), transform.forward);
               

                groundRoll += transform.eulerAngles.z;
                Debug.Log("GroundPitch:" + groundPitch);
                Debug.Log("GroundRoll:" + groundRoll);

                Vector3 eulers = transform.eulerAngles;
                
                // Get the overturn angle, which is the difference between the actual pitch and the 
                // pitch you should have depending on the ground
                
                float overturnRoll = eulers.z - groundRoll;

                // Update euler vector
                eulers.x = groundPitch;
                eulers.z = groundRoll;

                float oldOverTurnRoll = overturnRoll;

                //
                // Check for overturn
                //
                // Transform to local coords
                Vector3 centerOfMass = transform.InverseTransformPoint(GetCenterOfMass());
                centerOfMass.z = 0; // We only need right coordinates
                // Back to world coords
                centerOfMass = transform.TransformPoint(centerOfMass);
                // Where the center of mass falls on the horizontal plane
                Vector3 comFall = Vector3.ProjectOnPlane(centerOfMass - transform.position, Vector3.up);
                Vector3 rFall = Vector3.ProjectOnPlane(transform.right * cc.radius, Vector3.up);
                float massFactor = 1f;

                Debug.Log("ComFall:" + comFall);
                Debug.Log("ComFall.Magnitude:" + comFall.magnitude);
                Debug.Log("rFall:" + rFall);
                Debug.Log("rFall.Magnitude:" + rFall.magnitude);
                Vector3 dist = comFall - rFall;
                // Positive angle when the center of mass falls within the bob base, otherwise is negative
                float rRotForce = -massFactor * dist.magnitude * Vector3.Dot(dist.normalized, rFall.normalized);
                Debug.Log("rRotForce:" + rRotForce);
                //tmp = comFall + rFall;
                // Negative force when the center of mass falls within the bob base, otherwise is positive
                float lRotForce = rRotForce - massFactor;// -massFactor * tmp.magnitude * Vector3.Dot(tmp.normalized, rFall.normalized);
                //rRotForce *= massFactor;
                //lRotForce *= massFactor;
                Debug.Log("lRotForce:" + lRotForce);

                // Compute the overturn force given by the movement
                float externalOverturnForce = 0;

                float angle = Vector3.Angle(Vector3.ProjectOnPlane(cc.velocity, Vector3.up), Vector3.ProjectOnPlane(transform.right, Vector3.up));
                float range = 20;
                if(angle > 90 + range || angle < 90 - range)
                {
                    if (angle < 90 - range)
                    {
                        float angleFactor = angle / (90f - range) * 0.5f - 1f;
                        externalOverturnForce = angleFactor * 2 ;
                    }

                    else
                    {
                        float angleFactor = 0.5f * (2f - (180f - angle) / (180f - 90f - range));
                        externalOverturnForce = angleFactor * 2;
                    }
                        
                } 

                Debug.Log("angle:" + angle);
                Debug.Log("externalOverturnForce:" + externalOverturnForce);
                //externalOverturnForce = 0;
                //rollOverturn = 10;
                // If the bob is laying on the ground, there is no slope along its side and its center of mass falls within 
                // its base then there is no reason to compute the overturn force
                if (!(overturnRoll == 0 && externalOverturnForce == 0 && rRotForce >= 0 && lRotForce <= 0))
                {

                    float totalForce = 0;
                    
                    if(externalOverturnForce == 0)
                    {// No force is applied to overturn the bob, we apply center of mass only if the bob
                     // has already been overturned and/or the center of mass falls outside the base

                        if (overturnRoll != 0 || rRotForce < 0 || lRotForce > 0)
                        {
                            if(overturnRoll == 0)
                            {
                                totalForce = rRotForce < 0 ? rRotForce : lRotForce;
                            }
                            else
                            {
                                totalForce = overturnRoll > 0 ? lRotForce : rRotForce;
                            }
                        }
                    }
                    else
                    { 

                        totalForce = externalOverturnForce;

                        // If the bob has already been overturned we can easily choose between L and R forces
                        if(overturnRoll != 0)
                        {
                            if (overturnRoll > 0)
                                totalForce += lRotForce;
                            else
                                totalForce += rRotForce;
                        }
                        else
                        {
                            if (externalOverturnForce > 0) 
                                totalForce = Mathf.Max(0, totalForce + lRotForce);
                            else
                                totalForce = Mathf.Min(0, totalForce + rRotForce);
                        }
                    }

                    if(totalForce != 0)
                    {
                        float d = totalForce * 20 * Time.deltaTime;
                        if (Mathf.Sign(overturnRoll * totalForce) > 0)
                        {
                            overturnRoll += d;
                        }
                        else
                        {
                            if (overturnRoll > 0)
                                overturnRoll = Mathf.Max(0, overturnRoll + d);
                            else
                                overturnRoll = Mathf.Min(0, overturnRoll + d);
                        }
                    }
                    
                    //Quaternion.el

                    eulers.z += overturnRoll;
                 
                    Debug.Log("TotalForce:" + totalForce);
                    
                }

                transform.eulerAngles = eulers;
                //transform.rotation = Quaternion.AngleAxis(overturnRoll, transform.forward);
                //if (overturnRoll != 0)
                //    transform.Rotate(transform.forward, overturnRoll - oldOverTurnRoll, Space.Self);

            }    

            
        }

        
        /// <summary>
        /// Returns the center of mass in world coordinates
        /// </summary>
        /// <returns></returns>
        Vector3 GetCenterOfMass()
        {
            return transform.position - transform.up * cc.radius + (head.position - transform.position) * 0.8f;
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
