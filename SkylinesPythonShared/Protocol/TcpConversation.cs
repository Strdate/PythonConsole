using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using JsonFx.Json;

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
            string text = Encoding.UTF8.GetString(data);
            Console.WriteLine(text);
            return XmlDeserializeFromString<MessageHeader>(text);
            
            // return JsonDeserialize<MessageHeader>(data);
            // using (var memoryStream = new MemoryStream(data))
            //     return (MessageHeader)(new BinaryFormatter()).Deserialize(memoryStream);
        }

        public static byte[] Serialize(MessageHeader obj)
        {
            string xml = XmlSerializeToString(obj);
            Console.WriteLine(xml);
            var bytes1 = Encoding.UTF8.GetBytes(xml);
            byte[] bytes = new byte[bytes1.Length + 4];
            BitConverter.GetBytes(bytes1.Length).CopyTo(bytes, 0);
            bytes1.CopyTo(bytes, 4);
            return bytes;

            // byte[] json = JsonSerialize(obj);
            // byte[] result = new byte[json.Length + 4];
            // BitConverter.GetBytes(json.Length).CopyTo(result, 0);
            // json.CopyTo(result, 4);
            // return result;

            // using (var memoryStream = new MemoryStream())
            // {
            //     (new BinaryFormatter()).Serialize(memoryStream, obj);
            //     byte[] res = memoryStream.ToArray();
            //     byte[] bytes = new byte[res.Length + 4];
            //     BitConverter.GetBytes(res.Length).CopyTo(bytes, 0);
            //     res.CopyTo(bytes, 4);
            //     return bytes;
            // }
        }

        public static byte[] JsonSerialize(object obj)
        {
            var settings = new JsonWriterSettings();
            var result = new StringBuilder();
            settings.TypeHintName = "__type";
            var serializer = new JsonWriter(result, settings);
            serializer.Write(obj);
            return Encoding.UTF8.GetBytes(result.ToString());
        }

        public static T JsonDeserialize<T>(string src)
        {
            var settings = new JsonReaderSettings();
            settings.TypeHintName = "__type";
            var deserializer = new JsonReader(src, settings);
            return deserializer.Deserialize<T>();
        }

        public static T JsonDeserialize<T>(byte[] src)
        {
            return JsonDeserialize<T>(Encoding.UTF8.GetString(src));
        }

        public static string XmlSerializeToString(object objectInstance)
        {
            var serializer = new XmlSerializer(objectInstance.GetType());
            var sb = new StringBuilder();

            using (TextWriter writer = new StringWriter(sb)) {
                serializer.Serialize(writer, objectInstance);
            }

            return sb.ToString();
        }

        public static T XmlDeserializeFromString<T>(string objectData)
        {
            return (T)XmlDeserializeFromString(objectData, typeof(T));
        }

        public static object XmlDeserializeFromString(string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData)) {
                result = serializer.Deserialize(reader);
            }

            return result;
        }
    }
}
