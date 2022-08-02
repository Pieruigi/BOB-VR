using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bob.Test
{
    public class TestUI : MonoBehaviour
    {
        [SerializeField]
        TMP_Text speed;

        CharacterController characterController;

        // Start is called before the first frame update
        void Start()
        {
            characterController = GameObject.FindObjectOfType<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void LateUpdate()
        {
            speed.text = characterController.velocity.magnitude.ToString();   
        }
    }

}
