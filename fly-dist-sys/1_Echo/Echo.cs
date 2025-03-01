﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fly_dist_sys._1_Echo
{
    /*
     * 
     // build PublishSingleFile
     dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true -o publish
     
     // PowerShell
     docker run --rm -v  $HOME\fly-dist-sys\fly-dist-sys\publish:/mnt ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w echo --bin /mnt/fly-dist-sys --node-count 1 --time-limit 10

     // Git Bash
     MSYS_NO_PATHCONV=1 docker run --rm -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w echo --bin \
     /maelstrom/mnt/fly-dist-sys --node-count 1 --time-limit 10
     
    // volume logs
    MSYS_NO_PATHCONV=1 docker run --rm -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt \
     -v $HOME/fly-dist-sys/fly-dist-sys/logs:/maelstrom/store \
     ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w echo --bin \
     /maelstrom/mnt/fly-dist-sys --node-count 1 --time-limit 10

{"id":0,"src":"c0","dest":"n0","body":{"type":"init","node_id":"n0","node_ids":["n0"],"msg_id":1}}


{"src":"c1","dest":"n1","body":{"msg_id":1,"type":"echo","echo":"Please echo 45"}}
     
     */
    public static class Echo
    {
        public static async Task TestAsync()
        {
            var node = new Node();

            node.RegisterHandler("echo", async message =>
            {
                try
                {
                    Console.Error.WriteLine("Handling echo message");
                    var body = JsonSerializer.Deserialize<EchoMessageBody>(message.Body)!;
                    if (body == default) throw new ArgumentNullException("Invalid echo message body");
                    body.Type = "echo_ok";
                    await node.ReplyAsync(message, body);
                }
                catch (Exception ex) 
                {
                    Console.Error.WriteLine($"RegisterHandler {ex}");
                }
            });

            await node.RunAsync();
        }
    }
}
