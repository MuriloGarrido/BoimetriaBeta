namespace BoimetriaBeta.Models;

public sealed record DetectionResult(BoundingBox Box, float Confidence, int ClassId);
