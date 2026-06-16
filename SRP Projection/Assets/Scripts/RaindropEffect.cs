using System;
using UnityEngine;

public class RaindropEffect : MonoBehaviour
{
    public Material raindropMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (raindropMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        // Pass the source texture to the shader. The shader needs a property named _MainTex.
        raindropMaterial.SetTexture("_MainTex", source);
        Graphics.Blit(source, destination, raindropMaterial);
    }
}
