namespace Preferences.DTO
{
    public class AlertPreferenceDTO
    {
        public string UserOid { get; private set; }
        public string PreferenceOid { get; set; }
        public string AreaDesc { get; set; }
        public bool EmailNotification { get; set; }
        public bool InAppNotification { get; set; }
        public AlertPreferenceDTO() {
            UserOid = string.Empty;
            AreaDesc = string.Empty;
            PreferenceOid = string.Empty;
        }
        public AlertPreferenceDTO(string UserOid)
        {
            this.UserOid = UserOid;
            PreferenceOid = string.Empty;
            AreaDesc = string.Empty;
        }
        public AlertPreferenceDTO(string areaDesc,  bool emailNotification, bool inAppNotification)
        {
            PreferenceOid = string.Empty;
            AreaDesc = string.Empty;
            UserOid = string.Empty;
            AreaDesc = areaDesc;
            EmailNotification = emailNotification;
            InAppNotification = inAppNotification;
        }
    }
}
