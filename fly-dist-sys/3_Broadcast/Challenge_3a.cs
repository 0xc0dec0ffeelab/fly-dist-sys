using fly_dist_sys._2_Unique_ID_Generation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fly_dist_sys._3_Broadcast
{
    /*
     
     dotnet publish -r linux-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:DefineConstants=Challenge_3a -o publish
         
        MSYS_NO_PATHCONV=1 docker run --rm -v $HOME/fly-dist-sys/fly-dist-sys/publish:/maelstrom/mnt ghcr.io/0xc0dec0ffeelab/maelstrom:latest test -w broadcast --bin \
     /maelstrom/mnt/fly-dist-sys --node-count 1 --time-limit 20 --rate 10
     

{"src":"c1","dest":"n1","body":{"type":"init","node_id":"n1","node_ids":["n1"],"msg_id":1}}

{"src":"c1","dest":"n1","body":{"type":"topology", "topology": {"n1":[]},"msg_id":1}}

    {"src":"c1","dest":"n1","body":{"type":"broadcast", "message": 1000,"msg_id":1}}
    {"src":"c1","dest":"n1","body":{"type":"broadcast", "message": 2000,"msg_id":2}}
    {"src":"c1","dest":"n1","body":{"type":"broadcast", "message": 3000,"msg_id":3}}
    
    {"src":"c1","dest":"n1","body":{"type":"read", "msg_id":1}}
    {"src":"c1","dest":"n1","body":{"type":"read", "msg_id":2}}
    {"src":"c1","dest":"n1","body":{"type":"read", "msg_id":3}}
     */
    public static class Challenge_3a
    {
        public static async Task TestAsync()
        {
            SemaphoreSlim _messagesSemaphore = new(1, 1);
            SemaphoreSlim _topologiesSemaphore = new(1, 1);
            List<int> _messages = new();
            ConcurrentDictionary<string, List<string>> _topologies = new();

            var node = new Node();

            node.RegisterHandler("broadcast", async message =>
            {
                try
                {
                    var body = JsonSerializer.Deserialize<BroadcastMessageBody>(message.Body)!;
                    if (body == default) throw new ArgumentNullException("Invalid generate message body");

                    await node.AddTaskAsync(_messages, _messagesSemaphore, body.Message);

                    MessageBody resbody = new()
                    {
                        InReplyTo = body.MsgId,
                        Type = "broadcast_ok"
                    };
                    await node.SendAsync(message.Src, resbody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"RegisterHandler {ex}");
                }
            });

            node.RegisterHandler("read", message =>
            {
                try
                {
                    var body = JsonSerializer.Deserialize<MessageBody>(message.Body)!;
                    if (body == default) return Task.FromException(new ArgumentNullException("Invalid generate message body"));
                    ReadMessageBody resbody = new()
                    {
                        Messages = _messages,
                        InReplyTo = body.MsgId,
                        Type = "read_ok"
                    };
                    return node.SendAsync(message.Src, resbody);
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                    //Console.Error.WriteLine($"RegisterHandler {ex}");
                }
            });

            node.RegisterHandler("topology", async message =>
            {
                try
                {
                    var body = JsonSerializer.Deserialize<TopologyMessageBody>(message.Body)!;
                    if (body == default) throw new ArgumentNullException("Invalid generate message body");

                    await node.UpdateConcurrentDictionaryListAsync(body.Topology, _topologies, _topologiesSemaphore);
                    
                    MessageBody resbody = new()
                    {
                        InReplyTo = body.MsgId,
                        Type = "topology_ok"
                    };
                    await node.SendAsync(message.Src, resbody);
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
