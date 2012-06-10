using System;
using System.Collections.Generic;
using System.Threading;

namespace Commons.Media.Streaming
{
	public abstract class AbstractBufferedMediaPlayer : IBufferedMediaPlayer
	{
		public AbstractBufferedMediaPlayer (IMediaBufferGenerator bufferGenerator)
		{
			if (bufferGenerator == null)
				throw new ArgumentNullException ("bufferGenerator");
			generator = bufferGenerator;
			generator.BufferArrived += BufferArrived;
		}
		
		IMediaBufferGenerator generator;
		Queue<IMediaSample> samples = new Queue<IMediaSample> ();
		ManualResetEvent wait_sample_handle = new ManualResetEvent (false);
		
		object lock_obj = new object ();
		TimeSpan position;
		int max_buf_count = 0x400;

		public IMediaBufferGenerator BufferGenerator {
			get { return generator; }
		}
		
		public int MaxBufferCount {
			get { return max_buf_count; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("MaxBufferSize must be greater than 0");
				max_buf_count = value;
			}
		}
		
		public BufferEmptyOperation BufferEmptyOperation { get; set; }
		public event Action BufferFull;
		public BufferFullOperation BufferFullOperation { get; set; }
		public event Action BufferEmpty;
		public bool PauseSourceAsWell { get; set; }
		
		protected abstract void StartSource ();
		protected abstract void PauseSource ();
		protected abstract void ResumeSource ();
		protected abstract void StopSource ();
		
		protected virtual TimeSpan FramesToDuration (long frames)
		{
			throw new NotImplementedException ();
		}
		
		protected virtual long DurationToFrame (TimeSpan duration)
		{
			throw new NotImplementedException ();
		}
		
		// exposed to users.
		public void JumpToPosition (long position)
		{
			lock (lock_obj) {
				samples.Clear ();
				generator.SeekTo (position);
				this.position = FramesToDuration (position);
			}
		}
		
		public void JumpToTime (TimeSpan position)
		{
			lock (lock_obj) {
				samples.Clear ();
				generator.SeekTo (position);
				this.position = position;
			}
		}
		
		public void Pause ()
		{
			lock (lock_obj) {
				PauseSource ();
				if (PauseSourceAsWell)
					generator.Pause ();
			}
		}
		
		public void Resume ()
		{
			lock (lock_obj) {
				ResumeSource ();
			}
		}
		
		public void Play ()
		{
			lock (lock_obj) {
				StartSource ();
			}
		}
		
		public void Stop ()
		{
			lock (lock_obj) {
				StopSource ();
			}
		}

		public TimeSpan PlayTime {
			get { return position; }
		}
		
		public TimeSpan TotalTime {
			get { return generator.TotalTime; }
		}

		void BufferArrived (IMediaSample sample)
		{
			samples.Enqueue (sample);
			if (samples.Count == 1)
				wait_sample_handle.Set ();
			if (samples.Count == MaxBufferCount)
				if (BufferFull != null)
					BufferFull ();
		}
		
		protected IMediaSample GetNextSample ()
		{
			if (samples.Count > 0)
				return samples.Dequeue ();
			if (BufferEmpty != null)
				this.BufferEmpty ();
			wait_sample_handle.WaitOne ();
			return GetNextSample (); // loop
		}
	}
}

