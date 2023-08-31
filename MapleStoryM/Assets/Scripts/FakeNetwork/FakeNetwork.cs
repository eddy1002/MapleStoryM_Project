using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FakeNetwork : SingleTonMono<FakeNetwork>
{
    #region PublicMember
    public float delayTimeMs = 200f;
    #endregion

    #region PrivateMember
    private Thread networkThread;
    private readonly Queue<FakePacket> toServer = new Queue<FakePacket>();
    private readonly Queue<FakePacket> toClinet = new Queue<FakePacket>();
    private FakeListener server = null;
    private FakeListener client = null;
    #endregion

    #region Mono
    protected override void Awake()
    {
        base.Awake();
        networkThread = new Thread(new ThreadStart(SendingPacket))
        {
            IsBackground = true
        };
        networkThread.Start();
    }

    private void Update()
    {
        if (server == null)
        {
            GetServer();
        }
        if (client == null)
        {
            GetClient();
        }
    }
    #endregion

    #region Public
    /// <summary>
    /// 서버로 패킷을 보낸다
    /// </summary>
    /// <param name="packet"></param>
    public void SendPacketToServer(FakePacket packet)
    {
        if (packet != null)
        {
            toServer?.Enqueue(packet);
        }
    }

    /// <summary>
    /// 클라이언트로 패킷을 보낸다
    /// </summary>
    /// <param name="packet"></param>
    public void SendPacketToClient(FakePacket packet)
    {
        if (packet != null)
        {
            toClinet?.Enqueue(packet);
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// 패킷이 필요한 딜레이 시간만큼 지연됐는지 반환
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    protected bool IsDelayPass(FakePacket packet)
    {
        var diffTicks = DateTime.Now.Ticks - packet.GetBornTick();
        return diffTicks >= 0 && new TimeSpan(diffTicks).TotalMilliseconds >= delayTimeMs;
    }

    /// <summary>
    /// 태그를 바탕으로 패킷 리스너를 찾아서 반환
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    protected FakeListener GetListener(string tag)
    {
        var obj = GameObject.FindGameObjectWithTag(tag);
        if (obj != null && obj.TryGetComponent<FakeListener>(out var listener))
        {
            return listener;
        }
        return null;
    }

    protected FakeListener GetServer()
    {
        if (server == null)
        {
            server = GetListener("Server");
        }
        return server;
    }

    protected FakeListener GetClient()
    {
        if (client == null)
        {
            client = GetListener("Client");
        }
        return client;
    }

    /// <summary>
    /// 패킷을 임시로 보관하는 큐를 순서대로 처리한다
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="action"></param>
    protected void ProcessQueue(Queue<FakePacket> queue, Action<FakePacket> action)
    {
        if (queue != null)
        {
            while (queue.Count > 0)
            {
                var packet = queue.Peek();
                if (packet != null)
                {
                    if (IsDelayPass(packet))
                    {
                        queue.Dequeue();
                        action?.Invoke(packet);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 패킷을 일정 딜레이만큼 기다리고 해당 목표로 전달한다
    /// </summary>
    protected void SendingPacket()
    {
        try
        {
            while (true)
            {
                ProcessQueue(toServer, (packet) =>
                {
                    if (server != null)
                    {
                        server.AddPacket(packet);
                    }
                });
                ProcessQueue(toClinet, (packet) =>
                {
                    if (client != null)
                    {
                        client.AddPacket(packet);
                    }
                });
            }
        }
        catch (Exception packetException)
        {
            Debug.Log("PacketException : " + packetException.ToString());
        }
    }
    #endregion
}
