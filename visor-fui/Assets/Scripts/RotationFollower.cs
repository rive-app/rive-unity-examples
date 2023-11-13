using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationFollower : MonoBehaviour
{
    public Transform TargetTransform;
    public float speed = 7f;
    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, TargetTransform.rotation, speed * Time.deltaTime);
    }
}
