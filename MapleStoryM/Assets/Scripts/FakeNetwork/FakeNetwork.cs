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
    /// ������ ��Ŷ�� ������
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
    /// Ŭ���̾�Ʈ�� ��Ŷ�� ������
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
    /// ��Ŷ�� �ʿ��� ������ �ð���ŭ �����ƴ��� ��ȯ
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    protected bool IsDelayPass(FakePacket packet)
    {
        var diffTicks = DateTime.Now.Ticks - packet.GetBornTick();
        return diffTicks >= 0 && new TimeSpan(diffTicks).TotalMilliseconds >= delayTimeMs;
    }

    /// <summary>
    /// �±׸� �������� ��Ŷ �����ʸ� ã�Ƽ� ��ȯ
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
    /// ��Ŷ�� �ӽ÷� �����ϴ� ť�� ������� ó���Ѵ�
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
    /// ��Ŷ�� ���� �����̸�ŭ ��ٸ��� �ش� ��ǥ�� �����Ѵ�
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
