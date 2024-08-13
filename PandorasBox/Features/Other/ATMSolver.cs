using ECommons.Automation;
using ECommons.DalamudServices;
using ECommons.Interop;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PandorasBox.FeaturesSetup;
using System;
using static ECommons.GenericHelpers;

namespace PandorasBox.Features.Other
{
    internal class ATMSolver : Feature
    {
        public override string Name { get; } = "Auto Active Time Maneuver";
        public override string Description { get; } = "Automatically hits a button or mashes for you when the ATM is on screen.";

        public override FeatureType FeatureType => FeatureType.Other;
        private long Throttler { get; set; } = Environment.TickCount64;
        private Random random = new Random();
        public override void Enable()
        {
            Svc.Framework.Update += RunFeature;
            base.Enable();
        }

        bool hasDirectChat = false;

        private unsafe void RunFeature(IFramework framework)
        {
            // 調整 TryGetAddonByName 調用，移除第三個參數
            if ((TryGetAddonByName<AtkUnitBase>("QTE", out var addon) && addon->IsVisible) || TryGetAddonByName<AtkUnitBase>("QTE", out var addon2) && addon2->IsVisible)
            {
                DisableDirectChatIfNeeded();

                if (Environment.TickCount64 >= Throttler)
                {
                    if (ChatLogIsFocused())
                        WindowsKeypress.SendKeypress(ConvertToLimitedKeys(System.Windows.Forms.Keys.Escape));

                    WindowsKeypress.SendKeypress(ConvertToLimitedKeys(System.Windows.Forms.Keys.A));
                    Throttler = Environment.TickCount64 + random.Next(25, 50);
                }
            }
            else
            {
                EnableDirectChatIfNeeded();
            }
        }

        private unsafe bool ChatLogIsFocused()
        {
            var stage = AtkStage.Instance();
            var unitManagers = &stage->RaptureAtkUnitManager->AtkUnitManager.FocusedUnitsList;

            foreach (var i in unitManagers->Entries)
            {
                if (i.Value != null)
                {
                    var addonName = i.Value->NameString;
                    if (addonName == "ChatLog")
                        return true;
                }
            }

            return false;
        }

        private void EnableDirectChatIfNeeded()
        {
            if (hasDirectChat)
            {
                Svc.GameConfig.UiControl.Set("DirectChat", true);
                hasDirectChat = false;
            }
        }

        private void DisableDirectChatIfNeeded()
        {
            if (Svc.GameConfig.UiControl.GetBool("DirectChat"))
            {
                Svc.GameConfig.UiControl.Set("DirectChat", false);
                hasDirectChat = true;
            }
        }

        public override void Disable()
        {
            Svc.Framework.Update -= RunFeature;
            base.Disable();
        }

        private LimitedKeys ConvertToLimitedKeys(System.Windows.Forms.Keys key)
        {
            return key switch
            {
                System.Windows.Forms.Keys.A => LimitedKeys.A,
                System.Windows.Forms.Keys.B => LimitedKeys.B,
                System.Windows.Forms.Keys.Escape => LimitedKeys.Escape,
                _ => throw new ArgumentException("Unsupported key"),
            };
        }
    }
}
