using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore;
using ImGuiNET;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace AtlasCompletionInfo;

public class AtlasCompletionInfo : BaseSettingsPlugin<AtlasCompletionInfoSettings>
{

    List<(string, int)> AtlasMaps = [];
    List<(string, int)> AtlasUniqueMaps = [];
    public string[] MapsToExclude;

    private string[] CompletedMaps = [];

    List<(string, int)> MissingMaps = [];
    List<(string, int)> MissingUniqueMaps = [];

    private int AmountCompleted = 0;
    private int NewAmountCompleted = 0;
    private int AmountMissing = 0;
    private int AmountUniqueMissing = 0;


    public override bool Initialise()
    {
        Settings.Copy.OnPressed = CopyUncompletedMaps;
        Settings.SortAlphabetically.OnValueChanged += (sender, newValue) => UpdateMapsArrays();
        UpdateAtlasMaps();
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

    private void UpdateAtlasMaps()
    {
        var ingameState = GameController.Game.IngameState;
        var atlasNodesAux = ingameState.TheGame.Files.AtlasNodes.EntriesList;

        List<(string, int, int)> AtlasMapsFromAtlasNodes = [];
        foreach (var node in atlasNodesAux)
        {
            var baseAtlasNodeAddress = node.Address;
            //var tier0 = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x51);
            var tier0 = node.TierProgression[0];

            //var AtlasNodeNeighbours = ingameState.M.Read<int>(baseAtlasNodeAddress + 0x41);
            var AtlasNodeNeighbours = node.Connections.Count;

            AtlasMapsFromAtlasNodes.Add((node.Area.Name, AtlasNodeNeighbours, tier0));

        }

        AtlasUniqueMaps = AtlasMapsFromAtlasNodes
            .Where(item => item.Item2 == 1)
            .Select(tuple => (tuple.Item1, tuple.Item3)).ToList();

        MapsToExclude = AtlasMapsFromAtlasNodes
           .Where(item => item.Item2 <= 1)
           .Select(x => x.Item1).ToArray();

        AtlasMaps = AtlasMapsFromAtlasNodes.Select(tuple => (tuple.Item1, tuple.Item3)).ToList().Where(tuple => !MapsToExclude.Contains(tuple.Item1))
            .ToList();

    }

    private void UpdateMapsArrays()
    {
        CompletedMaps = GameController.IngameState.ServerData.BonusCompletedAreas.Select(area => area.Name).ToArray();
        if (CompletedMaps.Length > 0)
        {
            var MissingMapsAux = AtlasMaps.Where(tuple => !CompletedMaps.Contains(tuple.Item1)).ToList();
            var MissingUniqueMapsAux = AtlasUniqueMaps.Where(tuple => !CompletedMaps.Contains(tuple.Item1)).ToList();

            if (!Settings.SortAlphabetically)
            {
                MissingMaps = MissingMapsAux.OrderBy(tuple => tuple.Item2).ThenBy(tuple => tuple.Item1).ToList();
                MissingUniqueMaps = MissingUniqueMapsAux.OrderBy(tuple => tuple.Item2).ThenBy(tuple => tuple.Item1).ToList();
            }
            else
            {
                MissingMaps = MissingMapsAux.OrderBy(tuple => tuple.Item1).ToList();
                MissingUniqueMaps = MissingUniqueMapsAux.OrderBy(tuple => tuple.Item1).ToList();
            }
            AmountMissing = MissingMaps.Count;
            AmountUniqueMissing = MissingUniqueMaps.Count;
        }
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
            var missingMapsString = string.Join(separator, MissingMaps.Select(tuple => tuple.Item1 + (Settings.IncludeBaseTiers ? " T" + tuple.Item2 : string.Empty)));
            stringToClipboard += missingMapsString;
        }
        if (AmountUniqueMissing > 0)
        {
            var missingUniqueMapsString = string.Join(separator, MissingUniqueMaps.Select(tuple => tuple.Item1 + (Settings.IncludeBaseTiers ? " T" + tuple.Item2 : string.Empty)));
            stringToClipboard += separator + missingUniqueMapsString;
        }

        ImGui.SetClipboardText(stringToClipboard);
        DebugWindow.LogMsg("Uncompleted Maps copied", 3f);
    }

    public override void Render()
    {
        if (Settings.HighlightPartyMembers &&
            !GameController.IngameState.IngameUi.FullscreenPanels.Any(panel => panel.IsVisible) &&
            !GameController.Game.IngameState.IngameUi.OpenLeftPanel.IsVisible &&
            !GameController.IngameState.IngameUi.ChatTitlePanel.IsVisible)
        {
            var partyElement = GameController.IngameState.IngameUi.PartyElement;
            foreach (var partyPlayer in partyElement.PlayerElements)
            {
                var areaName = partyPlayer.ZoneName;
                if (!string.IsNullOrEmpty(areaName) &&
                    (MissingMaps.Any(map => map.Item1 == areaName) || MissingUniqueMaps.Any(map => map.Item1 == areaName)))
                {
                    var pos = partyPlayer.GetClientRect();
                    Graphics.DrawFrame(pos, new Color(0, 255, 0, 255), 2);
                }
            }

            
            /*
            var partyPlayers = partyElement?.Children.FirstOrDefault()?.Children.FirstOrDefault()?.Children;

            if (partyPlayers != null && partyPlayers.Count > 0)
            {
                foreach (var player in partyPlayers)
                {
                    var playerAreaElement = player.Children.ElementAtOrDefault(2);
                    var areaName = playerAreaElement?.Text;

                    if (!string.IsNullOrEmpty(areaName) &&
                        (MissingMaps.Any(map => map.Item1 == areaName) || MissingUniqueMaps.Any(map => map.Item1 == areaName)))
                    {
                        var pos = player.GetClientRect();
                        Graphics.DrawFrame(pos, new Color(0, 255, 0, 255), 2);
                    }
                }
            }*/
        }


        if (Settings.ShowMapsOnAtlas && GameController.IngameState.IngameUi.Atlas.IsVisible && !GameController.IngameState.IngameUi.OpenLeftPanel.IsVisible)
        {
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

                    var entryText = MissingMaps[i].Item1;

                    if (Settings.ShowTiers)
                    {
                        entryText += " " + MissingMaps[i].Item2;
                    }

                    var entryTextX = entryX + textPadding;
                    var entryTextY = entryY + (entryHeight - fontSize) / 2;

                    Graphics.DrawImage("menu-background.png", new RectangleF(entryX, entryY, entryWidth, entryHeight));
                    Graphics.DrawText(entryText, new Vector2(entryTextX, entryTextY));
                }

                // Unique Maps
                var uniqueEntryWidth = 200;
                var separatorPad = 30;
                var uniqueMapsStartX = startX + separatorPad + (entryWidth + columnSpacing) * ((MissingMaps.Count + maxEntriesPerColumn - 1) / maxEntriesPerColumn);
                for (int i = 0; i < AmountUniqueMissing; i++)
                {
                    var columnIndex = i / maxEntriesPerColumn;
                    var rowIndex = i % maxEntriesPerColumn;

                    var entryX = uniqueMapsStartX + columnIndex * (entryWidth + columnSpacing);
                    var entryY = startY + rowIndex * (entryHeight + rowSpacing);

                    var entryText = MissingUniqueMaps[i].Item1;

                    if (Settings.ShowTiers)
                    {
                        entryText += " " + MissingUniqueMaps[i].Item2;
                    }

                    var entryTextX = entryX + textPadding;
                    var entryTextY = entryY + (entryHeight - fontSize) / 2;

                    Graphics.DrawImage("menu-background.png", new RectangleF(entryX, entryY, uniqueEntryWidth, entryHeight));
                    Graphics.DrawText(entryText, new Vector2(entryTextX, entryTextY));
                }
            }
        }
    }
}