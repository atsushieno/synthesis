using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Media;
using Commons.Media.Streaming;

namespace Commons.Media.Streaming.Phone
{
    public class BufferGeneratorMediaStreamSource : MediaStreamSource
    {
        static readonly double tick_ratio = TimeSpan.TicksPerMillisecond / 10.0;

        public static long ToTick (TimeSpan duration)
        {
            return (long) (duration.Ticks / tick_ratio);
        }

        IMediaBufferGenerator gen;
        AudioParameters parameters;
        TimeSpan track_duration;

        public BufferGeneratorMediaStreamSource (IMediaBufferGenerator generator, AudioParameters parameters, TimeSpan trackDuration)
        {
            if (gen == null)
                throw new ArgumentNullException ("generator");
            parameters = parameters ?? AudioParameters.Default;
            this.gen = generator;
            track_duration = trackDuration;
        }

        protected override void CloseMedia()
        {
            gen.Stop ();
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotSupportedException();
        }

        Dictionary<MediaSampleAttributeKeys, string> empty_atts = new Dictionary<MediaSampleAttributeKeys, string>();
        MediaStreamDescription media_desc;
        long position;

        Queue<IMediaSample> samples;
        int stuck_count;

        void ReportGetSampleCompleted (IMediaSample sample)
        {
            ArraySegment<byte> buf = sample.GetBuffer<byte> ();
            position += ToTick (sample.Duration);
            var s = new MediaStreamSample (media_desc, new MemoryStream (buf.Array), buf.Offset, buf.Count, position, empty_atts);
            this.ReportGetSampleCompleted (s);
        }

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            if (mediaStreamType != MediaStreamType.Audio)
                throw new InvalidOperationException ("Only audio stream type is supported");
            if (samples == null) {
                samples = new Queue<IMediaSample> ();
                gen.BufferArrived += sample => {
                    if (stuck_count-- > 0)
                        this.ReportGetSampleCompleted (sample);
                    else
                        samples.Enqueue (sample);
                };
            }
            if (samples.Count > 0)
                ReportGetSampleCompleted (samples.Dequeue ());
            else
                stuck_count++;
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

        Action<TimeSpan> seek;

        protected override void SeekAsync(long seekToTime)
        {
            var time = new TimeSpan (seekToTime);
            if (seek == null)
                seek = gen.SeekTo;
            seek.BeginInvoke (time, result => {
                seek.EndInvoke (result);
                position = seekToTime;
                ReportSeekCompleted (seekToTime);
            }, null);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotSupportedException();
        }
    }
}
