using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	public class AudioParameters
	{
		public static AudioParameters Default {
			get { return new AudioParameters () { Channels = 2, SamplesPerSecond = 44100, BitsPerSample = 16 }; }
		}

		public byte Channels { get; set; }

		public int SamplesPerSecond { get; set; }

		public short BitsPerSample { get; set; }
	}
}
