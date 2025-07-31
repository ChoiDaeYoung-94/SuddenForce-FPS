using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace AD
{
    public class SoundManager : SingletonBase<SoundManager>, ISubManager
    {
        [Header("Audio Mixers")]
        [SerializeField] private AudioMixer _audioMixer;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private AudioSource _sfxAudioSource;

        [Header("Audio Clips")]
        public AudioClip BgmLoginClip;
        public AudioClip BgmLobbyClip;
        public AudioClip BgmGameClip;
        public AudioClip SFXUIClickClip;
        public AudioClip SFXUIOkClip;

        public async UniTask InitAsync()
        {
            float bgm = PlayerPrefs.GetFloat("BGM", 1f);
            float sfx = PlayerPrefs.GetFloat("SFX", 1f);

            SetBGMVolume(bgm);
            SetSFXVolume(sfx);
            
            await UniTask.Yield();
        }

        public void Release()
        {
            
        }

        #region Functions
        public void PlayBGM(AudioClip clip)
        {
            _bgmAudioSource.clip = clip;
            _bgmAudioSource.Play();
        }

        public void PauseBGM() => _bgmAudioSource.Pause();

        public void UnpauseBGM()
        {
            string temp_scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            if (temp_scene.Equals(AD.GameConstants.Scene.Login.ToString()))
                PlayBGM(BgmLoginClip);
            else if (temp_scene.Equals(AD.GameConstants.Scene.Lobby.ToString()))
                PlayBGM(BgmLobbyClip);
            else if (temp_scene.Equals(AD.GameConstants.GameScene.DesertHouse.ToString()))
                PlayBGM(BgmGameClip);
        }

        public void PlaySFX(AudioClip clip)
        {
            _sfxAudioSource.clip = clip;
            _sfxAudioSource.Play();
        }

        public void UI_Ok() => PlaySFX(SFXUIOkClip);

        public void UI_Click() => PlaySFX(SFXUIClickClip);

        public void SetBGMVolume(float volume)
        {
            _audioMixer.SetFloat("BGM", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);

            PlayerPrefs.SetFloat("BGM", volume);
        }

        public void SetSFXVolume(float volume)
        {
            _audioMixer.SetFloat("SFX", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);

            PlayerPrefs.SetFloat("SFX", volume);
        }
        #endregion
    }
}