using UnityEngine;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

public class CellSet
{
    public HashSet<TerrainCell> Cells;

    public TerrainCell Top;
    public TerrainCell Bottom;
    public TerrainCell Left;
    public TerrainCell Right;

    public int RectArea = 1;
    public int RectWidth = 1;
    public int RectHeight = 1;

    public void AddCell(TerrainCell cell)
    {
        Cells.Add(cell);

        if (((cell.Longitude - Left.Longitude) == -1) ||
            ((cell.Longitude - Left.Longitude - Manager.WorldWidth) == -1))
        {
            Left = cell;
        }

        if (((cell.Longitude - Right.Longitude) == 1) ||
            ((cell.Longitude - Right.Longitude + Manager.WorldWidth) == 1))
        {
            Right = cell;
        }

        if ((cell.Latitude - Top.Latitude) == -1)
        {
            Top = cell;
        }

        if ((cell.Latitude - Bottom.Latitude) == 1)
        {
            Bottom = cell;
        }
    }

    public void CalcRectangle()
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (right < left) right += Manager.WorldWidth;

        RectHeight = bottom - top + 1;
        RectWidth = right - left + 1;

        RectArea = RectWidth * RectHeight;
    }

    public bool IsCellEnclosed(TerrainCell cell)
    {
        int top = Top.Latitude;
        int bottom = Bottom.Latitude;
        int left = Left.Longitude;
        int right = Right.Longitude;

        // adjust for world wrap
        if (right < left) right += Manager.WorldWidth;

        if (!cell.Latitude.IsInsideRange(top, bottom)) return false;

        int longitude = cell.Longitude;

        if (longitude.IsInsideRange(left, right)) return true;

        longitude += Manager.WorldWidth;

        if (longitude.IsInsideRange(left, right)) return true;

        return false;
    }

    public void Merge(CellSet sourceSet)
    {
        Cells.UnionWith(sourceSet.Cells);

        if (Top.Latitude > sourceSet.Top.Latitude)
        {
            Top = sourceSet.Top;
        }

        if (Bottom.Latitude < sourceSet.Bottom.Latitude)
        {
            Bottom = sourceSet.Bottom;
        }

        bool offsetNeeded = false;
        bool rightOffsetDone = false;
        bool sourceSetRightOffsetDone = false;

        int rigthLongitude = Right.Longitude;
        if (Left.Longitude >= Right.Longitude)
        {
            rigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            rightOffsetDone = true;
        }

        int sourceSetRigthLongitude = sourceSet.Right.Longitude;
        if (sourceSet.Left.Longitude >= sourceSet.Right.Longitude)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            offsetNeeded = true;
            sourceSetRightOffsetDone = true;
        }

        int leftLongitude = Left.Longitude;
        if (offsetNeeded && !rightOffsetDone)
        {
            rigthLongitude += Manager.WorldWidth;
            leftLongitude += Manager.WorldWidth;
        }

        int sourceSetLeftLongitude = sourceSet.Left.Longitude;
        if (offsetNeeded && !sourceSetRightOffsetDone)
        {
            sourceSetRigthLongitude += Manager.WorldWidth;
            sourceSetLeftLongitude += Manager.WorldWidth;
        }

        if (leftLongitude > sourceSetLeftLongitude)
        {
            Left = sourceSet.Left;
        }

        if (rigthLongitude < sourceSetRigthLongitude)
        {
            Right = sourceSet.Right;
        }
    }
}
