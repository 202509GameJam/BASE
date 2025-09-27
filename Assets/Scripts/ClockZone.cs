using UnityEngine;

public class ClockZone : MonoBehaviour
{
    [Header("时钟区域设置")]
    public bool isLargeClock = false; // 是否是大时钟（有时针）
    public float rotationSpeed = 60f; // 这个时钟的旋转速度

    private MinuteHandRotator minuteHandRotator;
    private bool playerInZone = false;

    void Start()
    {
        // 获取分针旋转脚本
        minuteHandRotator = GetComponentInChildren<MinuteHandRotator>();
        if (minuteHandRotator != null)
        {
            minuteHandRotator.rotationSpeed = rotationSpeed;
        }

        // 注册到时钟管理器
        if (ClockManager.Instance != null)
        {
            ClockManager.Instance.RegisterClock(this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            if (ClockManager.Instance != null)
            {
                ClockManager.Instance.PlayerEnteredClock(this);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            if (ClockManager.Instance != null)
            {
                ClockManager.Instance.PlayerExitedClock(this);
            }
        }
    }

    public void SetRotationActive(bool active)
    {
        if (minuteHandRotator != null)
        {
            minuteHandRotator.SetRotationActive(active);
        }
    }

    public bool IsPlayerInZone()
    {
        return playerInZone;
    }
}