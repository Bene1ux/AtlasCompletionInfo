using System.Windows.Forms;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace AtlasCompletionInfo;

public class AtlasCompletionInfoSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);


    [Menu("General Settings", 0, CollapsedByDefault = false)]
    public EmptyNode Empty0 { get; set; }

    [Menu("Show Maps on Atlas screen", parentIndex = 0)]
    public ToggleNode ShowMapsOnAtlas { get; set; } = new ToggleNode(true);

    [Menu("Sort missing Maps alphabetically", parentIndex = 0)]
    public ToggleNode SortAlphabetically { get; set; } = new ToggleNode(false);

    [Menu("Show Base Map Tiers", parentIndex = 0)]
    public ToggleNode ShowTiers { get; set; } = new ToggleNode(true);

    [Menu("Highlight Party Members in uncompleted Maps", parentIndex = 0)]
    public ToggleNode HighlightPartyMembers { get; set; } = new ToggleNode(true);


    [Menu("Copy Settings", 1, CollapsedByDefault = false)]
    public EmptyNode Empty1 { get; set; }

    [Menu("Include Base Map Tiers", parentIndex = 1)]
    public ToggleNode IncludeBaseTiers { get; set; } = new ToggleNode(false);

    [Menu("Hotkey to Copy:", parentIndex = 1)]
    public HotkeyNode CopyHotkey { get; set; } = Keys.None;

    [Menu("Copy uncompleted Maps", parentIndex = 1)]
    public ButtonNode Copy { get; set; } = new ButtonNode();





}