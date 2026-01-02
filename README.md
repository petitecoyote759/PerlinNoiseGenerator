# ShortTools.Perlin

To use this library, the 2 main functions are GetValue and GeneratePerlinMap.

GeneratePerlinMap automatically creates a vector field and calls GetValue in order to populate a 2d float array.

GetValue returns a perlin noise at a given position assuming the vectors in the vector field have x and y values that are between -1 and 1, but
they are not normalised.

Some example code:

	float[,] firstMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 16);
    float[,] secondMap = Perlin.GeneratePerlinMap(renderer.screenwidth / scale, renderer.screenheight / scale, 8, 0.25f);

    PerlinMap = Perlin.CombineFloatMaps(firstMap, secondMap, 1.25f);

This code creates 2 perlin maps and combines them, and by using the CombineFloatMaps function it will keep the total value at 1, since the first map 
was scaled to 1 and the second was scaled to 0.25f, the total max would have been 1.25, which is why that was placed in as the scale.