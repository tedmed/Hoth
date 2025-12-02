namespace Preferences.DTO
{
    public class AlertPreferenceDTO
    {
        public string UserOid { get; private set; }
        public string PreferenceOid { get; set; }
        public string AreaDesc { get; set; }
        public string SpecificAreaDesc { get; set; }

        public bool EmailNotification { get; set; }
        public bool InAppNotification { get; set; }
        public AlertPreferenceDTO()
        {
            UserOid = string.Empty;
            AreaDesc = string.Empty;
            SpecificAreaDesc = string.Empty;
            PreferenceOid = string.Empty;
        }
        public AlertPreferenceDTO(string UserOid)
        {
            this.UserOid = UserOid;
            PreferenceOid = string.Empty;
            AreaDesc = string.Empty;
            SpecificAreaDesc = string.Empty;
        }
        public AlertPreferenceDTO(string areaDesc, string specificAreaDesc, bool emailNotification, bool inAppNotification)
        {
            PreferenceOid = string.Empty;
            AreaDesc = string.Empty;
            UserOid = string.Empty;
            AreaDesc = areaDesc;
            SpecificAreaDesc = specificAreaDesc;
            EmailNotification = emailNotification;
            InAppNotification = inAppNotification;
        }
    }
}
