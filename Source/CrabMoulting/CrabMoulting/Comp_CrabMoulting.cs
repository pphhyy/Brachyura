using System;
using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace CrabMoulting
{
    class Comp_CrabMoulting : ThingComp
    {
        public CompProperties_CrabMoulting Props
        {
            get
            {
                return (CompProperties_CrabMoulting)this.props;
            }
        }

        public float currentTicks = 0;

        public Pawn GetParent()
        {
            return parent as Pawn;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentTicks, "CrabMoultCurrentProgress", 0);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn p = GetParent();
            if (p.health.hediffSet.GetFirstHediffOfDef(Props.moultHediff) == null)
            {
                p.health.AddHediff(Props.moultHediff).Severity = 0.1f;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            Pawn pawn = GetParent();

            if (!pawn.Dead || (!Props.allowNonPlayer && (GetParent().Faction != null || GetParent().Faction != Faction.OfPlayer)))
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.moultHediff);
                if(hediff != null && hediff.Severity < Props.maxSeverity)
                {
                    if (currentTicks >= (Props.days * 60000) && pawn.Spawned)
                    {
                        hediff.Severity += Props.severityPerMoult;
                        currentTicks = 0;
                        DoMoult(pawn);

                        return;
                    }
                    currentTicks++;
                }
            }
        }

        public void DoMoult(Pawn p)
        {
            if (Props.regrowLimbs)
            {
                List<Hediff> hediffList = new List<Hediff>() { };
                foreach (Hediff hd in p.health.hediffSet.hediffs.Where(x => p.health.hediffSet.PartIsMissing(x.Part)))
                {
                    hediffList.Add(hd);
                }
                for (int i = 0; i < hediffList.Count; i++)
                {
                    p.health.RestorePart(hediffList[i].Part);
                }
            }
            if(Props.moultedItem != null)
            {
                Thing thing = ThingMaker.MakeThing(Props.moultedItem);
                thing.stackCount = Props.moultedCount;
                GenPlace.TryPlaceThing(thing, p.Position, p.Map, ThingPlaceMode.Near, null, null, default(Rot4));
            }
        }

        public override string CompInspectStringExtra()
        {
            if (Props.allowNonPlayer || (!Props.allowNonPlayer && (GetParent().Faction != null || GetParent().Faction == Faction.OfPlayer)))
            {
                return "pphhyy_Brachyura_MoultProgress".Translate((currentTicks / (Props.days * 60000)).ToStringPercent());
            }
            return "";
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: MAX",
                    action = delegate ()
                    {
                        currentTicks = Props.days * 60000;
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "DEBUG: RESET SEVERITY",
                    action = delegate ()
                    {
                        GetParent().health.hediffSet.GetFirstHediffOfDef(Props.moultHediff).Severity = Props.moultHediff.initialSeverity;
                    }
                };
            }
        }
    }
}
