using UnityEngine;

public interface IStatus<T>
{
    public T SetCurrentStatus(T newStatus);
}

//public interface IProgress<T>
//{
 //   public T Current { get; }

   // public T Max { get; }

//    public float Progress { get; }


//    public T Set(T newCurrent);
 //   public T Set(T newCurrent, T newMax);

//    public T AddCurrent(T value);
//    public T AddMax(T value);
