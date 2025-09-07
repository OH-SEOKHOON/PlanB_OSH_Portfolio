using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


//ScrollRect와 RectTransform 컴포넌트가 반드시 필요하다는 것을 명시.
//없으면 자동 추가
[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(RectTransform))]
public class ScrollViewController : MonoBehaviour
{
	//각 아이템(Cell)의 넓이를 설정하는 변수.
    [SerializeField] private float cellWidth;
    
    //셀 간격
    [SerializeField] private int spacing;
    
    //스크롤 기능을 담당하는 ScrollRect 컴포넌트를 저장할 변수.
    private ScrollRect _scrollRect;
    
    //스크롤 뷰의 RectTransform을 저장할 변수.
    private RectTransform _rectTransform;

    //private List<UIminion> _items;                   // Cell에 표시할 Item 리스트
    private List<UnitData> _items;
    
    //앞, 뒤로 셀을 추가하거나 제거할 수 있게 링크드 리스트를 사용했다.
    private LinkedList<UIminionCell> _visibleCells;      // 화면에 표시되고 있는 Cell 리스트
    
	//이전 프레임의 스크롤 위치(X 값)를 저장하는 변수. 초기값은 1f.
    private float _lastXValue = 1f;

    
    //유닛 생성
    [SerializeField] private UnitPanelController _unitPanelController;
    
	
    private void Awake()
    {
    	//시작하자마자 GetComponent<>()를 이용해 ScrollRect 및 RectTransform 컴포넌트를 가져옴.
        _scrollRect = GetComponent<ScrollRect>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
    	//LoadData()를 호출하여 리스트 아이템 데이터를 로드
        LoadData();
    }

    /// <summary>
    /// 현재 보여질 Cell 인덱스를 반환하는 메서드
    /// </summary>
    /// <returns>startIndex: 가장 위에 표시될 Cell 인덱스, endIndex: 가장 아래에 표시될 Cell Index</returns>
    private (int startIndex, int endIndex) GetVisibleIndexRange()
    {
    	//현재 스크롤 뷰의 가시 영역(visibleRect)을 계산.
        //x → 스크롤 뷰에서 수평 스크롤 위치
		//y → 스크롤 뷰에서 수직 스크롤 위치
		//width → 스크롤 뷰의 가로 크기
		//height → 스크롤 뷰의 세로 크기 (즉, 화면에 보이는 영역 높이)
        var visibleRect = new Rect(
            _scrollRect.content.anchoredPosition.x,
            _scrollRect.content.anchoredPosition.y,
            _rectTransform.rect.width,
            _rectTransform.rect.height);

        // 스크롤 위치에 따른 시작 인덱스 계산
        // 현재 스크롤 위치(visibleRect.X)를 cellWeight로 나눠 최상단 Cell의 인덱스를 구함.
        var startIndex = Mathf.FloorToInt(visibleRect.x / cellWidth);

        // 화면에 보이게 될 Cell 개수 계산 (소수점 올림 메서드 사용)
        int visibleCount = Mathf.CeilToInt(visibleRect.width / cellWidth);

        // 버퍼 추가
		// 첫 번째 보이는 셀을 한 칸 위로 미리 준비 (Max를 사용해 0 이전으로 인덱스를 넘지 않도록 보장)
		startIndex = Mathf.Max(0, startIndex - 1);      // startIndex가 0보다 크면 startIndex - 1, 아니면 0
		// 화면 위/아래로 여유 cell을 추가하여, 스크롤할 때 셀을 미리 로드하고 스크롤이 부드럽게 진행되도록 함
		visibleCount += 2;

		//시작 인덱스와 끝인덱스(시작인덱스+보이게될 인덱스 - 시작인덱스는 이미 있으니 -1)반환
        return (startIndex, startIndex + visibleCount - 1);
    }

    /// <summary>
    /// 특정 인덱스가 화면에 보여야 하는지 여부를 판단하는 메서드
    /// </summary>
    /// <param name="index">특정 인덱스</param>
    /// <returns>true, false</returns>
    private bool IsVisibleIndex(int index)
    {
    	// 화면에 표시되는 셀들의 시작과 끝 인덱스를 가져옴
    	var (startIndex, endIndex) = GetVisibleIndexRange();
    
    	// endIndex가 _items의 총 갯수(_items.Count - 1)를 넘어가지 않도록 조정
    	endIndex = Mathf.Min(endIndex, _items.Count - 1);
    
    	// index가 startIndex와 endIndex 사이에 포함되는지 확인
    	return startIndex <= index && index <= endIndex;
    }

    /// <summary>
    /// _items에 있는 값을 Scroll View에 표시하는 함수
    /// _items에 새로운 값이 추가되거나 기존 값이 삭제되면 호출됨
    /// </summary>
    private void ReloadData()
    {
        // _visibleCell 초기화
        _visibleCells = new LinkedList<UIminionCell>();
        
        // Content의 높이를 _items의 데이터의 수만큼 계산해서 너비를 지정
        
        //스크롤 뷰의 콘텐츠 영역의 크기를 가져와서 contentSizeDelta에 저장
        var contentSizeDelta = _scrollRect.content.sizeDelta;
       
        //콘텐츠의 너비는 전체 아이템 개수와 각 셀의 너비를 곱한 값으로 설정
        contentSizeDelta.x = _items.Count * (cellWidth + spacing);
        
        //계산된 contentSizeDelta를 스크롤 뷰의 콘텐츠 영역에 적용,
        _scrollRect.content.sizeDelta = contentSizeDelta;
        
        
        
        // 화면에 보이는 영역에 Cell 추가
        
        //GetVisibleIndexRange 메서드를 호출, 화면에 보이는 셀들의 시작 인덱스(startIndex)와
        //끝 인덱스(endIndex)를 가져옴
        var (startIndex, endIndex) = GetVisibleIndexRange();
        
        // endIndex가 _items의 총 갯수(_items.Count - 1)를 넘어가지 않도록 조정
        var maxEndIndex = Mathf.Min(endIndex, _items.Count - 1);
        
        //startIndex부터 maxEndIndex까지 순회
        for (int i = startIndex; i <= maxEndIndex; i++)
        {
            // 셀 만들기
            //오브젝트 풀(Object Pool)에서 비활성화된 Cell 오브젝트를 가져옴
            var cellObject = ObjectPool.Instance.GetObject();
            
            //가져온 cellObject에서 Cell 컴포넌트를 가져와 지역변수에 할당
            var cell = cellObject.GetComponent<UIminionCell>();
            
            //Cell에 아이템 데이터(_items[i])와 인덱스(i)를 설정
            cell.SetUnit(_items[i], i);
            
            //버튼 클릭메서드에 _unitPanelController의 SetMinionsPrefabs매서드를 동적 할당. (capturedIndex로 따로 뺀다음 할당하는 건 클로저 이슈 때문)
            int capturedIndex = i;
            cell.button.onClick.RemoveAllListeners();
            cell.button.onClick.AddListener(() => _unitPanelController.SetMinionsPrefabs(cell));
            
            //셀의 위치를 설정합니다.
			//i 값에 cellwidth를 곱하여 가로 방향으로 셀의 위치를 계산.
			float startX = cellWidth / 2f; // 첫 번째 셀이 콘텐츠 내부에 완전히 들어오도록 오프셋 추가
			cell.transform.localPosition = new Vector3(startX + i * (cellWidth + spacing), 0, 0);

			//이 셀을 _visibleCells 리스트의 끝에 추가
            _visibleCells.AddLast(cell);
        }
        
        
        if (contentSizeDelta.x <= _rectTransform.rect.width)
        {
	        // 콘텐츠의 너비가 뷰보다 작으면 스크롤 비활성화
	        _scrollRect.horizontal = false;
        }
        else
        {
	        // 콘텐츠의 너비가 뷰보다 크면 스크롤 가능
	        _scrollRect.horizontal = true;
        }
    }

	//아이템 데이터 로드 메서드 (start함수에서 호출되었던거)
    private void LoadData()
    {
        //아이템 리스트를 다음과 같이 초기화
        /*
        _items = new List<UIminion>
	    {
		    new UIminion { imageFileName = "archer", name = "Archer", count = 10},
		    new UIminion { imageFileName = "knight", name = "Knight", count = 10},
		    new UIminion { imageFileName = "pawn", name = "Pawn", count = 10},
		    new UIminion { imageFileName = "archer", name = "Archer", count = 10}
	    };
        */
        _items = UIManager.Instance.selectedUnits;

        if (_items == null || _items.Count == 0)
        {
            Debug.LogWarning("선택된 유닛이 없습니다.");
            return;
        }

        //_items에 있는 값을 Scroll View에 표시하는 함수 호출
        ReloadData();
    }

    #region Scroll Rect Events

	//스크롤의 값이 바뀌때마다 호출 될 메서드
    public void OnValueChanged(Vector2 value)
    {
        if (_lastXValue > value.x)
        {
            ////////////////////////////////////////
            // 왼쪽으로 스크롤

            // 1. 상단에 새로운 셀이 필요한지 확인 후 필요하면 추가
            
            //화면에 표시되는 첫 번째 셀을 가져와 지역변수 할당
            var firstCell = _visibleCells.First.Value;
            
            //첫 번째 셀의 인덱스를 하나 감소시켜 새로운 첫 번째 셀의 인덱스를 계산
            var newFirstIndex = firstCell.Index - 1;

			//IsVisibleIndex(newFirstIndex)를 호출해 이 인덱스가 화면에 보이는 범위 내에 있는지 확인
            if (IsVisibleIndex(newFirstIndex))
            {
            	//참이라면
                //Object Pool에서 셀을 가져와 cell 컴포넌트 할당
                var cell = ObjectPool.Instance.GetObject().GetComponent<UIminionCell>();
                
                //해당 인덱스를 이용하여 셀 정보 설정
                cell.SetUnit(_items[newFirstIndex], newFirstIndex);
                
                //셀 위치 조정
                float startX = cellWidth / 2f;
                cell.transform.localPosition = new Vector3(startX + newFirstIndex * (cellWidth + spacing), 0, 0);
                
                //_visibleCells의 앞에 셀을 추가 (링크드 리스트기에 가능함)
                _visibleCells.AddFirst(cell);
            }

            // 2. 하단에 있는 셀이 화면에서 벗어나면 제거
            //화면에 표시되는 마지막 셀을 가져옴
            var lastCell = _visibleCells.Last.Value;

			//IsVisibleIndex(lastCell.Index): 이 셀이 화면에 보이는 범위 내에 없는지 확인
            if (!IsVisibleIndex(lastCell.Index))
            {
            	//없다면 셀을 오브젝트 풀에 반환
                ObjectPool.Instance.ReturnObject(lastCell.gameObject);
                
                //_visibleCells에서 마지막 셀을 제거 (링크드 리스트기에 가능함)
                _visibleCells.RemoveLast();
            }
        }
        
        else
        {
            ////////////////////////////////////////
            // 오른쪽으로 스크롤

            // 1. 하단에 새로운 셀이 필요한지 확인 후 필요하면 추가
            
            //화면에 표시되는 마지막 셀을 가져옴.
            var lastCell = _visibleCells.Last.Value;
            
            //마지막 셀의 인덱스를 하나 증가시켜 새로운 마지막 셀의 인덱스를 계산
            var newLastIndex = lastCell.Index + 1;
            
            // 이 인덱스가 화면에 보이는 범위 내에 있으면
            if (IsVisibleIndex(newLastIndex))
            {
            	//Object Pool에서 셀을 가져오고,
                var cell = ObjectPool.Instance.GetObject().GetComponent<UIminionCell>();
                
                //해당 인덱스를 이용하여 셀을 설정
                cell.SetUnit(_items[newLastIndex], newLastIndex);
                
                //위치를 조정
                float startX = cellWidth / 2f;
                cell.transform.localPosition = new Vector3(startX + newLastIndex * (cellWidth + spacing), 0, 0);
                
                //_visibleCells의 뒤에 셀을 추가 (링크드 리스트기에 가능함)
                _visibleCells.AddLast(cell);
            }

            // 2. 상단에 있는 셀이 화면에서 벗어나면 제거
            
            //화면에 표시되는 첫 번째 셀을 가져옴.
            var firstCell = _visibleCells.First.Value;

			// 이 셀이 화면에 보이는 범위 내에 없는지 확인
            if (!IsVisibleIndex(firstCell.Index))
            {
            	//없는게 맞다면 오브젝트풀에 해당 셀 반환
                ObjectPool.Instance.ReturnObject(firstCell.gameObject);
                
                //_visibleCells에서 첫 번째 셀을 제거 (링크드 리스트기에 가능함)
                _visibleCells.RemoveFirst();
            }
        }
		
        //_lastYValue를 현재 스크롤 값 value.y로 업데이트
        _lastXValue = value.x;
    }

    #endregion
}