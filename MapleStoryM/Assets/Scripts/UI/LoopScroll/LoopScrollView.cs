using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopScrollView : MonoBehaviour
{
    #region PublicMember
    public UIPanel viewPanel = null;
    public Transform dummyPanel = null;
    public GameObject cell = null;
    public Vector2 cellSize = Vector2.zero;
    public System.Action<float> onScroll = null;
    public System.Action<GameObject, int> onUpdateScrollCell = null;
    #endregion

    #region PrivateMember
    private float lastPosition = -1f;
    private int lastShowStartIndex = -1;
    private int lastShowEndIndex = -1;
    private int cellRowCount = 0;
    private int cellColCount = 0;
    private int cellCount = 0;
    #endregion

    #region Public
    /// <summary>
    /// ��ũ�� �ʱ�ȭ
    /// </summary>
    /// <param name="onUpdateScrollCell"></param>
    public void InitScroll(System.Action<GameObject, int> onUpdateScrollCell)
    {
        this.onUpdateScrollCell = onUpdateScrollCell;
        SetCellCount();
        MakeCellList();
    }

    /// <summary>
    /// ��ũ�� ������ ����Ʈ�� ����
    /// </summary>
    /// <param name="count"></param>
    public void MakeList(int count)
	{
        cellCount = count;
        SetDummyPanel();
    }

    /// <summary>
    /// ��ũ���� ����
    /// </summary>
    /// <param name="reset"></param>
    public void RefreshScroll(bool reset = false)
    {
        if (viewPanel != null)
        {
            if (reset)
            {
                viewPanel.transform.localPosition = Vector2.zero;
                viewPanel.clipRange = new Vector4(0f, 0f, viewPanel.clipRange.z, viewPanel.clipRange.w);
            }
            CheckPosition(true);
        }
    }
    #endregion

    #region Private
    /// <summary>
    /// ������ �ʿ��� ��ũ�� ������ ������ ���Ѵ�
    /// </summary>
    private void SetCellCount()
	{
        cellRowCount = cellRowCount = 0;
        if (viewPanel != null)
        {
            if (cellSize.y > 0)
			{
                cellRowCount = Mathf.CeilToInt(viewPanel.clipRange.w / cellSize.y);
            }
            if (cellSize.x > 0)
            {
                cellColCount = Mathf.CeilToInt(viewPanel.clipRange.z / cellSize.x);
            }
        }
    }

    /// <summary>
    /// ������ �ʿ��� ��ũ�� �������� ����
    /// </summary>
    private void MakeCellList()
    {
        if (cell != null)
        {
            var count = (cellRowCount + 2) * cellColCount;
            if (cell.transform.parent.childCount < count)
            {
                cell.transform.localPosition = Vector2.zero;
                for (int i = 1; i < count; i++)
                {
                    if (cell.transform.parent.childCount <= i)
                    {
                        Instantiate(cell, cell.transform.parent);
                    }
                }
            }
        }
    }

    /// <summary>
    /// ��ũ�� ������ ����ϴ� ���� �г��� ũ�⸦ ����
    /// </summary>
    private void SetDummyPanel()
	{
        if (dummyPanel != null && viewPanel != null)
        {
            var lineCount = Mathf.CeilToInt((float)cellCount / cellColCount);
            dummyPanel.transform.localScale = new Vector2(viewPanel.clipRange.z, lineCount * cellSize.y);
        }
	}

    /// <summary>
    /// ����Ʈ �ȿ� �������� �������� �ε��� ���۰� ���� ��ȯ
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private (int, int) GetShowStartEndIndex(float position)
	{
        if (cellSize.y > 0)
        {
            var start = Mathf.FloorToInt(position / cellSize.y) * cellColCount;
            var end = start + cellRowCount * cellColCount - 1;
            return (start - cellColCount, end + cellColCount);
        }
        return (0, 0);
    }

    /// <summary>
    /// �ε����� ���ؼ� �ش� ��ũ�� �������� ��ġ�� ��ȯ
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private Vector2 GetCellPositionByIndex(int index)
    {
        if (cellColCount > 0)
        {
            var col = index % cellColCount;
            if (col < 0)
            {
                col += cellColCount;
            }
            var row = index / cellColCount;
            return new Vector2(col * cellSize.x, -row * cellSize.y);
        }
        return Vector2.zero;
    }

    /// <summary>
    /// ��ũ�� �������� �ε����� �°� �����Ѵ�
    /// </summary>
    /// <param name="child"></param>
    /// <param name="index"></param>
    /// <param name="needShow"></param>
    private void SetCell(Transform child, int index, bool needShow)
    {
        if (child != null)
        {
            if (child.gameObject.activeSelf != needShow)
            {
                child.gameObject.SetActive(needShow);
            }
            if (needShow)
            {
                child.transform.localPosition = GetCellPositionByIndex(index);
            }
            onUpdateScrollCell?.Invoke(child.gameObject, index);
        }
    }

    /// <summary>
    /// ��ũ�� ����Ʈ�� ����
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private void RefreshViewPanel(int start, int end)
    {
        if (cell != null)
        {
            var parent = cell.transform.parent;
            if (parent != null)
            {
                if (end >= cellCount)
                {
                    var index = cellCount - 1;
                    for (int i = parent.childCount - 1; i >= 0; i--)
                    {
                        SetCell(parent.GetChild(i), index, index >= start);
                        index--;
                    }
                }
                else
                {
                    var index = Mathf.Max(start, 0);
                    for (int i = 0; i < parent.childCount; i++)
                    {
                        SetCell(parent.GetChild(i), index, index <= end && index < cellCount);
                        index++;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ��ũ�� ��ġ�� ����Ǹ� ó���� ������Ʈ �Լ�
    /// </summary>
    /// <param name="position"></param>
    private void UpdateScroll(float position)
    {
        var startEnd = GetShowStartEndIndex(position);
        if (lastShowStartIndex != startEnd.Item1 || lastShowEndIndex != startEnd.Item2)
        {
            lastShowStartIndex = startEnd.Item1;
            lastShowEndIndex = startEnd.Item2;
            RefreshViewPanel(lastShowStartIndex, lastShowEndIndex);
        }
    }

    /// <summary>
    /// ��ũ�� ��ġ�� ���ߴ��� �˻�
    /// </summary>
    /// <param name="forced"></param>
    private void CheckPosition(bool forced = false)
    {
        if (viewPanel != null)
        {
            var current = viewPanel.transform.localPosition.y;
            if (forced || current != lastPosition)
            {
                lastPosition = current;
                UpdateScroll(lastPosition);
                onScroll?.Invoke(lastPosition);
            }
        }
    }
    #endregion

	#region Mono
	void Update()
    {
        CheckPosition();
    }
	#endregion
}
