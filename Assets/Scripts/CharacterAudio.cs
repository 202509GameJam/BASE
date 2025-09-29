using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    private AudioSource audioSource; // 用来播放音效
    public AudioClip[] footstepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip transClip;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // 脚步声
    public void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[index];
            audioSource.PlayOneShot(clip);
        }
    }

    // 起跳声
    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpClip);
    }

    public void PlayTransSound()
    {
        audioSource.PlayOneShot(transClip);
    }

    public void StopSound()
    {
        audioSource.Stop();
    }

    // 落地声
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            audioSource.PlayOneShot(landClip);
        }
    }
}
