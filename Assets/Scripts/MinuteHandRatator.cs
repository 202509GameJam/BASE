using UnityEngine;

public class MinuteHandRotator : MonoBehaviour
{
    [Header("��ת����")]
    public float rotationSpeed = 30f;
    public float startAngle = 0f; // ��������ʼ�Ƕ�
    public bool randomStartAngle = false; // �������Ƿ������ʼ�Ƕ�

    private bool isRotationActive = true;
    private float currentRotation = 0f;

    void Start()
    {
        // ���������ó�ʼ�Ƕ�
        if (randomStartAngle)
        {
            startAngle = Random.Range(0f, 360f);
        }
        currentRotation = startAngle;
        ApplyRotation(); // ����Ӧ�ó�ʼ�Ƕ�
    }

    void Update()
    {
        if (isRotationActive)
        {
            // ֻ�м���ʱ����ת
            currentRotation += rotationSpeed * Time.deltaTime;
            if (currentRotation >= 360f)
            {
                currentRotation -= 360f;
            }

            ApplyRotation();
        }
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, -currentRotation);
    }

    public void SetRotationActive(bool active)
    {
        isRotationActive = active;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    // ���������������ض��Ƕ�
    public void SetRotation(float angle)
    {
        currentRotation = angle % 360f;
        ApplyRotation();
    }

    // ������������ȡ��ǰ�Ƕ�
    public float GetCurrentRotation()
    {
        return currentRotation;
    }
}