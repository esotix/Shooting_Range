using UnityEngine;

public class Follow : MonoBehaviour
{
    public Transform follow;
    void Update()
    {
        transform.position = follow.position;
        transform.rotation = follow.rotation;
    }
}
