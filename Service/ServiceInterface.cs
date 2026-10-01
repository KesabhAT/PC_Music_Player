using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using NAudio.Dsp;

namespace PC_Music_Player.Service;

public interface IFilePickerService
{
    Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync();
}

public interface IAudioService
{
    string StartOrSwitchAudio(string FilePath);
    void StopAudio();

    string PlayPauseAudio();

    (long, string, string) GetSeekData();
    (long, string, string) SetSeekData(long Position);

}