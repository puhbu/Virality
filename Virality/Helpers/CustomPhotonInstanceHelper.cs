using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virality.Helpers
{
    public static class CustomPhotonInstanceHelper
    {
        public const string RealtimeAppIdKey = "REALTIME_APP_ID";
        public const string VoiceAppIdKey = "VOICE_APPID";

        public static string? DefaultRealtimeAppId = null;
        public static string? DefaultVoiceAppId = null;

        public static bool AppIdMatches(string? realtimeAppId, string? voiceAppId)
        {
            return PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime == realtimeAppId && PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice == voiceAppId;
        }

        public static string? GetCustomVoiceAppId()
        {
            // just use voice app id if we have it
            if (CustomAppIdIsValid(Virality.CustomPhotonVoiceAppId?.Value))
            {
                return Virality.CustomPhotonVoiceAppId?.Value;
            }

            if (!CustomAppIdIsValid(Virality.CustomPhotonRealtimeAppId?.Value)) 
                return null;

            return Virality.CustomPhotonRealtimeAppId?.Value;
        }

        public static bool AppIsDefault()
        {
            return AppIdMatches(DefaultRealtimeAppId, DefaultVoiceAppId);
        }

        public static void SetToDefaultApp()
        {
            if (DefaultRealtimeAppId == null || DefaultVoiceAppId == null)
                return;

            SetAppId(DefaultRealtimeAppId, DefaultVoiceAppId);
        }

        public static bool CustomAppIdIsValid(string? appId)
        {
            return !string.IsNullOrWhiteSpace(appId);
        }

        public static void ForceReconnection()
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectUsingSettings();

            Virality.Logger?.LogDebug("Forcing reconnection due to appid change...");
        }

        // TECHNICALLY realtime and voice apps are different. will this cause problems? only one way to find out...
        public static void SetAppId(string realtimeAppId, string voiceAppId)
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = realtimeAppId;
            // these two are unused ingame
            // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = appId;
            // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdFusion = appId;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = voiceAppId;
        }

        public static void LogAppId()
        {
            Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
            Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice);
        }

        public static void CaptureDefaults()
        {
            if (DefaultRealtimeAppId == null)
                DefaultRealtimeAppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            if (DefaultVoiceAppId == null)
                DefaultVoiceAppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice;
        }
    }
}
