using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("时间设置")]
    public float totalTimeSeconds = 300f; // 5分钟倒计时
    public float timeDecreaseRate = 1f; // 时间减少速度（现实1秒 = 游戏减少1秒）

    // 移除这个字段，因为UIManager会处理UI显示
    // public Text timeDisplay;

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
        isGameActive = true;
        isPlayerMoving = false;

        Debug.Log($"TimeManager初始化: 总时间={totalTimeSeconds}, 实例={(Instance != null ? "存在" : "null")}");
    }

    void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        // 只有玩家移动时，时间才减少
        if (isPlayerMoving)
        {
            // 添加调试信息
            Debug.Log($"时间减少中: {currentTime} -> {currentTime - timeDecreaseRate * Time.deltaTime}");

            currentTime -= timeDecreaseRate * Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("时间耗尽！");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    GameOver();
                }
            }
        }
        else
        {
            Debug.Log("玩家未移动，时间不减少");
        }
    }

    public void SetPlayerMoving(bool moving)
    {
        isPlayerMoving = moving;
        Debug.Log($"TimeManager.SetPlayerMoving: {moving}");
    }

    public bool IsPlayerMoving()
    {
        return isPlayerMoving;
    }

    // 移除UpdateTimeDisplay方法，因为UIManager会处理显示

    // 获取格式化时间的方法（供UIManager使用）
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        isGameActive = false;
        Time.timeScale = 0;
        Debug.Log("时间耗尽，游戏结束");
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (currentTime > totalTimeSeconds)
            currentTime = totalTimeSeconds;
    }

    // 重置时间（用于重新开始游戏）
    public void ResetTime()
    {
        currentTime = totalTimeSeconds;
        isGameActive = true;
        isPlayerMoving = false;
    }

    // 暂停/恢复时间（用于游戏状态管理）
    public void SetGameActive(bool active)
    {
        isGameActive = active;
    }

    // 获取当前时间（供其他系统使用）
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // 获取时间百分比（0-1）
    public float GetTimePercentage()
    {
        return Mathf.Clamp01(currentTime / totalTimeSeconds);
    }
}