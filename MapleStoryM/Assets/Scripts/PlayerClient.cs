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
	/// ����ȭ ����� ������� �ʰ� ���������� �÷��̾� ��ġ ����
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
	/// ����ȭ ����� �����Ͽ� �÷��̾� ��ġ ����
	/// </summary>
	/// <param name="packet"></param>
	/// <param name="actorCurrent"></param>
	public void SyncPosition(FakePacket packet, Packet_ActorCurrent actorCurrent)
	{
		var diffTicks = DateTime.Now.Ticks - packet.GetBornTick();
		var diffSeconds = new TimeSpan(diffTicks).TotalMilliseconds * 0.001f;
		var endPosition = Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;

		// ���� �浹���� ���� �ð��� ���մϴ�.
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
		// ���� �浹�ϰ� �ȴٸ�, �浹 �� ���� ��ȯ�� ���� �����ؼ� ����ȭ ������ ���մϴ�.
		if (remainBumpTime >= 0f && remainBumpTime < actorCurrent.remainOrderTime && remainBumpTime < diffSeconds)
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)(remainBumpTime - (diffSeconds - remainBumpTime));
			orderTimer = 0f;
			velocity = -actorCurrent.velocity;
		}
		// ��Ŷ�� �����̰� ���� ��ȯ������ ���� �ð����� ������ �ش� ������ ��ŭ ��Ŷ���� ���� �ӵ��� �̵��ϹǷ� �װ��� �����ؼ� ����ȭ ������ ���մϴ�.
		else if (actorCurrent.remainOrderTime >= diffSeconds)
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)diffSeconds;
			orderTimer = actorCurrent.remainOrderTime - (float)diffSeconds;
			velocity = actorCurrent.velocity;
		}
		// ���� ��ȯ�� �̷����ٰ� ����Ǹ� ���� ��ȯ �� �̵����� �����ؼ� ����ȭ ������ ���մϴ�.
		else
		{
			current.x = actorCurrent.currentX + actorCurrent.velocity * (float)(actorCurrent.remainOrderTime - (diffSeconds - actorCurrent.remainOrderTime));
			orderTimer = 0f;
			velocity = -actorCurrent.velocity;
		}

		transform.position = current;
	}

	/// <summary>
	/// ������Ʈ�� ���� �Լ�
	/// </summary>
	public void OnUpdate()
	{
		// �������� ���� ���� ��ȯ���� ���� �ð��� �̿��Ͽ� ��ü������ ������ ��ȯ�մϴ�.
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
		// ���� �浹�ϰ� �Ǹ� ��ü������ ������ ��ȯ�մϴ�.
		if ((nextX > 1f && nextX > beforeX) || (nextX < 0f && nextX < beforeX))
		{
			Order();
		}
		// �������� ���� �ӵ��� ��ü������ �����Դϴ�.
		else
		{
			transform.position = current;
		}
	}
	#endregion

	#region Private
	/// <summary>
	/// ���� ��ȯ ��� �Լ�
	/// </summary>
	private void Order()
	{
		orderTimer = 0f;
		velocity = -velocity;
	}

	/// <summary>
	/// ī�޶� ����� ���� ��ǥ�� ��ȯ
	/// </summary>
	/// <param name="position"></param>
	/// <returns></returns>
	private float GetCameraPositionX(Vector3 position)
	{
		return Camera.main.WorldToViewportPoint(position).x;
	}

	/// <summary>
	/// �κ��丮 ĭ�� �����ϴ� ������Ʈ �Լ�
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
