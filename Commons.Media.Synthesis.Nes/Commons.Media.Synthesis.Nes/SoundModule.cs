using System;
using Commons.Media.Synthesis;
using System.Threading;

namespace Commons.Media.Synthesis.Nes
{
	public delegate void GenerateSoundHandler (long currentTime, long duration, byte [] buffer);

	public class SoundModule
	{
		public SoundModule ()
		{
		}
		
		bool running;
		bool pause;
		ManualResetEvent pause_wait_handle = new ManualResetEvent (false);
		
		public void Start ()
		{
			running = true;
			while (running) {
				if (pause) {
					pause = false;
					pause_wait_handle.WaitOne ();
				}
				
				throw new NotImplementedException ();
			}
		}
		
		public void Pause ()
		{
			pause = true; // and let player loop pause by itself.
		}
		
		public void Stop ()
		{
			running = false; // and let player loop stop by itself.
		}
		
		public event GenerateSoundHandler GenerateSound;
	}
}

