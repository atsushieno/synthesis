using System;

namespace Commons.Media.Streaming
{
	// interface for media sample that IMediaBufferGenerator implementors will have to implement.
	public interface IMediaSample
	{
		ArraySegment<T> GetBuffer<T> ();
		
		TimeSpan Duration { get; }
	}
}
