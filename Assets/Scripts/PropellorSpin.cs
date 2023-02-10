using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spinning the plane's propellor relative to the local position of the plane

public class PropellorSpin : MonoBehaviour
{
    [SerializeField] private GameObject Propellor;
    [SerializeField] private Quaternion TargetRotation;
    [SerializeField] private float TargetAngle;
    [SerializeField, Range(0, 1000)] private float RotationRate;

    // default the rotation values
    private void Start()
    {
        TargetAngle = 0f;
        TargetRotation = Quaternion.AngleAxis(TargetAngle, Vector3.forward);
        Propellor.transform.localRotation = TargetRotation;
    }

    // move the propellor on every physics update
    private void FixedUpdate()
    {
        TargetAngle += RotationRate * Time.fixedDeltaTime;
        TargetRotation = Quaternion.AngleAxis(TargetAngle, Vector3.forward);
        Propellor.transform.localRotation = TargetRotation;
    }
}
