using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SilkySouls3.Enums;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Services;
using SilkySouls3.Utilities;
using SilkySouls3.ViewModels;
using SilkySouls3.Views;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IMemoryService _memoryService;

        private readonly DispatcherTimer _gameLoadedTimer;

        private readonly IStateService _stateService;
        private readonly IDlcService _dlcService;

        public MainWindow()
        {
            _memoryService = new MemoryService();
            _memoryService.StartAutoAttach();

            InitializeComponent();

            var savedLeft = SettingsManager.Default.WindowLeft;
            var savedTop = SettingsManager.Default.WindowTop;
            if ((savedLeft != 0 || savedTop != 0) && IsOnVisibleScreen(savedLeft, savedTop))
            {
                Left = savedLeft;
                Top = savedTop;
            }
            else WindowStartupLocation = WindowStartupLocation.CenterScreen;

            _stateService = new StateService(_memoryService);

            var hookManager = new HookManager(_memoryService, _stateService);
            var hotkeyManager = new HotkeyManager(_memoryService);


            IGameTickService gameTickService = new GameTickService(_stateService);
            IReminderService reminderService = new ReminderService(_memoryService, hookManager, _stateService);
            IParamService paramService = new ParamService(_memoryService);
            IEzStateService ezStateService = new EzStateService(_memoryService);

            _dlcService = new DlcService(_memoryService);

            IChrInsService chrInsService = new ChrInsService(_memoryService);
            ISpEffectService spEffectService = new SpEffectService(_memoryService, reminderService);
            ITravelService travelService = new TravelService(_memoryService, hookManager);
            IPlayerService playerService =
                new PlayerService(_memoryService, chrInsService, reminderService, travelService);
            IUtilityService utilityService = new UtilityService(_memoryService, hookManager, reminderService,
                paramService, playerService);
            IEventService eventService = new EventService(_memoryService);
            ITargetService targetService = new TargetService(_memoryService, hookManager, chrInsService,
                reminderService, playerService);
            IEnemyService enemyService = new EnemyService(_memoryService, hookManager);
            IItemService itemService = new ItemService(_memoryService);
            ISettingsService settingsService = new SettingsService(_memoryService);
            IDebugDrawService debugDrawService = new DebugDrawService(_memoryService, _stateService);

            ICinderService cinderService =
                new CinderService(_memoryService, hookManager, chrInsService, _stateService, spEffectService,
                    paramService, reminderService);

            var playerViewModel = new PlayerViewModel(playerService, hotkeyManager, _stateService, gameTickService,
                spEffectService);
            var utilityViewModel = new UtilityViewModel(utilityService, hotkeyManager,
                playerViewModel, debugDrawService, _stateService, ezStateService, eventService);
            var travelViewModel = new TravelViewModel(travelService, hotkeyManager, _stateService, _dlcService);
            var eventViewModel = new EventViewModel(eventService, _stateService, _dlcService);
            var targetViewModel = new TargetViewModel(targetService, hotkeyManager, debugDrawService, _stateService,
                gameTickService, spEffectService);
            var enemyViewModel = new EnemyViewModel(enemyService, cinderService, hotkeyManager, _stateService,
                paramService, debugDrawService, chrInsService, spEffectService, eventService);
            var itemViewModel = new ItemViewModel(itemService, _stateService);
            var settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager, _stateService);

            var playerTab = new PlayerTab(playerViewModel);
            var utilityTab = new UtilityTab(utilityViewModel);
            var eventTab = new EventTab(eventViewModel);
            var travelTab = new TravelTab(travelViewModel);
            var targetTab = new TargetTab(targetViewModel);
            var enemyTab = new EnemyTab(enemyViewModel);
            var itemTab = new ItemTab(itemViewModel);
            var settingsTab = new SettingsTab(settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Travel", Content = travelTab });
            MainTabControl.Items.Add(new TabItem { Header = "Event", Content = eventTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Target", Content = targetTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });

            settingsViewModel.ApplyStartUpOptions();
            Closing += MainWindow_Closing;

            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(25)
            };
            _gameLoadedTimer.Tick += Timer_Tick;
            _gameLoadedTimer.Start();

            VersionChecker.UpdateVersionText(AppVersion);

            if (SettingsManager.Default.EnableUpdateChecks)
            {
                VersionChecker.CheckForUpdates(this);
            }
        }

        private bool _wasAttached;
        private bool _isReady;
        private bool _loaded;
        private bool _appliedOneTimeFeatures;
        private CancellationTokenSource _attachCts;

        private void Timer_Tick(object sender, EventArgs e)
        {
            var isAttached = _memoryService.IsAttached;

            if (isAttached != _wasAttached)
            {
                _wasAttached = isAttached;
                if (isAttached) OnAttached();
                else OnDetached();
            }

            if (!isAttached || !_isReady) return;

            if (_stateService.IsLoaded())
            {
                if (_loaded) return;
                _loaded = true;
                _dlcService.CheckDlc();
                _stateService.Publish(State.Loaded);
                TrySetGameStartPrefs();
                if (_appliedOneTimeFeatures) return;
                _stateService.Publish(State.FirstLoaded);
                _appliedOneTimeFeatures = true;
            }
            else if (_loaded)
            {
                _stateService.Publish(State.NotLoaded);
                _loaded = false;
            }
        }

        private void OnAttached()
        {
            IsAttachedText.Text = "Attached to game";
            IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["AttachedBrush"];
            LaunchGameButton.IsEnabled = false;

            _attachCts = new CancellationTokenSource();
            _ = RunAttachSequenceAsync(_attachCts.Token);
        }

        private void OnDetached()
        {
            _attachCts?.Cancel();
            _isReady = false;
            _loaded = false;
            _appliedOneTimeFeatures = false;
            _stateService.Publish(State.NotLoaded);
            _stateService.Publish(State.Detached);
            IsAttachedText.Text = "Not attached";
            IsAttachedText.Foreground = (SolidColorBrush)Application.Current.Resources["NotAttachedBrush"];
            LaunchGameButton.IsEnabled = true;
        }

        private async Task RunAttachSequenceAsync(CancellationToken ct)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(2), ct);

                if (!PatchManager.Initialize(_memoryService))
                {
                    Console.WriteLine("PatchManager.Initialize failed; will retry on next attach.");
                    return;
                }


                ct.ThrowIfCancellationRequested();
                _memoryService.WriteBytes(Patches.NoLogo, AsmLoader.GetAsmBytes(AsmScript.NoLogo));
                _memoryService.AllocCodeCave();
                _stateService.Publish(State.Attached);
                
#if DEBUG
                PrintAll();
                Console.WriteLine($"Code cave: 0x{CustomCodeOffsets.Base.ToInt64():X}");
#endif
                ct.ThrowIfCancellationRequested();
                _isReady = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Attach sequence failed: {ex}");
            }
        }

        private void TrySetGameStartPrefs()
        {
            long gameTimeMs =
                _memoryService.Read<long>(_memoryService.Read<nint>(GameDataMan.Base) +
                                          GameDataMan.PlayerGameDataOffsets.InGameTime);

            if (gameTimeMs < 5000)
            {
                _stateService.Publish(State.OnNewGameStart);
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SettingsManager.Default.WindowLeft = Left;
            SettingsManager.Default.WindowTop = Top;
            SettingsManager.Default.Save();
        }

        private static bool IsOnVisibleScreen(double left, double top)
        {
            const double minVisibleX = 100;
            const double minVisibleY = 30;
            var vLeft = SystemParameters.VirtualScreenLeft;
            var vTop = SystemParameters.VirtualScreenTop;
            var vRight = vLeft + SystemParameters.VirtualScreenWidth;
            var vBottom = vTop + SystemParameters.VirtualScreenHeight;
            return left + minVisibleX > vLeft
                   && left < vRight - minVisibleX
                   && top + minVisibleY > vTop
                   && top < vBottom - minVisibleY;
        }

        private void LaunchGame_Click(object sender, RoutedEventArgs e) => Task.Run(GameLauncher.LaunchDarkSouls3);

        private void CheckUpdate_Click(object sender, RoutedEventArgs e) => VersionChecker.CheckForUpdates(this, true);
    }
}