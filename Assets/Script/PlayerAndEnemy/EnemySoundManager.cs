using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip deadSound;

    // ���� �Ҹ��� ����ϴ� �Լ�
    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
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

