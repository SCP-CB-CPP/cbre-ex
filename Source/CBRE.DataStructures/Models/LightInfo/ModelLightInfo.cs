using System.Collections.Generic;
using CBRE.DataStructures.Geometric;

namespace CBRE.DataStructures.Models.LightInfo;

public record ModelLightInfo(Matrix UsedWorldTransform, IReadOnlyList<MeshLightInfo> MeshLightInfos);
