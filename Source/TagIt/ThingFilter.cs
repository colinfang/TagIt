#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace CF_TagIt
{
    public class SpecialThingFilterWorker_Tagged : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => t.TryGetComp<CompFilterTag>() is { IsTagged: true };
        // `.HasComp` doesn't work on inheritance, which is inconsistent with `.GetComp`
        public override bool CanEverMatch(ThingDef def) => def.GetCompProperties<CompProperties_FilterTag>() is not null;
    }

    public class SpecialThingFilterWorker_Untagged : SpecialThingFilterWorker
    {
        public override bool Matches(Thing t) => t.TryGetComp<CompFilterTag>() is not { IsTagged: true };
        // This is only used to decide whether the filter shows up. I hope it is not used for filtering otherwise it could potentially block some things
        // `.HasComp` doesn't work on inheritance, which is inconsistent with `.GetComp`
        public override bool CanEverMatch(ThingDef def) => def.GetCompProperties<CompProperties_FilterTag>() is not null;

    }

    public class CompProperties_FilterTag : CompProperties
    {
        public CompProperties_FilterTag()
        {
            compClass = typeof(CompFilterTag);
        }

    }


    public class CompFilterTag : ThingComp
    {
        private bool isTagged = false;
        public bool IsTagged
        {
            get => isTagged;
            set
            {
                if (isTagged != value)
                {
                    isTagged = value;
                    UpdateOverlayHandle();
                }
            }
        }
        public OverlayHandle? overlayHandle;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref isTagged, nameof(IsTagged), false);
        }
        public override void PostSplitOff(Thing piece)
        {
            // See `CompRottable.PostSplitOff`
            piece.TryGetComp<CompFilterTag>().IsTagged = IsTagged;
        }
        public override void PreAbsorbStack(Thing otherStack, int count)
        {
            // See `CompRottable.PreAbsorbStack`
            if (!IsTagged)
            {
                IsTagged = otherStack.TryGetComp<CompFilterTag>().IsTagged;
            }
            
        }

        public void UpdateOverlayHandle()
        {
            // See `CompForbiddable.UpdateOverlayHandle`
            if (!Patcher.Settings.DisplayTagOverlay) { return; }
            if (parent.Spawned)
            {
                parent.Map.overlayDrawer.Disable(parent, ref overlayHandle);
                if (isTagged)
                {
                    overlayHandle = parent.Map.overlayDrawer.Enable(parent, ModifiedOverlayDrawer.OverlayTypeForTag);
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            // See `CompForbiddable.PostSpawnSetup`
            UpdateOverlayHandle();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Toggle
            {
                defaultLabel = "Tag",
                defaultDesc = "Tag for stockpile",
                isActive = () => IsTagged,
                icon = InitUtility.TagIcon,
                toggleAction = () => IsTagged = !IsTagged
            };
        }
    }



    [HarmonyPatch(typeof(ThingFilter))]
    [HarmonyPatch(nameof(ThingFilter.SetFromPreset))]
    class Patch_ThingFilter_SetFromPreset
    {
        public static SpecialThingFilterDef AllowTaggedDef = DefDatabase<SpecialThingFilterDef>.GetNamed("CF_TagIt_AllowTagged");

        public static void Prefix(ThingFilter __instance)
        {
            // New stockpiles default to not allow tagged
            __instance.SetAllow(AllowTaggedDef, allow: false);
        }
    }

}