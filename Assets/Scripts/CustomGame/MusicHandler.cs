using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace CustomGame
{
    class MusicHandler : MonoBehaviour {
        public bool enabled = true;
        public string apmPath;
        public AudioMixerGroup musicMixerGroup;
        AudioSource asrc;

        void Awake() {
            asrc = gameObject.AddComponent<AudioSource>();
            asrc.outputAudioMixerGroup = musicMixerGroup;
            asrc.volume = 0.7f;
            asrc.playOnAwake = false;
        }

        void Start() {
            if (!enabled) return;
            //asrc.clip = R2Audio.APM.DecodeFile(apmPath);
            asrc.Play();
        }
    }
}
