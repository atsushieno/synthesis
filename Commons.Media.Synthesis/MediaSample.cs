using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	[Obsolete]
	public class MediaSample<T>
	{
		public MediaSample (ArraySegment<T> buffer, TimeSpan duration)
		{
			Buffer = buffer;
			Duration = duration;
		}

		public ArraySegment<T> Buffer { get; private set; }

		public TimeSpan Duration { get; private set; }
	}
}
