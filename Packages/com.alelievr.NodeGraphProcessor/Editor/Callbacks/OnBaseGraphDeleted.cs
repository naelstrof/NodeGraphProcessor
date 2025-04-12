using UnityEditor;
using UnityEngine;

namespace GraphProcessor {
[ExecuteAlways]
public class DeleteCallback : AssetModificationProcessor {
    private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) {
        var objects = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (var obj in objects)
            if (obj is BaseGraph b) {
                foreach (var graphWindow in Resources.FindObjectsOfTypeAll<BaseGraphWindow>())
                    graphWindow.OnGraphDeleted();

                b.OnAssetDeleted();
            }

        return AssetDeleteResult.DidNotDelete;
    }
}
}