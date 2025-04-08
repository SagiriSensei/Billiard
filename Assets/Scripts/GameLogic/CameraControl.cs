using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraState
{
    Locked,
    Free,
    Around,
    Move,
    Stop
}

public class CameraControl : MonoBehaviour
{
    Camera m_mainCamera;
    Side m_side;
    Cue m_cue;
    float m_ballRadius;
    float m_limitEulerX;
    float m_eulerX;
    float m_eulerY;
    Quaternion m_targetRotation;
    Vector3 m_targetPosition;
    CameraState m_state;
    CameraState m_preState;

    public Transform m_rotateYAndPositionTransform;
    public Transform m_rotateXTransform;
    public float m_lerpStep;
    public float m_closeEnoughThreshold;

    public float m_lowAngle;
    public float m_hignAngle;
    public float m_aroundInnerRadius;
    public float m_aroundOuterRadius;

    public float m_moveSpeed;
    public float m_rotateSpeed;
    public float m_zoomSpeed;

    public float m_cueDisToWhiteBall;
    public float m_cueSideCircleScaler;

    private void Awake()
    {
        m_cue = FindObjectOfType<Cue>();
        m_side = FindObjectOfType<Side>();
        m_mainCamera = GetComponent<Camera>();
        m_state = CameraState.Stop;
        m_cue.gameObject.SetActive(false);
    }

    private void Start()
    {
        m_ballRadius = GameManager.instance.GetWhiteBallGameObject().GetComponent<SphereCollider>().radius;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return;
        if (m_state == CameraState.Free)
        {
            int forward = Input.GetKey(KeyCode.W) ? 1 : 0 - (Input.GetKey(KeyCode.S) ? 1 : 0);
            int right = Input.GetKey(KeyCode.D) ? 1 : 0 - (Input.GetKey(KeyCode.A) ? 1 : 0);
            m_rotateYAndPositionTransform.position += m_rotateXTransform.forward * forward * m_moveSpeed + m_rotateXTransform.right * right * m_moveSpeed;
            Rotate();
        } 
        else if (m_state == CameraState.Locked)
        {
           
        } 
        else if (m_state == CameraState.Around)
        {
            Rotate();
            float scroll = -Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                Vector3 direction = transform.position - m_rotateYAndPositionTransform.position;
                float distance = direction.magnitude;
                distance = Mathf.Clamp(distance + scroll * m_zoomSpeed, m_aroundInnerRadius, m_aroundOuterRadius);
                transform.position = m_rotateYAndPositionTransform.position + direction.normalized * distance;
            }
        } 
        else if (m_state == CameraState.Move)
        {
            transform.position = Vector3.Lerp(transform.position, m_targetPosition, m_lerpStep);
            transform.rotation = Quaternion.Lerp(transform.rotation, m_targetRotation, m_lerpStep);
            bool positionCloseEnough = (transform.position - m_targetPosition).magnitude <= m_closeEnoughThreshold;
            bool rotationCloseEnough = (transform.forward - (m_rotateYAndPositionTransform.position - m_targetPosition).normalized).magnitude <= m_closeEnoughThreshold;
            if (positionCloseEnough && rotationCloseEnough)
            {
                m_state = CameraState.Around;
                m_eulerX = 0;
                m_eulerY = 0;
                m_cue.gameObject.SetActive(true);
                m_side.ResetSide();
                SetCuePositionWithSide(Vector2.zero);
                GameManager.instance.SetAimState();
            }
        }
    }

    public void Rotate()
    {
        float horizontal = Input.GetAxis("Mouse X");
        float vertical = Input.GetAxis("Mouse Y");
        m_eulerY += horizontal * m_rotateSpeed;
        m_eulerY = Mathf.Clamp((m_eulerY + 360) % 360, 0, 360);
        m_eulerX -= vertical * m_rotateSpeed;
        m_eulerX = Mathf.Clamp(m_eulerX, -m_limitEulerX, m_limitEulerX);
        m_rotateYAndPositionTransform.localEulerAngles = new Vector3(0, m_eulerY, 0);
        m_rotateXTransform.localEulerAngles = new Vector3(m_eulerX, 0, 0);
    }

    public void SetFree()
    {
        m_state = CameraState.Free;
        m_limitEulerX = 90f;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        m_rotateYAndPositionTransform.position = position;
        m_rotateYAndPositionTransform.localEulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
        m_rotateXTransform.localEulerAngles = new Vector3(rotation.eulerAngles.x, 0, 0);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        m_eulerX = m_rotateXTransform.rotation.eulerAngles.x;
        m_eulerY = m_rotateYAndPositionTransform.rotation.eulerAngles.y;
        m_cue.gameObject.SetActive(false);
    }

    public void SetLockedTransform(Transform t)
    {
        m_state = CameraState.Locked;

        m_rotateYAndPositionTransform.position = Vector3.zero;
        m_rotateYAndPositionTransform.rotation = Quaternion.identity;
        transform.position = t.position;
        transform.rotation = t.rotation;
        m_cue.gameObject.SetActive(false);
    }

    public void SetAroundTransform(Transform t)
    {
        m_state = CameraState.Move;
        Vector3 direction = new Vector3(0, Mathf.Tan(Mathf.Deg2Rad * (m_lowAngle + m_hignAngle) / 2), -1).normalized;
        m_limitEulerX = (m_hignAngle - m_lowAngle) / 2;
        Vector3 originPosition = transform.position;
        Quaternion originRotation = transform.rotation;
        m_rotateYAndPositionTransform.position = t.position;
        m_rotateYAndPositionTransform.rotation = Quaternion.identity;
        m_rotateXTransform.localRotation = Quaternion.identity;
        transform.position = originPosition;
        transform.rotation = originRotation;
        m_targetPosition = m_rotateYAndPositionTransform.position + direction * (m_aroundOuterRadius + m_aroundInnerRadius) / 2;
        m_targetRotation = Quaternion.LookRotation(m_rotateYAndPositionTransform.position - m_targetPosition, Vector3.up);
    }

    public void RestoreState()
    {
        if (m_state == CameraState.Stop)
        {
            m_state = m_preState;
        }
    }

    public void SetStopState()
    {
        m_preState = m_state;
        m_state = CameraState.Stop;
    }

    public void SetCuePositionWithSide(Vector2 direction)
    {
        Vector3 position = Vector3.ProjectOnPlane(transform.position - m_rotateYAndPositionTransform.position, Vector3.up).normalized * m_cueDisToWhiteBall;
        position += m_rotateYAndPositionTransform.position;
        position += direction.x * m_cue.transform.right * m_ballRadius * m_cueSideCircleScaler;
        position += direction.y * m_cue.transform.up * m_ballRadius * m_cueSideCircleScaler;
        m_cue.SetPosition(position);
    }

    public void MoveCueInForward(float scaler)
    {
        Vector3 offset = m_cue.transform.forward * scaler;
        m_cue.Move(offset);
    }

    public void ResetCameraTransform()
    {
        m_rotateYAndPositionTransform.position = Vector3.zero;
        m_rotateYAndPositionTransform.rotation = Quaternion.identity;
        m_rotateXTransform.position = Vector3.zero;
        m_rotateXTransform.rotation = Quaternion.identity;
        m_cue.transform.position = Vector3.zero;
        m_cue.transform.rotation = Quaternion.identity;
    }

    public CameraState GetCameraState()
    {
        return m_state;
    }
}
