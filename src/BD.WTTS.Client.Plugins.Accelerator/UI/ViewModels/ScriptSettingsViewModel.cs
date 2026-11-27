namespace BD.WTTS.UI.ViewModels;

public sealed class ScriptSettingsViewModel : ViewModelBase
{
    string newCustomUserAgent = string.Empty;
    string testUserAgentInput = string.Empty;
    string? customUserAgentError;

    public ScriptSettingsViewModel(Func<Task> openScriptStoreAsync)
    {
        ProxySettings.MigrateScriptScope();

        AddCustomUserAgentCommand = ReactiveCommand.Create(AddCustomUserAgent).AddTo(this);
        RemoveCustomUserAgentCommand = ReactiveCommand.Create<string>(RemoveCustomUserAgent).AddTo(this);
        ResetScopeCommand = ReactiveCommand.Create(ResetScope).AddTo(this);
        FillSampleUserAgentCommand = ReactiveCommand.Create<string>(FillSampleUserAgent).AddTo(this);
        OpenScriptCatCommand = ReactiveCommand.Create(() => { Browser2.Open(ScriptExternalLinks.ScriptCatUrl); }).AddTo(this);
        OpenGreasyForkCommand = ReactiveCommand.Create(() => { Browser2.Open(ScriptExternalLinks.GreasyForkUrl); }).AddTo(this);
        OpenScriptStoreCommand = ReactiveCommand.CreateFromTask(openScriptStoreAsync).AddTo(this);
        OpenScriptsDirectoryCommand = ReactiveCommand.Create(OpenScriptsDirectory).AddTo(this);

        ProxySettings.IsEnableScript.Subscribe(_ => this.RaisePropertyChanged(nameof(IsEnableScript)), false).AddTo(this);
        ProxySettings.IsAutoCheckScriptUpdate.Subscribe(_ => this.RaisePropertyChanged(nameof(IsAutoCheckScriptUpdate)), false).AddTo(this);
        ProxySettings.OnlyEnableProxyScript.Subscribe(_ => this.RaisePropertyChanged(nameof(OnlyEnableProxyScript)), false).AddTo(this);
        ProxySettings.ScriptInjectScopeMode.Subscribe(_ => NotifyScopeChanged(), false).AddTo(this);
        ProxySettings.ScriptUserAgentPresets.Subscribe(_ => NotifyScopeChanged(), false).AddTo(this);
        ProxySettings.ScriptCustomUserAgents.Subscribe(LoadCustomRules).AddTo(this);
    }

    public ICommand AddCustomUserAgentCommand { get; }
    public ICommand RemoveCustomUserAgentCommand { get; }
    public ICommand ResetScopeCommand { get; }
    public ICommand FillSampleUserAgentCommand { get; }
    public ICommand OpenScriptCatCommand { get; }
    public ICommand OpenGreasyForkCommand { get; }
    public ICommand OpenScriptStoreCommand { get; }
    public ICommand OpenScriptsDirectoryCommand { get; }

    public bool IsEnableScript
    {
        get => ProxySettings.IsEnableScript.Value;
        set => ProxySettings.IsEnableScript.Value = value;
    }

    public bool IsAutoCheckScriptUpdate
    {
        get => ProxySettings.IsAutoCheckScriptUpdate.Value;
        set => ProxySettings.IsAutoCheckScriptUpdate.Value = value;
    }

    public bool OnlyEnableProxyScript
    {
        get => ProxySettings.OnlyEnableProxyScript.Value;
        set => ProxySettings.OnlyEnableProxyScript.Value = value;
    }

    public bool IsScopeAll
    {
        get => ProxySettings.ScriptInjectScopeMode.Value == ScriptInjectScopeMode.All;
        set
        {
            if (value)
                ProxySettings.ScriptInjectScopeMode.Value = ScriptInjectScopeMode.All;
        }
    }

    public bool IsScopeSelected
    {
        get => ProxySettings.ScriptInjectScopeMode.Value == ScriptInjectScopeMode.SelectedUserAgents;
        set
        {
            if (value)
                ProxySettings.ScriptInjectScopeMode.Value = ScriptInjectScopeMode.SelectedUserAgents;
        }
    }

    public bool PresetSteamBrowser
    {
        get => ProxySettings.ScriptUserAgentPresets.Value.HasFlag(ScriptUserAgentPreset.SteamBrowser);
        set => SetPreset(ScriptUserAgentPreset.SteamBrowser, value);
    }

    public bool PresetChrome
    {
        get => ProxySettings.ScriptUserAgentPresets.Value.HasFlag(ScriptUserAgentPreset.Chrome);
        set => SetPreset(ScriptUserAgentPreset.Chrome, value);
    }

    public bool PresetEdge
    {
        get => ProxySettings.ScriptUserAgentPresets.Value.HasFlag(ScriptUserAgentPreset.Edge);
        set => SetPreset(ScriptUserAgentPreset.Edge, value);
    }

    public ObservableCollection<string> CustomUserAgents { get; } = [];
    public bool HasCustomUserAgents => CustomUserAgents.Count > 0;
    public string NewCustomUserAgent
    {
        get => newCustomUserAgent;
        set
        {
            this.RaiseAndSetIfChanged(ref newCustomUserAgent, value ?? string.Empty);
            CustomUserAgentError = null;
        }
    }

    public string? CustomUserAgentError
    {
        get => customUserAgentError;
        private set
        {
            this.RaiseAndSetIfChanged(ref customUserAgentError, value);
            this.RaisePropertyChanged(nameof(HasCustomUserAgentError));
        }
    }

    public bool HasCustomUserAgentError => !string.IsNullOrWhiteSpace(CustomUserAgentError);

    public bool IsScopeEmptyWarning => IsScopeSelected && !PresetSteamBrowser && !PresetChrome && !PresetEdge && !HasCustomUserAgents;
    public string ScopeSummary
    {
        get
        {
            if (IsScopeAll)
                return Strings.Script_ScopeSummary_All;
            if (IsScopeEmptyWarning)
                return Strings.Script_ScopeSummary_None;

            var labels = new List<string>();
            if (PresetSteamBrowser)
                labels.Add(Strings.Script_Settings_Preset_Steam);
            if (PresetChrome)
                labels.Add(Strings.Script_Settings_Preset_Chrome);
            if (PresetEdge)
                labels.Add(Strings.Script_Settings_Preset_Edge);
            if (HasCustomUserAgents)
                labels.Add(Strings.Script_ScopeSummary_CustomCountFormat.Format(CustomUserAgents.Count));
            return Strings.Script_ScopeSummary_RestrictedFormat.Format(string.Join(", ", labels));
        }
    }

    public string TestUserAgentInput
    {
        get => testUserAgentInput;
        set
        {
            this.RaiseAndSetIfChanged(ref testUserAgentInput, value ?? string.Empty);
            this.RaisePropertyChanged(nameof(HasTestUserAgentInput));
            NotifyTestResult();
        }
    }
    public bool HasTestUserAgentInput => !string.IsNullOrWhiteSpace(TestUserAgentInput);
    public bool TestUserAgentMatched { get; private set; }
    public string TestUserAgentResultText { get; private set; } = string.Empty;

    void SetPreset(ScriptUserAgentPreset preset, bool selected)
    {
        var presets = ProxySettings.ScriptUserAgentPresets.Value;
        ProxySettings.ScriptUserAgentPresets.Value = selected ? presets | preset : presets & ~preset;
    }

    void LoadCustomRules(string? rules)
    {
        CustomUserAgents.Clear();
        foreach (var rule in ScriptUserAgentMatcher.ParseCustomRules(rules))
            CustomUserAgents.Add(rule);
        this.RaisePropertyChanged(nameof(HasCustomUserAgents));
        NotifyScopeChanged();
    }

    void AddCustomUserAgent()
    {
        if (!ScriptUserAgentMatcher.TryValidateCustomRule(NewCustomUserAgent, out var rule, out var error))
        {
            CustomUserAgentError = error;
            return;
        }
        if (CustomUserAgents.Contains(rule, StringComparer.OrdinalIgnoreCase))
        {
            CustomUserAgentError = Strings.Script_Settings_CustomUaDuplicate;
            return;
        }

        ProxySettings.ScriptCustomUserAgents.Value = string.Join("\n", CustomUserAgents.Append(rule));
        NewCustomUserAgent = string.Empty;
    }

    void RemoveCustomUserAgent(string rule)
    {
        var rules = string.Join("\n", CustomUserAgents.Where(value => value != rule));
        ProxySettings.ScriptCustomUserAgents.Value = rules.Length == 0 ? null : rules;
    }

    void ResetScope()
    {
        ProxySettings.ScriptInjectScopeMode.Value = ScriptInjectScopeMode.All;
        ProxySettings.ScriptUserAgentPresets.Value = ScriptUserAgentPreset.None;
        ProxySettings.ScriptCustomUserAgents.Value = null;
        NewCustomUserAgent = string.Empty;
    }

    void NotifyScopeChanged()
    {
        this.RaisePropertyChanged(nameof(IsScopeAll));
        this.RaisePropertyChanged(nameof(IsScopeSelected));
        this.RaisePropertyChanged(nameof(PresetSteamBrowser));
        this.RaisePropertyChanged(nameof(PresetChrome));
        this.RaisePropertyChanged(nameof(PresetEdge));
        this.RaisePropertyChanged(nameof(IsScopeEmptyWarning));
        this.RaisePropertyChanged(nameof(ScopeSummary));
        NotifyTestResult();
    }

    void NotifyTestResult()
    {
        var result = ScriptUserAgentMatcher.Evaluate(TestUserAgentInput.Trim(), ProxySettings.ScriptInjectScopeMode.Value,
            ProxySettings.ScriptUserAgentPresets.Value, CustomUserAgents);
        var reason = result.MatchedReason switch
        {
            nameof(ScriptInjectScopeMode.All) => Strings.Script_Settings_ScopeModeAll,
            nameof(ScriptUserAgentPreset.SteamBrowser) => Strings.Script_Settings_Preset_Steam,
            nameof(ScriptUserAgentPreset.Chrome) => Strings.Script_Settings_Preset_Chrome,
            nameof(ScriptUserAgentPreset.Edge) => Strings.Script_Settings_Preset_Edge,
            _ => result.MatchedReason,
        };

        TestUserAgentMatched = HasTestUserAgentInput && result.IsMatched;
        TestUserAgentResultText = TestUserAgentMatched ? Strings.Script_Settings_UaTesterMatch.Format(reason)
            : Strings.Script_Settings_UaTesterNoMatch;
        this.RaisePropertyChanged(nameof(TestUserAgentMatched));
        this.RaisePropertyChanged(nameof(TestUserAgentResultText));
    }

    void FillSampleUserAgent(string browser)
    {
        const string chromium = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";
        TestUserAgentInput = browser switch
        {
            "steam" => chromium + " Valve Steam Client",
            "chrome" => chromium,
            "edge" => chromium + " Edg/131.0.0.0",
            "firefox" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0",
            _ => TestUserAgentInput,
        };
    }

    static void OpenScriptsDirectory()
    {
        try
        {
            var path = Path.Combine(Plugin.Instance.AppDataDirectory, IScriptManager.DirName);
            var directory = Directory.CreateDirectory(path);
            IPlatformService.Instance.OpenFolderByDirectoryPath(directory);
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }
}
