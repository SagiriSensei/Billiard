using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    bool m_openMenu;
    CameraControl m_cameraControl;


    public Button m_restart;
    public Button m_backToMainMenu;
    public GameObject m_hitUI;
    public GameObject m_menu;
    public Image m_powerImage;

    private void Awake()
    {
        m_openMenu = false;
        m_cameraControl = Camera.main.GetComponent<CameraControl>();
    }
    void Start()
    {
        m_menu.SetActive(false);
        m_hitUI.SetActive(false);

        m_backToMainMenu.onClick.AddListener(BackToMainMenu);
        m_restart.onClick.AddListener(Restart);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (m_openMenu)
            {
                m_openMenu = false;
                Time.timeScale = 1;
                m_menu.SetActive(false);
                GameState curGameState = GameManager.instance.GetState();
                if (curGameState != GameState.Side) Cursor.lockState = CursorLockMode.Locked;
                if (curGameState == GameState.Aim || curGameState == GameState.Side) SetHitUIActive(true);
            }
            else
            {
                m_openMenu = true;
                m_menu.SetActive(true);
                Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                SetHitUIActive(false);
            }
        }
    }

    void SetAround()
    {
        m_cameraControl.SetFree();
    }

    public void SetHitUIActive(bool active)
    {
        m_hitUI.SetActive(active);
    }

    public void SetPowerBar(float fillAmount)
    {
        m_powerImage.fillAmount = fillAmount;
    }

    public void Restart()
    {
        Time.timeScale = 1;
        m_menu.SetActive(false);
        m_openMenu = false;
        GameManager.instance.Restart();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainScene");
    }
}
