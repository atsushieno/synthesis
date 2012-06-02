using System;
using System.Threading;
using Commons.Media.Streaming;

namespace Commons.Media.Synthesis.Nes
{
	public class NesMediaBufferGenerator : IMediaBufferGenerator
	{
		struct NesMediaSample : IMediaSample
		{
			ArraySegment<byte> buffer;
			
			public NesMediaSample (ArraySegment<byte> buffer)
			{
				this.buffer = buffer;
			}
			
			public TimeSpan Duration {
				get { throw new NotSupportedException (); }
			}
			
			public ArraySegment<T> GetBuffer<T> ()
			{
				if (typeof (T) == typeof (byte))
					return (ArraySegment<T>) (object) buffer;
				throw new NotSupportedException ();
			}
		}
		
		NesMachine machine;
		bool running;
		bool pause;
		ManualResetEvent pause_wait_handle = new ManualResetEvent (false);
		SoundModule synth;
		TimeSpan current_time;
		
		public NesMediaBufferGenerator (byte [] nsf)
		{
			if (nsf == null)
				throw new ArgumentNullException ("nsf");
			machine = new NesMachine ();
			machine.Cpu.Load (nsf);
			synth = new SoundModule (machine.Apu);
		}
		
		public event System.Action<Commons.Media.Streaming.IMediaSample> BufferArrived;
		
		public event System.Action Completed;
		
		public void Start ()
		{
			// set audio callback
			
			synth.NewBufferArrived += (buffer) => {
				BufferArrived (new NesMediaSample (buffer));
			};
			
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
		
		public void SeekTo (long streamPosition)
		{
			throw new NotSupportedException ();
		}
		
		public void SeekTo (System.TimeSpan timePosition)
		{
			throw new NotSupportedException ();
		}
		
		public void NotifyGenerator ()
		{
			throw new NotSupportedException ();
		}
		
		public System.TimeSpan TotalTime {
			get { return TimeSpan.MaxValue; } // indefinite
		}
	}
}

