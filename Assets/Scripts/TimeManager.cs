using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("ʱ������")]
    public float totalTimeSeconds = 300f; // 5���ӵ���ʱ
    public float timeDecreaseRate = 1f; // ʱ������ٶȣ���ʵ1�� = ��Ϸ����1�룩

    // �Ƴ�����ֶΣ���ΪUIManager�ᴦ��UI��ʾ
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

        Debug.Log($"TimeManager��ʼ��: ��ʱ��={totalTimeSeconds}, ʵ��={(Instance != null ? "����" : "null")}");
    }

    void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        // ֻ������ƶ�ʱ��ʱ��ż���
        if (isPlayerMoving)
        {
            // ��ӵ�����Ϣ
            Debug.Log($"ʱ�������: {currentTime} -> {currentTime - timeDecreaseRate * Time.deltaTime}");

            currentTime -= timeDecreaseRate * Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                Debug.Log("ʱ��ľ���");
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
            Debug.Log("���δ�ƶ���ʱ�䲻����");
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

    // �Ƴ�UpdateTimeDisplay��������ΪUIManager�ᴦ����ʾ

    // ��ȡ��ʽ��ʱ��ķ�������UIManagerʹ�ã�
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
        Debug.Log("ʱ��ľ�����Ϸ����");
    }

    public void AddTime(float seconds)
    {
        currentTime += seconds;
        if (currentTime > totalTimeSeconds)
            currentTime = totalTimeSeconds;
    }

    // ����ʱ�䣨�������¿�ʼ��Ϸ��
    public void ResetTime()
    {
        currentTime = totalTimeSeconds;
        isGameActive = true;
        isPlayerMoving = false;
    }

    // ��ͣ/�ָ�ʱ�䣨������Ϸ״̬����
    public void SetGameActive(bool active)
    {
        isGameActive = active;
    }

    // ��ȡ��ǰʱ�䣨������ϵͳʹ�ã�
    public float GetCurrentTime()
    {
        return currentTime;
    }

    // ��ȡʱ��ٷֱȣ�0-1��
    public float GetTimePercentage()
    {
        return Mathf.Clamp01(currentTime / totalTimeSeconds);
    }
}