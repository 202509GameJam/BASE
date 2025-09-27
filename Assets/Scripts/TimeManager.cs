using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("时间设置")]
    public float totalTimeSeconds = 300f; // 5分钟倒计时
    public float timeDecreaseRate = 1f; // 时间减少速度（现实1秒 = 游戏减少1秒）

    [Header("UI引用")]
    public Text timeDisplay;
    public GameObject gameOverPanel;

    private float currentTime;
    private bool isPlayerMoving = false;
    private bool isGameActive = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentTime = totalTimeSeconds;
        UpdateTimeDisplay();
    }

    void Update()
    {
        if (!isGameActive) return;

        // 只有玩家移动时，时间才减少
        if (isPlayerMoving)
        {
            currentTime -= timeDecreaseRate * Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver();
            }

            UpdateTimeDisplay();
        }
    }

    public void SetPlayerMoving(bool moving)
    {
        isPlayerMoving = moving;
    }

    public bool IsPlayerMoving()
    {
        return isPlayerMoving;
    }

    void UpdateTimeDisplay()
    {
        if (timeDisplay != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (currentTime > totalTimeSeconds)
            currentTime = totalTimeSeconds;
    }
}