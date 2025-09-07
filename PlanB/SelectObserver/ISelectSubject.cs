using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectSubject<T>
{
    //옵저버를 등록하는 메서드
    public void OnSelected(Minion minion);
}
