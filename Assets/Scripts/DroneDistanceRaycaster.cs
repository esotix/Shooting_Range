using System.Collections;
using UnityEngine;

public class DroneDistanceRaycaster : MonoBehaviour
{
    public Transform droneTransform;
    public Transform droneTargetTransform;

    public LayerMask layerMask = Physics.AllLayers;
    public float minimumDistanceFromObstacles = 0.1f;
    public float smoothingFactor = 25f;

    Transform tr;
    float currentDistance;

    private void Awake()
    {
        tr = transform;

        layerMask &= ~LayerMask.NameToLayer("Ignore Raycast");
        currentDistance = Vector3.Distance(droneTransform.position, droneTargetTransform.position);
    }

    private void LateUpdate()
    {
        Vector3 castDirection = droneTargetTransform.position - tr.position;
        float distance = GetDroneDistance(castDirection);
        currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothingFactor);
        droneTransform.position = tr.position + castDirection.normalized * currentDistance;
    }

    private float GetDroneDistance(Vector3 castDirection)
    {
        float distance = castDirection.magnitude + minimumDistanceFromObstacles;

        float sphereRadius = 0.5f;
        if (Physics.SphereCast(new Ray(tr.position, castDirection), sphereRadius, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacles);
        }
        return castDirection.magnitude;
    }
}
