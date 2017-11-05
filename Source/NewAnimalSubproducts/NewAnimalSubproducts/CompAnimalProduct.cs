using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NewAnimalSubproducts
{
    public class CompAnimalProduct : CompHasGatherableBodyResource
    {
        protected override int GatherResourcesIntervalDays
        {
            get
            {
                return this.Props.gatheringIntervalDays;
            }
        }



        protected override int ResourceAmount
        {
            get
            {
                return this.Props.resourceAmount;
            }
        }

        protected override ThingDef ResourceDef
        {
            get
            {
                return this.Props.resourceDef;
            }
        }

        protected override string SaveKey
        {
            get
            {
                return "resourceGrowth";
            }
        }

        public CompProperties_AnimalProduct Props
        {
            get
            {
                return (CompProperties_AnimalProduct)this.props;
            }
        }

        protected override bool Active
        {
            get
            {
                if (!base.Active)
                {
                    return false;
                }
                Pawn pawn = this.parent as Pawn;
                return pawn == null || pawn.ageTracker.CurLifeStage.shearable;
            }
        }

        public override string CompInspectStringExtra()
        {
            if (!this.Active)
            {
                return null;
            }

            if (ResourceDef.ToString() == "Owlbearfeathers")
            {
                return Translator.Translate("FeatherGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "WolfchickenFeathers")
            {
                return Translator.Translate("FeatherGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "RoyalJelly")
            {
                return Translator.Translate("JellyGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "Weapon_ThrownSac")
            {
                return Translator.Translate("SacGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "ChickenspiderSilk")
            {
                return Translator.Translate("SilkGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "PoisonAmpoule")
            {
                return Translator.Translate("PoisonGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "VirulentPoison")
            {
                return Translator.Translate("PoisonGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "SpidersnakeSkin")
            {
                return Translator.Translate("SkinShed") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "WolfsnakeSkin")
            {
                return Translator.Translate("SkinShed") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "SpidersnakeSilk")
            {
                return Translator.Translate("SilkGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "GR_EldritchInsectJelly")
            {
                return Translator.Translate("JellyGrowth2") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "GR_DarkYoungWoodLog")
            {
                return Translator.Translate("TentacleGrowth") + ": " + base.Fullness.ToStringPercent();
            }
            else if (ResourceDef.ToString() == "GR_ChocolateEgg")
            {
                return Translator.Translate("ChocolateEgg") + ": " + base.Fullness.ToStringPercent();
            }

            else  return Translator.Translate("ResourceGrowth") + ": " + base.Fullness.ToStringPercent();
        }



    }
}
