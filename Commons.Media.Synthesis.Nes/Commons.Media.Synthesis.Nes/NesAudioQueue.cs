using System;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis.Nes
{
	public class NesAudioQueue<T> : AudioQueueSync<T>
	{
		NesMachine machine;
		
		public NesAudioQueue (byte [] nsf)
		{
			if (nsf == null)
				throw new ArgumentNullException ("nsf");
			machine = new NesMachine ();
			machine.Cpu.Load (nsf);
			machine.Apu.Output.GenerateSound += (time, duration, buffer) => {
				// Note:
				// - CPU will run independent of the queue state.
				// - fill audio buffer here.
				// - but if the buffer is full, discard the operation.
				throw new NotImplementedException ();
			};
		}

		#region implemented abstract members of Commons.Media.Synthesis.AudioQueueSync
		public override void Close ()
		{
			throw new System.NotImplementedException ();
		}

		public override MediaSample<T> GetNextSample ()
		{
			throw new System.NotImplementedException ();
		}

		public override AudioQueueStatus Status {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public override void Seek (TimeSpan position)
		{
			throw new System.NotImplementedException ();
		}
		#endregion
	}
}

