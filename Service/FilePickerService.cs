using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

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
        var suggestedStartLocation = await _window.StorageProvider.TryGetFolderFromPathAsync(new Uri("/home/birat/Development/DotNet/PC_Music_Player/test_audio_samples"));

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