using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    public GameObject sceneControllerCamera;
    public CanvasGroup faderCanvasGroup;

    //public event Action<BundleObject> BeforeSceneUnload;
    //public event Action<BundleObject> AfterSceneLoad;

    public string startingSceneName = SceneList.OpeningScene;
    //public BundleObject bundleObject;
    private float fadeDuration = .5f;
    private bool isFading;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        // Start off game with black screen
        faderCanvasGroup.alpha = 1f;

        //BeforeSceneUnload += SaveLoadSystem.SavePlayerData;
        //AfterSceneLoad += SaveLoadSystem.LoadPlayerData;

        // Start first scene
        yield return StartCoroutine(FadeAndSwitchScenes(startingSceneName));

        StartCoroutine(Fade(0f));
    }

    public void FadeAndLoadScene(string sceneName)
    {
        if (!isFading)
            StartCoroutine(FadeAndSwitchScenes(sceneName));
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));

        sceneControllerCamera.SetActive(true);
        //BeforeSceneUnload?.Invoke(bundleObject);

        yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);

        yield return StartCoroutine(LoadSceneAndSetActive(sceneName));
        //AfterSceneLoad?.Invoke(bundleObject);

        if (sceneName != startingSceneName)
        {
            var loadTime = UnityEngine.Random.Range(0.3f, 0.5f);
            yield return new WaitForSeconds(loadTime);
        }

        sceneControllerCamera.SetActive(false);
        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    private IEnumerator Fade(float finalAlpha)
    {
        isFading = true;
        faderCanvasGroup.blocksRaycasts = true;

        float fadeSpeed = Mathf.Abs(faderCanvasGroup.alpha - finalAlpha) / fadeDuration;
        while (!Mathf.Approximately(faderCanvasGroup.alpha, finalAlpha))
        {
            faderCanvasGroup.alpha = Mathf.MoveTowards(faderCanvasGroup.alpha, finalAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        isFading = false;
        faderCanvasGroup.blocksRaycasts = false;
    }

    public string GetActiveSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    private void OnDisable()
    {
        //BeforeSceneUnload -= SaveLoadSystem.SavePlayerData;
        //AfterSceneLoad -= SaveLoadSystem.LoadPlayerData;
    }
}
