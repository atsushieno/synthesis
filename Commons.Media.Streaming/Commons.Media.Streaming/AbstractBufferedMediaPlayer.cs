using System;
using System.Collections.Generic;

namespace Commons.Media.Streaming
{
	public abstract class AbstractBufferedMediaPlayer : IBufferedMediaPlayer
	{
		public AbstractBufferedMediaPlayer (IMediaBufferGenerator bufferGenerator)
		{
			if (bufferGenerator == null)
				throw new ArgumentNullException ("bufferGenerator");
			generator = bufferGenerator;
		}
		
		IMediaBufferGenerator generator;
		Queue<IMediaSample> samples = new Queue<IMediaSample> ();
		object lock_obj = new object ();
		TimeSpan position;

		public IMediaBufferGenerator BufferGenerator {
			get { return generator; }
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
	}
}

