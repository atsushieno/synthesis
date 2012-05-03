using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	public abstract class MediaSample
	{
		public abstract ArraySegment<byte> Buffer { get; }

		public abstract TimeSpan Duration { get; }
	}
}
