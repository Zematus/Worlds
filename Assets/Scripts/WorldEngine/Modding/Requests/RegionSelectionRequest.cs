using System.Collections.Generic;

public class RegionSelectionRequest : EntitySelectionRequest<Region>
{
    public RegionSelectionRequest(ICollection<Region> collection) : base(collection)
    {
    }
}
