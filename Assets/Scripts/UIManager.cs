using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("��Ϸ����UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverDistanceText;
    public Button gameOverRestartButton;

    [Header("��Ϸʤ��UI")]
    public GameObject gameWinPanel;
    public TextMeshProUGUI gameWinDistanceText;
    public Button gameWinRestartButton;

    [Header("��Ϸ��UI")]
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI timeText;
    public Slider progressSlider;

    void Start()
    {
        InitializeUI();
        BindButtonEvents();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // ����UI���
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // ����UI��ʾ
        if (distanceText != null) distanceText.text = "����: 0.0m";
        if (timeText != null && TimeManager.Instance != null)
            timeText.text = $"ʱ��: {TimeManager.Instance.GetFormattedTime()}";
        if (progressSlider != null) progressSlider.value = 0f;
    }

    void BindButtonEvents()
    {
        // �����¿�ʼ��ť�¼�
        if (gameOverRestartButton != null)
            gameOverRestartButton.onClick.AddListener(RestartGame);

        if (gameWinRestartButton != null)
            gameWinRestartButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        UpdateInGameUI();
    }

    void UpdateInGameUI()
    {
        // ֻ������Ϸ�����вŸ���UI
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        // ���¾�����ʾ
        if (distanceText != null && GameProgressTracker.Instance != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            distanceText.text = $"����: {distance:F1}m";
        }

        // ����ʱ����ʾ
        if (timeText != null && TimeManager.Instance != null)
        {
            timeText.text = $"ʱ��: {TimeManager.Instance.GetFormattedTime()}";
        }

        // ���½�����
        if (progressSlider != null && GameProgressTracker.Instance != null)
        {
            progressSlider.value = GameProgressTracker.Instance.GetProgressPercentage();
        }
    }

    // ��ʾ��Ϸ��������
    public void ShowGameOver(float distance)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverDistanceText != null)
            {
                gameOverDistanceText.text = $"�ƶ�����: {distance:F1}m";
            }

            // ȷ����ť����
            SetButtonsInteractable(true);
        }
    }

    // ��ʾ��Ϸʤ������
    public void ShowGameWin(float distance)
    {
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
            if (gameWinDistanceText != null)
            {
                gameWinDistanceText.text = $"�ƶ�����: {distance:F1}m";
            }

            // ȷ����ť����
            SetButtonsInteractable(true);
        }
    }

    // ���ð�ť����״̬
    void SetButtonsInteractable(bool interactable)
    {
        if (gameOverRestartButton != null)
            gameOverRestartButton.interactable = interactable;
        if (gameWinRestartButton != null)
            gameWinRestartButton.interactable = interactable;
    }

    // ���¿�ʼ��Ϸ
    void RestartGame()
    {
        // ��ֹ��ε��
        SetButtonsInteractable(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("GameManagerʵ��δ�ҵ���");
            // ���÷�����ֱ�����¼��س���
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}