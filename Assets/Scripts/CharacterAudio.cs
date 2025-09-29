using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudio : MonoBehaviour
{
    private AudioSource audioSource; // ����������Ч
    public AudioClip[] footstepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip transClip;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // �Ų���
    public void PlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[index];
            audioSource.PlayOneShot(clip);
        }
    }

    // ������
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

    // �����
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            audioSource.PlayOneShot(landClip);
        }
    }
}
