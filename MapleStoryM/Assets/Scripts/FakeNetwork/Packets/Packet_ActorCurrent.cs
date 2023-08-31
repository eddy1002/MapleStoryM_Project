using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Packet_ActorCurrent
{
    #region PublicMember
    public long actorID;
    public float currentX;
    public float velocity;
    public float remainOrderTime;
    #endregion

    #region Public
    /// <summary>
    /// 패킷을 바이트 배열로 변환
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    public static byte[] ToByteArray(Packet_ActorCurrent packet)
    {
        var stream = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, packet.actorID);
        formatter.Serialize(stream, packet.currentX);
        formatter.Serialize(stream, packet.velocity);
        formatter.Serialize(stream, packet.remainOrderTime);
        return stream.ToArray();
    }

    /// <summary>
    /// 바이트 배열을 패킷으로 변환
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public static Packet_ActorCurrent FromByteArray(byte[] buffer)
    {
        var stream = new MemoryStream(buffer);
        var formatter = new BinaryFormatter();
        var packet = new Packet_ActorCurrent
        {
            actorID = (long)formatter.Deserialize(stream),
            currentX = (float)formatter.Deserialize(stream),
            velocity = (float)formatter.Deserialize(stream),
            remainOrderTime = (float)formatter.Deserialize(stream)
        };
        return packet;
    }
    #endregion
}
