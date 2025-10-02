using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    [Header("游戏边界")]
    public float rightBoundary = 50f;

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
    private bool respawnInvincible = false; // 新增：复活无敌状态

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
        if (zoneID > currentZoneID)
        {
            currentRespawnPoint = position;
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
        }
        else
        {
            Debug.Log($"保持当前复活点区域{currentZoneID}，忽略区域{zoneID}");
        }
    }

    public int GetCurrentZoneID()
    {
        return currentZoneID;
    }

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
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        CheckGrounded();
        HandleInput();
        CheckMovementStatus();
        HandleJump();

        // 只有在不处于复活无敌状态时才检查屏幕边界
        if (!respawnInvincible)
        {
            CheckScreenBounds();
        }

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

    void CheckRightBoundary()
    {
        if (transform.position.x >= rightBoundary)
        {
            if (GameManager.Instance != null)
            {
                Debug.Log($"到达右边界 {rightBoundary}，游戏胜利");
                GameManager.Instance.GameWin();
            }
        }
    }

    void Respawn()
    {
        if (isRespawning) return; // 防止重复触发复活

        isRespawning = true;
        respawnInvincible = true; // 开启复活无敌状态

        // 完全停止物理运动
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true; // 暂时关闭物理影响

        Vector3 respawnPosition = currentRespawnPoint;
        respawnPosition = EnsureSafeRespawnPosition(respawnPosition);

        // 直接设置位置，不使用渐变
        transform.position = respawnPosition;

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
        // 更精确的安全位置检测
        float checkHeight = 10f; // 从更高处开始检测
        Vector3 checkPosition = position + Vector3.up * checkHeight;

        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, checkHeight + 5f, groundLayer);
        if (hit.collider != null)
        {
            // 找到平台表面，放置在稍上方
            return hit.point + Vector2.up * 0.5f;
        }
        else
        {
            // 如果找不到平台，使用备用方案
            Debug.LogWarning("无法找到安全平台，使用备用位置");

            // 寻找场景中的任何平台
            Collider2D[] platforms = Physics2D.OverlapCircleAll(position, 20f, groundLayer);
            if (platforms.Length > 0)
            {
                Collider2D nearestPlatform = platforms[0];
                Vector3 platformTop = new Vector3(
                    nearestPlatform.bounds.center.x,
                    nearestPlatform.bounds.max.y + 0.5f,
                    nearestPlatform.bounds.center.z
                );
                return platformTop;
            }

            return position; // 最后的手段
        }
    }

    System.Collections.IEnumerator EnableControlAfterRespawn()
    {
        // 等待一帧确保位置稳定
        yield return null;

        // 重新启用物理
        rb.isKinematic = false;

        // 短暂等待确保物理稳定
        yield return new WaitForSeconds(0.1f);

        // 强制检查地面状态
        CheckGrounded();

        // 如果仍然不在平台上，进行更积极的调整
        if (!isGrounded)
        {
            Debug.LogWarning("复活后未检测到地面，进行位置调整");

            // 向下发射更长的射线寻找地面
            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            if (groundHit.collider != null)
            {
                transform.position = groundHit.point + Vector2.up * 0.6f;
                yield return new WaitForSeconds(0.1f);
                CheckGrounded();
            }
        }

        // 等待更长时间确保稳定
        yield return new WaitForSeconds(0.3f);

        isRespawning = false;

        // 保持短暂的无敌状态防止立即再次掉落
        yield return new WaitForSeconds(0.5f);
        respawnInvincible = false;

        Debug.Log("复活完成，恢复玩家控制");
    }

    void FixedUpdate()
    {
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
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isRespawning)
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

        // 更精确的地面检测：排除自己
        isGrounded = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                isGrounded = true;
                break;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(currentRespawnPoint, new Vector3(0.8f, 0.8f, 0.8f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(currentRespawnPoint, currentRespawnPoint + Vector3.down * 5f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(rightBoundary, -10, 0), new Vector3(rightBoundary, 10, 0));
        Gizmos.DrawWireCube(new Vector3(rightBoundary, 0, 0), new Vector3(0.2f, 20f, 0.1f));

        // 绘制复活安全检测线
        if (isRespawning)
        {
            Gizmos.color = Color.blue;
            Vector3 checkStart = currentRespawnPoint + Vector3.up * 10f;
            Gizmos.DrawLine(checkStart, checkStart + Vector3.down * 15f);
        }
    }
}