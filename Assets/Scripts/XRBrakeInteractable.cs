using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRBrakeInteractable : XRGrabInteractable
{
    bool fixedAttachPoint = true;
    float attachPointStartingAngle = 0; // The starting forward angle on grab enter
    float attachPointCurrentAngle = 0; // The current forward angle
    float brakeStartingAngle; // The starting forward angle on grab enter
    float startingAngleDiff; // attachPointStartingAngle - brakeStartingAngle ( on grab enter )
    Transform parent;
    float brakeMinAngle = 0; 
    float brakeMaxAngle = 90;

    protected override void Awake()
    {
        base.Awake();

        if (!attachTransform)
        {
            // Create a new attach point
            attachTransform = new GameObject("Attach Pivot").transform;
            attachTransform.SetParent(transform, false);
            fixedAttachPoint = false;
        }

        parent = transform.parent;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
       
        if(args.interactorObject is XRDirectInteractor)
        {
            Debug.Log("IsGrabInteractable");
            if (!fixedAttachPoint)
            {
                
                attachTransform.position = args.interactorObject.transform.position;
                attachTransform.rotation = args.interactorObject.transform.rotation;
            }

            // Compute the starting angle of the attachPoint vector: the angle between the pivot-attachPoint
            // and the bob forward vectors
            attachPointStartingAngle = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(attachTransform.position - transform.position, transform.right),
                parent.forward,
                transform.right);
            Debug.Log("attachPointStartingAngle:" + attachPointStartingAngle);
            attachPointCurrentAngle = attachPointStartingAngle;
            brakeStartingAngle = Vector3.SignedAngle(
                transform.forward,
                parent.forward,
                transform.right);
            Debug.Log("brakeStartingAngle:" + brakeStartingAngle);
            startingAngleDiff = attachPointStartingAngle - brakeStartingAngle;
        }

        base.OnSelectEntered(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        if(args.interactorObject is XRDirectInteractor)
        {
        }

        base.OnSelectExited(args);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        //base.ProcessInteractable(updatePhase);
        if(interactorsSelecting.Count > 0)
        {

            float attachPointLastAngle = attachPointCurrentAngle;

            Vector3 dist = attachTransform.position - transform.localPosition;
            attachTransform.parent = null;
            attachTransform.position = interactorsSelecting[0].transform.position;
            //transform.position = attachTransform.position - dist;
            attachTransform.parent = transform;
           
            
            // Compute the current angle
            attachPointCurrentAngle = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(attachTransform.position - transform.position, transform.right),
                parent.forward,
                transform.right);

            // Get the difference between the current angle and the starting one
            float diffAngle = attachPointCurrentAngle - attachPointLastAngle;
            
            if(diffAngle != 0)
            {
                if(diffAngle > 0)// We are pulling
                {
                    float max = startingAngleDiff + brakeMaxAngle;
                    if (attachPointCurrentAngle > max) // Too much
                    {
                        attachPointCurrentAngle = max;
                        diffAngle = attachPointCurrentAngle - attachPointLastAngle;
                    }
                }
                else// We are releasing
                {
                    float min = brakeMinAngle + startingAngleDiff;

                    if(attachPointCurrentAngle < min)
                    {
                        attachPointCurrentAngle = min;
                        diffAngle = attachPointCurrentAngle - attachPointLastAngle;
                    }
                }

                Debug.Log("AttachPointCurrentAngle:" + attachPointCurrentAngle);
            }
        }
            
    }
}
