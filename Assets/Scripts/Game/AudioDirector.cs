using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>Cross-fades between the title and run themes and plays one-shot effects at the chosen volumes.</summary>
    public sealed class AudioDirector : MonoBehaviour
    {
        const float MusicScale = 0.5f;

        AudioSource music;
        AudioSource effects;
        AudioClip wanted;
        float fade = 1f;
        float musicVolume = 0.7f;
        float effectsVolume = 0.8f;
        bool ducked;

        public AudioClip Current => music != null ? music.clip : null;

        public void Init()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
        }

        /// <summary>Volumes as levels from 0 to 10, as the profile stores them.</summary>
        public void SetVolumes(int musicLevel, int effectsLevel)
        {
            musicVolume = musicLevel / 10f;
            effectsVolume = effectsLevel / 10f;
        }

        public void PlayMusic(AudioClip clip) => wanted = clip;

        public void SetDucked(bool value) => ducked = value;

        public void Play(AudioClip clip, float volume = 1f, float pitchJitter = 0.05f)
        {
            if (clip == null || effects == null || effectsVolume <= 0f)
            {
                return;
            }
            effects.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            effects.PlayOneShot(clip, volume * effectsVolume);
        }

        void Update()
        {
            if (music == null)
            {
                return;
            }
            if (music.clip != wanted)
            {
                fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime * 2.5f);
                if (fade <= 0f || !music.isPlaying)
                {
                    music.clip = wanted;
                    if (wanted != null)
                    {
                        music.Play();
                    }
                }
            }
            else
            {
                fade = Mathf.MoveTowards(fade, 1f, Time.unscaledDeltaTime * 1.5f);
            }
            music.volume = MusicScale * musicVolume * fade * (ducked ? 0.4f : 1f);
        }
    }
}
