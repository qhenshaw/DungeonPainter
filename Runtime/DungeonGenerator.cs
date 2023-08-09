using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

namespace Qhenshaw.DungeonPainter.Runtime
{
    public class DungeonGenerator : MonoBehaviour
    {
        [field: SerializeField] public DungeonData DungeonData { get; private set; }
        [field: SerializeField] public DungeonTileset Tileset { get; private set; }
        [SerializeField] private Vector3 _newRoomSize = new Vector3(4f, 4f, 4f);
        [SerializeField] private int _seed = 0;
        [SerializeField] private float _gridSize = 2f;

        public List<Bounds> Rooms => DungeonData.Rooms;

        private readonly Vector3[] _directions =
        {
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, -1f),
            new Vector3(-1f, 0f, 0f),
        };

        private Transform GetContainer(string name)
        {
            Transform container = transform.Find(name);
            if(container == null)
            {
                container = new GameObject(name).transform;
                container.SetParent(transform);
            }

            return container;
        }

        public void AddRoom()
        {
            Vector3 center = _newRoomSize * 0.5f;
            if (Rooms.Count >= 1) center = Rooms[Rooms.Count - 1].center;
            Rooms.Add(new Bounds(center, _newRoomSize));
        }

        public void RemoveRoom()
        {
            Rooms.RemoveAt(Rooms.Count - 1);
        }

        public void GenerateAll()
        {
            // use random seed if seed == 0, otherwise use entered seed
            if(_seed == 0) Random.InitState(Random.Range(1, int.MaxValue));
            else Random.InitState(_seed);

            PlacePrefabs(Tileset.Tiles, "Tiles");
            PlacePrefabs(Tileset.Walls, "Walls");

            GetComponent<NavMeshSurface>()?.BuildNavMesh();
        }

        private void PlacePrefabs(PlaceableSet set, string containerName)
        {
            foreach (Bounds room in Rooms)
            {
                float sizeX = set.Size.x;
                float sizeZ = set.Size.z;
                int tileCountX = Mathf.RoundToInt(room.size.x / sizeX);
                int tileCountY = Mathf.RoundToInt(room.size.z /  sizeZ);
                Vector3 roomCorner = room.min;

                for (int x = 0; x < tileCountX; x++)
                {
                    for (int z = 0; z < tileCountY; z++) 
                    {
                        Vector3 corner = roomCorner + new Vector3(x * sizeX, 0f, z * sizeZ);
                        Vector3 center = corner + set.Size * 0.5f;
                        Vector3 flatCenter = center;
                        flatCenter.y = corner.y;
                        Bounds prefabBounds = new Bounds(center, set.Size);

                        if (!room.Contains(prefabBounds.min) || !room.Contains(prefabBounds.max)) continue;

                        if (set.WallCheckHeight <= 0f)   // basic object placement
                        {
                            Quaternion rotation = Quaternion.LookRotation(_directions[Random.Range(0, _directions.Length)]);
                            GameObject tile = null;
#if UNITY_EDITOR
                            tile = UnityEditor.PrefabUtility.InstantiatePrefab(set.Prefabs[Random.Range(0, set.Prefabs.Count)]) as GameObject;
#else
                            tile = Instantiate(set.Prefabs[Random.Range(0, set.Prefabs.Count)]);
#endif
                            tile.transform.SetPositionAndRotation(flatCenter, rotation);
                            tile.transform.SetParent(GetContainer(containerName));
                        }
                        else // extra check for walls/stairs/doors
                        {
                            foreach (Vector3 direction in _directions)
                            {
                                Vector3 wallTestPosition = center + direction * (sizeX * 0.5f + _gridSize * 0.5f);
                                wallTestPosition.y = corner.y + set.WallCheckHeight;
                                if(!TryGetContainingRoom(wallTestPosition, out Bounds wallroom))
                                {
                                    Quaternion rotation = Quaternion.LookRotation(-direction);
                                    GameObject tile = null;
#if UNITY_EDITOR
                                    tile = UnityEditor.PrefabUtility.InstantiatePrefab(set.Prefabs[Random.Range(0, set.Prefabs.Count)]) as GameObject;
#else
                                    tile = Instantiate(set.Prefabs[Random.Range(0, set.Prefabs.Count)]);
#endif
                                    tile.transform.SetPositionAndRotation(flatCenter, rotation);
                                    tile.transform.position += tile.transform.TransformVector(set.Offset);
                                    tile.transform.SetParent(GetContainer(containerName));
                                }
                            }
                        }
                    }
                }
            }
        }

        // check to see if point is contained in any room
        private bool TryGetContainingRoom(Vector3 position, out Bounds bounds)
        {
            foreach (Bounds room in Rooms)
            {
                if (room.Contains(position))
                {
                    bounds = room;
                    return true;
                }
            }

            bounds = new Bounds();
            return false;
        }
    }
}
