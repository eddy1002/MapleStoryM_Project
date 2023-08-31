using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakePacket
{
	#region PrivateMember
	private string packetType;
	private byte[] buffer;
	private long bornTick;
	#endregion

	#region Public
	public void SetPacket(string packetType, byte[] buffer, long bornTick)
	{
		this.packetType = packetType;
		this.buffer = buffer;
		this.bornTick = bornTick;
	}

	public string GetPacketType()
	{
		return packetType;
	}

	public byte[] GetBuffer()
	{
		return buffer;
	}

	public long GetBornTick()
	{
		return bornTick;
	}
	#endregion
}
