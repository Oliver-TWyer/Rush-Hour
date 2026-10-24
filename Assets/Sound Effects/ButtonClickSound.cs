using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickSound : MonoBehaviour
{
    [SerializeField] AudioSource sound;

    void PlaySound()
    {
        sound.Play();
    }
}
