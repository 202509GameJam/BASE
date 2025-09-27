using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float horizontalInput;
    private Vector2 lastPosition;
    private bool wasMoving = false;

    [Header("地面检测")]
    public float groundCheckRadius = 0.8f;
    public LayerMask groundLayer;
    public Transform groundCheckPoint;

    [Header("屏幕边界设置")]
    public float deathY = -10f;
    private Vector3 lastSafePosition;
    private bool isRespawning = false;

    private float movementThreshold = 0.01f;
    private float minMovementTime = 0.5f;
    private float movementStartTime;
    private bool isActuallyMoving = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosition = rb.position;
        lastSafePosition = transform.position;

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetPlayerMoving(false);
        }
    }

    void Update()
    {
        CheckGrounded();
        HandleInput();
        CheckMovementStatus();
        HandleJump();
        CheckScreenBounds();
    }

    void CheckScreenBounds()
    {
        if (isRespawning) return;

        if (transform.position.y < deathY)
        {
            Debug.Log("玩家掉出屏幕，开始复活");
            Respawn();
        }

        if (isGrounded)
        {
            UpdateSafePosition();
        }
    }

    void UpdateSafePosition()
    {
        if (isGrounded && IsPositionSafe(transform.position))
        {
            lastSafePosition = transform.position;
            Debug.Log("更新安全位置: " + lastSafePosition);
        }
    }

    bool IsPositionSafe(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 2f, groundLayer);
        return hit.collider != null;
    }

    void Respawn()
    {
        isRespawning = true;
        Vector3 safeRespawnPosition = FindSafeRespawnPosition();
        transform.position = safeRespawnPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.AddTime(-10f);
            Debug.Log("扣除10秒时间作为惩罚");
        }

        StartCoroutine(EnableControlAfterRespawn());
    }

    Vector3 FindSafeRespawnPosition()
    {
        Vector3[] candidatePositions = {
            lastSafePosition,
            FindNearestClockCenter(),
            FindNearestPlatformPosition(),
            Vector3.zero
        };

        foreach (Vector3 candidate in candidatePositions)
        {
            if (IsPositionSafe(candidate))
            {
                Debug.Log("找到安全复活位置: " + candidate);
                return candidate;
            }
        }

        return CreateSafePosition();
    }

    Vector3 FindNearestClockCenter()
    {
        GameObject[] clocks = GameObject.FindGameObjectsWithTag("Clock");
        Vector3 playerPosition = transform.position;
        Vector3 nearestClock = Vector3.zero;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject clock in clocks)
        {
            float distance = Vector3.Distance(playerPosition, clock.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestClock = clock.transform.position;
            }
        }

        return nearestClock;
    }

    Vector3 FindNearestPlatformPosition()
    {
        Collider2D[] platforms = Physics2D.OverlapCircleAll(transform.position, 20f, groundLayer);
        Vector3 nearestPlatform = Vector3.zero;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D platform in platforms)
        {
            Vector3 platformTop = new Vector3(
                platform.bounds.center.x,
                platform.bounds.max.y + 0.5f,
                platform.bounds.center.z
            );

            float distance = Vector3.Distance(transform.position, platformTop);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPlatform = platformTop;
            }
        }

        return nearestPlatform;
    }

    Vector3 CreateSafePosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 testPosition = lastSafePosition + new Vector3(
                Random.Range(-5f, 5f),
                Random.Range(0f, 5f),
                0
            );

            if (IsPositionSafe(testPosition))
            {
                Debug.Log("创建安全位置: " + testPosition);
                return testPosition;
            }
        }

        Debug.LogError("无法找到安全位置，返回场景中心");
        return Vector3.zero;
    }

    System.Collections.IEnumerator EnableControlAfterRespawn()
    {
        yield return new WaitForSeconds(0.1f);

        if (!isGrounded)
        {
            Debug.LogWarning("复活后位置不安全，尝试调整");
            for (int i = 0; i < 5; i++)
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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(lastSafePosition, new Vector3(0.5f, 0.5f, 0.5f));
    }
}