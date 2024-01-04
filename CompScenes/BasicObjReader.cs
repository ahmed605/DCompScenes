using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CompScenes
{
    // This is just for testing, DO NOT USE ON PRODUCTION CODE, it can only handle very very very few obj files 
    internal static class BasicObjReader
    {
        internal static (Vector3[] Vertices, uint[] Indices, Vector2[] UV) ReadFromString(string obj, Vector3? verticesModifier = null)
        {
            List<Vector3> vertices = new();
            List<Vector2> uvs = new();
            List<uint> indices = new();

            string[] lines = obj.Split("\n");
            foreach (string line in lines)
            {
                string[] parts = line.Split(' ');
                if (parts.Length is 4 or 3)
                {
                    if (parts[0] == "v")
                    {
                        Vector3 vertex = new(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));

                        if (verticesModifier is Vector3 vec)
                            vertex += vec;
                        
                        vertices.Add(vertex);
                    }
                    else if (parts[0] is "f")
                    {
                        indices.Add(uint.Parse(parts[1]) - 1);
                        indices.Add(uint.Parse(parts[2]) - 1);
                        indices.Add(uint.Parse(parts[3]) - 1);
                    }
                    else if (parts[0] == "vt")
                    {
                        uvs.Add(new(float.Parse(parts[1]), float.Parse(parts[2])));
                    }

                    // TODO: Load normals
                }
            }

            return (vertices.ToArray(), indices.ToArray(), uvs.ToArray());
        }
    }
}
