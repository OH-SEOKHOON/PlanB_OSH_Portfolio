using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectObserver<T>
{
    public void OnMinionSelecte(Minion minion);
}
