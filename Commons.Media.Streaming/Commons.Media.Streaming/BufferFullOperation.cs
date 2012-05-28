using System;

namespace Commons.Media.Streaming
{
		public enum BufferFullOperation
		{
			DiscardOld,
			DiscardNew,
			PauseSource,
			Notify
		}
}

