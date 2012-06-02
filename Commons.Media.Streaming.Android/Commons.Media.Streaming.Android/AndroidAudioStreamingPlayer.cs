using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.Media;
using Commons.Media.Streaming;

using Encoding = Android.Media.Encoding;

namespace Commons.Media.Synthesis.Droid
{
	public class AndroidAudioStreamingPlayer
	{
		class PlayerException : Exception
		{
			public PlayerException ()
                : base ()
			{
			}

			public PlayerException (string message)
                : base (message)
			{
			}

			public PlayerException (string message, Exception innerException)
                : base (message, innerException)
			{
			}
		}

		internal enum PlayerStatus
		{
			Stopped,
			Playing,
			Paused,
		}

		const int CompressionRate = 2;
		int min_buf_size;
		int buf_size;
		AudioTrack audio;
		bool pause, finish;
		AutoResetEvent pause_handle = new AutoResetEvent (false);
		TimeSpan total_time, current;
		Thread player_thread;
		IMediaBufferGenerator gen;

		ChannelConfiguration ToChannelConfiguration (byte channels)
		{
			switch (channels) {
			case 2:
				return ChannelConfiguration.Stereo;
			case 1:
				return ChannelConfiguration.Mono;
			default:
				throw new ArgumentException ("Only 1 or 2 channels are supported");
			}
		}

		public AndroidAudioStreamingPlayer (IMediaBufferGenerator generator, AudioParameters parameters, TimeSpan totalTime)
		{
			if (generator == null)
				throw new ArgumentNullException ("generator");
			gen = generator;

			min_buf_size = AudioTrack.GetMinBufferSize (
				parameters.SamplesPerSecond / CompressionRate * 2,
				ChannelOut.Stereo,
				Encoding.Pcm16bit
			);
			buf_size = min_buf_size * 8;

			// "* n" part is adjusted for device.
			audio = new AudioTrack (
				Android.Media.Stream.Music,
				parameters.SamplesPerSecond / CompressionRate * 2,
				ToChannelConfiguration (parameters.Channels),
				Android.Media.Encoding.Pcm16bit,
				buf_size * 4,
				AudioTrackMode.Stream
			);
			player_thread = new Thread (() => DoRun ());
			this.total_time = totalTime;
		}

		internal PlayerStatus Status { get; private set; }

		public void Pause ()
		{
			Status = PlayerStatus.Paused;
			pause = true;
		}

		public void Resume ()
		{
			Status = PlayerStatus.Playing;
			pause = false; // make sure to not get overwritten
			pause_handle.Set ();
		}

		DateTime last_seek;

		public void Seek (TimeSpan position)
		{
			if (position < TimeSpan.Zero || position >= total_time)
				return; // ignore
			if (DateTime.Now - last_seek < TimeSpan.FromMilliseconds (500))
				return; // too short seek operations
			last_seek = DateTime.Now;
			SpinWait.SpinUntil (() => !pause);
			gen.SeekTo (position);
		}

		public void Stop ()
		{
			finish = true; // and let player loop finish.
			pause_handle.Set ();
		}

		public void Start ()
		{
			if (Status != PlayerStatus.Stopped) {
				Stop ();
				SpinWait.SpinUntil (() => Status == PlayerStatus.Stopped);
			}
			player_thread.Start ();
		}

		public event Action Completed;
		public event Action<TimeSpan> ReportProgress;
		public event Action<Exception> PlayerError;

		protected void OnCompleted ()
		{
			if (Completed != null)
				Completed ();
		}

		protected void OnReportProgress (TimeSpan position)
		{
			if (ReportProgress != null)
				ReportProgress (position);
		}

		protected void OnPlayerError (Exception ex)
		{
			if (PlayerError != null)
				PlayerError (ex);
			else
				throw ex;
		}

		void DoRun ()
		{
			gen.SeekTo (TimeSpan.Zero);
			Status = PlayerStatus.Playing;
			audio.Play ();
			TimeSpan nextSleep = TimeSpan.Zero, nextDelay = TimeSpan.Zero;
			while (!finish) {
				if (pause) {
					pause = false;
					audio.Pause ();
					pause_handle.WaitOne ();
					audio.Play ();
				}
				Thread.Sleep (nextSleep);
				current += nextDelay;
				nextSleep = nextDelay;
				nextDelay = TimeSpan.FromMilliseconds (500); // error
				gen.BufferArrived += sample => {
					nextDelay = sample.Duration;
					var buffer = sample.GetBuffer<byte> ();
					var size = buffer.Count;
					if (current == total_time) {
						finish = true;
						if (size < 0)
							OnPlayerError (new PlayerException (String.Format (
								"vorbis error : {0}",
								size
							)));
					} else {
						OnReportProgress (current);

						// downgrade bitrate
						int actualSize = (int)size * 2 / CompressionRate;
						var bytes = buffer.Array;
						for (int i = 1; i < actualSize; i++)
							bytes [i] = bytes [i * CompressionRate / 2 + (CompressionRate / 2) - 1];
						if (size > 0) {
							audio.Flush ();
							audio.Write (bytes, 0, actualSize);
						}
					}
				};
			}
			audio.Flush ();
			audio.Stop ();
			OnCompleted ();
			Status = PlayerStatus.Stopped;
		}

		public void Dispose ()
		{
			if (audio.PlayState != PlayState.Stopped)
				audio.Stop ();
			audio.Release ();
		}
	}
}
