using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;

public class Remoting : MonoBehaviour
{
public string IpV4RemotingAddress = "10.189.82.110";
    public int ConnectRetryCount = 5;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
                StartCoroutine(ConnectRemotingSession());
            else
                StartCoroutine(DisconnectRemotingSession());
        }
    }

    IEnumerator ConnectRemotingSession()
    {
        if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Disconnected)
        {
            Debug.LogWarning("Remoting already connected. Please disconnect before trying to connect again.");
        }
        else
        {
            HolographicRemoting.Connect(IpV4RemotingAddress);
            yield return null;

            int currentRetries = 0;
            while (currentRetries < ConnectRetryCount && HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
            {
                currentRetries++;
                Debug.Log($"Waiting on connection: attempt {currentRetries}.");
                yield return new WaitForSeconds(1f);
            }

            if (currentRetries >= ConnectRetryCount)
            {
                Debug.LogError($"Unable to connect to remote session after {currentRetries} attempts.");
            }
            else
            {
                Debug.Log("Loading Windows MR for Remoting...");
                XRSettings.LoadDeviceByName("WindowsMR");
                yield return new WaitForSeconds(1);
                Debug.Log("Starting XR...");
                XRSettings.enabled = true;
                yield return new WaitForSeconds(1);
                Debug.Log($"XR activation state: {XRSettings.enabled}");

                if (!XRSettings.enabled)
                    HolographicRemoting.Disconnect();
            }
        }
    }

    IEnumerator DisconnectRemotingSession()
    {
        if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
        {
            Debug.LogWarning("Remoting is not connected. Please start an active remoting session before attempting to disconnect.");
        }
        else
        {
            HolographicRemoting.Disconnect();
            yield return null;

            int currentRetries = 0;
            while (currentRetries < ConnectRetryCount && HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Disconnected)
            {
                currentRetries++;
                Debug.Log($"Waiting on disconnect: attempt {currentRetries}.");
                yield return new WaitForSeconds(1f);
                currentRetries++;
            }

            if (currentRetries >= ConnectRetryCount)
            {
                Debug.LogError($"Unable to disconnedct from remote session after {currentRetries} attempts.");
            }
            else
            {
                XRSettings.enabled = false;
            }
        }
    }

}