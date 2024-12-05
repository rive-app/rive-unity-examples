using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateScript : MonoBehaviour
{
    public Vector3 RotateAmount = new Vector3(2,2,4);
    public float Speed = 10;

    void Update()
    {
        transform.Rotate(RotateAmount * Time.deltaTime * Speed);
    }
}
