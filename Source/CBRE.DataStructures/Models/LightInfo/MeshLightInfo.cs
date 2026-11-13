using System.Collections.Generic;

namespace CBRE.DataStructures.Models.LightInfo;

public record MeshLightInfo(IReadOnlyList<VertexLightInfo> Vertices, IReadOnlyList<int> Indices);
