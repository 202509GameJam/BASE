using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float jumpForce = 15f;

    [Header("��Ϸ�߽�")]
    public float rightBoundary = 50f;

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
    private bool respawnInvincible = false; // �����������޵�״̬

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
        if (zoneID > currentZoneID)
        {
            currentRespawnPoint = position;
            currentZoneID = zoneID;
            hasCustomRespawnPoint = true;
        }
        else
        {
            Debug.Log($"���ֵ�ǰ���������{currentZoneID}����������{zoneID}");
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
        if (GameManager.Instance != null && !GameManager.Instance.IsGamePlaying())
            return;

        CheckGrounded();
        HandleInput();
        CheckMovementStatus();
        HandleJump();

        // ֻ���ڲ����ڸ����޵�״̬ʱ�ż����Ļ�߽�
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
            Debug.Log("��ҵ�����Ļ����ʼ����");
            Respawn();
        }
    }

    void CheckRightBoundary()
    {
        if (transform.position.x >= rightBoundary)
        {
            if (GameManager.Instance != null)
            {
                Debug.Log($"�����ұ߽� {rightBoundary}����Ϸʤ��");
                GameManager.Instance.GameWin();
            }
        }
    }

    void Respawn()
    {
        if (isRespawning) return; // ��ֹ�ظ���������

        isRespawning = true;
        respawnInvincible = true; // ���������޵�״̬

        // ��ȫֹͣ�����˶�
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true; // ��ʱ�ر�����Ӱ��

        Vector3 respawnPosition = currentRespawnPoint;
        respawnPosition = EnsureSafeRespawnPosition(respawnPosition);

        // ֱ������λ�ã���ʹ�ý���
        transform.position = respawnPosition;

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
        // ����ȷ�İ�ȫλ�ü��
        float checkHeight = 10f; // �Ӹ��ߴ���ʼ���
        Vector3 checkPosition = position + Vector3.up * checkHeight;

        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, checkHeight + 5f, groundLayer);
        if (hit.collider != null)
        {
            // �ҵ�ƽ̨���棬���������Ϸ�
            return hit.point + Vector2.up * 0.5f;
        }
        else
        {
            // ����Ҳ���ƽ̨��ʹ�ñ��÷���
            Debug.LogWarning("�޷��ҵ���ȫƽ̨��ʹ�ñ���λ��");

            // Ѱ�ҳ����е��κ�ƽ̨
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

            return position; // �����ֶ�
        }
    }

    System.Collections.IEnumerator EnableControlAfterRespawn()
    {
        // �ȴ�һ֡ȷ��λ���ȶ�
        yield return null;

        // ������������
        rb.isKinematic = false;

        // ���ݵȴ�ȷ�������ȶ�
        yield return new WaitForSeconds(0.1f);

        // ǿ�Ƽ�����״̬
        CheckGrounded();

        // �����Ȼ����ƽ̨�ϣ����и������ĵ���
        if (!isGrounded)
        {
            Debug.LogWarning("�����δ��⵽���棬����λ�õ���");

            // ���·������������Ѱ�ҵ���
            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, 20f, groundLayer);
            if (groundHit.collider != null)
            {
                transform.position = groundHit.point + Vector2.up * 0.6f;
                yield return new WaitForSeconds(0.1f);
                CheckGrounded();
            }
        }

        // �ȴ�����ʱ��ȷ���ȶ�
        yield return new WaitForSeconds(0.3f);

        isRespawning = false;

        // ���ֶ��ݵ��޵�״̬��ֹ�����ٴε���
        yield return new WaitForSeconds(0.5f);
        respawnInvincible = false;

        Debug.Log("������ɣ��ָ���ҿ���");
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

        // ����ȷ�ĵ����⣺�ų��Լ�
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

        // ���Ƹ��ȫ�����
        if (isRespawning)
        {
            Gizmos.color = Color.blue;
            Vector3 checkStart = currentRespawnPoint + Vector3.up * 10f;
            Gizmos.DrawLine(checkStart, checkStart + Vector3.down * 15f);
        }
    }
}