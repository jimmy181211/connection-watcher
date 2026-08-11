using System.Media;

namespace ConnectionWatcher.App.Services;

internal static class AlertSoundPlayer
{
    private const int SampleRate = 22_050;
    private const double DurationSeconds = 0.5;
    private static readonly object Gate = new();
    private static MemoryStream? _waveStream;
    private static SoundPlayer? _player;
    private static int _loadedVolumePercent = -1;

    public static void Play(int volumePercent)
    {
        int safeVolume = Math.Clamp(volumePercent, 10, 100);
        lock (Gate)
        {
            if (_player is null || _loadedVolumePercent != safeVolume)
            {
                _player?.Stop();
                _player?.Dispose();
                _waveStream?.Dispose();
                _waveStream = new MemoryStream(CreateWave(safeVolume), writable: false);
                _player = new SoundPlayer(_waveStream);
                _player.Load();
                _loadedVolumePercent = safeVolume;
            }

            _player.Stop();
            _player.Play();
        }
    }

    private static byte[] CreateWave(int volumePercent)
    {
        const short channels = 1;
        const short bitsPerSample = 16;
        double gain = Math.Clamp(volumePercent, 10, 100) / 100.0;
        int sampleCount = (int)(SampleRate * DurationSeconds);
        int dataLength = sampleCount * channels * bitsPerSample / 8;

        using MemoryStream stream = new(44 + dataLength);
        using BinaryWriter writer = new(stream);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataLength);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(SampleRate);
        writer.Write(SampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(dataLength);

        for (int index = 0; index < sampleCount; index++)
        {
            double time = (double)index / SampleRate;
            double first = ChimeTone(time, 0, 0.24, 660, 0.17);
            double second = ChimeTone(time, 0.16, 0.32, 880, 0.15);
            double sample = Math.Clamp((first + second) * gain, -0.3, 0.3);
            writer.Write((short)(sample * short.MaxValue));
        }

        writer.Flush();
        return stream.ToArray();
    }

    private static double ChimeTone(
        double time,
        double start,
        double duration,
        double frequency,
        double amplitude)
    {
        double local = time - start;
        if (local < 0 || local >= duration)
        {
            return 0;
        }

        double attack = Math.Min(local / 0.018, 1);
        double release = Math.Min((duration - local) / 0.09, 1);
        double envelope = Math.Min(attack, release);
        return amplitude * envelope * Math.Sin(2 * Math.PI * frequency * local);
    }
}
