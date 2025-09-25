using UnityEngine;
using UnityEngine.AI;
using Player;
using Managers;

namespace Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum State
        {
            Idle,
            Patrol,
            Chase,
            Attack
        }

        [Header("References")]
        [SerializeField] private NavMeshAgent agent;

        [Header("Detection")]
        [SerializeField] private float sightRange = 12f;
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float fieldOfView = 120f;
        [SerializeField] private float memoryTime = 2f; // how long to remember the player after losing sight

        [Header("Attack")]
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float attackCooldown = 1.0f;

        [Header("Idle")]
        [SerializeField] private float idleDuration = 2.0f;

        [Header("Patrol")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitTime = 1.0f;
        [SerializeField] private float waypointTolerance = 0.4f;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;

        private State _state = State.Idle;
        private int _patrolIndex = 0;
        private float _stateTimer = 0f;
        private float _lastSeenPlayerTime = Mathf.NegativeInfinity;
        private float _lastAttackTime = Mathf.NegativeInfinity;

        private Transform _player;
        private PlayerHealth _playerHealth;

        private Vector3 Eyes => transform.position + Vector3.up * 1.6f;

        private void OnValidate()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
        }

        private void Awake()
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();

            var service = PlayerService.Instance;
            _player = service.PlayerTransform;
            _playerHealth = service.Health;

            agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.8f);
            SwitchState(patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Idle);
        }

        private void Update()
        {
            TickState();
        }

        private void TickState()
        {
            _stateTimer += Time.deltaTime;

            bool canSee = IsPlayerVisible();
            if (canSee) _lastSeenPlayerTime = Time.time;

            switch (_state)
            {
                case State.Idle:
                    agent.isStopped = true;
                    if (canSee)
                    {
                        SwitchState(State.Chase);
                        return;
                    }

                    if (_stateTimer >= idleDuration)
                    {
                        if (patrolPoints != null && patrolPoints.Length > 0)
                            SwitchState(State.Patrol);
                        else
                            _stateTimer = 0f; // stay idle
                    }
                    break;

                case State.Patrol:
                    if (canSee)
                    {
                        SwitchState(State.Chase);
                        return;
                    }

                    PatrolBehaviour();
                    break;

                case State.Chase:
                    ChaseBehaviour();

                    if (IsPlayerInAttackRange())
                    {
                        SwitchState(State.Attack);
                        return;
                    }

                    if (Time.time - _lastSeenPlayerTime > memoryTime)
                    {
                        SwitchState(patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Idle);
                        return;
                    }
                    break;

                case State.Attack:
                    AttackBehaviour();

                    if (!IsPlayerInAttackRange())
                    {
                        SwitchState(State.Chase);
                        return;
                    }

                    if (_player == null || _playerHealth == null)
                    {
                        SwitchState(State.Idle);
                        return;
                    }
                    break;
            }
        }

        private void PatrolBehaviour()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                SwitchState(State.Idle);
                return;
            }

            agent.isStopped = false;

            if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
            {
                agent.isStopped = true;
                if (_stateTimer >= patrolWaitTime)
                {
                    _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[_patrolIndex].position);
                    agent.isStopped = false;
                    _stateTimer = 0f;
                }
            }
            else if (agent.isStopped || agent.remainingDistance == Mathf.Infinity)
            {
                agent.isStopped = false;
                agent.SetDestination(patrolPoints[_patrolIndex].position);
            }
        }

        private void ChaseBehaviour()
        {
            if (_player == null)
            {
                SwitchState(State.Idle);
                return;
            }

            agent.isStopped = false;
            agent.SetDestination(_player.position);
        }

        private void AttackBehaviour()
        {
            if (_player == null || _playerHealth == null)
            {
                SwitchState(State.Idle);
                return;
            }

            // Stop to attack and face the player
            agent.isStopped = true;
            Vector3 toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }

            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                if (IsPlayerInAttackRange())
                {
                    _playerHealth.TakeDamage(attackDamage);
                    _lastAttackTime = Time.time;
                }
            }
        }

        private bool IsPlayerInAttackRange()
        {
            if (_player == null) return false;
            float dist = Vector3.Distance(transform.position, _player.position);
            return dist <= attackRange + 0.05f; // small tolerance
        }

        private bool IsPlayerVisible()
        {
            if (_player == null) return false;

            Vector3 toPlayer = (_player.position - transform.position);
            float sqrDist = toPlayer.sqrMagnitude;
            if (sqrDist > sightRange * sightRange)
                return false;

            // FOV check
            Vector3 dir = toPlayer.normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > fieldOfView * 0.5f)
                return false;

            // Line of sight check
            Vector3 origin = Eyes;
            Vector3 target = _player.position + Vector3.up * 1.0f;
            if (Physics.Linecast(origin, target, out RaycastHit hit))
            {
                if (hit.transform == _player || ( _player != null && hit.transform.IsChildOf(_player)))
                {
                    return true;
                }
                return false;
            }

            return true;
        }

        private void SwitchState(State next)
        {
            if (_state == next)
                return;

            OnExitState(_state);
            _state = next;
            _stateTimer = 0f;
            OnEnterState(_state);
        }

        private void OnEnterState(State s)
        {
            switch (s)
            {
                case State.Idle:
                    agent.isStopped = true;
                    break;
                case State.Patrol:
                    agent.isStopped = false;
                    if (patrolPoints != null && patrolPoints.Length > 0)
                    {
                        agent.SetDestination(patrolPoints[_patrolIndex].position);
                    }
                    break;
                case State.Chase:
                    agent.isStopped = false;
                    break;
                case State.Attack:
                    agent.isStopped = true;
                    break;
            }
        }

        private void OnExitState(State s)
        {
            switch (s)
            {
                case State.Attack:
                    // resume movement after attack if needed
                    break;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos) return;
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, sightRange);
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // FOV arcs (approximate)
            Vector3 left = Quaternion.Euler(0f, -fieldOfView * 0.5f, 0f) * transform.forward;
            Vector3 right = Quaternion.Euler(0f, fieldOfView * 0.5f, 0f) * transform.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, left * sightRange);
            Gizmos.DrawRay(transform.position, right * sightRange);
        }
    }
}
