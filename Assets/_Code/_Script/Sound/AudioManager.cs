using System;
using UnityEngine;

namespace _Code._Script.Sound
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private Sound[] musicSounds, sfxSounds;
        [SerializeField] private AudioSource musicSource, sfxSource;

        public static AudioManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }        
        }
        

        public void PlayMusic(string pName)
        {
            Sound s = Array.Find(musicSounds, x => x.name == pName);

            if (s == null) 
                return;
            
            musicSource.clip = s.clip;
            musicSource.Play();
        }

        public void PlaySfx(string pName)
        {
            Sound s = Array.Find(sfxSounds, x => x.name == pName);

            if (s == null) 
                return;
            
            sfxSource.clip = s.clip;
            sfxSource.PlayOneShot(s.clip);
        }

        public void ToggleMusic()
        {
            musicSource.mute = !musicSource.mute;
        }
        
        public void ToggleSfx()
        {
            sfxSource.mute = !sfxSource.mute;
        }

        public void MusicVolume(float pVolume)
        {
            musicSource.volume = pVolume;
        }
        
        public void SfxVolume(float pVolume)
        {
            sfxSource.volume = pVolume;
        }
    }
}
