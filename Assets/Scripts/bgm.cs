using UnityEngine;

public class bgm : MonoBehaviour
{
    // 单例模式的实例
    public static bgm instance;

    // 背景音乐音频剪辑
    public AudioClip mainBgm;
    public AudioClip gameBgm;

    public float bgmVolume = 0.5f;

    // 音频源组件
    private AudioSource audioSource;

    void Awake()
    {
        // 单例模式实现
        if (instance == null)
        {
            instance = this;
            // 防止游戏对象在场景切换时被销毁
            DontDestroyOnLoad(gameObject);

            // 添加音频源组件
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true; // 背景音乐循环播放
        }
        else
        {
            // 如果已经存在实例，销毁当前实例
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 开始播放主背景音乐
        audioSource.volume = bgmVolume;
        PlayBgm("mainBgm");

        // 开始时根据当前场景播放背景音乐
        //PlayBgmBasedOnScene();

        // 监听场景加载事件
        //SceneManager.sceneLoaded += OnSceneLoaded;
    }

     void OnDestroy()
    {
        // 移除场景加载事件监听
        //SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 当场景加载完成时调用
    /**
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 根据新加载的场景播放对应的背景音乐
        //PlayBgmBasedOnScene();
    }
    
    根据当前场景播放对应的背景音乐
    private void PlayBgmBasedOnScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        AudioClip clip = GetBgmClipForScene(sceneName);
        if (clip != null)
        {
            if (audioSource.clip != clip)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        else
        {
            Debug.LogError($"未找到场景 {sceneName} 对应的背景音乐剪辑。");
        }
        
        */

    // 播放指定名称的背景音乐
    public void PlayBgm(string bgmName)
    {
        AudioClip clip = GetBgmClip(bgmName);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError($"未找到名为 {bgmName} 的背景音乐剪辑。");
        }
    }

    // 根据名称获取背景音乐剪辑
    private AudioClip GetBgmClip(string name)
    {
        switch (name)
        {
            case "mainBgm":
                return mainBgm;
            case "gameBgm":
                return gameBgm;
            default:
                return null;
        }
    }
}