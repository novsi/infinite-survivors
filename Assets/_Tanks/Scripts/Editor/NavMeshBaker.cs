using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;

namespace Tanks.Editor
{
    public static class NavMeshBaker
    {
        [MenuItem("Tools/Bake All NavMesh Surfaces")]
        public static void BakeAllNavMeshSurfaces()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
            
            if (surfaces.Length == 0)
            {
                Debug.LogWarning("No NavMeshSurface components found in the scene.");
                return;
            }
            
            foreach (NavMeshSurface surface in surfaces)
            {
                surface.BuildNavMesh();
                Debug.Log($"NavMesh baked for: {surface.gameObject.name}");
            }
            
            Debug.Log($"Baked {surfaces.Length} NavMesh surface(s).");
        }
    }
}
