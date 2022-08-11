using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Bob
{
    public class GameManager : MonoBehaviour
    {
        #region properties
        public Vector3 CameraFloorOffsetPosition
        {
            get { return cameraFloorOffsetPosition; }
        }
        #endregion

        #region private fields
        Vector3 cameraFloorOffsetPosition;

        #endregion

        #region native methods
        private void Awake()
        {
#if UNITY_EDITOR
            SetCameraFloorOffsetPosition(new Vector3(0.18f, -0.75f, -0.3f));
#endif
        }

        // Start is called before the first frame update
        void Start()
        {
            //XROrigin rig = GameObject.FindObjectOfType<XROrigin>();

            //rig.CameraFloorOffsetObject.transform.localPosition = cameraFloorOffsetPosition;
        }

        // Update is called once per frame
        void Update()
        {

        }

        #endregion

        #region public methods
        public void SetCameraFloorOffsetPosition(Vector3 position)
        {
            cameraFloorOffsetPosition = position;

        } 
        #endregion
    }

}
