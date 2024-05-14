using System;
using System.Linq;
using ExileCore;
using ImGuiNET;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace AtlasCompletionInfo;

public class AtlasCompletionInfo : BaseSettingsPlugin<AtlasCompletionInfoSettings>
{
    private readonly string[] AtlasMaps = [
        "Academy",
        "Acid Caverns",
        "Arachnid Nest",
        "Arcade",
        "Arena",
        "Arid Lake",
        "Arsenal",
        "Ashen Wood",
        "Atoll",
        "Barrows",
        "Basilica",
        "Belfry",
        "Bog",
        "Bone Crypt",
        "Bramble Valley",
        "Burial Chambers",
        "Cage",
        "Canyon",
        "Castle Ruins",
        "Cells",
        "Cemetery",
        "Channel",
        "Cold River",
        "Conservatory",
        "Courthouse",
        "Courtyard",
        "Crimson Temple",
        "Crimson Township",
        "Crystal Ore",
        "Cursed Crypt",
        "Defiled Cathedral",
        "Desert Spring",
        "Dry Sea",
        "Dunes",
        "Dungeon",
        "Estuary",
        "Excavation",
        "Fields",
        "Flooded Mine",
        "Forbidden Woods",
        "Forking River",
        "Foundry",
        "Frozen Cabins",
        "Fungal Hollow",
        "Gardens",
        "Glacier",
        "Grave Trough",
        "Graveyard",
        "Grotto",
        "Iceberg",
        "Jungle Valley",
        "Lair",
        "Lava Chamber",
        "Lava Lake",
        "Lookout",
        "Mausoleum",
        "Maze",
        "Mineral Pools",
        "Moon Temple",
        "Museum",
        "Necropolis",
        "Orchard",
        "Overgrown Shrine",
        "Palace",
        "Park",
        "Phantasmagoria",
        "Pier",
        "Pit",
        "Plateau",
        "Plaza",
        "Port",
        "Primordial Pool",
        "Promenade",
        "Reef",
        "Residence",
        "Shipyard",
        "Shore",
        "Shrine",
        "Siege",
        "Silo",
        "Spider Forest",
        "Stagnation",
        "Strand",
        "Sulphur Vents",
        "Sunken City",
        "Temple",
        "Terrace",
        "Thicket",
        "Tower",
        "Toxic Sewer",
        "Tropical Island",
        "Underground River",
        "Underground Sea",
        "Vaal Pyramid",
        "Vault",
        "Volcano",
        "Waste Pool",
        "Wasteland",
        "Waterways",
        "Wharf"
    ];
    private readonly string[] AtlasUniqueMaps = [
        "Acton's Nightmare",
        "Caer Blaidd, Wolfpack's Den",
        "Death and Taxes",
        "Hallowed Ground",
        "Maelström of Chaos",
        "Mao Kun",
        "Oba's Cursed Trove",
        "Olmec's Sanctum",
        "Pillars of Arun",
        "Poorjoy's Asylum",
        "The Coward's Trial",
        "The Putrid Cloister",
        "The Twilight Temple",
        "Vaults of Atziri",
        "Whakawairua Tuahu"
    ];
    private string[] CompletedMaps = [];
    private string[] MissingMaps = [];
    private string[] MissingUniqueMaps = [];
    private int AmountCompleted = 0;
    private int NewAmountCompleted = 0;
    private int AmountMissing = 0;
    private int AmountUniqueMissing = 0;


    public override bool Initialise()
    {
        Settings.Copy.OnPressed = CopyUncompletedMaps;
        return true;
    }

    public override Job Tick()
    {
        NewAmountCompleted = GameController.IngameState.ServerData.BonusCompletedAreas.Count;

        if (NewAmountCompleted != AmountCompleted)
        {
            UpdateMapsArrays();
            AmountCompleted = NewAmountCompleted;
        }

        if (Settings.CopyHotkey.PressedOnce())
        {
            CopyUncompletedMaps();
        }

        return null;
    }

    private void UpdateMapsArrays()
    {
        CompletedMaps = GameController.IngameState.ServerData.BonusCompletedAreas.Select(area => area.Name).ToArray();
        MissingMaps = AtlasMaps.Except(CompletedMaps).ToArray();
        MissingUniqueMaps = AtlasUniqueMaps.Except(CompletedMaps).ToArray();

        AmountMissing = MissingMaps.Length;
        AmountUniqueMissing = MissingUniqueMaps.Length;
    }

    private void CopyUncompletedMaps()
    {
        if (AmountMissing + AmountUniqueMissing == 0)
        {
            return;
        }

        var stringToClipboard = "";
        var separator = ", ";

        if (AmountMissing > 0)
        {
            var missingMapsString = string.Join(separator, MissingMaps);

            stringToClipboard += missingMapsString;
        }
        if (AmountUniqueMissing > 0)
        {
            var missingUniqueMapsString = string.Join(separator, MissingUniqueMaps);
            stringToClipboard += separator + missingUniqueMapsString;
        }

        ImGui.SetClipboardText(stringToClipboard);
        DebugWindow.LogMsg("Uncompleted Maps copied", 3f);
    }

    public override void Render()
    {
        if (!Settings.ShowMapsOnAtlas || !GameController.IngameState.IngameUi.Atlas.IsVisible || GameController.IngameState.IngameUi.OpenLeftPanel.IsVisible)
        {
            return;
        }

        // Header
        var scrRect = GameController.Window.GetWindowRectangle();

        var headerX = scrRect.Width * 0.05f;
        var headerY = scrRect.Height * 0.08f;
        var headerWidth = scrRect.Width * 0.2f;
        var headerHeight = 20;
        var headerImage = new RectangleF(headerX, headerY, headerWidth, headerHeight);

        Graphics.DrawImage("preload-start.png", headerImage);

        var headerText = "Uncompleted Maps";
        var headerTextSize = Graphics.MeasureText(headerText);
        var headerTextX = headerX + (headerWidth - headerTextSize.X) / 2;
        var headerTextY = headerY + (headerHeight - headerTextSize.Y) / 2;

        Graphics.DrawText(headerText, new Vector2(headerTextX, headerTextY));

        // Missing Maps
        var hoverCheck = ImGui.GetMousePos();
        if (headerImage.Contains(hoverCheck.X, hoverCheck.Y))
        {

            var entryWidth = 150;
            var columnSpacing = 10;
            var rowSpacing = 5;
            var textPadding = 5;
            var startY = scrRect.Height * 0.15f;
            var endY = scrRect.Height * 0.70f;
            var startX = scrRect.Width * 0.08f;
            var entryHeight = (int)Math.Floor(scrRect.Height * 0.0167f);
            var fontSize = ImGui.GetFontSize();

            var maxEntriesPerColumn = (int)Math.Floor((endY - startY) / (entryHeight + rowSpacing));


            // Regular Maps
            for (int i = 0; i < AmountMissing; i++)
            {
                var columnIndex = i / maxEntriesPerColumn;
                var rowIndex = i % maxEntriesPerColumn;

                var entryX = startX + columnIndex * (entryWidth + columnSpacing);
                var entryY = startY + rowIndex * (entryHeight + rowSpacing);

                var entryText = MissingMaps[i];
                var entryTextX = entryX + textPadding;
                var entryTextY = entryY + (entryHeight - fontSize) / 2;

                Graphics.DrawImage("menu-background.png", new RectangleF(entryX, entryY, entryWidth, entryHeight));
                Graphics.DrawText(entryText, new Vector2(entryTextX, entryTextY));
            }

            // Unique Maps
            var uniqueEntryWidth = 200;
            var separatorPad = 30;
            var uniqueMapsStartX = startX + separatorPad + (entryWidth + columnSpacing) * ((MissingMaps.Length + maxEntriesPerColumn - 1) / maxEntriesPerColumn);
            for (int i = 0; i < AmountUniqueMissing; i++)
            {
                var columnIndex = i / maxEntriesPerColumn;
                var rowIndex = i % maxEntriesPerColumn;

                var entryX = uniqueMapsStartX + columnIndex * (entryWidth + columnSpacing);
                var entryY = startY + rowIndex * (entryHeight + rowSpacing);

                var entryText = MissingUniqueMaps[i];
                var entryTextX = entryX + textPadding;
                var entryTextY = entryY + (entryHeight - fontSize) / 2;

                Graphics.DrawImage("menu-background.png", new RectangleF(entryX, entryY, uniqueEntryWidth, entryHeight));
                Graphics.DrawText(entryText, new Vector2(entryTextX, entryTextY));
            }
        }
    }
}