using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    [Header("复活点设置")]
    public Transform respawnPoint;
    public int zoneID = 0;

    [Header("视觉设置")]
    public Color zoneColor = new Color(0, 1, 0, 0.3f);

    private BoxCollider2D zoneCollider;

    void Start()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        zoneCollider.isTrigger = true;

        if (respawnPoint == null)
        {
            respawnPoint = transform;
        }

        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.RegisterZone(this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.SetRespawnPoint(respawnPoint.position, zoneID);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = zoneColor;

        Vector3 size = zoneCollider != null ?
            new Vector3(zoneCollider.size.x, zoneCollider.size.y, 0.1f) :
            new Vector3(2, 5, 0.1f);

        Gizmos.DrawWireCube(transform.position, size);
        Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, zoneColor.a * 0.3f);
        Gizmos.DrawCube(transform.position, size);

        if (respawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(respawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, respawnPoint.position);
        }
    }
}