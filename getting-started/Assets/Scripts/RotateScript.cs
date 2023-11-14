using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public Vector3 RotateAmount;
    public float Speed = 10;

    void Update()
    {
        transform.Rotate(RotateAmount * Time.deltaTime * Speed);
    }
}
