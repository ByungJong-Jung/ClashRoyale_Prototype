using UnityEngine;
using Unity.AI.Navigation;

[System.Serializable]
public class MapTile : MonoBehaviour
{
    [SerializeField] public MeshRenderer renderer;
    [SerializeField] public GameObject objMark;
    [SerializeField] public bool hasObstacle;

    public void SetMapTileData(GameObject inObject)
    {
        objMark = inObject;
    }

    public void ShowTileMark()
    {
        if (hasObstacle)
            return;

        renderer.enabled = true;
    }

    public void HideTileMark()
    {
        renderer.enabled = false;
    }


}
