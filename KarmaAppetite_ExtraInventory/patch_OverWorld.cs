using System;

public class patch_OverWorld
{
    public static void Patch()
    {
        On.OverWorld.WorldLoaded += OverWorld_WorldLoaded;
    }

    private static void OverWorld_WorldLoaded(On.OverWorld.orig_WorldLoaded orig, OverWorld self)
    {
        orig.Invoke(self);
        if (self.game.session is StoryGameSession)
        {
            KarmaAppetite_ExtraInventory.RecreateSave(self.game.session as StoryGameSession);
            KarmaAppetite_ExtraInventory.ReloadInventories(self.game.session as StoryGameSession);
        }
    }

}
