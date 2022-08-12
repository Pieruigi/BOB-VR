using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XROffsetGrabInteractable : XRGrabInteractable
{

    protected override void Awake()
    {
        base.Awake();

        if (!attachTransform)
        {
            // Create a new attach point
            attachTransform = new GameObject("Attach Pivot").transform;
            attachTransform.SetParent(transform, false);
        }

    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
       
        if(args.interactorObject is XRDirectInteractor)
        {
            Debug.Log("IsGrabInteractable");
            attachTransform.position = args.interactorObject.transform.position;
            attachTransform.rotation = args.interactorObject.transform.rotation;
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
            Vector3 dist = attachTransform.position - transform.localPosition;
            attachTransform.parent = null;
            attachTransform.position = interactorsSelecting[0].transform.position;
            transform.position = attachTransform.position - dist;
            attachTransform.parent = transform;
            
        }
            
    }
}
