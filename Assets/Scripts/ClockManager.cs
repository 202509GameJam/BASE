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
        // 设置之前时钟的旋转状态
        if (currentPlayerClock != null)
        {
            currentPlayerClock.SetRotationActive(false);
        }

        // 设置新时钟为当前时钟
        currentPlayerClock = clock;

        // 新时钟的分针静止（即使玩家移动）
        if (currentPlayerClock != null)
        {
            currentPlayerClock.SetRotationActive(false);
        }

        // 更新其他时钟的旋转状态
        UpdateOtherClocksRotation();
    }

    public void PlayerExitedClock(ClockZone clock)
    {
        if (currentPlayerClock == clock)
        {
            currentPlayerClock = null;
            // 玩家不在任何时钟中，所有时钟都根据移动状态旋转
            UpdateAllClocksRotation();
        }
    }

    void Update()
    {
        // 如果玩家不在任何时钟中，根据移动状态更新所有时钟
        if (currentPlayerClock == null)
        {
            UpdateAllClocksRotation();
        }
        else
        {
            // 玩家在某个时钟中，只更新其他时钟
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
                // 当前时钟始终保持静止
                clock.SetRotationActive(false);
            }
        }
    }
}