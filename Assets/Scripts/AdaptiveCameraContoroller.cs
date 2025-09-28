using UnityEngine;

public class AdaptiveCameraController : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target; // 玩家对象

    [Header("摄像机设置")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 15f;

    [Header("边界引用")]
    public BoundaryManager boundaryManager;

    [Header("摄像机限制")]
    public bool enableHorizontalBounds = true;
    public bool enableVerticalBounds = false; // 横向游戏通常不限制垂直移动

    private Camera cam;
    private float baseOrthographicSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;

        // 自动查找目标
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // 自动查找边界管理器
        if (boundaryManager == null)
        {
            boundaryManager = FindObjectOfType<BoundaryManager>();
        }

        Set16_9AspectRatio();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 应用边界限制
        if (enableHorizontalBounds && boundaryManager != null)
        {
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                boundaryManager.LeftBoundaryX,
                boundaryManager.RightBoundaryX
            );
        }

        // 自适应摄像机大小
        AdaptCameraSize();

        // 平滑移动
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }

    void AdaptCameraSize()
    {
        if (boundaryManager == null) return;

        // 根据关卡宽度自适应摄像机大小
        float levelWidth = boundaryManager.LevelWidth;
        float targetSize = Mathf.Clamp(levelWidth / 3f, minOrthographicSize, maxOrthographicSize);

        // 平滑调整摄像机大小
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime);
    }

    void Set16_9AspectRatio()
    {
        float targetAspect = 16.0f / 9.0f;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }

    // 公共方法：手动更新摄像机边界
    public void UpdateCameraBounds()
    {
        if (boundaryManager != null)
        {
            Debug.Log("摄像机边界已更新");
        }
    }

    // 在Scene视图中可视化摄像机边界
    void OnDrawGizmosSelected()
    {
        if (enableHorizontalBounds && boundaryManager != null)
        {
            // 绘制左右边界线
            Gizmos.color = Color.yellow;
            Vector3 leftPoint = new Vector3(boundaryManager.LeftBoundaryX, 0, 0);
            Vector3 rightPoint = new Vector3(boundaryManager.RightBoundaryX, 0, 0);
            Gizmos.DrawLine(leftPoint + Vector3.up * 30, leftPoint + Vector3.down * 30);
            Gizmos.DrawLine(rightPoint + Vector3.up * 30, rightPoint + Vector3.down * 30);

            // 绘制摄像机视口
            Gizmos.color = Color.cyan;
            float cameraHeight = cam != null ? cam.orthographicSize * 2 : 10f;
            float cameraWidth = cameraHeight * (16f / 9f);
            Gizmos.DrawWireCube(transform.position, new Vector3(cameraWidth, cameraHeight, 0.1f));
        }
    }
}