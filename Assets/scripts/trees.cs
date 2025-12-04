using UnityEngine;

public class ClearAllTreePrototypes : MonoBehaviour
{
    void Start()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain != null)
        {
            // Remove all tree instances
            terrain.terrainData.treeInstances = new TreeInstance[0];

            // Remove all tree prototypes
            terrain.terrainData.treePrototypes = new TreePrototype[0];

            Debug.Log("All trees and tree prototypes cleared from terrain.");
        }
    }
}
