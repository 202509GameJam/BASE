using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("����Ŀ��")]
    public Transform target; // ��Ҷ���
    public float smoothSpeed = 0.125f; // ƽ�������ٶ�

    [Header("��������")]
    public bool followX = true; // �Ƿ����X��
    public bool followY = true; // �Ƿ����Y��
    public Vector2 offset = Vector2.zero; // ƫ����

    [Header("�߽�����")]
    public bool useBounds = false;
    public float minX = -10f, maxX = 10f;
    public float minY = -10f, maxY = 10f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.position;

        // ���δָ��Ŀ�꣬�Զ��������
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

        // ����Ŀ��λ��
        Vector3 desiredPosition = target.position;

        // Ӧ�����������
        if (!followX) desiredPosition.x = transform.position.x;
        if (!followY) desiredPosition.y = transform.position.y;

        // Ӧ��ƫ��
        desiredPosition.x += offset.x;
        desiredPosition.y += offset.y;
        desiredPosition.z = initialPosition.z; // ����Z�᲻��

        // Ӧ�ñ߽�����
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // ƽ���ƶ�
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    // �����ã���Scene��ͼ����ʾ��������
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