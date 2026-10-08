using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using NAudio.Wave;
using NAudio.Wave.Alsa;

namespace PC_Music_Player.Service;

public class FilePickerService : IFilePickerService
{

    private readonly Window _window;

    public FilePickerService(Window window)
    {
        _window = window;
    }
    public async Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync()
    {
        IStorageFolder? suggestedStartLocation;
            
           
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            suggestedStartLocation = await _window.StorageProvider.TryGetFolderFromPathAsync(new Uri("C:\\Development\\Dotnet\\PC_Music_Player\\test_audio_samples"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            suggestedStartLocation = await _window.StorageProvider.TryGetFolderFromPathAsync(new Uri("/home/birat/Development/DotNet/PC_Music_Player/test_audio_samples"));
        }
        else
        {
            throw new PlatformNotSupportedException("The current operating system is not supported for audio playback.");
        }

        var options = new FilePickerOpenOptions
        {
            Title = "Open Audio File",
            AllowMultiple = true,
            FileTypeFilter = [audioFiles],
            SuggestedStartLocation = suggestedStartLocation
        };

        return await _window.StorageProvider.OpenFilePickerAsync(options);

    }

    private FilePickerFileType audioFiles { get; } = new("audio files")
    {
        Patterns = new[] { "*.aac","*.ac3","*.aiff","*.aif","*.flac","*.m4a","*.mp3", "*.ogg","*.opus","*.wav","*.wma"},
        MimeTypes=new[]{"audio/*"}
    };
}