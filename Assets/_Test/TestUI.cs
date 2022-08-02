using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bob.Test
{
    public class TestUI : MonoBehaviour
    {
        [SerializeField]
        TMP_Text targetSpeed;
        
        [SerializeField]
        Transform targetVelocityH;

        [SerializeField]
        Transform targetvelocityV;

        CharacterController characterController;
        PlayerController playerController;

        // Start is called before the first frame update
        void Start()
        {
            characterController = GameObject.FindObjectOfType<CharacterController>();
            playerController = GameObject.FindObjectOfType<PlayerController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void LateUpdate()
        {
            targetSpeed.text = playerController.TargetVelocity.magnitude.ToString();   

        }
    }

}
