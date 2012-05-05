using System;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis.Sample
{
	class MainClass
	{
		public static void Main (string [] args)
		{
			var player = new PortAudioPlayer (new SampleAudioQueue (), AudioParameters.Default);
		}
	}
}
