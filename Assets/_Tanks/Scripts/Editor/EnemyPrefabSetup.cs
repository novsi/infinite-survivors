using UnityEngine;
using UnityEditor;

namespace TowerSurvivors
{
    public static class EnemyPrefabSetup
    {
        [MenuItem("Tools/Tower Survivors/Setup Enemy Prefabs")]
        public static void SetupEnemyPrefabs()
        {
            // Load EnemyData assets
            EnemyData fastEnemyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Tanks/ScriptableObjects/TowerSurvivors/Enemies/FastEnemyData.asset");
            EnemyData tankEnemyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Tanks/ScriptableObjects/TowerSurvivors/Enemies/TankEnemyData.asset");
            EnemyData bossEnemyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Tanks/ScriptableObjects/TowerSurvivors/Enemies/BossEnemyData.asset");

            // Setup FastEnemy prefab
            SetupPrefab("Assets/_Tanks/Prefabs/TowerSurvivors/Enemies/FastEnemy.prefab", fastEnemyData);
            
            // Setup TankEnemy prefab
            SetupPrefab("Assets/_Tanks/Prefabs/TowerSurvivors/Enemies/TankEnemy.prefab", tankEnemyData);
            
            // Setup BossEnemy prefab
            SetupPrefab("Assets/_Tanks/Prefabs/TowerSurvivors/Enemies/BossEnemy.prefab", bossEnemyData);

            AssetDatabase.SaveAssets();
            Debug.Log("Enemy prefabs setup complete!");
        }

        private static void SetupPrefab(string prefabPath, EnemyData enemyData)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Could not load prefab at {prefabPath}");
                return;
            }

            // Open prefab for editing
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                GameObject prefabRoot = editingScope.prefabContentsRoot;
                Enemy enemy = prefabRoot.GetComponent<Enemy>();
                
                if (enemy != null)
                {
                    // Use SerializedObject to set private field
                    SerializedObject so = new SerializedObject(enemy);
                    SerializedProperty prop = so.FindProperty("m_EnemyData");
                    prop.objectReferenceValue = enemyData;
                    so.ApplyModifiedProperties();
                    
                    Debug.Log($"Assigned {enemyData.name} to {prefabPath}");
                }
                else
                {
                    Debug.LogError($"No Enemy component found on {prefabPath}");
                }
            }
        }
    }
}
