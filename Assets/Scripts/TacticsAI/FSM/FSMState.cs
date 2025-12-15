using UnityEngine;
using TacticsAI.Core;

namespace TacticsAI.FSM
{
    /// <summary>
    /// FSM状态基类 - 有限状态机的状态抽象
    /// </summary>
    public abstract class FSMState
    {
        protected Unit owner;
        protected FSMController controller;

        public FSMState(Unit owner, FSMController controller)
        {
            this.owner = owner;
            this.controller = controller;
        }

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        public virtual void Enter()
        {
            Debug.Log($"[FSM] {owner.unitName} 进入状态: {GetType().Name}");
        }

        /// <summary>
        /// 退出状态时调用
        /// </summary>
        public virtual void Exit()
        {
            Debug.Log($"[FSM] {owner.unitName} 退出状态: {GetType().Name}");
        }

        /// <summary>
        /// 每帧更新（检查状态转换条件）
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// 执行状态行动
        /// </summary>
        public abstract void Execute();
    }

    /// <summary>
    /// FSM控制器 - 管理状态转换
    /// </summary>
    public class FSMController : MonoBehaviour, IAIController
    {
        protected Unit unit;
        protected FSMState currentState;

        [Header("FSM调试")]
        public string currentStateName;

        protected virtual void Awake()
        {
            unit = GetComponent<Unit>();
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        public void ChangeState(FSMState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }

            currentState = newState;
            currentStateName = newState?.GetType().Name ?? "None";

            if (currentState != null)
            {
                currentState.Enter();
            }
        }

        /// <summary>
        /// 更新状态机
        /// </summary>
        protected virtual void Update()
        {
            currentState?.Update();
        }

        /// <summary>
        /// 执行AI行动（由TacticsGameManager调用）
        /// </summary>
        public virtual void ExecuteAI()
        {
            currentState?.Update();  // 先检查状态转换
            currentState?.Execute(); // 执行当前状态行动
        }

        /// <summary>
        /// 获取当前单位
        /// </summary>
        public Unit GetUnit() => unit;
    }
}
