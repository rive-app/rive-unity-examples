using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rive;

[RequireComponent(typeof(RiveTexture))]
public class MinionLookScript : MonoBehaviour
{
    public Camera camera;
    public float xLookDamp = 0.5f;
    public float yLookDamp = 0.5f;

    private StateMachine _riveStateMachine;
    private SMINumber _lookHorizontalInput;
    private SMINumber _lookVerticalInput;

    void Start()
    {
        var riveTexture = GetComponent<RiveTexture>();
        _riveStateMachine = riveTexture.stateMachine;
        _lookHorizontalInput = _riveStateMachine.getNumber("track_x");
        _lookVerticalInput = _riveStateMachine.getNumber("track_y");
        float randomTime = Random.Range(0.0f, 4.0f);
        _riveStateMachine.advance(0.0f);
        _riveStateMachine.advance(randomTime);
    }

    void Update()
    {
        Vector3 directionToCamera = camera.transform.position - transform.position;
        directionToCamera.Normalize();
        float xProduct = Vector3.Dot(transform.right, directionToCamera);
        float yProduct = Vector3.Dot(transform.up, directionToCamera);
        _lookHorizontalInput.value = xProduct * xLookDamp * 100;
        _lookVerticalInput.value = yProduct * yLookDamp * 100;

    }
}
