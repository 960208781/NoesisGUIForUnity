using Noesis;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class NoesisMediaPlayer : NoesisApp.MediaPlayer
{
    public NoesisMediaPlayer(string uri)
    {
        _gameObject = new GameObject("MediaPlayer");
        _videoPlayer = _gameObject.AddComponent<VideoPlayer>();
        _videoPlayer.renderMode = VideoRenderMode.APIOnly;

        VideoClip video = VideoProvider.instance.GetVideoClip(uri);
        if (video != null)
        {
            _videoPlayer.clip = video;
            _videoPlayer.source = VideoSource.VideoClip;
        }
        else
        {
            _videoPlayer.url = uri;
            _videoPlayer.source = VideoSource.Url;
        }

        _videoPlayer.prepareCompleted += OnMediaOpened;
        _videoPlayer.loopPointReached += OnMediaEnded;
        _videoPlayer.errorReceived += OnMediaFailed;

        _videoPlayer.Prepare();
    }

    public override uint Width { get { return _videoPlayer.width; } }
    public override uint Height { get { return _videoPlayer.height; } }
    public override bool CanPause { get { return true; } }
    public override bool HasAudio { get { return _videoPlayer.audioTrackCount > 0; } }
    public override bool HasVideo { get { return true; } }
    public override double Duration { get { return _videoPlayer.length; } }
    public override double Position
    {
        get { return _videoPlayer.time; }
        set { _videoPlayer.time = value; }
    }
    public override float SpeedRatio
    {
        get { return _videoPlayer.playbackSpeed; }
        set { _videoPlayer.playbackSpeed = value; }
    }
    public override float Volume
    {
        get { return HasAudio ? _videoPlayer.GetDirectAudioVolume(0) : 0.5f; }
        set { if (HasAudio) _videoPlayer.SetDirectAudioVolume(0, value); }
    }
    public override bool IsMuted
    {
        get { return HasAudio ? _videoPlayer.GetDirectAudioMute(0) : false; }
        set { if (HasAudio) _videoPlayer.SetDirectAudioMute(0, value); }
    }

    public override void Play()
    {
        _keepPlaying = true;
        _videoPlayer.Play();
    }

    public override void Pause()
    {
        _keepPlaying = false;
        _videoPlayer.Pause();
    }

    public override void Stop()
    {
        // Unity VideoPlayer destroys all resource on Stop, so we use Seek(0)
        _keepPlaying = false;
        _videoPlayer.sendFrameReadyEvents = true;
        _videoPlayer.frameReady += OnFrameReady;
        _videoPlayer.time = 0.0f;
        _videoPlayer.Play();
    }

    public override void Close()
    {
        if (Application.isPlaying)
        {
            VideoPlayer.Destroy(_videoPlayer);
            GameObject.Destroy(_gameObject);
        }
    }

    public override ImageSource TextureSource
    {
        get { return _textureSource; }
    }

    #region Private members
    private void OnMediaOpened(VideoPlayer source)
    {
        _textureSource = new TextureSource(_videoPlayer.texture);

        if (_videoPlayer.audioTrackCount > 0)
        {
            _videoPlayer.EnableAudioTrack(0, true);
        }

        RaiseMediaOpened();

        // Preload first frame
        _videoPlayer.sendFrameReadyEvents = true;
        _videoPlayer.frameReady += OnFrameReady;
        _videoPlayer.Play();
    }

    private void OnMediaEnded(VideoPlayer source)
    {
        RaiseMediaEnded();
    }

    private void OnMediaFailed(VideoPlayer source, string message)
    {
        RaiseMediaFailed(new Exception(message));
    }

    private void OnFrameReady(VideoPlayer source, long index)
    {
        _videoPlayer.sendFrameReadyEvents = false;
        _videoPlayer.frameReady -= OnFrameReady;

        if (!_keepPlaying)
        {
            _videoPlayer.Pause();
        }

        _keepPlaying = false;
    }

    GameObject _gameObject;
    VideoPlayer _videoPlayer;
    TextureSource _textureSource;
    bool _keepPlaying = false;
    #endregion
}
