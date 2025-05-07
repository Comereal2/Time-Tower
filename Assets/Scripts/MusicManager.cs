using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager musicManager;
    public AudioClip dungeonTheme;
    public AudioClip bossTheme;
    public AudioClip mainMenuTheme;

    private void Awake()
    {
        musicManager = this;
    }

    /// <summary>
    /// Changes the music
    /// </summary>
    public void ChangeMusic(AudioClip song)
    {
        musicManager.GetComponent<AudioSource>().clip = song;
    }

    /// <summary>
    /// Plays a one-time audio clip directly to the player
    /// </summary>
    /// <param name="clip"></param>
    public void PlaySound(AudioClip clip)
    {
        //Checks if the current camera has an audio source and if not adds it
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>() == null ? Camera.main.AddComponent<AudioSource>() : Camera.main.GetComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }
}
