using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("��Ϸ״̬")]
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

            // ȷ����������Ҳ�ڳ����л�ʱ����
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

    // ��������ʱȷ�����й���������
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

    // ȷ����Ҫ�Ĺ���������
    void EnsureManagersExist()
    {
        // �����Щ�����������ڣ��Զ���������
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

    // ��ʼ������������
    void InitializeManagers()
    {
        uiManager = FindObjectOfType<UIManager>();

        // ������Ϸ״̬
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;

        // ȷ��ʱ����������ڻ״̬
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetGameActive(true);
        }
    }

    // ��Ϸ������ʱ��ľ���
    public void GameOver()
    {
        if (currentGameState != GameState.Playing) return;

        currentGameState = GameState.GameOver;
        Time.timeScale = 0f; // ��ͣ��Ϸ

        // ��ʾ��Ϸ����UI
        if (uiManager != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            uiManager.ShowGameOver(distance);
        }
        else
        {
            Debug.LogWarning("UIManagerδ�ҵ����޷���ʾ��Ϸ��������");
        }

        Debug.Log("��Ϸ�����������ƶ�����: " + GameProgressTracker.Instance.GetTraveledDistance());
    }

    // ��Ϸʤ���������ұ߽磩
    public void GameWin()
    {
        if (currentGameState != GameState.Playing) return;

        currentGameState = GameState.GameWin;
        Time.timeScale = 0f; // ��ͣ��Ϸ

        // ��ʾʤ��UI
        if (uiManager != null)
        {
            float distance = GameProgressTracker.Instance.GetTraveledDistance();
            uiManager.ShowGameWin(distance);
        }
        else
        {
            Debug.LogWarning("UIManagerδ�ҵ����޷���ʾʤ������");
        }

        Debug.Log("��Ϸʤ�����ƶ�����: " + GameProgressTracker.Instance.GetTraveledDistance());
    }

    // ���¿�ʼ��Ϸ
    public void RestartGame()
    {
        // ���ý���
        if (GameProgressTracker.Instance != null)
        {
            GameProgressTracker.Instance.ResetProgress();
        }

        // ����ʱ��
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.ResetTime();
        }

        // ������Ϸ״̬
        Time.timeScale = 1f;
        currentGameState = GameState.Playing;

        // ���¼��ص�ǰ����
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ��ͣ��Ϸ
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            currentGameState = GameState.Paused;
            Time.timeScale = 0f;
        }
    }

    // �ָ���Ϸ
    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            currentGameState = GameState.Playing;
            Time.timeScale = 1f;
        }
    }

    // ��ȡ��ǰ��Ϸ״̬
    public GameState GetCurrentGameState()
    {
        return currentGameState;
    }

    // �����Ϸ�Ƿ����ڽ�����
    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing;
    }
}