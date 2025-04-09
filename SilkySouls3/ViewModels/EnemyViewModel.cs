using System;
using System.Windows.Threading;
using SilkySouls3.Memory;
using SilkySouls3.Services;

namespace SilkySouls3.ViewModels
{
    public class EnemyViewModel : BaseViewModel
    {
        //TODO implement options enavbled
        private bool _areOptionsEnabled = true;
        private bool _isTargetOptionsEnabled;
        private bool _isValidTarget;
        private readonly DispatcherTimer _targetOptionsTimer;
        private int _targetCurrentHealth;
        private int _targetMaxHealth;
        private ulong _currentTargetId;
        private float _targetSpeed;
        private bool _isFreezeHealthEnabled;
        private bool _isDisableTargetAiEnabled;
        private bool _isRepeatActEnabled;

        private float _targetCurrentPoise;
        private float _targetMaxPoise;
        private float _targetPoiseTimer;
        private bool _showPoise;

        private int _targetCurrentBleed;
        private int _targetMaxBleed;
        private bool _showBleed;
        private bool _isBleedImmune;

        private int _targetCurrentPoison;
        private int _targetMaxPoison;
        private bool _showPoison;
        private bool _isPoisonImmune;

        private int _targetCurrentToxic;
        private int _targetMaxToxic;
        private bool _showToxic;
        private bool _isToxicImmune;

        private int _targetCurrentFrost;
        private int _targetMaxFrost;
        private bool _showFrost;
        private bool _isFrostImmune;

        private bool _showAllResistances;

        private readonly EnemyService _enemyService;

        public EnemyViewModel(EnemyService enemyService)
        {
            _enemyService = enemyService;

            _targetOptionsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(64)
            };
            _targetOptionsTimer.Tick += TargetOptionsTimerTick;
        }

        private void TargetOptionsTimerTick(object sender, EventArgs e)
        {
            // if (!IsTargetValid())
            // {
            //     IsValidTarget = false;
            //     return;
            // }
            //
            // IsValidTarget = true;
            TargetCurrentHealth = _enemyService.GetTargetHp();
            TargetMaxHealth = _enemyService.GetTargetMaxHp();
            //
            ulong targetId = _enemyService.GetTargetId();
            //
            if (targetId != _currentTargetId)
            {
                //     IsDisableTargetAiEnabled = _enemyService.IsTargetAiDisabled();
                //     IsRepeatActEnabled = IsCurrentTargetRepeating();
                IsFreezeHealthEnabled = _enemyService.IsTargetNoDamageEnabled();
                _currentTargetId = targetId;
                //     TargetMaxPoise = _enemyService.GetTargetMaxPoise();
                (IsPoisonImmune, IsToxicImmune, IsBleedImmune, IsFrostImmune) = _enemyService.GetImmunities();
                TargetMaxPoison = IsPoisonImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.PoisonMax);
                TargetMaxToxic = IsToxicImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.ToxicMax);
                TargetMaxBleed = IsBleedImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.BleedMax);
                TargetMaxFrost = IsFrostImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.FrostMax);

            }
            //
            // TargetSpeed = _enemyService.GetTargetSpeed();
            // TargetCurrentPoise = _enemyService.GetTargetPoise();
            // TargetPoiseTimer = _enemyService.GetTargetPoiseTimer();
            TargetCurrentPoison = IsPoisonImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.PoisonCurrent);
            TargetCurrentToxic = IsToxicImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.ToxicCurrent);
            TargetCurrentBleed = IsBleedImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.BleedCurrent);
            TargetCurrentFrost = IsFrostImmune ? 0 : _enemyService.GetTargetResistance(Offsets.WorldChrMan.ChrResistModule.FrostCurrent);
        }


        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        //TODO isvalidtarget

        public bool IsTargetOptionsEnabled
        {
            get => _isTargetOptionsEnabled;
            set
            {
                if (!SetProperty(ref _isTargetOptionsEnabled, value)) return;
                if (value)
                {
                    _enemyService.InstallTargetHook();
                    _targetOptionsTimer.Start();
                }
                else
                {
                    _targetOptionsTimer.Stop();
                    _enemyService.UninstallTargetHook();
                    // ShowPoise = false;
                    // ShowBleed = false;
                    // ShowPoison = false;
                    // ShowToxic = false;
                }
            }
        }


        public int TargetCurrentHealth
        {
            get => _targetCurrentHealth;
            set => SetProperty(ref _targetCurrentHealth, value);
        }

        public int TargetMaxHealth
        {
            get => _targetMaxHealth;
            set => SetProperty(ref _targetMaxHealth, value);
        }

        public void SetTargetHealth(int value)
        {
            int health = TargetMaxHealth * value / 100;
            _enemyService.SetTargetHp(health);
        }

        public bool IsFreezeHealthEnabled
        {
            get => _isFreezeHealthEnabled;
            set
            {
                SetProperty(ref _isFreezeHealthEnabled, value);
                _enemyService.ToggleTargetNoDamage(_isFreezeHealthEnabled);
            }
        }


        public float TargetCurrentPoise
        {
            get => _targetCurrentPoise;
            set => SetProperty(ref _targetCurrentPoise, value);
        }

        public float TargetMaxPoise
        {
            get => _targetMaxPoise;
            set => SetProperty(ref _targetMaxPoise, value);
        }

        public float TargetPoiseTimer
        {
            get => _targetPoiseTimer;
            set => SetProperty(ref _targetPoiseTimer, value);
        }

        public bool ShowPoise
        {
            get => _showPoise;
            set => SetProperty(ref _showPoise, value);
        }

        public int TargetCurrentBleed
        {
            get => _targetCurrentBleed;
            set => SetProperty(ref _targetCurrentBleed, value);
        }

        public int TargetMaxBleed
        {
            get => _targetMaxBleed;
            set => SetProperty(ref _targetMaxBleed, value);
        }

        public bool ShowBleed
        {
            get => _showBleed;
            set => SetProperty(ref _showBleed, value);
        }

        public bool IsBleedImmune
        {
            get => _isBleedImmune;
            set => SetProperty(ref _isBleedImmune, value);
        }

        public int TargetCurrentPoison
        {
            get => _targetCurrentPoison;
            set => SetProperty(ref _targetCurrentPoison, value);
        }

        public int TargetMaxPoison
        {
            get => _targetMaxPoison;
            set => SetProperty(ref _targetMaxPoison, value);
        }

        public bool ShowPoison
        {
            get => _showPoison;
            set => SetProperty(ref _showPoison, value);
        }

        public bool IsPoisonImmune
        {
            get => _isPoisonImmune;
            set => SetProperty(ref _isPoisonImmune, value);
        }

        public int TargetCurrentToxic
        {
            get => _targetCurrentToxic;
            set => SetProperty(ref _targetCurrentToxic, value);
        }

        public int TargetMaxToxic
        {
            get => _targetMaxToxic;
            set => SetProperty(ref _targetMaxToxic, value);
        }

        public bool ShowToxic
        {
            get => _showToxic;
            set => SetProperty(ref _showToxic, value);
        }

        public bool IsToxicImmune
        {
            get => _isToxicImmune;
            set => SetProperty(ref _isToxicImmune, value);
        }

        public int TargetCurrentFrost
        {
            get => _targetCurrentFrost;
            set => SetProperty(ref _targetCurrentFrost, value);
        }

        public int TargetMaxFrost
        {
            get => _targetMaxFrost;
            set => SetProperty(ref _targetMaxFrost, value);
        }

        public bool ShowFrost
        {
            get => _showFrost;
            set => SetProperty(ref _showFrost, value);
        }

        public bool IsFrostImmune
        {
            get => _isFrostImmune;
            set => SetProperty(ref _isFrostImmune, value);
        }
    }
}