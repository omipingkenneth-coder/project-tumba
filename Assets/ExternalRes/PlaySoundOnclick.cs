using UnityEngine;

public class PlaySoundOnClick : MonoBehaviour
{
    public AudioSource audioSource; // Assign in Inspector

    public void PlaySound()
    {
        if (audioSource != null)
            audioSource.Play();
    }
}
