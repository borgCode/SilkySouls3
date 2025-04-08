using System;
using System.Windows.Threading;
using SilkySouls3.Memory;

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
        
        public MainWindow()
        {
            
            _memoryIo = new MemoryIo();
            _memoryIo.StartAutoAttach();
            
            InitializeComponent();
            
            _hookManager = new HookManager(_memoryIo);
            _aobScanner = new AoBScanner(_memoryIo);
            
            
            
            //TODO INITS
            
            
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
                    
                    _hasAllocatedMemory = true;
                }
                
                if (_memoryIo.IsGameLoaded())
                {
                    if (_loaded) return;
                    _loaded = true;
                    
                }
                else if (_loaded)
                {
                
                    _loaded = false;
                }
            }
            else
            {
                _hookManager.ClearHooks();
            }
        }
    }
}