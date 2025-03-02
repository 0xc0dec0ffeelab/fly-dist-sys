using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
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
        
        readonly List<Task> _messageTasks = new();
        readonly List<Task> _callbackTasks = new();
        readonly List<Task> _messageTasksToWait = new();
        readonly List<Task> _callbackTasksToWait = new();
        readonly SemaphoreSlim _messageTaskSemaphore = new(1, 1);
        readonly SemaphoreSlim _callbackTaskSemaphore = new(1, 1);

        static readonly SemaphoreSlim _sendSemaphore = new(1, 1);
        
       
        public Node()
        {
        }
        private async Task ProcessTaskListAsync(List<Task> taskList, List<Task> tasksToWait, SemaphoreSlim semaphore)
        {
            while (true)
            {
                await semaphore.WaitAsync();
                try
                {
                    taskList.RemoveAll(task => task.IsCompleted);
                    tasksToWait.Clear();
                    tasksToWait.AddRange(taskList);
                }
                finally
                {
                    semaphore.Release();
                }

                if (tasksToWait.Count > 0)
                {
                    await Task.WhenAll(tasksToWait);
                }

                await Task.Delay(1000);
            }
        }
        public async Task UpdateConcurrentDictionaryListAsync<T, K>(ConcurrentDictionary<T, List<K>> fromDict, ConcurrentDictionary<T, List<K>> toDict, SemaphoreSlim semaphore) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(fromDict);
            ArgumentNullException.ThrowIfNull(toDict);

            await semaphore.WaitAsync();
            try
            {
                foreach (var kvp in fromDict)
                {
                    toDict[kvp.Key] = new List<K>(kvp.Value);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }
        public async Task AddTaskAsync<T>(List<T> taskList, SemaphoreSlim semaphore, T task)
        {
            await semaphore.WaitAsync();
            try
            {
                taskList.Add(task);
            }
            finally
            {
                semaphore.Release();
            }
        }
        //private async Task AddTaskAsync(List<Task> taskList, SemaphoreSlim semaphore, Task task)
        //{
        //    await semaphore.WaitAsync();
        //    try
        //    {
        //        taskList.Add(task);
        //    }
        //    finally
        //    {
        //        semaphore.Release();
        //    }
        //}

        public string GetUniqueId()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000
                         + (DateTime.UtcNow.Ticks % TimeSpan.TicksPerMillisecond) * 100;

            Span<byte> buffer = stackalloc byte[8];
            RandomNumberGenerator.Fill(buffer);
            long randomNum = BitConverter.ToInt64(buffer);

            return $"{timestamp}{randomNum}";
        }
        private Task HandleInitMessageAsync(Message message)
        {
            var body = JsonSerializer.Deserialize<InitMessageBody>(message.Body);

            if (body == default) throw new ArgumentNullException("Invalid init message body");

            NodeIds = body.NodeIds;
            NodeId = body.NodeId;

            if (_handlers.TryGetValue("init", out var hanadler))
            {
                return hanadler(message);
            }

            var resBody = new MessageBody
            {
                Type = "init_ok",
                InReplyTo = body.MsgId
            };
            return SendAsync(message.Src, resBody);
        }

        public Task ReplyAsync(Message request, RPCError error)
        {
            var reqBody = JsonSerializer.Deserialize<MessageBody>(request.Body);
            
            if (reqBody == null) throw new Exception("Invalid request message body");

            var body = new MessageBody
            {
                Type = "error",
                Code = error.Code,
                Text = error.Message,
                InReplyTo = reqBody.MsgId
            };

            return SendAsync(request.Src, body);

        }
        public Task SendAsync(string? dest, object body, CancellationToken cancellationToken = default)
        {
            var message = new Message
            {
                Src = NodeId,
                Dest = dest,
                Body = JsonSerializer.SerializeToElement(body)
            };

            var jsonMessage = JsonSerializer.Serialize(message);

            return _sendSemaphore.WaitAsync(cancellationToken)
                .ContinueWith(_ =>
                {
                    try
                    {
                        return Console.Out.WriteLineAsync(jsonMessage.AsMemory(), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    finally
                    {
                        _sendSemaphore.Release();
                    }
                }, cancellationToken).Unwrap();
        }


        //public async Task SendAsync(string? dest, object body, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        var message = new Message
        //        {
        //            Src = NodeId,
        //            Dest = dest,
        //            Body = JsonSerializer.SerializeToElement(body)
        //        };

        //        var jsonMessage = JsonSerializer.Serialize(message);

        //        await _sendSemaphore.WaitAsync(cancellationToken);
        //        try
        //        {
        //            await Console.Out.WriteLineAsync(jsonMessage.AsMemory(), cancellationToken);
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            //Console.Error.WriteLine("SendAsync operation was canceled.");
        //            throw;
        //        }
        //        finally
        //        {
        //            _sendSemaphore.Release();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //Console.Error.WriteLine($"Error sending message: {ex.Message}");
        //    }
        //}

        public void RegisterHandler(string type, Func<Message, Task> handler)
        {
            _handlers.TryAdd(type, handler);
        }

        public void RegisterCallback(int msgId, Func<Message, Task> callback)
        {
            _callbacks.TryAdd(msgId, callback);
        }
        private Task HandleCallbackAsync(Func<Message, Task> callback, Message message)
        {
            try
            {
                return callback(message);
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"Callback error: {ex}");
                return Task.FromException(ex);
            }
        }
        private Task HandleMessageAsync(Func<Message, Task> handler, Message message)
        {
            try
            {
                return handler(message);
            }
            catch (RPCError ex)
            {
                return ReplyAsync(message, ex);
            }
            catch (Exception ex)
            {
                //Console.Error.WriteLine($"Exception handling {JsonSerializer.Serialize(message)}:\n{ex}");
                // Crash = 13
                return ReplyAsync(message, new RPCError(13, ex.Message));
            }
        }

        public async Task<bool> RPCAsync(string dest, object body, Func<Message, Task> callback, CancellationToken cancellationToken)
        {
            int msgID = Interlocked.Increment(ref _nextMessageId);
            _callbacks.TryAdd(msgID, callback);
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
            _ = Task.Run(() => ProcessTaskListAsync(_messageTasks, _messageTasksToWait, _messageTaskSemaphore));
            _ = Task.Run(() => ProcessTaskListAsync(_callbackTasks, _callbackTasksToWait, _callbackTaskSemaphore));

            string? line;
            while ((line = await Console.In.ReadLineAsync()) != null)
            {
                //Console.Error.WriteLine($"{line}");

                try
                {
                    var message = JsonSerializer.Deserialize<Message>(line);

                    if (message == null) continue;
                    
                    var body = JsonSerializer.Deserialize<MessageBody>(message.Body);
                    if (body == null) continue;

                    if (body.InReplyTo.HasValue && body.InReplyTo != 0)
                    {
                        if (_callbacks.TryRemove(body.InReplyTo.Value, out var callback) && callback != null)
                        {
                            await AddTaskAsync(_callbackTasks, _callbackTaskSemaphore, HandleCallbackAsync(callback, message));
                        }
                        continue;
                    }

                    Func<Message, Task>? handler = default;
                    if (body.Type == "init")
                    {
                        handler = HandleInitMessageAsync;
                    }
                    else if (_handlers.TryGetValue(body.Type, out handler) == false)
                    {
                        continue;
                    }

                    await AddTaskAsync(_messageTasks, _messageTaskSemaphore ,HandleMessageAsync(handler, message));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error processing input: {ex}");
                }
            }
        }

    }
}
