using UnityEngine;

public interface IMapEntitySelectionRequest
{
    ModText Text { get; }

    RectInt GetEncompassingRectangle();
}
