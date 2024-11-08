using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSoundManager : MonoBehaviour
{
    public static BossSoundManager Instance;
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip throwSound;
    public AudioClip deadSound;
    void Awake()
    {
        // Singleton ���� ����
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    // ���� �Ҹ��� ����ϴ� �Լ�
    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    public void PlayThrowSound()
    {
        if (throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
    }


    public void PlayDeadSound()
    {
        if (deadSound != null)
        {
            audioSource.PlayOneShot(deadSound);
        }
    }

}

