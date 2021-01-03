using System;
using System.IO;
using RWCustom;

public class patch_PlayerProgression
{
    public static void Patch()
    {
        On.PlayerProgression.GetOrInitiateSaveState += PlayerProgression_GetOrInitiateSaveState;
    }

    private static SaveState PlayerProgression_GetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int saveStateNumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveAsDeathOrQuit)
    {
        if (File.Exists(Custom.RootFolderDirectory() + "extraInventory_size.txt"))
        {
            string text = File.ReadAllText("extraInventory_size.txt");
            int size;
            if (int.TryParse(text, out size))
            {
                if (size >= 0)
                {
                    KarmaAppetite_ExtraInventory.MaxSize = size;
                }
            }
        }
        return orig.Invoke(self, saveStateNumber, game, setup, saveAsDeathOrQuit);
    }
}
