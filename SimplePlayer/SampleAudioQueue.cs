using System;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis.Sample
{
	public class SampleAudioQueue : AudioQueueSync
	{
		public SampleAudioQueue ()
		{
			sample_bytes = new byte [0x1000];
			for (int i = 0; i < sample_bytes.Length; i++)
				sample_bytes [i] = 0x40;
			sample = new MediaSample (new ArraySegment<byte> (sample_bytes, 0, sample_bytes.Length), TimeSpan.FromSeconds (1000));
		}

		#region implemented abstract members of Commons.Media.Synthesis.AudioQueueSync
		public override void Close ()
		{
		}
		
		byte [] sample_bytes;
		MediaSample sample;
		AudioQueueStatus status = AudioQueueStatus.Ongoing;

		public override MediaSample GetNextSample ()
		{
			status = AudioQueueStatus.Completed;
			return sample;
		}

		public override AudioQueueStatus Status {
			get { return status; }
		}

		public override void Seek (TimeSpan position)
		{
			// do nothing
		}
		#endregion
	}
}

