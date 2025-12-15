using UnityEngine;

namespace TacticsAI.Core
{
    /// <summary>
    /// 战旗游戏调试UI
    /// 提供运行时切换AI类型、控制游戏等功能
    /// </summary>
    public class TacticsDebugUI : MonoBehaviour
    {
        private bool showUI = true;
        private Rect windowRect = new Rect(10, 10, 280, 400);

        private void OnGUI()
        {
            if (!showUI) return;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "战旗AI调试面板");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.Space(5);

            // 游戏状态
            GUILayout.Label("=== 游戏状态 ===");
            if (TacticsGameManager.Instance != null)
            {
                GUILayout.Label($"当前阶段: {TacticsGameManager.Instance.currentPhase}");
                GUILayout.Label($"回合数: {TacticsGameManager.Instance.turnNumber}");
                GUILayout.Label($"玩家单位: {TacticsGameManager.Instance.playerUnits.FindAll(u => !u.isDead).Count}");
                GUILayout.Label($"敌方单位: {TacticsGameManager.Instance.enemyUnits.FindAll(u => !u.isDead).Count}");
            }
            else
            {
                GUILayout.Label("游戏管理器未初始化");
            }

            GUILayout.Space(10);

            // AI类型切换
            GUILayout.Label("=== AI设置 ===");
            if (AIComparisonController.Instance != null)
            {
                GUILayout.Label($"当前AI类型: {AIComparisonController.Instance.currentAIType}");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("切换到 FSM"))
                {
                    AIComparisonController.Instance.SwitchAIType(AIType.FSM);
                }
                if (GUILayout.Button("切换到 行为树"))
                {
                    AIComparisonController.Instance.SwitchAIType(AIType.BehaviourTree);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            // 游戏控制
            GUILayout.Label("=== 游戏控制 ===");
            if (TacticsGameManager.Instance != null)
            {
                // 自动战斗开关
                bool newAutoBattle = GUILayout.Toggle(
                    TacticsGameManager.Instance.useAutoBattle,
                    "自动战斗（AI vs AI）"
                );
                if (newAutoBattle != TacticsGameManager.Instance.useAutoBattle)
                {
                    TacticsGameManager.Instance.useAutoBattle = newAutoBattle;
                }

                // 结束回合按钮
                if (TacticsGameManager.Instance.currentPhase == GamePhase.PlayerTurn)
                {
                    if (GUILayout.Button("结束玩家回合"))
                    {
                        TacticsGameManager.Instance.EndPlayerTurn();
                    }
                }

                // AI行动速度
                GUILayout.Label($"AI行动间隔: {TacticsGameManager.Instance.aiActionDelay:F1}秒");
                TacticsGameManager.Instance.aiActionDelay = GUILayout.HorizontalSlider(
                    TacticsGameManager.Instance.aiActionDelay, 0.1f, 2f
                );
            }

            GUILayout.Space(10);

            // 性能统计
            GUILayout.Label("=== 性能统计 ===");
            if (AIComparisonController.Instance != null)
            {
                GUILayout.Label($"FSM决策: {AIComparisonController.Instance.fsmDecisions} 次");
                GUILayout.Label($"BT决策: {AIComparisonController.Instance.btDecisions} 次");

                if (GUILayout.Button("打印详细统计"))
                {
                    AIComparisonController.Instance.PrintStats();
                }

                if (GUILayout.Button("重置统计"))
                {
                    AIComparisonController.Instance.ResetStats();
                }
            }

            GUILayout.Space(10);

            // 单位状态
            GUILayout.Label("=== 单位状态 ===");
            Unit[] units = FindObjectsOfType<Unit>();
            foreach (var unit in units)
            {
                if (unit.isDead) continue;
                string teamColor = unit.team == Team.Player ? "<color=blue>" : "<color=red>";
                GUILayout.Label($"{unit.unitName}: HP {unit.currentHealth}/{unit.maxHealth} @ ({unit.GridPosition.x},{unit.GridPosition.y})");
            }

            GUI.DragWindow();
        }

        private void Update()
        {
            // 按F1键显示/隐藏UI
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showUI = !showUI;
            }

            // 快捷键
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 空格结束回合
                if (TacticsGameManager.Instance?.currentPhase == GamePhase.PlayerTurn)
                {
                    TacticsGameManager.Instance.EndPlayerTurn();
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AIComparisonController.Instance?.SwitchAIType(AIType.FSM);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AIComparisonController.Instance?.SwitchAIType(AIType.BehaviourTree);
            }
        }
    }
}
