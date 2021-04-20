using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    public enum CameraView
    {
        FIRST_PERSON,
        THIRD_PERSON,
        CINEMATIC,
        FLY
    }

    public class CameraState
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

        public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
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

    private CameraState m_TargetCameraState = new CameraState();
    private CameraState m_InterpolatingCameraState = new CameraState();
    private LayerMask cameraMask;

    public CameraView view = CameraView.FIRST_PERSON;
    public int FOV = 70;

    [Header("First Person Settings")]
    public Transform fpHeadTarget = null;
    public Transform fpControlledTarget = null;
    public LayerMask fpViewMask = 0;
    public Vector3 fpOffset = Vector3.zero;
    public Vector2 fpPitchLimits = new Vector2(-60, 60);

    [Header("Third Person Settings")]
    public Transform thirdPersonLookTarget = null;
    public Transform thirdPersonControlledTarget = null;
    public LayerMask thirdPersonViewMask = 0;
    public Vector3 thirdPersonOffset = new Vector3(0, 0, 5);
    public Vector2 thirdPersonPitchLimits = new Vector2(-60, 60);

    [Header("Movement Settings")]
    public float boost = 3.5f;
    [Range(0.001f, 1f)] public float positionLerpTime = 0.2f;

    [Header("Rotation Settings")]
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));
    [Range(0.001f, 1f)] public float rotationLerpTime = 0.01f;
    public bool invertY = false;

    private void Start()
    {
        cameraMask = Camera.main.cullingMask;

        m_TargetCameraState.SetFromTransform(transform);
        m_InterpolatingCameraState.SetFromTransform(transform);
        SetView(view);
    }

    private Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = new Vector3();

        if (UIMaster.GetState() == UIState.FOCUSED) return direction;

        if (Input.GetButton("Forward")) direction += Vector3.forward;
        if (Input.GetButton("Backward")) direction += Vector3.back;
        if (Input.GetButton("Left")) direction += Vector3.left;
        if (Input.GetButton("Right")) direction += Vector3.right;
        if (Input.GetButton("Jump")) direction += Vector3.up;
        if (Input.GetButton("Crouch")) direction += Vector3.down;

        return direction;
    }

    private void HandleFirstPerson()
    {
        float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / 0.01f) * Time.deltaTime);   //  Force a low position lerp
        float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

        //  Rotation
        Vector2 mouseMovement = Vector3.zero;
        if (UIMaster.GetState() != UIState.FOCUSED)
        {
            mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
        m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
        m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;

        //  Position
        m_TargetCameraState.x = fpHeadTarget.position.x;
        m_TargetCameraState.y = fpHeadTarget.position.y;
        m_TargetCameraState.z = fpHeadTarget.position.z;

        // Framerate-independent interpolation
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        //  Pitch limits
        if (m_TargetCameraState.pitch < fpPitchLimits.x) m_TargetCameraState.pitch = fpPitchLimits.x;
        if (m_TargetCameraState.pitch > fpPitchLimits.y) m_TargetCameraState.pitch = fpPitchLimits.y;

        if (m_InterpolatingCameraState.pitch < fpPitchLimits.x) m_InterpolatingCameraState.pitch = fpPitchLimits.x;
        if (m_InterpolatingCameraState.pitch > fpPitchLimits.y) m_InterpolatingCameraState.pitch = fpPitchLimits.y;

        //  Update camera and targets
        m_InterpolatingCameraState.UpdateTransform(transform);
        fpHeadTarget.rotation = Quaternion.Euler( transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z );
        fpControlledTarget.rotation = Quaternion.Euler( fpControlledTarget.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, fpControlledTarget.rotation.eulerAngles.z );
    }

    private void HandleThirdPerson()
    {
        float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);   //  Force a low position lerp
        float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

        //  Rotation
        Vector2 mouseMovement = Vector3.zero;

        if (UIMaster.GetState() != UIState.FOCUSED)
        {
            mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
        m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
        m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;

        //  Position
        Vector3 targetPos = thirdPersonLookTarget.position + (thirdPersonLookTarget.rotation * thirdPersonOffset);
        m_TargetCameraState.x = targetPos.x;
        m_TargetCameraState.y = targetPos.y;
        m_TargetCameraState.z = targetPos.z;

        // Framerate-independent interpolation
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        //  Pitch limits
        if (m_TargetCameraState.pitch < thirdPersonPitchLimits.x) m_TargetCameraState.pitch = thirdPersonPitchLimits.x;
        if (m_TargetCameraState.pitch > thirdPersonPitchLimits.y) m_TargetCameraState.pitch = thirdPersonPitchLimits.y;

        if (m_InterpolatingCameraState.pitch < thirdPersonPitchLimits.x) m_InterpolatingCameraState.pitch = thirdPersonPitchLimits.x;
        if (m_InterpolatingCameraState.pitch > thirdPersonPitchLimits.y) m_InterpolatingCameraState.pitch = thirdPersonPitchLimits.y;

        //  Update camera and targets
        m_InterpolatingCameraState.UpdateTransform(transform);
        thirdPersonLookTarget.rotation = Quaternion.Euler( transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z );
        thirdPersonControlledTarget.rotation = Quaternion.Euler( thirdPersonControlledTarget.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, thirdPersonControlledTarget.rotation.eulerAngles.z );
    }

    private void HandleCinematic()
    {
        // Rotation
        if (Input.GetMouseButton(1))
        {
            Vector2 mouseMovement = Vector3.zero;

            if (UIMaster.GetState() != UIState.FOCUSED)
            {
                mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

                //  Show/hide cursor
                if (Input.GetMouseButton(1)) Cursor.lockState = CursorLockMode.Locked;
                else { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
            }

            float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (Input.GetButton("Sprint")) translation *= 10.0f;

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        boost += Input.mouseScrollDelta.y * 0.2f;
        translation *= Mathf.Pow(2.0f, boost);

        m_TargetCameraState.Translate(translation);

        // Framerate-independent interpolation
        float positionLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) * Time.deltaTime);
        float rotationLerpPct = 1f - Mathf.Exp(Mathf.Log(1f - 0.99f) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    private void HandleFly()
    {
        //  Show/hide cursor
        if (Input.GetMouseButton(1)) Cursor.lockState = CursorLockMode.Locked;
        else { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }

        // Rotation
        if (Input.GetMouseButton(1))
        {
            Vector2 mouseMovement = Vector3.zero;

            if (UIMaster.GetState() != UIState.FOCUSED)
            {
                mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y") * (invertY ? 1 : -1));

                //  Show/hide cursor
                if (Input.GetMouseButton(1)) Cursor.lockState = CursorLockMode.Locked;
                else { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
            }

            float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);
            m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
            m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
        }

        // Translation
        Vector3 translation = GetInputTranslationDirection() * Time.deltaTime;

        // Speed up movement when shift key held
        if (Input.GetButton("Sprint")) translation *= 10.0f;

        // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
        boost += Input.mouseScrollDelta.y * 0.2f;
        translation *= Mathf.Pow(2.0f, boost);

        m_TargetCameraState.Translate(translation);

        // Framerate-independent interpolation
        float positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
        float rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
        m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

        m_InterpolatingCameraState.UpdateTransform(transform);
    }

    private void SetView(CameraView view)
    {
        this.view = view;

        if (view == CameraView.FIRST_PERSON) Camera.main.cullingMask = (cameraMask.value) ^ (fpViewMask.value);
        else if (view == CameraView.THIRD_PERSON) Camera.main.cullingMask = (cameraMask.value) ^ (thirdPersonViewMask.value);
    }

    private void Update()
    {
        //  If the UI isn't open and we are playing or spectating, allow camera control
        if ( UIMaster.GetState() != UIState.FOCUSED && (GameMaster.GetState() == GameState.PLAYING || GameMaster.GetState() == GameState.SPECTATING) )
        {
            if (Input.GetButtonDown("Switch View"))         { SetView( view == CameraView.FIRST_PERSON ? CameraView.THIRD_PERSON : CameraView.FIRST_PERSON ); GameMaster.SetState(GameState.PLAYING); }

            if (Input.GetButtonDown("Cinematic Camera"))    { SetView(CameraView.CINEMATIC); GameMaster.SetState(GameState.SPECTATING); }
            if (Input.GetButtonDown("Fly Camera"))          { SetView(CameraView.FLY); GameMaster.SetState(GameState.SPECTATING); }

            if (Input.GetButtonDown("Camera Zoom")) Camera.main.fieldOfView = 20;
            if (Input.GetButtonUp("Camera Zoom")) Camera.main.fieldOfView = FOV;
        }

        switch (view)
        {
            case CameraView.FIRST_PERSON:
            HandleFirstPerson();
            break;

            case CameraView.THIRD_PERSON:
            HandleThirdPerson();
            break;

            case CameraView.CINEMATIC:
            HandleCinematic();
            break;

            case CameraView.FLY:
            HandleFly();
            break;
        }
    }
}
