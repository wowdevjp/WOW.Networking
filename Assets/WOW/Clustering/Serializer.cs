using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.XR;

namespace WOW.Clustering
{
    public static class Serializer
    {
        public static ValueToBinarySerializer BinarySerializer { get => new ValueToBinarySerializer(); }
        public static BinaryToValueSerializer ValueSerializer { get => new BinaryToValueSerializer(); }
    }

    public class BinaryToValueSerializer : ISerializer, IDisposable
    {
        private int dataCount = 0;
        private MemoryStream stream = null;
        private BinaryReader reader = null;

        public BinaryToValueSerializer SetBuffer(byte[] buffer)
        {
            stream = new MemoryStream(buffer);
            reader = new BinaryReader(stream);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            dataCount = reader.ReadInt32();

            return this;
        }

        public void Serialize<T>(ref T value)
        {
            System.Type valueType = typeof(T);

            if(valueType == typeof(byte[]))
            {
                int length = reader.ReadInt32();
                value = (T)(object)reader.ReadBytes(length); 
            }
            else if (valueType.IsEnum)
            {
                reader.ReadInt32();
                value = (T)(object)reader.ReadInt32();
            }
            else if (valueType == typeof(int))
            {
                reader.ReadInt32();
                value = (T)(object)reader.ReadInt32();
            }
            else if (valueType == typeof(double))
            {
                reader.ReadInt32();
                value = (T)(object)reader.ReadDouble();
            }
            else if (valueType == typeof(bool))
            {
                reader.ReadInt32();
                value = (T)(object)reader.ReadBoolean();
            }
            else if (valueType == typeof(string))
            {
                int length = reader.ReadInt32();
                value = (T)(object)System.Text.Encoding.UTF8.GetString(reader.ReadBytes(length));
            }
        }

        public void Dispose()
        {
            stream?.Dispose();
            reader?.Dispose();
            stream = null;
            reader = null;
        }
    }

    public class ValueToBinarySerializer : ISerializer, IDisposable
    {
        private int dataCount = 0;
        private List<byte[]> bytes = new List<byte[]>();

        public void Serialize<T>(ref T value)
        {
            System.Type valueType = typeof(T);

            if(valueType == typeof(byte[]))
            {
                bytes.Add((byte[])(object)value);
            }
            else if(valueType.IsEnum)
            {
                var integerBytes = BitConverter.GetBytes((int)(object)value);
                bytes.Add(integerBytes);
            }
            else if(valueType == typeof(int))
            {
                var integerBytes = BitConverter.GetBytes((int)(object)value);
                bytes.Add(integerBytes);
            }
            else if(valueType == typeof(double))
            {
                var doubleBytes = BitConverter.GetBytes((double)(object)value);
                bytes.Add(doubleBytes);
            }
            else if(valueType == typeof(bool))
            {
                var boolBytes = BitConverter.GetBytes((bool)(object)value);
                bytes.Add(boolBytes);
            }
            else if(valueType == typeof(string))
            {
                var stringBytes = System.Text.Encoding.UTF8.GetBytes((string)(object)value);
                bytes.Add(stringBytes);
            }

            dataCount++;
        }

        public byte[] GetBuffer()
        {
            using(var stream = new MemoryStream())
            using(var writer = new BinaryWriter(stream))
            {
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write(dataCount);
                
                foreach(var b in bytes)
                {
                    writer.Write(b.Length);
                    writer.Write(b);
                }

                byte[] buffer = stream.ToArray();
                return buffer;
            }
        }

        public void Dispose()
        {
            bytes.Clear();
            bytes = null;
        }
    }
}