Shader "Custom/Raindrops_Material"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Intensity ("Distortion Intensity", Range(0, 1)) = 0.05
        _OutlineStrength ("Outline Strength", Range(0, 0.5)) = 0.15
        
        // Streak properties
        _StreakDensity ("Streak Density", Range(5, 30)) = 15
        _StreakWidth ("Streak Width", Range(0.01, 0.1)) = 0.03
        _StreakLength ("Streak Length", Range(0.1, 0.8)) = 0.4
        _StreakSpeed ("Streak Speed", Range(-0.1, -3)) = -1.0
        _StreakBrightness ("Streak Brightness", Range(-0.3, 0.5)) = 0.05
        _ZigZagAmount ("ZigZag Amount", Range(0, 0.1)) = 0.03
        _ZigZagFrequency ("ZigZag Frequency", Range(1, 10)) = 4.0
        
        // Drop properties
        _NumDrops ("Number of Drops", Range(20, 150)) = 60
        _MinSize ("Minimum Drop Size", Range(0.003, 0.2)) = 0.05
        _MaxSize ("Maximum Drop Size", Range(0.01, 0.5)) = 0.15
        _DropBrightness ("Drop Brightness", Range(-0.3, 0.5)) = 0.08
        _DropLifetime ("Drop Lifetime", Range(0.5, 5)) = 2.5
        
        // Outline color tint
        _OutlineColorR ("Outline Color R", Range(0, 1)) = 0.8
        _OutlineColorG ("Outline Color G", Range(0, 1)) = 0.9
        _OutlineColorB ("Outline Color B", Range(0, 1)) = 1.0
        
        // Lighting properties
        _Glossiness ("Smoothness", Range(0, 1)) = 0.8
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularStrength ("Specular Strength", Range(0, 2)) = 1.0
        
        // Transparency
        _Transparency ("Transparency", Range(0, 1)) = 0.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
            sampler2D _MainTex;
            float _Intensity;
            float _OutlineStrength;
            
            // Streak properties
            float _StreakDensity;
            float _StreakWidth;
            float _StreakLength;
            float _StreakSpeed;
            float _StreakBrightness;
            float _ZigZagAmount;
            float _ZigZagFrequency;
            
            // Drop properties
            float _NumDrops;
            float _MinSize;
            float _MaxSize;
            float _DropBrightness;
            float _DropLifetime;
            
            // Outline color tint
            float _OutlineColorR;
            float _OutlineColorG;
            float _OutlineColorB;
            
            // Lighting properties
            float _Glossiness;
            float _Metallic;
            float3 _SpecularColor;
            float _SpecularStrength;
            
            // Transparency
            float _Transparency;
            
            // Pseudo-random number generator
            float random(float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // Get zigzag X position for a given Y position
            float getZigZagX(float xBase, float yPos, float zigZagAmount, float zigZagFreq, float2 seed)
            {
                float zig1 = sin(yPos * zigZagFreq * 3.14159 * 2.0 + seed.x * 10.0) * zigZagAmount;
                float zig2 = sin(yPos * zigZagFreq * 1.7 * 3.14159 * 2.0 + seed.y * 7.0) * zigZagAmount * 0.5;
                float zig3 = sin(yPos * zigZagFreq * 0.5 * 3.14159 * 2.0 + seed.x * 3.0) * zigZagAmount * 0.3;
                
                return xBase + zig1 + zig2 + zig3;
            }
            
            // Create a zigzag vertical streak
            float verticalStreak(float2 uv, float xBase, float yStart, float length, float width, float zigZagAmount, float zigZagFreq, float2 seed)
            {
                float yRelative = uv.y - yStart;
                
                if (yRelative < 0 || yRelative > length)
                    return 0;
                
                float zigX = getZigZagX(xBase, yRelative / length, zigZagAmount, zigZagFreq, seed);
                float xDist = abs(uv.x - zigX);
                float yDist = yRelative;
                
                float horizontalFade = 1.0 - smoothstep(0, width, xDist);
                float verticalFade = 1.0 - smoothstep(0, length, yDist);
                
                float strength = horizontalFade * verticalFade;
                
                float taper = 1.0 - smoothstep(length * 0.6, length, yDist);
                strength *= (0.3 + taper * 0.7);
                
                return strength;
            }
            
            // Procedural raindrop shape
            float raindrop(float2 uv, float2 center, float size)
            {
                float2 dir = uv - center;
                float dist = length(dir);
                
                float verticalStretch = 1.0 + dir.y * 0.3;
                float drop = 1.0 - smoothstep(0, size * verticalStretch, dist);
                
                return drop;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // ============================================
                // LIGHTING CALCULATION
                // ============================================
                float3 worldNormal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                
                // Diffuse lighting
                float diff = max(0, dot(worldNormal, lightDir));
                float3 diffuse = diff * _LightColor0.rgb;
                
                // Ambient lighting
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                // Specular lighting (Blinn-Phong)
                float spec = pow(max(0, dot(worldNormal, halfDir)), _Glossiness * 128);
                float3 specular = spec * _SpecularColor * _SpecularStrength * _LightColor0.rgb;
                
                // Shadows
                float shadow = SHADOW_ATTENUATION(i);
                
                // Combine lighting
                float3 lighting = (ambient + diffuse * shadow) * 0.5 + specular * shadow * 0.5;
                
                // ============================================
                // RAIN EFFECT
                // ============================================
                
                // Sample the base texture
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                
                float totalEffect = 0;
                float totalEdgeEffect = 0;
                float2 distortionUV = i.uv;
                
                // --- VERTICAL ZIGZAG STREAKS ---
                float numStreaks = _StreakDensity * 2.0;
                float streakSpacing = 1.0 / numStreaks;
                
                float streakIndex = round(i.uv.x / streakSpacing);
                float streakX = streakIndex * streakSpacing;
                
                float2 streakSeed = float2(streakIndex, streakIndex * 7.0);
                float randomOffset = random(streakSeed);
                float randomLength = 0.3 + random(streakSeed + float2(1, 0)) * _StreakLength;
                float randomWidth = _StreakWidth * (0.5 + random(streakSeed + float2(2, 0)) * 1.0);
                float randomSpeed = 0.5 + random(streakSeed + float2(3, 0)) * 1.5;
                float randomStrength = 0.5 + random(streakSeed + float2(4, 0)) * 0.5;
                float randomZigZag = 0.5 + random(streakSeed + float2(5, 0)) * 1.5;
                
                float streakTime = _Time.y * _StreakSpeed * randomSpeed;
                float yPos = frac(streakTime + randomOffset);
                
                float streak = verticalStreak(
                    i.uv,
                    streakX,
                    yPos,
                    randomLength,
                    randomWidth,
                    _ZigZagAmount * randomZigZag,
                    _ZigZagFrequency * (0.7 + random(streakSeed + float2(6, 0)) * 0.6),
                    streakSeed
                );
                streak *= randomStrength;
                
                if (streak > 0.001)
                {
                    totalEffect = max(totalEffect, streak);
                    totalEdgeEffect = max(totalEdgeEffect, streak * 0.3);
                    
                    float yRelativeDistort = i.uv.y - yPos;
                    float zigXDistort = getZigZagX(streakX, yRelativeDistort / max(randomLength, 0.01), _ZigZagAmount * randomZigZag, _ZigZagFrequency * (0.7 + random(streakSeed + float2(6, 0)) * 0.6), streakSeed);
                    
                    float horizontalDistort = (zigXDistort - i.uv.x) * streak * _Intensity * 1.5;
                    distortionUV.x += horizontalDistort;
                    
                    float verticalDistort = (i.uv.y - yPos) * streak * _Intensity * 0.3;
                    distortionUV.y += verticalDistort;
                }
                
                // --- STATIC DROPLETS ---
                int numDrops = (int)_NumDrops;
                float closestDrop = 0;
                float2 closestDropPos = float2(0, 0);
                float closestDropSize = 0;
                
                for (int j = 0; j < numDrops; j++)
                {
                    float2 dropSeed = float2(j * 7.0 + 100, j * 13.0 + 100);
                    float randomX = random(dropSeed);
                    float randomY = random(dropSeed + float2(1, 0));
                    float2 dropPos = float2(randomX, randomY);
                    
                    float randomSize = random(dropSeed + float2(2, 0));
                    float dropSize = lerp(_MinSize, _MaxSize, randomSize);
                    
                    float randomStartTime = random(dropSeed + float2(3, 0)) * 10.0;
                    float cycleTime = _DropLifetime + random(dropSeed + float2(4, 0)) * 2.0;
                    float dropAge = frac((_Time.y + randomStartTime) / cycleTime);
                    
                    float dropVisibility = 0;
                    
                    if (dropAge < 0.1)
                    {
                        dropVisibility = dropAge / 0.1;
                    }
                    else if (dropAge < 0.7)
                    {
                        dropVisibility = 1.0;
                    }
                    else
                    {
                        float fadeOut = (dropAge - 0.7) / 0.3;
                        dropVisibility = 1.0 - fadeOut * fadeOut;
                    }
                    
                    float dropValue = raindrop(i.uv, dropPos, dropSize) * dropVisibility;
                    
                    if (dropValue > 0.001)
                    {
                        totalEffect = max(totalEffect, dropValue);
                        totalEdgeEffect = max(totalEdgeEffect, dropValue * 0.3);
                        
                        if (dropValue > closestDrop)
                        {
                            closestDrop = dropValue;
                            closestDropPos = dropPos;
                            closestDropSize = dropSize;
                        }
                        
                        float2 dirToDrop = i.uv - dropPos;
                        float distToDrop = length(dirToDrop);
                        
                        if (distToDrop > 0.001)
                        {
                            float2 dir = normalize(dirToDrop);
                            float dropDistort = dropValue * _Intensity * 0.5;
                            
                            float edgeFactor = 1.0 - abs(distToDrop - dropSize * 0.5) / (dropSize * 0.5);
                            edgeFactor = max(0, edgeFactor);
                            distortionUV += dir * dropDistort * edgeFactor * 0.5;
                        }
                    }
                }
                
                // ============================================
                // FINAL: APPLY EFFECT WITH LIGHTING
                // ============================================
                
                // Sample texture with distortion
                fixed4 distortedColor = tex2D(_MainTex, distortionUV);
                
                // Apply lighting to base color
                float3 litColor = baseColor.rgb * lighting;
                
                // Apply lighting to distorted color
                float3 litDistorted = distortedColor.rgb * lighting;
                
                float4 finalColor = float4(litDistorted, 1);
                float3 finalUnlit = distortedColor.rgb;
                
                // --- ADD COLORED OUTLINES ---
                float edgeEffect = totalEdgeEffect;
                
                if (edgeEffect > 0.01)
                {
                    float3 outlineColor = float3(_OutlineColorR, _OutlineColorG, _OutlineColorB);
                    float outlineStrength = edgeEffect * _OutlineStrength;
                    
                    // Apply outline to both lit and unlit parts
                    finalColor.rgb = lerp(finalColor.rgb, outlineColor * lighting, outlineStrength * 0.3);
                    finalColor.rgb += outlineColor * lighting * outlineStrength * 0.2;
                }
                
                // --- ADD BRIGHTNESS ---
                float brightness = totalEffect * (_StreakBrightness + _DropBrightness) * 0.5;
                finalColor.rgb += brightness * lighting;
                
                // --- SPECULAR HIGHLIGHT ON DROPS ---
                // Drops catch specular light like glass
                float dropSpec = 0;
                if (closestDrop > 0.001)
                {
                    // Use the drop as a specular highlight mask
                    float2 dirToDrop = i.uv - closestDropPos;
                    float distToDrop = length(dirToDrop);
                    
                    // Drop edge catches light
                    float edgeHighlight = 1.0 - smoothstep(closestDropSize * 0.3, closestDropSize, distToDrop);
                    dropSpec = edgeHighlight * closestDrop * _SpecularStrength * 0.5;
                }
                
                // Add specular from drops
                float3 specularDrop = dropSpec * _SpecularColor * _LightColor0.rgb * shadow;
                finalColor.rgb += specularDrop;
                
                // --- BLEND ---
                float blendAmount = totalEffect * 0.7;
                float3 blendedColor = lerp(litColor, finalColor.rgb, blendAmount);
                
                // Keep the unlit outline for areas with no lighting
                if (edgeEffect > 0.01 && blendAmount < 0.1)
                {
                    blendedColor = lerp(blendedColor, finalUnlit, edgeEffect * 0.3);
                }
                
                finalColor.rgb = blendedColor;
                
                // --- TRANSPARENCY ---
                float alpha = 1.0 - _Transparency + totalEffect * 0.5;
                finalColor.a = alpha;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
        
        // Shadow pass
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                o.uv = v.uv;
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}