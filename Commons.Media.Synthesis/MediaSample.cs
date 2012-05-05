using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	public abstract class MediaSample
	{
		public MediaSample (ArraySegment<byte> buffer, TimeSpan duration)
		{
			Buffer = buffer;
			Duration = duration;
		}

		public ArraySegment<byte> Buffer { get; private set; }

		public TimeSpan Duration { get; private set; }
	}
}
