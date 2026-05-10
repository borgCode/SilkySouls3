using System;
using System.Windows.Input;
using SilkySouls3.Core;
using SilkySouls3.Data;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;
using SilkySouls3.Utilities;
using static SilkySouls3.Memory.Offsets;

namespace SilkySouls3.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        private readonly IEnemyService _enemyService;
        private readonly ICinderService _cinderService;
        private readonly HotkeyManager _hotkeyManager;
        private readonly IParamService _paramService;
        private readonly IDebugDrawService _debugDrawService;
        private readonly IChrInsService _chrInsService;
        private readonly ISpEffectService _spEffectService;
        private readonly IEventService _eventService;
        private readonly IReminderService _reminderService;
        private readonly IItemService _itemService;

        private bool _isFadedIn;
        private bool _isInHalfLightArena;
        private bool _isInCathedralOfTheDeep;
        private bool _hasPlacedPrismStonesThisLoad;
        private const int HalfLightBlockId = 0x33000000;
        private const int KilnBlockId = 0x29000000;
        private const int BarracksBlockId = 0x1E010000;
        private const int IrithyllBlockId = 0x25000000;
        private const int CathedralOfTheDeepBlockId = 0x23000000;

        private const int DeaconsBossGaugeId = 905220;

        public const int ArchDeaconEntityId = 3500800;
        public const int GlowingDeaconSpEffectId = 11521;

        private const int OceirosBossGaugeId = 902090;
        private const int OceirosPhaseTransitionAnimId = 1500;
        private const int OceirosEntityId = 3000830;

        private const int KingOfTheStormGaugeId = 905030;
        private const int KingOfTheStormEntityId = 3200850;

        public EnemyViewModel(IEnemyService enemyService, ICinderService cinderService, HotkeyManager hotkeyManager,
            IStateService stateService, IParamService paramService, IDebugDrawService debugDrawService,
            IChrInsService chrInsService, ISpEffectService spEffectService, IEventService eventService,
            IReminderService reminderService, IItemService itemService)
        {
            _enemyService = enemyService;
            _cinderService = cinderService;
            _debugDrawService = debugDrawService;
            _chrInsService = chrInsService;
            _spEffectService = spEffectService;
            _eventService = eventService;
            _reminderService = reminderService;
            _itemService = itemService;

            SetCinderPhaseCommand = new DelegateCommand<CinderPhase>(SetCinderPhase);
            CastSoulmassCommand = new DelegateCommand(CastSoulmass);
            RemoveSoulmassCommand = new DelegateCommand(RemoveSoulmass);
            PlacePrismStonesCommand = new DelegateCommand(PlacePrismStones);
            DeaconsPhase2Command = new DelegateCommand(DeaconsPhase2);
            DeaconsPhase2WithMoveCommand = new DelegateCommand(DeaconsPhase2WithMove);
            ForceOceirosPhaseTwoCommand = new DelegateCommand(ForceOceirosPhaseTwo);
            SkipKingOfTheStormCommand = new DelegateCommand(SkipKingOfTheStorm);
            DropDeaconSkullsCommand = new DelegateCommand(DropDeaconSkulls);

            _hotkeyManager = hotkeyManager;
            _paramService = paramService;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe(State.FadedIn, OnFadedIn);
            stateService.Subscribe<int>(State.BlockChanged,
                blockId =>
                {
                    _isInHalfLightArena = HalfLightBlockId == blockId;
                    _isInCathedralOfTheDeep = CathedralOfTheDeepBlockId == blockId;
                    AreCinderOptionsEnabled = KilnBlockId == blockId;
                    AreDsaOptionsEnabled = BarracksBlockId == blockId;
                    ArePontiffOptionsEnabled = IrithyllBlockId == blockId;
                    if (_isFadedIn && ArePontiffOptionsEnabled && IsPontiffNoCloneEnabled)
                        _enemyService.TogglePontiffNoClone(true);
                    OnPropertyChanged(nameof(CanPlacePrismStones));
                    OnPropertyChanged(nameof(CanDropDeaconSkulls));
                    if (!AreDsaOptionsEnabled)
                    {
                        SelectedLeftButterflyAnimation = 0;
                        SelectedRightButterflyAnimation = 0;
                    }
                });

            stateService.Subscribe<int>(State.BossFight,
                gaugeId =>
                {
                    AreDeaconsOptionsEnabled = gaugeId == DeaconsBossGaugeId;
                    AreOceirosOptionsEnabled = gaugeId == OceirosBossGaugeId;
                    AreKingOfTheStormOptionsEnabled = gaugeId == KingOfTheStormGaugeId;
                }
            );


            RegisterHotkeys();
        }

        #region Commands

        public ICommand SetCinderPhaseCommand { get; }
        public ICommand CastSoulmassCommand { get; }
        public ICommand RemoveSoulmassCommand { get; }
        public ICommand PlacePrismStonesCommand { get; }
        public ICommand DeaconsPhase2Command { get; }
        public ICommand DeaconsPhase2WithMoveCommand { get; }
        public ICommand ForceOceirosPhaseTwoCommand { get; }
        public ICommand SkipKingOfTheStormCommand { get; }
        public ICommand DropDeaconSkullsCommand { get; }

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
                if (!SetProperty(ref _isAllDisableAiEnabled, value)) return;
                if (_isAllDisableAiEnabled) _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoUpdate, _isAllDisableAiEnabled);
            }
        }

        private bool _isAllNoDamageEnabled;

        public bool IsAllNoDamageEnabled
        {
            get => _isAllNoDamageEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDamageEnabled, value))
                    _enemyService.ToggleDebugFlag(DebugFlags.AllNoDamage, _isAllNoDamageEnabled);
            }
        }

        private bool _isAllNoMoveEnabled;

        public bool IsAllNoMoveEnabled
        {
            get => _isAllNoMoveEnabled;
            set
            {
                if (!SetProperty(ref _isAllNoMoveEnabled, value)) return;
                if (_isAllNoMoveEnabled) _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoMove, _isAllNoMoveEnabled);
            }
        }

        private bool _isAllNoAttackEnabled;

        public bool IsAllNoAttackEnabled
        {
            get => _isAllNoAttackEnabled;
            set
            {
                if (!SetProperty(ref _isAllNoAttackEnabled, value)) return;
                if (_isAllNoAttackEnabled) _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoAttack, _isAllNoAttackEnabled);
            }
        }

        private bool _isAllNoDeathEnabled;

        public bool IsAllNoDeathEnabled
        {
            get => _isAllNoDeathEnabled;
            set
            {
                if (SetProperty(ref _isAllNoDeathEnabled, value))
                    _enemyService.ToggleDebugFlag(DebugFlags.AllNoDeath, _isAllNoDeathEnabled);
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

        private bool _isDrawNavigationEnabled;

        public bool IsDrawNavigationEnabled
        {
            get => _isDrawNavigationEnabled;
            set
            {
                if (!SetProperty(ref _isDrawNavigationEnabled, value)) return;
                if (value)
                    _debugDrawService.RequestDebugDraw();
                else
                    _debugDrawService.ReleaseDebugDraw();

                _enemyService.ToggleDrawNavigation(_isDrawNavigationEnabled);
            }
        }

        private bool _areCinderOptionsEnabled;

        public bool AreCinderOptionsEnabled
        {
            get => _areCinderOptionsEnabled;
            set => SetProperty(ref _areCinderOptionsEnabled, value);
        }

        private bool _areDeaconsOptionsEnabled;

        public bool AreDeaconsOptionsEnabled
        {
            get => _areDeaconsOptionsEnabled;
            set => SetProperty(ref _areDeaconsOptionsEnabled, value);
        }

        private bool _arePontiffOptionsEnabled;

        public bool ArePontiffOptionsEnabled
        {
            get => _arePontiffOptionsEnabled;
            set => SetProperty(ref _arePontiffOptionsEnabled, value);
        }

        private bool _isPontiffNoCloneEnabled;

        public bool IsPontiffNoCloneEnabled
        {
            get => _isPontiffNoCloneEnabled;
            set
            {
                if (!SetProperty(ref _isPontiffNoCloneEnabled, value) || !ArePontiffOptionsEnabled) return;
                if (_isPontiffNoCloneEnabled) _reminderService.TrySetReminder();
                _enemyService.TogglePontiffNoClone(_isPontiffNoCloneEnabled);
            }
        }

        private bool _areOceirosOptionsEnabled;

        public bool AreOceirosOptionsEnabled
        {
            get => _areOceirosOptionsEnabled;
            set => SetProperty(ref _areOceirosOptionsEnabled, value);
        }

        private bool _areKingOfTheStormOptionsEnabled;

        public bool AreKingOfTheStormOptionsEnabled
        {
            get => _areKingOfTheStormOptionsEnabled;
            set => SetProperty(ref _areKingOfTheStormOptionsEnabled, value);
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

        private bool _areDsaOptionsEnabled;

        public bool AreDsaOptionsEnabled
        {
            get => _areDsaOptionsEnabled;
            set => SetProperty(ref _areDsaOptionsEnabled, value);
        }

        private int _selectedLeftButterflyAnimation;

        public int SelectedLeftButterflyAnimation
        {
            get => _selectedLeftButterflyAnimation;
            set
            {
                if (!SetProperty(ref _selectedLeftButterflyAnimation, value)) return;
                if (_selectedLeftButterflyAnimation != 0) _reminderService.TrySetReminder();
                ApplyButterflyRng();
            }
        }

        private int _selectedRightButterflyAnimation;

        public int SelectedRightButterflyAnimation
        {
            get => _selectedRightButterflyAnimation;
            set
            {
                if (!SetProperty(ref _selectedRightButterflyAnimation, value)) return;
                if (_selectedRightButterflyAnimation != 0) _reminderService.TrySetReminder();
                ApplyButterflyRng();
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

        public bool CanDropDeaconSkulls => _isInCathedralOfTheDeep;

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
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoMove,
                () => { IsAllNoMoveEnabled = !IsAllNoMoveEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllNoAttack,
                () => { IsAllNoAttackEnabled = !IsAllNoAttackEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.AllRepeatAct,
                () => { IsAllRepeatActEnabled = !IsAllRepeatActEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.EnemyTargetingView,
                () => { IsTargetingViewEnabled = !IsTargetingViewEnabled; });
            _hotkeyManager.RegisterAction(HotkeyActions.DrawNavigation,
                () => { IsDrawNavigationEnabled = !IsDrawNavigationEnabled; });
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
            _hotkeyManager.RegisterAction(HotkeyActions.PlacePrismStones,
                () =>
                {
                    if (CanPlacePrismStones) PlacePrismStones();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.PontiffNoClone,
                () =>
                {
                    if (ArePontiffOptionsEnabled) IsPontiffNoCloneEnabled = !IsPontiffNoCloneEnabled;
                });
            _hotkeyManager.RegisterAction(HotkeyActions.OceirosPhaseTwo,
                () =>
                {
                    if (AreOceirosOptionsEnabled) ForceOceirosPhaseTwo();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.SkipKingOfTheStorm,
                () =>
                {
                    if (AreKingOfTheStormOptionsEnabled) SkipKingOfTheStorm();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.DeaconsPhaseTwo,
                () =>
                {
                    if (AreDeaconsOptionsEnabled) DeaconsPhase2();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.DeaconsPhaseTwoWithMove,
                () =>
                {
                    if (AreDeaconsOptionsEnabled) DeaconsPhase2WithMove();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.DropDeaconSkulls,
                () =>
                {
                    if (CanDropDeaconSkulls) DropDeaconSkulls();
                });
            _hotkeyManager.RegisterAction(HotkeyActions.CycleLeftButterflyAnimation,
                () =>
                {
                    if (AreDsaOptionsEnabled)
                        SelectedLeftButterflyAnimation =
                            (SelectedLeftButterflyAnimation + 1) % ButterflyData.AnimationIds.Length;
                });
            _hotkeyManager.RegisterAction(HotkeyActions.CycleRightButterflyAnimation,
                () =>
                {
                    if (AreDsaOptionsEnabled)
                        SelectedRightButterflyAnimation =
                            (SelectedRightButterflyAnimation + 1) % ButterflyData.AnimationIds.Length;
                });
        }

        private void ForceDeaconsPhase2HpAndFlags()
        {
            var archDeacon = _chrInsService.GetChrInsByEntityId(ArchDeaconEntityId);
            var maxHp = _chrInsService.GetMaxHp(archDeacon);

            _chrInsService.SetHp(archDeacon, maxHp * 65 / 100 - 1);
            _eventService.BatchSetEvent(DeaconData.PossessionEventFlags, false);
        }

        private void DeaconsPhase2()
        {
            ForceDeaconsPhase2HpAndFlags();

            foreach (var id in DeaconData.EntityIds)
            {
                var deacon = _chrInsService.GetChrInsByEntityId(id);
                _spEffectService.RemoveSpEffect(deacon, GlowingDeaconSpEffectId);
            }
        }

        private void DeaconsPhase2WithMove()
        {
            ForceDeaconsPhase2HpAndFlags();

            for (var i = 0; i < DeaconData.EntityIds.Length; i++)
            {
                var deacon = _chrInsService.GetChrInsByEntityId(DeaconData.EntityIds[i]);
                _chrInsService.ToggleFreezeAi(deacon, true);
                _spEffectService.RemoveSpEffect(deacon, GlowingDeaconSpEffectId);
                _chrInsService.ForceSetPosition(deacon, DeaconData.Positions[i]);
                _chrInsService.ToggleFreezeAi(deacon, false);
            }
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

        private void DropDeaconSkulls() => _itemService.DropItem(DeaconData.SkullDrop);

        private void OnGameLoaded()
        {
            if (IsAllDisableAiEnabled)
            {
                _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoUpdate, true);
            }

            if (IsAllNoDamageEnabled) _enemyService.ToggleDebugFlag(DebugFlags.AllNoDamage, true);
            if (IsAllNoDeathEnabled) _enemyService.ToggleDebugFlag(DebugFlags.AllNoDeath, true);
            if (IsAllNoMoveEnabled)
            {
                _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoMove, true);
            }

            if (IsAllNoAttackEnabled)
            {
                _reminderService.TrySetReminder();
                _enemyService.ToggleDebugFlag(DebugFlags.AllNoAttack, true);
            }

            if (IsAllRepeatActEnabled) _enemyService.ToggleAllRepeatAct(true);
            if (IsTargetingViewEnabled)
            {
                _debugDrawService.RequestDebugDraw();
                _enemyService.ToggleTargetingView(true);
            }

            if (IsDrawNavigationEnabled)
            {
                _debugDrawService.RequestDebugDraw();
                _enemyService.ToggleDrawNavigation(true);
            }

            if (IsCinderPhasedLocked) _cinderService.ToggleCinderPhaseLock(true);

            if (Math.Abs(_argoSpeedMultiplier - 1.0f) > float.Epsilon) ApplyArgoSpeed();
            AreOptionsEnabled = true;
        }

        private void OnFadedIn()
        {
            _isFadedIn = true;
            if (IsPontiffNoCloneEnabled && ArePontiffOptionsEnabled)
                _enemyService.TogglePontiffNoClone(true);
        }

        private void ApplyButterflyRng()
        {
            var leftId = ButterflyData.AnimationIds[_selectedLeftButterflyAnimation];
            var rightId = ButterflyData.AnimationIds[_selectedRightButterflyAnimation];
            var enable = leftId != 0f || rightId != 0f;

            _enemyService.ToggleButterflyRng(enable);
            if (!enable) return;

            _enemyService.SetLeftButterflyAttack(leftId);
            _enemyService.SetRightButterflyAttack(rightId);
        }

        private void ApplyArgoSpeed()
        {
            foreach (var (rowId, vanilla) in ArgoData.TalkRows)
            {
                var row = _paramService.GetParamRow((int)Param.TalkParam, rowId);
                if (row == IntPtr.Zero) continue;
                _paramService.Write(row, ArgoData.TalkParamSpeedOffset, vanilla / _argoSpeedMultiplier);
            }
        }

        private void OnGameNotLoaded()
        {
            AreCinderOptionsEnabled = false;
            AreDeaconsOptionsEnabled = false;
            AreDsaOptionsEnabled = false;
            ArePontiffOptionsEnabled = false;
            AreKingOfTheStormOptionsEnabled = false;
            AreOptionsEnabled = false;
            _isFadedIn = false;
            _isInHalfLightArena = false;
            _isInCathedralOfTheDeep = false;
            _hasPlacedPrismStonesThisLoad = false;
            OnPropertyChanged(nameof(CanPlacePrismStones));
            OnPropertyChanged(nameof(CanDropDeaconSkulls));
        }

        private void ForceOceirosPhaseTwo()
        {
            var oceiros = _chrInsService.GetChrInsByEntityId(OceirosEntityId);
            _chrInsService.RequestEventAnimation(oceiros, OceirosPhaseTransitionAnimId);
        }

        private void SkipKingOfTheStorm()
        {
            var kots = _chrInsService.GetChrInsByEntityId(KingOfTheStormEntityId);
            _chrInsService.SetHp(kots, 0);
        }

        #endregion
    }
}