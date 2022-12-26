using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { None = -1, Idle = 0, Wander, }

public class EnemyFSM : MonoBehaviour
{
    private EnemyState enemyState = EnemyState.None;          //현재 적 행동
    private Status status;                                    //이동 속도 등의 정보
    private NavMeshAgent navMeshAgent;                        //이동 제어를 위한 NavMeshAgent

    private void Awake()
    {
        status = GetComponent<Status>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        //NavMeshAgent 컴포넌트에서 회전을 업데이트하지 않도록 설정
        navMeshAgent.updateRotation = false;
    }

    private void OnEnable()
    {
        //적이 활성화될 때 적의 상태를 "대기"로 설정
        ChangeState(EnemyState.Idle);
    }

    private void OnDisable()
    {
        //적이 비활성화될 때 현재 재생중인 상태를 종료하고, 상태를 "None"으로 설정
        StopCoroutine(enemyState.ToString());

        enemyState = EnemyState.None;
    }

    public void ChangeState(EnemyState newState)
    {
        //현재 재생중인 상태와 비꾸려고 하는 상태가 같으면 바꿀 필요가 없기 때문에 return
        if (enemyState == newState) return;

        //이전에 재생중이던 상태 종료
        StopCoroutine(enemyState.ToString());
        //현재 적의 상태를 newState로 설정
        enemyState = newState;
        //새로운 상태 재생
        StartCoroutine(enemyState.ToString());
    }

    private IEnumerator Idle()
    {
        //n초 후에 "배회" 상태로 변경하는 코루틴 실행
        StartCoroutine("AutoChangeFromIdleToWander");

        while (true)
        {
            //"대기" 상태일 때 하는 행동

            yield return null;
        }
    }

    private IEnumerator AutoChangeFromIdleToWander()
    {
        //1~4초 시간 대기
        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        //상태를 "배회"로 변경
        ChangeState(EnemyState.Wander);
    }

    private IEnumerator Wander()
    {
        float currentTime = 0;
        float maxTime = 10;

        //이동 속도 설정
        navMeshAgent.speed = status.WalkSpeed;

        //목표 위치 설정
        navMeshAgent.SetDestination(CalculateWanderPosition());

        //목표 위치로 회전
        Vector3 to = new Vector3(navMeshAgent.destination.x, 30, navMeshAgent.destination.z);
        Vector3 from = new Vector3(transform.position.x, 30, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);

        while (true)
        {
            currentTime += Time.deltaTime;

            //목표위치에 근접하게 도달하거나 너무 오랜시간동안 배회하기 상태에 머물러 있으면
            to = new Vector3(navMeshAgent.destination.x, 30, navMeshAgent.destination.z);
            from = new Vector3(transform.position.x, 30, transform.position.z);
            if((to - from).sqrMagnitude < 0.01f || currentTime >= maxTime)
            {
                //상태를 "대기"로 변경
                ChangeState(EnemyState.Idle);
            }

            yield return null;
        }
    }

    private Vector3 CalculateWanderPosition()
    {
        float wanderRadius = 10;                  //현재 위치를 원점으로 하는 원의 반지름
        int wanderJitter = 0;                     //선택된 각도 (wanderJitterMin ~ wanderJitterMax)
        int wanderJitterMin = 0;                  //최소 각도
        int wanderJitterMax = 360;                //최대 각도

        //현재 적 캐릭터가 있는 월드의 중심 위치와 크기 (구역을 벗어난 행동을 하지 않도록)
        Vector3 rangePosition = Vector3.zero;
        Vector3 rangeScale = Vector3.one * 1500.0f;

        //자신의 위치를 중심으로 반지름(wanderRadius) 거리, 선택된 각도(wanderJitter)에 위치한 좌표를 목표지점으로 설정
        wanderJitter = Random.Range(wanderJitterMin, wanderJitterMax);
        Vector3 targetPosition = transform.position + SetAngle(wanderRadius, wanderJitter);

        //생성된 목표 위치가 자신의 이동구역을 벗어나지 않게 조절
        targetPosition.x = Mathf.Clamp(targetPosition.x, rangePosition.x - rangeScale.x * 0.5f, rangePosition.x + rangeScale.x * 0.5f);
        targetPosition.y = 30.0f;
        targetPosition.z = Mathf.Clamp(targetPosition.z, rangePosition.z - rangeScale.z * 0.5f, rangePosition.z + rangeScale.z * 0.5f);

        return targetPosition;
    }

    private Vector3 SetAngle(float radius, int angle)
    {
        Vector3 position = Vector3.zero;

        position.x = Mathf.Cos(angle) * radius;
        position.z = Mathf.Sin(angle) * radius;

        return position;
    }

    private void OnDrawGizmos()
    {
        //"배회" 상태일 때 이동한 경로 표시
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
    }
}
