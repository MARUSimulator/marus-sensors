// Copyright 2022 Laboratory for Underwater Systems and Technologies (LABUST)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Marus.Utils;
using Marus.Core; // Added Core for IPointCloudSensor

namespace Marus.Sensors
{
    public class RaycastLidar : SensorBase, IPointCloudSensor // Implemented Interface
    {
        public int WidthRes = 1024;
        public int HeightRes = 16;
        public float MaxDistance = 100;
        public float MinDistance = 0.2f;
        public float VerticalFieldOfView = 30;
        public float HorizontalFieldOfView = 360;

        public ComputeShader pointCloudShader;
        public Material ParticleMaterial;

        public NativeArray<Vector3> Points;
        public NativeArray<LidarReading> Readings;

        RaycastJobHelper<LidarReading> _raycastHelper;
        Coroutine _coroutine;
        [HideInInspector] public List<LidarConfig> Configs;
        [HideInInspector] public int ConfigIndex = 0;
        [SerializeField] [HideInInspector] public NativeArray<(float, float)> _rayAngles;
        public List<RayInterval> _rayIntervals;
        public RayDefinitionType _rayType;

        // Decoupled Annotation fields
        private MonoBehaviour _saver;
        private bool saverExists;

        [HideInInspector]
        private Dictionary<int, int> colliderLayer;
        public LayerMask blackHoleLayers;

        // Interface events replacing PointCloudManager
        public event Action<GameObject, string, int, Material, ComputeShader> OnPointCloudInitialized;
        public event Action<NativeArray<Vector3>> OnPointCloudUpdated;

        void Start()
        {
            int totalRays = WidthRes * HeightRes;
            colliderLayer = new Dictionary<int, int>();

            if(blackHoleLayers != 0)
            {
                GameObject[] objects = Helpers.FindGameObjectsInLayerMask(blackHoleLayers);

                foreach (var obj in objects)
                {
                    Collider instId = obj.GetComponent<Collider>();
                    if(instId)
                    {
                        colliderLayer.Add(instId.GetInstanceID(),instId.gameObject.layer);
                    }
                }
            }

            // Decoupled instantiation via reflection
            _saver = GetComponent("PointCloudSegmentationSaver") as MonoBehaviour;
            saverExists = _saver != null;

            InitializeRayArray();
            Points = new NativeArray<Vector3>(totalRays, Allocator.Persistent);
            Readings = new NativeArray<LidarReading>(totalRays, Allocator.Persistent);

            var directionsLocal = RaycastJobHelper.CalculateRayDirections(_rayAngles);
            _raycastHelper = new RaycastJobHelper<LidarReading>(gameObject,
                directionsLocal,
                OnLidarHit,
                OnFinish,
                maxDistance:MaxDistance,
                minDistance:MinDistance,
                sampleFrequency:SampleFrequency);

            // Invoke event instead of hardcoding PointCloudManager
            OnPointCloudInitialized?.Invoke(gameObject, name + "_PointCloud", totalRays, ParticleMaterial, pointCloudShader);

            _coroutine = StartCoroutine(_raycastHelper.RaycastInLoop());
        }

        protected override void SampleSensor()
        {
            // Invoke event instead of hardcoding PointCloudManager
            OnPointCloudUpdated?.Invoke(Points);
            _raycastHelper.SampleFrequency = SampleFrequency;
        }

        private void OnFinish(NativeArray<Vector3> points, NativeArray<LidarReading> readings)
        {
            points.CopyTo(this.Points);
            readings.CopyTo(this.Readings);
            hasData = true;
        }

        private LidarReading OnLidarHit(RaycastHit hit, Vector3 direction, int index)
        {
            var reading = new LidarReading();

            // Decoupled Annotation lookup via reflection
            if (saverExists)
            {
                var field = _saver.GetType().GetField("objectClassesAndInstances");
                if (field != null)
                {
                    var dict = field.GetValue(_saver) as Dictionary<int, (int, int)>;
                    if (dict != null && dict.TryGetValue(hit.colliderInstanceID, out var value))
                    {
                        reading.ClassId = value.Item1;
                        reading.InstanceId = value.Item2;
                    }
                }
            }

            if (hit.colliderInstanceID is not 0)
            {
                reading.IsValid = true;
            }
            if(colliderLayer.Count > 0)
            {
                if(colliderLayer.TryGetValue(hit.colliderInstanceID, out int layer))
                {
                    reading.IsValid = false;
                }
            }

            // New PR Fields
            reading.Ring = index % HeightRes;
            reading.Time = (uint) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            reading.Intensity = 255;

            return reading;
        }

        public void ApplyLidarConfig()
        {
            if (_rayAngles.IsCreated)
            {
                _rayAngles.Dispose();
            }
            var cfg = Configs[ConfigIndex];
            MaxDistance = cfg.MaxRange;
            MinDistance = cfg.MinRange;
            WidthRes = cfg.HorizontalResolution;
            HeightRes = cfg.VerticalResolution;
            HorizontalFieldOfView = cfg.HorizontalFieldOfView;
            VerticalFieldOfView = cfg.VerticalFieldOfView;
            SampleFrequency = cfg.Frequency;
            _rayType = cfg.Type;
            _rayIntervals = cfg.RayIntervals;
            if (cfg.Type == RayDefinitionType.Angles)
            {
                HeightRes = cfg.ChannelAngles.Count;
            }
            else if (cfg.Type == RayDefinitionType.Intervals)
            {
                if (_rayIntervals is null)
                {
                    _rayIntervals = new List<RayInterval>();
                }
                else
                {
                    if (_rayIntervals.Count > 0)
                    {
                        HeightRes = _rayIntervals.Sum(x => x.NumberOfRays);
                        VerticalFieldOfView = _rayIntervals.Last().EndingAngle - _rayIntervals.First().StartingAngle;
                    }
                }
            }
        }

        public void InitializeRayArray()
        {
            var cfg = Configs[ConfigIndex];
            if (cfg.Type == RayDefinitionType.Intervals)
            {
                var angles = RaycastJobHelper.InitVerticalAnglesFromIntervals(_rayIntervals, WidthRes, HorizontalFieldOfView);
                _rayAngles = RaycastJobHelper.InitCustomRays(angles, cfg.HorizontalResolution, HorizontalFieldOfView);
            }
            else if (cfg.Type == RayDefinitionType.Uniform)
            {
                _rayAngles = RaycastJobHelper.InitUniformRays(WidthRes, HeightRes, HorizontalFieldOfView, VerticalFieldOfView);
            }
            else if (cfg.Type == RayDefinitionType.Angles)
            {
                HeightRes = cfg.ChannelAngles.Count;
                _rayAngles = RaycastJobHelper.InitCustomRays(cfg.ChannelAngles, cfg.HorizontalResolution, HorizontalFieldOfView);
            }
        }

        void OnDestroy()
        {
            _raycastHelper?.Dispose();
            if (Points.IsCreated) Points.Dispose();
            if (Readings.IsCreated) Readings.Dispose();
            if (_rayAngles.IsCreated) _rayAngles.Dispose();
        }
    }

    [System.Serializable]
    public class LidarConfig
    {
        public string Name;
        public RayDefinitionType Type;
        public float Frequency;
        public string FrameId;
        public float MaxRange;
        public float MinRange;
        public int HorizontalResolution;
        public int VerticalResolution;
        public float HorizontalFieldOfView;
        public float VerticalFieldOfView;
        public List<float> ChannelAngles;
        public List<RayInterval> RayIntervals;
    }

    public enum RayDefinitionType
    {
        Uniform,
        Intervals,
        Angles
    }
}