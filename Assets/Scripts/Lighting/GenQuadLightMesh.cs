//The MIT License (MIT)
//Copyright (c) 2018 Vladimir Aksenov
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections.Generic;

namespace Game.Rendering
{
    // Required for static batching
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class GenQuadLightMesh : MonoBehaviour
    {
        public Color lightColor = Color.white;
        [Range(0, 1)]
        public float Shadowness= 0.5F;
        [Range(0, 1)]
        public float LightFadeOut = 0.5F;
        public float NLIntensity = 1;
        public float Brightness = 1;

        [Range(0,1)]
        public float FlickerIntensity = 0F;
        [Range(0, 1)]
        public float FlickerFrequency = 0F;

        public Mesh srcMesh;
        public bool useBatching = false;
        public bool isStatic = false;

        MeshRenderer ren;
        MeshFilter mFilter;
        Mesh dynamicMesh;
        bool wasBatched;

        List<Vector4> uv0 = new List<Vector4>(4);
        List<Vector4> uvs2 = new List<Vector4>(4);
        List<Vector4> uvs3 = new List<Vector4>(4);
        List<Color> colors = new List<Color>(4);
        Vector3 bakePos;
        Color bakeColor;
        Vector4 bakeParams;
        Vector2 bakedFlicker;

        void Batch()
        {
            if (!Application.isPlaying)
                return;
            if(useBatching)
            {
                Batching.Add(transform, dynamicMesh, ren.sharedMaterial);
                ren.enabled = false;
                wasBatched = true;
            }
        }

        void UnBatch()
        {
            if (!Application.isPlaying)
                return;
            if (wasBatched)
            {
                wasBatched = false;
                ren.enabled = true;
                Batching.Remove(transform, ren.sharedMaterial);
            }
        }

        void UpdateMesh()
        {
            if(dynamicMesh == null)
            {
                if (srcMesh == null)
                    return;
                ren = GetComponent<MeshRenderer>();
                mFilter = GetComponent<MeshFilter>();
                dynamicMesh = Instantiate(srcMesh);
                mFilter.sharedMesh = dynamicMesh;
                uv0.Clear();
                dynamicMesh.GetUVs(0, uv0);
            }
            uvs2.Clear();
            uvs3.Clear();
            colors.Clear();
            Vector3 pos = bakePos = transform.position;
            bakeColor = lightColor;
            bakeParams = new Vector4(Shadowness, LightFadeOut, NLIntensity, Brightness);
            bakedFlicker = new Vector2(FlickerFrequency, FlickerIntensity);
            for (int i = 0; i < dynamicMesh.vertexCount; ++i)
            {
                uvs2.Add(pos);
                uvs3.Add(bakeParams);
                colors.Add(lightColor);
                uv0[i] = new Vector4(uv0[i].x, uv0[i].y, FlickerFrequency,FlickerIntensity);
            }
            dynamicMesh.SetUVs(0, uv0);
            dynamicMesh.SetUVs(1, uvs2);
            dynamicMesh.SetUVs(2, uvs3);
            dynamicMesh.SetColors(colors);
            Batch();
        }

        void Release()
        {
            if (dynamicMesh != null)
                DestroyImmediate(dynamicMesh);
            mFilter.sharedMesh = srcMesh;
            UnBatch();
        }

        // Use this for initialization
        void OnEnable()
        {
            UpdateMesh();
        }

        private void Start()
        {
            UpdateMesh();
            if (gameObject.isStatic)
                isStatic = true;
        }

        private void OnDisable()
        {
            Release();
        }

        private void OnWillRenderObject()
        {
            if(isStatic && Application.isPlaying)
                return;
            if (transform.position != bakePos ||
                bakeColor != lightColor ||
                bakeParams.x != Shadowness ||
                bakeParams.y != LightFadeOut ||
                bakeParams.z != NLIntensity ||
                bakeParams.w != Brightness ||
                bakedFlicker.x != FlickerFrequency ||
                bakedFlicker.y != FlickerIntensity)
            {
                UpdateMesh();
            }
        }
    }
}