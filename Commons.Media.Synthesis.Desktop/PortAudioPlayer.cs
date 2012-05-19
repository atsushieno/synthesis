using System;
using Commons.Media.PortAudio;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis
{
	public class PortAudioPlayer<T>
	{
		AudioQueueSync<T> q;
		PortAudioStream stream;

		public PortAudioPlayer (AudioQueueSync<T> queue, AudioParameters parameters, PaSampleFormat sampleFormat, uint frames, object userData)
		{
			if (queue == null)
				throw new ArgumentNullException ("queue");
			this.q = queue;
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

		MediaSample<T> current_sample;
		int continue_remains;

		PaStreamCallbackResult StreamCallback (byte[] output, int offset, int byteCount, PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
		{
			while (true) {
				if (q.Status != AudioQueueStatus.Ongoing)
					break;
				if (current_sample == null || continue_remains <= 0) {
					current_sample = q.GetNextSample ();
					continue_remains = current_sample.Buffer.Count;
				}
				int current_offset = current_sample.Buffer.Offset + current_sample.Buffer.Count - continue_remains;
				int copySize = Math.Min (continue_remains, byteCount);
				Array.Copy (
					current_sample.Buffer.Array,
					current_offset,
					output,
					offset,
					copySize
				);
				continue_remains -= copySize;
				byteCount -= copySize;
				Console.WriteLine ("!!!! " + x++ + " : " + byteCount);
				if (byteCount == 0)
					break;
			}
			switch (q.Status) {
			case AudioQueueStatus.Completed:
				return PaStreamCallbackResult.Complete;
			case AudioQueueStatus.Error:
				return PaStreamCallbackResult.Abort;
			default:
				return PaStreamCallbackResult.Continue;
			}
		}
		int x;
		
		public void Play ()
		{
			stream.StartStream ();
		}
		
		public void Stop ()
		{
			stream.StopStream ();
		}
	}
}

