using UnityEngine;

public interface IOpenable 
{
    //ISP => Interface Segrafation Principle
    public bool IsOpen
    {
        get;
    }

    public void Open();

    public void Close();

    public void Toggle();
    void Caddrging(UIManager manager);
    void Caddrging(UIManager manager);
}
