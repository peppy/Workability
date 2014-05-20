using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using StatsdClient;

namespace Workability
{
    class main
    {
        private static WaveInEvent waveIn;

        /// <summary>
        /// Samples per second (hz)
        /// </summary>
        const int sample_rate = 200;

        /// <summary>
        /// How many channels should we record (1 for mono, 2 for stereo)
        /// </summary>
        const int channels = 2;

        static void Main(string[] args)
        {
            StatsdClient.Metrics.Configure(new MetricsConfig { StatsdServerName = "127.0.0.1" });

            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                Console.WriteLine("Device {0}: {1}, {2} channels", waveInDevice, deviceInfo.ProductName, deviceInfo.Channels);
            }

            Console.WriteLine();
            Console.Write("Select Device: ");

            int device = Int32.Parse(Console.ReadLine());

            waveIn = new WaveInEvent();
            waveIn.DeviceNumber = device;
            waveIn.DataAvailable += waveIn_DataAvailable;
            waveIn.WaveFormat = new WaveFormat(200, 2);
            waveIn.StartRecording();

            while (true) Thread.Sleep(100);
        }


        static void waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                float sample = (short)((e.Buffer[index + 1] << 8) | e.Buffer[index + 0]);

                sample = Math.Max(0, sample / 32768f);
                Metrics.Histogram("mic_noise", sample);
            }
        }
    }
}
