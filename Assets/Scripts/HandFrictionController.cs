using UnityEngine;

public class HandFrictionController : MonoBehaviour
{
    [Header("摩擦设置")]
    public PhysicsMaterial2D lowFrictionMaterial;   // 高角度使用
    public PhysicsMaterial2D mediumFrictionMaterial; // 中等角度使用  
    public PhysicsMaterial2D highFrictionMaterial;  // 低角度使用

    [Header("角度阈值")]
    public float highFrictionAngle = 30f;  // 低于此角度使用高摩擦
    public float mediumFrictionAngle = 60f; // 30-60度使用中等摩擦

    private BoxCollider2D handCollider;
    private float currentAngle;

    void Start()
    {
        handCollider = GetComponent<BoxCollider2D>();
        if (handCollider == null)
        {
            Debug.LogError("指针没有BoxCollider2D组件！");
        }

        // 初始应用中等摩擦
        ApplyFrictionBasedOnAngle();
    }

    void Update()
    {
        // 实时更新摩擦系数
        ApplyFrictionBasedOnAngle();
    }

    void ApplyFrictionBasedOnAngle()
    {
        // 获取当前指针角度（相对于垂直方向）
        currentAngle = GetHandAngle();

        // 根据角度选择摩擦材质
        if (Mathf.Abs(currentAngle) <= highFrictionAngle)
        {
            handCollider.sharedMaterial = highFrictionMaterial;
        }
        else if (Mathf.Abs(currentAngle) <= mediumFrictionAngle)
        {
            handCollider.sharedMaterial = mediumFrictionMaterial;
        }
        else
        {
            handCollider.sharedMaterial = lowFrictionMaterial;
        }
    }

    float GetHandAngle()
    {
        // 获取指针相对于垂直方向的角度
        // 0度 = 垂直向上, 90度 = 水平
        float angle = transform.eulerAngles.z;

        // 转换为-180到180的范围
        if (angle > 180f)
            angle -= 360f;

        // 取绝对值，因为我们关心角度大小而不是方向
        return Mathf.Abs(angle);
    }

    // 调试信息
    /*void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 60, 300, 20),
                     $"指针角度: {currentAngle:F1}°, 摩擦系数: {handCollider.sharedMaterial.friction:F2}");
        }
    }*/
}