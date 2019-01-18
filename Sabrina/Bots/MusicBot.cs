using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.VoiceNext;

namespace Sabrina.Bots
{
    class MusicBot
    {
        private DiscordClient _client;
        private static readonly string fileToPlay = "audio.mp3";
        public static VoiceNextClient VNext;

        public MusicBot(DiscordClient client)
        {
            _client = client;
            Task.Run(ContinuousPlay);
        }

        private async Task ContinuousPlay()
        {
            if(!File.Exists(fileToPlay))
            {
                return;
            }

            while (VNext == null)
            {
                await Task.Delay(100);
            }

            var voiceChannel = await _client.GetChannelAsync(451197681835048960);
            var vnc = await VNext.ConnectAsync(voiceChannel);

            // play
            Exception exc = null;
            await vnc.SendSpeakingAsync(true);
            try
            {

                var ffmpeg_inf = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i \"{fileToPlay}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                var ffmpeg = Process.Start(ffmpeg_inf);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                // let's buffer ffmpeg output
                using (var ms = new MemoryStream())
                {
                    await ffout.CopyToAsync(ms);
                    ms.Position = 0;

                    var buff = new byte[3840]; // buffer to hold the PCM data
                    var br = 0;
                    while ((br = ms.Read(buff, 0, buff.Length)) > 0)
                    {
                        if (br < buff.Length) // it's possible we got less than expected, let's null the remaining part of the buffer
                            for (var i = br; i < buff.Length; i++)
                                buff[i] = 0;

                        await vnc.SendAsync(buff, 20); // we're sending 20ms of data
                    }
                }
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
            }
        }
    }
}
