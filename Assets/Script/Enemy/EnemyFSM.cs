using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { None = -1, Idle = 0, Wander, }

public class EnemyFSM : MonoBehaviour
{
    private EnemyState enemyState = EnemyState.None;          //���� �� �ൿ
    private Status status;                                    //�̵� �ӵ� ���� ����
    private NavMeshAgent navMeshAgent;                        //�̵� ��� ���� NavMeshAgent

    private void Awake()
    {
        status = GetComponent<Status>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        //NavMeshAgent ������Ʈ���� ȸ���� ������Ʈ���� �ʵ��� ����
        navMeshAgent.updateRotation = false;
    }

    private void OnEnable()
    {
        //���� Ȱ��ȭ�� �� ���� ���¸� "���"�� ����
        ChangeState(EnemyState.Idle);
    }

    private void OnDisable()
    {
        //���� ��Ȱ��ȭ�� �� ���� ������� ���¸� �����ϰ�, ���¸� "None"���� ����
        StopCoroutine(enemyState.ToString());

        enemyState = EnemyState.None;
    }

    public void ChangeState(EnemyState newState)
    {
        //���� ������� ���¿� ��ٷ��� �ϴ� ���°� ������ �ٲ� �ʿ䰡 ���� ������ return
        if (enemyState == newState) return;

        //������ ������̴� ���� ����
        StopCoroutine(enemyState.ToString());
        //���� ���� ���¸� newState�� ����
        enemyState = newState;
        //���ο� ���� ���
        StartCoroutine(enemyState.ToString());
    }

    private IEnumerator Idle()
    {
        //n�� �Ŀ� "��ȸ" ���·� �����ϴ� �ڷ�ƾ ����
        StartCoroutine("AutoChangeFromIdleToWander");

        while (true)
        {
            //"���" ������ �� �ϴ� �ൿ

            yield return null;
        }
    }

    private IEnumerator AutoChangeFromIdleToWander()
    {
        //1~4�� �ð� ���
        int changeTime = Random.Range(1, 5);

        yield return new WaitForSeconds(changeTime);

        //���¸� "��ȸ"�� ����
        ChangeState(EnemyState.Wander);
    }

    private IEnumerator Wander()
    {
        float currentTime = 0;
        float maxTime = 10;

        //�̵� �ӵ� ����
        navMeshAgent.speed = status.WalkSpeed;

        //��ǥ ��ġ ����
        navMeshAgent.SetDestination(CalculateWanderPosition());

        //��ǥ ��ġ�� ȸ��
        Vector3 to = new Vector3(navMeshAgent.destination.x, 30, navMeshAgent.destination.z);
        Vector3 from = new Vector3(transform.position.x, 30, transform.position.z);
        transform.rotation = Quaternion.LookRotation(to - from);

        while (true)
        {
            currentTime += Time.deltaTime;

            //��ǥ��ġ�� �����ϰ� �����ϰų� �ʹ� �����ð����� ��ȸ�ϱ� ���¿� �ӹ��� ������
            to = new Vector3(navMeshAgent.destination.x, 30, navMeshAgent.destination.z);
            from = new Vector3(transform.position.x, 30, transform.position.z);
            if((to - from).sqrMagnitude < 0.01f || currentTime >= maxTime)
            {
                //���¸� "���"�� ����
                ChangeState(EnemyState.Idle);
            }

            yield return null;
        }
    }

    private Vector3 CalculateWanderPosition()
    {
        float wanderRadius = 10;                  //���� ��ġ�� �������� �ϴ� ���� ������
        int wanderJitter = 0;                     //���õ� ���� (wanderJitterMin ~ wanderJitterMax)
        int wanderJitterMin = 0;                  //�ּ� ����
        int wanderJitterMax = 360;                //�ִ� ����

        //���� �� ĳ���Ͱ� �ִ� ������ �߽� ��ġ�� ũ�� (������ ��� �ൿ�� ���� �ʵ���)
        Vector3 rangePosition = Vector3.zero;
        Vector3 rangeScale = Vector3.one * 1500.0f;

        //�ڽ��� ��ġ�� �߽����� ������(wanderRadius) �Ÿ�, ���õ� ����(wanderJitter)�� ��ġ�� ��ǥ�� ��ǥ�������� ����
        wanderJitter = Random.Range(wanderJitterMin, wanderJitterMax);
        Vector3 targetPosition = transform.position + SetAngle(wanderRadius, wanderJitter);

        //������ ��ǥ ��ġ�� �ڽ��� �̵������� ����� �ʰ� ����
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
        //"��ȸ" ������ �� �̵��� ��� ǥ��
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, navMeshAgent.destination - transform.position);
    }
}
