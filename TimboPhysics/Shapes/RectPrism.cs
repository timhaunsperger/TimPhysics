﻿using OpenTK.Mathematics;

namespace TimboPhysics;

public class RectPrism : Shape
{
    public RectPrism(Vector3d offset, double width, double height, double depth, Quaterniond rotation)
    {
        Center = offset;
        _baseIndices = new uint[]{  // Faces of basic cube
            2, 1, 0,
            0, 3, 2,
            0, 1, 5,
            5, 4, 0,
            6, 2, 3,
            3, 7, 6,
            4, 5, 6,
            6, 7, 4,
            3, 0, 4,
            4, 7, 3,
            5, 1, 2,
            2, 6, 5
        };
        _baseVertices = new []{  // Positions of vertices for basic cube
            new [] { 1, 1,-1, 0, 1, 0, 1.0, 1.0 }, //top right back     0
            new [] { 1,-1,-1, 0, 0, 0, 0.0, 0.0 }, //bottom right back  1
            new [] {-1,-1,-1, 0, 0, 0, 1.0, 0.0 }, //bottom left back   2
            new [] {-1, 1,-1, 0, 1, 0, 0.0, 1.0 }, //top left back      3
        
            new [] { 1, 1, 1, 0, 1, 0, 1.0, 0.0 }, //top right front    4
            new [] { 1,-1, 1, 0, 0, 0, 0.0, 1.0 }, //bottom right front 5
            new [] {-1,-1, 1, 0, 0, 0, 1.0, 1.0 }, //bottom left front  6
            new [] {-1, 1, 1, 0, 1, 0, 0.0, 0.0 }, //top left front     7
        };
        
        var outVertices = new Vector3d[8];
        
        // Scales position of cube vertex to size and rotation of rectangular prism
        for (int i = 0; i < _baseVertices.Length; i++) 
        {
            var scaledPos =  rotation * new Quaterniond(
                _baseVertices[i][0] * width, _baseVertices[i][1] * height, _baseVertices[i][2] * depth, 0) * Quaterniond.Conjugate(rotation);
            
            outVertices[i] = new Vector3d(scaledPos.Xyz+offset);
        }
        
        Indices = _baseIndices;
        Vertices = FlattenVertices(outVertices);
    }
}