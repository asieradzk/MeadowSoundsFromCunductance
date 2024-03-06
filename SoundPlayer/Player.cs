using System;
using System.Collections.Generic;
using NAudio.Wave;
using System.Threading;

public class VoltageMelodyPlayer
{
    private readonly string drumSamplePath;
    private readonly string bassSamplePath;
    private readonly List<float> voltageHistory = new List<float>();
    private int maxHistorySize = 10;
    private float speedModifier = 1.0f; // Initial speed modifier
    private bool isPlaying = false;
    private Thread playingThread;
    private readonly List<Thread> sampleThreads = new List<Thread>(); // Keep track of sample threads

    public VoltageMelodyPlayer(string drumPath, string bassPath)
    {
        drumSamplePath = drumPath;
        bassSamplePath = bassPath;
    }

    public void AddVoltage(float voltage)
    {
        if (voltageHistory.Count >= maxHistorySize)
            voltageHistory.RemoveAt(0);
        voltageHistory.Add(voltage);

        UpdateSpeedModifier();
    }

    private void UpdateSpeedModifier()
    {
        // Only proceed if there are enough measurements to start comparing differences
        if (voltageHistory.Count >= 2) 
        {
            // Initialize the largest difference found so far to zero
            float largestDifference = 0.0f;

            // Loop through the last 60 measurements (or fewer if fewer are available)
            int start = Math.Max(0, voltageHistory.Count - 60); // Start from the 60th last entry, or 0 if fewer than 60 entries
            for (int i = start + 1; i < voltageHistory.Count; i++) // Start from the second element in the range to compare with the first
            {
                float currentDifference = Math.Abs(voltageHistory[i] - voltageHistory[i - 1]);
                if (currentDifference > largestDifference)
                {
                    largestDifference = currentDifference;
                }
            }

            // Now, largestDifference holds the largest difference in the last 60 (or fewer) readings

            // Define the maximum expected difference; adjust based on your data
            float maxDifference = 0.01f; // Adjust this based on your expected range of differences

            // Scale the largest difference to the speedModifier range
            // This scales linearly from 0 to maxDifference to a range of 0.1f to 3.0f
            speedModifier = largestDifference / maxDifference * (3.0f - 0.1f) + 0.1f;

            // Ensure speedModifier stays within bounds
            speedModifier = Math.Max(0.05f, Math.Min(50.0f, speedModifier));
        }
    }




    private void PlayLoop()
    {
        bool playDrum = true;
        while (isPlaying)
        {
            string samplePath = playDrum ? drumSamplePath : bassSamplePath;
            playDrum = !playDrum;

            Thread sampleThread = new Thread(() => PlaySample(samplePath));
            sampleThread.Start();
            sampleThreads.Add(sampleThread); // Add the new thread to the list

            int sleepTime = (int)(10000 / speedModifier);
            Thread.Sleep(sleepTime);
        }

        // Ensure all sample threads finish before fully stopping
        foreach (var thread in sampleThreads)
        {
            thread.Join();
        }
    }

    private void PlaySample(string samplePath)
    {
        var audioFile = new AudioFileReader(samplePath);
        var outputDevice = new WaveOutEvent();
        outputDevice.Init(audioFile);
        outputDevice.PlaybackStopped += (sender, args) =>
        {
            outputDevice.Dispose();
            audioFile.Dispose();
        };
        outputDevice.Play();

        while (outputDevice.PlaybackState == PlaybackState.Playing)
        {
            Thread.Sleep(100); // Wait a bit between checks to reduce CPU usage
        }
    }

    public void Play()
    {
        if (isPlaying) return;

        isPlaying = true;
        playingThread = new Thread(PlayLoop);
        playingThread.Start();
    }

    public void Stop()
    {
        isPlaying = false;
        playingThread?.Join(); // Wait for the playing thread to finish
    }
}
