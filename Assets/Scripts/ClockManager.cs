using UnityEngine;
using System.Collections.Generic;

public class ClockManager : MonoBehaviour
{
    public static ClockManager Instance;

    private List<ClockZone> allClocks = new List<ClockZone>();
    private ClockZone currentPlayerClock = null;

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

    public void RegisterClock(ClockZone clock)
    {
        if (!allClocks.Contains(clock))
        {
            allClocks.Add(clock);
        }
    }

    public void PlayerEnteredClock(ClockZone clock)
    {
        // ����֮ǰʱ�ӵ���ת״̬
        if (currentPlayerClock != null)
        {
            currentPlayerClock.SetRotationActive(false);
        }

        // ������ʱ��Ϊ��ǰʱ��
        currentPlayerClock = clock;

        // ��ʱ�ӵķ��뾲ֹ����ʹ����ƶ���
        if (currentPlayerClock != null)
        {
            currentPlayerClock.SetRotationActive(false);
        }

        // ��������ʱ�ӵ���ת״̬
        UpdateOtherClocksRotation();
    }

    public void PlayerExitedClock(ClockZone clock)
    {
        if (currentPlayerClock == clock)
        {
            currentPlayerClock = null;
            // ��Ҳ����κ�ʱ���У�����ʱ�Ӷ������ƶ�״̬��ת
            UpdateAllClocksRotation();
        }
    }

    void Update()
    {
        // �����Ҳ����κ�ʱ���У������ƶ�״̬��������ʱ��
        if (currentPlayerClock == null)
        {
            UpdateAllClocksRotation();
        }
        else
        {
            // �����ĳ��ʱ���У�ֻ��������ʱ��
            UpdateOtherClocksRotation();
        }
    }

    void UpdateAllClocksRotation()
    {
        bool isPlayerMoving = TimeManager.Instance != null && TimeManager.Instance.IsPlayerMoving();

        foreach (ClockZone clock in allClocks)
        {
            clock.SetRotationActive(isPlayerMoving);
        }
    }

    void UpdateOtherClocksRotation()
    {
        bool isPlayerMoving = TimeManager.Instance != null && TimeManager.Instance.IsPlayerMoving();

        foreach (ClockZone clock in allClocks)
        {
            if (clock != currentPlayerClock)
            {
                clock.SetRotationActive(isPlayerMoving);
            }
            else
            {
                // ��ǰʱ��ʼ�ձ��־�ֹ
                clock.SetRotationActive(false);
            }
        }
    }
}