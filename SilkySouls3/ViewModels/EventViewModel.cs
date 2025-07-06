using System.Windows.Media;
using SilkySouls3.Memory;
using SilkySouls3.Services;

namespace SilkySouls3.ViewModels
{
    public class EventViewModel : BaseViewModel
    {
        private readonly EventService _eventService;
        private bool _isDisableEventEnabled;
        private bool _isArgoSpeedEnabled;
        private float _argoDuration;
        private string _setFlagId;
        private int _flagStateIndex;
        private string _getFlagId;
        
        private string _eventStatusText;
        private Brush _eventStatusColor;

        private bool _areButtonsEnabled;

        public EventViewModel(EventService eventService)
        {
            _eventService = eventService;
            ArgoDuration = 2.0f;
        }
        
                
        // public bool IsDisableEventsEnabled
        // {
        //     get => _isDisableEventsEnabled;
        //     set
        //     {
        //         if (!SetProperty(ref _isDisableEventsEnabled, value)) return;
        //         _eventService.ToggleDisableEvents(_isDisableEventsEnabled);
        //     }
        // }
        
        public string SetFlagId
        {
            get => _setFlagId;
            set => SetProperty(ref _setFlagId, value);
        }

        public int FlagStateIndex
        {
            get => _flagStateIndex;
            set => SetProperty(ref _flagStateIndex, value);
        }

        public void SetFlag()
        {
            if (string.IsNullOrWhiteSpace(SetFlagId))
                return;
            
            string trimmedFlagId = SetFlagId.Trim();
        
            if (!ulong.TryParse(trimmedFlagId, out ulong flagIdValue) || flagIdValue <= 0)
                return;
            _eventService.SetEvent(flagIdValue, FlagStateIndex == 0 ? 1 : 0);
        }
        
        public string GetFlagId
        {
            get => _getFlagId;
            set => SetProperty(ref _getFlagId, value);
        }
        
        public string EventStatusText
        {
            get => _eventStatusText;
            set => SetProperty(ref _eventStatusText, value);
        }

        public Brush EventStatusColor
        {
            get => _eventStatusColor;
            set => SetProperty(ref _eventStatusColor, value);
        }

        public void GetEvent()
        {
            if (string.IsNullOrWhiteSpace(GetFlagId))
                return;
            
            string trimmedFlagId = GetFlagId.Trim();
            
            if (!ulong.TryParse(trimmedFlagId, out ulong flagIdValue) || flagIdValue <= 0)
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
        
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set => SetProperty(ref _areButtonsEnabled, value);
        }
        
        public bool IsDisableEventEnabled
        {
            get => _isDisableEventEnabled;
            set
            {
                if (!SetProperty(ref _isDisableEventEnabled, value)) return;
                _eventService.ToggleDisableEvent(_isDisableEventEnabled);
            }
        }
        
        public bool IsArgoSpeedEnabled
        {
            get => _isArgoSpeedEnabled;
            set
            {
                if (!SetProperty(ref _isArgoSpeedEnabled, value)) return;
                _eventService.ToggleArgoHook(_isArgoSpeedEnabled);
                if (IsArgoSpeedEnabled) _eventService.SetArgoSpeed(ArgoDuration);
                
            }
        }
        
        public float ArgoDuration
        {
            get => _argoDuration;
            set
            {
                if (SetProperty(ref _argoDuration, value))
                {
                    if (!IsArgoSpeedEnabled) return;
                    _eventService.SetArgoSpeed(value);
                }
            }
        }
        
        public void DisableFeatures()
        {
            AreButtonsEnabled = false;
        }
        

        public void TryEnableFeatures()
        {
            AreButtonsEnabled = true;
            if (IsDisableEventEnabled) _eventService.ToggleDisableEvent(true);
            if (IsArgoSpeedEnabled) _eventService.ToggleArgoHook(true);
        }
        
        public void UnlockMidir()
        {
            _eventService.SetEvent(GameIds.EventFlags.UnlockMidir, 1);
        }
        
        public void MovePatchesToFirelink()
        {
            _eventService.SetMultipleEventsOn(GameIds.EventFlags.Patches);
        }

        public void MoveGreiratToFirelink()
        {
           _eventService.SetEvent(GameIds.EventFlags.Greirat, 1);
        }

        public void MoveKarlaToFirelink()
        {
            _eventService.SetEvent(GameIds.EventFlags.Karla, 1);
        }

        public void MoveCornyxToFirelink()
        {
            _eventService.SetEvent(GameIds.EventFlags.Cornyx, 1);
        }

        public void MoveOrbeckToFirelink()
        {
            _eventService.SetEvent(GameIds.EventFlags.Orbeck, 1);
        }

        public void MoveIrinaToFirelink()
        {
            _eventService.SetEvent(GameIds.EventFlags.Irina, 1);
        }
    }
}