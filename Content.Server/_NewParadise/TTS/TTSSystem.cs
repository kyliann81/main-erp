﻿using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Server.Light.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.VoiceMask;
using Content.Shared._NewParadise;
using Content.Shared._NewParadise.TTS;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._NewParadise.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IServerNetManager _netMgr = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly TTSPitchRateSystem _ttsPitchRateSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    private const int MaxMessageChars = 100 * 2; // same as SingleBubbleCharLimit * 2
    private bool _isEnabled;
    private string _apiUrl = string.Empty;

    public override void Initialize()
    {
        _cfg.OnValueChanged(NewParadiseCvars.TtsEnabled, v => _isEnabled = v, true);
        _cfg.OnValueChanged(NewParadiseCvars.TtsApiUrl, url => _apiUrl = url, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<SharedTTSComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeLocalEvent<TTSAnnouncementEvent>(OnAnnounceRequest);

        _netMgr.RegisterNetMessage<MsgRequestTTS>(OnRequestTTS);
        _netMgr.RegisterNetMessage<MsgRequestPreviewTTS>(OnRequestPreviewTTS);
    }

    private async void OnRequestPreviewTTS(MsgRequestPreviewTTS message)
    {
        var ttsData = await GenerateTTS(null, message.Text, message.VoiceId);

        if (ttsData == null)
        {
            return;
        }

        var ev = new PlayPreviewTTSEvent(ttsData);

        RaiseNetworkEvent(ev, message.MsgChannel);
    }

    private async void OnAnnounceRequest(TTSAnnouncementEvent ev)
    {
        if (!_isEnabled || string.IsNullOrEmpty(_apiUrl) || ev.Message.Length > MaxMessageChars)
        {
            return;
        }
        
        if (!_prototypeManager.TryIndex<TTSVoicePrototype>(ev.VoiceId, out var ttsPrototype))
            return;

        var message = FormattedMessage.RemoveMarkupPermissive(ev.Message);
        var soundData = await GenerateTTS(null, message, ttsPrototype.Speaker, speechRate: "slow", effect: "announce");

        if (soundData == null)
            return;

        RaiseNetworkEvent(new PlayTTSGlobalEvent(soundData, true));
    }

    private async void OnRequestTTS(MsgRequestTTS ev)
    {
        var url = _cfg.GetCVar(NewParadiseCvars.TtsApiUrl);
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!_playerManager.TryGetSessionByChannel(ev.MsgChannel, out var session) ||
            !_prototypeManager.TryIndex(ev.VoiceId, out var protoVoice))
            return;

        var soundData = await GenerateTTS(GetEntity(ev.Uid), ev.Text, protoVoice.Speaker);
        if (soundData != null)
        {
            RaiseNetworkEvent(new PlayTTSEvent(ev.Uid, soundData, false), Filter.SinglePlayer(session),
                false);
        }
    }

    private async void OnEntitySpoke(EntityUid uid, SharedTTSComponent component, EntitySpokeEvent args)
    {
        if (!_isEnabled || string.IsNullOrEmpty(_apiUrl) || args.Message.Length > MaxMessageChars)
        {
            return;
        }

        var voiceId = component.VoicePrototypeId;

        var voiceEv = new TransformSpeakerVoiceEvent(uid, voiceId);
        RaiseLocalEvent(uid, voiceEv);
        voiceId = voiceEv.VoiceId;

        if (!_prototypeManager.TryIndex(voiceId, out var protoVoice))
            return;

        var message = FormattedMessage.RemoveMarkupPermissive(args.Message);

        var soundData = await GenerateTTS(uid, message, protoVoice.Speaker);
        if (soundData is null)
            return;

        var ttsEvent = new PlayTTSEvent(GetNetEntity(uid), soundData, false);

        // Say
        if (args.ObfuscatedMessage is null)
        {
            RaiseNetworkEvent(ttsEvent, Filter.Pvs(uid), false);
            return;
        }

        // Whisper
        var wList = new List<string>
        {
            "тсс",
            "псс",
            "ччч",
            "ссч",
            "сфч",
            "тст"
        };

        var whisperSoundData = await GenerateTTS(uid, message, protoVoice.Speaker, effect: "pitch_shift");
        if (whisperSoundData is null)
            return;

        var whisperEvent = new PlayTTSEvent(GetNetEntity(uid), whisperSoundData, false, isWhisper: true);

        var chosenWhisperText = _random.Pick(wList);
        var obfSoundData = await GenerateTTS(uid, chosenWhisperText, protoVoice.Speaker);

        if (obfSoundData is null)
            return;

        var obfTtsEvent = new PlayTTSEvent(GetNetEntity(uid), obfSoundData, false);
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;

        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue)
                continue;
            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).LengthSquared();
            if (distance > (ChatSystem.VoiceRange * ChatSystem.VoiceRange))
                continue;

            EntityEventArgs actualEvent = distance > ChatSystem.WhisperClearRange
                ? obfTtsEvent
                : whisperEvent;

            RaiseNetworkEvent(actualEvent, Filter.SinglePlayer(session), false);
        }
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }

    // ReSharper disable once InconsistentNaming
    private async Task<byte[]?> GenerateTTS(EntityUid? uid, string text, string speaker, string? speechPitch = null,
        string? speechRate = null, string? effect = null)
    {
        var textSanitized = Sanitize(text);
        if (textSanitized == "")
            return null;

        string pitch;
        string rate;
        if (speechPitch == null || speechRate == null)
        {
            if (uid == null || !_ttsPitchRateSystem.TryGetPitchRate(uid.Value, out var pitchRate))
            {
                pitch = "medium";
                rate = "medium";
            }
            else
            {
                pitch = pitchRate[0];
                rate = pitchRate[1];
            }
        }
        else
        {
            pitch = speechPitch;
            rate = speechRate;
        }

        return await _ttsManager.ConvertTextToSpeech(speaker, textSanitized, pitch, rate, effect);
    }
}

public sealed class TransformSpeakerVoiceEvent(EntityUid sender, string voiceId) : EntityEventArgs
{
    public EntityUid Sender = sender;
    public ProtoId<TTSVoicePrototype> VoiceId = voiceId;
}
