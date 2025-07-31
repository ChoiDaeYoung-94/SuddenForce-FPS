using System;
using System.Collections.Generic;
using System.IO;
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
        [SerializeField] private List<AudioSource> _sfxSources = new();

        private int _sfxIndex = 0;
        private Dictionary<BGMType, AudioClip> _bgmClips = new();
        private Dictionary<SFXType, AudioClip> _sfxClips = new();

        public async UniTask InitAsync()
        {
            float bgm = PlayerPrefs.GetFloat("BGM", 1f);
            float sfx = PlayerPrefs.GetFloat("SFX", 1f);

            SetBGMVolume(bgm);
            SetSFXVolume(sfx);

            await LoadSoundTableAsync();
        }

        public void Release() { }

        #region Sound Table Loading
        private async UniTask LoadSoundTableAsync()
        {
            TextAsset csv = Resources.Load<TextAsset>("Table/SoundTable_ByEnum");
            if (csv == null)
            {
                Debug.LogError("SoundTable_ByEnum.csv not found in Resources/Table/");
                return;
            }

            using (StringReader reader = new(csv.text))
            {
                string line;
                bool skipHeader = true;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (skipHeader) { skipHeader = false; continue; }
                    string[] parts = line.Split(',');
                    if (parts.Length < 2) continue;

                    string type = parts[0];
                    string name = parts[1];

                    if (type.Equals("BGM", StringComparison.OrdinalIgnoreCase) && Enum.TryParse(name, out BGMType bgmType))
                    {
                        string path = $"Sounds/BGM/{name}";
                        AudioClip clip = Resources.Load<AudioClip>(path);
                        if (clip != null)
                            _bgmClips[bgmType] = clip;
                        else
                            Debug.LogWarning($"BGM clip not found at: {path}");
                    }
                    else if (type.Equals("SFX", StringComparison.OrdinalIgnoreCase) && Enum.TryParse(name, out SFXType sfxType))
                    {
                        string path = $"Sounds/SFX/{name}";
                        AudioClip clip = Resources.Load<AudioClip>(path);
                        if (clip != null)
                            _sfxClips[sfxType] = clip;
                        else
                            Debug.LogWarning($"SFX clip not found at: {path}");
                    }
                }
            }
        }
        #endregion

        #region SFX
        public void PlaySFX(SFXType type)
        {
            if (!_sfxClips.TryGetValue(type, out var clip)) return;

            var source = _sfxSources[_sfxIndex];
            source.clip = clip;
            source.Play();

            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;
        }
        #endregion

        #region BGM
        public async UniTask PlayBGM(BGMType type, float fadeTime = 1f)
        {
            if (!_bgmClips.TryGetValue(type, out var newClip)) return;
            if (_bgmAudioSource.clip == newClip) return;

            float originalVol = _bgmAudioSource.volume;

            // Fade out
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                _bgmAudioSource.volume = Mathf.Lerp(originalVol, 0, t / fadeTime);
                await UniTask.Yield();
            }

            _bgmAudioSource.Stop();
            _bgmAudioSource.clip = newClip;
            _bgmAudioSource.Play();

            // Fade in
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                _bgmAudioSource.volume = Mathf.Lerp(0, originalVol, t / fadeTime);
                await UniTask.Yield();
            }

            _bgmAudioSource.volume = originalVol;
        }
        #endregion

        #region Volume
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
