#nullable enable

namespace NeuroSdk
{
    public static class NeuroSdkStrings
    {
        public static readonly NeuroSdkFormatString MessageHandlerFailedCaughtException = "Message handler failed with exception: {0}";
        public const string VedalFaultSuffix = " (This is probably not your fault, blame Vedal.)";
        public const string ModFaultSuffix = " (This is probably not your fault, blame the game integration.)";

        public const string ActionFailedNoData = "Action failed. Missing command data.";
        public const string ActionFailedNoId = "Action failed. Missing command field 'id'.";
        public const string ActionFailedNoName = "Action failed. Missing command field 'name'.";
        public const string ActionFailedInvalidJson = "Action failed. Could not parse action parameters from JSON.";
        public const string ActionFailedUnregistered = "This action has been recently unregistered and can no longer be used.";
        public static readonly NeuroSdkFormatString ActionFailedUnknownAction = "Action failed. Unknown action '{0}'.";
        public static readonly NeuroSdkFormatString ActionFailedCaughtException = "Action failed. Caught exception: {0}";

        public static readonly NeuroSdkFormatString ActionFailedMissingRequiredParameter = "Action failed. Missing required '{0}' parameter.";
        public static readonly NeuroSdkFormatString ActionFailedInvalidParameter = "Action failed. Invalid '{0}' parameter.";
    }
}
