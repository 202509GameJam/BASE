using UnityEngine;

public class MinuteHandRotator : MonoBehaviour
{
    [Header("旋转设置")]
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
        // 在对象启用时应用初始旋转（包括编辑模式）
        ApplyInitialRotation();
    }

    void Update()
    {
        if (isRotationActive && isPlaying)
        {
            // 只有游戏运行时才更新旋转
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
        // 应用初始角度
        if (randomStartAngle && Application.isPlaying)
        {
            // 只在游戏运行时随机角度
            startAngle = Random.Range(0f, 360f);
        }

        currentRotation = startAngle;
        ApplyRotation();
    }

    void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0, 0, -currentRotation);
    }

    // 当Inspector中的值改变时调用（编辑模式下）
    void OnValidate()
    {
        // 如果不是在播放模式，立即应用角度变化
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