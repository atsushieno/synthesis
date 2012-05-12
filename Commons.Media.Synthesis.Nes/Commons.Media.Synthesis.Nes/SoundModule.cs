using System;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis.Nes
{
	public delegate void GenerateSoundHandler (long currentTime, long duration, byte [] buffer);

	// NOP-based sound module
	public class SoundModule
	{
		public SoundModule ()
		{
		}

		public event GenerateSoundHandler GenerateSound;
	}
}

