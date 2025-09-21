using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class StochasticTiledPlane : MonoBehaviour
{
    [Header("Grid & Size (XZ, Y up)")]
    [Min(1)] public int tilesX = 10;
    [Min(1)] public int tilesY = 10;
    public float sizeX = 10f;  // ancho en X (metros)
    public float sizeZ = 10f;  // largo en Z (metros)

    [Header("Randomization")]
    public int seed = 12345;

    [Header("Material (URP Lit con Albedo + Normal)")]
    public Material material;

    [Tooltip("Regenera al cambiar parámetros en editor")]
    public bool autoRegenerate = true;

    Mesh _mesh;

    void OnEnable()
    {
        Ensure();
        if (autoRegenerate) Generate();
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        Ensure();
        if (autoRegenerate) Generate();
    }

    void Ensure()
    {
        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "StochasticTiledPlaneMesh";
            _mesh.indexFormat = (tilesX * tilesY * 4 > 65535) ?
                UnityEngine.Rendering.IndexFormat.UInt32 :
                UnityEngine.Rendering.IndexFormat.UInt16;
            mf.sharedMesh = _mesh;
        }
        if (material != null) mr.sharedMaterial = material;
    }

    [ContextMenu("Generate Now")]
    public void Generate()
    {
        if (_mesh == null) Ensure();

        int quads = tilesX * tilesY;
        int vCount = quads * 4;
        int iCount = quads * 6;

        var vertices = new Vector3[vCount];
        var normals  = new Vector3[vCount];
        var tangents = new Vector4[vCount];
        var uvs      = new Vector2[vCount];
        var indices  = new int[iCount];

        System.Random rng = new System.Random(seed);

        // Tamaño de cada tile en mundo
        float dx = sizeX / tilesX;
        float dz = sizeZ / tilesY;

        // Origen: centrado en (0,0)
        float x0World = -sizeX * 0.5f;
        float z0World = -sizeZ * 0.5f;

        int v = 0;
        int t = 0;

        // Direcciones base en objeto (plano XZ, Y arriba)
        Vector3 Xdir = Vector3.right;  // +X
        Vector3 Zdir = Vector3.forward; // +Z
        Vector3 Ndir = Vector3.up;

        for (int y = 0; y < tilesY; y++)
        {
            for (int x = 0; x < tilesX; x++)
            {
                // Esquinas en mundo del tile (sin rotar geometría; solo UV rotan)
                float xA = x0World + x * dx;
                float xB = xA + dx;
                float zA = z0World + y * dz;
                float zB = zA + dz;

                // 4 vértices (A: izq, B: der / abajo-arriba)
                // v0 = (xA, zA), v1 = (xB, zA), v2 = (xA, zB), v3 = (xB, zB)
                vertices[v + 0] = new Vector3(xA, 0, zA);
                vertices[v + 1] = new Vector3(xB, 0, zA);
                vertices[v + 2] = new Vector3(xA, 0, zB);
                vertices[v + 3] = new Vector3(xB, 0, zB);

                normals[v + 0] = Ndir;
                normals[v + 1] = Ndir;
                normals[v + 2] = Ndir;
                normals[v + 3] = Ndir;

                // Decisión aleatoria por tile
                int rot = rng.Next(0, 4);        // 0..3 (0°,90°,180°,270°)
                bool flipX = rng.NextDouble() > 0.5;
                bool flipY = rng.NextDouble() > 0.5;

                // UV base por vértice (antes de transform)
                Vector2 uv0 = new Vector2(0, 0);
                Vector2 uv1 = new Vector2(1, 0);
                Vector2 uv2 = new Vector2(0, 1);
                Vector2 uv3 = new Vector2(1, 1);

                // Aplica flips
                uv0 = ApplyFlip(uv0, flipX, flipY);
                uv1 = ApplyFlip(uv1, flipX, flipY);
                uv2 = ApplyFlip(uv2, flipX, flipY);
                uv3 = ApplyFlip(uv3, flipX, flipY);

                // Aplica rotación por cuartos
                uv0 = ApplyRot90(uv0, rot);
                uv1 = ApplyRot90(uv1, rot);
                uv2 = ApplyRot90(uv2, rot);
                uv3 = ApplyRot90(uv3, rot);

                // Escribí UV
                uvs[v + 0] = uv0;
                uvs[v + 1] = uv1;
                uvs[v + 2] = uv2;
                uvs[v + 3] = uv3;

                // Tangente por tile (alineado con dirección de +U después de flip+rot)
                // Derivación: con uvT = (u',v') y reglas:
                // rot0: (u',v')=(u, v)        -> Udir =  signU * X
                // rot1: (u',v')=(v, 1-u)      -> Udir =  signV * Z
                // rot2: (u',v')=(1-u, 1-v)    -> Udir = -signU * X
                // rot3: (u',v')=(1-v, u)      -> Udir = -signV * Z
                int signU = flipX ? -1 : 1;
                int signV = flipY ? -1 : 1;
                Vector3 UdirWorld;

                switch (rot)
                {
                    case 0:  UdirWorld = Xdir * signU;       break;
                    case 1:  UdirWorld = Zdir * signV;       break;
                    case 2:  UdirWorld = Xdir * -signU;      break;
                    default: UdirWorld = Zdir * -signV;      break; // rot3
                }

                Vector3 tan = UdirWorld.normalized;
                // Handedness w: -1 si hay una sola inversión (flipX XOR flipY)
                float w = (flipX ^ flipY) ? -1f : 1f;
                Vector4 tangent = new Vector4(tan.x, tan.y, tan.z, w);

                tangents[v + 0] = tangent;
                tangents[v + 1] = tangent;
                tangents[v + 2] = tangent;
                tangents[v + 3] = tangent;

                // Índices (dos triángulos)
                indices[t + 0] = v + 0;
                indices[t + 1] = v + 2;
                indices[t + 2] = v + 1;
                indices[t + 3] = v + 2;
                indices[t + 4] = v + 3;
                indices[t + 5] = v + 1;

                v += 4;
                t += 6;
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.tangents = tangents;
        _mesh.uv = uvs;
        _mesh.triangles = indices;
        _mesh.RecalculateBounds();
    }

    static Vector2 ApplyFlip(Vector2 uv, bool flipX, bool flipY)
    {
        if (flipX) uv.x = 1f - uv.x;
        if (flipY) uv.y = 1f - uv.y;
        return uv;
    }

    // Rotación por cuartos (0/90/180/270)
    // 0: (x,y)
    // 1: (y, 1-x)
    // 2: (1-x, 1-y)
    // 3: (1-y, x)
    static Vector2 ApplyRot90(Vector2 uv, int rot)
    {
        switch (rot & 3)
        {
            case 1: return new Vector2(uv.y, 1f - uv.x);
            case 2: return new Vector2(1f - uv.x, 1f - uv.y);
            case 3: return new Vector2(1f - uv.y, uv.x);
            default: return uv;
        }
    }
}
