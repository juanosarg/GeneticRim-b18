using Verse;

namespace DraftingPatcher
{
    public class CompProperties_Draftable : CompProperties
    {

        public bool explodable = false;
        public bool rage = false;

        public CompProperties_Draftable()
        {
            this.compClass = typeof(CompDraftable);
        }
    }
}
