using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GentleBob : MonoBehaviour
{
    [SerializeField] private Transform target;

    public bool rotate = true;

    public float bobSpeed = 1.0f;
    public float rotateSpeed = 1.0f;

    public Vector3 positionStrength = Vector3.zero;
    public Vector3 rotationWeight = Vector3.zero;

    private Vector3 baseLocation;
    private Vector3 baseRotation;
    private Vector3 positionModifier;
    private Vector3 rotationModifier;

    private float locationRatio;
    private float rotationRatio;
    private float lastBobSpeed;

    public Transform GetTarget() { return target; }
    public void SetTarget(Transform t) { target = t; GrabTransform(); }

    public void GrabTransform()
    {
        if (target == null) return;

        baseLocation = target.localPosition;
        baseRotation = target.localRotation.eulerAngles;
    }

    private void Start()
    {
        GrabTransform();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (bobSpeed != lastBobSpeed) locationRatio = 0.0f;
        locationRatio = Mathf.Clamp(locationRatio + Time.deltaTime, 0.0f, 1.0f);

        if (bobSpeed > 0)
        {
            positionModifier.x = Mathf.Sin(Time.time * bobSpeed) * positionStrength.x;
            positionModifier.y = Mathf.Sin(Time.time * bobSpeed) * positionStrength.y;
            positionModifier.z = Mathf.Sin(Time.time * -bobSpeed) * positionStrength.z;

            rotationModifier.x = Mathf.Sin(Time.time * rotateSpeed) * 180 * rotationWeight.x;
            rotationModifier.y = Mathf.Sin(Time.time * rotateSpeed) * 180 * rotationWeight.y;
            rotationModifier.z = Mathf.Sin(Time.time * rotateSpeed) * 180 * rotationWeight.z;

            target.localPosition = Vector3.Lerp(target.localPosition, baseLocation + positionModifier, locationRatio);
            if (rotate) target.localRotation = Quaternion.Euler(baseRotation + rotationModifier);
        }
        else
        {
            target.localPosition = Vector3.Lerp(target.localPosition, baseLocation, locationRatio);
        }

        lastBobSpeed = bobSpeed;
    }
}
