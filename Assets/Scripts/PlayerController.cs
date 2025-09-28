using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    [Header("游戏边界")]
    public float rightBoundary = 50f; // 右边界，到达即游戏胜利

    private Rigidbody2D rb;
    public bool isGrounded;
    private float horizontalInput;
    private Vector2 lastPosition;
    private bool wasMoving = false;

    [Header("地面检测")]
    public float groundCheckRadius = 0.8f;
    public LayerMask groundLayer;
    public Transform groundCheckPoint;

    [Header("屏幕边界设置")]
    public float deathY = -10f;
    private bool isRespawning = false;

    [Header("物理设置")]
    public PhysicsMaterial2D playerPhysicsMaterial;

    private float movementThreshold = 0.01f;
    private float minMovementTime = 0.5f;
    private float movementStartTime;
    private bool isActuallyMoving = false;

    [Header("复活点系统")]
    public int startingZoneID = 0;
    private int currentZoneID = 0;
    private Vector3 currentRespawnPoint;
    private bool hasCustomRespawnPoint = false;

    void Awake()
    {
        // 设置静态实例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
        InitializeRespawnSystem();
        ApplyPlayerPhysicsMaterial();

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetPlayerMoving(false);
        }
    }

    void InitializeRespawnSystem()
    {
        currentZoneID = startingZoneID;

        if (RespawnManager.Instance != null)
        {
            currentRespawnPoint = RespawnManager.Instance.GetRespawnPosition(startingZoneID);
            hasCustomRespawnPoint = true;
        }
        else
        {
            currentRespawnPoint = transform.position;
            hasCustomRespawnPoint = false;
        }

        Debug.Log($"初始化复活点: 区域{currentZoneID}, 位置{currentRespawnPoint}");
    }

    public void SetRespawnPoint(Vector3 position, int zoneID)
    {
        // 只有当新区域ID大于当前区域ID时才更新复活点
        if (zoneID > currentZoneID)
        {
            currentRespawnPoint = position;
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
            //ShowRespawnPointFeedback();
        }
        else
        {
            Debug.Log($"保持当前复活点区域{currentZoneID}，忽略区域{zoneID}");
        }
    }

    void ShowRespawnPointFeedback()
    {
        Debug.Log($"复活点已更新: 区域{currentZoneID}, 位置{currentRespawnPoint}");
    }

    public int GetCurrentZoneID()
    {
        return currentZoneID;
    }

    // 新增方法：手动重置复活点（用于关卡重置等）
    public void ResetRespawnPoint(int zoneID)
    {
        if (RespawnManager.Instance != null)
        {
            currentRespawnPoint = RespawnManager.Instance.GetRespawnPosition(zoneID);
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
            Debug.Log($"手动重置复活点到区域{zoneID}");
        }
    }

    void ApplyPlayerPhysicsMaterial()
    {
        CircleCollider2D playerCollider = GetComponent<CircleCollider2D>();
        if (playerCollider != null && playerPhysicsMaterial != null)
        {
            playerCollider.sharedMaterial = playerPhysicsMaterial;
        }
    }

    void Update()
    {
        // 只有在游戏进行中才处理输入和移动
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        CheckGrounded();
        HandleInput();
        CheckMovementStatus();
        HandleJump();
        CheckScreenBounds();
        CheckRightBoundary();
    }

    void CheckScreenBounds()
    {
        if (isRespawning) return;

        if (transform.position.y < deathY)
        {
            Debug.Log("玩家掉出屏幕，开始复活");
            Respawn();
        }
    }

    // 检查右边界
    void CheckRightBoundary()
    {
        if (transform.position.x >= rightBoundary)
        {
            // 到达右边界，游戏胜利
            if (GameManager.Instance != null)
            {
                Debug.Log($"到达右边界 {rightBoundary}，游戏胜利");
                GameManager.Instance.GameWin(); // 修正：改为GameWin
            }
        }
    }

    void Respawn()
    {
        isRespawning = true;

        // 直接使用绑定的复活点
        Vector3 respawnPosition = currentRespawnPoint;

        // 确保复活点安全（在平台上）
        respawnPosition = EnsureSafeRespawnPosition(respawnPosition);

        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.AddTime(-10f);
            Debug.Log("扣除10秒时间作为惩罚");
        }

        StartCoroutine(EnableControlAfterRespawn());

        Debug.Log($"在区域{currentZoneID}复活, 位置: {respawnPosition}");
    }

    Vector3 EnsureSafeRespawnPosition(Vector3 position)
    {
        // 检查复活点是否安全（在平台上）
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 5f, groundLayer);
        if (hit.collider != null)
        {
            // 如果复活点在平台上方，直接使用
            return position;
        }
        else
        {
            // 如果复活点不在平台上，在复活点周围寻找安全位置
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 testPosition = position + new Vector3(
                    Mathf.Cos(angle) * 2f,
                    Mathf.Sin(angle) * 2f,
                    0
                );

                hit = Physics2D.Raycast(testPosition, Vector2.down, 5f, groundLayer);
                if (hit.collider != null)
                {
                    Debug.Log($"在复活点附近找到安全位置: {testPosition}");
                    return testPosition + Vector3.up * 0.5f; // 稍微抬高一点
                }
            }

            // 如果周围都找不到，返回原始位置（最后的手段）
            Debug.LogWarning("无法在复活点周围找到安全位置，使用原始位置");
            return position;
        }
    }

    System.Collections.IEnumerator EnableControlAfterRespawn()
    {
        yield return new WaitForSeconds(0.1f);

        // 确保玩家站在地面上
        if (!isGrounded)
        {
            for (int i = 0; i < 3; i++)
            {
                transform.position += Vector3.up * 0.5f;
                yield return new WaitForSeconds(0.1f);
                if (isGrounded) break;
            }
        }

        isRespawning = false;
        Debug.Log("复活完成，恢复玩家控制");
    }

    void FixedUpdate()
    {
        // 只有在游戏进行中才处理物理移动
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        HandleMovement();
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void HandleMovement()
    {
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SetPlayerMoving(true);
                movementStartTime = Time.time;
                isActuallyMoving = true;
            }
        }
    }

    void CheckMovementStatus()
    {
        Vector2 currentPosition = rb.position;
        float positionChange = Vector2.Distance(currentPosition, lastPosition);
        bool hasInput = Mathf.Abs(horizontalInput) > 0.1f;
        bool nowMoving = positionChange > movementThreshold || hasInput;

        if (nowMoving && !isActuallyMoving)
        {
            movementStartTime = Time.time;
            isActuallyMoving = true;
        }

        if (!nowMoving && isActuallyMoving)
        {
            if (Time.time - movementStartTime >= minMovementTime)
            {
                isActuallyMoving = false;
            }
        }

        if (isActuallyMoving != wasMoving)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SetPlayerMoving(isActuallyMoving);
            }
            wasMoving = isActuallyMoving;
        }

        lastPosition = currentPosition;
    }

    void CheckGrounded()
    {
        if (groundCheckPoint == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            groundCheckPoint.position,
            groundCheckRadius,
            groundLayer
        );

        isGrounded = colliders.Length > 0;
    }

    void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        // 绘制当前复活点
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(currentRespawnPoint, new Vector3(0.8f, 0.8f, 0.8f));

        // 绘制复活点安全检测线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(currentRespawnPoint, currentRespawnPoint + Vector3.down * 5f);

        // 绘制右边界
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(rightBoundary, -10, 0), new Vector3(rightBoundary, 10, 0));
        Gizmos.DrawWireCube(new Vector3(rightBoundary, 0, 0), new Vector3(0.2f, 20f, 0.1f));
    }
}