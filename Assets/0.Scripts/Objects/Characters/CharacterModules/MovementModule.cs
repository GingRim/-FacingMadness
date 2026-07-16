using System.Buffers;
using UnityEngine;

public class MovementModule : CharacterModule,IRunnable
{
    protected Vector3? targetDestination = null;
    protected Vector3? targetDirection = null;
    protected float targetTolerance;


    public sealed override System.Type RegistrationType =>   typeof(MovementModule);

    public override void OnRegistration(CharacterBase newOwner)
    {
        base.OnRegistration(newOwner);
        GameManager.OnPhysicsCharacter -= MovementUpdate;
        GameManager.OnPhysicsCharacter += MovementUpdate;
    }

    public override void OnUnregistration(CharacterBase oldOwner)
    {
        base.OnUnregistration(oldOwner);
        GameManager.OnPhysicsCharacter -= MovementUpdate;

    }

    public void RegistrationFunctions()
    {
        //매 프레임마다 작동하는 것 Update => 그냥 프레임
        //물리 계산을 할 때에 시간이 들쭉날쭉하면 조금 갔다가 멀리 갔다가 그러면 중간에 벽을 뚤어버릴 수도 있음
        //컴퓨터 연산이 늦어지는 경우 예를 들어 13초동안 컴퓨터가 자동을 안했다!
        //13초를 보정해줘야 하는데 기중이 있어야 한다.
        //0.02초마다 한다는 가정이 있다면 650번을 몰아서 하면 된다.
        //물리를 작동시키는 용도로 사용하는 Update => FixedUpdate
        GameManager.OnPhysicsCharacter -= MovementUpdate;
        GameManager.OnPhysicsCharacter += MovementUpdate;
    }
    public void UnRegistrationFunctions()
    {
        GameManager.OnPhysicsCharacter -= MovementUpdate;


    }

    public void PhysicsUpdate(float deltaTime)
    {

        UpdateToDestination(deltaTime);
        UpdateToDirection(deltaTime);


    }

    public void MovementUpdate(float deltaTime)
    {
        Vector3 originPosition = transform.position;
        PhysicsUpdate(deltaTime);
        Vector3 PositionDelta = transform.position - originPosition;
        Owner.MovementNitify(PositionDelta);
        //AnimationUpdate(PositionDelta);
    }

    public virtual float GetMoveSpeed() => 5.0f;
    public virtual float GetMoneSpeed(float deltaTime) => GetMoveSpeed() * deltaTime;

    public virtual void Translate(Vector3 delta)
    {
        transform.position += delta;
    }

    public void UpdateToDirection(float deltatime)
    {
        if (targetDirection is null) return;

        float currentMoveSpeed = GetMoneSpeed(deltatime * 5.0f);

        Translate(currentMoveSpeed * targetDirection.Value);



    }


    public void UpdateToDestination(float deltaTime)
    {
        if (targetDestination is null) return;

        // 해당 위치로 조금씩 가는 법!
        Vector3 currentMoveDirection = (targetDestination.Value - transform.position);
        //일단 얼마나 더 가야 해요?
        float distance = currentMoveDirection.magnitude;
        //거리가 인정범위 밖
        if (distance > targetTolerance)
        {
            //방향을 잡고
            currentMoveDirection.Normalize();
            //한번 이동할때 거리 정하기
            float currentMoveSpeed = GetMoneSpeed(deltaTime);
            // 거리를 구해야 하는데, 언제 작은 거리를 움지여야 하는가?
            // 주채가 지금 이동하는 거리가 남은 거리보다 클 때
            float resultMoveSpeed = Mathf.Min(currentMoveSpeed, distance);
            // 지금 이 프레임에 난느 몇m를 갈 수 있을까?
            //         2     30km/h = 60km
            // 거리 = 시간 * 속력
            Translate(resultMoveSpeed * currentMoveDirection); //원본

        }
    }

    public void MoveToDestination(Vector3 destination, float tolerance)
    {
        targetDirection = null;
        targetDestination = destination;
        targetTolerance = tolerance;
    }

    public void MoveToDirection(Vector3 direction)
    {
        targetDirection = null; // 목적지 제거한다.
        targetDirection = direction.normalized;
    }

    public void StopMovement()
    {
        targetDestination = null;
        targetDirection = null;
    }

}
