using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using NAudio.SoundFile;
using NAudio.Wave;
using NAudio.Wave.Alsa;
using NLayer.NAudioSupport;
using PC_Music_Player.Service;
using PC_Music_Player.ViewModels;

namespace PC_Music_Player.Views;

public partial class MainWindow : Window
{
    // public MainViewModel? MainViewModel
    // {
    //     get => base.DataContext as MainViewModel;
    //     set => base.DataContext = value;
    // }
    public MainWindow()
    {
        InitializeComponent();

        SeekSlider.AddHandler(InputElement.PointerPressedEvent, OnSeekSliderPressed,RoutingStrategies.Direct);
        SeekSlider.AddHandler(InputElement.PointerReleasedEvent, OnSeekSliderReleased,RoutingStrategies.Direct);
    }

    private void OnSeekSliderPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.IsSeekSliderHeld = true;
    }
    
    private void OnSeekSliderReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.IsSeekSliderHeld = false;
    }

}