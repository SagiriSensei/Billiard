using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Wait,
    Aim,
    Side,
    Hit,
    Win
}


public class GameManager : MonoBehaviour
{
    bool m_waitHit;
    float m_waitCameraTime;
    float m_curPower;
    GameState m_state;
    Vector3 m_hitPoint;
    Vector3 m_hitForce;
    GameObject m_whiteBallGameObject;
    CameraControl m_cameraControl;

    List<Ball> m_ballLists = new List<Ball>();

    public float m_forceFactor;
    public float m_maxPower;
    public float m_powerSpeed;
    public float m_hitSpeed;

    public UIManager m_uiManager;
    public List<Texture> m_ballTextures;
    public Transform m_observeTransform;
    public Transform m_cueHead;
    public Transform m_ballSpawn;
    public Transform m_whiteBallSpawn;
    public GameObject m_ballPrefab;
    public GameObject m_whiteBallPrefab;

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
        m_cameraControl = Camera.main.GetComponent<CameraControl>();
    }

    private void Start()
    {
        SpawnBalls();
    }

    private void Update()
    {
        SwitchState();
    }

    public void SwitchState()
    {
        if (m_state == GameState.Aim)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SetSideState();
            }
            else if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                //击打
                SetHitState();
            }
        }
        else if (m_state == GameState.Hit)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                SetAimState();
            } 
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                float prePower = m_curPower;
                m_curPower = Mathf.Clamp(m_curPower  + m_powerSpeed * Time.deltaTime, 0, m_maxPower);
                if (m_curPower >= m_maxPower || m_curPower <= 0) m_powerSpeed = -m_powerSpeed;
                m_cameraControl.MoveCueInForward(prePower - m_curPower);
                m_uiManager.SetPowerBar(m_curPower / m_maxPower);
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Hit();
            }
        }
        else if (m_state == GameState.Side)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SetAimState();
            }
        }
        else if (m_state == GameState.Wait)
        {
            if (m_waitHit)
            {
                if (Vector3.Distance(m_hitPoint, m_cueHead.position) <= 0.1f)
                {
                    //施加力
                    m_waitHit = false;
                    m_whiteBallGameObject.GetComponent<Ball>().AddForce(m_hitForce * m_curPower * m_forceFactor, m_hitPoint);
                    m_cameraControl.SetLockedTransform(m_observeTransform);
                }
                else
                {
                    m_cameraControl.MoveCueInForward(m_hitSpeed * m_curPower * Time.deltaTime);
                }
            }
            else
            {
                if (IsAllBallStop() && m_waitCameraTime > 0.1f)
                {
                    m_cameraControl.SetAroundTransform(m_whiteBallGameObject.transform);
                }
                else
                {
                    m_waitCameraTime += Time.deltaTime;
                }
            }
        }
    }

    public void AddBall(Ball ball)
    {
        m_ballLists.Add(ball);
    }

    public void RemoveBall(Ball ball)
    {
        if (ball.gameObject.tag == "WhiteBall")
        {
            ball.transform.position = m_whiteBallSpawn.position;
            ball.transform.rotation = Quaternion.identity;
            ball.SetVelocity(Vector3.zero, Vector3.zero);
            m_waitCameraTime = 0.0f;
            return;
        }
        else
        {
            m_ballLists.Remove(ball);
            GameObject.Destroy(ball.gameObject);
        }
    }

    public GameState GetState()
    {
        return m_state;
    }

    public void SetAimState()
    {
        m_curPower = 0f;
        m_uiManager.SetPowerBar(0f);
        m_state = GameState.Aim;
        Cursor.lockState = CursorLockMode.Locked;
        m_cameraControl.RestoreState();
        m_uiManager.SetHitUIActive(true);
    }

    public void SetSideState()
    {
        m_state = GameState.Side;
        Cursor.lockState = CursorLockMode.None;
        m_cameraControl.SetStopState();
    }

    public void SetHitState()
    {
        m_state = GameState.Hit;
        Cursor.lockState = CursorLockMode.Locked;
        m_powerSpeed = Mathf.Abs(m_powerSpeed);
        m_cameraControl.SetStopState();
    }

    public void SetWaitState()
    {
        m_waitCameraTime = 0.0f;
        m_state = GameState.Wait;
        m_uiManager.SetHitUIActive(false);
    }

    public GameObject GetWhiteBallGameObject()
    {
        return m_whiteBallGameObject;
    }

    public void Restart()
    {
        for (int i = 0; i < m_ballLists.Count; i++)
        {
            GameObject.Destroy(m_ballLists[i].gameObject);
        }
        m_ballLists.Clear();
        SpawnBalls();
        m_cameraControl.ResetCameraTransform();
    }

    private void Hit()
    {
        m_waitHit = true;
        //计算击球点
        Ray ray = new Ray(m_cueHead.position, m_cueHead.forward);
        RaycastHit hitInfo;
        string[] masks = { "WhiteBall" };
        if (Physics.Raycast(ray, out hitInfo, 100.0f, LayerMask.GetMask(masks)))
        {
            m_hitPoint = hitInfo.point;
            m_hitForce = (m_hitPoint - m_cueHead.position).normalized;
            Debug.DrawLine(m_cueHead.position, m_hitPoint);
        }
        SetWaitState();
    }

    void SpawnBalls()
    {
        SphereCollider sphere = m_ballPrefab.GetComponent<SphereCollider>();
        float radius = sphere.radius;
        Vector3 startSpawnPoint = m_ballSpawn.position + new Vector3(0, radius, 0);
        Vector3 direction1 = new Vector3(-1.2f, 0, 2).normalized;
        Vector3 direction2 = new Vector3(1.2f, 0, 2).normalized;
        List<Vector3> ballPoints = new List<Vector3>();
        ballPoints.Add(startSpawnPoint);
        int textureId = 0;
        SpawnBall(startSpawnPoint, m_ballTextures[textureId++]);
        for (int count = 1; count <= 4; count++)
        {
            List<Vector3> newBallPoints = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPoint = ballPoints[i] + direction1 * radius * 2.1f;
                SpawnBall(spawnPoint, m_ballTextures[textureId++]);
                newBallPoints.Add(spawnPoint);
                if (i == count - 1)
                {
                    spawnPoint = ballPoints[i] + direction2 * radius * 2.1f;
                    SpawnBall(spawnPoint, m_ballTextures[textureId++]);
                    newBallPoints.Add(spawnPoint);
                }
            }
            ballPoints = newBallPoints;
        }
        m_whiteBallGameObject = GameObject.Instantiate(m_whiteBallPrefab, m_whiteBallSpawn.position + new Vector3(0, radius, 0), Quaternion.identity);
        SetWaitState();
    }

    GameObject SpawnBall(Vector3 spawnPoint, Texture texture)
    {
        GameObject ball = GameObject.Instantiate(m_ballPrefab, spawnPoint, m_ballPrefab.transform.rotation);
        ball.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        return ball;
    }

    bool IsAllBallStop()
    {
        for (int i = 0; i < m_ballLists.Count; i++)
        {
            Rigidbody rb = m_ballLists[i].GetComponent<Rigidbody>();
            if (rb.velocity.magnitude > 0.0001f || rb.angularVelocity.magnitude > 0.0001f)
            {
                return false;
            }
        }
        return true;
    }
}
