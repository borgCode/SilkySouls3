using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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
        private readonly IChrInsService _chrInsService;
        private readonly ISpEffectService _spEffectService;
        private readonly IEventService _eventService;
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
        private const int CathedralOfTheDeepBlockId = 0x23000000;

        public const int ArchDeaconEntityId = 3500800;
        public const int GlowingDeaconSpEffectId = 11521;

        private static readonly int[] DeaconEntityIds =
        [
            3500810, 3500811, 3500812, 3500813, 3500814, 3500815, 3500816, 3500817, 3500818, 3500819,
            3500820, 3500821, 3500822, 3500823, 3500824, 3500825, 3500826, 3500827, 3500828, 3500834, 3500835, 
            3500836, 3500837, 3500838, 3500839, 3500840, 3500841, 3500842,
        ];
        
        public static readonly Vector3[] DeaconPositions =
        [
            new(-500.325f, -197.0186f, -644.5127f),
            new(-494.1169f, -197.019f, -648.8835f),
            new(-505.5398f, -196.9706f, -646.1632f),
            new(-497.2601f, -197.0191f, -642.5104f),
            new(-496.4046f, -197.0151f, -643.2489f),
            new(-498.8778f, -197.0181f, -652.1129f),
            new(-494.6716f, -196.9687f, -642.7067f),
            new(-504.1354f, -196.9857f, -645.472f),
            new(-505.4075f, -196.9684f, -648.8774f),
            new(-506.8764f, -196.729f, -648.8887f),
            new(-501.0263f, -197.0184f, -646.4899f),
            new(-496.0489f, -196.9697f, -640.0248f),
            new(-495.8222f, -197.0192f, -644.7987f),
            new(-500.9214f, -197.0184f, -645.7462f),
            new(-496.7068f, -196.9684f, -652.5776f),
            new(-504.675f, -196.8416f, -650.919f),
            new(-499.6657f, -197.0003f, -653.0974f),
            new(-505.7625f, -196.9681f, -647.0373f),
            new(-497.1692f, -197.0191f, -643.4066f),
            new(-504.4316f, -196.9681f, -643.8191f),
            new(-492.0796f, -196.9694f, -644.3951f),
            new(-496.9165f, -197.0184f, -651.6621f),
            new(-502.5919f, -197.0181f, -647.851f),
            new(-495.4756f, -197.0186f, -651.1453f),
            new(-504.0388f, -196.9671f, -651.3212f),
            new(-501.8805f, -197.0181f, -650.9636f),
            new(-501.434f, -197.0182f, -647.2271f),
            new(-498.639f, -196.9692f, -639.585f)
        ];
        
        private static readonly List<int> DeaconPossessionEventFlags =
            Enumerable.Range(13504810, 33)
                .Concat(Enumerable.Range(13504850, 33))
                .ToList();


        public EnemyViewModel(IEnemyService enemyService, ICinderService cinderService, HotkeyManager hotkeyManager,
            IStateService stateService, IParamService paramService, IDebugDrawService debugDrawService,
            IChrInsService chrInsService, ISpEffectService spEffectService, IEventService eventService)
        {
            _enemyService = enemyService;
            _cinderService = cinderService;
            _debugDrawService = debugDrawService;
            _chrInsService = chrInsService;
            _spEffectService = spEffectService;
            _eventService = eventService;

            SetCinderPhaseCommand = new DelegateCommand<CinderPhase>(SetCinderPhase);
            CastSoulmassCommand = new DelegateCommand(CastSoulmass);
            RemoveSoulmassCommand = new DelegateCommand(RemoveSoulmass);
            PlacePrismStonesCommand = new DelegateCommand(PlacePrismStones);
            DeaconsPhase2Command = new DelegateCommand(DeaconsPhase2);
            DeaconsPhase2WithMoveCommand = new DelegateCommand(DeaconsPhase2WithMove);

            _hotkeyManager = hotkeyManager;
            _paramService = paramService;

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
            stateService.Subscribe<int>(State.BlockChanged,
                blockId =>
                {
                    _isInHalfLightArena = HalfLightBlockId == blockId;
                    AreCinderOptionsEnabled = KilnBlockId == blockId;
                    AreDeaconsOptionsEnabled = CathedralOfTheDeepBlockId == blockId;
                    OnPropertyChanged(nameof(CanPlacePrismStones));
                });

            RegisterHotkeys();
        }

        #region Commands

        public ICommand SetCinderPhaseCommand { get; }
        public ICommand CastSoulmassCommand { get; }
        public ICommand RemoveSoulmassCommand { get; }
        public ICommand PlacePrismStonesCommand { get; }
        public ICommand DeaconsPhase2Command { get; }
        public ICommand DeaconsPhase2WithMoveCommand { get; }

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

        private bool _areDeaconsOptionsEnabled;

        public bool AreDeaconsOptionsEnabled
        {
            get => _areDeaconsOptionsEnabled;
            set => SetProperty(ref _areDeaconsOptionsEnabled, value);
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

        private void ForceDeaconsPhase2HpAndFlags()
        {
            var archDeacon = _chrInsService.ChrInsByEntityId(ArchDeaconEntityId);
            var maxHp = _chrInsService.GetMaxHp(archDeacon);

            _chrInsService.SetHp(archDeacon, maxHp * 65 / 100 - 1);
            _eventService.BatchSetEvent(DeaconPossessionEventFlags, false);
        }

        private void DeaconsPhase2()
        {
            ForceDeaconsPhase2HpAndFlags();

            foreach (var id in DeaconEntityIds)
            {
                var deacon = _chrInsService.ChrInsByEntityId(id);
                _spEffectService.RemoveSpEffect(deacon, GlowingDeaconSpEffectId);
            }
        }

        private async void DeaconsPhase2WithMove()
        {
            ForceDeaconsPhase2HpAndFlags();
            
            await Task.Delay(250);

            for (var pass = 0; pass < 3; pass++)
            {
                for (var i = 0; i < DeaconEntityIds.Length; i++)
                {
                    var deacon = _chrInsService.ChrInsByEntityId(DeaconEntityIds[i]);

                    _spEffectService.RemoveSpEffect(deacon, GlowingDeaconSpEffectId);
                    _chrInsService.ForceSetPosition(deacon, DeaconPositions[i]);
                }

                await Task.Delay(50);
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
            AreDeaconsOptionsEnabled = false;
            AreOptionsEnabled = false;
            _isInHalfLightArena = false;
            _hasPlacedPrismStonesThisLoad = false;
            OnPropertyChanged(nameof(CanPlacePrismStones));
        }

        #endregion
    }
}