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
    [HarmonyPatch(typeof(OverlayDrawer))]
    [HarmonyPatch(nameof(OverlayDrawer.DrawAllOverlays))]
    public static class Patch_OverlayDrawer_DrawAllOverlays
    {
        public static bool Prefix(OverlayDrawer __instance, ref Vector3 ___curOffset)
        {
            if (Patcher.Settings.DisplayTagOverlay) {
                var drawer = new ModifiedOverlayDrawer(__instance);
                drawer.DrawAllOverlays(ref ___curOffset);
                return false;
            } else {
                return true;
            }
        }
    }



    // Hack
    // TODO: Find a better way to inject my code
    public class ModifiedOverlayDrawer
    {
        public readonly OverlayDrawer drawer;
        public readonly Dictionary<Thing, ThingOverlaysHandle> overlayHandles;
        public readonly Dictionary<Thing, OverlayTypes> overlaysToDraw;
        public readonly DrawBatch drawBatch;
        
        public static readonly OverlayTypes OverlayTypeForTag = (OverlayTypes)0x200;

        public ModifiedOverlayDrawer(OverlayDrawer drawer)
        {
            this.drawer = drawer;
            overlayHandles = (Dictionary <Thing, ThingOverlaysHandle>)FI_overlayHandles.GetValue(drawer);
            overlaysToDraw = (Dictionary<Thing, OverlayTypes>)FI_overlaysToDraw.GetValue(drawer);
            drawBatch = (DrawBatch)FI_drawBatch.GetValue(drawer);
        }


        public static readonly FieldInfo FI_overlayHandles = typeof(OverlayDrawer).GetField("overlayHandles", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new ArgumentException("OverlayDrawer.overlayHandles is not found");
        public static readonly FieldInfo FI_overlaysToDraw = typeof(OverlayDrawer).GetField("overlaysToDraw", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new ArgumentException("OverlayDrawer.overlaysToDraw is not found");
        public static readonly FieldInfo FI_drawBatch = typeof(OverlayDrawer).GetField("drawBatch", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new ArgumentException("OverlayDrawer.drawBatch is not found");
        public static readonly float BaseAlt = (float)(typeof(OverlayDrawer).GetField("BaseAlt", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new ArgumentException("OverlayDrawer.BaseAlt is not found")).GetValue(null);


        public static Action<TTarget, TArg> GetAccessor<TTarget, TArg>(string name)
        {
            // A simple helper that reduces explicit typing.
            // Can omit binder & modifiers in .Net 6
            var x = typeof(TTarget).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(TArg) }, null) ?? throw new ArgumentException($"Cannot find {typeof(TTarget).Name}.{name}");
            return (Action<TTarget, TArg>)Delegate.CreateDelegate(typeof(Action<TTarget, TArg>), x);
        }

        public static Func<TTarget, TArg, TReturn> GetAccessor<TTarget, TArg, TReturn>(string name)
        {
            // A simple helper that reduces explicit typing.
            // Can omit binder & modifiers in .Net 6
            var x = typeof(TTarget).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(TArg) }, null) ?? throw new ArgumentException($"Cannot find {typeof(TTarget).Name}.{name}");
            return (Func<TTarget, TArg, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TArg, TReturn>), x);
        }


        public static readonly Func<OverlayDrawer, Thing, float> Accessor_StackOffsetFor = GetAccessor<OverlayDrawer, Thing, float>("StackOffsetFor");

        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderBurningWick = GetAccessor<OverlayDrawer, Thing>("RenderBurningWick");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderNeedsPowerOverlay = GetAccessor<OverlayDrawer, Thing>("RenderNeedsPowerOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderPowerOffOverlay = GetAccessor<OverlayDrawer, Thing>("RenderPowerOffOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderBrokenDownOverlay = GetAccessor<OverlayDrawer, Thing>("RenderBrokenDownOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderOutOfFuelOverlay = GetAccessor<OverlayDrawer, Thing>("RenderOutOfFuelOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderForbiddenBigOverlay = GetAccessor<OverlayDrawer, Thing>("RenderForbiddenBigOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderForbiddenOverlay = GetAccessor<OverlayDrawer, Thing>("RenderForbiddenOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderForbiddenRefuelOverlay = GetAccessor<OverlayDrawer, Thing>("RenderForbiddenRefuelOverlay");
        public static readonly Action<OverlayDrawer, Thing> Accessor_RenderQuestionMarkOverlay = GetAccessor<OverlayDrawer, Thing>("RenderQuestionMarkOverlay");

        public float StackOffsetFor(Thing parent) => Accessor_StackOffsetFor(drawer, parent);

        public void RenderBurningWick(Thing parent) => Accessor_RenderBurningWick(drawer, parent);
        public void RenderNeedsPowerOverlay(Thing parent) => Accessor_RenderNeedsPowerOverlay(drawer, parent);
        public void RenderPowerOffOverlay(Thing parent) => Accessor_RenderPowerOffOverlay(drawer, parent);
        public void RenderBrokenDownOverlay(Thing parent) => Accessor_RenderBrokenDownOverlay(drawer, parent);
        public void RenderOutOfFuelOverlay(Thing parent) => Accessor_RenderOutOfFuelOverlay(drawer, parent);
        public void RenderForbiddenBigOverlay(Thing parent) => Accessor_RenderForbiddenBigOverlay(drawer, parent);
        public void RenderForbiddenOverlay(Thing parent) => Accessor_RenderForbiddenOverlay(drawer, parent);
        public void RenderForbiddenRefuelOverlay(Thing parent) => Accessor_RenderForbiddenRefuelOverlay(drawer, parent);
        public void RenderQuestionMarkOverlay(Thing parent) => Accessor_RenderQuestionMarkOverlay(drawer, parent);

        public void RenderTagOverlay(Thing t)
        {
            // See `OverlayDrawer.RenderForbiddenOverlay` & `OverlayDrawer.RenderQuestionMarkOverlay`
            var drawPos = t.DrawPos;
            drawPos.z += t.RotatedSize.z * 0.3f;
            drawPos.x += t.RotatedSize.x * 0.3f;
            // What is altitude?
            drawPos.y = BaseAlt + 0.162162155f;
            drawBatch.DrawMesh(MeshPool.plane05, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), InitUtility.TagMaterial, 0, renderInstanced: true);
        }


        public void DrawAllOverlays(ref Vector3 curOffset)
        {
            try
            {
                foreach (KeyValuePair<Thing, ThingOverlaysHandle> overlayHandle in overlayHandles)
                {
                    if (!overlayHandle.Key.Fogged())
                    {
                        drawer.DrawOverlay(overlayHandle.Key, overlayHandle.Value.OverlayTypes);
                    }
                }
                foreach (KeyValuePair<Thing, OverlayTypes> item in overlaysToDraw)
                {
                    curOffset = Vector3.zero;
                    Thing key = item.Key;
                    OverlayTypes value = item.Value;
                    if ((value & OverlayTypes.BurningWick) != 0)
                    {
                        RenderBurningWick(key);
                    }
                    else
                    {
                        OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
                        int bitCountOf = Gen.GetBitCountOf((long)(value & overlayTypes));
                        float num = StackOffsetFor(key);
                        switch (bitCountOf)
                        {
                            case 1:
                                curOffset = Vector3.zero;
                                break;
                            case 2:
                                curOffset = new Vector3(-0.5f * num, 0f, 0f);
                                break;
                            case 3:
                                curOffset = new Vector3(-1.5f * num, 0f, 0f);
                                break;
                        }
                        if ((value & OverlayTypes.NeedsPower) != 0)
                        {
                            RenderNeedsPowerOverlay(key);
                        }
                        if ((value & OverlayTypes.PowerOff) != 0)
                        {
                            RenderPowerOffOverlay(key);
                        }
                        if ((value & OverlayTypes.BrokenDown) != 0)
                        {
                            RenderBrokenDownOverlay(key);
                        }
                        if ((value & OverlayTypes.OutOfFuel) != 0)
                        {
                            RenderOutOfFuelOverlay(key);
                        }
                    }
                    if ((value & OverlayTypes.ForbiddenBig) != 0)
                    {
                        RenderForbiddenBigOverlay(key);
                    }
                    if ((value & OverlayTypes.Forbidden) != 0)
                    {
                        RenderForbiddenOverlay(key);
                    }
                    if ((value & OverlayTypes.ForbiddenRefuel) != 0)
                    {
                        RenderForbiddenRefuelOverlay(key);
                    }
                    if ((value & OverlayTypes.QuestionMark) != 0)
                    {
                        RenderQuestionMarkOverlay(key);
                    }
                    // This is the only thing I added
                    if ((value & OverlayTypeForTag) != 0)
                    {
                        RenderTagOverlay(key);
                    }
                }
            }
            finally
            {
                overlaysToDraw.Clear();
            }
            drawBatch.Flush();
        }
    }


}