using System;
using System.IO;
using Commons.Media.Synthesis;

namespace Commons.Media.Synthesis.Sample
{
    class PaTestData
    {
        public PaTestData ()
        {
            for (int i = 0; i < Sine.Length; i++)
                Sine[i] = (float) Math.Sin (((double) i / Sine.Length) * Math.PI * 2);
        }
        public float [] Sine = new float [200];
        public int LeftPhase;
        public int RightPhase;
        public string Message;
    }
	
	[Obsolete]
	public class SampleAudioQueue : AudioQueueSync<byte>
	{
		public SampleAudioQueue (uint bufferSize)
		{
			this.buffer_size = bufferSize;
		}

		#region implemented abstract members of Commons.Media.Synthesis.AudioQueueSync
		public override void Close ()
		{
		}
		
		uint buffer_size;
		byte [] sample_bytes;
		MediaSample<byte> sample;
		AudioQueueStatus status = AudioQueueStatus.Ongoing;

		public override MediaSample<byte> GetNextSample ()
		{
			if (sample == null) {
				var data = new PaTestData ();
				var ms = new MemoryStream ();
				BinaryWriter bw = new BinaryWriter (ms);
				for (int i = 0; i < buffer_size; i++) {
					bw.Write (data.Sine [data.LeftPhase]);
					bw.Write (data.Sine [data.RightPhase]);
					data.LeftPhase++;
					if (data.LeftPhase >= data.Sine.Length)
						data.LeftPhase -= data.Sine.Length;
					data.RightPhase += 3;
					if (data.RightPhase >= data.Sine.Length)
						data.RightPhase -= data.Sine.Length;
				}
				bw.Close ();
				sample_bytes = ms.ToArray ();
				sample = new MediaSample<byte> (
					new ArraySegment<byte> (sample_bytes, 0, sample_bytes.Length),
					TimeSpan.FromMilliseconds (100)
				);
			}
			//status = AudioQueueStatus.Completed;
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

