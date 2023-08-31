using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeClient : FakeListener
{
	#region PublicMember
	public PlayerClient playerSync = null;
	public PlayerClient playerForced = null;
	#endregion

	#region Override
	protected override void ProcessPacket(FakePacket packet)
	{
		base.ProcessPacket(packet);
	}
	#endregion

	#region Network
	public void OnSyncActorCurrent(FakePacket packet)
	{
		if (packet != null)
		{
			var actorCurrent = Packet_ActorCurrent.FromByteArray(packet.GetBuffer());
			if (actorCurrent != null)
			{
				if (playerSync != null)
				{
					playerSync.SyncPosition(packet, actorCurrent);
				}
				if (playerForced != null)
				{
					playerForced.ForcedPosition(actorCurrent);
				}
			}
		}
	}
	#endregion
}
