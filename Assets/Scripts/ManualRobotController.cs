using UnityEngine;
using Valve.VR;

public class ManualRobotController : MonoBehaviour
{
    public Transform carryPoint;
    public Puck[] pucks;

    public float moveSpeed = 2f;
    public float turnSpeed = 90f;
    public float grabDistance = 1f;

    private Puck carriedPuck;
    private Rigidbody puckRb;
    private Collider[] puckColliders;

    public bool IsMoving { get; private set; }

    private void Update()
    {
        bool forward = SteamVR_Actions.default_Forward.GetState(SteamVR_Input_Sources.LeftHand);
        bool backward = SteamVR_Actions.default_Backward.GetState(SteamVR_Input_Sources.LeftHand);
        bool left = SteamVR_Actions.default_Left.GetState(SteamVR_Input_Sources.LeftHand);
        bool right = SteamVR_Actions.default_Right.GetState(SteamVR_Input_Sources.LeftHand);

        if (forward) transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        if (backward) transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        if (left) transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
        if (right) transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

        IsMoving = forward || backward || left || right;

        if (SteamVR_Actions.default_Magnet.GetStateDown(SteamVR_Input_Sources.LeftHand))
            PickUpPuck();

        if (SteamVR_Actions.default_Release.GetStateDown(SteamVR_Input_Sources.LeftHand))
            ReleasePuck();
    }

    private void PickUpPuck()
    {
        if (carriedPuck != null) return;

        float minDistance = grabDistance;
        Puck nearestPuck = null;

        foreach (Puck puck in pucks)
        {
            if (puck == null || puck.isSorted || puck.isCarried) continue;

            float distance = Vector3.Distance(transform.position, puck.transform.position);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPuck = puck;
            }
        }

        if (nearestPuck == null) return;

        carriedPuck = nearestPuck;
        carriedPuck.isCarried = true;

        puckRb = carriedPuck.GetComponent<Rigidbody>();
        if (puckRb == null) puckRb = carriedPuck.GetComponentInChildren<Rigidbody>();

        puckColliders = carriedPuck.GetComponentsInChildren<Collider>();

        if (puckRb != null)
        {
            puckRb.isKinematic = true;
            puckRb.useGravity = false;
        }

        foreach (Collider col in puckColliders)
            col.enabled = false;

        carriedPuck.transform.SetParent(carryPoint);
        carriedPuck.transform.localPosition = Vector3.zero;
        carriedPuck.transform.localRotation = Quaternion.identity;
    }

    private void ReleasePuck()
    {
        if (carriedPuck == null) return;

        carriedPuck.transform.SetParent(null);
        carriedPuck.isCarried = false;

        if (puckRb != null)
        {
            puckRb.isKinematic = false;
            puckRb.useGravity = true;
        }

        foreach (Collider col in puckColliders)
            col.enabled = true;

        carriedPuck = null;
        puckRb = null;
        puckColliders = null;
    }
}