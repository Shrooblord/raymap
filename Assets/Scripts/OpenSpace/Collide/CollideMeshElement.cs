﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenSpace.Collide {
    public class CollideMeshElement : ICollideGeometricElement {
        public CollideMeshObject mesh;
        public Pointer offset;

        public Pointer off_material;
        public Pointer off_triangles; // num_triangles * 3 * 0x2
        public Pointer off_mapping; // num_triangles * 3 * 0x2. Max: num_uvs-1
        public Pointer off_normals; // num_triangles * 3 * 0x4. 1 normal per face, kinda logical for collision I guess
        public Pointer off_uvs;
        public ushort num_triangles;
        public ushort num_mapping;
        public Pointer off_unk;
        public Pointer off_unk2;
        public ushort num_mapping_entries;

        public GameMaterial gameMaterial;
        public int[] vertex_indices = null;
        public Vector3[] normals = null;
        public int[] mapping = null;
        public Vector2[] uvs = null;


        private GameObject gao = null;
        public GameObject Gao {
            get {
                if (gao == null) {
                    gao = new GameObject("Collide Submesh @ " + offset);// Create object and read triangle data
                    CreateUnityMesh();
                }
                return gao;
            }
        }

        public CollideMeshElement(Pointer offset, CollideMeshObject mesh) {
            this.mesh = mesh;
            this.offset = offset;
        }

        private void CreateUnityMesh() {
            if(num_triangles > 0) {
                Vector3[] new_vertices = new Vector3[num_triangles * 3];
                Vector3[] new_normals = new Vector3[num_triangles * 3];
                Vector2[] new_uvs = new Vector2[num_triangles * 3];

                for (int j = 0; j < num_triangles * 3; j++) {
                    new_vertices[j] = mesh.vertices[vertex_indices[j]];
                    new_normals[j] = normals[j/3];
                    if (uvs != null) new_uvs[j] = uvs[mapping[j]];
                }
                int[] triangles = new int[num_triangles * 3];
                for (int j = 0; j < num_triangles; j++) {
                    triangles[(j * 3) + 0] = (j * 3) + 0;
                    triangles[(j * 3) + 1] = (j * 3) + 2;
                    triangles[(j * 3) + 2] = (j * 3) + 1;
                }
                Mesh meshUnity = new Mesh();
                meshUnity.vertices = new_vertices;
                meshUnity.normals = new_normals;
                meshUnity.triangles = triangles;
                if (uvs != null) meshUnity.uv = new_uvs;
                MeshFilter mf = gao.AddComponent<MeshFilter>();
                mf.mesh = meshUnity;
                MeshRenderer mr = gao.AddComponent<MeshRenderer>();
                mr.material = MapLoader.Loader.baseMaterial;
                //mr.material.SetTexture("_MainTex", Util.CreateDummyCheckerTexture());
                if (gameMaterial != null && gameMaterial.collideMaterial != null) {
                    CollideMaterial cm = gameMaterial.collideMaterial;
                    if (cm.NoCollision) {
                        mr.material = MapLoader.Loader.baseTransparentMaterial;
                        //mr.material.SetTexture("_MainTex", Util.CreateDummyCheckerTexture());
                        mr.material.color = new Color(1, 1, 1, 0.3f); // transparent cyan
                    }
                    if (cm.Slide) mr.material.color = Color.blue;
                    if (cm.Water) {
                        mr.material = MapLoader.Loader.baseTransparentMaterial;
                        //mr.material.SetTexture("_MainTex", Util.CreateDummyCheckerTexture());
                        mr.material.color = new Color(0, 1, 1, 0.5f); // transparent cyan
                    }
                    if (cm.ClimbableWall || cm.HangableCeiling) {
                        mr.material.color = new Color(244f / 255f, 131f / 255f, 66f / 255f); // ORANGE
                    }
                    if (cm.LavaDeathWarp || cm.DeathWarp) {
                        mr.material.color = Color.red;
                    }
                    if (cm.HurtTrigger) mr.material.color = new Color(126 / 255f, 2 / 255f, 204 / 255f); // purple
                    if (cm.FallTrigger) mr.material.color = Color.black;
                    if (cm.Trampoline) mr.material.color = Color.yellow;
                    if (cm.Electric) mr.material.color = new Color(219f / 255f, 140 / 255f, 212 / 255f); // pink
                    if (cm.Wall) mr.material.color = Color.grey;
                    if (cm.GrabbableLedge) mr.material.color = Color.green;
                    if (cm.FlagUnknown || cm.FlagUnk2 || cm.FlagUnk3) mr.material.color = new Color(124 / 255f, 68 / 255f, 33 / 255f); // brown
                }
            }
        }

        public static CollideMeshElement Read(EndianBinaryReader reader, Pointer offset, CollideMeshObject m) {
            MapLoader l = MapLoader.Loader;
            CollideMeshElement sm = new CollideMeshElement(offset, m);
            //l.print(offset + " - " + m.num_vertices);
            sm.off_material = Pointer.Read(reader);
            if (Settings.s.engineMode == Settings.EngineMode.R2) {
                sm.num_triangles = reader.ReadUInt16();
                sm.num_mapping = reader.ReadUInt16();
                sm.off_triangles = Pointer.Read(reader);
                sm.off_mapping = Pointer.Read(reader);
                sm.off_normals = Pointer.Read(reader);
                sm.off_uvs = Pointer.Read(reader);
                Pointer.Read(reader); // table of num_unk vertex indices (vertices, because max = num_vertices - 1)
                reader.ReadUInt16(); // num_unk
                reader.ReadUInt16();
            } else {
                sm.off_triangles = Pointer.Read(reader);
                sm.off_normals = Pointer.Read(reader);
                sm.num_triangles = reader.ReadUInt16();
                reader.ReadUInt16();
                reader.ReadUInt32();
                sm.off_mapping = Pointer.Read(reader);
                sm.off_unk = Pointer.Read(reader); // num_mapping_entries * 3 floats 
                sm.off_unk2 = Pointer.Read(reader); // num_mapping_entries * 1 float
                sm.num_mapping = reader.ReadUInt16();
                reader.ReadUInt16();
            }

            if(sm.off_material != null) sm.gameMaterial = GameMaterial.FromOffsetOrRead(sm.off_material, reader);
            Pointer.Goto(ref reader, sm.off_triangles);
            sm.vertex_indices = new int[sm.num_triangles * 3];
            for (int j = 0; j < sm.num_triangles; j++) {
                sm.vertex_indices[(j * 3) + 0] = reader.ReadInt16();
                sm.vertex_indices[(j * 3) + 1] = reader.ReadInt16();
                sm.vertex_indices[(j * 3) + 2] = reader.ReadInt16();
            }
            Pointer.Goto(ref reader, sm.off_normals);
            sm.normals = new Vector3[sm.num_triangles];
            for (int j = 0; j < sm.num_triangles; j++) {
                float x = reader.ReadSingle();
                float z = reader.ReadSingle();
                float y = reader.ReadSingle();
                sm.normals[j] = new Vector3(x, y, z);
            }

            if (sm.num_mapping > 0 && sm.off_mapping != null) {
                Pointer.Goto(ref reader, sm.off_mapping);
                sm.mapping = new int[sm.num_triangles * 3];
                for (int i = 0; i < sm.num_triangles; i++) {
                    sm.mapping[(i * 3) + 0] = reader.ReadInt16();
                    sm.mapping[(i * 3) + 1] = reader.ReadInt16();
                    sm.mapping[(i * 3) + 2] = reader.ReadInt16();
                }
                if (sm.off_uvs != null) {
                    Pointer.Goto(ref reader, sm.off_uvs);
                    sm.uvs = new Vector2[sm.num_mapping];
                    for (int i = 0; i < sm.num_mapping; i++) {
                        sm.uvs[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                    }
                }
            }

            /*R3Pointer.Goto(ref reader, sm.off_mapping);
            sm.mapping = new int[sm.num_triangles * 3];
            for (int j = 0; j < sm.num_triangles; j++) {
                sm.mapping[(j * 3) + 0] = reader.ReadInt16();
                sm.mapping[(j * 3) + 1] = reader.ReadInt16();
                sm.mapping[(j * 3) + 2] = reader.ReadInt16();
            }
            R3Pointer.Goto(ref reader, sm.off_unk);
            sm.normals = new Vector3[sm.num_mapping_entries];
            for (int j = 0; j < sm.num_mapping_entries; j++) {
                float x = reader.ReadSingle();
                float z = reader.ReadSingle();
                float y = reader.ReadSingle();
                sm.normals[j] = new Vector3(x, y, z);
            }*/
            return sm;
        }

        // Call after clone
        public void Reset() {
            gao = null;
        }

        public ICollideGeometricElement Clone(CollideMeshObject mesh) {
            CollideMeshElement sm = (CollideMeshElement)MemberwiseClone();
            sm.mesh = mesh;
            sm.Reset();
            return sm;
        }
    }
}
