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
        public static string? DefaultRealtimeAppId = null;
        public static string? DefaultVoiceAppId = null;

        public static bool AppIsDefault()
        {
            return DefaultRealtimeAppId == PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime && DefaultVoiceAppId == PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice;
        }

        public static void SetToDefaultApp()
        {
            if (DefaultRealtimeAppId == null || DefaultVoiceAppId == null)
                return;

            SetAppId(DefaultRealtimeAppId, DefaultVoiceAppId);
        }

        public static void ForceReconnection()
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.ConnectUsingSettings();
        }

        // TECHNICALLY realtime and voice apps are different. will this cause problems? only one way to find out...
        public static void SetAppId(string realtimeAppId, string voiceAppId)
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = realtimeAppId;
            // these two are unused ingame
            // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = appId;
            // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdFusion = appId;
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = voiceAppId;

            // PhotonNetwork.Disconnect();
            // PhotonNetwork.ConnectUsingSettings();
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
