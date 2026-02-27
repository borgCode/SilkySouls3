using System;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Memory;
using SilkySouls3.Utilities;

namespace SilkySouls3.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private readonly IEnemyService _enemyService;
        private readonly ICinderService _cinderService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IParamService _paramService;
        private readonly IDebugDrawService _debugDrawService;
        private static readonly float[] ButterflyAnimationIds = { 0f, 3000f, 3001f, 3002f };

        private const int TalkParamSpeedOffset = 0x2C;

        private static readonly (uint RowId, float VanillaDuration)[] ArgoTalkRows =
        [
            (88000200, 10f),
            (88000201, 8f),
            (88000202, 9f),
            (88000203, 6f),
            (88000204, 3f),
            (88000600, 4f),
            (88000601, 9f)
        ];

        private bool _isInHalfLightArena;
        private bool _hasPlacedPrismStonesThisLoad;
        private const int HalfLightBlockId = 0x33000000;
        private const int KilnBlockId = 0x29000000;

        public EnemyViewModel(IEnemyService enemyService, ICinderService cinderService, HotkeyManager hotkeyManager,
            IStateService stateService, IParamService paramService, IDebugDrawService debugDrawService)
        {
            _enemyService = enemyService;
            _cinderService = cinderService;
            _debugDrawService = debugDrawService;

            SetCinderPhaseCommand = new DelegateCommand<CinderPhase>(SetCinderPhase);
            CastSoulmassCommand = new DelegateCommand(CastSoulmass);
            RemoveSoulmassCommand = new DelegateCommand(RemoveSoulmass);
            PlacePrismStonesCommand = new DelegateCommand(PlacePrismStones);

            _hotkeyManager = hotkeyManager;
            _paramService = paramService;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe<int>(State.BlockChanged,
                blockId =>
                {
                    _isInHalfLightArena = HalfLightBlockId == blockId;
                    AreCinderOptionsEnabled = KilnBlockId == blockId;
                    OnPropertyChanged(nameof(CanPlacePrismStones));
                });

            RegisterHotkeys();
        }

        #region Commands

        public ICommand SetCinderPhaseCommand { get; }
        public ICommand CastSoulmassCommand { get; }
        public ICommand RemoveSoulmassCommand { get; }
        public ICommand PlacePrismStonesCommand { get; }

        #endregion

        #region Properties

        private bool _areOptionsEnabled = true;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        private bool _isAllDisableAiEnabled;

        public bool IsAllDisableAiEnabled
        {
            get => _isAllDisableAiEnabled;
            set
            {
                if (SetProperty(ref _isAllDisableAiEnabled, value))
                    _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoUpdate, _isAllDisableAiEnabled);
            }
        }

        private bool _isAllNoDamageEnabled;

        public bool IsAllNoDamageEnabled
        {
            get => _isAllNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDamageEnabled, value))
                    _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoDamage, _isAllNoDamageEnabled);
            }
        }

        private bool _isAllNoDeathEnabled;

        public bool IsAllNoDeathEnabled
        {
            get => _isAllNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDeathEnabled, value))
                    _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoDeath, _isAllNoDeathEnabled);
            }
        }

        private bool _isAllRepeatActEnabled;

        public bool IsAllRepeatActEnabled
        {
            get => _isAllRepeatActEnabled;
            set
            {
                if (SetProperty(ref _isAllRepeatActEnabled, value))
                    _enemyService.ToggleAllRepeatAct(_isAllRepeatActEnabled);
            }
        }

        private bool _isTargetingViewEnabled;

        public bool IsTargetingViewEnabled
        {
            get => _isTargetingViewEnabled;
            set
            {
                if (!SetProperty(ref _isTargetingViewEnabled, value)) return;
                if (value)
                    _debugDrawService.RequestDebugDraw();
                else
                    _debugDrawService.ReleaseDebugDraw();

                _enemyService.ToggleTargetingView(_isTargetingViewEnabled);
            }
        }

        private bool _areCinderOptionsEnabled;

        public bool AreCinderOptionsEnabled
        {
            get => _areCinderOptionsEnabled;
            set => SetProperty(ref _areCinderOptionsEnabled, value);
        }

        private bool _isCinderPhaseLocked;

        public bool IsCinderPhasedLocked
        {
            get => _isCinderPhaseLocked;
            set
            {
                if (!SetProperty(ref _isCinderPhaseLocked, value)) return;
                _cinderService.ToggleCinderPhaseLock(_isCinderPhaseLocked);
            }
        }

        private bool _isEndlessSoulmassEnabled;

        public bool IsEndlessSoulmassEnabled
        {
            get => _isEndlessSoulmassEnabled;
            set
            {
                if (!SetProperty(ref _isEndlessSoulmassEnabled, value)) return;
                _cinderService.ToggleEndlessSoulmass(_isEndlessSoulmassEnabled);
            }
        }

        private bool _isCinderNoStaggerEnabled;

        public bool IsCinderNoStaggerEnabled
        {
            get => _isCinderNoStaggerEnabled;
            set
            {
                if (!SetProperty(ref _isCinderNoStaggerEnabled, value)) return;
                _cinderService.ToggleCinderStagger(_isCinderNoStaggerEnabled);
            }
        }

        private bool _isNoSoulmassRemoveOnStaggerEnabled;

        public bool IsNoSoulmassRemoveOnStaggerEnabled
        {
            get => _isNoSoulmassRemoveOnStaggerEnabled;
            set
            {
                if (!SetProperty(ref _isNoSoulmassRemoveOnStaggerEnabled, value)) return;
                _cinderService.ToggleNoSoulmassRemoveOnStagger(_isNoSoulmassRemoveOnStaggerEnabled);
            }
        }

        private bool _isButterflyRngEnabled;

        public bool IsButterflyRngEnabled
        {
            get => _isButterflyRngEnabled;
            set
            {
                // if (SetProperty(ref _isButterflyRngEnabled, value))
                //     _enemyService.ToggleButterflyRng(_isButterflyRngEnabled);
            }
        }

        private int _selectedLeftButterflyAnimation;

        public int SelectedLeftButterflyAnimation
        {
            get => _selectedLeftButterflyAnimation;
            set
            {
                // if (SetProperty(ref _selectedLeftButterflyAnimation, value))
                //     _enemyService.SetLeftButterflyAttack(ButterflyAnimationIds[value]);
            }
        }

        private int _selectedRightButterflyAnimation;

        public int SelectedRightButterflyAnimation
        {
            get => _selectedRightButterflyAnimation;
            set
            {
                // if (SetProperty(ref _selectedRightButterflyAnimation, value))
                //     _enemyService.SetRightButterflyAttack(ButterflyAnimationIds[value]);
            }
        }

        private float _argoSpeedMultiplier = 1.0f;

        public float ArgoSpeedMultiplier
        {
            get => _argoSpeedMultiplier;
            set
            {
                if (SetProperty(ref _argoSpeedMultiplier, value))
                    ApplyArgoSpeed();
            }
        }

        public bool CanPlacePrismStones => _isInHalfLightArena && !_hasPlacedPrismStonesThisLoad;

        #endregion

        #region Private Methods

        private void RegisterHotkeys()
        {
            _hotkeyManager.RegisterAction(HotkeyActions.DisableAi,
                () => { IsAllDisableAiEnabled = !IsAllDisableAiEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDeath,
                () => { IsAllNoDeathEnabled = !IsAllNoDeathEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoDamage,
                () => { IsAllNoDamageEnabled = !IsAllNoDamageEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllRepeatAct,
                () => { IsAllRepeatActEnabled = !IsAllRepeatActEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.SetSwordPhase, () => SetCinderPhase(CinderPhase.Sword));
            _hotkeyManager.RegisterAction(HotkeyActions.SetLancePhase, () => SetCinderPhase(CinderPhase.Lance));
            _hotkeyManager.RegisterAction(HotkeyActions.SetCurvedPhase, () => SetCinderPhase(CinderPhase.Curved));
            _hotkeyManager.RegisterAction(HotkeyActions.SetStaffPhase, () => SetCinderPhase(CinderPhase.Staff));
            _hotkeyManager.RegisterAction(HotkeyActions.SetGwynPhase, () => SetCinderPhase(CinderPhase.Gwyn));
            _hotkeyManager.RegisterAction(HotkeyActions.PhaseLock, () => IsCinderPhasedLocked = !IsCinderPhasedLocked);
            _hotkeyManager.RegisterAction(HotkeyActions.CastSoulmass, CastSoulmass);
            _hotkeyManager.RegisterAction(HotkeyActions.RemoveSoulmass, RemoveSoulmass);
            _hotkeyManager.RegisterAction(HotkeyActions.EndlessSoulmass,
                () => IsEndlessSoulmassEnabled = !IsEndlessSoulmassEnabled);
            _hotkeyManager.RegisterAction(HotkeyActions.CinderNoStagger,
                () => IsCinderNoStaggerEnabled = !IsCinderNoStaggerEnabled);
        }

        private void SetCinderPhase(CinderPhase phase) => _cinderService.ForcePhaseTransition(phase);

        private void CastSoulmass() => _cinderService.CastSoulMass();

        private void RemoveSoulmass() => _cinderService.RemoveSoulmass();

        private void PlacePrismStones()
        {
            _enemyService.PlacePrismStones();
            _hasPlacedPrismStonesThisLoad = true;
            OnPropertyChanged(nameof(CanPlacePrismStones));
        }

        private void OnGameLoaded()
        {
            if (IsAllDisableAiEnabled) _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoUpdate, true);
            if (IsAllNoDamageEnabled) _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoDamage, true);
            if (IsAllNoDeathEnabled) _enemyService.ToggleDebugFlag(Offsets.DebugFlags.AllNoDeath, true);
            if (IsAllRepeatActEnabled) _enemyService.ToggleAllRepeatAct(true);
            if (IsTargetingViewEnabled)
            {
                _debugDrawService.RequestDebugDraw();
                _enemyService.ToggleTargetingView(true);
            }
            if (IsButterflyRngEnabled)
            {
                // _enemyService.ToggleButterflyRng(true);
                // _enemyService.SetLeftButterflyAttack(ButterflyAnimationIds[SelectedLeftButterflyAnimation]);
                // _enemyService.SetRightButterflyAttack(ButterflyAnimationIds[SelectedRightButterflyAnimation]);
            }

            if (Math.Abs(_argoSpeedMultiplier - 1.0f) > float.Epsilon) ApplyArgoSpeed();
            AreOptionsEnabled = true;
        }

        private void ApplyArgoSpeed()
        {
            foreach (var (rowId, vanilla) in ArgoTalkRows)
            {
                var row = _paramService.GetParamRow((int)Param.TalkParam, rowId);
                if (row == IntPtr.Zero) continue;
                _paramService.Write(row, TalkParamSpeedOffset, vanilla / _argoSpeedMultiplier);
            }
        }

        private void OnGameNotLoaded()
        {
            IsCinderPhasedLocked = false;
            IsCinderNoStaggerEnabled = false;
            IsEndlessSoulmassEnabled = false;
            IsNoSoulmassRemoveOnStaggerEnabled = false;
            AreCinderOptionsEnabled = false;
            AreOptionsEnabled = false;
            _isInHalfLightArena = false;
            _hasPlacedPrismStonesThisLoad = false;
            OnPropertyChanged(nameof(CanPlacePrismStones));
        }

        #endregion
    }
}