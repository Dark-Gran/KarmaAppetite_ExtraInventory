using System;

public class patch_StoryGameSession
{
    public static void Patch()
    {
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        
    }

    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);
        KarmaAppetite_ExtraInventory.AddInventory(self, player);
    }

}
