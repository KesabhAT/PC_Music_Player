using System;
using System.Formats.Asn1;
using System.IO;
using System.Threading;
using NAudio.SoundFile;
using NAudio.Wave;
using NAudio.Wave.Alsa;
using NLayer.NAudioSupport;

namespace PC_Music_Player.Service;

public class AudioService : IAudioService
{
    private IWavePlayer? outputDevice;
    private WaveStream? audioFile;

    public void StopAudio()
    {
        outputDevice?.Stop();
    }

    public string PlayPauseAudio()
    {
        if (outputDevice?.PlaybackState == PlaybackState.Playing)
        {
            outputDevice?.Pause();
            return "Paused";
        }
        else if (outputDevice?.PlaybackState == PlaybackState.Paused)
        {
            outputDevice?.Play();
            return "Playing";
        }
        else
        {
            return "Stopped";
        }

    }

    private readonly ReaderWriterLockSlim _lock = new();
    public string StartOrSwitchAudio(string AudioFilePath)
    {
        _lock.EnterWriteLock();
        try
        {
            if (outputDevice?.PlaybackState == PlaybackState.Playing || outputDevice?.PlaybackState == PlaybackState.Paused)
            {
                outputDevice.Stop(); //the outputDevice.PlaybackStopped event occurs in this same thread
            }

            var extension = Path.GetExtension(AudioFilePath).ToLower();

            if (extension == ".mp3")
            {
                audioFile = new Mp3FileReaderBase(AudioFilePath, waveformat => new Mp3FrameDecompressor(waveformat));
            }
            else if (extension == ".wav")
            {
                audioFile = new WaveFileReader(AudioFilePath);
            }
            else if (extension == ".aiff" || extension == ".aif")
            {
                audioFile = new AiffFileReader(AudioFilePath);
            }
            else if (extension == ".flac" || extension == ".ogg" || extension == ".opus")
            {
                audioFile = new SoundFileReader(AudioFilePath);
            }
            else if (extension == ".aac" || extension == ".m4a" || extension == ".wma")
            {
                Console.WriteLine("Thes file extension is not supported on linux");
                return "00:00";
            }
            else
            {
                Console.WriteLine("This file extension is not supported");
                return "00:00";
            }


            outputDevice = new AlsaOut();
            outputDevice.PlaybackStopped += OnPlaybackStopped;
            outputDevice.Init(audioFile);

            var totalTime = audioFile.TotalTime.Hours > 0 ? audioFile.TotalTime.Hours + ":" : "" +
                                    audioFile.TotalTime.Minutes.ToString("00") + ":" +
                                    audioFile.TotalTime.Seconds.ToString("00");

            outputDevice.Play();
            return totalTime;

        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public (long, string, string) GetSeekData()
    {
        _lock.EnterReadLock();
        try
        {
            if (outputDevice != null && audioFile != null &&
            (outputDevice.PlaybackState == PlaybackState.Playing || outputDevice?.PlaybackState == PlaybackState.Paused))
            {
                var position = audioFile.Position * 100 / audioFile.Length;
                var currentTime = audioFile.CurrentTime.Hours > 0 ? audioFile.CurrentTime.Hours + ":" : "" +
                                    audioFile.CurrentTime.Minutes.ToString("00") + ":" +
                                    audioFile.CurrentTime.Seconds.ToString("00");
                return (position, currentTime, "");
            }
            return (-1, "", "");
        }
        finally
        {
            _lock.ExitReadLock();
        }

    }
    
    public (long,string,string) SetSeekData(long Position)
    {
        _lock.EnterWriteLock();
        try
        {
            if (outputDevice != null && audioFile != null &&
            (outputDevice.PlaybackState == PlaybackState.Playing || outputDevice?.PlaybackState == PlaybackState.Paused))
            {
                audioFile.Position = Position * audioFile.Length/100;
                var currentTime = audioFile.CurrentTime.Hours > 0 ? audioFile.CurrentTime.Hours + ":" : "" +
                                    audioFile.CurrentTime.Minutes.ToString("00") + ":" +
                                    audioFile.CurrentTime.Seconds.ToString("00");
                return (audioFile.Position *100/audioFile.Length ,currentTime, "");
            }
            return ( -1,"", "");
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
    
    private void OnPlaybackStopped(object? sender, StoppedEventArgs args)
    {
        outputDevice?.Dispose();
        outputDevice = null;
        audioFile?.Dispose();
        audioFile = null;
    }

}