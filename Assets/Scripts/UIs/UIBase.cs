
using Unity.VisualScripting;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public GameObject SetChild(GameObject newchild)
    {
        if(!newchild) return null;

        newchild.transform.SetParent(transform);

        return OnSetChild(newchild);
    }

    protected virtual GameObject OnSetChild(GameObject newchild)
    {
      return newchild;
    }

    public void UnsetChild(GameObject oldVhild)
    {
        if (!oldVhild) return;
        {
            if (oldVhild.transform.parent == transform)
            {
                oldVhild.transform.SetParent(null); 
            }
            OnUnsetChild(oldVhild);
        }
    }

    public virtual void OnUnsetChild(GameObject oldChild)
    {

    }
}
