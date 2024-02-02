
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

namespace Celeste64;


public record ArchipelagoConnectionInfo
{
    public string Url { get; init; } = "wss://archipelago.gg:38281";
    public string SlotName { get; init; } = "Madeline";
    public string Password { get; init; } = "";

    /// <summary>
    /// Returns the default connection information.
    /// </summary>
    public static readonly ArchipelagoConnectionInfo Default = new();
}

public class ArchipelagoManager
{
    private static readonly Version _supportedArchipelagoVersion = new(0, 4, 3);

    private readonly ArchipelagoConnectionInfo _connectionInfo;
    private ArchipelagoSession? _session;
    private DeathLinkService? _deathLinkService;
    private DateTime _lastDeath;

    public DeathLink? DeathLinkData { get; private set; }
    public bool IsDeathLinkSafe { get; set; }
    public bool Ready { get; private set; }
    public List<Tuple<int, NetworkItem>> ItemQueue { get; private set; } = new();
    public Dictionary<long, NetworkItem> LocationDictionary { get; private set; } = new();
    public List<Tuple<JsonMessageType, JsonMessagePart[]>> ChatLog { get; } = new();

    public bool CanCollect => _session.RoomState.CollectPermissions is Permissions.Goal or Permissions.Enabled;
    public bool CanRelease => _session.RoomState.ReleasePermissions is Permissions.Goal or Permissions.Enabled;
    public bool CanRemaining => _session.RoomState.RemainingPermissions is Permissions.Goal or Permissions.Enabled;
    public string Seed => _session.RoomState.Seed;
    public int Slot => _session.ConnectionInfo.Slot;
    public bool DeathLink => _session.ConnectionInfo.Tags.Contains("DeathLink");
    public int HintPoints => _session.RoomState.HintPoints;
    public int HintCost => _session.RoomState.HintCost;
    public Hint[] Hints => _session.DataStorage.GetHints();


    public int StrawberriesRequired { get; set; }


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

    public void CheckLocations(params long[] locations)
    {
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
            Log.Info("Append Item");
            ItemQueue.Add(new(i++, helper.DequeueItem()));
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
        //// Special handling for PrintJSON messages only, otherwise we log complete details about packet below.
        //if (packet is PrintJsonPacket messagePacket)
        //{
        //    // Treat unknown message types as "Chat" for forwards compat so we don't crash on new ones.
        //    ChatLog.Add(new(messagePacket.MessageType ?? JsonMessageType.Chat, messagePacket.Data));
        //    var message = messagePacket
        //        .Data
        //        .Aggregate(new StringBuilder(), (sb, data) => sb.Append(data.Text))
        //        .ToString();
        //
        //    RandUtil.Console("Archipelago", $"Message Received: {message}");
        //    return;
        //}
        //
        //RandUtil.Console("Archipelago", $"Packet Received: {packet.GetType().Name}");
        //RandUtil.PrintProperties(packet);
    }

    private static void OnError(Exception exception, string message)
    {
        //RandUtil.Console(
        //    "Archipelago",
        //    $"Encountered an unhandled exception in ArchipelagoManager: " +
        //            $"{message}\n\nStack Trace:\n{exception.StackTrace}"
        //);
    }
}
