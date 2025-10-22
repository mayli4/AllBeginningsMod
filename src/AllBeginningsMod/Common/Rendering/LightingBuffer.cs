using AllBeginningsMod.Utilities;

namespace AllBeginningsMod.Common.Rendering;

//todo murder this bruh

/// <summary>
///     Draws arbitrary meshes that are affected by ingame lighting.
/// </summary>
internal static class LightingBuffer {
    private static short[] _indices;
    private static VertexPositionColorTexture[] _vertices;
    private static int _lastMeshWidth;
    private static int _lastMeshHeight;
    private static int _vertexCount;
    private static int _primitiveCount;

    public static LightMesh Prepare(Rectangle tileArea) {
        int meshWidth = tileArea.Width + 1;
        int meshHeight = tileArea.Height + 1;

        _vertexCount = meshWidth * meshHeight;
        int indexCount = tileArea.Width * tileArea.Height * 6;
        _primitiveCount = indexCount / 3;

        if(_vertices == null || _vertices.Length < _vertexCount)
            _vertices = new VertexPositionColorTexture[_vertexCount];

        bool sizeChanged = meshWidth != _lastMeshWidth || meshHeight != _lastMeshHeight;
        if(sizeChanged && (_indices == null || _indices.Length < indexCount))
            _indices = new short[indexCount];

        PopulateVertices(tileArea, meshWidth);

        if(sizeChanged) {
            PopulateIndices(tileArea.Width, tileArea.Height, meshWidth);
            _lastMeshWidth = meshWidth;
            _lastMeshHeight = meshHeight;
        }

        return new LightMesh();
    }

    private static void PopulateVertices(Rectangle tileArea, int meshWidth) {
        int horizontalSamples = tileArea.Width / 2 + 1;
        int verticalSamples = tileArea.Height / 2 + 1;

        for(int j = 0; j < verticalSamples; j++) {
            for(int i = 0; i < horizontalSamples; i++) {
                Lighting.GetCornerColors(tileArea.X + i * 2, tileArea.Y + j * 2, out var colors);

                bool isRightEdge = i * 2 == tileArea.Width;
                bool isBottomEdge = j * 2 == tileArea.Height;

                int x = i * 2;
                int y = j * 2;

                float u = x / (float)tileArea.Width;
                float v = y / (float)tileArea.Height;

                _vertices[y * meshWidth + x] = new VertexPositionColorTexture(
                    new Vector3((tileArea.X + x) * 16f, (tileArea.Y + y) * 16f, 0f),
                    colors.TopLeftColor,
                    new Vector2(u, v)
                );

                if(!isRightEdge) {
                    _vertices[y * meshWidth + x + 1] = new VertexPositionColorTexture(
                        new Vector3((tileArea.X + x + 1) * 16f, (tileArea.Y + y) * 16f, 0f),
                        colors.TopRightColor,
                        new Vector2((x + 1) / (float)tileArea.Width, v)
                    );
                }

                if(!isBottomEdge) {
                    _vertices[(y + 1) * meshWidth + x] = new VertexPositionColorTexture(
                        new Vector3((tileArea.X + x) * 16f, (tileArea.Y + y + 1) * 16f, 0f),
                        colors.BottomLeftColor,
                        new Vector2(u, (y + 1) / (float)tileArea.Height)
                    );
                }

                if(!isRightEdge && !isBottomEdge) {
                    _vertices[(y + 1) * meshWidth + x + 1] = new VertexPositionColorTexture(
                        new Vector3((tileArea.X + x + 1) * 16f, (tileArea.Y + y + 1) * 16f, 0f),
                        colors.BottomRightColor,
                        new Vector2((x + 1) / (float)tileArea.Width, (y + 1) / (float)tileArea.Height)
                    );
                }
            }
        }
    }

    private static void PopulateIndices(int width, int height, int meshWidth) {
        int counter = 0;
        for(int j = 0; j < height; j++) {
            for(int i = 0; i < width; i++) {
                short v1 = (short)(i + j * meshWidth);
                short v2 = (short)(i + 1 + j * meshWidth);
                short v3 = (short)(i + (j + 1) * meshWidth);
                short v4 = (short)(i + 1 + (j + 1) * meshWidth);

                _indices[counter++] = v1;
                _indices[counter++] = v2;
                _indices[counter++] = v3;
                _indices[counter++] = v2;
                _indices[counter++] = v4;
                _indices[counter++] = v3;
            }
        }
    }

    internal struct LightMesh {
        private readonly Effect _effect;
        private readonly Matrix _matrix;
        private Color _color;

        public LightMesh() {
            _effect = null;
            _matrix = Matrix.Identity;
            _color = Color.White;
        }

        private LightMesh(Effect effect, Matrix matrix, Color color) {
            _effect = effect;
            _matrix = matrix;
            _color = color;
        }

        public LightMesh WithEffect(Effect effect) {
            return new LightMesh(effect, _matrix, _color);
        }

        public LightMesh WithColor(Color color) {
            return new LightMesh(_effect, _matrix, color);
        }

        public void Draw(Texture2D texture) {
            if(_vertices == null || _indices == null)
                return;

            var gd = Main.instance.GraphicsDevice;

            var shader = Shaders.Fragment.LightMesh.Value;

            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            Matrix view = Main.GameViewMatrix.TransformationMatrix;
            Matrix renderMatrix = Matrix.CreateTranslation(-Main.screenPosition.Vec3()) * view * projection;

            shader.Parameters["uWorldViewProjection"]?.SetValue(renderMatrix);
            shader.Parameters["layer1Texture"]?.SetValue(texture);
            shader.Parameters["tintColor"]?.SetValue(_color.ToVector4());

            gd.BlendState = BlendState.AlphaBlend;
            gd.SamplerStates[0] = SamplerState.PointClamp;

            foreach(EffectPass pass in shader.CurrentTechnique.Passes) {
                pass.Apply();
                gd.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertices,
                    0,
                    _vertexCount,
                    _indices,
                    0,
                    _primitiveCount
                );
            }
        }
    }
}