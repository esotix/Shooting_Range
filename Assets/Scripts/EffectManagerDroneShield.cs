using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EffectManagerDroneShield : MonoBehaviour
{
    public GameObject shieldEffect;
    public Renderer droneModel;
    public Transform player;
    public InputActionReference buttonAAction;

    private float endAlpha = 0.085f;
    private bool isEffectRunning = false;

    private void OnEnable()
    {
        buttonAAction.action.performed += OnButtonAPressed;
    }

    private void OnDisable()
    {
        buttonAAction.action.performed -= OnButtonAPressed;
    }

    private void OnButtonAPressed(InputAction.CallbackContext context)
    {
        if (!isEffectRunning)
        {
            StartCoroutine(RunAllEffects());
        }
    }

    IEnumerator RunAllEffects()
    {
        isEffectRunning = true;

        Coroutine shield = StartCoroutine(Shield());
        Coroutine drone = StartCoroutine(Drone());
        StartCoroutine(FollowPlayer());

        yield return shield;
        yield return drone;

        isEffectRunning = false;
    }

    IEnumerator FollowPlayer()
    {
        while (isEffectRunning)
        {
            shieldEffect.transform.position = player.position;
            yield return null;
        }
    }
    IEnumerator Shield()
    {
        Material mat = shieldEffect.GetComponent<Renderer>().material;
        float alpha = 0f;
        shieldEffect.SetActive(true);

        while (alpha < endAlpha)
        {
            alpha += 0.001f;
            mat.SetFloat("_Alpha", alpha);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(3f);
        while (alpha > 0f)
        {
            alpha -= 0.001f;
            mat.SetFloat("_Alpha", alpha);
            yield return new WaitForSeconds(0.01f);
        }
        shieldEffect.SetActive(false);
    }

    IEnumerator Drone()
    {
        Material droneMat = droneModel.material;
        droneMat.EnableKeyword("_EMISSION");
        Color endColor = Color.black;
        Color startColor = Color.white;
        float timeToBlack = 5f;
        float time = timeToBlack;

        while (time > 0f)
        {
            time -= Time.deltaTime;
            droneMat.SetColor("_EmissionColor", Color.Lerp(startColor, endColor, 1 - (time / timeToBlack)));
            yield return null;
        }

        droneMat.SetColor("_EmissionColor", endColor);
        yield return new WaitForSeconds(3f);

        time = 5f;
        while (time > 0f)
        {
            time -= Time.deltaTime;
            float lerp = (Mathf.Sin(Time.time * 5f) + 1.0f) / 2.0f;
            float intensity = Mathf.Lerp(-1f, 5f, lerp);
            droneMat.SetColor("_EmissionColor", startColor * intensity);
            yield return null;
        }

        droneMat.SetColor("_EmissionColor", startColor);
    }
}