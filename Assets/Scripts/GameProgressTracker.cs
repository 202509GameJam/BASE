using UnityEngine;

public class GameProgressTracker : MonoBehaviour
{
    public static GameProgressTracker Instance;

    [Header("进度记录")]
    public float farthestXPosition = 0f;
    public float startXPosition = 0f;

    [Header("游戏边界")]
    public float rightBoundary = 174f; // 右边界，到达即胜利

    private PlayerController player;

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
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            startXPosition = player.transform.position.x;
            farthestXPosition = startXPosition;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // 更新最远移动距离
            float currentX = player.transform.position.x;
            if (currentX > farthestXPosition)
            {
                farthestXPosition = currentX;
            }

            // 检查是否到达右边界
            if (currentX >= rightBoundary)
            {
                GameManager.Instance.GameWin();
            }
        }
    }

    // 获取进度百分比（0到1）
    public float GetProgressPercentage()
    {
        float totalDistance = rightBoundary - startXPosition;
        float traveledDistance = farthestXPosition - startXPosition;
        return Mathf.Clamp01(traveledDistance / totalDistance);
    }

    // 获取实际移动距离
    public float GetTraveledDistance()
    {
        return farthestXPosition - startXPosition;
    }

    // 重置进度
    public void ResetProgress()
    {
        farthestXPosition = startXPosition;
    }

    // 设置新的起始位置（用于关卡切换）
    public void SetStartPosition(float newStartX)
    {
        startXPosition = newStartX;
        farthestXPosition = newStartX;
    }
}