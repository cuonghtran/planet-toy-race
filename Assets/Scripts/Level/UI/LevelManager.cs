using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager SharedInstance;

    [Header("Data:")]
    public CoreData coreData;

    public bool isTransfering;
    public bool gamePaused;
    public float scoreModifier = 1.0f;

    public float timeToActivateTakeOffPad = 10f; // in seconds
    public float timeToIncreaseSpeed = 9;

    int finalScore;
    bool newHighScore = false;
    float buffer = 0.2f;

    private void Awake()
    {
        if (SharedInstance == null)
            SharedInstance = this;

        gamePaused = true;

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        StartCoroutine(StartGame());

        InvokeRepeating(nameof(ModifySpeedAndScore), timeToIncreaseSpeed, timeToIncreaseSpeed);
    }

    public void ModifySpeedAndScore()
    {
        PlayerController_RB.Instance.speed += buffer;
        scoreModifier += buffer;
    }

    public void UpgradeScore()
    {
        scoreModifier += buffer;
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.6f);
        gamePaused = false;
        AudioManager.SharedInstance.Play("Engine2");
    }

    public void AfterDeath()
    {
        //var rng = Random.Range(0, 100);
        //if (rng < 40)
        //    AdsManager.SharedInstance.PlayRewardedVideoAd();

        finalScore = Mathf.RoundToInt(UIManager.SharedInstance.ScoreValue);
        if (finalScore > coreData.Highscore)
        {
            coreData.Highscore = finalScore;
            newHighScore = true;
        }

        SaveLoadSystem.SaveGame(coreData);
        UIManager.SharedInstance.SetUpMenu(finalScore, coreData.Highscore, newHighScore, coreData.SoundOn);
    }
}
