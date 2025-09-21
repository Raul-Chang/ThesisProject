using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class BushPerimeterAutoSpawner : MonoBehaviour
{
    [Header("Prefabs de arbustos")]
    public List<GameObject> shrubPrefabs = new();

    [Header("Distribución")]
    [Min(0.1f)] public float spacing = 1f;
    [Range(0f, 0.9f)] public float spacingJitter = 0.25f;
    [Range(0f, 1f)] public float lateralJitter = 0.15f;
    public float outwardOffset = 0.3f;
    public bool invertOutward = false;

    [Header("Orientación / escala")]
    public bool faceAlongEdge = false;
    public bool randomYaw = true;
    [Range(0f, 45f)] public float maxYaw = 15f;
    public Vector2 uniformScaleRange = new(0.9f, 1.15f);

    public enum HeightMode { GroundRaycast, FixedY, RelativeToWallBottom }
    [Header("Altura")]
    public HeightMode heightMode = HeightMode.GroundRaycast;
    public float fixedY = 0f;          // si usás FixedY
    public float baseOffset = 0f;      // subir/bajar todo
    public float wallBottomOffset = 0; // si usás RelativeToWallBottom
    public float prefabPivotOffset = 0;// corrige pivots

    [Header("Suelo (raycast)")]
    public bool snapToGround = true;
    public bool alignToSurfaceNormal = true;
    public LayerMask groundMask = ~0;
    public float raycastUpOffset = 5f;
    public float raycastDownDistance = 30f;

    [Header("Gestión")]
    public bool clearOnRespawn = true;
    public Transform parentForInstances; // si null crea "_Bushes (auto)"
    public int seed = 12345;

    void Start()             { Respawn(); }            // ← genera al entrar en Play
    void OnEnable()          { if (!Application.isPlaying) { /* opcional: Respawn(); */ } }
    void Reset()             { EnsureParent(); }

    [ContextMenu("Respawn Perimeter Bushes")]
    public void Respawn()
    {
        if (shrubPrefabs == null || shrubPrefabs.Count == 0) { Debug.LogWarning($"{name}: asigná prefabs"); return; }
        EnsureParent();
        if (clearOnRespawn) ClearPrevious();

        var hull = BuildConvexHullXZ();
        if (hull.Count < 3) { Debug.LogWarning($"{name}: hull insuficiente"); return; }

        System.Random rng = new(seed);

        for (int i = 0; i < hull.Count; i++)
        {
            Vector2 a2 = hull[i];
            Vector2 b2 = hull[(i + 1) % hull.Count];

            Vector3 a = new(a2.x, 0, a2.y);
            Vector3 b = new(b2.x, 0, b2.y);

            Vector3 edge = b - a;
            float segLen = new Vector2(edge.x, edge.z).magnitude;
            if (segLen < 0.001f) continue;

            Vector3 dir = edge.normalized;
            Vector3 outward = new(dir.z, 0, -dir.x); // CCW → afuera
            outward = (invertOutward ? -outward : outward).normalized;

            float traveled = 0f; int safety = 0;
            while (traveled <= segLen && safety++ < 100000)
            {
                float j = 1f + (((float)rng.NextDouble() * 2f - 1f) * spacingJitter);
                float step = Mathf.Max(0.05f, spacing * j);

                float t = Mathf.Clamp01(traveled / segLen);
                Vector3 pos = Vector3.Lerp(a, b, t);

                // offset fijo + jitter lateral
                float lat = ((float)rng.NextDouble() * 2f - 1f) * lateralJitter;
                pos += outward * (outwardOffset + lat);

                // ---- ALTURA ----
                float y;
                Vector3 up = Vector3.up;

                if (heightMode == HeightMode.FixedY)
                    y = fixedY;
                else if (heightMode == HeightMode.RelativeToWallBottom)
                    y = GetWallMinY() + wallBottomOffset;
                else // GroundRaycast
                {
                    y = pos.y;
                    if (snapToGround && Physics.Raycast(pos + Vector3.up * raycastUpOffset, Vector3.down,
                        out RaycastHit hit, raycastDownDistance + raycastUpOffset, groundMask, QueryTriggerInteraction.Ignore))
                    {
                        y = hit.point.y;
                        if (alignToSurfaceNormal) up = hit.normal;
                    }
                }

                y += baseOffset + prefabPivotOffset;
                pos.y = y;

                // rotación
                Quaternion rot;
                if (faceAlongEdge)
                {
                    var lookFwd = Vector3.ProjectOnPlane(dir, up).normalized;
                    rot = (lookFwd.sqrMagnitude > 1e-6f) ? Quaternion.LookRotation(lookFwd, up) : Quaternion.identity;
                    float extraYaw = randomYaw ? (float)(rng.NextDouble() * 360f) : ((float)rng.NextDouble() * 2f - 1f) * maxYaw;
                    rot = rot * Quaternion.AngleAxis(extraYaw, up);
                }
                else
                {
                    float yaw = randomYaw ? (float)rng.NextDouble() * 360f : ((float)rng.NextDouble() * 2f - 1f) * maxYaw;
                    rot = Quaternion.AngleAxis(yaw, up);
                }

                // escala
                float sc = Mathf.Lerp(uniformScaleRange.x, uniformScaleRange.y, (float)rng.NextDouble());

                var prefab = shrubPrefabs[rng.Next(shrubPrefabs.Count)];
                if (prefab)
                {
                    var go = (GameObject)Instantiate(prefab, pos, rot, parentForInstances);
                    go.transform.localScale = Vector3.one * sc;

                    // marca de dueño → así solo borra lo suyo
                    var mk = go.AddComponent<BushSpawnMarker>();
                    mk.owner = this;
                }

                traveled += step;
            }
        }
    }

    // ---------- utilidades ----------
    void EnsureParent()
    {
        if (parentForInstances) return;
        var t = transform.Find("_Bushes (auto)");
        if (t) parentForInstances = t;
        else
        {
            var holder = new GameObject("_Bushes (auto)");
            holder.transform.SetParent(transform, false);
            parentForInstances = holder.transform;
        }
    }

    void ClearPrevious()
    {
        if (!parentForInstances) return;
        var toDel = new List<GameObject>();
        foreach (Transform c in parentForInstances)
        {
            var m = c.GetComponent<BushSpawnMarker>();
            if (m != null && m.owner == this) toDel.Add(c.gameObject);
        }
#if UNITY_EDITOR
        foreach (var go in toDel) Object.DestroyImmediate(go);
#else
        foreach (var go in toDel) Destroy(go);
#endif
    }

    float GetWallMinY()
    {
        float minY = float.PositiveInfinity;
        var rends = GetComponentsInChildren<Renderer>();
        foreach (var r in rends) if (r) minY = Mathf.Min(minY, r.bounds.min.y);
        return float.IsInfinity(minY) ? transform.position.y : minY;
    }

    List<Vector2> BuildConvexHullXZ()
    {
        var set = new HashSet<Vector2>();
        var mfs = GetComponentsInChildren<MeshFilter>();
        foreach (var mf in mfs)
        {
            if (!mf || !mf.sharedMesh) continue;
            var verts = mf.sharedMesh.vertices;
            var t = mf.transform;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 w = t.TransformPoint(verts[i]);
                set.Add(new Vector2(Mathf.Round(w.x * 1000f) / 1000f, Mathf.Round(w.z * 1000f) / 1000f));
            }
        }
        var pts = new List<Vector2>(set);
        return ConvexHull(pts);
    }

    static List<Vector2> ConvexHull(List<Vector2> p)
    {
        if (p == null || p.Count < 3) return new List<Vector2>(p ?? new());
        p.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
        List<Vector2> lower = new();
        foreach (var v in p)
        {
            while (lower.Count >= 2 && Cross(lower[^2], lower[^1], v) <= 0f) lower.RemoveAt(lower.Count - 1);
            lower.Add(v);
        }
        List<Vector2> upper = new();
        for (int i = p.Count - 1; i >= 0; i--)
        {
            var v = p[i];
            while (upper.Count >= 2 && Cross(upper[^2], upper[^1], v) <= 0f) upper.RemoveAt(upper.Count - 1);
            upper.Add(v);
        }
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);
        lower.AddRange(upper); // CCW
        return lower;
    }

    static float Cross(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
}
