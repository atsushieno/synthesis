using System;

namespace Commons.Media.Streaming
{
	// custom stream buffer generator that application developers would like to implement.
	public interface IMediaBufferGenerator
	{
		void Start ();
		void Pause ();
		void Stop ();
		void SeekTo (long streamPosition);
		void SeekTo (TimeSpan timePosition);
		void NotifyGenerator ();
		event Action<IMediaSample> BufferArrived;
		event Action Completed;
		TimeSpan TotalTime { get; }
	}
}

