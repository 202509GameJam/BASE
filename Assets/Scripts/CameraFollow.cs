using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target; // 玩家对象
    public float smoothSpeed = 0.125f; // 平滑跟随速度

    [Header("跟随限制")]
    public bool followX = true; // 是否跟随X轴
    public bool followY = true; // 是否跟随Y轴
    public Vector2 offset = Vector2.zero; // 偏移量

    [Header("边界限制")]
    public bool useBounds = false;
    public float minX = -10f, maxX = 10f;
    public float minY = -10f, maxY = 10f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;

        // 如果未指定目标，自动查找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position;

        // 应用轴跟随限制
        if (!followX) desiredPosition.x = transform.position.x;
        if (!followY) desiredPosition.y = transform.position.y;

        // 应用偏移
        desiredPosition.x += offset.x;
        desiredPosition.y += offset.y;
        desiredPosition.z = initialPosition.z; // 保持Z轴不变

        // 应用边界限制
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // 调试用：在Scene视图中显示跟随区域
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}