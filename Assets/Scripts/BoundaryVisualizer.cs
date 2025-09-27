using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoundaryVisualizer : MonoBehaviour
{
    public Color leftBoundaryColor = Color.red;
    public Color rightBoundaryColor = Color.green;

    void OnDrawGizmos()
    {
        BoundaryManager boundaryManager = FindObjectOfType<BoundaryManager>();

        if (boundaryManager == null || boundaryManager.leftBoundary == null || boundaryManager.rightBoundary == null)
            return;

        Vector3 leftPos = boundaryManager.leftBoundary.position;
        Vector3 rightPos = boundaryManager.rightBoundary.position;

        // ���Ʊ߽���
        Gizmos.color = leftBoundaryColor;
        Gizmos.DrawLine(leftPos + Vector3.up * 50, leftPos + Vector3.down * 50);

        Gizmos.color = rightBoundaryColor;
        Gizmos.DrawLine(rightPos + Vector3.up * 50, rightPos + Vector3.down * 50);

        // ���ƿ��ָʾ
        Gizmos.color = Color.white;
        Gizmos.DrawLine(new Vector3(leftPos.x, 20, 0), new Vector3(rightPos.x, 20, 0));

        // ֻ�ڱ༭���л����ı�
#if UNITY_EDITOR
        Vector3 labelPos = new Vector3((leftPos.x + rightPos.x) / 2, 25, 0);
        Handles.Label(labelPos, $"Width: {boundaryManager.LevelWidth:F1}");
#endif
    }
}