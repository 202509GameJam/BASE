using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // �ص���ʼ
    public void LoadSceneStart()
    {
        SceneManager.LoadScene(0);
    }

    // �ؿ�
    public void LevelScene()
    {
        SceneManager.LoadScene(1);
    }
}
