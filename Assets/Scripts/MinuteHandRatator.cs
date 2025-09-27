using UnityEngine;

public class MinuteHandRotator : MonoBehaviour
{
    [Header("旋转设置")]
    public float rotationSpeed = 30f;
    public float startAngle = 0f; // 新增：初始角度
    public bool randomStartAngle = false; // 新增：是否随机初始角度

    private bool isRotationActive = true;
    private float currentRotation = 0f;

    void Start()
    {
        // 新增：设置初始角度
        if (randomStartAngle)
        {
            startAngle = Random.Range(0f, 360f);
        }
        currentRotation = startAngle;
        ApplyRotation(); // 立即应用初始角度
    }

    void Update()
    {
        if (isRotationActive)
        {
            // 只有激活时才旋转
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

    // 新增方法：设置特定角度
    public void SetRotation(float angle)
    {
        currentRotation = angle % 360f;
        ApplyRotation();
    }

    // 新增方法：获取当前角度
    public float GetCurrentRotation()
    {
        return currentRotation;
    }
}