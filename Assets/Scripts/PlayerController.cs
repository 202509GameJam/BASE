using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    [Header("��Ϸ�߽�")]
    public float rightBoundary = 50f; // �ұ߽磬���Ｔ��Ϸʤ��

    private Rigidbody2D rb;
    public bool isGrounded;
    private float horizontalInput;
    private Vector2 lastPosition;
    private bool wasMoving = false;

    [Header("������")]
    public float groundCheckRadius = 0.8f;
    public LayerMask groundLayer;
    public Transform groundCheckPoint;

    [Header("��Ļ�߽�����")]
    public float deathY = -10f;
    private bool isRespawning = false;

    [Header("��������")]
    public PhysicsMaterial2D playerPhysicsMaterial;

    private float movementThreshold = 0.01f;
    private float minMovementTime = 0.5f;
    private float movementStartTime;
    private bool isActuallyMoving = false;

    [Header("�����ϵͳ")]
    public int startingZoneID = 0;
    private int currentZoneID = 0;
    private Vector3 currentRespawnPoint;
    private bool hasCustomRespawnPoint = false;

    void Awake()
    {
        // ���þ�̬ʵ��
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

        Debug.Log($"��ʼ�������: ����{currentZoneID}, λ��{currentRespawnPoint}");
    }

    public void SetRespawnPoint(Vector3 position, int zoneID)
    {
        // ֻ�е�������ID���ڵ�ǰ����IDʱ�Ÿ��¸����
        if (zoneID > currentZoneID)
        {
            currentRespawnPoint = position;
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
            //ShowRespawnPointFeedback();
        }
        else
        {
            Debug.Log($"���ֵ�ǰ���������{currentZoneID}����������{zoneID}");
        }
    }

    void ShowRespawnPointFeedback()
    {
        Debug.Log($"������Ѹ���: ����{currentZoneID}, λ��{currentRespawnPoint}");
    }

    public int GetCurrentZoneID()
    {
        return currentZoneID;
    }

    // �����������ֶ����ø���㣨���ڹؿ����õȣ�
    public void ResetRespawnPoint(int zoneID)
    {
        if (RespawnManager.Instance != null)
        {
            currentRespawnPoint = RespawnManager.Instance.GetRespawnPosition(zoneID);
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
            Debug.Log($"�ֶ����ø���㵽����{zoneID}");
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
        // ֻ������Ϸ�����вŴ���������ƶ�
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
            Debug.Log("��ҵ�����Ļ����ʼ����");
            Respawn();
        }
    }

    // ����ұ߽�
    void CheckRightBoundary()
    {
        if (transform.position.x >= rightBoundary)
        {
            // �����ұ߽磬��Ϸʤ��
            if (GameManager.Instance != null)
            {
                Debug.Log($"�����ұ߽� {rightBoundary}����Ϸʤ��");
                GameManager.Instance.GameWin(); // ��������ΪGameWin
            }
        }
    }

    void Respawn()
    {
        isRespawning = true;

        // ֱ��ʹ�ð󶨵ĸ����
        Vector3 respawnPosition = currentRespawnPoint;

        // ȷ������㰲ȫ����ƽ̨�ϣ�
        respawnPosition = EnsureSafeRespawnPosition(respawnPosition);

        transform.position = respawnPosition;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.AddTime(-10f);
            Debug.Log("�۳�10��ʱ����Ϊ�ͷ�");
        }

        StartCoroutine(EnableControlAfterRespawn());

        Debug.Log($"������{currentZoneID}����, λ��: {respawnPosition}");
    }

    Vector3 EnsureSafeRespawnPosition(Vector3 position)
    {
        // ��鸴����Ƿ�ȫ����ƽ̨�ϣ�
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 5f, groundLayer);
        if (hit.collider != null)
        {
            // ����������ƽ̨�Ϸ���ֱ��ʹ��
            return position;
        }
        else
        {
            // �������㲻��ƽ̨�ϣ��ڸ������ΧѰ�Ұ�ȫλ��
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
                    Debug.Log($"�ڸ���㸽���ҵ���ȫλ��: {testPosition}");
                    return testPosition + Vector3.up * 0.5f; // ��΢̧��һ��
                }
            }

            // �����Χ���Ҳ���������ԭʼλ�ã������ֶΣ�
            Debug.LogWarning("�޷��ڸ������Χ�ҵ���ȫλ�ã�ʹ��ԭʼλ��");
            return position;
        }
    }

    System.Collections.IEnumerator EnableControlAfterRespawn()
    {
        yield return new WaitForSeconds(0.1f);

        // ȷ�����վ�ڵ�����
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
        Debug.Log("������ɣ��ָ���ҿ���");
    }

    void FixedUpdate()
    {
        // ֻ������Ϸ�����вŴ��������ƶ�
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

        // ���Ƶ�ǰ�����
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(currentRespawnPoint, new Vector3(0.8f, 0.8f, 0.8f));

        // ���Ƹ���㰲ȫ�����
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(currentRespawnPoint, currentRespawnPoint + Vector3.down * 5f);

        // �����ұ߽�
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(rightBoundary, -10, 0), new Vector3(rightBoundary, 10, 0));
        Gizmos.DrawWireCube(new Vector3(rightBoundary, 0, 0), new Vector3(0.2f, 20f, 0.1f));
    }
}