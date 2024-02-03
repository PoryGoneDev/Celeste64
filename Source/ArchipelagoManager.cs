
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
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Celeste64;



[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(ArchipelagoConnectionInfo))]
public partial class ArchipelagoConnectionInfoContext : JsonSerializerContext { }
public record ArchipelagoConnectionInfo
{
    public string Url { get; init; } = "wss://archipelago.gg:38281";
    public string SlotName { get; init; } = "Madeline";
    public string Password { get; init; } = "";
}

public class ArchipelagoManager
{
    private static readonly Version _supportedArchipelagoVersion = new(0, 4, 3);

    private readonly ArchipelagoConnectionInfo _connectionInfo;
    private ArchipelagoSession? _session;
    private DeathLinkService? _deathLinkService;
    private DateTime _lastDeath;

    public bool GoalSent = false;

    public DeathLink? DeathLinkData { get; private set; }
    public bool IsDeathLinkSafe { get; set; }
    public bool Ready { get; private set; }
    public List<Tuple<int, NetworkItem>> ItemQueue { get; private set; } = new();
    public List<long> CollectedLocations { get; private set; } = new();
    public Dictionary<long, NetworkItem> LocationDictionary { get; private set; } = new();
    public HashSet<long> SentLocations { get; set; } = [];

    public int Slot => _session.ConnectionInfo.Slot;
    public bool DeathLink => _session.ConnectionInfo.Tags.Contains("DeathLink");
    public int HintPoints => _session.RoomState.HintPoints;
    public int HintCost => _session.RoomState.HintCost;
    public Hint[] Hints => _session.DataStorage.GetHints();


    public int StrawberriesRequired { get; set; }

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
        { 0xCA0008, "Breakable Blocks" }
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
        bool DeathLinkEnabled = Convert.ToBoolean(((LoginSuccessful)result).SlotData["death_link"]);

        // Initialize DeathLink service.
        _deathLinkService = _session.CreateDeathLinkService();
        _deathLinkService.OnDeathLinkReceived += OnDeathLink;
        if (DeathLinkEnabled)
        {
            _deathLinkService.EnableDeathLink();
        }

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
    }

    public void SendDeathLinkIfEnabled(string cause)
    {
        // Do not send any DeathLink messages if it's not enabled.
        if (!DeathLink)
        {
            return;
        }

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

        foreach (var item in locations.Locations)
        {
            LocationDictionary[item.Location] = item;
        }
    }

    private void OnPacketReceived(ArchipelagoPacketBase packet)
    {

    }

    private static void OnError(Exception exception, string message)
    {

    }

    public void CheckReceivedItemQueue()
    {
        for (int index = Save.CurrentRecord.GetFlag("ItemRcv"); index < ItemQueue.Count; index++)
        {
            var item = ItemQueue[index].Item2;

            Audio.Play(Sfx.sfx_secret);
            Log.Info($"Received {ItemIDToString[item.Item]} from {GetPlayerName(item.Player)}.");

            if (item.Item == 0xCA0000)
            {
                Save.CurrentRecord.IncFlag("Strawberries");
            }
            else if (item.Item == 0xCA0001)
            {
                Save.CurrentRecord.SetFlag("DashRefill");
            }
            else if (item.Item == 0xCA0002)
            {
                Save.CurrentRecord.SetFlag("DoubleDashRefill");
            }
            else if (item.Item == 0xCA0003)
            {
                Save.CurrentRecord.SetFlag("Feather");
            }
            else if (item.Item == 0xCA0004)
            {
                Save.CurrentRecord.SetFlag("Coin");
            }
            else if (item.Item == 0xCA0005)
            {
                Save.CurrentRecord.SetFlag("Cassette");
            }
            else if (item.Item == 0xCA0006)
            {
                Save.CurrentRecord.SetFlag("TrafficBlock");
            }
            else if (item.Item == 0xCA0007)
            {
                Save.CurrentRecord.SetFlag("Spring");
            }
            else if (item.Item == 0xCA0008)
            {
                Save.CurrentRecord.SetFlag("Breakables");
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

        CheckLocations(locationsToCheck.ToArray());
    }

    public void HandleCollectedLocations()
    {
        foreach (var newLoc in CollectedLocations)
        {
            if (LocationIDToString.ContainsKey((int)newLoc))
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
        }
    }
}
