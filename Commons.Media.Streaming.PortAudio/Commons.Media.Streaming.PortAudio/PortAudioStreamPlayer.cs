using System;
using System.Collections.Generic;
using Commons.Media.PortAudio;
using Commons.Media.Streaming;

namespace Commons.Media.Streaming.PortAudio
{
	public class PortAudioStreamPlayer : AbstractBufferedMediaPlayer
	{
		PortAudioStream stream;
		
		public PortAudioStreamPlayer (IMediaBufferGenerator generator, AudioParameters parameters, PaSampleFormat sampleFormat, uint frames, object userData)
			: base (generator)
		{
			stream = new PortAudioOutputStream (
				parameters.Channels,
				sampleFormat,
				parameters.SamplesPerSecond,
				frames,
				StreamCallback,
				userData
			);
		}

		public PortAudioStream PortAudioStream {
			get { return stream; }
		}
		
		IMediaSample last_sample;
		int remaining;

		PaStreamCallbackResult StreamCallback (byte[] output, int offset, int byteCount, PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
		{
			bool takeNewBuffer = last_sample == null;
			do {
				var sample = takeNewBuffer ? GetNextSample () : last_sample;
				if (sample == null) // complete
					return PaStreamCallbackResult.Complete;
				var arr = sample.GetBuffer<byte> ();
				int rem = takeNewBuffer ? arr.Count : remaining;
				int size = Math.Min (rem, byteCount);
				Array.Copy (arr.Array, arr.Offset, output, offset, size);
				byteCount -= size;
				remaining -= size;
				if (remaining == 0)
					takeNewBuffer = true;
			} while (byteCount > 0);
			return PaStreamCallbackResult.Continue;
		}

		protected override void StartSource ()
		{
			stream.StartStream ();
		}

		protected override void PauseSource ()
		{
			stream.StopStream ();
		}

		protected override void ResumeSource ()
		{
			stream.StartStream ();
		}

		protected override void StopSource ()
		{
			stream.StopStream ();
		}
	}
}

