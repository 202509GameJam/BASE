using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePoint : MonoBehaviour
{
    [Header("要关掉的贴图")]
    public GameObject closeTile1;
    public GameObject closeTile2;

    [Header("要打开的贴图")]
    public GameObject closeTile3;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检测是否是Player进入
        if (other.CompareTag("Player"))
        {
            if (closeTile1 != null) closeTile1.gameObject.SetActive(false);
            if (closeTile2 != null) closeTile2.gameObject.SetActive(false);
            if (closeTile3 != null) closeTile3.gameObject.SetActive(true);
        }
    }
}
