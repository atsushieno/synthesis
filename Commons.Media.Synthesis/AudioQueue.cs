using System;
using System.Net;

namespace Commons.Media.Synthesis
{
	public enum AudioQueueStatus
	{
		Ongoing,
		Completed,
		Error
	}

	public interface IAudioQueue<T>
	{
		void Close ();

		IAsyncResult BeginGetNextSample (AsyncCallback callback, object state);

		MediaSample<T> EndGetNextSample (IAsyncResult result);

		AudioQueueStatus Status { get; }

		IAsyncResult BeginSeek (TimeSpan position, AsyncCallback callback, object state);

		void EndSeek (IAsyncResult result);
	}

	public abstract class AudioQueueSync<T> : IAudioQueue<T>
	{
		protected AudioQueueSync ()
		{
			get_next_sample = new Func<MediaSample<T>> (GetNextSample);
			seek = new Action<TimeSpan> (Seek);
		}

		public abstract void Close ();

		public abstract MediaSample<T> GetNextSample ();

		public abstract AudioQueueStatus Status { get; }

		public abstract void Seek (TimeSpan position);

		Func<MediaSample<T>> get_next_sample;

		public IAsyncResult BeginGetNextSample (AsyncCallback callback, object state)
		{
			return get_next_sample.BeginInvoke (callback, state);
		}

		public MediaSample<T> EndGetNextSample (IAsyncResult result)
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
