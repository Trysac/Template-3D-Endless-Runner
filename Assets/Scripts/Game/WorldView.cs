using System.Collections.Generic;
using GauntletRun.Core;
using UnityEngine;

namespace GauntletRun.Game
{
    /// <summary>
    /// Shows a run: the corridor around the knight, and a model for every obstacle and pickup the
    /// core keeps, matched by id. Models appear as the road is generated and go when it drops them.
    /// </summary>
    public sealed class WorldView
    {
        const int SegmentsBehind = 2;
        const int SegmentsAhead = 14;

        sealed class ItemView
        {
            public Transform Root;
            public Transform Pendulum;
            public float Spin;
        }

        readonly Transform root;
        readonly Transform corridor;
        readonly Transform items;
        readonly Dictionary<int, Transform> segments = new Dictionary<int, Transform>();
        readonly Dictionary<int, ItemView> obstacles = new Dictionary<int, ItemView>();
        readonly Dictionary<int, ItemView> pickups = new Dictionary<int, ItemView>();
        readonly HashSet<int> seen = new HashSet<int>();
        readonly List<int> stale = new List<int>();
        Run shown;

        public WorldView(Transform parent)
        {
            root = new GameObject("World").transform;
            root.SetParent(parent, false);
            corridor = new GameObject("Corridor").transform;
            corridor.SetParent(root, false);
            items = new GameObject("Items").transform;
            items.SetParent(root, false);
        }

        public int ObstacleCount => obstacles.Count;
        public int PickupCount => pickups.Count;
        public int SegmentCount => segments.Count;

        public Transform ObstacleView(int id) => obstacles.TryGetValue(id, out ItemView view) ? view.Root : null;

        public Transform PickupView(int id) => pickups.TryGetValue(id, out ItemView view) ? view.Root : null;

        public void Sync(Run run, float time)
        {
            if (run != shown)
            {
                Clear();
                shown = run;
            }
            if (run == null)
            {
                return;
            }
            SyncCorridor(run.Distance);
            SyncObstacles(run);
            SyncPickups(run, time);
        }

        void Clear()
        {
            foreach (ItemView view in obstacles.Values) Object.Destroy(view.Root.gameObject);
            foreach (ItemView view in pickups.Values) Object.Destroy(view.Root.gameObject);
            obstacles.Clear();
            pickups.Clear();
        }

        void SyncCorridor(float distance)
        {
            int first = Mathf.FloorToInt(distance / Models.SegmentLength) - SegmentsBehind;
            int last = first + SegmentsBehind + SegmentsAhead;
            stale.Clear();
            foreach (int index in segments.Keys)
            {
                if (index < first || index > last) stale.Add(index);
            }
            foreach (int index in stale)
            {
                Object.Destroy(segments[index].gameObject);
                segments.Remove(index);
            }
            for (int index = first; index <= last; index++)
            {
                if (!segments.ContainsKey(index))
                {
                    Transform segment = Models.Segment(corridor, index);
                    segment.localPosition = new Vector3(0f, 0f, index * Models.SegmentLength);
                    segments[index] = segment;
                }
            }
        }

        void SyncObstacles(Run run)
        {
            seen.Clear();
            foreach (Obstacle o in run.Obstacles)
            {
                seen.Add(o.Id);
                if (!obstacles.TryGetValue(o.Id, out ItemView view))
                {
                    view = new ItemView { Root = Models.Obstacle(o.Kind, items, o.Id, out Transform pendulum) };
                    view.Pendulum = pendulum;
                    view.Root.localPosition = new Vector3(Tuning.LaneX(o.Lane), 0f, o.Z);
                    obstacles[o.Id] = view;
                }
                if (o.Smashed && view.Root.gameObject.activeSelf)
                {
                    view.Root.gameObject.SetActive(false);
                }
                if (view.Pendulum != null)
                {
                    view.Pendulum.localEulerAngles = new Vector3(0f, 0f, -run.AxeAngle(o) * Mathf.Rad2Deg);
                }
            }
            Drop(obstacles);
        }

        void SyncPickups(Run run, float time)
        {
            seen.Clear();
            foreach (Pickup p in run.Pickups)
            {
                if (p.Taken)
                {
                    continue;
                }
                seen.Add(p.Id);
                if (!pickups.TryGetValue(p.Id, out ItemView view))
                {
                    view = new ItemView { Root = Models.Pickup(p.Kind, items), Spin = p.Id * 37f };
                    pickups[p.Id] = view;
                }
                float bob = p.Pulled || p.Kind == PickupKind.Coin ? 0f : Mathf.Sin(time * 3f + p.Id) * 0.12f;
                view.Root.localPosition = new Vector3(p.X, p.Y + bob, p.Z);
                // Coins spin; the others only sway, so their glow never turns edge-on.
                float turn = p.Kind == PickupKind.Coin ? view.Spin + time * 220f : Mathf.Sin(time * 2f + p.Id) * 35f;
                view.Root.localEulerAngles = new Vector3(0f, turn, 0f);
            }
            Drop(pickups);
        }

        void Drop(Dictionary<int, ItemView> views)
        {
            stale.Clear();
            foreach (int id in views.Keys)
            {
                if (!seen.Contains(id)) stale.Add(id);
            }
            foreach (int id in stale)
            {
                Object.Destroy(views[id].Root.gameObject);
                views.Remove(id);
            }
        }
    }
}
