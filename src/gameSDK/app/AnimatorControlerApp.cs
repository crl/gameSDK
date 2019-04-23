using foundation;
using UnityEngine;
using UnityEngine.AI;

namespace gameSDK
{
    [RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
    public class AnimatorControlerApp : MonoBehaviour
    {
        protected NavMeshAgent _navMeshAgent;
        protected Animator _animator;
        protected AnimatorStateMachineImplement _implement;

        protected virtual void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            InputManager inputManager = InputManager.getInstance();
            inputManager.registKeyDown(KeyCode.A, keyHandle, true);
            inputManager.registKeyDown(KeyCode.S, keyHandle, true);
            inputManager.registKeyDown(KeyCode.W, keyHandle, true);
            inputManager.registKeyDown(KeyCode.D, keyHandle, true);


            inputManager.registKeyDown(KeyCode.J, attackHandle, true);
            inputManager.registKeyDown(KeyCode.K, attackHandle, true);
            inputManager.registKeyDown(KeyCode.L, attackHandle, true);

            _implement = GetComponent<AnimatorStateMachineImplement>();
            _implement.addEventListener(EventX.CHANGE, animatorStateMachineHandle);
            _implement.implement(_animator, gameObject);
        }

        protected virtual void animatorStateMachineHandle(EventX e)
        {
            int data = (int) e.data;
        }

        private void keyHandle(KeyCode keycode)
        {
            Vector2 delta = Vector2.zero;
            switch (keycode)
            {
                case KeyCode.A:
                    delta.x = -1;
                    break;
                case KeyCode.D:
                    delta.x = 1;
                    break;
                case KeyCode.W:
                    delta.y = 1;
                    break;
                case KeyCode.S:
                    delta.y = -1;
                    break;
            }

            onMoveHandle(delta);
        }

        private float lastTime = 0;

        protected virtual void attackHandle(KeyCode keyCode)
        {
            if (Time.realtimeSinceStartup - lastTime < 0.5f)
            {
                return;
            }
            lastTime = Time.realtimeSinceStartup;

            _animator.SetBool("IsWalk", false);
            _animator.SetBool("IsUnsafe", true);
            _animator.SetTrigger("TirggerSkill");

            int v = UnityEngine.Random.Range(1, 9);
            _animator.SetInteger("SkillType", v);
        }

        protected virtual void onMoveHandle(Vector2 delta, float scale = 1.0f)
        {
            Quaternion q = Quaternion.Euler(0, Mathf.Atan2(-delta.y, delta.x) * Mathf.Rad2Deg, 0);
            Vector3 target = this.transform.position + q * BaseApp.MainCamera.transform.right;
            if (_navMeshAgent.SetDestination(target))
            {
                _animator.SetBool("IsWalk", true);
            }
        }

        protected virtual void Update()
        {
            if (_navMeshAgent.enabled)
            {
                if (_navMeshAgent.hasPath)
                {
                    _animator.SetBool("IsWalk", _navMeshAgent.remainingDistance > _navMeshAgent.stoppingDistance);
                }
                else
                {
                    _animator.SetBool("IsWalk", false);
                }
            }
        }
    }
}