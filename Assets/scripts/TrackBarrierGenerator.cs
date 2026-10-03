using UnityEngine;
using System.Collections.Generic;

public class TrackBarrierGenerator : MonoBehaviour
{
    [Header("Track")]
    public Transform waypointParent;

    [Header("Road")]
    public float roadHalfWidth = 7f;
    public float barrierOffset = 1f;

    [Header("Barrier Size")]
    public float barrierLength = 3f;
    public float barrierWidth = 0.7f;
    public float barrierHeight = 1.5f;

    [Header("Barrier Appearance")]
    public Material barrierMaterial;

    [Header("Options")]
    public bool generateLeftBarrier = true;
    public bool generateRightBarrier = true;
    public bool closeLoop = true;

    private List<Transform> waypoints = new List<Transform>();
    private Transform barrierParent;

    public void GenerateBarriers()
    {
        if (waypointParent == null)
        {
            Debug.LogError("Waypoint Parent is missing!");
            return;
        }

        ClearBarriers();

        waypoints.Clear();

        foreach (Transform child in waypointParent)
        {
            waypoints.Add(child);
        }

        if (waypoints.Count < 2)
        {
            Debug.LogError("You need at least 2 waypoints.");
            return;
        }

        GameObject parentObject = new GameObject("Generated Barriers");
        barrierParent = parentObject.transform;

        if (generateLeftBarrier)
            GenerateSide(true);

        if (generateRightBarrier)
            GenerateSide(false);

        Debug.Log("Barriers generated!");
    }

    void GenerateSide(bool leftSide)
    {
        for (int i = 0; i < waypoints.Count; i++)
        {
            int nextIndex = i + 1;

            if (nextIndex >= waypoints.Count)
            {
                if (closeLoop)
                    nextIndex = 0;
                else
                    break;
            }

            Vector3 current = waypoints[i].position;
            Vector3 next = waypoints[nextIndex].position;

            Vector3 direction = next - current;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                continue;

            direction.Normalize();

            Vector3 right =
                Vector3.Cross(Vector3.up, direction).normalized;

            Vector3 side = leftSide ? -right : right;

            Vector3 start =
                current +
                side * (roadHalfWidth + barrierOffset);

            Vector3 end =
                next +
                side * (roadHalfWidth + barrierOffset);

            float distance =
                Vector3.Distance(start, end);

            int pieces =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        distance / barrierLength
                    )
                );

            for (int j = 0; j < pieces; j++)
            {
                float t =
                    (j + 0.5f) / pieces;

                Vector3 position =
                    Vector3.Lerp(start, end, t);

                Vector3 lookDirection =
                    end - start;

                lookDirection.y = 0f;

                Quaternion rotation =
                    Quaternion.LookRotation(
                        lookDirection.normalized,
                        Vector3.up
                    );

                CreateBarrier(
                    position,
                    rotation,
                    leftSide
                );
            }
        }
    }

    void CreateBarrier(
        Vector3 position,
        Quaternion rotation,
        bool leftSide)
    {
        GameObject barrier =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube
            );

        barrier.name =
            leftSide
            ? "Left Barrier"
            : "Right Barrier";

        barrier.transform.SetParent(
            barrierParent
        );

        barrier.transform.position =
            position +
            Vector3.up *
            (barrierHeight / 2f);

        barrier.transform.rotation =
            rotation;

        barrier.transform.localScale =
            new Vector3(
                barrierWidth,
                barrierHeight,
                barrierLength
            );

        if (barrierMaterial != null)
        {
            Renderer renderer =
                barrier.GetComponent<Renderer>();

            renderer.material =
                barrierMaterial;
        }
    }

    public void ClearBarriers()
    {
        GameObject existing =
            GameObject.Find(
                "Generated Barriers"
            );

        if (existing != null)
        {
            DestroyImmediate(existing);
        }

        barrierParent = null;
    }
}