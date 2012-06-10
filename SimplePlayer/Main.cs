using System;
using System.IO;
using System.Reflection;
using Commons.Media.Streaming;
using Commons.Media.Streaming.PortAudio;
using Commons.Media.Synthesis.Nes;
using Commons.Media.PortAudio;

namespace Commons.Media.Synthesis.Sample
{
    class MainClass
	{
		public static void Main (string[] args)
		{
			var nsfStream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("enotest.nsf");
			var nsf = new byte [nsfStream.Length];
			nsfStream.Read (nsf, 0, nsf.Length);
			nsfStream.Close ();
			var p = new PortAudioStreamPlayer (
				new NesMediaBufferGenerator (nsf),
				AudioParameters.Default,
				PaSampleFormat.Int8,
				0x4000,
				null
			);
			p.Play ();
			Console.WriteLine ("Type [CR] to quit...");
			Console.ReadLine ();
		}
	}
}
