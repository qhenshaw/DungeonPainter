using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Qhenshaw.DungeonPainter.Runtime
{
    [CreateAssetMenu(menuName = "Level Design/Tileset")]
    public class DungeonTileset : ScriptableObject
    {
        [field: SerializeField] public PlaceableSet Tiles { get; private set; }
        [field: SerializeField] public PlaceableSet Walls { get; private set; }
    }

    [System.Serializable]
    public struct PlaceableSet
    {
        public List<GameObject> Prefabs;
        public Vector3 Size;
        public Vector3 FlatSize => new Vector3(Size.x, 0f, Size.z);
        public Vector3 Offset;
        public float WallCheckHeight;
    }
}