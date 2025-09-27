using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    [Header("�߽�����")]
    public Transform leftBoundary;    // ��߽磨����ǽ��
    public Transform rightBoundary;   // �ұ߽磨δ����Ϸ��������

    [Header("�߽�λ�ã���ʵʱ������")]
    [SerializeField] private float _leftBoundaryX = -15f;
    [SerializeField] private float _rightBoundaryX = 35f;
    [SerializeField] private float boundaryHeight = 60f;

    // �������ԣ����������ű�����
    public float LeftBoundaryX
    {
        get => _leftBoundaryX;
        set { _leftBoundaryX = value; UpdateBoundaryPositions(); }
    }

    public float RightBoundaryX
    {
        get => _rightBoundaryX;
        set { _rightBoundaryX = value; UpdateBoundaryPositions(); }
    }

    public float LevelWidth => _rightBoundaryX - _leftBoundaryX;

    void Start()
    {
        // ȷ���߽���ȷ����
        if (leftBoundary != null && rightBoundary != null)
        {
            SetupBoundaries();
        }
        else
        {
            Debug.LogWarning("�߽�����δ���ã�");
        }
    }

    // �ؼ��޸�����Inspectorֵ�ı�ʱ��������
    void OnValidate()
    {
        // ��ʹ�ڱ༭ģʽ��Ҳ���±߽�λ��
        if (leftBoundary != null && rightBoundary != null)
        {
            UpdateBoundaryPositions();
        }
    }

    void Update()
    {
        // �ڱ༭ģʽ��ʵʱ���±߽�λ��
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateBoundaryPositions();
        }
#endif
    }

    void SetupBoundaries()
    {
        // ������߽�Ϊʵ����ײ��������ǽ��
        BoxCollider2D leftCollider = leftBoundary.GetComponent<BoxCollider2D>();
        if (leftCollider != null)
        {
            leftCollider.isTrigger = false;
        }

        // �����ұ߽�Ϊ��������δ����Ϸ��������
        BoxCollider2D rightCollider = rightBoundary.GetComponent<BoxCollider2D>();
        if (rightCollider != null)
        {
            rightCollider.isTrigger = true;
        }

        UpdateBoundaryPositions();
    }

    void UpdateBoundaryPositions()
    {
        // ������߽�λ�úʹ�С
        if (leftBoundary != null)
        {
            leftBoundary.position = new Vector3(_leftBoundaryX, 0, 0);
            BoxCollider2D collider = leftBoundary.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, boundaryHeight);
            }

            // ������Ϣ
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log($"��߽���µ�: {_leftBoundaryX}");
            }
#endif
        }

        // �����ұ߽�λ�úʹ�С
        if (rightBoundary != null)
        {
            rightBoundary.position = new Vector3(_rightBoundaryX, 0, 0);
            BoxCollider2D collider = rightBoundary.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, boundaryHeight);
            }

            // ������Ϣ
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log($"�ұ߽���µ�: {_rightBoundaryX}");
            }
#endif
        }
    }

    // ���������������߽�λ��
    public void SetBoundaries(float newLeftX, float newRightX)
    {
        _leftBoundaryX = newLeftX;
        _rightBoundaryX = newRightX;
        UpdateBoundaryPositions();

        Debug.Log($"�߽��Ѹ���: ��={newLeftX}, ��={newRightX}, ���={LevelWidth}");
    }

    // ��֤�߽������Ƿ����
    public bool ValidateBoundaries()
    {
        if (_leftBoundaryX >= _rightBoundaryX)
        {
            Debug.LogError("��߽粻�ܴ��ڻ�����ұ߽磡");
            return false;
        }

        if (LevelWidth < 10f)
        {
            Debug.LogWarning("�ؿ���Ƚ�С������Ӱ����Ϸ����");
        }

        return true;
    }
}