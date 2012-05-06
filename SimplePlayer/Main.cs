using System;
using Commons.Media.Synthesis;
using Commons.Media.PortAudio;

namespace Commons.Media.Synthesis.Sample
{
    class MainClass
	{
		public static void Main (string [] args)
		{
			/*
			Console.WriteLine (Configuration.HostApiCount);
			Console.WriteLine (Configuration.DefaultHostApi);
			var apiinfo = Configuration.GetHostApiInfo (Configuration.DefaultHostApi);
			Console.WriteLine (apiinfo.defaultOutputDevice);
			Console.WriteLine (apiinfo.name);
			Console.WriteLine (Configuration.DeviceCount);
			Console.WriteLine (Configuration.DefaultInputDevice);
			Console.WriteLine (Configuration.DefaultOutputDevice);
			Console.WriteLine (Configuration.VersionString);
			*/
			uint bufSize = 0x8000;
			var player = new PortAudioPlayer (new SampleAudioQueue (bufSize), new AudioParameters () { Channels = 2, SamplesPerSecond = 44100, BitsPerSample = 32 }, PaSampleFormat.Float32, bufSize, null);
			/*
			Console.WriteLine (player.PortAudioStream.IsStopped);
			Console.WriteLine (player.PortAudioStream.IsActive);
			Console.WriteLine (player.PortAudioStream.StreamInfo.sampleRate);
			Console.WriteLine (player.PortAudioStream.StreamInfo.structVersion);
			 */
			player.Play ();
			Console.WriteLine ("Type [CR] to stop it.");
			Console.ReadLine ();
			player.Stop ();
		}
	}
}
