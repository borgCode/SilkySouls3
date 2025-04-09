using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SilkySouls3.Memory;
using SilkySouls3.Services;
using SilkySouls3.ViewModels;
using SilkySouls3.Views;

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
        
        private readonly UtilityViewModel _utilityViewModel;
        private readonly EnemyViewModel _enemyViewModel;
        
        public MainWindow()
        {
            
            _memoryIo = new MemoryIo();
            _memoryIo.StartAutoAttach();
            
            InitializeComponent();
            
            _hookManager = new HookManager(_memoryIo);
            _aobScanner = new AoBScanner(_memoryIo);
            
            var utilityService = new UtilityService(_memoryIo, _hookManager);
            var enemyService = new EnemyService(_memoryIo, _hookManager);
            var cinderService = new CinderService(_memoryIo, _hookManager);
            //TODO INITS
            
            _utilityViewModel = new UtilityViewModel(utilityService);
            _enemyViewModel = new EnemyViewModel(enemyService, cinderService);
            
            var utilityTab = new UtilityTab(_utilityViewModel);
            var enemyTab = new EnemyTab(_enemyViewModel);
            
            MainTabControl.Items.Add(new TabItem { Header = "Utility", Content = utilityTab });
            MainTabControl.Items.Add(new TabItem { Header = "Enemies", Content = enemyTab });
            
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
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_memoryIo.IsAttached)
            {
                
                if (!_hasScanned)
                {
                    _aobScanner.Scan();
                    _hasScanned = true;
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
            }
        }

        private void TryEnableFeatures()
        {
            _enemyViewModel.TryEnableFeatures();
        }

        private void DisableFeatures()
        {
            _enemyViewModel.DisableFeatures();
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