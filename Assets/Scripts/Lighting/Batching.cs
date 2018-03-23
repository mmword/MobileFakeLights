//The MIT License (MIT)
//Copyright (c) 2018 Vladimir Aksenov
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using UnityEngine;

public class Batching : MonoBehaviour
{
    class BatchElement
    {
        public Mesh mesh;
        public Transform element;
    }

    class BatchGroup
    {
        public bool RequireUpdate;
        public string name { get; private set; }
        public List<BatchElement> Elelemts { get; private set; }
        public Mesh BatchedMesh { get; private set; }
        public GameObject BatchedObject { get; private set; }
        public Material material { get; private set; }

        public void Batch()
        {
            if (Elelemts.Count < 1)
                return;

            MeshFilter batchedMF;
            MeshRenderer batchedMR;

            if (BatchedObject == null)
            {
                BatchedObject = new GameObject("batched group : " + name);
                batchedMF = BatchedObject.AddComponent<MeshFilter>();
                batchedMR = BatchedObject.AddComponent<MeshRenderer>();
            }
            else
            {
                batchedMF = BatchedObject.GetComponent<MeshFilter>();
                batchedMR = BatchedObject.GetComponent<MeshRenderer>();
            }

            if (BatchedMesh == null)
            {
                BatchedMesh = new Mesh();
                BatchedMesh.name = "combined batched mesh";
                batchedMF.sharedMesh = BatchedMesh;
            }
            else
                BatchedMesh.Clear();

            batchedMR.sharedMaterial = material;
            Vector3 average = Vector3.zero;

            for (int i = 0; i < Elelemts.Count; ++i)
                average += Elelemts[i].element.position;
            average *= (1F / Elelemts.Count);

            BatchedObject.transform.position = average;
            Matrix4x4 toEnt = BatchedObject.transform.worldToLocalMatrix;

            CombineInstance[] combine = new CombineInstance[Elelemts.Count];
            for(int i = 0;i < Elelemts.Count;++i)
            {
                combine[i] = new CombineInstance();
                combine[i].mesh = Elelemts[i].mesh;
                combine[i].transform = toEnt * Elelemts[i].element.localToWorldMatrix;
            }

            BatchedMesh.CombineMeshes(combine);
            BatchedMesh.RecalculateBounds();
        }

        public void Release()
        {
            if (BatchedObject != null)
                DestroyImmediate(BatchedObject);
            if (BatchedMesh != null)
                DestroyImmediate(BatchedMesh);
            Elelemts.Clear();
        }

        public BatchGroup(string name, Material material)
        {
            Elelemts = new List<BatchElement>();
            this.name = name;
            this.material = material;
            RequireUpdate = false;
        }
    }

    static Batching _Instance;

    Dictionary<Material, BatchGroup> GroupsByMaterial = new Dictionary<Material, BatchGroup>();
    bool ComponentHasDestroy;
    int reqToUpdate;

    BatchGroup AllocGroup(string name,Material mat)
    {
        BatchGroup group = null;
        if(!GroupsByMaterial.TryGetValue(mat,out group))
        {
            GroupsByMaterial[mat] = group = new BatchGroup(name, mat);
        }
        return group;
    }

    public static void Add(Transform el,Mesh mesh, Material mt)
    {
        if (_Instance == null)
            return;

        var gr = _Instance.AllocGroup(mt.name, mt);
        int idx = gr.Elelemts.FindIndex(x => x.element == el);
        if (idx < 0)
        {
            BatchElement be = new BatchElement()
            {
                element = el,
                mesh = mesh,
            };
            gr.Elelemts.Add(be);
        }
        gr.RequireUpdate = true;
        _Instance.reqToUpdate++;
    }
    
    public static void Remove(Transform el, Material mt)
    {
        if (_Instance == null)
            return;

        BatchGroup group = null;
        if (!_Instance.ComponentHasDestroy && _Instance.GroupsByMaterial.TryGetValue(mt, out group))
        {
            int idx = group.Elelemts.FindIndex(x => x.element == el);
            if(idx >= 0)
            {
                group.Elelemts.RemoveAt(idx);
                if (group.Elelemts.Count < 1)
                    group.Release();
                else
                {
                    group.RequireUpdate = true;
                    _Instance.reqToUpdate++;
                }
            }
        }
    }

    private void Update()
    {
        if(reqToUpdate > 0)
        {
            reqToUpdate = 0;
            foreach (var gr in GroupsByMaterial.Values)
            {
                if(gr.RequireUpdate)
                {
                    gr.Batch();
                    gr.RequireUpdate = false;
                }
            }
        }
    }

    private void Awake()
    {
        if(_Instance != null)
        {
            Debug.LogError("Batching can have only one instance");
            enabled = false;
            return;
        }
        _Instance = this;
        reqToUpdate = 0;
    }

    private void OnDestroy()
    {
        ComponentHasDestroy = true;
        if(GroupsByMaterial.Count > 0)
        {
            foreach (var gr in GroupsByMaterial.Values)
                gr.Release();
        }
        GroupsByMaterial.Clear();
    }

}
