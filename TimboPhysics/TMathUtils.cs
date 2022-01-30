using OpenTK.Mathematics;

namespace TimboPhysics;

public static class TMathUtils
{
    public static Vector3d GetCenter(Vector3d p0, Vector3d p1, Vector3d p2)
    {
        return (p0 + p1 + p2) / 3;
    }
    
    public static Vector3d GetNormal(Vector3d p0, Vector3d p1, Vector3d p2)
    {
        var invNormal = Vector3d.Cross(p1 - p0, p2 - p0).Normalized();
        return invNormal * -1;
    }
    
    public static double GetArea(Vector3d p0, Vector3d p1, Vector3d p2)
    {
        double d0 = Vector3d.Distance(p0, p1);
        double d1 = Vector3d.Distance(p1, p2);
        double d2 = Vector3d.Distance(p2, p0);
        double s = (d0 + d1 + d2) / 2;
        return Math.Sqrt(s * (s - d0) * (s - d1) * (s - d2));
    }

    public static double GetVolume(Vector3d p0, Vector3d p1, Vector3d p2, Vector3d point)
    {
        var normal = GetNormal(p0, p1, p2) * -1;
        var center = GetCenter(p0, p1, p2);
        var distance = point - center;
        var height = Vector3d.Dot(normal, distance);
            
        return GetArea(p0, p1, p2)*height/3f;
    }
    
    public static bool IsPointBehindPlane(Vector3d p0, Vector3d p1, Vector3d p2, Vector3d point)
    {
        var center = GetCenter(p0, p1, p2);
        var normal = GetNormal(p0, p1, p2);
        return Vector3d.Dot((center - point).Normalized(), normal.Normalized()) > 0;
    }
}