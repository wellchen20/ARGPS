using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ARLocation
{
    public class WorldVoxelController : MonoBehaviour
    {
        // Start is called before the first frame update
        public PrefabDatabase PrefabDatabase;

        [System.Serializable]
        class Voxel
        {
            public string PrefabId;
            public int i, j, k;

            [System.NonSerialized]
            public GameObject Instance;
        }

        [System.Serializable]
        class WorldChunk
        {
            public List<Voxel> Voxels = new List<Voxel>();
            public Location ChunkLocation;
            public float ChunkRotation;
            public bool HasLocation;
            public int Length;

            [System.NonSerialized]
            public Vector3 Origin;

            [System.NonSerialized]
            public GameObject ChunkContainer;

            [System.NonSerialized]
            public Bounds Bounds;
        }

        [System.Serializable]
        class World
        {
            public List<WorldChunk> Chunks = new List<WorldChunk>();
        }

        [System.Serializable]
        public class ElementsSettingsData
        {
            public Button ClearWorldBtn;
            public Button PickAxeBtn;
            public Button BrickBtn;
            public Button StoneBtn;
            public Button GoldBtn;
            public AudioClip Create;
            public AudioClip Destroy;
            public ParticleSystem BrickParticle;
            public GameObject IndicatorPlane;
        }

        public enum Tools
        {
            PickAxe,
            Block
        }

        public enum Blocks
        {
            Brick,
            Stone,
            Gold
        }


        private Tools currentTool = Tools.Block;
        private Blocks currentBlock = Blocks.Brick;

        [System.Serializable]
        public class RaycastMarginSettings
        {
            public float Top;
            public float Bottom;
            public float Left;
            public float Right;

            public bool IsInside(Vector2 v)
            {
                if (v.x < Left || v.x > (1 - Right)) return false;
                if (v.y < Bottom || v.y > (1 - Top)) return false;

                return true;
            }
        }

        [System.Serializable]
        public class SettingsData
        {
            public float CunkScale = 1.0f;
        }

        public ElementsSettingsData Elements;
        public RaycastMarginSettings RaycastMargins;
        public SettingsData Settings;

        private World world = new World();

        void Start()
        {
            if (!RestoreWorldFromLocalStorage() || world.Chunks.Count == 0)
            {
                WorldChunk test = new WorldChunk
                {
                    Voxels = new List<Voxel>
            {
                new Voxel
                {
                    PrefabId = "Brick",
                    Instance = null,
                    i = 0, j = 0, k = 0
                }
            },
                    Origin = new Vector3(0, -1.4f, 4),
                    Length = 100
                };

                for (int i = 1; i < 10; i++)
                {
                    test.Voxels.Add(new Voxel { PrefabId = "Brick", i = i, j = 0, k = 0 });
                }

                for (int i = 1; i < 10; i++)
                {
                    test.Voxels.Add(new Voxel { PrefabId = "Brick", i = i, j = 0, k = 9 });
                }

                for (int i = 1; i < 10; i++)
                {
                    test.Voxels.Add(new Voxel { PrefabId = "Brick", i = 0, j = 0, k = i });
                }

                for (int i = 1; i < 10; i++)
                {
                    test.Voxels.Add(new Voxel { PrefabId = "Brick", i = 9, j = 0, k = i });
                }

                world.Chunks.Add(test);

                BuildWorld();
            }

            ARLocationProvider.Instance.OnLocationUpdated.AddListener(OnLocationUpdatedListener);

            InitUiListeners();

            Utils.Misc.HideGameObject(Elements.IndicatorPlane);
        }

        private void InitUiListeners()
        {
            Elements.ClearWorldBtn.onClick.AddListener(() =>
            {
                ClearWorld();
            });

            Elements.PickAxeBtn.onClick.AddListener(() =>
            {
                SetCurrentTool(Tools.PickAxe);
            });

            Elements.BrickBtn.onClick.AddListener(() =>
            {
                SetCurrentBlock(Blocks.Brick);
            });

            Elements.StoneBtn.onClick.AddListener(() =>
            {
                SetCurrentBlock(Blocks.Stone);
            });

            Elements.GoldBtn.onClick.AddListener(() =>
            {
                SetCurrentBlock(Blocks.Gold);
            });
        }

        private void SetCurrentBlock(Blocks b)
        {
            SetCurrentTool(Tools.Block);
            currentBlock = b;
        }

        private string GetMeshIdForBlock(Blocks b)
        {
            switch (b)
            {
                case Blocks.Brick:
                    return "Brick";
                case Blocks.Stone:
                    return "Stone";
                case Blocks.Gold:
                    return "Gold";
                default:
                    return "";
            }
        }

        private void SetCurrentTool(Tools t)
        {
            currentTool = t;
        }

        private void OnLocationUpdatedListener(Location l)
        {
            world.Chunks.ForEach(c =>
            {
                c.ChunkLocation = ARLocationProvider.Instance.GetLocationForWorldPosition(c.ChunkContainer.transform.position);
                c.ChunkLocation.Altitude = 0;
                c.ChunkLocation.AltitudeMode = AltitudeMode.GroundRelative;
                var arLocationRoot = ARLocationManager.Instance.gameObject.transform;
                float angle = Vector3.SignedAngle(c.ChunkContainer.transform.forward, arLocationRoot.forward, new Vector3(0, 1, 0));
                c.ChunkRotation = angle;
                c.HasLocation = true;
            });
        }

        string GetJsonFilename()
        {
            var s = "WorldVoxelCraft";

            return Application.persistentDataPath + "/" + s + ".json";
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) SaveWorldToLocalStorage();
        }

        private void OnDestroy()
        {
            SaveWorldToLocalStorage();
        }

        void SaveWorldToLocalStorage()
        {
            var json = JsonUtility.ToJson(world);

            try
            {
                System.IO.File.WriteAllText(GetJsonFilename(), json);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::SaveWorld]: Failed to open json file for writing.");
                return;
            }

            Debug.Log("Saved " + GetJsonFilename());
        }

        bool RestoreWorldFromLocalStorage()
        { 
            string json = "";
            try
            {
                json = System.IO.File.ReadAllText(GetJsonFilename(), System.Text.Encoding.UTF8);
            }
            catch
            {
                Debug.Log("[ARLocation::WorldBuilder::RestoreWorld]: Failed to open json file for reading.");
                //RandomPopulateWorld();
                return false;
            }

            world = JsonUtility.FromJson<World>(json);
            BuildWorld();
            Debug.Log($"Restored world from json file '{GetJsonFilename()}'");

            return true;
        }

        void BuildWorld()
        {
            world.Chunks.ForEach(BuildChunk);
        }


        // Update is called once per frame
        void Update()
        {



            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            // todo: sort chunks
            if ((Application.isEditor && Input.GetMouseButtonDown(0)) || (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began))
            {
                if (Application.isEditor)
                {
                    var p = new Vector2(Input.mousePosition.x, Input.mousePosition.y) / new Vector2(Screen.width, Screen.height);
                    if (!RaycastMargins.IsInside(p)) return;
                }
                else
                {
                    var p = Input.touches[0].position / new Vector2(Screen.width, Screen.height);
                    if (!RaycastMargins.IsInside(p)) return;
                }

                foreach (var c in world.Chunks)
                {
                    VoxelHit h;
                    if (RaycastChunk(ray, c, out h))
                    {
                        if (currentTool == Tools.PickAxe)
                        {
                            //RemoveVoxelFromChunk(c, h.Voxel);
                            var i = Instantiate(Elements.BrickParticle);
                            i.transform.position = h.Voxel.Instance.transform.position;
                            i.GetComponent<ParticleSystemRenderer>().material = h.Voxel.Instance.GetComponent<MeshRenderer>().material;
                            i.Play();
                            RemoveVoxelFromChunk(c, h.Voxel);

                            var a = Camera.main.GetComponent<AudioSource>();
                            if (a)
                            {
                                a.clip = Elements.Destroy;
                                a.Play();
                            }
                        }
                        else
                        {
                            var Normal = c.ChunkContainer.transform.InverseTransformDirection(h.Normal);
                            AddVoxelToChunk(c, GetMeshIdForBlock(currentBlock), h.Voxel.i + Mathf.FloorToInt(Normal.x), h.Voxel.j + Mathf.FloorToInt(Normal.y), h.Voxel.k + Mathf.FloorToInt(Normal.z));
                            var a = Camera.main.GetComponent<AudioSource>();
                            if (a)
                            {
                                a.clip = Elements.Create;
                                a.Play();
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var c in world.Chunks)
                {
                    VoxelHit h;
                    if (RaycastChunk(ray, c, out h))
                    {
                        var Normal = c.ChunkContainer.transform.InverseTransformDirection(h.Normal);
                        Elements.IndicatorPlane.transform.SetParent(c.ChunkContainer.transform);
                        
                        Elements.IndicatorPlane.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        Elements.IndicatorPlane.transform.localEulerAngles = new Vector3(0, 0, 0);
                        Elements.IndicatorPlane.transform.localPosition = new Vector3(h.Voxel.i + Mathf.FloorToInt(Normal.x) * 0.505f, h.Voxel.j + Mathf.FloorToInt(Normal.y) * 0.505f, h.Voxel.k + Mathf.FloorToInt(Normal.z) * 0.505f);
                        //Elements.IndicatorPlane.transform.localRotation = Quaternion.FromToRotation(new Vector3(0, 1, 0), Normal);

                        if (Mathf.Abs(Normal.x) < 0.0001f && Mathf.Abs(Normal.y) < 0.0001f)
                        {
                            float sign = Normal.z < 0 ? -1.0f : 1.0f;
                            Elements.IndicatorPlane.transform.localEulerAngles = new Vector3(sign * 90.0f, 0, 0);
                        } else if (Mathf.Abs(Normal.z) < 0.0001f && Mathf.Abs(Normal.y) < 0.0001f)
                        {
                            float sign = Normal.x < 0 ? 1.0f : -1.0f;
                            Elements.IndicatorPlane.transform.localEulerAngles = new Vector3(0, 0, sign * 90.0f);
                        }

                        Debug.Log(Normal);
                        Utils.Misc.ShowGameObject(Elements.IndicatorPlane);
                    }
                    else
                    {
                        Utils.Misc.HideGameObject(Elements.IndicatorPlane);
                    }
                }
            }
        }

        private void RemoveVoxelFromChunk(WorldChunk c, Voxel voxel)
        {
            c.Voxels.Remove(voxel);
            Destroy(voxel.Instance);
        }

        void AddVoxelToChunk(WorldChunk c, string PrefabId, int i, int j, int k)
        {
            Voxel v = new Voxel { PrefabId = PrefabId, i = i, j = j, k = k };
            v.Instance = Instantiate(PrefabDatabase.GetEntryById(PrefabId), c.ChunkContainer.transform);
            v.Instance.transform.localPosition = new Vector3(v.i, v.j, v.k);
            c.Voxels.Add(v);
        }

        struct VoxelHit
        {
            public Voxel Voxel;
            public Vector3 Normal;
        }

        bool RaycastChunk(Ray ray, WorldChunk chunk, out VoxelHit hit)
        {
            //if (!chunk.Bounds.IntersectRay(ray)) {
            //    hit = new VoxelHit();
            //    return false;
            //}

            VoxelHit currentHit = new VoxelHit();
            float currentDistance = 0;
            bool hasHit = false;
            foreach (var v in chunk.Voxels)
            {
                if (v.Instance)
                {
                    var collider = v.Instance.GetComponent<BoxCollider>();

                    Debug.Assert(collider);

                    RaycastHit h;
                    if (collider.Raycast(ray, out h, chunk.Length))
                    {
                        if (!hasHit || h.distance < currentDistance)
                        {
                            hasHit = true;
                            currentDistance = h.distance;
                            currentHit = new VoxelHit { Voxel = v, Normal = h.normal };
                        }
                    }
                }
            }

            hit = currentHit;

            if (hasHit)
            {
                hit = currentHit;
                return true;
            }
            else
            {
                var chunkOrigin = chunk.ChunkContainer.transform.position;
                var plane = new Plane(new Vector3(0, 1, 0), -chunkOrigin.y + 0.5f * Settings.CunkScale);
                float d;
                if (plane.Raycast(ray, out d))
                {
                    var p = chunk.ChunkContainer.transform.InverseTransformPoint(ray.GetPoint(d));

                    var i = Mathf.FloorToInt(p.x + 0.5f);
                    var j = -1;
                    var k = Mathf.FloorToInt(p.z + 0.5f);

                    hit = new VoxelHit { Voxel = new Voxel { PrefabId = "", i = i, j = j, k = k } , Normal = new Vector3(0, 1, 0) };

                    return true;
                }
            }

            return false;
        }

        void BuildChunk(WorldChunk chunk)
        {
            chunk.ChunkContainer = new GameObject();
            chunk.ChunkContainer.transform.localScale = new Vector3(Settings.CunkScale, Settings.CunkScale, Settings.CunkScale);
            if (chunk.HasLocation)
            {
                PlaceAtLocation.AddPlaceAtComponent(chunk.ChunkContainer, chunk.ChunkLocation, new PlaceAtLocation.PlaceAtOptions { });
                chunk.ChunkContainer.transform.localEulerAngles = new Vector3(0, chunk.ChunkRotation, 0);
            }
            else
            {
                chunk.ChunkContainer.transform.position = chunk.Origin;
                chunk.Bounds = new Bounds(chunk.Origin, new Vector3(chunk.Length, chunk.Length, chunk.Length));
            }

            foreach (var v in chunk.Voxels)
            {
                v.Instance = Instantiate(PrefabDatabase.GetEntryById(v.PrefabId), chunk.ChunkContainer.transform);
                v.Instance.transform.localPosition = new Vector3(v.i, v.j, v.k);
            }
        }

        void ClearChunk(WorldChunk chunk)
        {
            foreach (var v in chunk.Voxels)
            {
                Destroy(v.Instance);
            }

            chunk.Voxels = new List<Voxel>();
        }

        void ClearWorld()
        {
            world.Chunks.ForEach(ClearChunk);

            world.Chunks = new List<WorldChunk>();
        }
    }
}
