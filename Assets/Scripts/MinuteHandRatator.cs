using UnityEngine;

public class MinuteHandRotator : MonoBehaviour
{
    [Header("��ת����")]
    public float rotationSpeed = 30f;

    private bool isRotationActive = true;
    private float currentRotation = 0f;

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

            transform.rotation = Quaternion.Euler(0, 0, -currentRotation);
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
}