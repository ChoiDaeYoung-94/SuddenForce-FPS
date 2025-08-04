using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace AD
{
    public class SoundManager : SingletonBase<SoundManager>, ISubManager
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioSource _bgmAudioSource;
        [SerializeField] private List<AudioSource> _sfxSources = new();

        private Dictionary<BGMType, AudioClip> _bgmClips = new();
        private Dictionary<SFXType, AudioClip> _sfxClips = new();

        public async UniTask InitAsync()
        {
            float bgm = PlayerPrefs.GetFloat("BGM", 1f);
            float sfx = PlayerPrefs.GetFloat("SFX", 1f);

            SetBGMVolume(bgm);
            SetSFXVolume(sfx);

            await InitSoundTableAsync();
        }

        public void Release()
        {
        }

        #region Sound Table Loading

        private async UniTask InitSoundTableAsync()
        {
            var table = Managers.TableManager.GetTable<GameData.SoundTableData>();
            if (table == null || table.Count == 0)
            {
                DebugLogger.LogError("SoundTableData가 비어있습니다.");
                return;
            }

            foreach (var row in table)
            {
                if (string.IsNullOrWhiteSpace(row.Type) || string.IsNullOrWhiteSpace(row.Name))
                {
                    continue;
                }

                string path = $"{GameConstants.GetPath(GameConstants.ResourceCategory.Sound)}{row.GetKey()}";
                AudioClip clip = await Managers.ResourceManager.LoadAsync<AudioClip>(path);
                if (clip == null)
                {
                    DebugLogger.LogWarning($"{row.Type} clip not found at: {path}");
                    continue;
                }

                if (row.Type.Equals("BGM", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse(row.Name, out BGMType bgmType))
                    {
                        _bgmClips[bgmType] = clip;
                    }
                }
                else if (row.Type.Equals("SFX", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse(row.Name, out SFXType sfxType))
                    {
                        _sfxClips[sfxType] = clip;
                    }
                }
            }
        }

        #endregion

        #region Play Sound

        public async UniTask PlayBGM(BGMType type, float fadeTime = 1f)
        {
            if (!_bgmClips.TryGetValue(type, out var newClip)) return;
            if (_bgmAudioSource.clip == newClip) return;

            float originalVol = _bgmAudioSource.volume;

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

        public void PlaySFX(SFXType type, float volume = 1f)
        {
            if (!_sfxClips.TryGetValue(type, out var clip)) return;

            int count = _sfxSources.Count;
            for (int i = 0; i < count; i++)
            {
                if (!_sfxSources[i].isPlaying)
                {
                    var source = _sfxSources[i];
                    source.clip = clip;
                    source.volume = volume;
                    source.Play();
                    break;
                }
            }
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