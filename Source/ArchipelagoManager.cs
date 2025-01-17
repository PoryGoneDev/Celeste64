
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Foster.Framework;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Packets;
using System.Text;
using Archipelago.MultiClient.Net.Models;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Newtonsoft.Json.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace Celeste64;



[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ArchipelagoConnectionInfo))]
public partial class ArchipelagoConnectionInfoContext : JsonSerializerContext { }
public record ArchipelagoConnectionInfo
{
    public string Url { get; init; } = "wss://archipelago.gg:38281";
    public string SlotName { get; init; } = "Madeline";
    public string Password { get; init; } = "";
    public bool SeeGhosts { get; init; } = false;
}
public struct ArchipelagoMessage
{
    public string Text { get; init; } = "";
    public int RemainingTime { get; set; } = 300;

    public ArchipelagoMessage(string text)
    {
        Text = text;
    }
}

public class ArchipelagoManager
{
    private static readonly Version _supportedArchipelagoVersion = new(7, 7, 7);

    private readonly ArchipelagoConnectionInfo _connectionInfo;
    private ArchipelagoSession? _session;
    private DeathLinkService? _deathLinkService;
    private DateTime _lastDeath;

    public bool GoalSent = false;

    public DeathLink? DeathLinkData { get; private set; }
    public bool IsDeathLinkSafe { get; set; }
    public bool Ready { get; private set; }
    public List<Tuple<int, ItemInfo>> ItemQueue { get; private set; } = new();
    public List<long> CollectedLocations { get; private set; } = new();
    public Dictionary<long, ItemInfo> LocationDictionary { get; private set; } = new();
    public HashSet<long> SentLocations { get; set; } = [];
    public List<ArchipelagoMessage> MessageLog { get; set; } = new();

    public int Slot => _session.ConnectionInfo.Slot;
    public bool DeathLink => _session.ConnectionInfo.Tags.Contains("DeathLink");
    public int HintPoints => _session.RoomState.HintPoints;
    public int HintCost => _session.RoomState.HintCost;
    public Hint[] Hints => _session.DataStorage.GetHints();


    public int StrawberriesRequired { get; set; }
    public bool Friendsanity { get; set; }
    public bool Signsanity { get; set; }
    public bool Carsanity { get; set; }
    public bool Checkpointsanity { get; set; }
    public bool MoveShuffle { get; set; }
    public int BadelineSource { get; set; }
    public int BadelineFrequency { get; set; }
    public int BadelineSpeed { get; set; }
    public bool BadelinesDisabled { get; set; }
    public int BadelinesDisableTimer = 0;
    public int DeathLinkAmnesty { get; set; }
    public int DeathsCounted = 0;


    public static Dictionary<string, int> LocationStringToID { get; set; } = new Dictionary<string, int>
    {
        { "1/0",  0xCA0000 },
        { "1/1",  0xCA0001 },
        { "1/2",  0xCA0002 },
        { "1/3",  0xCA0003 },
        { "1/4",  0xCA0004 },
        { "1/5",  0xCA0005 },
        { "1/6",  0xCA0006 },
        { "1/7",  0xCA0007 },
        { "1/8",  0xCA0008 },
        { "1/9",  0xCA0009 },
        { "1/10", 0xCA000A },
        { "1/11", 0xCA000B },
        { "1/12", 0xCA000C },
        { "1/13", 0xCA000D },
        { "1/14", 0xCA000E },
        { "1/15", 0xCA000F },
        { "1/16", 0xCA0010 },
        { "1/17", 0xCA0011 },
        { "1/18", 0xCA0012 },
        { "1/19", 0xCA0013 },
        { "1-1/0", 0xCA0014 },
        { "1-2/0", 0xCA0015 },
        { "1-3/0", 0xCA0016 },
        { "1-4/0", 0xCA0017 },
        { "1-5/0", 0xCA0018 },
        { "1-6/0", 0xCA0019 },
        { "1-7/0", 0xCA001A },
        { "1-8/0", 0xCA001B },
        { "1-9/0", 0xCA001C },
        { "1-10/0", 0xCA001D },

        { "Granny1", 0xCA0100 },
        { "Granny2", 0xCA0101 },
        { "Granny3", 0xCA0102 },
        { "Theo1",   0xCA0103 },
        { "Theo2",   0xCA0104 },
        { "Theo3",   0xCA0105 },
        { "Baddy1",  0xCA0106 },
        { "Baddy2",  0xCA0107 },
        { "Baddy3",  0xCA0108 },

        { "Sign1", 0xCA0200 },
        { "Sign2", 0xCA0201 },
        { "Sign3", 0xCA0202 },
        { "Sign4", 0xCA0203 },
        { "CreditsSign", 0xCA0204 },

        { "Car1", 0xCA0300 },
        { "Car2", 0xCA0301 },

        { "Tutorial",         0xCA0400 },
        { "After Tutorial",   0xCA0401 },
        { "East Tower",       0xCA0402 },
        { "Rooftop",          0xCA0403 },
        { "Freeway",          0xCA0404 },
        { "Freeway Feather",  0xCA0405 },
        { "Feather Platform", 0xCA0406 },
        { "Tower Entrance",   0xCA0407 },
        { "Tower",            0xCA0408 },
        { "Baddy Island",     0xCA0409 },
    };

    public static Dictionary<int, string> LocationIDToString { get; set; } = new Dictionary<int, string>
    {
        { 0xCA0000, "1/0" },
        { 0xCA0001, "1/1" },
        { 0xCA0002, "1/2" },
        { 0xCA0003, "1/3" },
        { 0xCA0004, "1/4" },
        { 0xCA0005, "1/5" },
        { 0xCA0006, "1/6" },
        { 0xCA0007, "1/7" },
        { 0xCA0008, "1/8" },
        { 0xCA0009, "1/9" },
        { 0xCA000A, "1/10" },
        { 0xCA000B, "1/11" },
        { 0xCA000C, "1/12" },
        { 0xCA000D, "1/13" },
        { 0xCA000E, "1/14" },
        { 0xCA000F, "1/15" },
        { 0xCA0010, "1/16" },
        { 0xCA0011, "1/17" },
        { 0xCA0012, "1/18" },
        { 0xCA0013, "1/19" },
        { 0xCA0014, "1-1/0" },
        { 0xCA0015, "1-2/0" },
        { 0xCA0016, "1-3/0" },
        { 0xCA0017, "1-4/0" },
        { 0xCA0018, "1-5/0" },
        { 0xCA0019, "1-6/0" },
        { 0xCA001A, "1-7/0" },
        { 0xCA001B, "1-8/0" },
        { 0xCA001C, "1-9/0" },
        { 0xCA001D, "1-10/0" },

        // Don't need to !collect these
        // { 0xCA0100, "Granny1" },
        // { 0xCA0101, "Granny2" },
        // { 0xCA0102, "Granny3" },
        // { 0xCA0103, "Theo1" },
        // { 0xCA0104, "Theo2" },
        // { 0xCA0105, "Theo3" },
        // { 0xCA0106, "Baddy1" },
        // { 0xCA0107, "Baddy2" },
        // { 0xCA0108, "Baddy3" },

        // { 0xCA0200, "Sign1" },
        // { 0xCA0201, "Sign2" },
        // { 0xCA0202, "Sign3" },
        // { 0xCA0203, "Sign4" },
        // { 0xCA0204, "CreditsSign" },

        // { 0xCA0300, "Car1" },
        // { 0xCA0301, "Car2" },

        { 0xCA0400, "Tutorial" },
        { 0xCA0401, "After Tutorial" },
        { 0xCA0402, "East Tower" },
        { 0xCA0403, "Rooftop" },
        { 0xCA0404, "Freeway" },
        { 0xCA0405, "Freeway Feather" },
        { 0xCA0406, "Feather Platform" },
        { 0xCA0407, "Tower Entrance" },
        { 0xCA0408, "Tower" },
        { 0xCA0409, "Baddy Island" },
    };

    public static Dictionary<long, string> ItemIDToString { get; set; } = new Dictionary<long, string>
    {
        { 0xCA0000, "Strawberry" },
        { 0xCA0001, "Dash Refills" },
        { 0xCA0002, "Double Dash Refills" },
        { 0xCA0003, "Feathers" },
        { 0xCA0004, "Coins" },
        { 0xCA0005, "Cassettes" },
        { 0xCA0006, "Traffic Blocks" },
        { 0xCA0007, "Springs" },
        { 0xCA0008, "Breakable Blocks" },
        { 0xCA0009, "Raspberry" },
        { 0xCA000A, "Grounded Dash" },
        { 0xCA000B, "Air Dash" },
        { 0xCA000C, "Skid Jump" },
        { 0xCA000D, "Climb" },

        { 0xCA0020, "Intro Checkpoint" },
        { 0xCA0021, "Granny Checkpoint" },
        { 0xCA0022, "South-East Tower Checkpoint" },
        { 0xCA0023, "Climb Sign Checkpoint" },
        { 0xCA0024, "Freeway Checkpoint" },
        { 0xCA0025, "Freeway Feather Checkpoint" },
        { 0xCA0026, "Feather Maze Checkpoint" },
        { 0xCA0027, "Double Dash House Checkpoint" },
        { 0xCA0028, "Badeline Tower Checkpoint" },
        { 0xCA0029, "Badeline Island Checkpoint" },
    };

    public static Dictionary<string, string> CheckpointAPToInternal { get; set; } = new Dictionary<string, string>
    {
        { "Intro Checkpoint",             "Tutorial" },
        { "Granny Checkpoint",            "After Tutorial" },
        { "South-East Tower Checkpoint",  "East Tower" },
        { "Climb Sign Checkpoint",        "Rooftop" },
        { "Freeway Checkpoint",           "Freeway" },
        { "Freeway Feather Checkpoint",   "Freeway Feather" },
        { "Feather Maze Checkpoint",      "Feather Platform" },
        { "Double Dash House Checkpoint", "Tower Entrance" },
        { "Badeline Tower Checkpoint",    "Tower" },
        { "Badeline Island Checkpoint",   "Baddy Island" },
    };

    public static Dictionary<string, string> CheckpointInternalToAP { get; set; } = new Dictionary<string, string>
    {
        { "Tutorial",         "Intro Checkpoint" },
        { "After Tutorial",   "Granny Checkpoint" },
        { "East Tower",       "South-East Tower Checkpoint" },
        { "Rooftop",          "Climb Sign Checkpoint" },
        { "Freeway",          "Freeway Checkpoint" },
        { "Freeway Feather",  "Freeway Feather Checkpoint" },
        { "Feather Platform", "Feather Maze Checkpoint" },
        { "Tower Entrance",   "Double Dash House Checkpoint" },
        { "Tower",            "Badeline Tower Checkpoint" },
        { "Baddy Island",     "Badeline Island Checkpoint" },
    };

    public static List<string> CheckpointList { get; } = new List<string>
    {
        "Tutorial",
        "After Tutorial",
        "East Tower",
        "Rooftop",
        "Freeway",
        "Freeway Feather",
        "Feather Platform",
        "Tower Entrance",
        "Tower",
        "Baddy Island",
    };

    private static string AP_JSON_FILE = "AP.json";
    private static string? connectionInfoPath = null;
    public static string ConnectionInfoPath
    {
        get
        {
            if (connectionInfoPath == null)
            {
                var baseFolder = AppContext.BaseDirectory;
                var searchUpPath = "";
                int up = 0;
                while (!File.Exists(Path.Join(baseFolder, searchUpPath, AP_JSON_FILE)) && up++ < 5)
                    searchUpPath = Path.Join(searchUpPath, "..");
                if (!File.Exists(Path.Join(baseFolder, searchUpPath, AP_JSON_FILE)))
                    throw new Exception($"Unable to find {AP_JSON_FILE} File from '{baseFolder}'");
                connectionInfoPath = Path.Join(baseFolder, searchUpPath, AP_JSON_FILE);
            }

            return connectionInfoPath;
        }
    }

    public ArchipelagoManager(ArchipelagoConnectionInfo connectionInfo)
    {
        _connectionInfo = connectionInfo;
    }

    public async Task<LoginFailure> TryConnect()
    {
        _lastDeath = DateTime.MinValue;
        _session = ArchipelagoSessionFactory.CreateSession(_connectionInfo.Url);

        // (Re-)initialize state.
        DeathLinkData = null;
        IsDeathLinkSafe = false;
        Ready = false;
        ItemQueue = new();
        LocationDictionary = new();

        // Watch for the following events.
        _session.Socket.ErrorReceived += OnError;
        _session.Socket.PacketReceived += OnPacketReceived;
        _session.MessageLog.OnMessageReceived += OnMessageReceived;
        _session.Items.ItemReceived += OnItemReceived;
        _session.Locations.CheckedLocationsUpdated += OnLocationReceived;

        // Attempt to connect to the server.
        try
        {
            await _session.ConnectAsync();
        }
        catch (Exception ex)
        {
            Disconnect();
            return new($"Unable to establish an initial connection to the Archipelago server @ {_connectionInfo.Url}");
        }

        var result = await _session.LoginAsync(
            "Celeste 64",
            _connectionInfo.SlotName,
            ItemsHandlingFlags.AllItems,
            _supportedArchipelagoVersion,
            uuid: Guid.NewGuid().ToString(),
            password: _connectionInfo.Password
        );

        if (!result.Successful)
        {
            Disconnect();
            return result as LoginFailure;
        }

        // Load randomizer data.
        StrawberriesRequired = Convert.ToInt32(((LoginSuccessful)result).SlotData["strawberries_required"]);
        Friendsanity = Convert.ToBoolean(((LoginSuccessful)result).SlotData["friendsanity"]);
        Signsanity = Convert.ToBoolean(((LoginSuccessful)result).SlotData["signsanity"]);
        Carsanity = Convert.ToBoolean(((LoginSuccessful)result).SlotData["carsanity"]);
        Checkpointsanity = Convert.ToBoolean(((LoginSuccessful)result).SlotData["checkpointsanity"]);
        MoveShuffle = Convert.ToBoolean(((LoginSuccessful)result).SlotData["move_shuffle"]);
        Player.CNormal = Convert.ToInt32(((LoginSuccessful)result).SlotData["madeline_one_dash_hair_color"]);
        Player.CTwoDashes = Convert.ToInt32(((LoginSuccessful)result).SlotData["madeline_two_dash_hair_color"]);
        Player.CNoDash = Convert.ToInt32(((LoginSuccessful)result).SlotData["madeline_no_dash_hair_color"]);
        Player.CFeather = Convert.ToInt32(((LoginSuccessful)result).SlotData["madeline_feather_hair_color"]);
        BadelineSource = Convert.ToInt32(((LoginSuccessful)result).SlotData["badeline_chaser_source"]);
        BadelineFrequency = Convert.ToInt32(((LoginSuccessful)result).SlotData["badeline_chaser_frequency"]);
        BadelineSpeed = Convert.ToInt32(((LoginSuccessful)result).SlotData["badeline_chaser_speed"]);
        DeathLinkAmnesty = Convert.ToInt32(((LoginSuccessful)result).SlotData["death_link_amnesty"]);
        bool DeathLinkEnabled = Convert.ToBoolean(((LoginSuccessful)result).SlotData["death_link"]);

        // Initialize DeathLink service.
        _deathLinkService = _session.CreateDeathLinkService();
        _deathLinkService.OnDeathLinkReceived += OnDeathLink;
        if (DeathLinkEnabled)
        {
            _deathLinkService.EnableDeathLink();
        }

        // TODO: Wrap this and only do if active
        AddPlayerListCallback($"C64_OtherPlayers_List", PlayerListUpdated);

        // Build dictionary of locations with item information for fast lookup.
        await BuildLocationDictionary();

        // Return null to signify no error.
        Ready = true;
        return null;
    }
    public void Disconnect()
    {
        Ready = false;

        // Clear DeathLink events.
        if (_deathLinkService != null)
        {
            _deathLinkService.OnDeathLinkReceived -= OnDeathLink;
            _deathLinkService = null;
        }

        // Clear events and session object.
        if (_session != null)
        {
            _session.Socket.ErrorReceived -= OnError;
            _session.Items.ItemReceived -= OnItemReceived;
            _session.Locations.CheckedLocationsUpdated -= OnLocationReceived;
            _session.Socket.PacketReceived -= OnPacketReceived;
            _session.Socket.DisconnectAsync(); // It'll disconnect on its own time.
            _session = null;
        }
    }

    public void ClearDeathLink()
    {
        DeathLinkData = null;
        DeathsCounted = 0;
    }

    public void SendDeathLinkIfEnabled(string cause)
    {
        // Do not send any DeathLink messages if it's not enabled.
        if (!DeathLink)
        {
            return;
        }

        DeathsCounted = DeathsCounted + 1;
        if (DeathsCounted < DeathLinkAmnesty)
        {
            return;
        }

        DeathsCounted = 0;

        // Log our current time so we can make sure we ignore our own DeathLink.
        _lastDeath = DateTime.Now;
        cause = $"{_session.Players.GetPlayerAlias(Slot)} {cause}.";

        try
        {
            _deathLinkService.SendDeathLink(new(_session.Players.GetPlayerAlias(Slot), cause));
        }
        catch (ArchipelagoSocketClosedException)
        {
            // TODO: Send a message to the client that connection has been dropped.
            Disconnect();
        }

        Game.Instance.ArchipelagoManager.ClearDeathLink();
    }

    public void CheckLocations(long[] locations)
    {
        foreach (var locationID in locations)
        {
            SentLocations.Add(locationID);
        }

        try
        {
            _session.Locations.CompleteLocationChecks(locations);
        }
        catch (ArchipelagoSocketClosedException)
        {
            // TODO: Send a message to the client that connection has been dropped.
            Disconnect();
        }
    }
    public void UpdateGameStatus(ArchipelagoClientState state)
    {
        SendPacket(new StatusUpdatePacket { Status = state });
    }

    public string GetPlayerName(int slot)
    {
        if (slot == 0)
        {
            return "Archipelago";
        }

        var name = _session.Players.GetPlayerAlias(slot);
        return string.IsNullOrEmpty(name) ? $"Unknown Player {slot}" : name;
    }

    public string GetLocationName(long location)
    {
        var name = _session.Locations.GetLocationNameFromId(location);
        return string.IsNullOrEmpty(name) ? $"Unknown Location {location}" : name;
    }

    public string GetItemName(long item)
    {
        var name = _session.Items.GetItemName(item);
        return string.IsNullOrEmpty(name) ? $"Unknown Item {item}" : name;
    }

    public void EnableDeathLink()
    {
        _deathLinkService.EnableDeathLink();
    }

    public void DisableDeathLink()
    {
        _deathLinkService.DisableDeathLink();
    }

    public bool IsLocationChecked(long id)
    {
        // Verify location exists first, we'll treat locations that don't exist as already checked.
        return !_session.Locations.AllLocations.Contains(id) || _session.Locations.AllLocationsChecked.Contains(id);
    }

    public int LocationsCheckedCount()
    {
        return _session.Locations.AllLocationsChecked.Count();
    }

    private void SendPacket(ArchipelagoPacketBase packet)
    {
        try
        {
            _session.Socket.SendPacket(packet);
        }
        catch (ArchipelagoSocketClosedException)
        {
            // TODO: Send a message to the client that connection has been dropped.
            Disconnect();
        }
    }

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        var i = helper.Index;
        while (helper.Any())
        {
            ItemQueue.Add(new(i++, helper.DequeueItem()));
        }
    }

    private void OnLocationReceived(ReadOnlyCollection<long> newCheckedLocations)
    {
        foreach (var newLoc in newCheckedLocations)
        {
            CollectedLocations.Add(newLoc);
        }
    }

    private void OnDeathLink(DeathLink deathLink)
    {
        // If we receive a DeathLink that is after our last death, let's set it.
        if (!IsDeathLinkSafe && DateTime.Compare(deathLink.Timestamp, _lastDeath) > 0)
        {
            DeathLinkData = deathLink;
        }
    }

    private async Task BuildLocationDictionary()
    {
        var locations = await _session.Locations.ScoutLocationsAsync(false, _session.Locations.AllLocations.ToArray());

        foreach (var item in locations)
        {
            LocationDictionary[item.Key] = item.Value;
        }
    }

    private void OnMessageReceived(LogMessage message)
    {
        switch (message)
        {
            case ItemSendLogMessage:
                ItemSendLogMessage itemSendMessage = (ItemSendLogMessage)message;

                if (itemSendMessage.IsRelatedToActivePlayer && !itemSendMessage.IsReceiverTheActivePlayer)
                {
                    MessageLog.Add(new ArchipelagoMessage(message.ToString()));
                }
                break;
        }
    }

    private static void OnError(Exception exception, string message)
    {

    }

    public void CheckReceivedItemQueue()
    {
        int audioGuard = 0;
        for (int index = Save.CurrentRecord.GetFlag("ItemRcv"); index < ItemQueue.Count; index++)
        {
            var item = ItemQueue[index].Item2;

            if (audioGuard < 3)
            {
                audioGuard++;
                Audio.Play(Sfx.sfx_secret);
            }

            Log.Info($"Received {ItemIDToString[item.ItemId]} from {GetPlayerName(item.Player)}.");
            MessageLog.Add(new ArchipelagoMessage($"Received {ItemIDToString[item.ItemId]} from {GetPlayerName(item.Player)}."));

            if (item.ItemId == 0xCA0000)
            {
                Save.CurrentRecord.IncFlag("Strawberries");
            }
            else if (item.ItemId == 0xCA0001)
            {
                Save.CurrentRecord.SetFlag("DashRefill");
            }
            else if (item.ItemId == 0xCA0002)
            {
                Save.CurrentRecord.SetFlag("DoubleDashRefill");
            }
            else if (item.ItemId == 0xCA0003)
            {
                Save.CurrentRecord.SetFlag("Feather");
            }
            else if (item.ItemId == 0xCA0004)
            {
                Save.CurrentRecord.SetFlag("Coin");
            }
            else if (item.ItemId == 0xCA0005)
            {
                Save.CurrentRecord.SetFlag("Cassette");
            }
            else if (item.ItemId == 0xCA0006)
            {
                Save.CurrentRecord.SetFlag("TrafficBlock");
            }
            else if (item.ItemId == 0xCA0007)
            {
                Save.CurrentRecord.SetFlag("Spring");
            }
            else if (item.ItemId == 0xCA0008)
            {
                Save.CurrentRecord.SetFlag("Breakables");
            }
            else if (item.ItemId == 0xCA000A)
            {
                Save.CurrentRecord.SetFlag("Grounded Dash");
            }
            else if (item.ItemId == 0xCA000B)
            {
                Save.CurrentRecord.SetFlag("Air Dash");
            }
            else if (item.ItemId == 0xCA000C)
            {
                Save.CurrentRecord.SetFlag("Skid Jump");
            }
            else if (item.ItemId == 0xCA000D)
            {
                Save.CurrentRecord.SetFlag("Climb");
            }
            else if (item.ItemId >= 0xCA0020 && item.ItemId <= 0xCA0029)
            {
                string checkpointStr = ItemIDToString[item.ItemId];
                string internalName = "Item_" + CheckpointAPToInternal[checkpointStr];
                Save.CurrentRecord.SetFlag(internalName);
            }

            Save.CurrentRecord.SetFlag("ItemRcv", index + 1);
        }
    }

    public void CheckLocationsToSend()
    {
        List<long> locationsToCheck = new List<long>();
        foreach (var strawbID in Save.CurrentRecord.Strawberries)
        {
            if (LocationStringToID.ContainsKey(strawbID))
            {
                long locationID = LocationStringToID[strawbID];
                if (!SentLocations.Contains(locationID))
                {
                    locationsToCheck.Add(locationID);
                }
            }
            else
            {
                Log.Info($"Untracked Strawberry: {strawbID}");
            }
        }

        foreach (var nameIDPair in LocationStringToID)
        {
            if (Save.CurrentRecord.Strawberries.Contains(nameIDPair.Key))
            {
                continue;
            }

            if (Save.CurrentRecord.GetFlag(nameIDPair.Key) > 0)
            {
                long locationID = nameIDPair.Value;
                if (!SentLocations.Contains(locationID))
                {
                    locationsToCheck.Add(locationID);
                }
            }
        }

        CheckLocations(locationsToCheck.ToArray());
    }

    public void HandleCollectedLocations()
    {
        // Change this if we need to !collect non-Strawberry locations
        foreach (var newLoc in CollectedLocations)
        {
            if (LocationIDToString.ContainsKey((int)newLoc))
            {
                if (newLoc < 0xCA0400)
                {
                    string strawberryLocID = LocationIDToString[(int)newLoc];
                    if (!Save.CurrentRecord.Strawberries.Contains(strawberryLocID))
                    {
                        Save.CurrentRecord.Strawberries.Add(strawberryLocID);
                    }

                    if (strawberryLocID.Contains("-"))
                    {
                        string subMapID = strawberryLocID.Split("/")[0];
                        if (!Save.CurrentRecord.CompletedSubMaps.Contains(subMapID))
                        {
                            Save.CurrentRecord.CompletedSubMaps.Add(subMapID);
                        }
                    }
                }
                else if (newLoc < 0xCA0500)
                {
                    string locID = LocationIDToString[(int)newLoc];

                    Save.CurrentRecord.SetFlag(locID);
                }
            }
        }
    }

    public void HandleMessageQueue(Batcher batch, SpriteFont font, Rect bounds)
    {
        for (int i = Math.Min(Math.Max(4, MessageLog.Count - 1), 4); i >= 0; i--)
        {
            if (MessageLog.Count > i)
            {
                batch.Text(font, Game.Instance.ArchipelagoManager.MessageLog[i].Text, bounds.BottomLeft, new Vec2(0, 5 - i), new Foster.Framework.Color(0xF5, 0x42, 0xC8, 0xFF));
                ArchipelagoMessage updatedMessage = Game.Instance.ArchipelagoManager.MessageLog[i];
                updatedMessage.RemainingTime -= 1;
                if (updatedMessage.RemainingTime <= 0)
                {
                    Game.Instance.ArchipelagoManager.MessageLog.RemoveAt(i);
                }
                else
                {
                    Game.Instance.ArchipelagoManager.MessageLog[i] = updatedMessage;
                }
            }
        }
    }

    #region Multiplayer
    public Dictionary<string, AltPlayer> OtherPlayers = new Dictionary<string, AltPlayer> { };
    public Dictionary<string, string> otherPlayersData = [];
    public List<string> TrackedPlayerNames = [];
    public OtherPlayerData ourLastSetData;

    private bool listCallbackSet = false;
    public bool addedOurNameToList = false;
    public bool GhostPlayersActive => _connectionInfo.SeeGhosts;

    public struct OtherPlayerData
    {
        public string Name;
        public string Sublevel;
        public Vector2 Facing;
        public Vector3 Position;
        public string HairColor;

        public bool RoughlyEqual(OtherPlayerData otherData)
        {
            if (this.Name == otherData.Name &&
                this.Sublevel == otherData.Sublevel &&
                (Vector2.Distance(this.Facing, otherData.Facing) < 0.1f) &&
                (Vector3.Distance(this.Position, otherData.Position) < 0.1f) &&
                this.HairColor == otherData.HairColor)
            {
                return true;
            }

            return false;
        }
    }

    public void PlayerListUpdated(List<string> otherPlayerNames)
    {
        foreach (string otherPlayerName in otherPlayerNames)
        {
            if (otherPlayerName != GetPlayerName(Slot) && !TrackedPlayerNames.Contains(otherPlayerName))
            {
                TrackedPlayerNames.Add(otherPlayerName);
            }
        }
    }

    public void RequestPlayerData()
    {
        List<string> keys = new List<string>();

        foreach (string name in TrackedPlayerNames)
        {
            keys.Add($"C64_OtherPlayer_{name}");
        }

        GetPacket packet = new GetPacket();
        packet.Keys = keys.ToArray();

        //Task.Run(() => _session.Socket.SendPacketAsync(packet));
        _session.Socket.SendPacketAsync(packet);
    }

    private void OnPacketReceived(ArchipelagoPacketBase packet)
    {
        if (packet.PacketType == ArchipelagoPacketType.Retrieved)
        {
            if (_connectionInfo.SeeGhosts)
            {
                RetrievedPacket retPacket = packet as RetrievedPacket;

                foreach (KeyValuePair<string, JToken> entry in retPacket.Data)
                {
                    if (entry.Key.StartsWith("C64_OtherPlayer_"))
                    {
                        PlayerUpdated(entry.Key, entry.Value);
                    }
                }
            }
        }
    }

    public void PlayerUpdated(string key, JToken otherPlayer)
    {
        otherPlayersData[key] = otherPlayer.ToString();
    }

    public void AddPlayerListCallback(string key, Action<List<string>> callback)
    {
        if (!listCallbackSet)
        {
            listCallbackSet = true;
            //Log.Info($"Adding callback for C64_OtherPlayers_List");
            _session.DataStorage[key].OnValueChanged += (oldData, newData, _) => {
                List<string> otherPlayers = JsonConvert.DeserializeObject<List<string>>(newData.ToString());
                callback(otherPlayers);
            };
        }
    }

    public void AddPlayerDataCallback(string key, Action<string, JToken> callback)
    {
        _session.DataStorage[key].OnValueChanged += (oldData, newData, _) => {
            callback(key, newData);
        };
    }



    public void Set(string key, Vector3 value)
    {
        var token = JToken.FromObject(value);
        _session.DataStorage[key] = token;
    }

    public void Read(string key, out Vector3? outValue)
    {
        try
        {
            var value = _session.DataStorage[key];
            value.Initialize(0);
            outValue = value.To<Vector3>();
        }
        catch (Exception ex)
        {
            outValue = null;
            return;
        }
    }

    public void Set(string key, Vector2 value)
    {
        var token = JToken.FromObject(value);
        _session.DataStorage[key] = token;
    }

    public void Read(string key, out Vector2? outValue)
    {
        try
        {
            var value = _session.DataStorage[key];
            value.Initialize(0);
            outValue = value.To<Vector2>();
        }
        catch (Exception ex)
        {
            outValue = null;
            return;
        }
    }

    public void Set(string key, string value)
    {
        var token = JToken.FromObject(value);
        _session.DataStorage[key] = token;
    }

    public void Read(string key, out string? outValue)
    {
        try
        {
            var value = _session.DataStorage[key];
            value.Initialize("");
            outValue = value.To<string>();
        }
        catch (Exception ex)
        {
            outValue = null;
            return;
        }
    }

    public void Set(string key, OtherPlayerData value)
    {
        var token = JToken.FromObject(value);
        _session.DataStorage[key] = token;
    }

    public async Task<OtherPlayerData?> ReadPlayerDataAsync(string key)
    {
        try
        {
            _session.DataStorage[key].Initialize("");
            var value = await _session.DataStorage[key].GetAsync<OtherPlayerData>();
            return value;
        }
        catch (Exception ex)
        {
            Log.Info($"Error Reading OtherPlayerData from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<string?> ReadStringAsync(string key)
    {
        try
        {
            _session.DataStorage[key].Initialize("");
            var value = await _session.DataStorage[key].GetAsync<string>();
            return value;
        }
        catch (Exception ex)
        {
            Log.Info($"Error Reading string from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<List<string>> ReadListStringAsync(string key)
    {
        try
        {
            _session.DataStorage[key].Initialize(new List<string>());
            var value = await _session.DataStorage[key].GetAsync<List<string>>();
            return value;
        }
        catch (Exception ex)
        {
            Log.Info($"Error Reading List<string> from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<Vector2?> ReadVector2Async(string key)
    {
        try
        {
            _session.DataStorage[key].Initialize(0);
            var value = await _session.DataStorage[key].GetAsync<Vector2>();
            return value;
        }
        catch (Exception ex)
        {
            Log.Info($"Error Reading Vector2 from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<Vector3?> ReadVector3Async(string key)
    {
        try
        {
            _session.DataStorage[key].Initialize(0);
            var value = await _session.DataStorage[key].GetAsync<Vector3>();
            return value;
        }
        catch (Exception ex)
        {
            Log.Info($"Error Reading Vector3 from DataStorage key [{key}].{Environment.NewLine}Message: {ex.Message}{Environment.NewLine}Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    public void AddToList(string key, string value)
    {
        var currentValue = _session.DataStorage[key];
        currentValue.Initialize(new List<string>());

        var currentList = new List<string>();
        if (currentValue is not null)
            currentList = currentValue.To<List<string>>();

        if (!currentList.Contains(value))
            currentList.Add(value);

            var token = JToken.FromObject(currentList);
            _session.DataStorage[key] = token;
    }

    public void Read(string key, out List<string> outValue)
    {
        try
        {
            var value = _session.DataStorage[key];
            value.Initialize(new List<string>());

            outValue = value.To<List<string>>();
        }
        catch (Exception ex)
        {
            outValue = new List<string>();
            return;
        }
    }
    #endregion
}
