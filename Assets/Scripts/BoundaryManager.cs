using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    [Header("边界引用")]
    public Transform leftBoundary;    // 左边界（空气墙）
    public Transform rightBoundary;   // 右边界（未来游戏结束区域）

    [Header("边界位置（可实时调整）")]
    [SerializeField] private float _leftBoundaryX = -15f;
    [SerializeField] private float _rightBoundaryX = 35f;
    [SerializeField] private float boundaryHeight = 60f;

    // 公共属性，便于其他脚本访问
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
        // 确保边界正确设置
        if (leftBoundary != null && rightBoundary != null)
        {
            SetupBoundaries();
        }
        else
        {
            Debug.LogWarning("边界引用未设置！");
        }
    }

    // 关键修复：在Inspector值改变时立即更新
    void OnValidate()
    {
        // 即使在编辑模式下也更新边界位置
        if (leftBoundary != null && rightBoundary != null)
        {
            UpdateBoundaryPositions();
        }
    }

    void Update()
    {
        // 在编辑模式下实时更新边界位置
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UpdateBoundaryPositions();
        }
#endif
    }

    void SetupBoundaries()
    {
        // 设置左边界为实体碰撞器（空气墙）
        BoxCollider2D leftCollider = leftBoundary.GetComponent<BoxCollider2D>();
        if (leftCollider != null)
        {
            leftCollider.isTrigger = false;
        }

        // 设置右边界为触发器（未来游戏结束区域）
        BoxCollider2D rightCollider = rightBoundary.GetComponent<BoxCollider2D>();
        if (rightCollider != null)
        {
            rightCollider.isTrigger = true;
        }

        UpdateBoundaryPositions();
    }

    void UpdateBoundaryPositions()
    {
        // 更新左边界位置和大小
        if (leftBoundary != null)
        {
            leftBoundary.position = new Vector3(_leftBoundaryX, 0, 0);
            BoxCollider2D collider = leftBoundary.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, boundaryHeight);
            }

            // 调试信息
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log($"左边界更新到: {_leftBoundaryX}");
            }
#endif
        }

        // 更新右边界位置和大小
        if (rightBoundary != null)
        {
            rightBoundary.position = new Vector3(_rightBoundaryX, 0, 0);
            BoxCollider2D collider = rightBoundary.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, boundaryHeight);
            }

            // 调试信息
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Debug.Log($"右边界更新到: {_rightBoundaryX}");
            }
#endif
        }
    }

    // 公共方法：调整边界位置
    public void SetBoundaries(float newLeftX, float newRightX)
    {
        _leftBoundaryX = newLeftX;
        _rightBoundaryX = newRightX;
        UpdateBoundaryPositions();

        Debug.Log($"边界已更新: 左={newLeftX}, 右={newRightX}, 宽度={LevelWidth}");
    }

    // 验证边界设置是否合理
    public bool ValidateBoundaries()
    {
        if (_leftBoundaryX >= _rightBoundaryX)
        {
            Debug.LogError("左边界不能大于或等于右边界！");
            return false;
        }

        if (LevelWidth < 10f)
        {
            Debug.LogWarning("关卡宽度较小，可能影响游戏体验");
        }

        return true;
    }
}