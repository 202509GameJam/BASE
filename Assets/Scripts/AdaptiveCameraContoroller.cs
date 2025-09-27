using UnityEngine;

public class AdaptiveCameraController : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target; // ��Ҷ���

    [Header("���������")]
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float minOrthographicSize = 5f;
    public float maxOrthographicSize = 15f;

    [Header("�߽�����")]
    public BoundaryManager boundaryManager;

    [Header("���������")]
    public bool enableHorizontalBounds = true;
    public bool enableVerticalBounds = false; // ������Ϸͨ�������ƴ�ֱ�ƶ�

    private Camera cam;
    private float baseOrthographicSize;

    void Start()
    {
        cam = GetComponent<Camera>();
        baseOrthographicSize = cam.orthographicSize;

        // �Զ�����Ŀ��
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        // �Զ����ұ߽������
        if (boundaryManager == null)
        {
            boundaryManager = FindObjectOfType<BoundaryManager>();
        }

        Set16_9AspectRatio();
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ����Ŀ��λ��
        Vector3 desiredPosition = target.position + offset;

        // Ӧ�ñ߽�����
        if (enableHorizontalBounds && boundaryManager != null)
        {
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                boundaryManager.LeftBoundaryX,
                boundaryManager.RightBoundaryX
            );
        }

        // ����Ӧ�������С
        AdaptCameraSize();

        // ƽ���ƶ�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        smoothedPosition.z = offset.z;

        transform.position = smoothedPosition;
    }

    void AdaptCameraSize()
    {
        if (boundaryManager == null) return;

        // ���ݹؿ��������Ӧ�������С
        float levelWidth = boundaryManager.LevelWidth;
        float targetSize = Mathf.Clamp(levelWidth / 3f, minOrthographicSize, maxOrthographicSize);

        // ƽ�������������С
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

    // �����������ֶ�����������߽�
    public void UpdateCameraBounds()
    {
        if (boundaryManager != null)
        {
            Debug.Log("������߽��Ѹ���");
        }
    }

    // ��Scene��ͼ�п��ӻ�������߽�
    void OnDrawGizmosSelected()
    {
        if (enableHorizontalBounds && boundaryManager != null)
        {
            // �������ұ߽���
            Gizmos.color = Color.yellow;
            Vector3 leftPoint = new Vector3(boundaryManager.LeftBoundaryX, 0, 0);
            Vector3 rightPoint = new Vector3(boundaryManager.RightBoundaryX, 0, 0);
            Gizmos.DrawLine(leftPoint + Vector3.up * 30, leftPoint + Vector3.down * 30);
            Gizmos.DrawLine(rightPoint + Vector3.up * 30, rightPoint + Vector3.down * 30);

            // ����������ӿ�
            Gizmos.color = Color.cyan;
            float cameraHeight = cam != null ? cam.orthographicSize * 2 : 10f;
            float cameraWidth = cameraHeight * (16f / 9f);
            Gizmos.DrawWireCube(transform.position, new Vector3(cameraWidth, cameraHeight, 0.1f));
        }
    }
}