using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeServer : FakeListener
{
	#region PublicMember
	public UIInput inputPing = null;
	#endregion

	#region Mono
	private void Awake()
	{
		if (inputPing != null)
		{
			inputPing.text = Mathf.RoundToInt(FakeNetwork.Instance.delayTimeMs).ToString();
			inputPing.onSubmit = (text) => {
				if (float.TryParse(text, out var value))
				{
					FakeNetwork.Instance.delayTimeMs = value;
				}
			};
		}
	}
	#endregion

	#region Override
	protected override void ProcessPacket(FakePacket packet)
	{
		base.ProcessPacket(packet);
		if (packet != null)
		{
			Debug.LogWarning($"Server RecievePacket {packet.GetPacketType()} {packet.GetBuffer()} {packet.GetBornTick()}");
		}
	}
	#endregion
}
