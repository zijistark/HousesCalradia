using System.Runtime.CompilerServices;

using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;

namespace HousesCalradia.Patches
{
    /// <summary>
    /// Disable periodic, untargeted, unfiltered AI marriages added in e1.5.5, as it's a redundant
    /// and inferior system to the one in Houses of Calradia.
    /// </summary>
    internal sealed class RomanceCampaignBehaviorPatch : Patch
    {
        private static readonly Reflect.Method<RomanceCampaignBehavior> TargetMethod = new("CheckNpcMarriages");
        private static readonly Reflect.Method<RomanceCampaignBehaviorPatch> PatchMethod = new(nameof(CheckNpcMarriagesPrefix));

        internal RomanceCampaignBehaviorPatch()
            : base(Type.Prefix, TargetMethod, PatchMethod, HarmonyLib.Priority.HigherThanNormal) { }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool CheckNpcMarriagesPrefix() => false;
    }
}
