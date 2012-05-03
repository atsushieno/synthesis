using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	public interface IAudioQueue
	{
		void Close ();

		IAsyncResult BeginGetNextSample (AsyncCallback callback, object state);

		MediaSample EndGetNextSample (IAsyncResult result);

		IAsyncResult BeginSeek (TimeSpan position, AsyncCallback callback, object state);

		void EndSeek (IAsyncResult result);
	}

	public abstract class AudioQueueSync : IAudioQueue
	{
		protected AudioQueueSync ()
		{
			get_next_sample = new Func<MediaSample> (GetNextSample);
			seek = new Action<TimeSpan> (Seek);
		}

		public abstract void Close ();

		public abstract MediaSample GetNextSample ();

		public abstract void Seek (TimeSpan position);

		Func<MediaSample> get_next_sample;

		public IAsyncResult BeginGetNextSample (AsyncCallback callback, object state)
		{
			return get_next_sample.BeginInvoke (callback, state);
		}

		public MediaSample EndGetNextSample (IAsyncResult result)
		{
			return get_next_sample.EndInvoke (result);
		}

		Action<TimeSpan> seek;

		public IAsyncResult BeginSeek (TimeSpan position, AsyncCallback callback, object state)
		{
			return seek.BeginInvoke (position, callback, state);
		}

		public void EndSeek (IAsyncResult result)
		{
			seek.EndInvoke (result);
		}
	}
}
