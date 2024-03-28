using BomberKnight.BombElements;
using BomberKnight.ItemData;
using BomberKnight.ModInterop.Randomizer;
using ItemChanger;
using KorzUtils.Helper;
using Modding;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.StringLogic;
using RandomizerMod.Logging;
using RandomizerMod.Menu;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BomberKnight.ModInterop;

internal class RandomizerInterop
{
    #region Properties

    internal static RandoSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file.
    /// </summary>
    public static bool PlayingRandomizer
    {
        get
        {
            if (ModHooks.GetMod("Randomizer 4", true) is not Mod)
                return false;
            else
                return RandoFile;
        }
    }

    /// <summary>
    /// Gets the flag, that indicates if this is a rando file. To prevent missing reference exceptions, this is seperated from <see cref="PlayingRandomizer"/>.
    /// </summary>
    private static bool RandoFile => RandomizerMod.RandomizerMod.IsRandoSave;

    #endregion

    internal static void Initialize()
    {
        RandoMenu.Initialize();
        SettingsLog.AfterLogSettings += SettingsLog_AfterLogSettings;
        RequestBuilder.OnUpdate.Subscribe(60f, ApplySettings);
        RandoController.OnCalculateHash += RandoController_OnCalculateHash;
        RCData.RuntimeLogicOverride.Subscribe(60f, ModifyLogic);

        if (ModHooks.GetMod("RandoSettingsManager") is Mod)
            HookRandoSettingsManager();

        CondensedSpoilerLogger.AddCategory("Bomb items:", () => Settings.Enabled, new()
        {
            ItemManager.BombBag,
            ItemManager.SporeBomb,
            ItemManager.BounceBomb,
            ItemManager.EchoBomb,
            ItemManager.GoldBomb,
            ItemManager.PowerBomb
        });
    }

    private static int RandoController_OnCalculateHash(RandoController arg1, int arg2)
    {
        if (!Settings.Enabled)
            return 0;
        return Settings.Place == Enums.RandoType.Vanilla ? 227 : 0;
    }

    private static void HookRandoSettingsManager()
    {
        RandoSettingsManagerMod.Instance.RegisterConnection(new SimpleSettingsProxy<RandoSettings>(BomberKnight.Instance,
        RandoMenu.Instance.PassSettings,
        () => Settings.Enabled ? Settings : null));
    }

    private static void SettingsLog_AfterLogSettings(LogArguments args, TextWriter textWriter)
    {
        textWriter.WriteLine("Bomber Knight settings:");
        using Newtonsoft.Json.JsonTextWriter jsonTextWriter = new(textWriter) { CloseOutput = false, };
        RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jsonTextWriter, Settings);
        textWriter.WriteLine();
    }

    private static void ApplySettings(RequestBuilder builder)
    {
        if (Settings.Enabled)
        {
            if (Settings.Place == Enums.RandoType.Randomized)
            {
                builder.AddItemByName(ItemManager.BombBag, 3);
                builder.AddItemByName(ItemManager.BombMasterCharm);
                builder.AddItemByName(ItemManager.PyromaniacCharm);
                builder.AddItemByName(ItemManager.ShellSalvagerCharm);
                builder.AddItemByName(ItemManager.SporeBomb);
                builder.AddItemByName(ItemManager.BounceBomb);
                builder.AddItemByName(ItemManager.GoldBomb);
                builder.AddItemByName(ItemManager.EchoBomb);
                builder.AddItemByName(ItemManager.PowerBomb);

                builder.AddLocationByName(ItemManager.GreenpathBombBagChallenge);
                builder.AddLocationByName(ItemManager.DeepnestBombBagChallenge);
                builder.AddLocationByName(ItemManager.EdgeBombBagChallenge);
                builder.AddLocationByName(ItemManager.BombMasterCharmPuzzle);
                builder.AddLocationByName(ItemManager.PyromaniacCharmPuzzle);
                builder.AddLocationByName(ItemManager.ShellSalvagerCharmPuzzle);
                builder.AddLocationByName(ItemManager.SporeBombPuzzle);
                builder.AddLocationByName(ItemManager.BounceBombPuzzle);
                builder.AddLocationByName(ItemManager.EchoBombPuzzle);
                builder.AddLocationByName(ItemManager.GoldBombPuzzle);
                builder.AddLocationByName(ItemManager.PowerBombPuzzle);
            }
            else
            {
                builder.AddToVanilla(ItemManager.BombBag, ItemManager.GreenpathBombBagChallenge);
                builder.AddToVanilla(ItemManager.BombBag, ItemManager.DeepnestBombBagChallenge);
                builder.AddToVanilla(ItemManager.BombBag, ItemManager.EdgeBombBagChallenge);
                builder.AddToVanilla(ItemManager.BombMasterCharm, ItemManager.BombMasterCharmPuzzle);
                builder.AddToVanilla(ItemManager.PyromaniacCharm, ItemManager.PyromaniacCharmPuzzle);
                builder.AddToVanilla(ItemManager.ShellSalvagerCharm, ItemManager.ShellSalvagerCharmPuzzle);
                builder.AddToVanilla(ItemManager.SporeBomb, ItemManager.SporeBombPuzzle);
                builder.AddToVanilla(ItemManager.BounceBomb, ItemManager.BounceBombPuzzle);
                builder.AddToVanilla(ItemManager.EchoBomb, ItemManager.EchoBombPuzzle);
                builder.AddToVanilla(ItemManager.GoldBomb, ItemManager.GoldBombPuzzle);
                builder.AddToVanilla(ItemManager.PowerBomb, ItemManager.PowerBombPuzzle);
            }
        }
    }

    private static void ModifyLogic(GenerationSettings settings, LogicManagerBuilder builder)
    {
        if (Settings.Enabled)
        {
            JsonLogicFormat jsonLogicFormat = new();
            ItemManager.Seed = settings.Seed;
            BombManager.Active = true;
            using Stream termStream = ResourceHelper.LoadResource<BomberKnight>("ItemChangerData.Randomizer.Terms.json");
            builder.DeserializeFile(LogicFileType.Terms, jsonLogicFormat, termStream);

            builder.AddItem(new SingleItem(ItemManager.BombBag, new RandomizerCore.TermValue(builder.GetTerm("BOMBBAG"), 1)));
            builder.AddItem(new SingleItem(ItemManager.BombMasterCharm, new RandomizerCore.TermValue(builder.GetTerm("CHARMS"), 1)));
            builder.AddItem(new SingleItem(ItemManager.PyromaniacCharm, new RandomizerCore.TermValue(builder.GetTerm("CHARMS"), 1)));
            builder.AddItem(new SingleItem(ItemManager.ShellSalvagerCharm, new RandomizerCore.TermValue(builder.GetTerm("CHARMS"), 1)));
            builder.AddItem(new SingleItem(ItemManager.SporeBomb, new RandomizerCore.TermValue(builder.GetTerm("SPOREBOMB"), 1)));
            builder.AddItem(new SingleItem(ItemManager.BounceBomb, new RandomizerCore.TermValue(builder.GetTerm("BOUNCEBOMB"), 1)));
            builder.AddItem(new SingleItem(ItemManager.EchoBomb, new RandomizerCore.TermValue(builder.GetTerm("ECHOBOMB"), 1)));
            builder.AddItem(new SingleItem(ItemManager.GoldBomb, new RandomizerCore.TermValue(builder.GetTerm("GOLDBOMB"), 1)));
            builder.AddItem(new SingleItem(ItemManager.PowerBomb, new RandomizerCore.TermValue(builder.GetTerm("POWERBOMB"), 1)));

            using Stream locationStream = ResourceHelper.LoadResource<BomberKnight>("ItemChangerData.Randomizer.LocationLogic.json");
            builder.DeserializeFile(LogicFileType.Locations, jsonLogicFormat, locationStream);

            Dictionary<string, LogicClause> macros = ReflectionHelper.GetField<LogicProcessor, Dictionary<string, LogicClause>>(builder.LP, "macros");
            foreach (string term in macros.Keys.ToList())
                builder.DoSubst(new(term, "QUAKE", "(QUAKE | BOMBBAG)"));
        }
        else
            BombManager.Active = false;
    }
}
