using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 回到开始
    public void LoadSceneStart()
    {
        SceneManager.LoadScene(0);
    }

    // 重开
    public void LevelScene()
    {
        SceneManager.LoadScene(1);
    }
}
