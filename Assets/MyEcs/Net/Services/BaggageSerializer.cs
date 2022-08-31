using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using System.Reflection;
using Mirror;
using MyEcs.Spawn;

namespace MyEcs.Net
{
    public static class BaggageSerializer
    {

        static Dictionary<Type, byte> typeToId;
        static List<Type> idToType;
        static bool _init = false;
        public static void Init()
        {
            if (_init)
                return;
            _init = true;
            typeToId = new Dictionary<Type, byte>();
            idToType = TypeSerializer.ProduceIdList(typeof(IBaggage));
            for (byte i = 0; i < idToType.Count; i++)
            {
                typeToId.Add(idToType[i], i);
            }
        }
        public static Type GetType(byte id)
        {
            return idToType[id];
        }
        public static byte GetObjectId(object obj)
        {
            return GetTypeId(obj.GetType());
        }
        public static byte GetTypeId(Type type)
        {
            return typeToId[type];
        }

        static object[] oneArgBuffer = new object[1];
        static object[] zeroArgBuffer = new object[0];
        public static void WriteBaggagePayload(this NetworkWriter writer, BaggagePayload payload)
        {
            byte len = (byte)payload.List.Count;
            writer.WriteByte(len);
            for (byte i = 0; i < len; i++)
            {
                oneArgBuffer[0] = payload.List[i];
                Type type = oneArgBuffer[0].GetType();
                writer.WriteByte(GetTypeId(type));
                typeof(NetworkWriter)
                    .GetMethod("Write")
                    .MakeGenericMethod(type)
                    .Invoke(writer, oneArgBuffer);
            }
        }
        public static BaggagePayload ReadBaggagePayload(this NetworkReader reader)
        {
            var msg = new BaggagePayload();
            byte len = reader.ReadByte();
            for (byte i = 0; i < len; i++)
            {
                byte id = reader.ReadByte();
                Type type = GetType(id);
                object obj = typeof(NetworkReader)
                    .GetMethod("Read")
                    .MakeGenericMethod(type)
                    .Invoke(reader, zeroArgBuffer);
                oneArgBuffer[0] = obj;
                typeof(BaggagePayload).GetMethod("Add")
                    .MakeGenericMethod(type)
                    .Invoke(msg, oneArgBuffer);
            }
            return msg;
        }
    }
}