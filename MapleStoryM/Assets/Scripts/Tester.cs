using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    #region PublicMember
    public long ID = 0;
    public float sendTick = 0.5f;
    public float velocity = 3f;
    public float orderTime = 5f;
    #endregion

    #region PrivateMember
    private float myVelocity = 0f;
    private float sendTimer = 0f;
    private float orderTimer = 0f;
    #endregion

    #region Mono
    private void Update()
    {
        SendTimerDown();
        OrderTimerDown();
        OnUpdate();
    }
	#endregion

	#region Private
    /// <summary>
    /// 현재 위치를 패킷으로 전송
    /// </summary>
    private void SendMyCurrent()
	{
        sendTimer = sendTick;
        var actorCurrent = new Packet_ActorCurrent
        {
            actorID = ID,
            currentX = transform.position.x,
            velocity = myVelocity,
            remainOrderTime = orderTimer
        };
        var packet = new FakePacket();
        packet.SetPacket("SyncActorCurrent", Packet_ActorCurrent.ToByteArray(actorCurrent), System.DateTime.Now.Ticks);
        FakeNetwork.Instance.SendPacketToClient(packet);
    }

    /// <summary>
    /// 주기적으로 위치를 전송하는 타이머 갱신
    /// </summary>
    private void SendTimerDown()
    {
        if (sendTimer <= 0f)
        {
            SendMyCurrent();
        }
        else
        {
            sendTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 방향 전환 명령 함수
    /// </summary>
    private void Order()
	{
        orderTimer = orderTime * Random.Range(0.75f, 1.25f);
        myVelocity = myVelocity == 0f ? velocity : -myVelocity;
        SendMyCurrent();
    }

    /// <summary>
    /// 방향 전환을 위한 타이머 갱신
    /// </summary>
    private void OrderTimerDown()
    {
        if (orderTimer <= 0f)
        {
            Order();
        }
        else
        {
            orderTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 카메라 경계의 월드 좌표를 반환
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private float GetCameraPositionX(Vector3 position)
	{
        return Camera.main.WorldToViewportPoint(position).x;
    }

    private void OnUpdate()
    {
        var beforeX = GetCameraPositionX(transform.position);
        var current = transform.position;
        current.x += myVelocity * Time.deltaTime;
        var nextX = GetCameraPositionX(current);
        // 벽에 충돌하면 방향 전환
        if ((nextX > 1f && nextX > beforeX) || (nextX < 0f && nextX < beforeX))
        {
            Order();
        }
        else
        {
            transform.position = current;
        }
    }
    #endregion
}
