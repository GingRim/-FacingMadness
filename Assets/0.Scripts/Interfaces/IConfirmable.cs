using System;
using UnityEngine;

public interface IConfirmable
{
    public void Confirm();

    public void SetConfirmAction(Action newAction);

}
