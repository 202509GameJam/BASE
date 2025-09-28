using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("复活点设置")]
    public List<RespawnZone> respawnZones = new List<RespawnZone>();
    public int defaultZoneID = 0; // 默认起始区域

    private Dictionary<int, RespawnZone> zoneDictionary = new Dictionary<int, RespawnZone>();

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

        InitializeZones();
    }

    void InitializeZones()
    {
        // 将列表转换为字典以便快速查找
        foreach (RespawnZone zone in respawnZones)
        {
            if (!zoneDictionary.ContainsKey(zone.zoneID))
            {
                zoneDictionary.Add(zone.zoneID, zone);
            }
        }
    }

    public void RegisterZone(RespawnZone zone)
    {
        if (!respawnZones.Contains(zone))
        {
            respawnZones.Add(zone);
            zoneDictionary[zone.zoneID] = zone;
        }
    }

    public Vector3 GetRespawnPosition(int zoneID)
    {
        if (zoneDictionary.ContainsKey(zoneID))
        {
            return zoneDictionary[zoneID].respawnPoint.position;
        }

        // 如果找不到指定区域，返回默认区域
        if (zoneDictionary.ContainsKey(defaultZoneID))
        {
            return zoneDictionary[defaultZoneID].respawnPoint.position;
        }

        // 如果连默认区域都没有，返回世界原点
        return Vector3.zero;
    }

    public RespawnZone GetZone(int zoneID)
    {
        return zoneDictionary.ContainsKey(zoneID) ? zoneDictionary[zoneID] : null;
    }
}