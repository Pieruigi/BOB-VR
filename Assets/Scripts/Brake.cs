using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Bob
{
    public class Brake : MonoBehaviour
    {
        XRGrabInteractable interactable;
        IXRSelectInteractor interactor;
        Vector3 attachPositionDefault;
        Quaternion attachRotationDefault;
        Vector3 positionDefault;
        Vector3 eulerAnglesDefault;
        Vector3 parentPositionDefault;
        Vector3 parentEulerAnglesDefault;

        Transform parent;

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();

            parent = transform.parent;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (interactor == null)
                return;

            // Debug.Log("Transform:" + transform.eulerAngles);
            // Reset eulers
            Vector3 eulers = new Vector3(transform.eulerAngles.x, parent.eulerAngles.y, parent.eulerAngles.z);
            transform.eulerAngles = eulers;
        }

        private void OnEnable()
        {
            interactable.firstSelectEntered.AddListener(OnFirstSelectEntered);
            interactable.lastSelectExited.AddListener(OnLastSelectEntered);
        }

        private void OnDisable()
        {
            interactable.firstHoverEntered.RemoveAllListeners();
        }

        void OnFirstSelectEntered(SelectEnterEventArgs args)
        {
            Debug.Log("OnFirstSelectEntered");
            if(args.interactorObject is XRDirectInteractor)
            {
                interactor = args.interactorObject;
                attachPositionDefault = interactable.attachTransform.position;
                attachRotationDefault = interactable.attachTransform.rotation;
                positionDefault = transform.position;
                eulerAnglesDefault = transform.eulerAngles;
                parentPositionDefault = parent.position;
                parentEulerAnglesDefault = parent.eulerAngles;

                Debug.Log("positionDefault:" + positionDefault);
                Debug.Log("eulerAnglesDefault:" + eulerAnglesDefault);
                Debug.Log("parentPositionDefault:" + parentPositionDefault);
                Debug.Log("parentEulerAnglesDefault:" + parentEulerAnglesDefault);
                //Debug.Log("AttachDefPosition:" + attachPositionDefault);
                //Debug.Log("AttachDefRotation:" + attachRotationDefault);
                //Debug.Log("positionDefault:" + positionDefault);
                //Debug.Log("rotationDefault:" + rotationDefault.eulerAngles);
            }
                
        }

        void OnLastSelectEntered(SelectExitEventArgs args)
        {
            Debug.Log("OnFirstSelectEntered");
            if (args.interactorObject is XRDirectInteractor && interactor == args.interactorObject)
            {
                interactor = null;
                
            }

        }
    }

}
