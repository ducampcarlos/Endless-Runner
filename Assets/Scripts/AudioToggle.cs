using UnityEngine;

public class AudioToggle : MonoBehaviour
{
    public void ToggleSFX()
    {
        AudioManager.Instance.ToggleSFX();
    }

    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
    }
}
