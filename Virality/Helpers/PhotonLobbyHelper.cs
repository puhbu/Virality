using Photon.Pun;

namespace Virality.Helpers;

/// <summary>
///     Helper class for Photon lobby functionality.
/// </summary>
public static class PhotonLobbyHelper
{
    /// <summary>
    ///     Checks if the current scene is the surface.
    /// </summary>
    /// <returns> True if the current scene is the surface, false otherwise. </returns>
    public static bool IsOnSurface()
    {
        return PhotonGameLobbyHandler.IsSurface;
    }

    // TECHNICALLY realtime and voice apps are different. will this cause problems? only one way to find out...
    public static void SetAppId(string appId)
    {
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = appId;
        // these two are unused ingame
        // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat = appId;
        // PhotonNetwork.PhotonServerSettings.AppSettings.AppIdFusion = appId;
        PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = appId;

        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings();
    }

    public static void LogAppId()
    {
        Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime);
        Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat);
        Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdFusion);
        Virality.Logger?.LogDebug(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice);
    }
}