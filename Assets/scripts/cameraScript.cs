using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
     public Transform Target;
     private Vector3 FollowDistance;
    void Start()
    {
        FollowDistance = Target.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
         transform.position = Target.position - FollowDistance;
    }
}
