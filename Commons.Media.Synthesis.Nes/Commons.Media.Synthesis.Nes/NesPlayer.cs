using System;
using Commons.Media.Streaming;
using Commons.Media.Synthesis;
using System.Threading;

namespace Commons.Media.Synthesis.Nes
{
	public class NesPlayer
	{
		NesMachine machine;
		NesAudioQueue<byte> q;
		
		bool running;
		bool pause;
		ManualResetEvent pause_wait_handle = new ManualResetEvent (false);
		SoundModule synth;
		TimeSpan current_time;
		
		public NesPlayer (byte [] nsf)
		{
			if (nsf == null)
				throw new ArgumentNullException ("nsf");
			machine = new NesMachine ();
			machine.Cpu.Load (nsf);
			synth = new SoundModule (machine.Apu);
		}
		
		public void Play ()
		{
			// set audio callback
			
			synth.NewBufferArrived += (buffer) => {
				q.AddSample (new MediaSample<byte> (buffer, current_time));
			};
			q = new NesAudioQueue<byte> ();
			
			// start APU synth
			synth.Start ();

			// run CPU
			machine.Cpu.Run ();
		}
		
		public void Pause ()
		{
			// pause CPU
			machine.Cpu.Pause ();
			// pause APU synth
			synth.Pause ();
		}
		
		public void Stop ()
		{
			// stop CPU
			machine.Cpu.Pause (); // FIXME: do we need another method?
			// stop APU synth
			synth.Stop ();
		}
		
		public void Seek ()
		{
			// I don't think it's doable. just error out.
		}
	}
	
}

