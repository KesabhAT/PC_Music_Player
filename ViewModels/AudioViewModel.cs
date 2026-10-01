using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PC_Music_Player.Service;

namespace PC_Music_Player.ViewModels;

public partial class AudioViewModel : ViewModelBase
{
    public string Label { get; set; }

    public string FilePath { get; set; }

    public LinkedListNode<AudioViewModel> LLNAudio { get; set; } = null!;

    [ObservableProperty]
    public partial string Background { get; set; } = "Transparent";
    public Action<AudioViewModel>? PlayAudio { get; set; }

    public Action<AudioViewModel>? RemoveAudio { get; set; }
    
    public AudioViewModel(string Label, string FilePath)
    {
        this.Label = Label;
        this.FilePath = FilePath;
    }

    [RelayCommand]
    private void Play()
    {
        PlayAudio?.Invoke(this);
    }


    [RelayCommand]
    private void Remove()
    {
        RemoveAudio?.Invoke(this);
        // Console.WriteLine($"Button '{Label}' was clicked");
    }


}