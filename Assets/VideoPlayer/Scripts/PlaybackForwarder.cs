using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VideoPlayer
{
    [RequireComponent(typeof(PlaybackEngine))]
    public class PlaybackForwarder : MonoBehaviour
    {
        private PlaybackEngine engine;

        private void Awake()
        {
            engine = GetComponent<PlaybackEngine>();
            engine.enabled = false;
        }

        public void Play(string mediaSource)
        {
            if(!UnityEngine.WSA.Application.RunningOnAppThread())
            {
                throw new Exception("PlaybackForwarder::Play must be called from App/Main thread");
            }

            this.Log($"Play  mediaSource = {mediaSource}");

            engine.VideoPath = mediaSource;
            engine.enabled = true;
        }

        private void Log(string msg, [CallerMemberName] string methodName = null)
        {
            UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "PlaybackForwarder::{0}  {1}", methodName, msg);
        }

    }
}
