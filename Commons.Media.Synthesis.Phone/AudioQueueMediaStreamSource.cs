using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Media;

namespace Commons.Media.Synthesis.Phone
{
    public class AudioQueueMediaStreamSource : MediaStreamSource
    {
        static readonly double tick_ratio = TimeSpan.TicksPerMillisecond / 10.0;

        public static long ToTick (TimeSpan duration)
        {
            return (long) (duration.Ticks / tick_ratio);
        }

        IAudioQueue q;
        AudioParameters parameters;
        TimeSpan track_duration;

        public AudioQueueMediaStreamSource (IAudioQueue queue, AudioParameters parameters, TimeSpan trackDuration)
        {
            if (q == null)
                throw new ArgumentNullException ("queue");
            parameters = parameters ?? AudioParameters.Default;
            this.q = queue;
            track_duration = trackDuration;
        }

        protected override void CloseMedia()
        {
            q.Close ();
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotSupportedException();
        }

        Dictionary<MediaSampleAttributeKeys, string> empty_atts = new Dictionary<MediaSampleAttributeKeys, string>();
        MediaStreamDescription media_desc;
        long position;

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            if (mediaStreamType != MediaStreamType.Audio)
                throw new InvalidOperationException ("Only audio stream type is supported");
            q.BeginGetNextSample ((result) => {
                var sample = q.EndGetNextSample (result);
                ArraySegment<byte> buf = sample.Buffer;
                position += ToTick (sample.Duration);
                var s = new MediaStreamSample (media_desc, new MemoryStream (buf.Array), buf.Offset, buf.Count, position, empty_atts);
                this.ReportGetSampleCompleted (s);
                }, null);
        }

        protected override void OpenMediaAsync()
        {
            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            var mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            var mediaStreamDescriptions = new List<MediaStreamDescription>();

            var wfx = new MediaParsers.WaveFormatExtensible () {
                FormatTag = 1, // PCM
                Channels = parameters.Channels,
                SamplesPerSec = parameters.SamplesPerSecond,
                AverageBytesPerSecond = parameters.SamplesPerSecond * 2 * 2,
                BlockAlign = 0,
                BitsPerSample = parameters.BitsPerSample,
                Size = 0 };

            mediaStreamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = wfx.ToHexString();
            this.media_desc = new MediaStreamDescription(MediaStreamType.Audio, mediaStreamAttributes);

            mediaStreamDescriptions.Add(this.media_desc);

            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = this.track_duration.Ticks.ToString (CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = true.ToString ();
        }

        protected override void SeekAsync(long seekToTime)
        {
            var time = new TimeSpan (seekToTime);
            q.BeginSeek (time, (result) => { q.EndSeek (result); position = seekToTime; ReportSeekCompleted (seekToTime); }, null);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotSupportedException();
        }
    }
}
