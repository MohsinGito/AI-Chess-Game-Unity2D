using System;
using UnityEngine;
using Utilities.Audio;
using System.Collections;
using UnityEngine.SceneManagement;

public class OverlayUiManager : Singleton<OverlayUiManager>
{

    [field: SerializeField] public SettingsUI SettingsUI { private set; get; }
    [field: SerializeField] public GameObject PreLoaderUi { private set; get; }

    private void Start()
    {
        LoadScene("Menu", () => AudioController.Instance.PlayAudio(AudioName.MENU_BG_MUSIC) );
    }

    public void LoadScene(string _sceneName, Action _onLoadComplete = null)
    {
        StartCoroutine(StartLoading(_sceneName, _onLoadComplete));
    }

    private IEnumerator StartLoading(string _sceneName, Action _onLoadComplete)
    {
        PreLoaderUi.SetActive(true);
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneName);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        PreLoaderUi.SetActive(false);
        _onLoadComplete?.Invoke();
    }

}