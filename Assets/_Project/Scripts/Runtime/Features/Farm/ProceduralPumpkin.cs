using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Runtime.Features.Farm
{
    using UnityEngine;

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralPumpkin : MonoBehaviour
    {
        [Header("Настройки формы тыквы")]
        [SerializeField] private int _segments = 24;
        [SerializeField] private int _rings = 18;
        [SerializeField] private float _radius = 0.5f;
        [SerializeField] private float _flattening = 0.65f;
        [SerializeField] private float _bumpIntensity = 0.12f;

        [Header("Настройки хвостика")]
        [SerializeField] private float _stemHeight = 0.25f;
        [SerializeField] private float _stemRadius = 0.04f;
        [SerializeField] private int _stemSegments = 8;
        [SerializeField] private float _stemCurve = 0.15f;

        void Start()
        {
            GeneratePumpkinMesh();
        }

        // Этот атрибут создает кнопку в контекстном меню компонента (три точки / шестеренка справа в инспекторе)
        [ContextMenu("Generate Pumpkin")]
        public void GeneratePumpkinMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            // Создаем новый меш
            Mesh mesh = new Mesh();
            mesh.name = "ProceduralPumpkinWithStem";

            // --- 1. Генерация вершин тыквы ---
            int pumpkinVerticesCount = (_rings + 1) * (_segments + 1);
            Vector3[] pumpkinVertices = new Vector3[pumpkinVerticesCount];
            Vector2[] pumpkinUV = new Vector2[pumpkinVerticesCount];
            int[] pumpkinTriangles = new int[_rings * _segments * 6];

            int v = 0;
            int t = 0;

            for (int y = 0; y <= _rings; y++)
            {
                float vPercent = (float)y / _rings;
                float phi = vPercent * Mathf.PI;

                for (int x = 0; x <= _segments; x++)
                {
                    float uPercent = (float)x / _segments;
                    float theta = uPercent * Mathf.PI * 2f;

                    float sinPhi = Mathf.Sin(phi);
                    float cosPhi = Mathf.Cos(phi);

                    float bump = Mathf.Sin(theta * _segments);
                    float shapeFactor = sinPhi;
                    float currentRadius = _radius + (bump * _bumpIntensity * shapeFactor);

                    float posX = currentRadius * sinPhi * Mathf.Cos(theta);
                    float posY = _radius * cosPhi * _flattening;
                    if (y < _rings / 2) posY -= (1f - sinPhi) * 0.1f * _radius;

                    float posZ = currentRadius * sinPhi * Mathf.Sin(theta);

                    pumpkinVertices[v] = new Vector3(posX, posY, posZ);
                    pumpkinUV[v] = new Vector2(uPercent, vPercent);

                    if (y < _rings && x < _segments)
                    {
                        int current = v;
                        int next = v + 1;
                        int rowNext = v + _segments + 1;
                        int rowNextNext = v + _segments + 2;

                        pumpkinTriangles[t++] = current;
                        pumpkinTriangles[t++] = rowNext;
                        pumpkinTriangles[t++] = next;

                        pumpkinTriangles[t++] = next;
                        pumpkinTriangles[t++] = rowNext;
                        pumpkinTriangles[t++] = rowNextNext;
                    }
                    v++;
                }
            }

            // --- 2. Генерация вершин хвостика ---
            int stemRings = 4;
            int stemVerticesCount = (stemRings + 1) * (_stemSegments + 1);
            Vector3[] stemVertices = new Vector3[stemVerticesCount];
            Vector2[] stemUV = new Vector2[stemVerticesCount];
            int[] stemTriangles = new int[stemRings * _stemSegments * 6];

            float pumpkinTopY = _radius * _flattening - (1f - Mathf.Sin(0)) * 0.1f * _radius;
            Vector3 stemBaseOffset = new Vector3(0, pumpkinTopY - 0.02f, 0);

            int sv = 0;
            int st = 0;

            for (int y = 0; y <= stemRings; y++)
            {
                float vPercent = (float)y / stemRings;
                float currentHeight = vPercent * _stemHeight;
                float curveOffset = Mathf.Pow(vPercent, 2) * _stemCurve;
                float currentRadius = Mathf.Lerp(_stemRadius * 1.5f, _stemRadius * 0.6f, vPercent);

                for (int x = 0; x <= _stemSegments; x++)
                {
                    float uPercent = (float)x / _stemSegments;
                    float theta = uPercent * Mathf.PI * 2f;

                    float posX = currentRadius * Mathf.Cos(theta) + curveOffset;
                    float posY = currentHeight;
                    float posZ = currentRadius * Mathf.Sin(theta);

                    stemVertices[sv] = new Vector3(posX, posY, posZ) + stemBaseOffset;
                    stemUV[sv] = new Vector2(uPercent, vPercent);

                    if (y < stemRings && x < _stemSegments)
                    {
                        int current = sv;
                        int next = sv + 1;
                        int rowNext = sv + _stemSegments + 1;
                        int rowNextNext = sv + _stemSegments + 2;

                        stemTriangles[st++] = current;
                        stemTriangles[st++] = next;
                        stemTriangles[st++] = rowNext;

                        stemTriangles[st++] = next;
                        stemTriangles[st++] = rowNextNext;
                        stemTriangles[st++] = rowNext;
                    }
                    sv++;
                }
            }

            // --- 3. Объединение ---
            Vector3[] finalVertices = new Vector3[pumpkinVerticesCount + stemVerticesCount];
            Vector2[] finalUV = new Vector2[pumpkinVerticesCount + stemVerticesCount];

            System.Array.Copy(pumpkinVertices, 0, finalVertices, 0, pumpkinVerticesCount);
            System.Array.Copy(stemVertices, 0, finalVertices, pumpkinVerticesCount, stemVerticesCount);

            System.Array.Copy(pumpkinUV, 0, finalUV, 0, pumpkinVerticesCount);
            System.Array.Copy(stemUV, 0, finalUV, pumpkinVerticesCount, stemVerticesCount);

            for (int i = 0; i < stemTriangles.Length; i++)
            {
                stemTriangles[i] += pumpkinVerticesCount;
            }

            mesh.vertices = finalVertices;
            mesh.uv = finalUV;

            mesh.subMeshCount = 2;
            mesh.SetTriangles(pumpkinTriangles, 0);
            mesh.SetTriangles(stemTriangles, 1);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Безопасное удаление старого меша в редакторе, чтобы не забивать память
            if (meshFilter.sharedMesh != null && !Application.isPlaying)
            {
                DestroyImmediate(meshFilter.sharedMesh);
            }

            meshFilter.mesh = mesh;
        }
    }
}
