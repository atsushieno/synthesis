using System;

namespace Commons.Media.Streaming
{
	// API that platform supporters would like to implement (and hence no need to expose to users)
	public interface IBufferedMediaPlayer : IMediaPlayer
	{
		IMediaBufferGenerator BufferGenerator { get; }
		BufferFullOperation BufferFullOperation { get; }
		event Action BufferFull;
		BufferEmptyOperation BufferEmptyOperation { get; }
		event Action BufferEmpty;
		bool PauseSourceAsWell { get; }
	}
}

