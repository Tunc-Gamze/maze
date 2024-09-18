using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RunnerGame
{
   public class playerScript : MonoBehaviour
   {
     public float MoveSpeed;
     public Rigidbody RBody;
     public float JumpSpeed;
     public bool IsGrounded;
     public GameObject Coin; 

    void Start()
    {
        RBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 moveVec = new Vector3(moveX, 0, moveZ) * MoveSpeed * Time.fixedDeltaTime;
            RBody.AddForce(moveVec);

            
            if (Input.GetKey(KeyCode.Space) && IsGrounded)
            {
                RBody.AddForce(0, JumpSpeed, 0);
                IsGrounded = false;
            }

           
    }

    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.tag == "Ground")
            {
                IsGrounded = true;
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Finish")
            {
                Debug.Log("next level or play again or exit");
            }
           
            
        }
        
            

    }
}