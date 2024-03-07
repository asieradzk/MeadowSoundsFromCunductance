using System;
using System.Collections.Generic;
using NAudio.Wave;
using System.Threading;

public class VoltageMelodyPlayer
{
    private readonly List<string> samplePaths;
    private readonly List<List<float>> voltageCollections = new List<List<float>>();
    private readonly int maxHistorySize = 60; // Consider the last 60 measurements for calculating speed modifier
    private readonly List<float> speedModifiers = new List<float>(); // Separate speed modifier for each sample
    private readonly List<DateTime> nextPlayTimes = new List<DateTime>(); // Next play times for each sample
    private bool isPlaying = false;
    private readonly List<Thread> playThreads = new List<Thread>(); // One thread per sample for independent playback

    public VoltageMelodyPlayer(string path1, string path2, string path3, string path4)
    {
        samplePaths = new List<string> { path1, path2, path3, path4 };
        
        // Initialize collections for each sample and their speed modifiers
        for (int i = 0; i < 4; i++)
        {
            voltageCollections.Add(new List<float>());
            speedModifiers.Add(1.0f); // Start with an initial speed modifier for each sample
            nextPlayTimes.Add(DateTime.MinValue); // Initialize with past times to allow immediate playing
        }
    }

    public void AddVoltage(float voltage1, float voltage2, float voltage3, float voltage4)
    {
        List<float> voltages = new List<float> { voltage1, voltage2, voltage3, voltage4 };

        for (int i = 0; i < 4; i++)
        {
            List<float> collection = voltageCollections[i];
            if (collection.Count >= maxHistorySize)
            {
                collection.RemoveAt(0);
            }
            collection.Add(voltages[i]);

            // Update the speed modifier for each sample based on its voltage history
            UpdateSpeedModifier(i);
        }
    }

    private void UpdateSpeedModifier(int index)
    {
        List<float> voltageHistory = voltageCollections[index];
        if (voltageHistory.Count >= 2)
        {
            float largestDifference = 0.0f;

            // Find the largest voltage difference in the last 60 measurements
            for (int i = 1; i < voltageHistory.Count; i++)
            {
                float difference = Math.Abs(voltageHistory[i] - voltageHistory[i - 1]);
                largestDifference = Math.Max(largestDifference, difference);
            }

            // Map the largest difference from range [0, 3.3] to [0.05, 50.0] for speedModifier
            float newSpeedModifier = (largestDifference / 3.3f) * (50.0f - 0.05f) + 0.05f;
            speedModifiers[index] = newSpeedModifier;
        }
    }

    public void Start()
    {
        if (isPlaying) return;

        isPlaying = true;
        for (int i = 0; i < samplePaths.Count; i++)
        {
            int sampleIndex = i; // Capture the current index for lambda expressions
            playThreads.Add(new Thread(() => ContinuousPlay(sampleIndex)));
            playThreads[sampleIndex].Start();
        }
    }

    private void ContinuousPlay(int sampleIndex)
    {
        while (isPlaying)
        {
            if (DateTime.Now >= nextPlayTimes[sampleIndex])
            {
                PlaySample(samplePaths[sampleIndex]);
                // Set the next play time based on the current speedModifier
                double interval = 500 / speedModifiers[sampleIndex]; // Assuming base interval of 10000 ms, adjust as needed
                nextPlayTimes[sampleIndex] = DateTime.Now.AddMilliseconds(interval);
                var bpm = (60 / (interval / 1000));
                string bpmString;
                if (bpm < 170)
                {
                    bpmString = bpm.ToString();
                }else
                {
                    bpmString = "170/Max";
                }
                Console.WriteLine($"Sample {sampleIndex + 1} played. BPM: {bpmString}");
            }
            Thread.Sleep(100); // Sleep to reduce CPU usage, adjust as needed
        }
    }

    private void PlaySample(string samplePath)
    {
        using (var audioFile = new AudioFileReader(samplePath))
        using (var outputDevice = new WaveOutEvent())
        {
            outputDevice.Init(audioFile);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(100); // Wait a bit between checks to reduce CPU usage
            }
        }
    }

    public void Stop()
    {
        isPlaying = false;
        foreach (var thread in playThreads)
        {
            thread?.Join(); // Wait for all playing threads to finish
        }
    }
}
