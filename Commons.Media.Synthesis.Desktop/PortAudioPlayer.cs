using System;
using Commons.Media.PortAudio;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis
{
	public class PortAudioPlayer
	{
		AudioQueueSync q;
		PortAudioStream stream;

		public PortAudioPlayer (AudioQueueSync queue, AudioParameters parameters)
		{
			if (queue == null)
				throw new ArgumentNullException ("queue");
			this.q = queue;
			stream = new PortAudioOutputStream (
				parameters.Channels,
				0,
				parameters.BitsPerSample,
				0x1000,
				StreamCallback,
				IntPtr.Zero
			);
		}

		MediaSample current_sample;
		int continue_remains;

		PaStreamCallbackResult StreamCallback (byte[] output, int offset, int byteCount, PaStreamCallbackTimeInfo timeInfo, PaStreamCallbackFlags statusFlags, IntPtr userData)
		{
			while (true) {
				switch (q.Status) {
				case AudioQueueStatus.Completed:
					return PaStreamCallbackResult.Complete;
				case AudioQueueStatus.Error:
					return PaStreamCallbackResult.Abort;
				}
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
				byteCount -= copySize;
				if (byteCount == 0)
					break;
			}
			return PaStreamCallbackResult.Continue;
		}
		
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
