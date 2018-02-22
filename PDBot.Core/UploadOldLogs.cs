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
        }

        public Task EveryHour()
        {
            return Task.FromResult(false);
        }

        public async Task EveryMinute()
        {
            foreach (var file in Directory.EnumerateFiles("Logs"))
            {
                var age = DateTime.Now.Subtract(File.GetLastWriteTime(file));
                if (age.TotalMinutes < 60)
                    continue;
                var id = int.Parse(Path.GetFileNameWithoutExtension(file));
                if (DecksiteApi.LogUploaded(id))
                {
                    string destFileName = Path.Combine("Logs", "Archive", id + ".txt");
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
                Console.WriteLine($"Uploading {id} to logsite...");
                await DecksiteApi.UploadLogAsync(id);
                return;
            }
        }
    }
}
