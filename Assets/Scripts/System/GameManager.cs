using System.IO;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager SharedInstance;

    public CoreData coreData;
    private readonly string firstPlay = "FirstPlay";
    public readonly string soundTogglePref = "SoundToggle";
    public AudioMixerGroup mainMixerGroup;

    private void Awake()
    {
        if (SharedInstance == null)
            SharedInstance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitDataOnOpeningGame();
    }

    void InitDataOnOpeningGame()
    {
        try
        {
            MusicSettings();
            SaveLoadSystem.LoadGame(coreData);
        }
        catch (FileNotFoundException e)
        {
            InitPlayerData();
            FirstPlayMusicSettings();
        }
    }

    void InitPlayerData()
    {
        coreData.Highscore = 0;
        coreData.SoundOn = true;
    }

    public void FirstPlayMusicSettings()
    {
        coreData.SoundOn = true;
        mainMixerGroup.audioMixer.SetFloat("Volume", 0);
        PlayerPrefs.SetInt(soundTogglePref, 1);
    }

    public void MusicSettings()
    {
        var soundValue = coreData.SoundOn;
        if (soundValue)
        {
            mainMixerGroup.audioMixer.SetFloat("Volume", 0);
            coreData.SoundOn = true;
        }
        else
        {
            mainMixerGroup.audioMixer.SetFloat("Volume", -80);
            coreData.SoundOn = false;
        }
    }

    public void SetSound(bool soundOn)
    {
        if (soundOn)
        {
            mainMixerGroup.audioMixer.SetFloat("Volume", 0);
            coreData.SoundOn = true;
        }
        else
        {
            mainMixerGroup.audioMixer.SetFloat("Volume", -80);
            coreData.SoundOn = false;
        }
    }
}
