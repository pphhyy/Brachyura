using System;
using Verse;
using RimWorld;
using System.Linq;
using System.Collections.Generic;

namespace CrabMoulting
{
    class CompProperties_CrabMoulting : CompProperties
    {

        public CompProperties_CrabMoulting()
        {
            this.compClass = typeof(Comp_CrabMoulting);
        }

        public float days = 15;
        public HediffDef moultHediff;
        public float severityPerMoult = 0.1f;
        public float maxSeverity = 1f;

        public bool regrowLimbs = true;
        public ThingDef moultedItem;
        public int moultedCount;

        public bool allowNonPlayer = false;
    }
}
