using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PC_Music_Player.Service;

namespace PC_Music_Player.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public double _audioSeekPosition;
    [ObservableProperty]
    public partial string TotalAudioTime { get; set; } = "/ 00:00";
    [ObservableProperty]
    public partial string CurrentAudioTime { get; set; } = "00:00";

    [ObservableProperty]
    public partial string PlaylistLabel { get; set; } = "Add Audio to show them in the Playlist";
    public ObservableCollection<AudioViewModel> AudioViewModels { get; } = new();

    private HashSet<string> UniqueAudioFilePaths = new HashSet<string>();
    private LinkedList<AudioViewModel> LLAudio = new LinkedList<AudioViewModel>();
    private AudioViewModel? PlayingAudio;


    private readonly IFilePickerService _filePickerService;

    private readonly IAudioService _audioService;
    public MainViewModel(IFilePickerService filePickerService, IAudioService audioService)
    {
        _filePickerService = filePickerService;
        _audioService = audioService;

    }

    [RelayCommand]
    private async Task AddAudio()
    {
        var files = await _filePickerService.OpenFilePickerAsync();
        foreach (var file in files)
        {
            string filePath = file.Path.LocalPath;

            if (UniqueAudioFilePaths.Contains(filePath))
            {
                continue;
            }

            UniqueAudioFilePaths.Add(filePath);
            var AduioToAdd = new AudioViewModel(file.Name, filePath) { PlayAudio = PlayAudio, RemoveAudio = RemoveAudio };
            //Adding the AudioViewModel to linked list and then keeping the refrence to the linked list node in the AudioViewModel itself   
            AduioToAdd.LLNAudio = LLAudio.AddLast(AduioToAdd);

            AudioViewModels.Add(AduioToAdd);
        }
        if (AudioViewModels.Count > 0)
        {
            PlaylistLabel = "Playlist";
        }
    }

    private void RemoveAudio(AudioViewModel AudioToRemove)
    {
        if (PlayingAudio != null && PlayingAudio.FilePath == AudioToRemove.FilePath)
        {
            if (AudioToRemove.LLNAudio.Next is LinkedListNode<AudioViewModel> llnNextAudio)
            {
                PlayAudio(llnNextAudio.Value);
            }
            else
            {
                Stop();
            }
        }

        UniqueAudioFilePaths.Remove(AudioToRemove.FilePath);
        LLAudio.Remove(AudioToRemove.LLNAudio);
        AudioViewModels.Remove(AudioToRemove);
        if (AudioViewModels.Count == 0)
        {
            PlaylistLabel = "Add Audio to show them in the Playlist";
        }
    }

    [RelayCommand]
    private void Previous()
    {
        if (PlayingAudio!=null && PlayingAudio.LLNAudio.Previous is LinkedListNode<AudioViewModel> llnPreviousAudio)
        {
            PlayAudio(llnPreviousAudio.Value); 
        }
    }

    [RelayCommand]
    private void Next()
    {
        if (PlayingAudio!=null && PlayingAudio.LLNAudio.Next is LinkedListNode<AudioViewModel> llnNextAudio)
        {
            PlayAudio(llnNextAudio.Value); 
        }
    }
    [RelayCommand]
    private void PlayPause()
    {

        var newState = _audioService.PlayPauseAudio();
        if (newState == "Playing")
        {
            RunSeekPolling();
        }
        else if (newState == "Paused")
        {
            StopSeekPolling();
        }
        else if(newState == "Stopped")
        {
            if (LLAudio.First is LinkedListNode<AudioViewModel> llnFirstAudio)
            {
                PlayAudio(llnFirstAudio.Value);
            }
        }
    }

    private void PlayAudio(AudioViewModel AudioViewModel)
    {
        PlayingAudio?.Background = "Transparent";
        PlayingAudio = AudioViewModel;
        PlayingAudio.Background = "DodgerBlue";
        TotalAudioTime = "/ "+ _audioService.StartOrSwitchAudio(AudioViewModel.FilePath);
        CurrentAudioTime = "00:00";
        AudioSeekPosition = 0;
        //run this line after StartOrSwitchAudio
        RunSeekPolling();

    }
    private Task? SeekDataPollingTask;
    private CancellationTokenSource? _cts;

    private void RunSeekPolling()
    {
        if (SeekDataPollingTask == null || SeekDataPollingTask.IsCompleted == true ||
            (_cts != null && _cts.IsCancellationRequested))
        {
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            SeekDataPollingTask = SeekPolling(_cts.Token);
        }

    }
    
    private void StopSeekPolling()
    {
        if(SeekDataPollingTask != null && SeekDataPollingTask.IsCompleted != true &&
            _cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    private async Task SeekPolling(CancellationToken _ct)
    {
        while (!_ct.IsCancellationRequested && PlayingAudio != null)
        {
            var (position, currentTime, remainingTime) = _audioService.GetSeekData();
            if (position == -1)
            {
                if (PlayingAudio.LLNAudio.Next is LinkedListNode<AudioViewModel> llnNextAudio)
                {
                    PlayAudio(llnNextAudio.Value);
                }
                else
                {
                    Stop();
                }
            }
            else
            {
                CurrentAudioTime = currentTime;
                if (IsSeekSliderHeld)
                {
                    #pragma warning disable MVVMTK0034
                    //Setting the value of the slider via SetProperty so that the Seek Position moves but the OnAudioSeekPositionChanged Event is not triggered
                    SetProperty(ref _audioSeekPosition, position, nameof(AudioSeekPosition));
                    #pragma warning restore MVVMTK0034
                }
            }
            try
            {
                await Task.Delay(1000, _ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    //This is set to true when user is holding the Audio Seek Slider with the pointer
    public bool IsSeekSliderHeld;
    partial void OnAudioSeekPositionChanged(double oldValue, double newValue)
    {
        var (newPosition, currentTime, remainingTime) = _audioService.SetSeekData((long)newValue);
        if (newPosition > -1)
        {
            //Setting the value of the slider via SetProperty so that the Seek Position moves but the OnAudioSeekPositionChanged Event is not triggered
            SetProperty(ref _audioSeekPosition, newPosition, nameof(AudioSeekPosition));
            CurrentAudioTime = currentTime;
        }
    }

    [RelayCommand]
    public void Stop()
    {
        PlayingAudio?.Background = "Transparent";
        PlayingAudio = null;
        CurrentAudioTime = "00:00";
        TotalAudioTime = "/ 00:00";
        AudioSeekPosition = 0;
        _audioService.StopAudio();
        
        StopSeekPolling();
    }

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to PC Music Player!";
}
