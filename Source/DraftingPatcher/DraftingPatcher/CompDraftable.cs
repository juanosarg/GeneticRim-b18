
using Verse;

namespace DraftingPatcher
{
    class CompDraftable : ThingComp
    {
        

        public CompProperties_Draftable Props
        {
            get
            {
                return (CompProperties_Draftable)this.props;
            }
        }

        public bool GetExplodable
        {
            get
            {
                return this.Props.explodable;
            }
        }

        public bool GetRage
        {
            get
            {
                return this.Props.rage;
            }
        }



    }
}
