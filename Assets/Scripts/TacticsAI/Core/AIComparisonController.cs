using UnityEngine;
using TacticsAI.FSM;
using TacticsAI.BehaviourTree;

namespace TacticsAI.Core
{
    /// <summary>
    /// AI类型枚举
    /// </summary>
    public enum AIType
    {
        FSM,            // 有限状态机
        BehaviourTree   // 行为树
    }

    /// <summary>
    /// AI比较控制器 - 用于在FSM和行为树之间切换
    /// </summary>
    public class AIComparisonController : MonoBehaviour
    {
        public static AIComparisonController Instance { get; private set; }

        [Header("AI设置")]
        public AIType currentAIType = AIType.FSM;

        [Header("性能统计")]
        public int fsmDecisions = 0;
        public int btDecisions = 0;
        public float fsmTotalTime = 0f;
        public float btTotalTime = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 切换AI类型
        /// </summary>
        public void SwitchAIType(AIType newType)
        {
            if (currentAIType == newType) return;

            currentAIType = newType;
            Debug.Log($"[AI比较] 切换到 {newType} 模式");

            // 更新所有单位的AI控制器
            Unit[] allUnits = FindObjectsOfType<Unit>();
            foreach (var unit in allUnits)
            {
                UpdateUnitAI(unit);
            }
        }

        /// <summary>
        /// 为单位配置AI控制器
        /// </summary>
        public void SetupUnitAI(Unit unit)
        {
            // 移除现有的AI控制器
            RemoveExistingAI(unit);

            // 根据单位类型和AI类型添加控制器
            if (currentAIType == AIType.FSM)
            {
                if (unit.unitType == UnitType.Melee)
                {
                    unit.gameObject.AddComponent<MeleeFSMController>();
                }
                else
                {
                    unit.gameObject.AddComponent<RangedFSMController>();
                }
            }
            else
            {
                if (unit.unitType == UnitType.Melee)
                {
                    unit.gameObject.AddComponent<MeleeBTController>();
                }
                else
                {
                    unit.gameObject.AddComponent<RangedBTController>();
                }
            }

            Debug.Log($"[AI比较] 为 {unit.unitName} 配置 {currentAIType} AI ({unit.unitType})");
        }

        /// <summary>
        /// 更新单位AI控制器
        /// </summary>
        private void UpdateUnitAI(Unit unit)
        {
            SetupUnitAI(unit);
        }

        /// <summary>
        /// 移除现有的AI控制器
        /// </summary>
        private void RemoveExistingAI(Unit unit)
        {
            // 移除FSM控制器
            var fsmController = unit.GetComponent<FSMController>();
            if (fsmController != null)
            {
                Destroy(fsmController);
            }

            // 移除行为树控制器
            var btController = unit.GetComponent<BTController>();
            if (btController != null)
            {
                Destroy(btController);
            }
        }

        /// <summary>
        /// 记录AI决策时间（用于性能比较）
        /// </summary>
        public void RecordDecision(AIType type, float time)
        {
            if (type == AIType.FSM)
            {
                fsmDecisions++;
                fsmTotalTime += time;
            }
            else
            {
                btDecisions++;
                btTotalTime += time;
            }
        }

        /// <summary>
        /// 获取平均决策时间
        /// </summary>
        public float GetAverageDecisionTime(AIType type)
        {
            if (type == AIType.FSM)
            {
                return fsmDecisions > 0 ? fsmTotalTime / fsmDecisions : 0;
            }
            return btDecisions > 0 ? btTotalTime / btDecisions : 0;
        }

        /// <summary>
        /// 重置统计
        /// </summary>
        public void ResetStats()
        {
            fsmDecisions = 0;
            btDecisions = 0;
            fsmTotalTime = 0f;
            btTotalTime = 0f;
        }

        /// <summary>
        /// 打印统计信息
        /// </summary>
        public void PrintStats()
        {
            Debug.Log("=== AI性能统计 ===");
            Debug.Log($"FSM: {fsmDecisions} 次决策, 平均耗时 {GetAverageDecisionTime(AIType.FSM) * 1000:F4} ms");
            Debug.Log($"BT:  {btDecisions} 次决策, 平均耗时 {GetAverageDecisionTime(AIType.BehaviourTree) * 1000:F4} ms");
        }
    }
}
