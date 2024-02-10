using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Utilities.Audio;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{

    [SerializeField] private Image soundImage;
    [SerializeField] private Image musicImage;
    [SerializeField] private Sprite enableSprite;
    [SerializeField] private Sprite disableSprite;
    [SerializeField] private CanvasGroup panelFade;

    private bool soundActive;
    private bool musicActive;
    private Dictionary<AudioType, Image> audioTypeImages;
    private Dictionary<AudioType, bool> audioStates;

    private void Start()
    {
        audioStates = new Dictionary<AudioType, bool>();
        audioTypeImages = new Dictionary<AudioType, Image>()
        {
            { AudioType.Sound, soundImage },
            { AudioType.Music, musicImage }
        };

        if (PlayerPrefs.GetInt("First Play") == 1)
        {
            soundActive = PlayerPrefs.GetInt("Sound", 0) == 1;
            musicActive = PlayerPrefs.GetInt("Music", 0) == 1;
            AudioController.Instance.MuteSFX(!soundActive);
            AudioController.Instance.MuteMusic(!musicActive);
        }
        else
        {
            soundActive = musicActive = true;
            PlayerPrefs.SetInt("Sound", 1);
            PlayerPrefs.SetInt("Music", 1);
            PlayerPrefs.SetInt("First Play", 1);
            PlayerPrefs.Save();
        } 
    }

    public void Display()
    {
        audioStates[AudioType.Sound] = soundActive;
        audioStates[AudioType.Music] = musicActive;
        audioTypeImages[AudioType.Sound].sprite = audioStates[AudioType.Sound] ? enableSprite : disableSprite;
        audioTypeImages[AudioType.Music].sprite = audioStates[AudioType.Music] ? enableSprite : disableSprite;

        panelFade.alpha = 0;
        panelFade.gameObject.SetActive(true);
        panelFade.DOFade(1, 0.25f);
    }

    public void Hide()
    {
        panelFade.DOFade(0, 0.25f).OnComplete(() => panelFade.gameObject.SetActive(false));
    }

    public void AudioButtonClick(int audioType)
    {
        AudioType type = (AudioType)audioType;
        ToggleAudioState(type);
    }

    private void ToggleAudioState(AudioType type)
    {
        audioStates[type] = !audioStates[type];
        audioTypeImages[type].sprite = audioStates[type] ? enableSprite : disableSprite;

        switch (type)
        {
            case AudioType.Sound:
                soundActive = audioStates[type];
                AudioController.Instance.MuteSFX(!audioStates[type]);
                SaveInt("Sound", soundActive ? 1 : 0);
                break;
            case AudioType.Music:
                musicActive = audioStates[type];
                AudioController.Instance.MuteMusic(!audioStates[type]);
                SaveInt("Music", musicActive ? 1 : 0);
                break;
        }
    } 

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    private void SaveInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

}

public enum AudioType
{
    Sound = 0,
    Music = 1
}