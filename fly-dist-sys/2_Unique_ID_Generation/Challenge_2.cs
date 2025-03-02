using fly_dist_sys._1_Echo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fly_dist_sys._2_Unique_ID_Generation
{
    public static class Challenge_2
    {
        /*
         
        dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:DefineConstants=Challenge_2 -o publish
         
        MSYS_NO_PATHCONV=1 docker run --rm -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w unique-ids --bin \
     /maelstrom/mnt/fly-dist-sys --time-limit 30 --rate 1000 --node-count 3 --availability total --nemesis partition
         */
        public static async Task TestAsync()
        {
            var node = new Node();

            node.RegisterHandler("generate", message =>
            {
                try
                {
                    var body = JsonSerializer.Deserialize<MessageBody>(message.Body)!;
                    if (body == default) return Task.FromException(new ArgumentNullException("Invalid generate message body"));
                    MessageBody_2 resbody = new()
                    {
                        Id = node.GetUniqueId(),
                        InReplyTo = body.MsgId,
                        Type = "generate_ok"
                    };
                    return node.SendAsync(message.Src, resbody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"RegisterHandler {ex}");
                    return Task.FromException(ex);
                }
            });

            await node.RunAsync();
        }
    }
}
