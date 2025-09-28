using UnityEngine;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [Header("���������")]
    public List<RespawnZone> respawnZones = new List<RespawnZone>();
    public int defaultZoneID = 0; // Ĭ����ʼ����

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
        // ���б�ת��Ϊ�ֵ��Ա���ٲ���
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

        // ����Ҳ���ָ�����򣬷���Ĭ������
        if (zoneDictionary.ContainsKey(defaultZoneID))
        {
            return zoneDictionary[defaultZoneID].respawnPoint.position;
        }

        // �����Ĭ������û�У���������ԭ��
        return Vector3.zero;
    }

    public RespawnZone GetZone(int zoneID)
    {
        return zoneDictionary.ContainsKey(zoneID) ? zoneDictionary[zoneID] : null;
    }
}