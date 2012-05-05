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
			var player = new PortAudioPlayer (new SampleAudioQueue (), AudioParameters.Default);
		}
	}
}
