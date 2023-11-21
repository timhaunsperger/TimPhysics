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
        var invNormal = Vector3d.Cross(p1 - p0, p2 - p0);
        invNormal.Normalize();
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
        return GetArea(p0, p1, p2)*PointPlaneDist(p0, p1, p2, point)/3f;
    }
    
    public static double PointPlaneDist(Vector3d p0, Vector3d p1, Vector3d p2, Vector3d point)
    {
        var normal = GetNormal(p0, p1, p2);
        return Vector3d.Dot(p0 - point, normal);
    }
    public static Vector3d PointPlaneVect(Vector3d p0, Vector3d p1, Vector3d p2, Vector3d point)
    {
        var normal = GetNormal(p0, p1, p2);
        return Vector3d.Dot(p0 - point, normal) * normal;
    }
    public static Vector3d LinearVelocity(Vector3d rotAxis, Vector3d offset, double angVelocity)
    {
        var rad  = Vector3d.Dot(offset, rotAxis) * rotAxis - offset;
        return Vector3d.Cross(offset, rotAxis).Normalized() * angVelocity * rad.Length;
    }
    
    public static double[] Flatten(double[][] inp)
    {
        var outp = new double[inp.Length * 8];
        for (int i = 0; i < inp.Length; i++)
        {
            Array.Copy(inp[i], 0, outp, i * 8, 8);
        }
        return outp;
    }
}