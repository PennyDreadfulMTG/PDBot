using PDBot.Core.API;
using PDBot.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDBot.Core
{
    class UploadOldLogs : ICronObject
    {
        public UploadOldLogs()
        {
            Directory.CreateDirectory(Path.Combine("Logs", "Archive"));
            Directory.CreateDirectory(Path.Combine("Logs", "Failed"));
        }

        public Task EveryHourAsync()
        {
            return Task.FromResult(false);
        }

        public async Task EveryMinuteAsync()
        {
            foreach (var file in Directory.EnumerateFiles("Logs"))
            {
                var age = DateTime.Now.Subtract(File.GetLastWriteTime(file));
                if (age.TotalMinutes < 60)
                    continue;
                var id = int.Parse(Path.GetFileNameWithoutExtension(file));
                if (await DecksiteApi.LogUploadedAsync(id))
                {
                    var destFileName = Path.Combine("Logs", "Archive", id + ".txt");
                    if (File.Exists(destFileName))
                    {
                        Console.WriteLine($"Reuploading {id} to logsite...");
                        File.Delete(destFileName);
                        await DecksiteApi.UploadLogAsync(id);
                        return;
                    }
                    Console.WriteLine($"Archiving log for {id}...");
                    File.Move(file, destFileName);
                    return;
                }
                try
                {

                    Console.WriteLine($"Uploading {id} to logsite...");
                    await DecksiteApi.UploadLogAsync(id);
                    return;
                }
                catch (UriFormatException)
                {
                    // This log is too long.
                    Console.WriteLine($"Upload failed.");
                    var destFileName = Path.Combine("Logs", "Failed", id + ".txt");
                    File.Move(file, destFileName);
                }
            }
        }
    }
}
