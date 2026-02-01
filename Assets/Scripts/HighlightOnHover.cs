using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class HighlightOnHover : MonoBehaviour
{
    private List<Material> originalMaterials = new List<Material>();
    private List<Material> highlightMaterials = new List<Material>();
    private List<Renderer> childRenderers = new List<Renderer>();

    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            childRenderers.Add(rend);

            originalMaterials.Add(rend.material);

            Material highlightMat = new Material(rend.material);
            highlightMat.color = highlightColor;

            if (highlightMat.HasProperty("_EmissionColor"))
            {
                highlightMat.EnableKeyword("_EMISSION");
                highlightMat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
            }

            highlightMaterials.Add(highlightMat);
        }
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        for (int i = 0; i < childRenderers.Count; i++)
        {
            childRenderers[i].material = highlightMaterials[i];
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        for (int i = 0; i < childRenderers.Count; i++)
        {
            childRenderers[i].material = originalMaterials[i];
        }
    }
}