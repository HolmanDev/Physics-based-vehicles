using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class ColliderExtensions
{
    public static BoxCollider CopyFrom(this BoxCollider newBoxCollider, BoxCollider boxCollider)
    {
        boxCollider.DeepCopyTo(newBoxCollider);
        return newBoxCollider;
    }

    public static BoxCollider CopyFrom(this BoxCollider newBoxCollider, MeshCollider meshCollider)
    {
        Vector3[] vertices = meshCollider.sharedMesh.vertices;
        float maxX = vertices.Max(vertex => vertex.x);
        float minX = vertices.Min(vertex => vertex.x);
        float maxY = vertices.Max(vertex => vertex.y);
        float minY = vertices.Min(vertex => vertex.y);
        float maxZ = vertices.Max(vertex => vertex.z);
        float minZ = vertices.Min(vertex => vertex.z);
        Vector3 partLocalSize = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);
        Vector3 partLocalCenter = new Vector3(maxX + minX, maxY + minY, maxZ + minZ) * 0.5f;
        newBoxCollider.size = partLocalSize;
        newBoxCollider.center = partLocalCenter;
        return newBoxCollider;
    }

    public static BoxCollider CopyFrom(this BoxCollider newBoxCollider, CapsuleCollider capsuleCollider)
    {
        Vector3 dir = Vector3Extensions.Direction(capsuleCollider.direction);
        Vector3 partColliderLocalSize = (Vector3.one - dir) * capsuleCollider.radius * 2f + dir * capsuleCollider.height;
        newBoxCollider.size = partColliderLocalSize;
        newBoxCollider.center = capsuleCollider.center;
        return newBoxCollider;
    }

    public static BoxCollider DeepCopyTo(this BoxCollider original, BoxCollider copy)
    {
        copy.size = original.size;
        copy.center = original.center;
        copy.isTrigger = original.isTrigger;
        return copy;
    }
}
