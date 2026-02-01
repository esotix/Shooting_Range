using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneRB3DCP : MonoBehaviour
{
    [Header("A1 - Contraintes")]
    public GameObject following;
    public Transform droneModel;
    public Vector3 offset = new Vector3(-1f, 1.2f, 2.5f);
    public LayerMask obstacleMask;
    public float exclusionZoneRadius = 1.0f;

    [Header("A2 - Paramètres")]
    public int N = 32;
    public float coneAngle = 60f;
    public float lookAheadBase = 3.5f;
    public float lookAheadSpeedFactor = 1.2f;
    public float clearanceRadius = 0.4f;
    public float maxSpeed = 5f;
    public float maxAccel = 4f;
    public float maxTurnRate = 120f;
    public float stoppingDistance = 1f;
    public float minSpeedThreshold = 0.1f;
    public float hoverStiffness = 4f;
    public float hoverDamping = 0.8f;
    public float microMovementAmplitude = 0.2f;

    [Header("A2 - Pondérations")]
    public float wSafe = 2.5f;
    public float wFollow = 1.5f;
    public float wLoS = 1.0f;
    public float wDyn = 0.8f;

    [Header("B4 - Stabilisation")]
    public float hysteresisThreshold = 0.15f;
    public float smoothingFactor = 0.3f;

    private Rigidbody rb;
    private Transform followingTransform;
    private Vector3 lastChosenDirection;
    private Vector3 smoothedVelocity;
    private Vector3 lastPlayerPosition;
    private float hoverTime;
    private float hoverBlend;

    private List<CandidateDirection> candidatesPool;
    private RaycastHit[] raycastHitsBuffer = new RaycastHit[1];

    private const float HALF_CONE_CACHE_MULTIPLIER = 0.5f * Mathf.Deg2Rad;
    private const float GOLDEN_RATIO = 1.618033988749895f;
    private const float PI = Mathf.PI;

    private float maxAnglePerFrameCache;
    private float stoppingDistanceSquared;
    private float minSpeedThresholdSquared;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        followingTransform = following.transform;
        lastChosenDirection = transform.forward;
        lastPlayerPosition = followingTransform.position;

        candidatesPool = new List<CandidateDirection>(N);
        for (int i = 0; i < N; i++)
        {
            candidatesPool.Add(new CandidateDirection());
        }

        stoppingDistanceSquared = stoppingDistance * stoppingDistance;
        minSpeedThresholdSquared = minSpeedThreshold * minSpeedThreshold;

        StartCoroutine(RB3DCPLoop());
    }

    IEnumerator RB3DCPLoop()
    {
        WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();

        while (following != null)
        {
            Vector3 dronePos = transform.position;
            Vector3 playerPos = followingTransform.position;
            Vector3 anchorPos = playerPos + followingTransform.TransformDirection(offset);

            Vector3 playerVelocity = (playerPos - lastPlayerPosition) / Time.fixedDeltaTime;
            lastPlayerPosition = playerPos;

            Vector3 toAnchor = anchorPos - dronePos;
            float distanceToTargetSqr = toAnchor.sqrMagnitude;
            bool playerIsMoving = playerVelocity.sqrMagnitude > minSpeedThresholdSquared;

            bool shouldHover = distanceToTargetSqr < stoppingDistanceSquared && !playerIsMoving;

            hoverBlend = shouldHover ?
                Mathf.Lerp(hoverBlend, 1f, 0.05f) :
                Mathf.Lerp(hoverBlend, 0f, 0.1f);

            if (shouldHover) hoverTime += Time.fixedDeltaTime;
            else hoverTime = 0f;

            Vector3 d0 = toAnchor.normalized;
            int validCount = GenerateAndFilterCandidates(dronePos, d0);

            Vector3 followVelocity = Vector3.zero;

            if (validCount == 0)
            {
                hoverBlend = 1f;
            }
            else
            {
                maxAnglePerFrameCache = maxTurnRate * Time.fixedDeltaTime;
                float distanceToTarget = Mathf.Sqrt(distanceToTargetSqr);

                ScoreCandidates(dronePos, anchorPos, distanceToTarget, validCount);

                CandidateDirection best = FindBestCandidate(validCount);
                Vector3 chosenDirection = SelectDirection(best, validCount);

                float adaptiveSpeed = CalculateAdaptiveSpeed(best, distanceToTarget);

                followVelocity = CalculateFollowVelocity(chosenDirection, adaptiveSpeed);
            }

            Vector3 hoverVelocity = CalculateHoverVelocity(toAnchor);
            Vector3 finalVelocity = Vector3.Lerp(followVelocity, hoverVelocity, hoverBlend);

            smoothedVelocity = finalVelocity;
            rb.linearVelocity = finalVelocity;

            Quaternion targetRotation = followingTransform.rotation;
            droneModel.rotation = Quaternion.Slerp(
                droneModel.rotation,
                targetRotation,
                Time.fixedDeltaTime * 5f
            );

            yield return waitFixed;
        }
    }

    int GenerateAndFilterCandidates(Vector3 origin, Vector3 baseDir)
    {
        candidatesPool[0].direction = baseDir;

        float halfCone = coneAngle * HALF_CONE_CACHE_MULTIPLIER;
        float oneMinusCosHalfCone = 1f - Mathf.Cos(halfCone);
        float nMinusOne = N - 1f;

        for (int i = 1; i < N; i++)
        {
            float t = i / nMinusOne;
            float phi = Mathf.Acos(1f - oneMinusCosHalfCone * t);
            float theta = PI * GOLDEN_RATIO * i;

            float sinPhi = Mathf.Sin(phi);
            Vector3 localDir = new Vector3(
                sinPhi * Mathf.Cos(theta),
                sinPhi * Mathf.Sin(theta),
                Mathf.Cos(phi)
            );

            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, baseDir);
            candidatesPool[i].direction = rotation * localDir;
        }

        float L = lookAheadBase + lookAheadSpeedFactor * rb.linearVelocity.magnitude;
        float minDistance = clearanceRadius * 2f;
        int validCount = 0;

        for (int i = 0; i < N; i++)
        {
            CandidateDirection candidate = candidatesPool[i];

            if (Physics.SphereCast(origin, clearanceRadius, candidate.direction,
                out RaycastHit hit, L, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.distance < minDistance) continue;
                candidate.hitDistance = hit.distance;
            }
            else
            {
                candidate.hitDistance = L;
            }

            if (validCount != i)
            {
                CandidateDirection temp = candidatesPool[validCount];
                candidatesPool[validCount] = candidate;
                candidatesPool[i] = temp;
            }
            validCount++;
        }

        return validCount;
    }

    void ScoreCandidates(Vector3 dronePos, Vector3 anchorPos, float distanceToTarget, int count)
    {
        Vector3 playerForward = followingTransform.forward;
        float distToAnchor = Vector3.Distance(dronePos, anchorPos);

        for (int i = 0; i < count; i++)
        {
            CandidateDirection candidate = candidatesPool[i];
            float score = 0f;

            Vector3 projectedPos = dronePos + candidate.direction * candidate.hitDistance;
            float distToTarget = Vector3.Distance(projectedPos, anchorPos);
            score += wFollow * distToTarget;

            if (Physics.SphereCast(dronePos, clearanceRadius, (anchorPos - dronePos).normalized,
                out RaycastHit hit, distToAnchor, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                score += wLoS * 5.0f;
            }

            float angleChange = Vector3.Angle(lastChosenDirection, candidate.direction);
            if (angleChange > maxAnglePerFrameCache)
            {
                score += wDyn * (angleChange / maxAnglePerFrameCache);
            }

            if (candidate.hitDistance < lookAheadBase)
            {
                float ttc = candidate.hitDistance / Mathf.Max(rb.linearVelocity.magnitude, 0.1f);
                score += wSafe * Mathf.Max(0, 3.0f - ttc);
            }

            if (distanceToTarget < exclusionZoneRadius)
            {
                float dot = Vector3.Dot(candidate.direction, playerForward);
                if (dot > 0.7f)
                {
                    score += 100f;
                }
            }

            candidate.score = score;
        }
    }

    CandidateDirection FindBestCandidate(int count)
    {
        CandidateDirection best = candidatesPool[0];
        for (int i = 1; i < count; i++)
        {
            if (candidatesPool[i].score < best.score)
            {
                best = candidatesPool[i];
            }
        }
        return best;
    }

    Vector3 SelectDirection(CandidateDirection best, int count)
    {
        Vector3 chosenDirection = lastChosenDirection;

        if (count > 1)
        {
            CandidateDirection current = null;
            for (int i = 0; i < count; i++)
            {
                if (Vector3.Angle(candidatesPool[i].direction, lastChosenDirection) < 5f)
                {
                    current = candidatesPool[i];
                    break;
                }
            }

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

        return chosenDirection;
    }

    float CalculateAdaptiveSpeed(CandidateDirection best, float distanceToTarget)
    {
        float adaptiveSpeed = maxSpeed;

        if (distanceToTarget < stoppingDistance * 3f)
        {
            float speedFactor = distanceToTarget / (stoppingDistance * 3f);
            adaptiveSpeed *= Mathf.Max(0.2f, speedFactor);
        }

        if (best.hitDistance < lookAheadBase + lookAheadSpeedFactor * maxSpeed)
        {
            float safeSpeed = Mathf.Sqrt(2f * maxAccel * best.hitDistance);
            adaptiveSpeed = Mathf.Min(adaptiveSpeed, Mathf.Max(0.5f, safeSpeed * 0.8f));
        }

        return adaptiveSpeed;
    }

    Vector3 CalculateFollowVelocity(Vector3 chosenDirection, float adaptiveSpeed)
    {
        Vector3 targetVelocity = chosenDirection * adaptiveSpeed;
        Vector3 desiredAccel = (targetVelocity - smoothedVelocity) / Time.fixedDeltaTime;
        desiredAccel = Vector3.ClampMagnitude(desiredAccel, maxAccel);

        Vector3 velocity = smoothedVelocity + desiredAccel * Time.fixedDeltaTime;
        return Vector3.ClampMagnitude(velocity, adaptiveSpeed);
    }

    Vector3 CalculateHoverVelocity(Vector3 toTarget)
    {
        Vector3 springForce = toTarget * hoverStiffness;

        Vector3 microMovement = new Vector3(
            Mathf.Sin(hoverTime * 2f) * microMovementAmplitude,
            Mathf.Sin(hoverTime * 3f) * microMovementAmplitude * 0.5f,
            Mathf.Cos(hoverTime * 2.5f) * microMovementAmplitude
        );

        return (springForce + microMovement) - rb.linearVelocity * (1f - hoverDamping);
    }
}

public class CandidateDirection
{
    public Vector3 direction;
    public float hitDistance;
    public float score;
}