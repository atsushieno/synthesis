using System;

namespace Commons.Media.Streaming
{
	// Features that application developers would like to use.
	public interface IMediaPlayer
	{
		void Play ();
		void Pause ();
		void Stop ();
		void JumpToTime (TimeSpan position);
		void JumpToPosition (long position);
		TimeSpan PlayTime { get; }
		TimeSpan TotalTime { get; }
	}
}

