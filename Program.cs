using SimpleGraphicsLib;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;


namespace ShortTools.Perlin
{
    public static class Perlin
    {
        public static float[,] GeneratePerlinMap(int width, int height, int gridSize, float scale = 1f, Random? random = null)
        {
            float[,] map = new float[width, height];

            Vector2[,] vectorField = GenVectorField((width / gridSize) + 2, (height / gridSize) + 2, random);

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


        private static Vector2[,] GenVectorField(int width, int height, Random? random)
        {
            Vector2[,] field = new Vector2[width, height];
            random ??= new Random();

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
        static float[,]? perlinMap = null;
        static float[,]? treeMap = null;
        static List<ResourceMap> resourceMaps = new List<ResourceMap>();

        internal struct ResourceMap
        {
            public string name;
            public float[,] map;
            public int minDist;
            public float minValue;
            public Tuple<byte, byte, byte> colour;
            public ResourceMap(string name, float[,] map, int minDist, Tuple<byte, byte, byte> colour, float minValue = 0.1f)
            {
                this.name = name; this.map = map; this.minDist = minDist; this.minValue = minValue; this.colour = colour;
            }
        }


        const int scale = 2;
        static Tuple<int, int> centre = new Tuple<int, int>(0,0);
        const bool askForSeed = true;

        private static void Main()
        {
            int seed;
            while (askForSeed)
            {
                Console.WriteLine($"Autogen seed? [Y/N]");
                string option = (Console.ReadLine() ?? "").ToUpperInvariant();
                if (option == "Y")
                {
                    seed = new Random().Next(int.MinValue, int.MaxValue);
                    break;
                }
                else if (option == "N")
                {
                    Console.Write("Seed: ");
                    if (int.TryParse(Console.ReadLine() ?? "", out int result))
                    {
                        seed = result;
                        Console.Write('\n');
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"That is not a number!\n\n");
                    }
                }
                else
                {
                    Console.WriteLine($"Not a valid input! Please input either Y or N.\n\n");
                }
            }

            Random random = new Random(seed);
            Console.WriteLine($"Seed: {seed}");





            using (renderer = new GraphicsHandler(
                render: Render,
                flags: [RendererFlag.OutputToTerminal, RendererFlag.HalfSizedWindow]))
            {
                Console.WriteLine("Starting Perlin Demo");
                Console.WriteLine($"Dimensions = {renderer.screenwidth}x{renderer.screenheight}");

                renderer.Pause();

                centre = new Tuple<int, int>(renderer.screenwidth / 2, renderer.screenheight / 2);
                treeMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 8, random: random);
                resourceMaps.Add(
                    new ResourceMap("Glucryte", Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 4, random: random), 
                    0, new Tuple<byte, byte, byte>(0, 255, 100), 0.4f));
                resourceMaps.Add(
                    new ResourceMap("Fructate", Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 6, random: random), 
                    100, new Tuple<byte, byte, byte>(100, 255, 50), 0.2f));
                resourceMaps.Add(
                    new ResourceMap("Galactyte", Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 4, random: random), 
                    200, new Tuple<byte, byte, byte>(150, 100, 200), 0.5f));

                tempCreateMap(renderer.screenwidth, renderer.screenheight, random);

                renderer.Resume();
                
                Console.WriteLine("Perlin Map Loaded");

                Thread.Sleep(10000);

                Console.WriteLine("Terminating");
            }
        }

        static void tempCreateMap(int width, int height, Random random)
        {
            float[,] continentMap = Perlin.GeneratePerlinMap(width / scale, height / scale, 64, 4f, random: random);
            float[,] firstMap = Perlin.GeneratePerlinMap(width / scale, height / scale, 16, random: random);
            float[,] secondMap = Perlin.GeneratePerlinMap(width / scale, height / scale, 8, 0.25f, random: random);

            perlinMap = Perlin.CombineFloatMaps(firstMap, secondMap, 1.25f);
            perlinMap = Perlin.CombineFloatMaps(continentMap, perlinMap, 5f);

            perlinMap = ApplyFuncToMap(perlinMap);

            perlinMap = CreateIsland(perlinMap);
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




        const bool coloured = true;
        private static void Render()
        {
            if (perlinMap is null) { return; }

            // perlin map goes from -1 to 1, so to turn to black and white we can do
            // (x + 1) * 255 / 2
            for (int x = 0; x < perlinMap.GetLength(0); x++)
            {
                for (int y = 0; y < perlinMap.GetLength(1); y++)
                {
                    byte r;
                    byte g;
                    byte b;

                    if (coloured)
                    {
                        Tuple<byte, byte, byte> colours = GetColours(x, y, perlinMap);
                        r = colours.Item1; g = colours.Item2; b = colours.Item3;
                    }
                    else
                    {
                        float value = float.Clamp((perlinMap[x, y] + 1) * 255 / 2, 0, 255);
                        byte colour = (byte)(value);
                        r = g = b = colour;
                    }

                    
                    renderer.SetPixel(x * scale, y * scale, scale, scale, r, g, b);
                }
            }
        }



        private static Tuple<byte, byte, byte> GetColours(int x, int y, float[,] PerlinMap)
        {
            float value = PerlinMap[x, y];

            if (
                (value > 0 && GetTileGradient(x, y, PerlinMap) > 0.27f) ||
                (value > 0.4f && GetTileGradient(x, y, PerlinMap) > 0.15f))
            {
                return new Tuple<byte, byte, byte>(
                    (byte)(100),
                    (byte)(100),
                    (byte)(100)
                    );
            }

            if (value < 0f) // from -1 to 0 so just add one ig
            {
                return new Tuple<byte, byte, byte>(
                    (byte)(10 + (20 * (value + 1))),
                    (byte)(60 + (40 * (value + 1))),
                    (byte)(50 + (200 * (value + 1)))
                    );
            }
            else if (value < 0.1f)
            {
                return new Tuple<byte, byte, byte>(200, 200, 20);
            }
            else
            { // 0.1 to 1, thats 0.9 which is basically 0 - 1 : grass
                if (treeMap?[x, y] > 0.1f)
                {
                    return new Tuple<byte, byte, byte>(
                        (byte)(10 + value * 10),
                        (byte)(100 + value * 50),
                        (byte)(10 + value * 20)
                        );
                }
                else
                {
                    return CheckForResources(x, y, value);
                    
                }
            }
        }


        private static Tuple<byte, byte, byte> CheckForResources(int x, int y, float value)
        {
            foreach (ResourceMap resourceMap in resourceMaps)
            {
                float distance = MathF.Abs(x - (centre.Item1 / 2)) + MathF.Abs(y - (centre.Item2 / 2));
                if (distance < resourceMap.minDist) { continue; }

                if (resourceMap.map[x, y] > resourceMap.minValue) { return resourceMap.colour; }
            }




            return new Tuple<byte, byte, byte>(
                        (byte)(10 + value * 10),
                        (byte)(150 + value * 50),
                        (byte)(10 + value * 20)
                        );
        }







        private static float GetTileGradient(int x, int y, float[,] PerlinMap)
        {
            if (x == 0 || y == 0 || x == PerlinMap.GetLength(0) - 1 || y == PerlinMap.GetLength(1) - 1) { return 0f; }

            float mainTile = PerlinMap[x, y];
            
            float dn = MathF.Abs(PerlinMap[x    , y - 1] - mainTile);
            float de = MathF.Abs(PerlinMap[x + 1, y    ] - mainTile);
            float ds = MathF.Abs(PerlinMap[x    , y + 1] - mainTile);
            float dw = MathF.Abs(PerlinMap[x - 1, y    ] - mainTile);

            return (dn + de + ds + dw);
        }




        const float islandWidth = 40f;
        private static float[,] CreateIsland(float[,] map)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            float[,] outMap = new float[width, map.GetLength(1)];   
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float altitude = GetIslandHeight(width / 2, height / 2, x, y, islandWidth);
                    if (altitude > 0)
                    {
                        outMap[x, y] = float.Clamp(map[x, y] + altitude, -1, 1);
                    }
                    else
                    {
                        outMap[x, y] = map[x, y];
                    }
                }
            }

            return outMap;
        }

        private static float GetIslandHeight(int cx, int cy, int x, int y, float islandWidth = 20f)
        {
            return MathF.Tanh(4 * (MathF.Exp(-((MathF.Pow(x - cx, 2)) + (MathF.Pow(y - cy, 2))) / (islandWidth * islandWidth)) - 0.5f));
        }
    }
}