using System;
using System.Collections.Generic;
using Commons.Media.PortAudio;
using Commons.Media.Streaming;

namespace Commons.Media.Synthesis.Desktop
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

		PaStreamCallbackResult StreamCallback (byte[] output, int offset, int byteCount, PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
		{
			throw new NotImplementedException ();
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

