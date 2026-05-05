using System;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.ViewModels
{
    public class UtilityViewModel : BaseViewModel
    {
        private float _desiredGameSpeed = -1f;
        private const float DefaultGameSpeed = 1f;
        private const float Epsilon = 0.0001f;

        private readonly IUtilityService _utilityService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly PlayerViewModel _playerViewModel;
        private readonly IDebugDrawService _debugDrawService;
        private readonly IEzStateService _ezStateService;
        private readonly IEventService _eventService;

        private const float DefaultNoclipSpeedScale = 1f;

        private const float DefaultFov = 43.0f;

        private bool _wasNoDeathEnabled;

        public UtilityViewModel(IUtilityService utilityService,
            HotkeyManager hotkeyManager, PlayerViewModel playerViewModel, IDebugDrawService debugDrawService,
            IStateService stateService, IEzStateService ezStateService, IEventService eventService)
        {
            _utilityService = utilityService;
            _hotkeyManager = hotkeyManager;
            _playerViewModel = playerViewModel;
            _debugDrawService = debugDrawService;
            _ezStateService = ezStateService;
            _eventService = eventService;

            Fps = 75f;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe(State.FirstLoaded, FirstLoaded);

            BreakAllObjectsCommand = new DelegateCommand(_utilityService.BreakAllObjects);
            RestoreAllObjectsCommand = new DelegateCommand(_utilityService.RestoreAllObjects);
            OpenShopCommand = new DelegateCommand<int[]>(OpenShop);
            TravelCommand = new DelegateCommand(OpenTravelMenu);
            LevelUpCommand = new DelegateCommand(OpenLevelUpMenu);
            ReinforceWeaponCommand = new DelegateCommand(OpenReinforceWeaponMenu);
            InfuseWeaponCommand = new DelegateCommand(OpenInfuseWeaponMenu);
            RepairCommand = new DelegateCommand(OpenRepairMenu);
            AttunementCommand = new DelegateCommand(OpenAttunementMenu);
            AllotEstusCommand = new DelegateCommand(OpenAllotEstusMenu);
            MoveCamToPlayerCommand = new DelegateCommand(MoveCamToPlayer);
            MovePlayerToCamCommand = new DelegateCommand(MovePlayerToCam);
            SetDefaultFovCommand = new DelegateCommand(SetDefaultFov);


            RegisterHotkeys();
            ApplyPrefs();
        }

        #region Commands

        public ICommand BreakAllObjectsCommand { get; }
        public ICommand RestoreAllObjectsCommand { get; }
        public ICommand OpenShopCommand { get; }
        public ICommand TravelCommand { get; }
        public ICommand LevelUpCommand { get; }
        public ICommand ReinforceWeaponCommand { get; }
        public ICommand InfuseWeaponCommand { get; }
        public ICommand RepairCommand { get; }
        public ICommand AttunementCommand { get; }
        public ICommand AllotEstusCommand { get; }
        public ICommand MoveCamToPlayerCommand { get; }
        public ICommand MovePlayerToCamCommand { get; }
        public ICommand SetDefaultFovCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _is100DropEnabled;

        public bool Is100DropEnabled
        {
            get => _is100DropEnabled;
            set
            {
                if (!SetProperty(ref _is100DropEnabled, value)) return;
                _utilityService.Toggle100Drop(_is100DropEnabled);
            }
        }

        private bool _isFullLineUpEnabled;

        public bool IsFullLineUpEnabled
        {
            get => _isFullLineUpEnabled;
            set
            {
                if (!SetProperty(ref _isFullLineUpEnabled, value)) return;
                _utilityService.ToggleFullLineUp(_isFullLineUpEnabled);
            }
        }

        private float _gameSpeed;

        public float GameSpeed
        {
            get => _gameSpeed;
            set
            {
                if (SetProperty(ref _gameSpeed, value))
                {
                    _utilityService.SetGameSpeed(value);
                    if (IsRememberSpeedEnabled && Math.Abs(value - DefaultGameSpeed) > Epsilon)
                    {
                        SettingsManager.Default.GameSpeed = value;
                    }
                }
            }
        }

        private bool _isRememberSpeedEnabled;

        public bool IsRememberSpeedEnabled
        {
            get => _isRememberSpeedEnabled;
            set
            {
                if (SetProperty(ref _isRememberSpeedEnabled, value))
                {
                    if (_isRememberSpeedEnabled)
                    {
                        SettingsManager.Default.RememberGameSpeed = _isRememberSpeedEnabled;
                        if (Math.Abs(GameSpeed - DefaultGameSpeed) > Epsilon)
                        {
                            SettingsManager.Default.GameSpeed = GameSpeed;
                        }
                    }
                    else
                    {
                        SettingsManager.Default.GameSpeed = DefaultGameSpeed;
                        SettingsManager.Default.RememberGameSpeed = _isRememberSpeedEnabled;
                    }

                    SettingsManager.Default.Save();
                }
            }
        }

        private bool _isDbgFpsEnabled;

        public bool IsDbgFpsEnabled
        {
            get => _isDbgFpsEnabled;
            set
            {
                if (!SetProperty(ref _isDbgFpsEnabled, value)) return;
                _utilityService.ToggleDbgFps(_isDbgFpsEnabled);
                _utilityService.SetFps(Fps);
            }
        }

        private float _fps;

        public float Fps
        {
            get => _fps;
            set
            {
                if (!SetProperty(ref _fps, value)) return;
                _utilityService.SetFps(value);
            }
        }

        private bool _isNoClipEnabled;

        public bool IsNoClipEnabled
        {
            get => _isNoClipEnabled;
            set
            {
                if (!SetProperty(ref _isNoClipEnabled, value)) return;

                if (_isNoClipEnabled)
                {
                    IsFreeCamEnabled = false;
                    _utilityService.WriteNoClipSpeed(_noClipSpeedScale);
                    _wasNoDeathEnabled = _playerViewModel.IsNoDeathEnabled;
                    _playerViewModel.IsNoDeathEnabled = true;
                }
                else
                {
                    _playerViewModel.IsNoDeathEnabled = _wasNoDeathEnabled;
                }

                _utilityService.ToggleNoClip(_isNoClipEnabled);
            }
        }

        private float _noClipSpeedScale = DefaultNoclipSpeedScale;

        public float NoClipSpeedScale
        {
            get => _noClipSpeedScale;
            set
            {
                if (SetProperty(ref _noClipSpeedScale, value))
                {
                    if (!IsNoClipEnabled) return;
                    _utilityService.WriteNoClipSpeed(_noClipSpeedScale);
                }
            }
        }

        private bool _isFreeCamEnabled;

        public bool IsFreeCamEnabled
        {
            get => _isFreeCamEnabled;
            set
            {
                if (!SetProperty(ref _isFreeCamEnabled, value)) return;
                if (_isFreeCamEnabled)
                {
                    IsNoClipEnabled = false;
                }
                else
                {
                    _isPlayerMovementEnabled = false;
                    OnPropertyChanged(nameof(IsPlayerMovementEnabled));
                }

                _utilityService.ToggleFreeCam(_isFreeCamEnabled);
            }
        }

        private bool _isPlayerMovementEnabled;

        public bool IsPlayerMovementEnabled
        {
            get => _isPlayerMovementEnabled;
            set
            {
                if (!SetProperty(ref _isPlayerMovementEnabled, value)) return;
                if (!IsFreeCamEnabled) return;
                _utilityService.TogglePlayerMovementForFreeCam(_isPlayerMovementEnabled);
            }
        }

        private bool _isFreezeWorldEnabled;

        public bool IsFreezeWorldEnabled
        {
            get => _isFreezeWorldEnabled;
            set
            {
                if (!SetProperty(ref _isFreezeWorldEnabled, value)) return;
                _utilityService.ToggleFreezeWorld(_isFreezeWorldEnabled);
            }
        }

        private bool _isCamVertIncreaseEnabled;

        public bool IsCamVertIncreaseEnabled
        {
            get => _isCamVertIncreaseEnabled;
            set
            {
                if (!SetProperty(ref _isCamVertIncreaseEnabled, value)) return;
                _utilityService.ToggleCamVertIncrease(_isCamVertIncreaseEnabled);
            }
        }

        private bool _isDeathCamEnabled;

        public bool IsDeathCamEnabled
        {
            get => _isDeathCamEnabled;
            set
            {
                if (!SetProperty(ref _isDeathCamEnabled, value)) return;
                _utilityService.ToggleDeathCam(_isDeathCamEnabled);
            }
        }

        private float _cameraFov;

        public float CameraFov
        {
            get => _cameraFov;
            set
            {
                if (SetProperty(ref _cameraFov, value))
                {
                    _utilityService.SetFov(_cameraFov);
                }
            }
        }

        private bool _isHitboxEnabled;

        public bool IsHitboxEnabled
        {
            get => _isHitboxEnabled;
            set
            {
                if (!SetProperty(ref _isHitboxEnabled, value)) return;
                _utilityService.ToggleHitboxView(_isHitboxEnabled);
            }
        }

        private bool _isSoundViewEnabled;

        public bool IsSoundViewEnabled
        {
            get => _isSoundViewEnabled;
            set
            {
                if (!SetProperty(ref _isSoundViewEnabled, value)) return;
                _utilityService.ToggleSoundView(_isSoundViewEnabled);
            }
        }

        private bool _isDrawLowHitEnabled;

        public bool IsDrawLowHitEnabled
        {
            get => _isDrawLowHitEnabled;
            set
            {
                if (!SetProperty(ref _isDrawLowHitEnabled, value)) return;
                IsHideMapEnabled = _isDrawLowHitEnabled;
                _utilityService.ToggleHitIns(HitFlags.LowHit, _isDrawLowHitEnabled);
            }
        }

        private bool _isDrawHighHitEnabled;

        public bool IsDrawHighHitEnabled
        {
            get => _isDrawHighHitEnabled;
            set
            {
                if (!SetProperty(ref _isDrawHighHitEnabled, value)) return;
                IsHideMapEnabled = _isDrawHighHitEnabled;
                _utilityService.ToggleHitIns(HitFlags.HighHit, _isDrawHighHitEnabled);
            }
        }

        private bool _isDrawChrRagdollEnabled;

        public bool IsDrawChrRagdollEnabled
        {
            get => _isDrawChrRagdollEnabled;
            set
            {
                if (!SetProperty(ref _isDrawChrRagdollEnabled, value)) return;
                if (value)
                    _debugDrawService.RequestDebugDraw();
                else
                    _debugDrawService.ReleaseDebugDraw();

                _utilityService.ToggleHitIns(HitFlags.ChrRagdoll, _isDrawChrRagdollEnabled);
            }
        }

        private bool _isHideMapEnabled;

        public bool IsHideMapEnabled
        {
            get => _isHideMapEnabled;
            set
            {
                if (!SetProperty(ref _isHideMapEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Map, _isHideMapEnabled);
            }
        }

        private bool _isHideObjectsEnabled;

        public bool IsHideObjectsEnabled
        {
            get => _isHideObjectsEnabled;
            set
            {
                if (!SetProperty(ref _isHideObjectsEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Obj, _isHideObjectsEnabled);
            }
        }

        private bool _isHideCharactersEnabled;

        public bool IsHideCharactersEnabled
        {
            get => _isHideCharactersEnabled;
            set
            {
                if (!SetProperty(ref _isHideCharactersEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Chr, _isHideCharactersEnabled);
            }
        }

        private bool _isHideSfxEnabled;

        public bool IsHideSfxEnabled
        {
            get => _isHideSfxEnabled;
            set
            {
                if (!SetProperty(ref _isHideSfxEnabled, value)) return;
                _utilityService.ToggleGroupMask(GroupMask.Sfx, _isHideSfxEnabled);
            }
        }

        #endregion

        #region Public Methods

        public void SetSpeed(float value) => GameSpeed = value;

        public void MoveCamToPlayer()
        {
            if (IsFreeCamEnabled) _utilityService.MoveCamToPlayer();
        }

        public void MovePlayerToCam()
        {
            if (IsFreeCamEnabled) _utilityService.MovePlayerToCam();
        }

        public void SetDefaultFov()
        {
            _utilityService.SetFov(DefaultFov);
            CameraFov = _utilityService.GetFov();
        }

        #endregion

        #region Private Methods

        private void ToggleSpeed()
        {
            if (!AreOptionsEnabled) return;

            if (!IsApproximately(GameSpeed, DefaultGameSpeed))
            {
                _desiredGameSpeed = GameSpeed;
                SetSpeed(DefaultGameSpeed);
            }
            else if (_desiredGameSpeed >= 0)
            {
                SetSpeed(_desiredGameSpeed);
            }
        }

        private bool IsApproximately(float a, float b) => Math.Abs(a - b) < Epsilon;

        private void ApplyPrefs()
        {
            _isRememberSpeedEnabled = SettingsManager.Default.RememberGameSpeed;
            OnPropertyChanged(nameof(IsRememberSpeedEnabled));
            if (_isRememberSpeedEnabled) _desiredGameSpeed = SettingsManager.Default.GameSpeed;
        }

        private void OpenTravelMenu()
        {
            _eventService.SetEvent(EventFlag.FirelinkBonfire, true);
            _eventService.SetEvent(EventFlag.CoiledSword, true);
            _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.WarpMenu);
        }

        private void OpenLevelUpMenu() => _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.LevelUp);

        private void OpenReinforceWeaponMenu()
        {
            foreach (var upgradeMenuFlag in EzState.TalkCommands.UpgradeMenuFlags)
            {
                _ezStateService.ExecuteTalkCommand(upgradeMenuFlag);
            }

            _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.OpenUpgrade);
        }

        private void OpenInfuseWeaponMenu()
        {
            foreach (var infuseFlag in EzState.TalkCommands.InfuseMenuFlags)
            {
                _ezStateService.ExecuteTalkCommand(infuseFlag);
            }

            _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.OpenInfuse);
        }

        private void OpenRepairMenu() => _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.Repair);
        private void OpenAttunementMenu() => _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.OpenAttunement);
        private void OpenAllotEstusMenu() => _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.OpenAllotEstus);

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.NoClip, () =>
            {
                if (!AreOptionsEnabled) return;
                IsNoClipEnabled = !IsNoClipEnabled;
            });
            _hotkeyManager.RegisterAction(HotkeyActions.EnableDeathCam, () =>
            {
                if (!AreOptionsEnabled) return;
                IsDeathCamEnabled = !IsDeathCamEnabled;
            });
            _hotkeyManager.RegisterAction(HotkeyActions.IncreaseNoClipSpeed, () =>
            {
                if (IsNoClipEnabled) NoClipSpeedScale = Math.Min(5, NoClipSpeedScale + 0.50f);
            });

            _hotkeyManager.RegisterAction(HotkeyActions.DecreaseNoClipSpeed, () =>
            {
                if (IsNoClipEnabled) NoClipSpeedScale = Math.Max(0.5f, NoClipSpeedScale - 0.50f);
            });

            _hotkeyManager.RegisterAction(HotkeyActions.IncreaseGameSpeed,
                () => SetSpeed(Math.Min(10, GameSpeed + 0.50f)));
            _hotkeyManager.RegisterAction(HotkeyActions.DecreaseGameSpeed,
                () => SetSpeed(Math.Max(0, GameSpeed - 0.50f)));
            _hotkeyManager.RegisterAction(HotkeyActions.ToggleGameSpeed, ToggleSpeed);
            _hotkeyManager.RegisterAction(HotkeyActions.EnableFreeCam, () =>
            {
                if (!AreOptionsEnabled) return;
                IsFreeCamEnabled = !IsFreeCamEnabled;
            });
            _hotkeyManager.RegisterAction(HotkeyActions.MoveCamToPlayer, MoveCamToPlayer);
        }

        private void OpenShop(int[] shopParams) =>
            _ezStateService.ExecuteTalkCommand(EzState.TalkCommands.OpenRegularShop(shopParams[0], shopParams[1]));

        private void OnGameLoaded()
        {
            if (IsHitboxEnabled) _utilityService.ToggleHitboxView(true);
            if (IsSoundViewEnabled) _utilityService.ToggleSoundView(true);

            if (IsHideMapEnabled) _utilityService.ToggleGroupMask(GroupMask.Map, true);
            if (IsHideObjectsEnabled) _utilityService.ToggleGroupMask(GroupMask.Obj, true);
            if (IsHideCharactersEnabled) _utilityService.ToggleGroupMask(GroupMask.Chr, true);
            if (IsHideSfxEnabled) _utilityService.ToggleGroupMask(GroupMask.Sfx, true);
            if (IsDrawLowHitEnabled)
            {
                IsHideMapEnabled = true;
                _utilityService.ToggleHitIns(HitFlags.LowHit, true);
            }

            if (IsDrawHighHitEnabled)
            {
                IsHideMapEnabled = true;
                _utilityService.ToggleHitIns(HitFlags.HighHit, true);
            }

            if (IsDrawChrRagdollEnabled)
            {
                _debugDrawService.RequestDebugDraw();
                _utilityService.ToggleHitIns(HitFlags.ChrRagdoll, true);
            }

            GameSpeed = _utilityService.GetGameSpeed();
            CameraFov = _utilityService.GetFov();
            AreOptionsEnabled = true;
        }

        private void OnGameNotLoaded()
        {
            IsNoClipEnabled = false;
            IsDeathCamEnabled = false;
            IsFreeCamEnabled = false;
            IsFreezeWorldEnabled = false;
            AreOptionsEnabled = false;
        }

        private void FirstLoaded()
        {
            if (IsCamVertIncreaseEnabled) _utilityService.ToggleCamVertIncrease(true);
            if (Is100DropEnabled) _utilityService.Toggle100Drop(true);
            if (IsDbgFpsEnabled)
            {
                _utilityService.ToggleDbgFps(true);
                _utilityService.SetFps(Fps);
            }

            if (IsFullLineUpEnabled) _utilityService.ToggleFullLineUp(true);
        }

        #endregion
    }
}