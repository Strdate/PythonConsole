using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SkylinesPythonShared
{
    public class TcpConversation
    {
        protected Socket _client;
        private byte[] _buffer = new byte[0];
        private Queue<MessageHeader> _msgQueue = new Queue<MessageHeader>();

        protected TcpConversation(Socket client)
        {
            _client = client;
        }

        protected MessageHeader AwaitMessage()
        {
            while(true)
            {
                if (_msgQueue.Count > 0)
                {
                    return _msgQueue.Dequeue();
                }

                byte[] data = new byte[_client.SendBufferSize];
                int j = _client.Receive(data);

                byte[] adata = new byte[_buffer.Length + j];
                Array.Copy(_buffer, 0, adata, 0, _buffer.Length);
                Array.Copy(data, 0, adata, _buffer.Length, j);
                _buffer = new byte[0];

                int pointer = 0;
                while (pointer < adata.Length)
                {
                    if(adata.Length - pointer < 4) {
                        _buffer = new byte[adata.Length - pointer];
                        Array.Copy(adata, pointer, _buffer, 0, adata.Length - pointer);
                        break;
                    }
                    int msgLength = BitConverter.ToInt32(adata, pointer);
                    if (pointer + msgLength + 4 > adata.Length)
                    {
                        _buffer = new byte[adata.Length - pointer];
                        Array.Copy(adata, pointer, _buffer, 0, adata.Length - pointer);
                        break;
                    }
                    byte[] bytesMsg = new byte[msgLength];
                    Array.Copy(adata, pointer + 4, bytesMsg, 0, msgLength);
                    MessageHeader msg = Deserialize(bytesMsg);
                    pointer += msgLength + 4;
                    _msgQueue.Enqueue(msg);
                }
            }
        }

        protected virtual void SendMessage(object obj, string type, long requestId = 0, bool ignoreReturnValue = false)
        {
            MessageHeader msg = new MessageHeader();
            msg.requestId = requestId;
            msg.payload = obj;
            msg.messageType = type;
            _client.Send(Serialize(msg));
        }

        public static MessageHeader Deserialize(byte[] data)
        {
            using (var memoryStream = new MemoryStream(data))
                return (MessageHeader)(new BinaryFormatter()).Deserialize(memoryStream);
        }

        public static byte[] Serialize(MessageHeader obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, obj);
                byte[] res = memoryStream.ToArray();
                byte[] bytes = new byte[res.Length + 4];
                BitConverter.GetBytes(res.Length).CopyTo(bytes, 0);
                res.CopyTo(bytes, 4);
                return bytes;
            }
        }
    }
}
