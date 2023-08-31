using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FakeListener : MonoBehaviour
{
    #region PrivateMember
    private readonly Queue<FakePacket> packets = new Queue<FakePacket>();
    #endregion

    #region Mono
    protected virtual void Update()
    {
        ListenRequest();
    }
    #endregion

    #region Public
    /// <summary>
    /// 패킷을 큐에 넣는다
    /// </summary>
    /// <param name="packet"></param>
    public void AddPacket(FakePacket packet)
    {
        if (packet != null)
        {
            packets.Enqueue(packet);
        }
    }
    #endregion

    #region Protected
    /// <summary>
    /// 큐에 들어있는 패킷을 순서대로 처리한다
    /// </summary>
    protected void ListenRequest()
    {
        if (packets != null)
        {
            while (packets.Count > 0)
            {
                ProcessPacket(packets.Dequeue());
            }
        }
    }

    /// <summary>
    /// 패킷의 타입을 바탕으로 처리 함수를 호출한다
    /// </summary>
    /// <param name="packet"></param>
    protected virtual void ProcessPacket(FakePacket packet)
    {
        if (packet != null)
        {
            var method = GetType().GetMethod($"On{packet.GetPacketType()}");
            method?.Invoke(this, new object[] { packet });
        }
    }
    #endregion
}
