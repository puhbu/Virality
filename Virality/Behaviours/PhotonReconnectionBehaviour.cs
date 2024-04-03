using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virality.Behaviours
{
    public class PhotonReconnectionBehaviour : MonoBehaviourPunCallbacks
    {
        public static PhotonReconnectionBehaviour? Instance { get; private set; }
        public static List<Action?> InvokeOnConnected = new();

        public void Start()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = this;
        }

        // used to get the exact moment we're connected to the photon server
        // needed for smooth midgame id switching
        public override void OnConnectedToMaster()
        {
            foreach (var action in InvokeOnConnected)
            {
                action?.Invoke();
            }

            InvokeOnConnected.Clear();
        }

        public void InvokeCallbackOnConnection(Action callback)
        {
            InvokeOnConnected.Add(callback);
        }
    }
}
