using SimpleGraphicsLib;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;


namespace ShortTools.Perlin
{
    public static class Perlin
    {
        public static float[,] GeneratePerlinMap(int width, int height, int gridSize, float scale = 1f)
        {
            float[,] map = new float[width, height];

            Vector2[,] vectorField = GenVectorField((width / gridSize) + 2, (height / gridSize) + 2);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map[x, y] = GetValue(x, y, gridSize, vectorField, scale);
                }
            }

            return map;
        }




        [MethodImpl(MethodImplOptions.AggressiveOptimization|MethodImplOptions.AggressiveInlining)]
        public static float GetValue(int x, int y, int gridSize, Vector2[,] vectors, float scale = 1f)
        {

            int Squarex = x / gridSize;
            int Squarey = y / gridSize;

            float northwest = Vector2.Dot(vectors[x / gridSize, y / gridSize],
                new Vector2((Squarex * gridSize) - x, Squarey * gridSize - y));

            float northeast = Vector2.Dot(vectors[(x) / gridSize + 1, y / gridSize],
                new Vector2(((Squarex + 1) * gridSize) - x, Squarey * gridSize - y));

            float southwest = Vector2.Dot(vectors[x / gridSize, (y) / gridSize + 1],
                new Vector2((Squarex * gridSize) - x, ((Squarey + 1) * gridSize) - y));

            float southeast = Vector2.Dot(vectors[(x) / gridSize + 1, (y) / gridSize + 1],
                new Vector2(((Squarex + 1) * gridSize) - x, ((Squarey + 1) * gridSize) - y));

            float North = Lerp(northwest, northeast, ((x - Squarex * gridSize) / (float)gridSize));
            float South = Lerp(southwest, southeast, ((x - Squarex * gridSize) / (float)gridSize));

            float value = Lerp(North, South, ((y - Squarey * gridSize) / (float)gridSize)); // will be between - gridsize / 2 and gridsize / 2
            // to be between -1 and 1, it should do this
            return float.Clamp((value * 1.414f / gridSize) * scale, -1, 1);
        }



        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private static float Lerp(float a0, float a1, float w) // perc  is distance from A -> B
        {
            return (a1 - a0) * (3.0f - w * 2.0f) * w * w + a0;
        }


        private static Vector2[,] GenVectorField(int width, int height)
        {
            Vector2[,] field = new Vector2[width, height];
            Random random = new Random();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float angle = (float)(random.NextDouble() * Math.PI * 2);
                    field[x, y] = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                }
            }

            return field;
        }



        public static float[,] CombineFloatMaps(float[,] a, float[,] b, float totalValue = 1f)
        {
            if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1)) 
            { throw new InvalidOperationException($"The given inputs were not the same dimensions. A: {a.GetLength(0)}x{a.GetLength(1)}, B: {b.GetLength(0)}x{b.GetLength(1)}"); }

            int width = a.GetLength(0);
            int height = a.GetLength(1);

            float[,] output = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    output[x, y] = (a[x, y] + b[x, y]) / totalValue;
                }
            }

            return output;
        }
    }







    internal static class PerlinTester
    {
        static GraphicsHandler renderer;
        static float[,]? PerlinMap = null;
        const int scale = 2;


        private static void Main()
        {
            using (renderer = new GraphicsHandler(
                render: Render,
                flags: [RendererFlag.OutputToTerminal, RendererFlag.HalfSizedWindow]))
            {
                Console.WriteLine("Starting Perlin Demo");
                Console.WriteLine($"Dimensions = {renderer.screenwidth}x{renderer.screenheight}");

                renderer.Pause();

                float[,] continentMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 64, 4f);
                float[,] firstMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 16);
                float[,] secondMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 8, 0.25f);

                PerlinMap = Perlin.CombineFloatMaps(firstMap, secondMap, 1.25f);
                PerlinMap = Perlin.CombineFloatMaps(continentMap, PerlinMap, 5f);

                PerlinMap = ApplyFuncToMap(PerlinMap);

                renderer.Resume();
                
                Console.WriteLine("Perlin Map Loaded");

                Thread.Sleep(10000);

                Console.WriteLine("Terminating");
            }
        }



        private static float[,] ApplyFuncToMap(float[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            float[,] outMap = new float[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    outMap[x, y] = MathF.Tanh(4 * map[x, y]);
                }
            }

            return outMap;
        }





        private static void Render()
        {
            

            if (PerlinMap is null) { return; }

            // perlin map goes from -1 to 1, so to turn to black and white we can do
            // (x + 1) * 255 / 2
            for (int x = 0; x < PerlinMap.GetLength(0); x++)
            {
                for (int y = 0; y < PerlinMap.GetLength(1); y++)
                {
                    byte colour = (byte)((PerlinMap[x, y] + 1) * 255 / 2);

                    renderer.SetPixel(x * scale, y * scale, scale, scale, colour, colour, colour);
                }
            }
        }
    }
}