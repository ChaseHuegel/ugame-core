using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMotor : MonoBehaviour
{
    public class FollowState
    {
        public float yaw;
        public float pitch;
        public float roll;
        public float x;
        public float y;
        public float z;

        public void SetFromTransform(Transform t)
        {
            pitch = t.eulerAngles.x;
            yaw = t.eulerAngles.y;
            roll = t.eulerAngles.z;
            x = t.position.x;
            y = t.position.y;
            z = t.position.z;
        }

        public void Translate(Vector3 translation)
        {
            Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

            x += rotatedTranslation.x;
            y += rotatedTranslation.y;
            z += rotatedTranslation.z;
        }

        public void LerpTowards(FollowState target, float positionLerpPct, float rotationLerpPct)
        {
            yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
            pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);

            x = Mathf.Lerp(x, target.x, positionLerpPct);
            y = Mathf.Lerp(y, target.y, positionLerpPct);
            z = Mathf.Lerp(z, target.z, positionLerpPct);
        }

        public void UpdateTransform(Transform t)
        {
            t.eulerAngles = new Vector3(pitch, yaw, roll);
            t.position = new Vector3(x, y, z);
        }
    }

    private FollowState m_TargetCameraState = new FollowState();
    private FollowState m_InterpolatingCameraState = new FollowState();
    private Vector3 velocity = Vector3.zero;

    public Transform target = null;
    public Vector3 offset = Vector3.zero;

    [Range(0f, 1f)] public float positionLerpTime = 0.2f;
    [Range(0f, 1f)] public float rotationLerpTime = 0.01f;

    private void OnEnable()
    {
        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
    }

    private void HandleFollow()
    {
        float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

        Vector3 targetPos = target.position + (target.rotation * offset);

        transform.position = Vector3.SmoothDamp( transform.position, targetPos, ref velocity, positionLerpTime );
        transform.rotation = Quaternion.Slerp( transform.rotation, target.rotation, rotationLerpPct );
    }

    private void LateUpdate()
    {
        HandleFollow();
    }
}
