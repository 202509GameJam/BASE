using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("游戏结束UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverDistanceText;
    public Button gameOverRestartButton;

    [Header("游戏胜利UI")]
    public GameObject gameWinPanel;
    public TextMeshProUGUI gameWinDistanceText;
    public Button gameWinRestartButton;

    [Header("游戏内UI")]
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
        // 隐藏UI面板
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // 重置UI显示
        if (distanceText != null) distanceText.text = "距离: 0.0m";
        if (timeText != null && TimeManager.Instance != null)
            timeText.text = $"时间: {TimeManager.Instance.GetFormattedTime()}";
        if (progressSlider != null) progressSlider.value = 0f;
    }

    void BindButtonEvents()
    {
        // 绑定重新开始按钮事件
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
        // 只有在游戏进行中才更新UI
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        // 更新距离显示
        if (distanceText != null && GameProgressTracker.Instance != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            distanceText.text = $"距离: {distance:F1}m";
        }

        // 更新时间显示
        if (timeText != null && TimeManager.Instance != null)
        {
            timeText.text = $"时间: {TimeManager.Instance.GetFormattedTime()}";
        }

        // 更新进度条
        if (progressSlider != null && GameProgressTracker.Instance != null)
        {
            progressSlider.value = GameProgressTracker.Instance.GetProgressPercentage();
        }
    }

    // 显示游戏结束界面
    public void ShowGameOver(float distance)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverDistanceText != null)
            {
                gameOverDistanceText.text = $"移动距离: {distance:F1}m";
            }

            // 确保按钮可用
            SetButtonsInteractable(true);
        }
    }

    // 显示游戏胜利界面
    public void ShowGameWin(float distance)
    {
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
            if (gameWinDistanceText != null)
            {
                gameWinDistanceText.text = $"移动距离: {distance:F1}m";
            }

            // 确保按钮可用
            SetButtonsInteractable(true);
        }
    }

    // 设置按钮交互状态
    void SetButtonsInteractable(bool interactable)
    {
        if (gameOverRestartButton != null)
            gameOverRestartButton.interactable = interactable;
        if (gameWinRestartButton != null)
            gameWinRestartButton.interactable = interactable;
    }

    // 重新开始游戏
    void RestartGame()
    {
        // 防止多次点击
        SetButtonsInteractable(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            Debug.LogError("GameManager实例未找到！");
            // 备用方案：直接重新加载场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}