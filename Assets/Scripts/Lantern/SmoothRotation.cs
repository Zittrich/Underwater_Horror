using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothRotation : MonoBehaviour
{
    public Transform target;
    public float movementTime = 1;
    public float rotationSpeed = 0.1f;

    Vector3 refPos;
    Vector3 refRot;

    void Update()
    {
        if (!target)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref refPos, movementTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);
    }
}
