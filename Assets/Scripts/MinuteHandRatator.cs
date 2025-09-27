using UnityEngine;

public class MinuteHandRotator : MonoBehaviour
{
    [Header("��ת����")]
    public float rotationSpeed = 30f;
    public float startAngle = 0f;
    public bool randomStartAngle = false;

    private bool isRotationActive = true;
    private float currentRotation = 0f;
    private bool isPlaying = false;

    void Start()
    {
        isPlaying = true;
        ApplyInitialRotation();
    }

    void OnEnable()
    {
        // �ڶ�������ʱӦ�ó�ʼ��ת�������༭ģʽ��
        ApplyInitialRotation();
    }

    void Update()
    {
        if (isRotationActive && isPlaying)
        {
            // ֻ����Ϸ����ʱ�Ÿ�����ת
            currentRotation += rotationSpeed * Time.deltaTime;
            if (currentRotation >= 360f)
            {
                currentRotation -= 360f;
            }

            ApplyRotation();
        }
    }

    void ApplyInitialRotation()
    {
        // Ӧ�ó�ʼ�Ƕ�
        if (randomStartAngle && Application.isPlaying)
        {
            // ֻ����Ϸ����ʱ����Ƕ�
            startAngle = Random.Range(0f, 360f);
        }

        currentRotation = startAngle;
        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, -currentRotation);
    }

    // ��Inspector�е�ֵ�ı�ʱ���ã��༭ģʽ�£�
    void OnValidate()
    {
        // ��������ڲ���ģʽ������Ӧ�ýǶȱ仯
        if (!Application.isPlaying)
        {
            ApplyInitialRotation();
        }
    }

    public void SetRotationActive(bool active)
    {
        isRotationActive = active;
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    public void SetRotation(float angle)
    {
        currentRotation = angle % 360f;
        ApplyRotation();
    }

    public float GetCurrentRotation()
    {
        return currentRotation;
    }
}