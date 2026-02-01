using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneDebug : MonoBehaviour
{
    [Header("A1 - Contraintes")]
    public GameObject following;
    public Vector3 offset = new Vector3(0, 3, -5);
    public LayerMask obstacleMask = ~0; // PAR DÉFAUT : TOUT
    public float exclusionZoneRadius = 1.0f;

    [Header("A2 - Paramètres")]
    public int N = 32;
    public float coneAngle = 60f;
    public float lookAheadBase = 2.0f;
    public float lookAheadSpeedFactor = 0.5f;
    public float clearanceRadius = 0.35f;
    public float maxSpeed = 5f;
    public float maxAccel = 3f;
    public float maxTurnRate = 90f;

    [Header("A2 - Pondérations")]
    public float wSafe = 2.0f;
    public float wFollow = 1.5f;
    public float wLoS = 1.0f;
    public float wDyn = 0.8f;

    [Header("B4 - Stabilisation")]
    public float hysteresisThreshold = 0.15f;
    public float smoothingFactor = 0.3f;

    [Header("B5 - Fallback")]
    public float cautiousSpeedMultiplier = 0.3f;
    public float hoverDuration = 0.5f;

    [Header("DEBUG")]
    public bool showDebugLogs = true;

    private Rigidbody rb;
    private Vector3 lastChosenDirection;
    private Vector3 smoothedVelocity;
    private bool inCautiousMode = false;
    private int frameCount = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastChosenDirection = transform.forward;
        smoothedVelocity = Vector3.zero;

        if (showDebugLogs)
        {
            Debug.Log("=== DRONE DEBUG START ===");
            Debug.Log($"Position drone: {transform.position}");
            Debug.Log($"ObstacleMask value: {obstacleMask.value}");

            Collider[] nearby = Physics.OverlapSphere(transform.position, 50f);
            Debug.Log($"Objets dans 50m: {nearby.Length}");

            foreach (var col in nearby)
            {
                Debug.Log($"  - {col.name} | Layer: {LayerMask.LayerToName(col.gameObject.layer)} | Distance: {Vector3.Distance(transform.position, col.transform.position):F2}m");
            }

            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 100f, obstacleMask))
            {
                Debug.Log($"✅ TEST RAYCAST OK: {hit.collider.name} à {hit.distance:F2}m");
            }
            else
            {
                Debug.LogError("❌ AUCUN RAYCAST HIT - Vérifie tes colliders !");
            }
        }

        StartCoroutine(RB3DCPLoop());
    }

    IEnumerator RB3DCPLoop()
    {
        while (following != null)
        {
            frameCount++;

            Vector3 dronePos = transform.position;
            Vector3 anchorPos = following.transform.position + following.transform.TransformDirection(offset);
            Vector3 d0 = (anchorPos - dronePos).normalized;

            List<CandidateDirections> candidates = GenerateCandidates(dronePos, d0);

            float L = lookAheadBase + lookAheadSpeedFactor * rb.linearVelocity.magnitude;
            List<CandidateDirections> validCandidates = FilterObstacles(dronePos, candidates, L);

            if (showDebugLogs && frameCount % 60 == 0)
            {
                Debug.Log($"Frame {frameCount}: {validCandidates.Count}/{candidates.Count} directions valides");
            }

            if (validCandidates.Count == 0)
            {
                if (showDebugLogs) Debug.LogWarning("Cautious Mode activé !");
                yield return StartCoroutine(CautiousMode());
                continue;
            }

            foreach (var candidate in validCandidates)
            {
                candidate.score = ComputeScore(dronePos, anchorPos, candidate, L);
            }

            validCandidates.Sort((a, b) => a.score.CompareTo(b.score));
            CandidateDirections best = validCandidates[0];

            Vector3 chosenDirection = lastChosenDirection;
            if (validCandidates.Count > 1)
            {
                CandidateDirections current = validCandidates.Find(c =>
                    Vector3.Angle(c.direction, lastChosenDirection) < 5f);

                if (current != null && (current.score - best.score) / current.score < hysteresisThreshold)
                {
                    chosenDirection = current.direction;
                }
                else
                {
                    chosenDirection = best.direction;
                }
            }
            else
            {
                chosenDirection = best.direction;
            }

            chosenDirection = Vector3.Slerp(lastChosenDirection, chosenDirection, smoothingFactor);
            lastChosenDirection = chosenDirection;

            Vector3 targetVelocity = chosenDirection * maxSpeed;
            Vector3 desiredAccel = (targetVelocity - smoothedVelocity) / Time.fixedDeltaTime;
            desiredAccel = Vector3.ClampMagnitude(desiredAccel, maxAccel);

            smoothedVelocity += desiredAccel * Time.fixedDeltaTime;
            smoothedVelocity = Vector3.ClampMagnitude(smoothedVelocity, maxSpeed);

            rb.linearVelocity = smoothedVelocity;

            inCautiousMode = false;
            yield return new WaitForFixedUpdate();
        }
    }

    List<CandidateDirections> GenerateCandidates(Vector3 origin, Vector3 baseDir)
    {
        List<CandidateDirections> candidates = new List<CandidateDirections>();
        candidates.Add(new CandidateDirections { direction = baseDir });

        float halfCone = coneAngle * 0.5f * Mathf.Deg2Rad;

        for (int i = 1; i < N; i++)
        {
            float phi = Mathf.Acos(1f - (1f - Mathf.Cos(halfCone)) * i / (N - 1f));
            float theta = Mathf.PI * (1f + Mathf.Sqrt(5f)) * i;

            Vector3 localDir = new Vector3(
                Mathf.Sin(phi) * Mathf.Cos(theta),
                Mathf.Sin(phi) * Mathf.Sin(theta),
                Mathf.Cos(phi)
            );

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, baseDir);
            Vector3 worldDir = rotation * localDir;

            candidates.Add(new CandidateDirections { direction = worldDir.normalized });
        }

        return candidates;
    }

    List<CandidateDirections> FilterObstacles(Vector3 origin, List<CandidateDirections> candidates, float L)
    {
        List<CandidateDirections> valid = new List<CandidateDirections>();
        int rejectedCount = 0;

        foreach (var candidate in candidates)
        {
            if (Physics.SphereCast(origin, clearanceRadius, candidate.direction,
                out RaycastHit hit, L, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                candidate.hitDistance = hit.distance;
                if (hit.distance < clearanceRadius * 2f)
                {
                    rejectedCount++;
                    continue;
                }
            }
            else
            {
                candidate.hitDistance = L;
            }

            valid.Add(candidate);
        }

        if (showDebugLogs && frameCount % 60 == 0 && rejectedCount > 0)
        {
            Debug.Log($"Rejeté {rejectedCount} directions trop proches d'obstacles");
        }

        return valid;
    }

    float ComputeScore(Vector3 dronePos, Vector3 anchorPos, CandidateDirections candidate, float L)
    {
        float score = 0f;

        Vector3 projectedPos = dronePos + candidate.direction * L;
        float distToTarget = Vector3.Distance(projectedPos, anchorPos);
        score += wFollow * distToTarget;

        if (Physics.Raycast(dronePos, anchorPos - dronePos, Vector3.Distance(dronePos, anchorPos), obstacleMask))
        {
            score += wLoS * 5.0f;
        }

        float angleChange = Vector3.Angle(lastChosenDirection, candidate.direction);
        float maxAnglePerFrame = maxTurnRate * Time.fixedDeltaTime;
        if (angleChange > maxAnglePerFrame)
        {
            score += wDyn * (angleChange / maxAnglePerFrame);
        }

        if (candidate.hitDistance < L)
        {
            float ttc = candidate.hitDistance / Mathf.Max(rb.linearVelocity.magnitude, 0.1f);
            score += wSafe * Mathf.Max(0, 3.0f - ttc);
        }

        Vector3 toAnchor = anchorPos - dronePos;
        Vector3 playerForward = following.transform.forward;
        if (Vector3.Dot(toAnchor.normalized, playerForward) > 0.7f && toAnchor.magnitude < exclusionZoneRadius)
        {
            score += 100f;
        }

        return score;
    }

    IEnumerator CautiousMode()
    {
        if (inCautiousMode) yield break;

        inCautiousMode = true;

        rb.linearVelocity *= cautiousSpeedMultiplier;

        yield return new WaitForSeconds(hoverDuration);

        clearanceRadius *= 1.5f;
        yield return new WaitForSeconds(0.5f);
        clearanceRadius /= 1.5f;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || following == null || rb == null) return;

        Vector3 dronePos = transform.position;
        Vector3 anchorPos = following.transform.position + following.transform.TransformDirection(offset);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(dronePos, anchorPos);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(dronePos, lastChosenDirection * 2f);

        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(following.transform.position, exclusionZoneRadius);

        float testDistance = lookAheadBase + lookAheadSpeedFactor * rb.linearVelocity.magnitude;

        for (int i = 0; i < 8; i++)
        {
            float angle = (360f / 8) * i;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            if (Physics.SphereCast(dronePos, clearanceRadius, direction,
                out RaycastHit hit, testDistance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(dronePos, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.3f);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(dronePos, direction * testDistance);
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(dronePos, clearanceRadius);
    }
}

public class CandidateDirections
{
    public Vector3 direction;
    public float hitDistance;
    public float score;
}
