using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager SharedInstance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Image SoundButtonImage;
    public Sprite soundOnImage;
    public Sprite soundOffImage;

    [Header("Menu panel")]
    public CanvasGroup menuPanelCG;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    float scoreValue;
    public float ScoreValue { get { return scoreValue; } }
    float baseScoreModifier = 1.0f;
    bool soundOn;

    private void Awake()
    {
        if (SharedInstance == null)
            SharedInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        scoreValue = 0;
    }

    private void Update()
    {
        if (!LevelManager.SharedInstance.isTransfering && !LevelManager.SharedInstance.gamePaused)
        {
            scoreValue += Time.deltaTime * (baseScoreModifier + LevelManager.SharedInstance.scoreModifier);
            scoreText.SetText(Mathf.RoundToInt(scoreValue).ToString());
        }
    }

    public void ToggleSound()
    {
        soundOn = !soundOn;
        if (soundOn)
            SoundButtonImage.sprite = soundOnImage;
        else SoundButtonImage.sprite = soundOffImage;

        GameManager.SharedInstance.SetSound(soundOn);
    }

    public void SetUpMenu(int finalScore, int currentHighscore, bool newHighScore, bool soundOn)
    {
        // disable game score
        scoreText.gameObject.SetActive(false);

        menuPanelCG.alpha = 1;
        menuPanelCG.interactable = true;
        menuPanelCG.blocksRaycasts = true;

        SetSound(soundOn);
        finalScoreText.text = finalScore.ToString();
        highScoreText.SetText("Highscore: " + currentHighscore.ToString());
    }

    void SetSound(bool soundOn)
    {
        if (soundOn)
            SoundButtonImage.sprite = soundOnImage;
        else SoundButtonImage.sprite = soundOffImage;
    }

    public void PlayAgain_Click()
    {
        SceneController.Instance.FadeAndLoadScene(SceneList.Level1);
    }

    public void MenuButton_Click()
    {
        SceneController.Instance.FadeAndLoadScene(SceneList.OpeningScene);
    }
}
