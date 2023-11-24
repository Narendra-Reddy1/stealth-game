using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{

    #region Variables

    public List<Transform> patrollingPoints;
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    public float patrolCoolDownTimer = 1.25f;
    public float attackRadius = 1.5f;
    public Collider swordCollider;
    //time to chase target after target goes out of FOV.
    public float timeToChaseAfterTargetLost = 3f;

    [SerializeField] private Animator _animator;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField]
    private EnemyState _currentEnemyState;

    [Space(5f)]
    [Header("FOV")]
    public float visionRange = 10f;
    [Range(1, 360)]
    public float visionAngle = 45f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;


    [SerializeField] private List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution = 1;
    public int edgeResolveIterations = 4;
    public float edgeDstThreshold = .5f;

    public Color32 _patrollingFovColor;
    public Color32 _detectionFovColor;
    public Material _fovMaterial;
    public MeshFilter viewMeshFilter;
    public MeshRenderer viewMeshRenderer;
    private Mesh _viewMesh;

    private Transform _targetToChase;
    private float _targetLostChaseTimerCounter = 0;
    #endregion Variables

    #region Unity Methods
    private void OnEnable()
    {
        GlobalEventHandler.OnLevelCompleted += Callback_On_Level_Complete;
    }
    private void OnDisable()
    {
        GlobalEventHandler.OnLevelCompleted -= Callback_On_Level_Complete;
    }
    void Start()
    {
        _targetLostChaseTimerCounter = timeToChaseAfterTargetLost;
        _viewMesh = new Mesh();
        _viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = _viewMesh;

        InvokeRepeating(nameof(_CanSeePlayer), 0f, .2f);
        // InvokeRepeating(nameof(_CanSmellPlayer), 0f, 1f);
    }
    private void Update()
    {
        if (GlobalVariables.CurrentGameState != GameState.Running) return;
        switch (_currentEnemyState)
        {
            case EnemyState.Idle:
                _Idle();
                break;
            case EnemyState.Patrol:
                _Patrol();
                break;
            case EnemyState.Chase:
                _Chase();
                break;
            case EnemyState.Attack:
                _Attack();
                break;
        }
    }
    void LateUpdate()
    {
        _DrawFieldOfView();
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position, attackRadius);
    }
    #endregion Unity Methods

    #region Public Methods
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    #endregion Public Methods

    #region Private Methods

    private int _patrolPointIndex = 0;
    float _patrolCooldownCounter = 0;
    private void _Patrol()
    {
        if (patrollingPoints.Count <= 0) return;
        float distanceToPatrolPoint = Vector3.Distance(transform.position, patrollingPoints[_patrolPointIndex].position);
        if (distanceToPatrolPoint > 0.2f)
        {
            _animator.SetFloat("Speed", patrolSpeed);
            ResetAttackState();
            //_animator.SetFloat("MotionSpeed", patrolSpeed / 3);
            _navMeshAgent.SetDestination(patrollingPoints[_patrolPointIndex].position);
        }
        else if (distanceToPatrolPoint <= 0.2f)
        {
            _currentEnemyState = EnemyState.Idle;
        }
    }
    private void _Idle()
    {
        _animator.SetFloat("Speed", Mathf.Lerp(_animator.GetFloat("Speed"), 0f, 2 * Time.deltaTime));
        //_animator.SetFloat("MotionSpeed", patrolSpeed / 3);
        if (_patrolCooldownCounter >= 0)
        {
            _patrolCooldownCounter -= Time.deltaTime;
            if (_patrolCooldownCounter < 0)
            {
                _patrolCooldownCounter = Random.Range(1.25f, patrolCoolDownTimer);
                _patrolPointIndex++;
                if (_patrolPointIndex >= patrollingPoints.Count)
                    _patrolPointIndex = 0;
                _navMeshAgent.isStopped = false;
                _currentEnemyState = EnemyState.Patrol;
            }
        }
    }
    private void _Chase()
    {
        if (_targetToChase == null)
        {
            _currentEnemyState = EnemyState.Patrol;
            return;
        }
        _navMeshAgent.speed = chaseSpeed;
        _animator.SetFloat("Speed", chaseSpeed);
        _navMeshAgent.SetDestination(_targetToChase.position);
        if (visibleTargets.Count <= 0)
        {
            _targetLostChaseTimerCounter -= Time.deltaTime;
            if (_targetLostChaseTimerCounter < 0)
            {
                _targetLostChaseTimerCounter = timeToChaseAfterTargetLost;
                _navMeshAgent.isStopped = true;
                _currentEnemyState = EnemyState.Idle;
            }
        }
        else
            _targetLostChaseTimerCounter = timeToChaseAfterTargetLost;

        if (Vector3.Distance(_targetToChase.position, transform.position) <= attackRadius)
            _currentEnemyState = EnemyState.Attack;
    }

    private void _Attack()
    {
        Collider[] targetsInAttackRadius = Physics.OverlapSphere(transform.position, attackRadius, targetMask);
        Debug.Log($"Attack State");
        if (targetsInAttackRadius.Length > 0)
        {
            //GameOver
            swordCollider.enabled = true;
            transform.LookAt(_targetToChase, Vector3.up);
            Debug.Log($"Attacking....");
            _animator.SetBool("CanAttack", true);
        }
        else
        {
            Debug.Log($"No Targets in Attack range");
            ResetAttackState();
            _currentEnemyState = EnemyState.Chase;
        }
    }
    private void ResetAttackState()
    {
        _animator.SetBool("CanAttack", false);
        swordCollider.enabled = false;
    }
    private bool _CanSeePlayer()
    {
        visibleTargets.Clear();
        bool isTargetVisible = false;
        viewMeshRenderer.material.color = _patrollingFovColor;
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, visionRange, targetMask);
        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < visionAngle / 2)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                Debug.DrawLine(transform.position, dirToTarget * dstToTarget, Color.red);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    if (!visibleTargets.Contains(target))
                        visibleTargets.Add(target);
                    viewMeshRenderer.material.color = _detectionFovColor;
                    _targetToChase = target;
                    isTargetVisible = true;
                    //Change State to Chase
                    _currentEnemyState = EnemyState.Chase;
                }
            }
        }
        return isTargetVisible;
    }

    private bool _CanSmellPlayer()
    {
        bool isPlayerBeside = false;
        if (Physics.SphereCast(transform.position, attackRadius, transform.forward, out var hit, targetMask))
        {
            _targetToChase = hit.transform;
            _currentEnemyState = EnemyState.Attack;
            isPlayerBeside = true;
        }
        return isPlayerBeside;
    }
    private void _DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(visionAngle * meshResolution);
        float stepAngleSize = visionAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - visionAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = _ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = _FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        _viewMesh.Clear();

        _viewMesh.vertices = vertices;
        _viewMesh.triangles = triangles;
        _viewMesh.RecalculateNormals();
    }

    private EdgeInfo _FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = _ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    private ViewCastInfo _ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, visionRange, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * visionRange, visionRange, globalAngle);
        }
    }

    #endregion Private Methods

    #region Callbacks
    private void Callback_On_Level_Complete()
    {
        _currentEnemyState = EnemyState.Idle;
        _animator.SetFloat("Speed", 0);
    }
    #endregion Callbacks

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}
public enum EnemyState
{
    Idle = 0,
    Patrol = 1,
    Chase = 2,
    Attack = 3,
}
