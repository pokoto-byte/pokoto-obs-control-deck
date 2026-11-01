using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OBSControlDeck
{
    public partial class MainForm : Form
    {
        private const int ClientRpcVersion = 1;
        private const int ScenesEventSubscription = 1 << 2;
        private const int SceneItemsEventSubscription = 1 << 7;
        private const int InputsEventSubscription = 1 << 3;
        private const int OutputsEventSubscription = 1 << 6;
        private const int EventSubscriptions = ScenesEventSubscription | SceneItemsEventSubscription | InputsEventSubscription | OutputsEventSubscription;
        private static readonly Color FunctionVisibleColor = Color.FromArgb(0, 143, 61);
        private static readonly Color FunctionHiddenColor = Color.FromArgb(166, 31, 43);
        private static readonly Color FunctionEmptyColor = Color.FromArgb(48, 54, 65);
        private static readonly Color FunctionEmptyTextColor = Color.LightGray;
        private static readonly Color SaveReplayColor = Color.FromArgb(244, 107, 11);
        private static readonly Color SceneActiveColor = Color.FromArgb(0, 143, 61);
        private static readonly Color SceneInactiveColor = Color.FromArgb(48, 54, 65);
        private static readonly Color AudioUnmutedColor = Color.FromArgb(0, 143, 61);
        private static readonly Color AudioMutedColor = Color.FromArgb(166, 31, 43);
        private static readonly Color AudioEmptyColor = Color.FromArgb(48, 54, 65);
        private static readonly Color AudioEmptyTextColor = Color.LightGray;
        private const string SaveReplayRequestPrefix = "SaveReplay-";
        private const string TriggerSceneHotkeyRequestPrefix = "TriggerSceneHotkey-";
        private static readonly Color SceneHotkeyOnlyColor = Color.FromArgb(35, 136, 232);
        private sealed record SceneHotkeyOp(int Slot, string? SceneName, string HotkeyName);
        private readonly Dictionary<string, SceneHotkeyOp> _sceneHotkeyOps = new();
        private const string ReplayBufferStatusRequestPrefix = "GetReplayBufferStatus-";
        private const string HotkeyListRequestPrefix = "GetHotkeyList-";
        private readonly List<string> _obsHotkeys = new();
        private readonly ToolStripMenuItem _mnuAssignHotkey = new("Assign Hotkey...");
        private readonly ToolStripMenuItem _mnuRemoveHotkey = new("Remove Hotkey");
        private static readonly Color SaveReplaySavedColor = Color.FromArgb(0, 143, 61);
        private static readonly Color SaveReplayErrorColor = Color.FromArgb(166, 31, 43);
        private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);

        private ClientWebSocket? _ws;
        private CancellationTokenSource? _cts;
        private readonly string _configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OBSControlDeck");
        private readonly string _passwordFile;
        private readonly string _profilesDir;
        private readonly string _lastProfileFile;
        private string _activeProfileName = string.Empty;
        private Profile? _activeProfile;
        private bool _loadingProfile;
        private bool _isReplayBufferActive;
        private bool _replayBusy;
        private const string TriggerReplayHotkeyRequestPrefix = "TriggerReplayHotkey-";
        private string? _replayOpHotkey;
        private bool _replayOpSaveConfirmed;
        private string? _replayOpHotkeyRequestId;
        private CancellationTokenSource? _replayFeedbackCts;

        public MainForm()
        {
            _passwordFile = Path.Combine(_configDir, "password.txt");
            InitializeComponent();
            tvObsItems.BeforeCheck += TvObsItems_BeforeCheck;
            tvObsItems.AfterCheck += TvObsItems_AfterCheck;
            _functionButtons = new[]
            {
                btnF1Home, btnF1Visit, btnF2Home, btnF2Visit, btnF3Home,
                btnF3Visit, btnF4Home, btnF4Visit, btnF5Home, btnF5Visit
            };
            foreach (var functionButton in _functionButtons)
            {
                functionButton.Click += FunctionButton_Click;
                functionButton.MouseDown += FunctionButton_MouseDown;
                functionButton.MouseMove += FunctionButton_MouseMove;
            }
            grpFunctionTexts.DragEnter += FunctionGroup_DragOver;
            grpFunctionTexts.DragOver += FunctionGroup_DragOver;
            grpFunctionTexts.DragDrop += FunctionGroup_DragDrop;
            chkUnlockFunctionLayout.Checked = false;
            grpFunctionTexts.AllowDrop = false;
            grpScenes.AllowDrop = false;
            grpReplay.AllowDrop = false;
            foreach (var group in new[] { grpScenes, grpReplay })
            {
                group.DragEnter += SceneGroup_DragOver;
                group.DragOver += SceneGroup_DragOver;
                group.DragDrop += SceneGroup_DragDrop;
            }
            foreach (var sceneSlot in SceneSlots())
            {
                sceneSlot.Button.MouseDown += SceneButton_MouseDown;
                sceneSlot.Button.MouseMove += SceneButton_MouseMove;
            }
            chkUnlockFunctionLayout.CheckedChanged += (_, _) =>
            {
                _dragOrigin = null;
                grpFunctionTexts.AllowDrop = chkUnlockFunctionLayout.Checked;
                grpScenes.AllowDrop = chkUnlockFunctionLayout.Checked;
                grpReplay.AllowDrop = chkUnlockFunctionLayout.Checked;
                RefreshSceneButtons();
                RefreshFunctionButtons();
            };
            mnuRenameButton.Click += mnuRenameButton_Click;
            mnuResetButtonName.Click += mnuResetButtonName_Click;
            foreach (var functionButton in _functionButtons)
            {
                functionButton.Padding = Padding.Empty;
                functionButton.Resize += (s, _) =>
                {
                    var b = (Button)s!;
                    if (_buttonOriginalText.TryGetValue(b, out var original)) ApplyAdaptiveButtonText(b, original);
                };
            }
            btnConnect.Click += BtnConnect_Click;
            btnDiretta.Click += BtnDiretta_Click;
            btnSaveReplay.Click += BtnSaveReplay_Click;
            btnSaveReplay.UseVisualStyleBackColor = false;
            btnSaveReplay.BackColor = SaveReplayColor;
            btnSaveReplay.ForeColor = Color.White;
            UpdateReplayButtonNormalState();
            btnWireless.Click += async (_, _) => await ToggleAudioMuteAsync(cmbWireless);
            foreach (var combo in AudioCombos()) combo.SelectionChangeCommitted += AudioCombo_SelectionChangeCommitted;
            btnPrePartita.Click += async (_, _) => await SwitchSceneAsync(1);
            btnFormazCasa.Click += async (_, _) => await SwitchSceneAsync(3);
            btnFormazOspiti.Click += async (_, _) => await SwitchSceneAsync(4);
            btnIntervallo.Click += async (_, _) => await SwitchSceneAsync(5);
            btnFinePartita.Click += async (_, _) => await SwitchSceneAsync(6);
            btnReplayMain.Click += async (_, _) => await SwitchSceneAsync(7);
            btnReplaySX.Click += async (_, _) => await SwitchSceneAsync(8);
            btnReplayDX.Click += async (_, _) => await SwitchSceneAsync(9);

            _profilesDir = Path.Combine(_configDir, "Profiles");
            _lastProfileFile = Path.Combine(_profilesDir, "lastprofile.txt");

            LoadProfiles();

            Shown += MainForm_Shown;
            txtPassword.TextChanged += TxtPassword_TextChanged;
            foreach (var sceneButton in new[] { btnPrePartita, btnDiretta, btnFormazCasa, btnFormazOspiti, btnIntervallo, btnFinePartita, btnReplayMain, btnReplaySX, btnReplayDX })
            {
                sceneButton.ContextMenuStrip = cmsButtonRename;
            }
            btnSaveReplay.ContextMenuStrip = cmsButtonRename;
            _mnuAssignHotkey.Click += MnuAssignHotkey_Click;
            _mnuRemoveHotkey.Click += MnuRemoveHotkey_Click;
            cmsButtonRename.Items.AddRange(new ToolStripItem[] { _mnuAssignHotkey, _mnuRemoveHotkey });

            // legacy: try to read standalone password file if present and no profile provided it
            try
            {
                if (File.Exists(_passwordFile) && string.IsNullOrEmpty(txtPassword.Text))
                {
                    txtPassword.Text = File.ReadAllText(_passwordFile);
                }
            }
            catch
            {
                // ignore read errors
            }
        }

        private void TxtPassword_TextChanged(object? sender, EventArgs e)
        {
            try
            {
                if (!Directory.Exists(_configDir)) Directory.CreateDirectory(_configDir);
                File.WriteAllText(_passwordFile, txtPassword.Text ?? string.Empty);
                // update active profile password if loaded
                if (_activeProfile != null)
                {
                    _activeProfile.Password = txtPassword.Text ?? string.Empty;
                    SaveActiveProfile();
                }
            }
            catch
            {
                // ignore write errors
            }
        }

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            Shown -= MainForm_Shown;
            EnsureMinimumFormSize();
            RestoreWindowSize();
            CaptureMainResponsiveLayout();
            ApplyMainResponsiveLayout();
            tabControl.SizeChanged += (_, _) => ApplyMainResponsiveLayout();
            if (_activeProfile?.AutoConnect == true && _ws == null) BtnConnect_Click(null, EventArgs.Empty);
        }

        private string WindowSettingsFile => Path.Combine(_configDir, "window.json");

        private sealed class WindowSettings
        {
            public int WindowWidth { get; set; }
            public int WindowHeight { get; set; }
        }

        private void RestoreWindowSize()
        {
            try
            {
                if (!File.Exists(WindowSettingsFile)) return;
                var s = JsonSerializer.Deserialize<WindowSettings>(File.ReadAllText(WindowSettingsFile));
                if (s == null || s.WindowWidth <= 0 || s.WindowHeight <= 0) return;

                var wa = Screen.FromControl(this).WorkingArea;
                var width = Math.Min(Math.Max(s.WindowWidth, MinimumSize.Width), wa.Width);
                var height = Math.Min(Math.Max(s.WindowHeight, MinimumSize.Height), wa.Height);
                if (width != Width || height != Height) Size = new Size(width, height);

                var left = Math.Max(wa.Left, Math.Min(Left, wa.Right - Width));
                var top = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - Height));
                if (left != Left || top != Top) Location = new Point(left, top);
            }
            catch
            {
                // ignore invalid settings
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel || WindowState != FormWindowState.Normal) return;
            try
            {
                if (Width <= 0 || Height <= 0) return;
                if (!Directory.Exists(_configDir)) Directory.CreateDirectory(_configDir);
                var s = new WindowSettings { WindowWidth = Math.Max(Width, MinimumSize.Width), WindowHeight = Math.Max(Height, MinimumSize.Height) };
                File.WriteAllText(WindowSettingsFile, JsonSerializer.Serialize(s));
            }
            catch
            {
                // ignore write errors
            }
        }

        private static readonly double[] MainGroupHeightWeights = [27, 18, 46, 9];
        private sealed class MainGroupGeometry
        {
            public required GroupBox Group { get; init; }
            public Rectangle Bounds { get; init; }
            public required List<List<(Button Button, Rectangle Bounds)>> Rows { get; init; }
        }

        private readonly List<MainGroupGeometry> _mainGroups = new();
        private Size _baseMainClientSize;
        private bool _baseMainLayoutCaptured;
        private bool _isApplyingMainResponsiveLayout;

        private void CaptureMainResponsiveLayout()
        {
            if (_baseMainLayoutCaptured) return;
            var extra = Size - MinimumSize;
            _baseMainClientSize = new Size(
                tabControl.ClientSize.Width - Math.Max(0, extra.Width),
                tabControl.ClientSize.Height - Math.Max(0, extra.Height));

            var tolerance = LogicalToDeviceUnits(4);
            foreach (var group in new[] { grpScenes, grpReplay, grpFunctionTexts, grpAudio })
            {
                var buttons = group.Controls.OfType<Button>().OrderBy(b => b.Top).ThenBy(b => b.Left).ToList();
                var rows = new List<List<(Button, Rectangle)>>();
                var rowTop = int.MinValue;
                foreach (var b in buttons)
                {
                    if (rows.Count == 0 || b.Top - rowTop > tolerance)
                    {
                        rows.Add(new List<(Button, Rectangle)>());
                        rowTop = b.Top;
                    }
                    rows[^1].Add((b, b.Bounds));
                }
                foreach (var row in rows) row.Sort((x, y) => x.Item2.Left.CompareTo(y.Item2.Left));
                _mainGroups.Add(new MainGroupGeometry { Group = group, Bounds = group.Bounds, Rows = rows });
            }
            _baseMainLayoutCaptured = true;
        }

        private void ApplyMainResponsiveLayout()
        {
            if (!_baseMainLayoutCaptured || _isApplyingMainResponsiveLayout) return;
            _isApplyingMainResponsiveLayout = true;
            tabControl.SuspendLayout();
            foreach (var g in _mainGroups) g.Group.SuspendLayout();
            try
            {
                var deltaW = Math.Max(0, tabControl.ClientSize.Width - _baseMainClientSize.Width);
                var deltaH = Math.Max(0, tabControl.ClientSize.Height - _baseMainClientSize.Height);
                var groupExtra = DistributePixels(deltaH, MainGroupHeightWeights);
                var offsetTop = 0;

                for (var gi = 0; gi < _mainGroups.Count; gi++)
                {
                    var geo = _mainGroups[gi];
                    var extraH = groupExtra[gi];
                    geo.Group.SetBounds(geo.Bounds.Left, geo.Bounds.Top + offsetTop, geo.Bounds.Width + deltaW, geo.Bounds.Height + extraH);
                    offsetTop += extraH;

                    var rowExtra = DistributePixels(extraH, geo.Rows.Select(r => (double)r.Max(b => b.Bounds.Height)).ToArray());
                    var yOffset = 0;
                    for (var ri = 0; ri < geo.Rows.Count; ri++)
                    {
                        var row = geo.Rows[ri];
                        var colExtra = DistributePixels(deltaW, row.Select(b => (double)b.Bounds.Width).ToArray());
                        var xOffset = 0;
                        for (var ci = 0; ci < row.Count; ci++)
                        {
                            var (button, b) = row[ci];
                            button.SetBounds(b.Left + xOffset, b.Top + yOffset, b.Width + colExtra[ci], b.Height + rowExtra[ri]);
                            xOffset += colExtra[ci];
                        }
                        yOffset += rowExtra[ri];
                    }
                }
            }
            finally
            {
                foreach (var g in _mainGroups) g.Group.ResumeLayout(false);
                tabControl.ResumeLayout(false);
                _isApplyingMainResponsiveLayout = false;
            }
        }

        private static int[] DistributePixels(int total, double[] weights)
        {
            var result = new int[weights.Length];
            var sum = weights.Sum();
            if (weights.Length == 0) return result;
            double cumulative = 0;
            var previousEdge = 0;
            for (var i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                var edge = i == weights.Length - 1 || sum <= 0 ? total : (int)Math.Round(total * cumulative / sum);
                result[i] = edge - previousEdge;
                previousEdge = edge;
            }
            return result;
        }

        private void EnsureMinimumFormSize()
        {
            var margin = LogicalToDeviceUnits(4);
            var display = tabMain.DisplayRectangle;
            var extraW = tabMain.Width - display.Width;
            var extraH = tabMain.Height - display.Height;
            var needTabW = 0;
            var needTabH = 0;
            foreach (TabPage page in tabMain.TabPages)
            {
                foreach (Control c in page.Controls)
                {
                    needTabW = Math.Max(needTabW, c.Right);
                    needTabH = Math.Max(needTabH, c.Bottom);
                }
            }
            needTabW += extraW + margin;
            needTabH += extraH + margin;

            var growW = Math.Max(0, needTabW - tabMain.Width);
            var growH = Math.Max(0, needTabH - tabMain.Height);
            var needClientW = Math.Max(ClientSize.Width + growW, lblObsStatus.Right + margin);
            var needClientH = ClientSize.Height + growH;
            var nonClient = Size - ClientSize;

            var wa = Screen.FromControl(this).WorkingArea;
            var minW = Math.Min(Math.Max(MinimumSize.Width, needClientW + nonClient.Width), wa.Width);
            var minH = Math.Min(Math.Max(MinimumSize.Height, needClientH + nonClient.Height), wa.Height);
            MinimumSize = new Size(minW, minH);

            var width = Math.Min(Math.Max(Width, minW), wa.Width);
            var height = Math.Min(Math.Max(Height, minH), wa.Height);
            if (width != Width || height != Height) Size = new Size(width, height);

            var left = Math.Max(wa.Left, Math.Min(Left, wa.Right - Width));
            var top = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - Height));
            if (left != Left || top != Top) Location = new Point(left, top);
        }

        private async void BtnConnect_Click(object? sender, EventArgs e)
        {
            if (_ws != null)
            {
                await DisconnectAsync();
                return;
            }

            var host = txtHost.Text.Trim();
            if (host.Length == 0 || !int.TryParse(txtPort.Text.Trim(), out var port) || port < 1 || port > 65535)
            {
                ShowError("Host o porta non validi.");
                return;
            }

            btnConnect.Enabled = false;
            btnConnect.Text = "CONNESSIONE...";

            var cts = new CancellationTokenSource(HandshakeTimeout);
            try
            {
                var ws = await ConnectAndIdentifyAsync(new Uri($"ws://{host}:{port}"), txtPassword.Text, cts.Token);

                // handshake completato: il timeout non serve più
                cts.CancelAfter(Timeout.InfiniteTimeSpan);
                _ws = ws;
                _cts = cts;
                UpdateConnectedUi();
                _ = MonitorAsync(ws, cts.Token);
            }
            catch (Exception ex)
            {
                var timedOut = cts.IsCancellationRequested;
                cts.Dispose();
                UpdateDisconnectedUi();
                ShowError(timedOut ? "Timeout: OBS non ha risposto entro 10 secondi." : DescribeError(ex));
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        private async Task DisconnectAsync()
        {
            var ws = _ws;
            var cts = _cts;
            _ws = null;
            _cts = null;

            if (ws != null)
            {
                try
                {
                    using var closeTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnect", closeTimeout.Token);
                }
                catch
                {
                    // chiusura best-effort
                }
                ws.Dispose();
            }

            cts?.Cancel();
            UpdateDisconnectedUi();
        }

        // Hello (op 0) -> Identify (op 1) -> Identified (op 2)
        private static async Task<ClientWebSocket> ConnectAndIdentifyAsync(Uri uri, string password, CancellationToken ct)
        {
            var ws = new ClientWebSocket();
            try
            {
                await ws.ConnectAsync(uri, ct);

                var hello = await ReceiveJsonAsync(ws, ct);
                if (GetOp(hello) != 0)
                {
                    throw new InvalidDataException("Risposta inattesa da OBS: messaggio Hello non ricevuto.");
                }

                var helloData = hello["d"];
                var serverRpc = helloData?["rpcVersion"]?.GetValue<int>() ?? ClientRpcVersion;

                var identifyData = new JsonObject
                {
                    ["rpcVersion"] = Math.Min(ClientRpcVersion, serverRpc),
                    ["eventSubscriptions"] = EventSubscriptions
                };

                var auth = helloData?["authentication"];
                if (auth != null)
                {
                    if (string.IsNullOrEmpty(password))
                    {
                        throw new InvalidOperationException("OBS richiede una password: inseriscila nella scheda SETTINGS.");
                    }

                    var challenge = auth["challenge"]?.GetValue<string>() ?? string.Empty;
                    var salt = auth["salt"]?.GetValue<string>() ?? string.Empty;
                    identifyData["authentication"] = ComputeAuthentication(password, salt, challenge);
                }

                var identify = new JsonObject { ["op"] = 1, ["d"] = identifyData };
                await ws.SendAsync(Encoding.UTF8.GetBytes(identify.ToJsonString()), WebSocketMessageType.Text, true, ct);

                var identified = await ReceiveJsonAsync(ws, ct);
                if (GetOp(identified) != 2)
                {
                    throw new InvalidDataException("Risposta inattesa da OBS: messaggio Identified non ricevuto.");
                }

                return ws;
            }
            catch
            {
                ws.Dispose();
                throw;
            }
        }

        // base64(sha256(base64(sha256(password + salt)) + challenge))
        private static string ComputeAuthentication(string password, string salt, string challenge)
        {
            var secret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret + challenge)));
        }

        private static int GetOp(JsonNode message) => message["op"]?.GetValue<int>() ?? -1;

        private static async Task<JsonNode> ReceiveJsonAsync(ClientWebSocket ws, CancellationToken ct)
        {
            var text = await ReceiveTextAsync(ws, ct);
            return JsonNode.Parse(text) ?? throw new InvalidDataException("Messaggio JSON vuoto ricevuto da OBS.");
        }

        // Riceve un messaggio completo, anche se frammentato in più frame.
        private static async Task<string> ReceiveTextAsync(ClientWebSocket ws, CancellationToken ct)
        {
            var buffer = new byte[4096];
            using var ms = new MemoryStream();
            ValueWebSocketReceiveResult result;
            do
            {
                result = await ws.ReceiveAsync(buffer.AsMemory(), ct);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new IOException(DescribeClose(ws));
                }
                ms.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        }

        // Dopo Identified: legge i messaggi per rilevare la chiusura della connessione.
        private async Task MonitorAsync(ClientWebSocket ws, CancellationToken ct)
        {
            const string sceneListRequestId = "GetSceneList-1";
            try
            {
                var request = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "GetSceneList",
                        ["requestId"] = sceneListRequestId
                    }
                };
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, ct);

                var programSceneRequest = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "GetCurrentProgramScene",
                        ["requestId"] = CurrentProgramSceneRequestId
                    }
                };
                await ws.SendAsync(Encoding.UTF8.GetBytes(programSceneRequest.ToJsonString()), WebSocketMessageType.Text, true, ct);

                var replayStatusRequest = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "GetReplayBufferStatus",
                        ["requestId"] = ReplayBufferStatusRequestPrefix + "1"
                    }
                };
                await ws.SendAsync(Encoding.UTF8.GetBytes(replayStatusRequest.ToJsonString()), WebSocketMessageType.Text, true, ct);

                await RequestInputListAsync(ws, ct);

                var hotkeyListRequest = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "GetHotkeyList",
                        ["requestId"] = HotkeyListRequestPrefix + "1"
                    }
                };
                await ws.SendAsync(Encoding.UTF8.GetBytes(hotkeyListRequest.ToJsonString()), WebSocketMessageType.Text, true, ct);

                while (true)
                {
                    var text = await ReceiveTextAsync(ws, ct);
                    try
                    {
                        var msg = JsonNode.Parse(text);
                        if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() == sceneListRequestId)
                        {
                            // do not await: schedule async handling so MonitorAsync keeps receiving
                            _ = HandleSceneListAsync(msg["d"]!, ws, ct);
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() == CurrentProgramSceneRequestId)
                        {
                            SetCurrentProgramScene(msg["d"]?["responseData"]?["currentProgramSceneName"]?.GetValue<string>());
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() == InputListRequestId)
                        {
                            HandleInputListResponse(msg["d"]!, ws, ct);
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string audioRid && audioRid.StartsWith(AudioRequestPrefix))
                        {
                            HandleAudioResponse(audioRid, msg["d"]!, ws, ct);
                        }
                        else if (msg != null && GetOp(msg) == 5 && msg["d"]?["eventType"]?.GetValue<string>() == "InputMuteStateChanged")
                        {
                            var muteEvent = msg["d"]?["eventData"];
                            var muteInputName = muteEvent?["inputName"]?.GetValue<string>();
                            var muteInputMuted = muteEvent?["inputMuted"]?.GetValue<bool>();
                            if (muteInputName != null && muteInputMuted != null && _audioInputs.Contains(muteInputName))
                            {
                                _audioMuteStates[muteInputName] = muteInputMuted.Value;
                                RefreshAudioButtons();
                            }
                        }
                        else if (msg != null && GetOp(msg) == 5 && msg["d"]?["eventType"]?.GetValue<string>() == "SceneItemEnableStateChanged")
                        {
                            var eventData = msg["d"]?["eventData"];
                            var evSceneName = eventData?["sceneName"]?.GetValue<string>();
                            var evSceneItemId = eventData?["sceneItemId"]?.GetValue<int>();
                            var evEnabled = eventData?["sceneItemEnabled"]?.GetValue<bool>();
                            if (evSceneName != null && evSceneItemId != null && evEnabled != null)
                            {
                                ApplyConfirmedStateToButtons(new FunctionSlot(evSceneName, string.Empty, evSceneItemId), evEnabled.Value);
                            }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string hkRid && hkRid.StartsWith(HotkeyListRequestPrefix))
                        {
                            _obsHotkeys.Clear();
                            if (msg["d"]?["responseData"]?["hotkeys"] is JsonArray hotkeys)
                            {
                                foreach (var h in hotkeys)
                                {
                                    if (h?.GetValue<string>() is string hn && hn.Length > 0) _obsHotkeys.Add(hn);
                                }
                            }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string rbStatusRid && rbStatusRid.StartsWith(ReplayBufferStatusRequestPrefix))
                        {
                            _isReplayBufferActive = msg["d"]?["responseData"]?["outputActive"]?.GetValue<bool>() ?? false;
                            UpdateReplayButtonNormalState();
                        }
                        else if (msg != null && GetOp(msg) == 5 && msg["d"]?["eventType"]?.GetValue<string>() == "ReplayBufferStateChanged")
                        {
                            _isReplayBufferActive = msg["d"]?["eventData"]?["outputActive"]?.GetValue<bool>() ?? false;
                            UpdateReplayButtonNormalState();
                        }
                        else if (msg != null && GetOp(msg) == 5 && msg["d"]?["eventType"]?.GetValue<string>() == "ReplayBufferSaved")
                        {
                            if (_replayBusy && !_replayOpSaveConfirmed)
                            {
                                _replayOpSaveConfirmed = true;
                                var hotkeyName = _replayOpHotkey;
                                if (!string.IsNullOrEmpty(hotkeyName) && hotkeyName != "NONE" && _obsHotkeys.Contains(hotkeyName))
                                {
                                    _replayOpHotkeyRequestId = TriggerReplayHotkeyRequestPrefix + Guid.NewGuid().ToString("N");
                                    _ = TriggerReplayHotkeyAsync(ws, hotkeyName, _replayOpHotkeyRequestId);
                                }
                                else
                                {
                                    _ = ShowReplayFeedbackAsync("SAVED", SaveReplaySavedColor);
                                }
                            }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string thkRid && thkRid.StartsWith(TriggerReplayHotkeyRequestPrefix))
                        {
                            if (_replayBusy && thkRid == _replayOpHotkeyRequestId)
                            {
                                _replayOpHotkeyRequestId = null;
                                var ok = msg["d"]?["requestStatus"]?["result"]?.GetValue<bool>() == true;
                                _ = ok ? ShowReplayFeedbackAsync("SAVED", SaveReplaySavedColor) : ShowReplayFeedbackAsync("HOTKEY ERROR", SaveReplayErrorColor);
                            }
                        }
                        else if (msg != null && GetOp(msg) == 5 && msg["d"]?["eventType"]?.GetValue<string>() == "CurrentProgramSceneChanged")
                        {
                            SetCurrentProgramScene(msg["d"]?["eventData"]?["sceneName"]?.GetValue<string>());
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string rid2 && rid2.StartsWith("SceneItems-"))
                        {
                            // scene items response
                            try
                            {
                                _ = HandleSceneItemsAsync(rid2, msg["d"]!, ct);
                            }
                            catch { }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string sid2 && sid2.StartsWith(FunctionStateRequestPrefix))
                        {
                            HandleFunctionStateResponse(sid2, msg["d"]!);
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string fid && fid.StartsWith(FunctionRequestPrefix))
                        {
                            HandleFunctionResponse(fid, msg["d"]!);
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string svid && svid.StartsWith(SaveReplayRequestPrefix))
                        {
                            if (msg["d"]?["requestStatus"]?["result"]?.GetValue<bool>() != true && _replayBusy)
                            {
                                _replayOpSaveConfirmed = true;
                                _ = ShowReplayFeedbackAsync("ERROR", SaveReplayErrorColor);
                            }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string shkRid && shkRid.StartsWith(TriggerSceneHotkeyRequestPrefix))
                        {
                            HandleSceneHotkeyResponse(shkRid, msg["d"]!, ws);
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string sid && sid.StartsWith(SetSceneItemEnabledPrefix))
                        {
                            if (msg["d"]?["requestStatus"]?["result"]?.GetValue<bool>() != true)
                            {
                                var comment = msg["d"]?["requestStatus"]?["comment"]?.GetValue<string>();
                                ShowError("OBS non ha modificato la visibilità della Source." + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
                            }
                        }
                        else if (msg != null && GetOp(msg) == 7 && msg["d"]?["requestId"]?.GetValue<string>() is string rid && rid.StartsWith("SetScene-"))
                        {
                            if (msg["d"]?["requestStatus"]?["result"]?.GetValue<bool>() != true)
                            {
                                var comment = msg["d"]?["requestStatus"]?["comment"]?.GetValue<string>();
                                ShowError("OBS non ha cambiato scena." + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
                            }
                        }
                    }
                    catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException)
                    {
                        ShowError("Risposta scene non valida da OBS.");
                    }
                }
            }
            catch
            {
                // chiusura da server, errore di rete o disconnessione manuale
            }

            if (ReferenceEquals(_ws, ws))
            {
                _ws = null;
                _cts = null;
                ws.Dispose();
                UpdateDisconnectedUi();
            }
        }

        private void HandleSceneList(JsonNode data)
        {
            if (data["requestStatus"]?["result"]?.GetValue<bool>() != true)
            {
                var comment = data["requestStatus"]?["comment"]?.GetValue<string>();
                ShowError("OBS non ha restituito l'elenco scene." + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
                return;
            }

            // legacy synchronous handler replaced by async version
            // kept for compatibility but delegate to new async implementation
            _ = HandleSceneListAsync(data, null, CancellationToken.None);
        }

        private async Task HandleSceneListAsync(JsonNode data, ClientWebSocket? ws, CancellationToken ct)
        {
            var scenes = data["responseData"]?["scenes"] as JsonArray;
            var sceneNames = new List<string>();
            if (scenes != null)
            {
                foreach (var scene in scenes)
                {
                    var name = scene?["sceneName"]?.GetValue<string>();
                    if (name != null) sceneNames.Add(name);
                }
            }

            // populate hidden lstScenes for compatibility
            lstScenes.BeginUpdate();
            lstScenes.Items.Clear();
            foreach (var n in sceneNames) lstScenes.Items.Add(n);
            lstScenes.EndUpdate();

            if (sceneNames.Count > 0 && _activeProfile != null)
            {
                _activeProfile.SelectedScenes ??= new List<string>();
                var removed = _activeProfile.SelectedScenes.RemoveAll(n => !sceneNames.Contains(n));
                var changed = SyncSceneSlotOrder();
                if (removed > 0 || changed) SaveProfileToFile(_activeProfile);
            }

            RefreshSceneButtons();

            // build tree view
            tvObsItems.BeginUpdate();
            tvObsItems.Nodes.Clear();

            if (ws == null) ws = _ws;
            if (ws == null || ws.State != WebSocketState.Open)
            {
                tvObsItems.EndUpdate();
                return;
            }

            var counter = 0;
            foreach (var sceneName in sceneNames)
            {
                var node = new TreeNode(sceneName) { Tag = new { SceneName = sceneName } };
                tvObsItems.Nodes.Add(node);
                RestoreSceneNodeCheckState(node);

                // request scene items
                var reqId = "SceneItems-" + (++counter) + "-" + Guid.NewGuid().ToString("N");
                var request = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "GetSceneItemList",
                        ["requestId"] = reqId,
                        ["requestData"] = new JsonObject { ["sceneName"] = sceneName }
                    }
                };

                try
                {
                    var json = request.ToJsonString();

                    // register mapping requestId -> sceneName for later correlation
                    try
                    {
                        lock (_sceneItemsRequestMap)
                        {
                            _sceneItemsRequestMap[reqId] = sceneName;
                        }
                    }
                    catch { }

                    await ws.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, ct);
                }
                catch
                {
                    // on error remove mapping
                    try { lock (_sceneItemsRequestMap) { _sceneItemsRequestMap.Remove(reqId); } } catch { }
                }
            }

            tvObsItems.EndUpdate();
        }

        private async Task HandleSceneItemsAsync(string requestId, JsonNode data, CancellationToken ct)
        {
            // retrieve sceneName from the registered mapping (response does not include it)
            string? sceneName = null;
            try
            {
                lock (_sceneItemsRequestMap)
                {
                    if (_sceneItemsRequestMap.TryGetValue(requestId, out var s))
                    {
                        sceneName = s;
                        _sceneItemsRequestMap.Remove(requestId);
                    }
                }
            }
            catch { }

            if (sceneName == null) return;

            // find node (on UI thread)
            TreeNode? sceneNode = null;
            Invoke(() =>
            {
                foreach (TreeNode n in tvObsItems.Nodes)
                {
                    if ((n.Tag as dynamic)?.SceneName == sceneName) { sceneNode = n; break; }
                }
            });

            if (sceneNode == null)
            {
                return;
            }

            // parse items
            var items = data["responseData"]?["sceneItems"] as JsonArray;
            if (items == null)
            {
                return;
            }

            // add child nodes on UI thread and log each add
            Invoke(() =>
            {
                tvObsItems.BeginUpdate();
                try
                {
                    foreach (var it in items)
                    {
                        var sceneItemId = it?["sceneItemId"]?.GetValue<int?>();
                        var sourceName = it?["sourceName"]?.GetValue<string>();
                        var enabled = it?["sceneItemEnabled"]?.GetValue<bool?>();
                        if (sourceName == null) continue;
                        var child = new TreeNode(sourceName) { Tag = new { SceneName = sceneName, SourceName = sourceName, SceneItemId = sceneItemId, SceneItemEnabled = enabled } };
                        sceneNode.Nodes.Add(child);
                    }
                }
                finally
                {
                    tvObsItems.EndUpdate();
                }
                RestoreSceneCheckStates(sceneNode);
                RefreshFunctionButtons();
            });
        }

        private const int MaxSourcesPerScene = 10;
        private const int MaxSelectedScenes = 9;
        private bool _suppressCheckEvents;

        private void RestoreSceneNodeCheckState(TreeNode sceneNode)
        {
            var wanted = _activeProfile?.SelectedScenes?.Contains(sceneNode.Text) == true;
            _suppressCheckEvents = true;
            try { sceneNode.Checked = wanted; }
            finally { _suppressCheckEvents = false; }
        }

        private void TvObsItems_BeforeCheck(object? sender, TreeViewCancelEventArgs e)
        {
            if (_suppressCheckEvents) return;
            var parent = e.Node?.Parent;
            if (parent == null)
            {
                if (!e.Node!.Checked && tvObsItems.Nodes.Cast<TreeNode>().Count(n => n.Checked) >= MaxSelectedScenes)
                {
                    e.Cancel = true;
                    MessageBox.Show(this, "È possibile selezionare al massimo 9 Scene.", "Limite Scene", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            if (!e.Node!.Checked && parent.Nodes.Cast<TreeNode>().Count(n => n.Checked) >= MaxSourcesPerScene)
            {
                e.Cancel = true;
                MessageBox.Show(this, "Per ogni scena possono essere selezionate al massimo 10 Sources.", "Limite Sources", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TvObsItems_AfterCheck(object? sender, TreeViewEventArgs e)
        {
            if (_suppressCheckEvents || e.Node == null) return;
            if (e.Node.Parent == null)
            {
                if (_activeProfile == null) return;
                _activeProfile.SelectedScenes ??= new List<string>();
                if (e.Node.Checked)
                {
                    if (!_activeProfile.SelectedScenes.Contains(e.Node.Text)) _activeProfile.SelectedScenes.Add(e.Node.Text);
                }
                else
                {
                    _activeProfile.SelectedScenes.Remove(e.Node.Text);
                }
                SyncSceneSlotOrder();
                SaveActiveProfile();
                RefreshSceneButtons();
                RefreshFunctionButtons();
                return;
            }
            StoreSceneSelection(e.Node.Parent);
            if (_activeProfile != null && _activeProfile.FunctionSlotOrder.TryGetValue(e.Node.Parent.Text, out var savedOrder))
            {
                var checkedNames = e.Node.Parent.Nodes.Cast<TreeNode>().Where(n => n.Checked).Select(n => n.Text).ToHashSet();
                for (var i = 0; i < savedOrder.Count; i++)
                {
                    if (savedOrder[i] != null && !checkedNames.Contains(savedOrder[i]!)) savedOrder[i] = null;
                }
            }
            SaveActiveProfile();
            RefreshFunctionButtons();
        }

        private void StoreSceneSelection(TreeNode sceneNode)
        {
            if (_activeProfile == null) return;
            var selected = sceneNode.Nodes.Cast<TreeNode>().Where(n => n.Checked).Select(n => n.Text).ToList();
            if (selected.Count == 0) _activeProfile.SelectedSources.Remove(sceneNode.Text);
            else _activeProfile.SelectedSources[sceneNode.Text] = selected;
        }

        private void RestoreSceneCheckStates(TreeNode sceneNode)
        {
            List<string>? wanted = null;
            if (_activeProfile != null && _activeProfile.SelectedSources.TryGetValue(sceneNode.Text, out var list)) wanted = list;

            _suppressCheckEvents = true;
            try
            {
                var count = 0;
                foreach (TreeNode child in sceneNode.Nodes)
                {
                    var check = wanted != null && count < MaxSourcesPerScene && wanted.Contains(child.Text);
                    if (check) count++;
                    child.Checked = check;
                }
            }
            finally
            {
                _suppressCheckEvents = false;
            }
        }

        private void RestoreAllSourceCheckStates()
        {
            foreach (TreeNode sceneNode in tvObsItems.Nodes)
            {
                RestoreSceneNodeCheckState(sceneNode);
                RestoreSceneCheckStates(sceneNode);
            }
        }

        private sealed record FunctionSlot(string SceneName, string SourceName, int? SceneItemId);

        private const string FunctionRequestPrefix = "GetSceneItemEnabled-";
        private const string FunctionStateRequestPrefix = "GetSceneItemEnabledState-";
        private const string SetSceneItemEnabledPrefix = "SetSceneItemEnabled-";
        private int _functionRequestCounter;
        private readonly Dictionary<string, FunctionSlot> _pendingFunctionRequests = new();

        private async void FunctionButton_Click(object? sender, EventArgs e)
        {
            if (chkUnlockFunctionLayout.Checked) return;
            if (sender is Button button) await ToggleFunctionSourceAsync(button);
        }

        private Point? _dragOrigin;

        private int HitTestSceneSlot(int screenX, int screenY)
        {
            var point = new Point(screenX, screenY);
            var slots = SceneSlots();
            for (var i = 0; i < slots.Length; i++)
            {
                var b = slots[i].Button;
                if (b.RectangleToScreen(b.ClientRectangle).Contains(point)) return i;
            }
            return -1;
        }

        private int SceneSlotIndexOf(Button button) => Array.FindIndex(SceneSlots(), s => s.Button == button);

        private void SceneButton_MouseDown(object? sender, MouseEventArgs e)
        {
            _dragOrigin = e.Button == MouseButtons.Left && chkUnlockFunctionLayout.Checked ? e.Location : null;
        }

        private void SceneButton_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragOrigin is not { } origin || sender is not Button button) return;
            if (!chkUnlockFunctionLayout.Checked || e.Button != MouseButtons.Left) return;
            var index = SceneSlotIndexOf(button);
            if (index < 0 || GetSceneSlotData(index + 1) == null) return;

            var size = SystemInformation.DragSize;
            if (Math.Abs(e.X - origin.X) < size.Width / 2 && Math.Abs(e.Y - origin.Y) < size.Height / 2) return;

            _dragOrigin = null;
            button.DoDragDrop(button, DragDropEffects.Move);
        }

        private void SceneGroup_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = chkUnlockFunctionLayout.Checked
                && e.Data?.GetData(typeof(Button)) is Button src && SceneSlotIndexOf(src) >= 0
                && HitTestSceneSlot(e.X, e.Y) >= 0
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void SceneGroup_DragDrop(object? sender, DragEventArgs e)
        {
            _dragOrigin = null;
            if (!chkUnlockFunctionLayout.Checked || e.Data?.GetData(typeof(Button)) is not Button source) return;
            MoveOrSwapSceneSlots(SceneSlotIndexOf(source), HitTestSceneSlot(e.X, e.Y));
        }

        private void MoveOrSwapSceneSlots(int from, int to)
        {
            var p = _activeProfile;
            if (p == null || from < 0 || to < 0 || from >= SceneSlotCount || to >= SceneSlotCount || from == to) return;
            if (p.SceneButtonSlots == null || p.SceneButtonSlots.Count != SceneSlotCount) return;
            if (p.SceneButtonSlots[from] == null) return;

            var slots = new List<SceneButtonSlot?>(p.SceneButtonSlots);
            (slots[from], slots[to]) = (slots[to], slots[from]);
            p.SceneButtonSlots = slots;
            p.SceneSlotOrder = slots.Select(s => s?.SceneName).ToList();
            SaveActiveProfile();
            RefreshSceneButtons();
        }

        private string GetSceneDisplayText(string sceneName) =>
            _activeProfile != null && _activeProfile.SceneAliases.TryGetValue(sceneName, out var alias) && !string.IsNullOrWhiteSpace(alias) ? alias : sceneName;

        private bool MigrateLegacySceneAliases(Profile p)
        {
            if (p.SceneAliasesMigrated) return false;
            p.SceneAliasesMigrated = true;
            p.SceneAliases ??= new Dictionary<string, string>();
            var generic = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var s = 1; s <= SceneSlotCount; s++) generic.Add(GetSceneDefaultText(s));
            for (var s = 1; s <= SceneSlotCount && p.SceneSlotOrder != null && s <= p.SceneSlotOrder.Count; s++)
            {
                var scene = p.SceneSlotOrder[s - 1];
                var legacy = GetSceneButtonText(p, s);
                if (scene == null || string.IsNullOrWhiteSpace(legacy) || generic.Contains(legacy.Trim())) continue;
                if (!p.SceneAliases.ContainsKey(scene)) p.SceneAliases[scene] = legacy.Trim();
            }
            return true;
        }

        private void FunctionButton_MouseDown(object? sender, MouseEventArgs e)
        {
            _dragOrigin = e.Button == MouseButtons.Left && chkUnlockFunctionLayout.Checked ? e.Location : null;
        }

        private void FunctionButton_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_dragOrigin is not { } origin || sender is not Button button) return;
            if (!chkUnlockFunctionLayout.Checked || e.Button != MouseButtons.Left || button.Tag is not FunctionSlot) return;

            var size = SystemInformation.DragSize;
            if (Math.Abs(e.X - origin.X) < size.Width / 2 && Math.Abs(e.Y - origin.Y) < size.Height / 2) return;

            _dragOrigin = null;
            button.DoDragDrop(button, DragDropEffects.Move);
        }

        private int HitTestFunctionSlot(int screenX, int screenY)
        {
            var client = grpFunctionTexts.PointToClient(new Point(screenX, screenY));
            for (var i = 0; i < _functionButtons.Length; i++)
            {
                var b = _functionButtons[i];
                var topLeft = grpFunctionTexts.PointToClient(b.Parent!.PointToScreen(b.Location));
                if (new Rectangle(topLeft, b.Size).Contains(client)) return i;
            }
            return -1;
        }

        private void FunctionGroup_DragOver(object? sender, DragEventArgs e)
        {
            e.Effect = chkUnlockFunctionLayout.Checked
                && e.Data?.GetData(typeof(Button)) is Button src && Array.IndexOf(_functionButtons, src) >= 0
                && HitTestFunctionSlot(e.X, e.Y) >= 0
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void FunctionGroup_DragDrop(object? sender, DragEventArgs e)
        {
            _dragOrigin = null;
            if (!chkUnlockFunctionLayout.Checked || e.Data?.GetData(typeof(Button)) is not Button source) return;
            MoveOrSwapFunctionSlots(Array.IndexOf(_functionButtons, source), HitTestFunctionSlot(e.X, e.Y));
        }

        private void MoveOrSwapFunctionSlots(int from, int to)
        {
            if (_activeProfile == null || string.IsNullOrEmpty(_currentProgramSceneName)) return;
            if (from < 0 || to < 0 || from == to) return;

            var order = _functionButtons.Select(b => (b.Tag as FunctionSlot)?.SourceName).ToList();
            (order[from], order[to]) = (order[to], order[from]);
            _activeProfile.FunctionSlotOrder[_currentProgramSceneName] = order;
            SaveActiveProfile();
            RefreshFunctionButtons();
        }

        private List<string?> BuildFunctionSlotNames(string sceneName, List<string> selected)
        {
            var result = new List<string?>(new string?[_functionButtons.Length]);
            if (_activeProfile != null && _activeProfile.FunctionSlotOrder.TryGetValue(sceneName, out var saved))
            {
                for (var i = 0; i < result.Count && i < saved.Count; i++)
                {
                    if (saved[i] != null && selected.Contains(saved[i]!) && !result.Contains(saved[i])) result[i] = saved[i];
                }
            }

            foreach (var name in selected)
            {
                if (result.Contains(name)) continue;
                var free = result.IndexOf(null);
                if (free < 0) break;
                result[free] = name;
            }
            return result;
        }

        private async Task ToggleFunctionSourceAsync(Button button)
        {
            var ws = _ws;
            if (ws == null || ws.State != WebSocketState.Open) return;
            if (button.Tag is not FunctionSlot slot || slot.SceneItemId == null) return;
            if (slot.SceneName != _currentProgramSceneName) return;

            var requestId = FunctionRequestPrefix + (++_functionRequestCounter) + "-" + Guid.NewGuid().ToString("N");
            lock (_pendingFunctionRequests) { _pendingFunctionRequests[requestId] = slot; }

            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "GetSceneItemEnabled",
                    ["requestId"] = requestId,
                    ["requestData"] = new JsonObject { ["sceneName"] = slot.SceneName, ["sceneItemId"] = slot.SceneItemId }
                }
            };

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, timeout.Token);
            }
            catch (Exception ex)
            {
                lock (_pendingFunctionRequests) { _pendingFunctionRequests.Remove(requestId); }
                ShowError("Invio del comando a OBS non riuscito: " + DescribeError(ex));
            }
        }

        private void HandleFunctionStateResponse(string requestId, JsonNode d)
        {
            FunctionSlot? slot;
            lock (_pendingFunctionRequests)
            {
                if (!_pendingFunctionRequests.Remove(requestId, out slot)) return;
            }

            if (d["requestStatus"]?["result"]?.GetValue<bool>() != true) return;
            if (slot.SceneName != _currentProgramSceneName) return;

            var enabled = d["responseData"]?["sceneItemEnabled"]?.GetValue<bool>();
            if (enabled == null) return;
            ApplyConfirmedStateToButtons(slot, enabled.Value);
        }

        private void HandleFunctionResponse(string requestId, JsonNode d)
        {
            FunctionSlot? slot;
            lock (_pendingFunctionRequests)
            {
                if (!_pendingFunctionRequests.Remove(requestId, out slot)) return;
            }

            if (d["requestStatus"]?["result"]?.GetValue<bool>() != true)
            {
                var comment = d["requestStatus"]?["comment"]?.GetValue<string>();
                ShowError("OBS non ha restituito lo stato della Source." + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
                return;
            }

            if (slot.SceneName != _currentProgramSceneName) return;

            var enabled = d["responseData"]?["sceneItemEnabled"]?.GetValue<bool>();
            if (enabled == null) return;
            ApplyConfirmedStateToButtons(slot, enabled.Value);

            var setId = SetSceneItemEnabledPrefix + (++_functionRequestCounter) + "-" + Guid.NewGuid().ToString("N");
            var ws = _ws;
            if (ws == null || ws.State != WebSocketState.Open) return;

            var setRequest = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "SetSceneItemEnabled",
                    ["requestId"] = setId,
                    ["requestData"] = new JsonObject
                    {
                        ["sceneName"] = slot.SceneName,
                        ["sceneItemId"] = slot.SceneItemId,
                        ["sceneItemEnabled"] = !enabled.Value
                    }
                }
            };
            _ = ws.SendAsync(Encoding.UTF8.GetBytes(setRequest.ToJsonString()), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private Button[] _functionButtons = Array.Empty<Button>();
        private readonly Dictionary<Button, Font> _baseButtonFonts = new();
        private readonly Dictionary<Button, Font> _adaptedFonts = new();
        private readonly Dictionary<Button, string> _buttonOriginalText = new();
        private readonly ToolTip _buttonToolTip = new();
        private const float MinButtonFontSize = 7F;

        private void RefreshFunctionButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RefreshFunctionButtons));
                return;
            }

            foreach (var button in _functionButtons)
            {
                ApplyAdaptiveButtonText(button, string.Empty);
                button.Tag = null;
                button.Enabled = chkUnlockFunctionLayout.Checked;
                UpdateFunctionButtonAppearance(button, null);
            }

            if (string.IsNullOrEmpty(_currentProgramSceneName) || _activeProfile == null) return;
            if (!IsObsConnected() || _activeProfile.SelectedScenes?.Contains(_currentProgramSceneName) != true)
            {
                foreach (var button in _functionButtons) button.Enabled = false;
                return;
            }
            if (!_activeProfile.SelectedSources.TryGetValue(_currentProgramSceneName, out var sources)) return;

            var sceneNode = tvObsItems.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == _currentProgramSceneName);

            var slotNames = BuildFunctionSlotNames(_currentProgramSceneName, sources);
            for (var i = 0; i < _functionButtons.Length; i++)
            {
                var sourceName = slotNames[i];
                if (sourceName == null) continue;
                int? sceneItemId = null;
                var sourceNode = sceneNode?.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == sourceName);
                if (sceneNode != null && sceneNode.Nodes.Count > 0 && sourceNode == null) continue;
                if (sourceNode != null) sceneItemId = (sourceNode.Tag as dynamic)?.SceneItemId;

                var button = _functionButtons[i];
                ApplyAdaptiveButtonText(button, GetFunctionAlias(_currentProgramSceneName, sourceName) ?? sourceName);
                button.Tag = new FunctionSlot(_currentProgramSceneName, sourceName, sceneItemId);
                button.Enabled = true;
                UpdateFunctionButtonAppearance(button, null);
                _ = RequestFunctionEnabledStateAsync(button);
            }
        }

        private async void BtnSaveReplay_Click(object? sender, EventArgs e)
        {
            var ws = _ws;
            if (ws == null || ws.State != WebSocketState.Open || !_isReplayBufferActive || _replayBusy) return;

            _replayBusy = true;
            _replayOpSaveConfirmed = false;
            _replayOpHotkeyRequestId = null;
            _replayOpHotkey = _activeProfile?.ReplayHotkeyName;
            btnSaveReplay.Enabled = false;
            btnSaveReplay.Text = "SAVING...";

            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "SaveReplayBuffer",
                    ["requestId"] = SaveReplayRequestPrefix + Guid.NewGuid().ToString("N")
                }
            };

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, timeout.Token);
            }
            catch
            {
                _replayOpSaveConfirmed = true;
                _ = ShowReplayFeedbackAsync("ERROR", SaveReplayErrorColor);
            }
        }

        private async Task TriggerReplayHotkeyAsync(ClientWebSocket ws, string hotkeyName, string requestId)
        {
            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "TriggerHotkeyByName",
                    ["requestId"] = requestId,
                    ["requestData"] = new JsonObject { ["hotkeyName"] = hotkeyName }
                }
            };

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, timeout.Token);
            }
            catch
            {
                if (_replayBusy && _replayOpHotkeyRequestId == requestId)
                {
                    _replayOpHotkeyRequestId = null;
                    _ = ShowReplayFeedbackAsync("HOTKEY ERROR", SaveReplayErrorColor);
                }
            }
        }

        private void UpdateReplayButtonNormalState()
        {
            if (IsDisposed || _replayBusy) return;

            var enabled = _ws?.State == WebSocketState.Open && _isReplayBufferActive;
            btnSaveReplay.Text = "SAVE REPLAY";
            btnSaveReplay.BackColor = enabled ? SaveReplayColor : SceneInactiveColor;
            btnSaveReplay.ForeColor = enabled ? Color.White : Color.LightGray;
            btnSaveReplay.Enabled = enabled;
        }

        private async Task ShowReplayFeedbackAsync(string text, Color color)
        {
            _replayFeedbackCts?.Cancel();
            var cts = _replayFeedbackCts = new CancellationTokenSource();
            btnSaveReplay.Text = text;
            btnSaveReplay.BackColor = color;
            btnSaveReplay.ForeColor = Color.White;
            btnSaveReplay.Enabled = false;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (IsDisposed) return;
            _replayBusy = false;
            _replayOpHotkey = null;
            _replayOpSaveConfirmed = false;
            _replayOpHotkeyRequestId = null;
            UpdateReplayButtonNormalState();
        }

        private void ApplyAdaptiveButtonText(Button button, string text)
        {
            if (!_baseButtonFonts.TryGetValue(button, out var baseFont))
            {
                baseFont = button.Font;
                _baseButtonFonts[button] = baseFont;
            }
            _buttonOriginalText[button] = text;

            var area = new Size(Math.Max(1, button.ClientSize.Width - 6), Math.Max(1, button.ClientSize.Height - 4));
            var font = baseFont;
            var ownsFont = false;
            var display = text;
            var adapted = false;

            while (!string.IsNullOrEmpty(text))
            {
                if (TextRenderer.MeasureText(text, font, Size.Empty, TextFormatFlags.NoPadding).Width <= area.Width)
                {
                    display = text;
                    break;
                }

                var wrapped = WrapTwoLines(text, font, area);
                if (wrapped != null)
                {
                    display = wrapped;
                    adapted = true;
                    break;
                }

                if (font.Size <= MinButtonFontSize)
                {
                    display = text;
                    adapted = true;
                    break;
                }

                if (ownsFont) font.Dispose();
                font = new Font(baseFont.FontFamily, Math.Max(MinButtonFontSize, font.Size - 0.5F), baseFont.Style, baseFont.Unit);
                ownsFont = true;
                adapted = true;
            }

            _adaptedFonts.Remove(button, out var previous);
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.Font = font;
            if (ownsFont) _adaptedFonts[button] = font;
            previous?.Dispose();

            button.Text = display;
            _buttonToolTip.SetToolTip(button, adapted ? text : null);
        }

        private static string? WrapTwoLines(string text, Font font, Size area)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2) return null;
            if (TextRenderer.MeasureText("Ag", font).Height * 2 > area.Height) return null;

            string? best = null;
            var bestWidth = int.MaxValue;
            for (var i = 1; i < words.Length; i++)
            {
                var first = string.Join(' ', words, 0, i);
                var second = string.Join(' ', words, i, words.Length - i);
                var widest = Math.Max(
                    TextRenderer.MeasureText(first, font, Size.Empty, TextFormatFlags.NoPadding).Width,
                    TextRenderer.MeasureText(second, font, Size.Empty, TextFormatFlags.NoPadding).Width);
                if (widest <= area.Width && widest < bestWidth)
                {
                    best = first + "\n" + second;
                    bestWidth = widest;
                }
            }
            return best;
        }

        private void UpdateFunctionButtonAppearance(Button button, bool? sceneItemEnabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateFunctionButtonAppearance(button, sceneItemEnabled)));
                return;
            }

            button.UseVisualStyleBackColor = false;
            button.BackColor = sceneItemEnabled == true ? FunctionVisibleColor : sceneItemEnabled == false ? FunctionHiddenColor : FunctionEmptyColor;
            button.ForeColor = sceneItemEnabled == null ? FunctionEmptyTextColor : Color.White;
        }

        private async Task RequestFunctionEnabledStateAsync(Button button)
        {
            var ws = _ws;
            if (ws == null || ws.State != WebSocketState.Open) return;
            if (button.Tag is not FunctionSlot slot || slot.SceneItemId == null) return;

            var requestId = FunctionStateRequestPrefix + (++_functionRequestCounter) + "-" + Guid.NewGuid().ToString("N");
            lock (_pendingFunctionRequests) { _pendingFunctionRequests[requestId] = slot; }

            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "GetSceneItemEnabled",
                    ["requestId"] = requestId,
                    ["requestData"] = new JsonObject { ["sceneName"] = slot.SceneName, ["sceneItemId"] = slot.SceneItemId }
                }
            };

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, timeout.Token);
            }
            catch (Exception ex)
            {
                lock (_pendingFunctionRequests) { _pendingFunctionRequests.Remove(requestId); }
                ShowError("Lettura stato Source non riuscita: " + DescribeError(ex));
            }
        }

        private readonly Dictionary<string, string> _sceneItemsRequestMap = new();

        // ---------- Profiles management ----------
        private void LoadProfiles()
        {
            try
            {
                if (!Directory.Exists(_profilesDir)) Directory.CreateDirectory(_profilesDir);

                var files = Directory.GetFiles(_profilesDir, "*.json");
                cmbProfile.BeginUpdate();
                cmbProfile.Items.Clear();

                foreach (var f in files)
                {
                    try
                    {
                        var name = Path.GetFileNameWithoutExtension(f);
                        cmbProfile.Items.Add(name);
                    }
                    catch
                    {
                        // ignore
                    }
                }

                // ensure at least one profile exists
                if (cmbProfile.Items.Count == 0)
                {
                    var def = new Profile { ProfileName = "Default" };
                    SaveProfileToFile(def);
                    cmbProfile.Items.Add(def.ProfileName);
                }

                // read last active
                if (File.Exists(_lastProfileFile))
                {
                    try { _activeProfileName = File.ReadAllText(_lastProfileFile); } catch { }
                }

                cmbProfile.EndUpdate();

                // select and load
                if (!string.IsNullOrEmpty(_activeProfileName) && cmbProfile.Items.Contains(_activeProfileName))
                {
                    cmbProfile.SelectedItem = _activeProfileName;
                    LoadProfile(_activeProfileName);
                }
                else
                {
                    cmbProfile.SelectedIndex = 0;
                    LoadProfile(cmbProfile.SelectedItem as string ?? string.Empty);
                }

                // wire buttons
                btnNewProfile.Click += BtnNewProfile_Click;
                btnRenameProfile.Click += BtnRenameProfile_Click;
                btnDeleteProfile.Click += BtnDeleteProfile_Click;
                cmbProfile.SelectedIndexChanged += CmbProfile_SelectedIndexChanged;
                chkAutoConnect.CheckedChanged += ChkAutoConnect_CheckedChanged;
                chkAlwaysOnTop.CheckedChanged += ChkAlwaysOnTop_CheckedChanged;
            }
            catch
            {
                // ignore profile load errors
            }
        }

        private void CmbProfile_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbProfile.SelectedItem is not string name) return;
            // save current
            if (!string.IsNullOrEmpty(_activeProfileName)) SaveActiveProfile();
            LoadProfile(name);
            SaveLastProfile(name);
        }

        private void SaveLastProfile(string name)
        {
            try
            {
                if (!Directory.Exists(_profilesDir)) Directory.CreateDirectory(_profilesDir);
                File.WriteAllText(_lastProfileFile, name);
            }
            catch { }
        }

        private void LoadProfile(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) return;
                var file = Path.Combine(_profilesDir, name + ".json");
                if (!File.Exists(file)) return;
                var json = File.ReadAllText(file);
                var p = JsonSerializer.Deserialize<Profile>(json);
                if (p == null) return;
                _activeProfile = p;
                _activeProfileName = p.ProfileName;
                _sceneHotkeyOps.Clear();
                chkUnlockFunctionLayout.Checked = false;

                // apply to UI
                _loadingProfile = true;
                txtHost.Text = p.Host;
                txtPort.Text = p.Port.ToString();
                txtPassword.Text = p.Password ?? string.Empty;
                chkAutoConnect.Checked = p.AutoConnect;
                chkAlwaysOnTop.Checked = p.AlwaysOnTop;
                TopMost = p.AlwaysOnTop;
                _loadingProfile = false;

                if (SyncSceneSlotOrder())
                if (MigrateLegacySceneAliases(p)) SaveProfileToFile(p);
                RestoreAudioSelections(IsObsConnected());
                RestoreAllSourceCheckStates();
                RefreshFunctionButtons();

                if (_ws != null && _ws.State == WebSocketState.Open)
                {
                    _ = RequestInputListAsync(_ws, CancellationToken.None);
                }

                RefreshSceneButtons();
            }
            catch { _loadingProfile = false; }
        }

        private const int SceneSlotCount = 9;

        private (Button Button, int Slot)[] SceneSlots() =>
            [(btnPrePartita, 1), (btnDiretta, 2), (btnFormazCasa, 3), (btnFormazOspiti, 4), (btnIntervallo, 5), (btnFinePartita, 6), (btnReplayMain, 7), (btnReplaySX, 8), (btnReplayDX, 9)];

        private SceneButtonSlot? GetSceneSlotData(int slot)
        {
            var slots = _activeProfile?.SceneButtonSlots;
            if (slots == null || slot < 1 || slot > slots.Count) return null;
            return slots[slot - 1];
        }

        private string? GetSlotScene(int slot)
        {
            var name = GetSceneSlotData(slot)?.SceneName;
            return string.IsNullOrEmpty(name) ? null : name;
        }

        private string? GetSlotHotkey(int slot)
        {
            var name = GetSceneSlotData(slot)?.HotkeyName;
            return string.IsNullOrEmpty(name) ? null : name;
        }

        private void StoreSlotData(int slot, Action<SceneButtonSlot> update)
        {
            var p = _activeProfile;
            if (p?.SceneButtonSlots == null || slot < 1 || slot > p.SceneButtonSlots.Count) return;
            var data = p.SceneButtonSlots[slot - 1] ?? new SceneButtonSlot();
            update(data);
            if (string.IsNullOrEmpty(data.SceneName) && string.IsNullOrEmpty(data.HotkeyName)) data = null!;
            p.SceneButtonSlots[slot - 1] = data;
            p.SceneSlotOrder = p.SceneButtonSlots.Select(s => s?.SceneName).ToList();
        }

        private bool SyncSceneSlotOrder()
        {
            var p = _activeProfile;
            if (p == null) return false;
            p.SelectedScenes ??= new List<string>();
            var beforeJson = p.SceneButtonSlots == null ? null : JsonSerializer.Serialize(p.SceneButtonSlots);
            var beforeOrder = p.SceneSlotOrder;
            var slots = new List<SceneButtonSlot?>(new SceneButtonSlot?[SceneSlotCount]);
            var used = new HashSet<string>();
            for (var i = 0; i < SceneSlotCount; i++)
            {
                SceneButtonSlot? s = null;
                if (p.SceneButtonSlots is { Count: SceneSlotCount })
                {
                    var src = p.SceneButtonSlots[i];
                    if (src != null) s = new SceneButtonSlot { SceneName = src.SceneName, HotkeyName = src.HotkeyName, HotkeyOnlyDisplayName = src.HotkeyOnlyDisplayName };
                }
                else if (p.SceneSlotOrder is { Count: SceneSlotCount } && p.SceneSlotOrder[i] != null)
                {
                    s = new SceneButtonSlot { SceneName = p.SceneSlotOrder[i] };
                }

                if (s == null) continue;
                if (string.IsNullOrEmpty(s.SceneName) || !p.SelectedScenes.Contains(s.SceneName) || !used.Add(s.SceneName)) s.SceneName = null;
                if (string.IsNullOrEmpty(s.HotkeyName)) s.HotkeyName = null;
                if (s.SceneName == null && s.HotkeyName == null) continue;
                slots[i] = s;
            }

            var pending = p.SelectedScenes
                .Where(n => !used.Contains(n))
                .OrderBy(n => { var idx = lstScenes.Items.IndexOf(n); return idx < 0 ? int.MaxValue : idx; })
                .ToList();
            foreach (var name in pending)
            {
                var free = slots.IndexOf(null);
                if (free < 0) break;
                slots[free] = new SceneButtonSlot { SceneName = name };
                used.Add(name);
            }

            var order = slots.Select(s => s?.SceneName).ToList();
            var changed = beforeJson == null || beforeJson != JsonSerializer.Serialize(slots)
                || beforeOrder == null || beforeOrder.Count != SceneSlotCount || !beforeOrder.SequenceEqual(order);
            if (changed)
            {
                p.SceneButtonSlots = slots;
                p.SceneSlotOrder = order;
            }
            return changed;
        }

        private void RefreshSceneButtons()
        {
            var connected = IsObsConnected();
            foreach (var (button, slot) in SceneSlots())
            {
                var data = GetSceneSlotData(slot);
                var scene = GetSlotScene(slot);
                var hotkey = GetSlotHotkey(slot);
                button.UseVisualStyleBackColor = false;
                button.ForeColor = Color.White;
                if (scene != null)
                {
                    button.Enabled = connected || chkUnlockFunctionLayout.Checked;
                    button.Text = GetSceneDisplayText(scene);
                    var isActive = !string.IsNullOrEmpty(_currentProgramSceneName) && scene == _currentProgramSceneName;
                    button.BackColor = isActive ? SceneActiveColor : SceneInactiveColor;
                }
                else if (hotkey != null)
                {
                    button.Enabled = connected || chkUnlockFunctionLayout.Checked;
                    button.Text = string.IsNullOrWhiteSpace(data?.HotkeyOnlyDisplayName) ? hotkey : data!.HotkeyOnlyDisplayName;
                    button.BackColor = SceneHotkeyOnlyColor;
                }
                else
                {
                    button.Enabled = connected || chkUnlockFunctionLayout.Checked;
                    button.Text = string.Empty;
                    button.BackColor = SceneInactiveColor;
                    button.ForeColor = Color.LightGray;
                }
            }
        }

        private static string GetSceneButtonText(Profile? profile, int slot)
        {
            var text = profile == null ? null : slot switch
            {
                1 => profile.Scene1ButtonText,
                2 => profile.Scene2ButtonText,
                3 => profile.Scene3ButtonText,
                4 => profile.Scene4ButtonText,
                5 => profile.Scene5ButtonText,
                6 => profile.Scene6ButtonText,
                7 => profile.Scene7ButtonText,
                8 => profile.Scene8ButtonText,
                _ => profile?.Scene9ButtonText
            };
            return string.IsNullOrWhiteSpace(text) ? GetSceneDefaultText(slot) : text;
        }

        private static string GetSceneDefaultText(int slot) => slot switch
        {
            7 => "MAIN",
            8 => "SX",
            9 => "DX",
            _ => $"SCENE {slot}"
        };

        private void SetSceneButtonText(Profile profile, int slot, string text)
        {
            switch (slot)
            {
                case 1: profile.Scene1ButtonText = text; break;
                case 2: profile.Scene2ButtonText = text; break;
                case 3: profile.Scene3ButtonText = text; break;
                case 4: profile.Scene4ButtonText = text; break;
                case 5: profile.Scene5ButtonText = text; break;
                case 6: profile.Scene6ButtonText = text; break;
                case 7: profile.Scene7ButtonText = text; break;
                case 8: profile.Scene8ButtonText = text; break;
                default: profile.Scene9ButtonText = text; break;
            }
        }

        private void SaveActiveProfile()
        {
            try
            {
                if (_activeProfile == null) return;
                // update from UI
                _activeProfile.Host = txtHost.Text;
                _activeProfile.Port = int.TryParse(txtPort.Text, out var p) ? p : _activeProfile.Port;
                _activeProfile.Password = txtPassword.Text ?? string.Empty;
                _activeProfile.AutoConnect = chkAutoConnect.Checked;
                if (!_loadingProfile)
                {
                    _activeProfile.Mic1InputName = AudioComboValue(cmbMic1, _activeProfile.Mic1InputName);
                    _activeProfile.Mic2InputName = AudioComboValue(cmbMic2, _activeProfile.Mic2InputName);
                    _activeProfile.WirelessInputName = AudioComboValue(cmbWireless, _activeProfile.WirelessInputName);
                }

                SaveProfileToFile(_activeProfile);
            }
            catch { }
        }

        private void SaveProfileToFile(Profile p)
        {
            try
            {
                if (!Directory.Exists(_profilesDir)) Directory.CreateDirectory(_profilesDir);
                var file = Path.Combine(_profilesDir, p.ProfileName + ".json");
                File.WriteAllText(file, JsonSerializer.Serialize(p));
            }
            catch { }
        }

        private void SaveProfileAs(Profile p, string newName)
        {
            p.ProfileName = newName;
            SaveProfileToFile(p);
        }

        private void SaveLastAndActive()
        {
            if (!string.IsNullOrEmpty(_activeProfileName)) SaveLastProfile(_activeProfileName);
            SaveActiveProfile();
        }

        private void BtnNewProfile_Click(object? sender, EventArgs e)
        {
            var name = Prompt.ShowDialog("Nome profilo:", "Nuovo profilo", string.Empty, this);
            if (string.IsNullOrWhiteSpace(name)) return;
            if (cmbProfile.Items.Contains(name))
            {
                MessageBox.Show(this, "Profilo già esistente.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var p = new Profile { ProfileName = name };
            SaveProfileToFile(p);
            cmbProfile.Items.Add(name);
            cmbProfile.SelectedItem = name;
        }

        private void BtnRenameProfile_Click(object? sender, EventArgs e)
        {
            if (cmbProfile.SelectedItem is not string name) return;
            var newName = Prompt.ShowDialog("Nuovo nome:", "Rinomina profilo", name, this);
            if (string.IsNullOrWhiteSpace(newName) || newName == name) return;
            if (cmbProfile.Items.Contains(newName))
            {
                MessageBox.Show(this, "Profilo già esistente.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var oldFile = Path.Combine(_profilesDir, name + ".json");
            var newFile = Path.Combine(_profilesDir, newName + ".json");
            try
            {
                if (File.Exists(oldFile)) File.Move(oldFile, newFile);
                cmbProfile.Items[cmbProfile.Items.IndexOf(name)] = newName;
                cmbProfile.SelectedItem = newName;
                if (_activeProfile != null && _activeProfile.ProfileName == name) _activeProfile.ProfileName = newName;
                SaveLastProfile(newName);
            }
            catch { }
        }

        private void BtnDeleteProfile_Click(object? sender, EventArgs e)
        {
            if (cmbProfile.SelectedItem is not string name) return;
            if (cmbProfile.Items.Count == 1)
            {
                MessageBox.Show(this, "Non è possibile eliminare l'unico profilo.", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var res = MessageBox.Show(this, $"Eliminare il profilo '{name}'?", "Conferma", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            try
            {
                var file = Path.Combine(_profilesDir, name + ".json");
                if (File.Exists(file)) File.Delete(file);
                var idx = cmbProfile.Items.IndexOf(name);
                cmbProfile.Items.RemoveAt(idx);
                var newSel = idx < cmbProfile.Items.Count ? idx : cmbProfile.Items.Count - 1;
                cmbProfile.SelectedIndex = newSel;
            }
            catch { }
        }

        private void ChkAutoConnect_CheckedChanged(object? sender, EventArgs e)
        {
            if (_activeProfile != null)
            {
                _activeProfile.AutoConnect = chkAutoConnect.Checked;
                SaveActiveProfile();
            }
        }

        private void ChkAlwaysOnTop_CheckedChanged(object? sender, EventArgs e)
        {
            TopMost = chkAlwaysOnTop.Checked;
            if (_loadingProfile || _activeProfile == null) return;
            _activeProfile.AlwaysOnTop = chkAlwaysOnTop.Checked;
            SaveActiveProfile();
        }

        private static Task InvokeAsync(Action a) => Task.Factory.StartNew(a, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.FromCurrentSynchronizationContext());

        private int _setSceneCounter;

        private const string CurrentProgramSceneRequestId = "CurrentProgramScene-1";
        private string _currentProgramSceneName = string.Empty;

        private void ApplyConfirmedStateToButtons(FunctionSlot target, bool enabled)
        {
            foreach (var button in _functionButtons)
            {
                if (button.Tag is FunctionSlot slot && slot.SceneName == target.SceneName && slot.SceneItemId == target.SceneItemId)
                {
                    UpdateFunctionButtonAppearance(button, enabled);
                }
            }
        }

        private void SetCurrentProgramScene(string? sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || sceneName == _currentProgramSceneName) return;
            _currentProgramSceneName = sceneName;
            RefreshSceneButtons();
            RefreshFunctionButtons();
        }

        private async void BtnDiretta_Click(object? sender, EventArgs e) => await SwitchSceneAsync(2);

        private async Task SwitchSceneAsync(int slot)
        {
            var ws = _ws;
            if (chkUnlockFunctionLayout.Checked) return;
            var scene = GetSlotScene(slot);
            var hotkey = GetSlotHotkey(slot);
            if (string.IsNullOrEmpty(scene) && string.IsNullOrEmpty(hotkey)) return;
            if (ws == null || ws.State != WebSocketState.Open) return;

            if (hotkey != null)
            {
                if (!_obsHotkeys.Contains(hotkey) || _sceneHotkeyOps.Values.Any(o => o.Slot == slot)) return;
                var hotkeyRequestId = TriggerSceneHotkeyRequestPrefix + slot + "-" + Guid.NewGuid().ToString("N");
                _sceneHotkeyOps[hotkeyRequestId] = new SceneHotkeyOp(slot, scene, hotkey);
                var hotkeyRequest = new JsonObject
                {
                    ["op"] = 6,
                    ["d"] = new JsonObject
                    {
                        ["requestType"] = "TriggerHotkeyByName",
                        ["requestId"] = hotkeyRequestId,
                        ["requestData"] = new JsonObject { ["hotkeyName"] = hotkey }
                    }
                };
                try
                {
                    using var hotkeyTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await ws.SendAsync(Encoding.UTF8.GetBytes(hotkeyRequest.ToJsonString()), WebSocketMessageType.Text, true, hotkeyTimeout.Token);
                }
                catch
                {
                    _sceneHotkeyOps.Remove(hotkeyRequestId);
                }
                return;
            }

            if (string.IsNullOrEmpty(scene)) return;
            await SendSetSceneAsync(ws, scene);
        }

        private void HandleSceneHotkeyResponse(string requestId, JsonNode data, ClientWebSocket ws)
        {
            if (!_sceneHotkeyOps.Remove(requestId, out var op)) return;
            if (data["requestStatus"]?["result"]?.GetValue<bool>() != true) return;
            if (string.IsNullOrEmpty(op.SceneName) || !ReferenceEquals(_ws, ws)) return;
            _ = SendSetSceneAsync(ws, op.SceneName);
        }

        private async Task SendSetSceneAsync(ClientWebSocket ws, string scene)
        {
            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "SetCurrentProgramScene",
                    ["requestId"] = "SetScene-" + (++_setSceneCounter),
                    ["requestData"] = new JsonObject { ["sceneName"] = scene }
                }
            };

            try
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, timeout.Token);
            }
            catch (Exception ex)
            {
                ShowError("Invio del comando a OBS non riuscito: " + DescribeError(ex));
            }
        }

        private static string DescribeClose(ClientWebSocket ws)
        {
            var code = (int?)ws.CloseStatus;
            return code switch
            {
                4009 => "Autenticazione fallita: password errata.",
                4010 => "Versione RPC non supportata da OBS.",
                4007 => "OBS ha rifiutato la connessione: identificazione non completata.",
                _ => $"OBS ha chiuso la connessione{(code.HasValue ? $" (codice {code})" : string.Empty)}."
            };
        }

        private static string DescribeError(Exception ex) => ex switch
        {
            WebSocketException => "Impossibile connettersi a OBS. Controlla host, porta e che il server WebSocket sia attivo.",
            JsonException => "Risposta non valida da OBS.",
            _ => ex.Message
        };

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "OBS Control Deck", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateConnectedUi()
        {
            lblObsStatus.Text = "● OBS Connesso";
            lblObsStatus.ForeColor = Color.Green;
            btnConnect.Text = "DISCONNETTI";
        }

        private void UpdateDisconnectedUi()
        {
            lstScenes.Items.Clear();
            _audioInputs.Clear();
            _audioMuteStates.Clear();
            _audioProbePending = 0;
            lock (_audioRequests) { _audioRequests.Clear(); }
            foreach (var combo in AudioCombos()) combo.Items.Clear();
            RefreshAudioButtons();
            foreach (var button in _functionButtons) UpdateFunctionButtonAppearance(button, null);
            lblObsStatus.Text = "● OBS Disconnesso";
            lblObsStatus.ForeColor = Color.Red;
            btnConnect.Text = "CONNETTI";
            chkUnlockFunctionLayout.Checked = false;
            _isReplayBufferActive = false;
            _replayBusy = false;
            _replayOpHotkey = null;
            _replayOpSaveConfirmed = false;
            _replayOpHotkeyRequestId = null;
            _replayFeedbackCts?.Cancel();
            _sceneHotkeyOps.Clear();
            UpdateReplayButtonNormalState();
            RefreshSceneButtons();
            RefreshFunctionButtons();
        }

        private const string InputListRequestId = "GetInputList-1";
        private const string AudioRequestPrefix = "Audio-";
        private enum AudioRequestKind { Probe, Read, Toggle, Set, Confirm }
        private sealed record AudioRequest(string InputName, AudioRequestKind Kind, bool Value = false);
        private readonly List<string> _audioInputs = new();
        private readonly Dictionary<string, bool> _audioMuteStates = new();
        private readonly Dictionary<string, AudioRequest> _audioRequests = new();
        private readonly SemaphoreSlim _audioSendGate = new(1, 1);
        private int _audioRequestCounter;
        private int _audioProbePending;

        private ComboBox[] AudioCombos() => [cmbMic1, cmbMic2, cmbWireless];

        private (Button Button, ComboBox Combo)[] AudioSlots() => [(btnMic1, cmbMic1), (btnMic2, cmbMic2), (btnWireless, cmbWireless)];

        private bool IsObsConnected() => _ws != null && _ws.State == WebSocketState.Open;

        private async void btnMic2_Click(object sender, EventArgs e)
        {
            await ToggleAudioMuteAsync(cmbMic2);
        }

        private async Task ToggleAudioMuteAsync(ComboBox combo)
        {
            if (combo.SelectedItem is not string input || !IsObsConnected() || !_audioInputs.Contains(input)) return;
            await SendAudioRequestAsync(input, AudioRequestKind.Toggle);
        }

        private async Task SendAudioRequestAsync(string input, AudioRequestKind kind, bool value = false, ClientWebSocket? ws = null, CancellationToken ct = default)
        {
            ws ??= _ws;
            if (ws == null || ws.State != WebSocketState.Open) return;

            var requestId = AudioRequestPrefix + kind + "-" + (++_audioRequestCounter);
            lock (_audioRequests) { _audioRequests[requestId] = new AudioRequest(input, kind, value); }

            var requestData = new JsonObject { ["inputName"] = input };
            if (kind == AudioRequestKind.Set) requestData["inputMuted"] = value;
            var request = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = kind == AudioRequestKind.Set ? "SetInputMute" : "GetInputMute",
                    ["requestId"] = requestId,
                    ["requestData"] = requestData
                }
            };

            try
            {
                await _audioSendGate.WaitAsync(ct);
                try
                {
                    await ws.SendAsync(Encoding.UTF8.GetBytes(request.ToJsonString()), WebSocketMessageType.Text, true, ct);
                }
                finally
                {
                    _audioSendGate.Release();
                }
            }
            catch (Exception ex)
            {
                lock (_audioRequests) { _audioRequests.Remove(requestId); }
                if (kind == AudioRequestKind.Probe) ResolveAudioProbe();
                else ShowError("Richiesta audio non inviata a OBS: " + DescribeError(ex));
            }
        }

        private async Task RequestInputListAsync(ClientWebSocket ws, CancellationToken ct)
        {
            var inputListRequest = new JsonObject
            {
                ["op"] = 6,
                ["d"] = new JsonObject
                {
                    ["requestType"] = "GetInputList",
                    ["requestId"] = InputListRequestId
                }
            };
            await ws.SendAsync(Encoding.UTF8.GetBytes(inputListRequest.ToJsonString()), WebSocketMessageType.Text, true, ct);
        }

        private void HandleInputListResponse(JsonNode d, ClientWebSocket ws, CancellationToken ct)
        {
            if (d["requestStatus"]?["result"]?.GetValue<bool>() != true)
            {
                var comment = d["requestStatus"]?["comment"]?.GetValue<string>();
                ShowError("OBS non ha restituito l'elenco input audio." + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
                return;
            }

            var inputs = d["responseData"]?["inputs"] as JsonArray;
            var names = inputs?.Select(i => i?["inputName"]?.GetValue<string>()).OfType<string>().ToList() ?? new List<string>();
            _audioInputs.Clear();
            _audioProbePending = names.Count;
            if (names.Count == 0)
            {
                PopulateAudioCombos();
                return;
            }
            _ = SendAudioProbesAsync(names, ws, ct);
        }

        private async Task SendAudioProbesAsync(List<string> names, ClientWebSocket ws, CancellationToken ct)
        {
            foreach (var name in names) await SendAudioRequestAsync(name, AudioRequestKind.Probe, false, ws, ct);
        }

        private void ResolveAudioProbe()
        {
            if (_audioProbePending > 0) _audioProbePending--;
            if (_audioProbePending == 0) PopulateAudioCombos();
        }

        private void HandleAudioResponse(string requestId, JsonNode d, ClientWebSocket ws, CancellationToken ct)
        {
            AudioRequest? req;
            lock (_audioRequests)
            {
                if (!_audioRequests.Remove(requestId, out req)) return;
            }

            var ok = d["requestStatus"]?["result"]?.GetValue<bool>() == true;
            var muted = d["responseData"]?["inputMuted"]?.GetValue<bool>();
            switch (req.Kind)
            {
                case AudioRequestKind.Probe:
                    if (ok && muted != null)
                    {
                        if (!_audioInputs.Contains(req.InputName)) _audioInputs.Add(req.InputName);
                        _audioMuteStates[req.InputName] = muted.Value;
                    }
                    ResolveAudioProbe();
                    break;
                case AudioRequestKind.Read:
                case AudioRequestKind.Confirm:
                    if (ok && muted != null)
                    {
                        _audioMuteStates[req.InputName] = muted.Value;
                        RefreshAudioButtons();
                    }
                    break;
                case AudioRequestKind.Toggle:
                    if (ok && muted != null) _ = SendAudioRequestAsync(req.InputName, AudioRequestKind.Set, !muted.Value, ws, ct);
                    else ReportAudioError("OBS non ha restituito lo stato mute dell'input.", d);
                    break;
                case AudioRequestKind.Set:
                    if (ok) _ = SendAudioRequestAsync(req.InputName, AudioRequestKind.Confirm, false, ws, ct);
                    else ReportAudioError("OBS non ha modificato lo stato mute dell'input.", d);
                    break;
            }
        }

        private void ReportAudioError(string message, JsonNode d)
        {
            var comment = d["requestStatus"]?["comment"]?.GetValue<string>();
            ShowError(message + (string.IsNullOrEmpty(comment) ? string.Empty : " " + comment));
        }

        private void PopulateAudioCombos()
        {
            _audioInputs.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (var combo in AudioCombos())
            {
                combo.BeginUpdate();
                combo.Items.Clear();
                foreach (var input in _audioInputs) combo.Items.Add(input);
                combo.EndUpdate();
            }
            RestoreAudioSelections(true);
        }

        private void RestoreAudioSelections(bool readStates)
        {
            foreach (var combo in AudioCombos())
            {
                var saved = GetProfileAudioInput(combo);
                combo.SelectedItem = !string.IsNullOrEmpty(saved) && combo.Items.Contains(saved) ? saved : null;
                if (readStates && combo.SelectedItem is string input) _ = SendAudioRequestAsync(input, AudioRequestKind.Read);
            }
            RefreshAudioButtons();
        }

        private string? GetProfileAudioInput(ComboBox combo) =>
            combo == cmbMic1 ? _activeProfile?.Mic1InputName :
            combo == cmbMic2 ? _activeProfile?.Mic2InputName :
            _activeProfile?.WirelessInputName;

        private static string? AudioComboValue(ComboBox combo, string? current) => combo.SelectedItem as string ?? current;

        private void AudioCombo_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (sender is not ComboBox combo) return;
            SaveActiveProfile();
            if (combo.SelectedItem is string input) _ = SendAudioRequestAsync(input, AudioRequestKind.Read);
            RefreshAudioButtons();
        }

        private void RefreshAudioButtons()
        {
            foreach (var (button, combo) in AudioSlots()) UpdateAudioButtonAppearance(button, combo);
        }

        private void UpdateAudioButtonAppearance(Button button, ComboBox combo)
        {
            button.UseVisualStyleBackColor = false;
            var input = combo.SelectedItem as string;
            if (!IsObsConnected() || input == null || !_audioMuteStates.TryGetValue(input, out var muted))
            {
                button.Enabled = false;
                button.BackColor = AudioEmptyColor;
                button.ForeColor = AudioEmptyTextColor;
                return;
            }

            button.Enabled = true;
            button.BackColor = muted ? AudioMutedColor : AudioUnmutedColor;
            button.ForeColor = Color.White;
        }

        private Button? _contextFunctionButton;

        private string? GetFunctionAlias(string sceneName, string sourceName)
        {
            if (_activeProfile == null) return null;
            return _activeProfile.FunctionAliases.TryGetValue(sceneName, out var aliases) && aliases.TryGetValue(sourceName, out var alias) ? alias : null;
        }

        private void cmsButtonRename_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _contextFunctionButton = (cmsButtonRename.SourceControl as Button);
            var scene = GetSceneSlot(_contextFunctionButton);
            var hasSlot = _contextFunctionButton?.Tag is FunctionSlot;
            var enabled = scene is { } s ? GetSceneSlotData(s.Slot) != null : hasSlot;
            var isReplay = _contextFunctionButton == btnSaveReplay;
            mnuRenameButton.Visible = !isReplay;
            mnuResetButtonName.Visible = !isReplay;
            _mnuAssignHotkey.Visible = isReplay || scene != null;
            _mnuRemoveHotkey.Visible = isReplay || scene != null;
            _mnuRemoveHotkey.Enabled = scene is { } rs ? GetSlotHotkey(rs.Slot) != null : !string.IsNullOrEmpty(_activeProfile?.ReplayHotkeyName);
            mnuRenameButton.Enabled = enabled;
            mnuResetButtonName.Enabled = enabled;
        }

        private void MnuAssignHotkey_Click(object? sender, EventArgs e)
        {
            if (_activeProfile == null) return;
            const string none = "NONE";
            var sceneSlot = GetSceneSlot(_contextFunctionButton);
            var saved = sceneSlot is { } ss ? GetSlotHotkey(ss.Slot) : _activeProfile.ReplayHotkeyName;
            var items = new List<string> { none };
            foreach (var h in _obsHotkeys) if (!items.Contains(h)) items.Add(h);
            if (!string.IsNullOrEmpty(saved) && !items.Contains(saved)) items.Add(saved);

            using var dialog = new Form
            {
                Text = "Assign Hotkey",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(360, 110),
                ShowInTaskbar = false,
                TopMost = TopMost
            };
            var lbl = new Label { Text = "Hotkey:", Location = new Point(12, 12), AutoSize = true };
            var cmb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(12, 34), Width = 336 };
            cmb.Items.AddRange(items.ToArray());
            cmb.SelectedItem = string.IsNullOrEmpty(saved) ? none : saved;
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(192, 70), Width = 75 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(273, 70), Width = 75 };
            dialog.Controls.AddRange(new Control[] { lbl, cmb, ok, cancel });
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;

            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            var selected = cmb.SelectedItem as string;
            var chosen = string.IsNullOrEmpty(selected) || selected == none ? null : selected;
            if (sceneSlot is { } target)
            {
                StoreSlotData(target.Slot, d => d.HotkeyName = chosen);
                SaveActiveProfile();
                RefreshSceneButtons();
                return;
            }
            _activeProfile.ReplayHotkeyName = chosen;
            SaveActiveProfile();
        }

        private void MnuRemoveHotkey_Click(object? sender, EventArgs e)
        {
            if (_activeProfile == null) return;
            if (GetSceneSlot(_contextFunctionButton) is { } target)
            {
                StoreSlotData(target.Slot, d =>
                {
                    d.HotkeyName = null;
                    if (string.IsNullOrEmpty(d.SceneName)) d.HotkeyOnlyDisplayName = null;
                });
                SaveActiveProfile();
                RefreshSceneButtons();
                return;
            }
            _activeProfile.ReplayHotkeyName = null;
            SaveActiveProfile();
        }

        private (int Slot, Button Button)? GetSceneSlot(Button? button)
        {
            foreach (var (b, slot) in SceneSlots())
            {
                if (b == button) return (slot, b);
            }
            return null;
        }

        private string? ShowRenameDialog(string current)
        {
            using var dialog = new Form
            {
                Text = "Rename button",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MinimizeBox = false,
                MaximizeBox = false,
                ClientSize = new Size(300, 110),
                ShowInTaskbar = false,
                TopMost = TopMost
            };
            var lbl = new Label { Text = "Button name", Location = new Point(12, 12), AutoSize = true };
            var txt = new TextBox { Text = current, Location = new Point(12, 34), Width = 276 };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(132, 70), Width = 75 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(213, 70), Width = 75 };
            dialog.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            dialog.AcceptButton = ok;
            dialog.CancelButton = cancel;
            dialog.Shown += (_, _) =>
            {
                txt.Focus();
                txt.SelectAll();
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return null;
            return txt.Text.Trim();
        }

        private void mnuRenameButton_Click(object? sender, EventArgs e)
        {
            if (GetSceneSlot(_contextFunctionButton) is { } scene)
            {
                if (_activeProfile == null) return;
                if (GetSlotScene(scene.Slot) is not { } sceneName)
                {
                    if (GetSlotHotkey(scene.Slot) is not { } hotkeyName) return;
                    var hotkeyText = ShowRenameDialog(GetSceneSlotData(scene.Slot)?.HotkeyOnlyDisplayName ?? hotkeyName);
                    if (string.IsNullOrEmpty(hotkeyText)) return;
                    StoreSlotData(scene.Slot, d => d.HotkeyOnlyDisplayName = hotkeyText);
                    SaveActiveProfile();
                    RefreshSceneButtons();
                    return;
                }
                var sceneText = ShowRenameDialog(GetSceneDisplayText(sceneName));
                if (string.IsNullOrEmpty(sceneText)) return;
                _activeProfile.SceneAliases[sceneName] = sceneText;
                SaveActiveProfile();
                RefreshSceneButtons();
                return;
            }

            if (_contextFunctionButton?.Tag is not FunctionSlot slot || _activeProfile == null) return;

            var current = GetFunctionAlias(slot.SceneName, slot.SourceName) ?? slot.SourceName;
            var alias = ShowRenameDialog(current);
            if (string.IsNullOrEmpty(alias)) return;

            if (!_activeProfile.FunctionAliases.TryGetValue(slot.SceneName, out var aliases))
            {
                aliases = new Dictionary<string, string>();
                _activeProfile.FunctionAliases[slot.SceneName] = aliases;
            }
            aliases[slot.SourceName] = alias;
            SaveProfileToFile(_activeProfile);
            ApplyAdaptiveButtonText(_contextFunctionButton, alias);
        }

        private void mnuResetButtonName_Click(object? sender, EventArgs e)
        {
            if (GetSceneSlot(_contextFunctionButton) is { } scene)
            {
                if (_activeProfile == null) return;
                if (GetSlotScene(scene.Slot) is not { } sceneName)
                {
                    StoreSlotData(scene.Slot, d => d.HotkeyOnlyDisplayName = null);
                    SaveActiveProfile();
                    RefreshSceneButtons();
                    return;
                }
                _activeProfile.SceneAliases.Remove(sceneName);
                SaveActiveProfile();
                RefreshSceneButtons();
                return;
            }

            if (_contextFunctionButton?.Tag is not FunctionSlot slot || _activeProfile == null) return;

            if (_activeProfile.FunctionAliases.TryGetValue(slot.SceneName, out var aliases))
            {
                aliases.Remove(slot.SourceName);
                if (aliases.Count == 0) _activeProfile.FunctionAliases.Remove(slot.SceneName);
                SaveProfileToFile(_activeProfile);
            }
            ApplyAdaptiveButtonText(_contextFunctionButton, slot.SourceName);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
