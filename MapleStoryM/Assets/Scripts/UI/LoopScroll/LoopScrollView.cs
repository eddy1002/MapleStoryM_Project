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
    /// 스크롤 초기화
    /// </summary>
    /// <param name="onUpdateScrollCell"></param>
    public void InitScroll(System.Action<GameObject, int> onUpdateScrollCell)
    {
        this.onUpdateScrollCell = onUpdateScrollCell;
        SetCellCount();
        MakeCellList();
    }

    /// <summary>
    /// 스크롤 아이템 리스트를 생성
    /// </summary>
    /// <param name="count"></param>
    public void MakeList(int count)
	{
        cellCount = count;
        SetDummyPanel();
    }

    /// <summary>
    /// 스크롤을 갱신
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
    /// 루프에 필요한 스크롤 아이템 갯수를 구한다
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
    /// 루프에 필요한 스크롤 아이템을 생성
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
    /// 스크롤 범위를 대신하는 더미 패널의 크기를 설정
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
    /// 뷰포트 안에 보여야할 아이템의 인덱스 시작과 끝을 반환
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
    /// 인덱스를 통해서 해당 스크롤 아이템의 위치를 반환
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
    /// 스크롤 아이템을 인덱스에 맞게 설정한다
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
    /// 스크롤 뷰포트를 갱신
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
    /// 스크롤 수치가 변경되면 처리할 업데이트 함수
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
    /// 스크롤 수치가 변했는지 검사
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
