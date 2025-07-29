using UnityEngine;
using System.Collections.Generic;
public class InGameData
{
    public const float RAYCAST_VALUE = 200f;
    public static readonly Vector3 INFINITY_POS = new Vector3(100f,100f,100f);
    public const float ENTITY_SIZE_OFFSET = 1.125f;
    public static readonly Color TEAM_BLUE = new Color(0.106f, 0.247f, 0.647f);
    public static readonly Color TEAM_RED = new Color(0.85f, 0.08f, 0.1f);


    private static Material _hitEffectMaterial;
    public static Material HitEffectMaterial
    {
        get
        {
            if (_hitEffectMaterial == null)
            {
                _hitEffectMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _hitEffectMaterial.color = new Color(0.894f, 0.047f, 0.047f);
            }

            return _hitEffectMaterial;
        }
    }
}

// TODO 나중에 관리 방법 바꿔야 함. 
public class ResourceEffectPath
{
    public static Dictionary<string, string> ResourceEffectPaths = new Dictionary<string, string>()
    {
        { "Hit","Effects/Hit" },
        { "Explosion","Effects/Explosion" }
    };

    public static string GetEffectResourcePath(string inKey)
    {
        if (ResourceEffectPaths.ContainsKey(inKey))
        {
            return ResourceEffectPaths[inKey];
        }

        return null;
    }
}
