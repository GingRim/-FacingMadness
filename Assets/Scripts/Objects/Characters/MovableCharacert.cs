using UnityEngine;


//pawn : 조종할수 있지만 이동이 불가능한 캐릭터
//Character : 조종하고 이동 기능이 있는 캐릭터
public class MovableCharacert : CharacterBase, IRunnable, IFunctionable
{
    protected Vector3 targetDestination;
    protected float targetTolerance;

    private void Start()
    {
        RegistrationFunctions();
    }

    public void RegistrationFunctions()
    {
        //매 프레임마다 작동하는 것 Update => 그냥 프레임
        //물리 계산을 할 때에 시간이 들쭉날쭉하면 조금 갔다가 멀리 갔다가 그러면 중간에 벽을 뚤어버릴 수도 있음
        //컴퓨터 연산이 늦어지는 경우 예를 들어 13초동안 컴퓨터가 자동을 안했다!
        //13초를 보정해줘야 하는데 기중이 있어야 한다.
        //0.02초마다 한다는 가정이 있다면 650번을 몰아서 하면 된다.
        //물리를 작동시키는 용도로 사용하는 Update => FixedUpdate
        GameManager.OnPhysicsCharacter -= PhysicsUpdate;
        GameManager.OnPhysicsCharacter += PhysicsUpdate;
    }
    public void UnRegistrationFunctions()
    {
        GameManager.OnPhysicsCharacter -= PhysicsUpdate;
        
    }

    public void PhysicsUpdate(float deltaTime)
    {
        // 해당 위치로 조금씩 가는 법!
        Vector3 currentMoveDirection = (targetDestination - transform.position);
        //일단 얼마나 더 가야 해요?
        float distance = currentMoveDirection.magnitude;
        //거리가 인정범위 밖
        if (distance > targetTolerance)
        {
            //방향을 잡고
            currentMoveDirection.Normalize();
            // 지금 이 프레임에 난느 몇m를 갈 수 있을까?
            //         2     30km/h = 60km
            // 거리 = 시간 * 속력
            transform.position += deltaTime * 5.0f *  currentMoveDirection;
        }
    }

    public void MoveToDestination(Vector3 destination, float tolerance)
    {
        targetDestination = destination;
        targetTolerance = tolerance;
    }

    public void MoveToDirection(Vector3 direction)
    {
        
    }

    public void StopMovement()
    {
        
    }

}
