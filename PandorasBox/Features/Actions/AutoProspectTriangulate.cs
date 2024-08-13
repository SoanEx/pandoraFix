using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using PandorasBox.FeaturesSetup;
using System.Linq;

namespace PandorasBox.Features.Actions
{
    public unsafe class AutoProspectTriangulate : Feature
    {
        public override string Name => "Auto-Prospect/Triangulate";

        public override string Description => "When switching to MIN or BTN, automatically activate the other jobs searching ability.";

        public override FeatureType FeatureType => FeatureType.Actions;

        public override bool UseAutoConfig => true;

        public class Configs : FeatureConfig
        {
            [FeatureConfigOption("Set delay (seconds)", FloatMin = 0.1f, FloatMax = 10f, EditorSize = 300)]
            public float ThrottleF = 0.1f;

            [FeatureConfigOption("Include Truth of Mountains/Forests", "", 1)]
            public bool AddTruth = false;
        }

        public Configs Config { get; private set; }

        private void ActivateBuff(uint? jobValue)
        {
            if (jobValue is null) return;
            if (jobValue is not (16 or 17)) return; // 16 是採礦工，17 是園藝工
            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.BetweenAreas]) return;
            TaskManager.DelayNext(this.GetType().Name, (int)(Config.ThrottleF * 1000));
            var am = ActionManager.Instance();
            if (Svc.ClientState.LocalPlayer?.StatusList.Where(x => x.StatusId == 217 || x.StatusId == 225).Count() == 2)
                return;
            if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Gathering])
            {
                TaskManager.Abort();
                return;
            }

            // 無論當前職業，啟動「山岳之真實」和「森林之真實」
            if (am->GetActionStatus(ActionType.Action, 210) == 0)
            {
                TaskManager.Enqueue(() => am->UseAction(ActionType.Action, 210)); // 啟動Prospect
            }
            if (am->GetActionStatus(ActionType.Action, 227) == 0)
            {
                TaskManager.Enqueue(() => am->UseAction(ActionType.Action, 227)); // 啟動Triangulate
            }

            if (Config.AddTruth)
            {
                if (am->GetActionStatus(ActionType.Action, 221) == 0)
                {
                    TaskManager.Enqueue(() => am->UseAction(ActionType.Action, 221)); // 啟動「山岳之真實」
                }
                if (am->GetActionStatus(ActionType.Action, 238) == 0)
                {
                    TaskManager.Enqueue(() => am->UseAction(ActionType.Action, 238)); // 啟動「森林之真實」
                }
            }
        }

        public override void Enable()
        {
            Config = LoadConfig<Configs>() ?? new Configs();
            Events.OnJobChanged += ActivateBuff;
            base.Enable();
        }

        public override void Disable()
        {
            SaveConfig(Config);
            Events.OnJobChanged -= ActivateBuff;
            base.Disable();
        }
    }
}
