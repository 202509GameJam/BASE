using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("ʱ������")]
    public float totalTimeSeconds = 300f; // 5���ӵ���ʱ
    public float timeDecreaseRate = 1f; // ʱ������ٶȣ���ʵ1�� = ��Ϸ����1�룩

    [Header("UI����")]
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

        // ֻ������ƶ�ʱ��ʱ��ż���
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