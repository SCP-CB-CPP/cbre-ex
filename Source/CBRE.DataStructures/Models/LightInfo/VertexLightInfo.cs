using CBRE.DataStructures.Geometric;
using OpenTK;

namespace CBRE.DataStructures.Models.LightInfo;

public record struct VertexLightInfo(MeshVertex Vertex, Coordinate Position, Vector3 Light);
