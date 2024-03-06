// See https://aka.ms/new-console-template for more information
using System.IO.Ports;
using System.Text.RegularExpressions;
using NAudio.Wave;

// Replace 'COM4' with your actual device COM port
string portName = "COM4";
int baudRate = 115200;

// Paths to your drum and bass samples
string drumSamplePath = @"C:\Users\admin\Downloads\Nord Drum Sample Pack\SNR\GARAGE2.wav"; // Replace with actual path
string bassSamplePath = @"C:\Users\admin\Downloads\Nord Drum Sample Pack\PERC\FOILED.wav"; // Replace with actual path

//user inputs both samples as paths
Console.WriteLine("Enter the path to the drum sample: ");
drumSamplePath = Console.ReadLine();
//remove quotes that are added to the path
drumSamplePath = drumSamplePath.Replace("\"", "");
Console.WriteLine("Enter the path to the bass sample: ");
bassSamplePath = Console.ReadLine();
//remove quotes that are added to the path
bassSamplePath = bassSamplePath.Replace("\"", "");


 void TestPlay()
{
    using (var audioFile = new AudioFileReader(bassSamplePath))
    using (var outputDevice = new WaveOutEvent())
    {
        outputDevice.Init(audioFile);
        outputDevice.Play();
        Thread.Sleep(5000); // Wait for 5 seconds to hear the playback
    }
}

//TestPlay();

// Initialize your VoltageMelodyPlayer
VoltageMelodyPlayer player = new VoltageMelodyPlayer(drumSamplePath, bassSamplePath);
player.Play(); // Start playing the music, the tempo will be adjusted based on voltage

using (SerialPort port = new SerialPort(portName, baudRate))
{
    bool portOpened = false;

    while (!portOpened)
    {
        try
        {
            port.Open();
            portOpened = true;
            Console.WriteLine($"Listening on {portName}...");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to open port {portName}: {e.Message}. Retrying in 5 seconds...");
            Thread.Sleep(5000); // Wait for 5 seconds before retrying
        }
    }

    float voltageRead;

    player.Play();
    while (true)
    {
        if (port.BytesToRead > 0)
        {
            string message = port.ReadLine();
            // Use a regular expression to extract the first floating point number from the message
            Match match = Regex.Match(message, @"[-+]?\d*\.?\d+"); // Matches a floating-point number

            if (match.Success)
            {
                // We found a number, now try to parse it
                string numberString = match.Value;
                if (float.TryParse(numberString, out voltageRead))
                {
                    Console.WriteLine($"G_c readout: {voltageRead}");
                    // Pass the voltage to your player to adjust the music
                    player.AddVoltage(voltageRead);
                }
                else
                {
                    Console.WriteLine($"Failed to parse '{numberString}' as float.");
                }
            }
            else
            {
                Console.WriteLine("No numeric data found in the message.");
            }
        }


        // Prevent the program from consuming too much CPU
        Thread.Sleep(10);
    }
}
// Ideally, you would have a way to exit this loop and call player.Stop() to clean up resources.