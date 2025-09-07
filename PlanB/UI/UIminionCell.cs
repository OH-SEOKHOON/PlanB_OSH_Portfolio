using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIminionCell : MonoBehaviour
{
    //이미지 참조변수
    public Button button;
    
    //제목 텍스트 참조변수
    [SerializeField] private TMP_Text titleText;
    
    //숫자 참조 변수
    [SerializeField] private TMP_Text countText;
    private int count;
    
    // 유닛 이미지
    [SerializeField] private Image minionImage;

    //선택된 스프라이트
    [SerializeField] private Image _highlightImage;



    //목록에서 현재 셀의 위치(인덱스)를 저장하는 변수.
    //프로퍼티를 사용해 외부에선 읽기만 할 수 있고 쓰기는 내부클래스에서만 할 수 있게 한다.
    public int Index { get; private set; }

    //아이템 설정 메서드 (여기서 Index의 쓰기가 일어남)
    /*
    public void SetUnit(UIminion minion, int index)
    {
        //리소스에서 스프라이트를 로드하여 이미지를 변경
        minionImage.sprite = Resources.Load<Sprite>("BattleScene/Minions/" + minion.imageFileName);

        //제목을 UI 텍스트에 적용.
        titleText.text = minion.name;
        
        count = minion.count;
        countText.text = count.ToString();

        //현재 셀의 Index 값을 파라미터로 받은 index값으로 저장.
        Index = index;
        
        var color = _highlightImage.color;
        color.a = 0f;
        _highlightImage.color = color;
    }
    */

    public void SetUnit(UnitData unit, int index)
    {
        //리소스에서 스프라이트를 로드하여 이미지를 변경
        minionImage.sprite = unit.icon;
        //제목을 UI 텍스트에 적용.
        titleText.text = unit.name;

        index = unit.index;
        count = 1;



        //count = minion.count;
        //countText.text = count.ToString();

        //현재 셀의 Index 값을 파라미터로 받은 index값으로 저장.
    }

    // 셀 강조 처리 (선택/해제)
    public void SetHighlighted(bool isSelected)
    {
        var color = _highlightImage.color;
        color.a = isSelected ? 1f : 0f;
        _highlightImage.color = color;
    }

    
    public bool IsSpawned()
    {
        if (count == 0)
        {
            return false;
        }

        count--;
        countText.text = count.ToString();
        return true;
    }
    
}
