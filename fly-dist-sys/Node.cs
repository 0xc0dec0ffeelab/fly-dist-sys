using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace fly_dist_sys
{
    public class Node
    {
		public string? NodeId { get; set; }
		public List<string>? NodeIds { get; set; } = Enumerable.Empty<string>().ToList();

        int _nextMessageId;

        readonly ConcurrentDictionary<string, Func<Message, Task>> _handlers = new();

        readonly ConcurrentDictionary<int, Func<Message, Task>> _callbacks = new();
        
        readonly List<Task> _runningTasks = new();

        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        public Node()
        {
            //RegisterHandler("init", HandleInitMessageAsync);
        }

        private async Task HandleInitMessageAsync(Message message)
        {
            Console.Error.WriteLine($"HandleInitMessageAsync: ==============> Before");
            var body = JsonSerializer.Deserialize<InitMessageBody>(message.Body);

            if (body == default) throw new ArgumentNullException("Invalid init message body");

            NodeIds = body.Ids;
            NodeId = body.Id;

            if (_handlers.TryGetValue("init", out var hanadler))
            {
                await hanadler(message);
            }
            
            await ReplyAsync(message, new MessageBody { Type = "init_ok" });
        }

        public async Task ReplyAsync<T>(Message request, T body) where T : MessageBody
        {
            Console.Error.WriteLine($"ReplyAsync<T>: ==============> Before");
            var reqBody = JsonSerializer.Deserialize<T>(request.Body);
            if (reqBody == null) throw new Exception("Invalid request T body");

            body.InReplyTo = reqBody.MsgId;

            await SendAsync(request.Src, body);

        }

        public async Task ReplyAsync(Message request, MessageBody body)
        {
            Console.Error.WriteLine($"ReplyAsync: ==============> Before");
            var reqBody = JsonSerializer.Deserialize<MessageBody>(request.Body);
            if (reqBody == null) throw new Exception("Invalid request message body");

            body.InReplyTo = reqBody.MsgId;
            
            await SendAsync(request.Src, body);

        }
        public async Task ReplyAsync(Message request, RPCError error)
        {
            Console.Error.WriteLine($"ReplyAsync RPCError: ==============> Before");
            var reqBody = JsonSerializer.Deserialize<MessageBody>(request.Body);
            
            if (reqBody == null) throw new Exception("Invalid request message body");

            var body = new MessageBody
            {
                Type = "error",
                Code = error.Code,
                Text = error.Message,
                InReplyTo = reqBody.MsgId
            };

            await SendAsync(request.Src, body);

        }
        public async Task SendAsync(string? dest, object body, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = new Message
                {
                    Src = NodeId,
                    Dest = dest,
                    Body = JsonSerializer.SerializeToElement(body)
                };

                var jsonMessage = JsonSerializer.Serialize(message);

                await _semaphore.WaitAsync(cancellationToken);
                try
                {
                    await Console.Out.WriteLineAsync(jsonMessage.AsMemory(), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    //Console.Error.WriteLine("SendAsync operation was canceled.");
                    throw;
                }
                finally
                {
                    _semaphore.Release();
                }

            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void RegisterHandler(string type, Func<Message, Task> handler)
        {
            _handlers.TryAdd(type, handler);
        }

        public void RegisterCallback(int msgId, Func<Message, Task> callback)
        {
            _callbacks.TryAdd(msgId, callback);
        }
        private async Task HandleCallbackAsync(Func<Message, Task> callback, Message message)
        {
            try
            {
                await callback(message);
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"Callback error: {ex}");
            }
        }
        private async Task HandleMessageAsync(Func<Message, Task> handler, Message message)
        {
            try
            {
                await handler(message);
            }
            catch (RPCError ex)
            {
                await ReplyAsync(message, ex);
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"Exception handling {JsonSerializer.Serialize(message)}:\n{ex}");
                // Crash = 13
                await ReplyAsync(message, new RPCError(13, ex.Message));
            }
        }

        public async Task<bool> RPCAsync(string dest, object body, Func<Message, Task> callback, CancellationToken cancellationToken)
        {
            int msgID = Interlocked.Increment(ref _nextMessageId);
            _callbacks.TryAdd(msgID, callback);
            Console.Error.WriteLine($"RPCAsync : ==============> Before");
            var bodyDict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(body));
            if (bodyDict == null) return false;
            
            bodyDict["msg_id"] = msgID;

            try
            {
                await SendAsync(dest, bodyDict, cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                _callbacks.TryRemove(msgID, out _);
                return false;
            }
        }

        public async Task<(Message?, RPCError?)> SyncRPCAsync(CancellationToken cancellationToken, string dest, object body)
        {
            var tcs = new TaskCompletionSource<Message>();

            var task = RPCAsync(dest, body, (Message message) =>
            {
                tcs.TrySetResult(message);
                return Task.CompletedTask;
            }, cancellationToken);


            if (task.IsFaulted)
            {
                return (null, new RPCError(1, "Failed to send RPC request"));
            }

            try
            {
                var message = await tcs.Task.WaitAsync(cancellationToken);
                return (message, null);
            }
            catch(OperationCanceledException)
            {
                return (null, new RPCError(2, "RPC request timed out"));
            }

        }

        public async Task RunAsync()
        {
            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null)
            {
                Console.Error.WriteLine($"{line}");
                //continue;

                try
                {
                    Console.Error.WriteLine($"message: ==============> Before");
                    var message = JsonSerializer.Deserialize<Message>(line);
                    Console.Error.WriteLine($"message: ==============> After");

                    if (message == null) continue;
                    
                    Console.Error.WriteLine($"message: ==============> {message}");

                    var body = JsonSerializer.Deserialize<MessageBody>(message.Body);
                    if (body == null) continue;

                    Console.Error.WriteLine($"body: ==============> {body}");

                    //Console.Error.WriteLine($"Received {JsonSerializer.Serialize(message)}");

                    if (body.InReplyTo != 0)
                    {
                        if (_callbacks.TryRemove(body.InReplyTo, out var callback) && callback != null)
                        {
                            _runningTasks.Add(HandleCallbackAsync(callback, message));
                        }
                    }

                    Func<Message, Task>? handler = default;
                    if (body.Type == "init")
                    {
                        handler = HandleInitMessageAsync;
                    }
                    else if (_handlers.TryGetValue(body.Type, out handler) == false)
                    {
                        //Console.Error.WriteLine($"No handler for {line}");
                        continue;
                    }

                    _runningTasks.Add(HandleMessageAsync(handler, message));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing input: {ex}");
                }
            }

            await Task.WhenAll(_runningTasks);
            // clear completed tasks
            _runningTasks.RemoveAll(task => task.IsCompleted);
        }

    }
}
