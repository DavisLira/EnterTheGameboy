using Mirror;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraSetup : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        var vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
        }
    }
}