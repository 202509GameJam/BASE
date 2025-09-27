using UnityEngine;

public class ClockZone : MonoBehaviour
{
    [Header("ʱ����������")]
    public bool isLargeClock = false; // �Ƿ��Ǵ�ʱ�ӣ���ʱ�룩
    public float rotationSpeed = 60f; // ���ʱ�ӵ���ת�ٶ�

    private MinuteHandRotator minuteHandRotator;
    private bool playerInZone = false;

    void Start()
    {
        // ��ȡ������ת�ű�
        minuteHandRotator = GetComponentInChildren<MinuteHandRotator>();
        if (minuteHandRotator != null)
        {
            minuteHandRotator.rotationSpeed = rotationSpeed;
        }

        // ע�ᵽʱ�ӹ�����
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