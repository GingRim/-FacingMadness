using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public delegate void MovementEvent(Vector3 move);
public delegate void LookAtEvent(Vector3 direction);
//                               실제 데미지를 제공한 사물    데미지를 주라고 시킨 놈
public delegate void DamageEvent(GameObject damageCauser, ControllerBase instigator,float damage);

public class CharacterBase : MonoBehaviour
{
    public void MovementNitify(Vector3 move) => OnMovement?.Invoke(move);
    public event MovementEvent OnMovement;
    public void LookAtNitify(Vector3 direction) => OnLookAt?.Invoke(direction);
    public event LookAtEvent OnLookAt;
    public void DamageNitify(GameObject damageCauser, ControllerBase instigator, float damage) => OnDamage?.Invoke(damageCauser, instigator, damage);
    public event DamageEvent OnDamage;




    //가장 중요한 기능!
    //말을 했을 때 말을 잘 들어먹는 것
    ControllerBase _controller;
    public ControllerBase Controller => _controller;

    protected Vector3 _lookRotation;
    public Vector3 LookRotation => _lookRotation;

    public virtual string DisplayName => "Character";

    // 모튤을 저장해놓기!
    // List :  추가/제거가 쉽다. <-> 메모리 효율이 낮고, 전체 순환이 느리다.

    Dictionary<System.Type, CharacterModule> moduleDictipnary = new();
    
    public void AddModule(System.Type wantType, CharacterModule wantModule)
    {
        if(moduleDictipnary.TryAdd(wantType, wantModule))
        {
            wantModule.OnRegistration(this);
        }
    }

    public void AddAllModuleFromObject(GameObject target)
    {
        if(!target) return;

        foreach(CharacterModule currentModule in target.GetComponentsInChildren<CharacterModule>())
        {
            AddModule(currentModule.RegistrationType, currentModule);
        }
    }

    public void RemoveModule(System.Type wantType)
    {
        if (moduleDictipnary.ContainsKey(wantType))
        {
        
            moduleDictipnary[wantType].OnRegistration(this);// 넌 해제 된거야
            moduleDictipnary.Remove(wantType);// 그 다음에 제거
        
        }    
    }

    public void RemoveAllModule()
    {
        foreach (CharacterModule currentModule in moduleDictipnary.Values)
        {
            currentModule.OnUnregistration(this);
        }
    }

    public T GetModule<T>() where T : CharacterModule
    {
        moduleDictipnary.TryGetValue(typeof(T), out CharacterModule result);
        return result as T;
    }

    public ControllerBase Possessed(ControllerBase from) //Possessed = 빙의되다.
    {
        //영혼이 있다면 다른 영혼이 있을 수 있는가?
        if (Controller) UnPossessed();
        _controller = from;
        AddAllModuleFromObject(gameObject);
        OnPossessed(Controller);
        return Controller;
    }

   

    public virtual void OnPossessed(ControllerBase newController){ }

    public void UnPossessed()
    {
        if(Controller)OnUnPossessed(Controller);
        _controller = null;
    }

    public virtual void OnUnPossessed(ControllerBase oldController) { }
    public bool UnPossessed(ControllerBase oldController)
    {
        if (Controller != oldController) return false;
        UnPossessed();
        return true;
    }


}