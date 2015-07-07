﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a 3D object composed of one or more <see cref="BXDAMesh.BXDASubMesh"/> and physical properties of the object.
/// </summary>
public class BXDAMesh : RWObject
{

    /// <summary>
    /// The physical properties of this object.
    /// </summary>
    public PhysicalProperties physics
    {
        get;
        private set;
    }

    /// <summary>
    /// This object's sub meshes.
    /// </summary>
    public List<BXDASubMesh> meshes
    {
        get;
        private set;
    }

    /// <summary>
    /// This object's collision meshes.
    /// </summary>
    public List<BXDASubMesh> colliders
    {
        get;
        private set;
    }

    /// <summary>
    /// Creates an empty BXDA Mesh.
    /// </summary>
    public BXDAMesh()
    {
        physics = new PhysicalProperties();
        meshes = new List<BXDASubMesh>();
        colliders = new List<BXDASubMesh>();
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(BXDIO.FORMAT_VERSION);
        WriteMeshList(writer, meshes);
        WriteMeshList(writer, colliders);
        physics.WriteData(writer);
    }

    public void ReadData(BinaryReader reader)
    {
        // Sanity check
        uint version = reader.ReadUInt32();
        BXDIO.CheckReadVersion(version);
        meshes.Clear();
        colliders.Clear();
        ReadMeshList(reader, meshes);
        ReadMeshList(reader, colliders);

        physics.ReadData(reader);
    }

    /// <summary>
    /// Writes all the sub meshes in the given list to the given stream.
    /// </summary>
    /// <param name="writer">Output stream</param>
    /// <param name="meshes">Mesh list</param>
    private static void WriteMeshList(BinaryWriter writer, List<BXDASubMesh> meshes)
    {
        writer.Write(meshes.Count);
        foreach (BXDASubMesh mesh in meshes)
        {
            mesh.WriteData(writer);
        }
    }

    /// <summary>
    /// Reads a list of meshes from the given stream, adding them to the list passed into this function.
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="meshes">List to output to</param>
    private static void ReadMeshList(BinaryReader reader, List<BXDASubMesh> meshes)
    {
        int meshCount = reader.ReadInt32();
        for (int id = 0; id < meshCount; id++)
        {
            BXDASubMesh mesh = new BXDASubMesh();
            mesh.ReadData(reader);
            meshes.Add(mesh);
        }
    }

    /// <summary>
    /// Represents an indexed triangle mesh with normals and optional colors and texture coordinates.
    /// </summary>
    public class BXDASubMesh : RWObject
    {

        /// <summary>
        /// Vertex positions.  Three values (X, Y, Z) per vertex.
        /// </summary>
        public double[] verts;

        /// <summary>
        /// Vertex normals.  Three values (X, Y, Z) composing one unit vector per vertex.
        /// </summary>
        public double[] norms;

        /// <summary>
        /// A list of indexed surfaces that make up the mesh
        /// </summary>
        public List<BXDASurface> surfaces = new List<BXDASurface>();

        public void WriteData(BinaryWriter writer)
        {
            int vertCount = verts.Length / 3;
            byte meshFlags = (byte)((norms != null ? 1 : 0));

            writer.Write(meshFlags);
            writer.WriteArray(verts, 0, vertCount * 3);
            if (norms != null)
            {
                writer.WriteArray(norms, 0, vertCount * 3);
            }

            writer.Write(surfaces.Count);
            foreach (BXDASurface surface in surfaces)
            {
                surface.WriteData(writer);
            }
        }

        public void ReadData(BinaryReader reader)
        {
            byte meshFlags = reader.ReadByte();
            norms = (meshFlags & 1) == 1 ? new double[1 * 3] : null;
            verts = reader.ReadArray<double>();
            if (norms != null)
            {
                norms = reader.ReadArray<double>();
            }

            int surfaceCount = reader.ReadInt32();
            for (int i = 0; i < surfaceCount; i++)
            {
                BXDASurface nextSurface = new BXDASurface();
                nextSurface.ReadData(reader);
                surfaces.Add(nextSurface);
            }
        }

    }

    public class BXDASurface : RWObject
    {

        public bool hasColor = false;
        /// <summary>
        /// The color of the material packed as an unsigned integer 
        /// </summary>
        public uint color = 0xFFFFFFFF;

        /// <summary>
        /// The transparency of the material.  [0-1]
        /// </summary>
        public float transparency;

        /// <summary>
        /// The translucency of the material.  [0-1]
        /// </summary>
        public float translucency;

        /// <summary>
        /// The specular intensity of the material.  [0-1]
        /// </summary>
        public float specular = 0;

        /// <summary>
        /// The zero based index buffer for this specific surface of the mesh.
        /// </summary>
        public int[] indicies;

        public void WriteData(BinaryWriter writer)
        {
            int facetCount = indicies.Length / 3;

            writer.Write(hasColor);
            if (hasColor)
            {
                writer.Write(color);
            }
            writer.Write(transparency);
            writer.Write(translucency);
            writer.Write(specular);

            writer.WriteArray(indicies, 0, facetCount * 3);
        }

        public void ReadData(BinaryReader reader)
        {
            hasColor = reader.ReadBoolean();

            if (hasColor)
            {
                color = reader.ReadUInt32();
            }
            transparency = reader.ReadSingle();
            translucency = reader.ReadSingle();
            specular = reader.ReadSingle();

            indicies = reader.ReadArray<Int32>();
        }

    }

}