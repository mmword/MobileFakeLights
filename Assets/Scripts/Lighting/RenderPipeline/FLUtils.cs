//The MIT License (MIT)
//Copyright (c) 2018 Vladimir Aksenov
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Game.Rendering
{
    static class FLUtils
    {
        public static Material CreateEngineMaterial(Shader shader)
        {
            var mat = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            return mat;
        }

        public static Mesh CreateQuadMesh(bool uvStartsAtTop)
        {
            float topV, bottomV;
            if (uvStartsAtTop)
            {
                topV = 0.0f;
                bottomV = 1.0f;
            }
            else
            {
                topV = 1.0f;
                bottomV = 0.0f;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector3(-1.0f,  1.0f, 0.0f),
                new Vector3( 1.0f, -1.0f, 0.0f),
                new Vector3( 1.0f,  1.0f, 0.0f)
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0.0f, bottomV),
                new Vector2(0.0f, topV),
                new Vector2(1.0f, bottomV),
                new Vector2(1.0f, topV)
            };

            mesh.triangles = new int[] { 0, 1, 2, 2, 1, 3 };
            return mesh;
        }

    }

    static class CmdBufferPool
    {
        static Stack<CommandBuffer> pool = new Stack<CommandBuffer>();
        static int refCount = 0;

        public static CommandBuffer Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "fl pool buffer";
            CommandBuffer buffer = null;

            if (pool.Count < 1)
                buffer = new CommandBuffer();
            else
                buffer = pool.Pop();
            refCount++;
            buffer.name = name;
            return buffer;
        }

        public static CommandBuffer Get()
        {
            return Get(string.Empty);
        }

        public static void Release(CommandBuffer buffer)
        {
            buffer.Clear();
            pool.Push(buffer);
            refCount--;
        }

        public static int ReferencesCount
        {
            get
            {
                return refCount;
            }
        }
    }
}
