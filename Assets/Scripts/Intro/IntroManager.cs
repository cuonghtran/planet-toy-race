using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public Image SoundButtonImage;
    public Sprite soundOnImage;
    public Sprite soundOffImage;
    bool soundOn;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        Invoke(nameof(Theme), 0.5f);
    }

    void Theme()
    {
        AudioManager.SharedInstance.PlayTheme("Theme");
    }

    public void OnPlayButton_Click()
    {
        SceneController.Instance.FadeAndLoadScene(SceneList.Level1);
    }

    public void OnToggleSound_Click()
    {
        ToggleSound();
    }

    void ToggleSound()
    {
        soundOn = !soundOn;
        if (soundOn)
            SoundButtonImage.sprite = soundOnImage;
        else SoundButtonImage.sprite = soundOffImage;

        GameManager.SharedInstance.SetSound(soundOn);
    }
}
