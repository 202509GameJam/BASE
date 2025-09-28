using UnityEngine;

public class GameProgressTracker : MonoBehaviour
{
    public static GameProgressTracker Instance;

    [Header("���ȼ�¼")]
    public float farthestXPosition = 0f;
    public float startXPosition = 0f;

    [Header("��Ϸ�߽�")]
    public float rightBoundary = 174f; // �ұ߽磬���Ｔʤ��

    private PlayerController player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            startXPosition = player.transform.position.x;
            farthestXPosition = startXPosition;
        }
    }

    void Update()
    {
        if (player != null)
        {
            // ������Զ�ƶ�����
            float currentX = player.transform.position.x;
            if (currentX > farthestXPosition)
            {
                farthestXPosition = currentX;
            }

            // ����Ƿ񵽴��ұ߽�
            if (currentX >= rightBoundary)
            {
                GameManager.Instance.GameWin();
            }
        }
    }

    // ��ȡ���Ȱٷֱȣ�0��1��
    public float GetProgressPercentage()
    {
        float totalDistance = rightBoundary - startXPosition;
        float traveledDistance = farthestXPosition - startXPosition;
        return Mathf.Clamp01(traveledDistance / totalDistance);
    }

    // ��ȡʵ���ƶ�����
    public float GetTraveledDistance()
    {
        return farthestXPosition - startXPosition;
    }

    // ���ý���
    public void ResetProgress()
    {
        farthestXPosition = startXPosition;
    }

    // �����µ���ʼλ�ã����ڹؿ��л���
    public void SetStartPosition(float newStartX)
    {
        startXPosition = newStartX;
        farthestXPosition = newStartX;
    }
}