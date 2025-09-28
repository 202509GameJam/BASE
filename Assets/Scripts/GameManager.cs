using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("游戏状态")]
    public GameState currentGameState = GameState.Playing;

    private UIManager uiManager;

    public enum GameState
    {
        Playing,
        GameOver,
        GameWin,
        Paused
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 确保其他单例也在场景切换时存在
            EnsureManagersExist();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeManagers();
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;
    }

    // 场景加载时确保所有管理器存在
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeManagers();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 确保必要的管理器存在
    void EnsureManagersExist()
    {
        // 如果这些管理器不存在，自动创建它们
        if (TimeManager.Instance == null)
        {
            GameObject timeManagerObj = new GameObject("TimeManager");
            timeManagerObj.AddComponent<TimeManager>();
            timeManagerObj.transform.SetParent(transform);
        }

        if (GameProgressTracker.Instance == null)
        {
            GameObject progressTrackerObj = new GameObject("GameProgressTracker");
            progressTrackerObj.AddComponent<GameProgressTracker>();
            progressTrackerObj.transform.SetParent(transform);
        }
    }

    // 初始化管理器引用
    void InitializeManagers()
    {
        uiManager = FindObjectOfType<UIManager>();

        // 重置游戏状态
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;

        // 确保时间管理器处于活动状态
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetGameActive(true);
        }
    }

    // 游戏结束（时间耗尽）
    public void GameOver()
    {
        if (currentGameState != GameState.Playing) return;

        currentGameState = GameState.GameOver;
        Time.timeScale = 0f; // 暂停游戏

        // 显示游戏结束UI
        if (uiManager != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            uiManager.ShowGameOver(distance);
        }
        else
        {
            Debug.LogWarning("UIManager未找到，无法显示游戏结束界面");
        }

        Debug.Log("游戏结束！最终移动距离: " + GameProgressTracker.Instance.GetTraveledDistance());
    }

    // 游戏胜利（到达右边界）
    public void GameWin()
    {
        if (currentGameState != GameState.Playing) return;

        currentGameState = GameState.GameWin;
        Time.timeScale = 0f; // 暂停游戏

        // 显示胜利UI
        if (uiManager != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            uiManager.ShowGameWin(distance);
        }
        else
        {
            Debug.LogWarning("UIManager未找到，无法显示胜利界面");
        }

        Debug.Log("游戏胜利！移动距离: " + GameProgressTracker.Instance.GetTraveledDistance());
    }

    // 重新开始游戏
    public void RestartGame()
    {
        // 重置进度
        if (GameProgressTracker.Instance != null)
        {
            GameProgressTracker.Instance.ResetProgress();
        }

        // 重置时间
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.ResetTime();
        }

        // 重置游戏状态
        Time.timeScale = 1f;
        currentGameState = GameState.Playing;

        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            currentGameState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    // 恢复游戏
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            currentGameState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    // 获取当前游戏状态
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    // 检查游戏是否正在进行中
    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing;
    }
}