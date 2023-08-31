using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClient : MonoBehaviour
{
	#region PublicMember
	public UIButton showInventoryButton = null;
	public LoopScrollView inventory = null;
	#endregion

	#region PrivateMember
	private float velocity = 0f;
	private float orderTimer = 0f;
	#endregion

	#region Mono
	private void Awake()
	{
		if (showInventoryButton != null)
		{
			showInventoryButton.OnClick = () =>
			{
				if (inventory != null)
				{
					inventory.gameObject.SetActive(!inventory.gameObject.activeSelf);
					if (inventory.gameObject.activeSelf)
					{
						inventory.InitScroll(UpdateInventoryCell);
						inventory.MakeList(100);
						inventory.RefreshScroll(true);
					}
				}
			};
		}
	}

	private void Update()
	{
		OnUpdate();
	}
	#endregion

	#region Public
	/// <summary>
	/// 동기화 방식을 사용하지 않고 강제적으로 플레이어 위치 적용
	/// </summary>
	/// <param name="actorCurrent"></param>
	public void ForcedPosition(Packet_ActorCurrent actorCurrent)
	{
		var current = transform.position;
		current.x = actorCurrent.currentX;
		velocity = actorCurrent.velocity;
		transform.position = current;
	}

	/// <summary>
	/// 동기화 방식을 적용하여 플레이어 위치 적용
	/// </summary>
	/// <param name="packet"></param>
	/// <param name="actorCurrent"></param>
	public void SyncPosition(FakePacket packet, Packet_ActorCurrent actorCurrent)
	{
		var diffTicks = DateTime.Now.Ticks - packet.GetBornTick();
		var diffSeconds = new TimeSpan(diffTicks).TotalMilliseconds * 0.001f;
		var endPosition = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

		// 벽에 충돌까지 남은 시간을 구합니다.
		var remainBumpTime = -1f;
		if (actorCurrent.velocity > 0f)
		{
			remainBumpTime = (endPosition - actorCurrent.currentX) / actorCurrent.velocity;
		}
		else if (actorCurrent.velocity < 0f)
		{
			remainBumpTime = (actorCurrent.currentX + endPosition) / -actorCurrent.velocity;
		}

		var current = transform.position;
		// 벽에 충돌하게 된다면, 충돌 후 방향 전환할 것을 예상해서 동기화 지점을 구합니다.
		if (remainBumpTime >= 0f && remainBumpTime < actorCurrent.remainOrderTime && remainBumpTime < diffSeconds)
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)(remainBumpTime - (diffSeconds - remainBumpTime));
			orderTimer = 0f;
			velocity = -actorCurrent.velocity;
		}
		// 패킷의 딜레이가 방향 전환까지의 남은 시간보다 적으면 해당 딜레이 만큼 패킷으로 받은 속도로 이동하므로 그것을 예상해서 동기화 지점을 구합니다.
		else if (actorCurrent.remainOrderTime >= diffSeconds)
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)diffSeconds;
			orderTimer = actorCurrent.remainOrderTime - (float)diffSeconds;
			velocity = actorCurrent.velocity;
		}
		// 방향 전환이 이뤄진다고 예상되면 방향 전환 후 이동까지 예상해서 동기화 지점을 구합니다.
		else
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)(actorCurrent.remainOrderTime - (diffSeconds - actorCurrent.remainOrderTime));
			orderTimer = 0f;
			velocity = -actorCurrent.velocity;
		}

		transform.position = current;
	}

	/// <summary>
	/// 업데이트를 위한 함수
	/// </summary>
	public void OnUpdate()
	{
		// 서버에서 받은 방향 전환까지 남은 시간을 이용하여 자체적으로 방향을 전환합니다.
		if (orderTimer > 0f)
		{
			orderTimer -= Time.deltaTime;
			if (orderTimer <= 0f)
			{
				Order();
			}
		}
		var beforeX = GetCameraPositionX(transform.position);
		var current = transform.position;
		current.x += velocity * Time.deltaTime;
		var nextX = GetCameraPositionX(current);
		// 벽에 충돌하게 되면 자체적으로 방향을 전환합니다.
		if ((nextX > 1f && nextX > beforeX) || (nextX < 0f && nextX < beforeX))
		{
			Order();
		}
		// 서버에서 받은 속도로 자체적으로 움직입니다.
		else
		{
			transform.position = current;
		}
	}
	#endregion

	#region Private
	/// <summary>
	/// 방향 전환 명령 함수
	/// </summary>
	private void Order()
	{
		orderTimer = 0f;
		velocity = -velocity;
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

	/// <summary>
	/// 인벤토리 칸을 설정하는 업데이트 함수
	/// </summary>
	/// <param name="cell"></param>
	/// <param name="index"></param>
	private void UpdateInventoryCell(GameObject cell, int index)
	{
		if (cell != null)
		{
			var label = cell.transform.Find("IndexText");
			if (label != null && label.TryGetComponent<UILabel>(out var uiLabel))
			{
				uiLabel.text = index.ToString();
			}
		}
	}
	#endregion
}
