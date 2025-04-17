using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
        
        private readonly MemoryIo _memoryIo;
        private readonly AoBScanner _aobScanner;
        private readonly DispatcherTimer _gameLoadedTimer;
        private readonly HookManager _hookManager;


        private readonly PlayerViewModel _playerViewModel;
        private readonly UtilityViewModel _utilityViewModel;
        private readonly EnemyViewModel _enemyViewModel;
        private readonly ItemViewModel _itemViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        
        public MainWindow()
        {
            
            _memoryIo = new MemoryIo();
            _memoryIo.StartAutoAttach();
            
            InitializeComponent();
            
            _hookManager = new HookManager(_memoryIo);
            _aobScanner = new AoBScanner(_memoryIo);
            var hotkeyManager = new HotkeyManager(_memoryIo);

            var playerService = new PlayerService(_memoryIo);
            var utilityService = new UtilityService(_memoryIo, _hookManager);
            var enemyService = new EnemyService(_memoryIo, _hookManager);
            var cinderService = new CinderService(_memoryIo, _hookManager);
            var itemService = new ItemService(_memoryIo, _hookManager);
            var settingsService = new SettingsService(_memoryIo);

            _playerViewModel = new PlayerViewModel(playerService, hotkeyManager);
            _utilityViewModel = new UtilityViewModel(utilityService, hotkeyManager, _playerViewModel);
            _enemyViewModel = new EnemyViewModel(enemyService, cinderService, hotkeyManager);
            _itemViewModel = new ItemViewModel(itemService);
            _settingsViewModel = new SettingsViewModel(settingsService, hotkeyManager);

            var playerTab = new PlayerTab(_playerViewModel);
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(_enemyViewModel);
            var itemTab = new ItemTab(_itemViewModel);
            var settingsTab = new SettingsTab(_settingsViewModel);

            MainTabControl.Items.Add(new TabItem { Header = "Player", Content = playerTab });
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            MainTabControl.Items.Add(new TabItem { Header = "Items", Content = itemTab });
            MainTabControl.Items.Add(new TabItem { Header = "Settings", Content = settingsTab });
            
            _settingsViewModel.ApplyStartUpOptions();
            
            _gameLoadedTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1)
            };
            _gameLoadedTimer.Tick += Timer_Tick;
            _gameLoadedTimer.Start();
            
            
        }
        private bool _loaded;
        private bool _hasScanned;
        private bool _hasAllocatedMemory;
        private bool _hasAppliedNoLogo;
        private bool _appliedOneTimeFeatures;

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryIo.IsAttached)
            {
                IsAttachedText.Text = "Attached to game";
                
                if (!_hasScanned)
                {
                    _aobScanner.Scan();
                    _hasScanned = true;
                }

                if (!_hasAppliedNoLogo)
                {
                    _memoryIo.WriteBytes(Patches.NoLogo, AsmLoader.GetAsmBytes("NoLogo"));
                    _hasAppliedNoLogo = true;
                }
                
                if (!_hasAllocatedMemory)
                {
                    _memoryIo.AllocCodeCave();
                    Console.WriteLine($"Code cave: 0x{CodeCaveOffsets.Base.ToInt64():X}");
                    _hasAllocatedMemory = true;
                }
                
                if (_memoryIo.IsGameLoaded())
                {
                    if (_loaded) return;
                    _loaded = true;
                    TryEnableFeatures();
                    TrySetGameStartPrefs();
                    if (_appliedOneTimeFeatures) return;
                    ApplyOneTimeFeatures();
                    _appliedOneTimeFeatures = true;
                }
                else if (_loaded)
                {
                    DisableFeatures();
                    _loaded = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
                DisableFeatures();
                _settingsViewModel.ResetAttached();
                _loaded = false;
                _hasAllocatedMemory = false;
                _hasAppliedNoLogo = false;
                _appliedOneTimeFeatures = false;
                IsAttachedText.Text = "Not attached";
            }
        }

        private void TryEnableFeatures()
        {
            _playerViewModel.TryEnableFeatures();
            _utilityViewModel.TryEnableFeatures();
            _enemyViewModel.TryEnableFeatures();
            _itemViewModel.TryEnableFeatures();
            _settingsViewModel.ApplyLoadedOptions();
        }

        private void TrySetGameStartPrefs()
        {
            ulong gameDataPtr = _memoryIo.ReadUInt64(GameDataMan.Base);
            IntPtr inGameTimePtr = (IntPtr)(gameDataPtr + GameDataMan.InGameTime);
            long gameTimeMs = _memoryIo.ReadInt64(inGameTimePtr);
            if (gameTimeMs < 5000)
            {
                _playerViewModel.TrySetNgPref();
                _itemViewModel.TrySpawnWeaponPref();
            }
        }

        private void ApplyOneTimeFeatures()
        {
            _utilityViewModel.TryApplyOneTimeFeatures();
        }

        private void DisableFeatures()
        {
            _playerViewModel.DisableFeatures();
            _utilityViewModel.DisableFeatures();
            _enemyViewModel.DisableFeatures();
            _itemViewModel.DisableFeatures();
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

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
    }
}