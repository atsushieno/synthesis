using System;
using System.IO;
using System.Reflection;
using Commons.Media.Synthesis;
using Commons.Media.Synthesis.Desktop;
using Commons.Media.Synthesis.Nes;
using Commons.Media.PortAudio;

namespace Commons.Media.Synthesis.Sample
{
    class MainClass
	{
		public static void Main (string[] args)
		{
			var nsfStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("enotest.nsf");
			var nsf = new byte [nsfStream.Length];
			nsfStream.Read (nsf, 0, nsf.Length);
			nsfStream.Close ();
			var p = new PortAudioStreamPlayer (new NesMediaBufferGenerator (nsf), AudioParameters.Default, PaSampleFormat.Int8, 0x4000, null);
			p.Play ();
		}
		
		public static void Main2 (string[] args)
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
			uint bufSize = 0x10000;
			var player = new PortAudioPlayer<byte> (
				new SampleAudioQueue (bufSize), new AudioParameters () { Channels = 2, SamplesPerSecond = 44100, BitsPerSample = 32 }, PaSampleFormat.Float32, bufSize, null);
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
