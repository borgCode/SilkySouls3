using System.Windows.Input;
using System.Windows.Media;
using SilkySouls3.Core;
using SilkySouls3.Enums;
using SilkySouls3.GameIds;
using SilkySouls3.Interfaces;

namespace SilkySouls3.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        private readonly IEventService _eventService;
        private readonly IDlcService _dlcService;

        public EventViewModel(IEventService eventService, IStateService stateService, IDlcService dlcService)
        {
            _eventService = eventService;
            _dlcService = dlcService;

            SetFlagCommand = new DelegateCommand(SetFlag);
            GetEventCommand = new DelegateCommand(GetEvent);
            UnlockMidirCommand = new DelegateCommand(UnlockMidir);
            MoveToFirelinkCommand = new DelegateCommand<int>(MoveToFirelink);
            MovePatchesToFirelinkCommand = new DelegateCommand(MovePatchesToFirelink);

            stateService.Subscribe(State.Loaded, OnGameLoaded);
            stateService.Subscribe(State.NotLoaded, OnGameNotLoaded);
        }

        #region Commands

        public ICommand SetFlagCommand { get; }
        public ICommand GetEventCommand { get; }
        public ICommand UnlockMidirCommand { get; }
        public ICommand MoveToFirelinkCommand { get; }
        public ICommand MovePatchesToFirelinkCommand { get; }

        #endregion

        #region Properties
        
        private bool _areOptionsEnabled;

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }
        
        private bool _isDlc2Available;

        public bool IsDlc2Available
        {
            get => _isDlc2Available;
            set => SetProperty(ref _isDlc2Available, value);
        }

        private string _setFlagId;

        public string SetFlagId
        {
            get => _setFlagId;
            set => SetProperty(ref _setFlagId, value);
        }

        private int _flagStateIndex;

        public int FlagStateIndex
        {
            get => _flagStateIndex;
            set => SetProperty(ref _flagStateIndex, value);
        }

        private string _getFlagId;

        public string GetFlagId
        {
            get => _getFlagId;
            set => SetProperty(ref _getFlagId, value);
        }

        private string _eventStatusText;

        public string EventStatusText
        {
            get => _eventStatusText;
            set => SetProperty(ref _eventStatusText, value);
        }

        private Brush _eventStatusColor;

        public Brush EventStatusColor
        {
            get => _eventStatusColor;
            set => SetProperty(ref _eventStatusColor, value);
        }

        
        private bool _isDisableEventsEnabled;

        public bool IsDisableEventsEnabled
        {
            get => _isDisableEventsEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventsEnabled, value)) return;
                _eventService.ToggleDisableEvents(_isDisableEventsEnabled);
            }
        }

        private bool _isDrawEventsEnabled;

        public bool IsDrawEventsEnabled
        {
            get => _isDrawEventsEnabled;
            set
            {
                if (!SetProperty(ref _isDrawEventsEnabled, value)) return;
                _eventService.ToggleDrawEvents(_isDrawEventsEnabled);
            }
        }

        #endregion

        #region Private Methods

        private void SetFlag()
        {
            if (string.IsNullOrWhiteSpace(SetFlagId))
                return;

            if (!int.TryParse(SetFlagId.Trim(), out int flagIdValue) || flagIdValue <= 0)
                return;
            _eventService.SetEvent(flagIdValue, FlagStateIndex == 0);
        }

        private void GetEvent()
        {
            if (string.IsNullOrWhiteSpace(GetFlagId))
                return;

            if (!int.TryParse(GetFlagId.Trim(), out int flagIdValue) || flagIdValue <= 0)
                return;

            if (_eventService.GetEvent(flagIdValue))
            {
                EventStatusText = "True";
                EventStatusColor = Brushes.Chartreuse;
            }
            else
            {
                EventStatusText = "False";
                EventStatusColor = Brushes.Red;
            }
        }

        private void UnlockMidir() => _eventService.SetEvent(EventFlag.UnlockMidir, true);

        private void MoveToFirelink(int eventId) => _eventService.SetEvent(eventId, true);

        private void MovePatchesToFirelink()
        {
            foreach (var id in EventFlag.Patches) _eventService.SetEvent(id, true);
        }

        private void OnGameNotLoaded()
        {
            AreOptionsEnabled = false;
        }

        private void OnGameLoaded()
        {
            AreOptionsEnabled = true;
            if (IsDisableEventsEnabled) _eventService.ToggleDisableEvents(true);
            if (IsDrawEventsEnabled) _eventService.ToggleDrawEvents(true);
            IsDlc2Available = _dlcService.IsDlc2Available;
        }

        #endregion
    }
}
