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
    /// ��Ŷ�� ť�� �ִ´�
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
    /// ť�� ����ִ� ��Ŷ�� ������� ó���Ѵ�
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
    /// ��Ŷ�� Ÿ���� �������� ó�� �Լ��� ȣ���Ѵ�
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
