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
    public class Patcher : Mod
    {
        public static Settings Settings = new();

        public Patcher(ModContentPack pack) : base(pack)
        {
            Settings = GetSettings<Settings>();
            // Do something to apply the settings
            DoPatching();
        }
        public override string SettingsCategory()
        {
            return "Tag It";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var list = new Listing_Standard();
            list.Begin(inRect);
            list.CheckboxLabeled("Display tag overlay", ref Settings.DisplayTagOverlay, "Whether to display tag icon on top of the item. Disable it if you spot incompatibility.");
            list.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings() {
            base.WriteSettings();
            // Do something to apply the settings
        }

        public void DoPatching()
        {
            var harmony = new Harmony("com.colinfang.TagIt");
            harmony.PatchAll();
        }
    }

    public class Settings : ModSettings
    {
        public bool DisplayTagOverlay = true;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref DisplayTagOverlay, nameof(DisplayTagOverlay), true);
            base.ExposeData();
        }
    }

    [StaticConstructorOnStartup]
    public static class InitUtility
    {
        public static readonly Texture2D TagIcon = ContentFinder<Texture2D>.Get("CF_TagIt/UI/Tag");
        public static readonly Material TagMaterial = MaterialPool.MatFrom("CF_TagIt/UI/Tag", ShaderDatabase.MetaOverlay);

        static InitUtility()
        {
            foreach (var t in DefDatabase<ThingDef>.AllDefs)
            {
                if (t.EverHaulable)
                {
                    t.comps.Add(new CompProperties_FilterTag());
                }

            }
        }
    }

}