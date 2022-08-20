using UnityEngine;

public interface IMapEntitySelectionRequest : IEntitySelectionRequest
{
    RectInt GetEncompassingRectangle();
}
