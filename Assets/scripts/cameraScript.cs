using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public Transform Target;
    private Vector3 followDistance;

    private void Start()
    {
        if (Target == null)
        {
            Debug.LogWarning("Camera target is not assigned; follow disabled.", this);
            enabled = false;
            return;
        }

        followDistance = Target.position - transform.position;
    }

    private void LateUpdate()
    {
        if (Target == null)
        {
            enabled = false;
            return;
        }

        transform.position = Target.position - followDistance;
    }
}
