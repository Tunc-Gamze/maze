using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using DG.Tweening;

namespace RunnerGame
{
    public class pointScript : MonoBehaviour
    {
    
       public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
               Debug.Log("asdfg");
               Destroy(this.gameObject);

            }
            
        }
    

    
    
    }
}