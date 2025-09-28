using UnityEngine;

public class HandFrictionController : MonoBehaviour
{
    [Header("Ħ������")]
    public PhysicsMaterial2D lowFrictionMaterial;   // �߽Ƕ�ʹ��
    public PhysicsMaterial2D mediumFrictionMaterial; // �еȽǶ�ʹ��  
    public PhysicsMaterial2D highFrictionMaterial;  // �ͽǶ�ʹ��

    [Header("�Ƕ���ֵ")]
    public float highFrictionAngle = 30f;  // ���ڴ˽Ƕ�ʹ�ø�Ħ��
    public float mediumFrictionAngle = 60f; // 30-60��ʹ���е�Ħ��

    private BoxCollider2D handCollider;
    private float currentAngle;

    void Start()
    {
        handCollider = GetComponent<BoxCollider2D>();
        if (handCollider == null)
        {
            Debug.LogError("ָ��û��BoxCollider2D�����");
        }

        // ��ʼӦ���е�Ħ��
        ApplyFrictionBasedOnAngle();
    }

    void Update()
    {
        // ʵʱ����Ħ��ϵ��
        ApplyFrictionBasedOnAngle();
    }

    void ApplyFrictionBasedOnAngle()
    {
        // ��ȡ��ǰָ��Ƕȣ�����ڴ�ֱ����
        currentAngle = GetHandAngle();

        // ���ݽǶ�ѡ��Ħ������
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
        // ��ȡָ������ڴ�ֱ����ĽǶ�
        // 0�� = ��ֱ����, 90�� = ˮƽ
        float angle = transform.eulerAngles.z;

        // ת��Ϊ-180��180�ķ�Χ
        if (angle > 180f)
            angle -= 360f;

        // ȡ����ֵ����Ϊ���ǹ��ĽǶȴ�С�����Ƿ���
        return Mathf.Abs(angle);
    }

    // ������Ϣ
    /*void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 60, 300, 20),
                     $"ָ��Ƕ�: {currentAngle:F1}��, Ħ��ϵ��: {handCollider.sharedMaterial.friction:F2}");
        }
    }*/
}