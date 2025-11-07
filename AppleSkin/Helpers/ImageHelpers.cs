//this is ai generated, im too dumb to make this
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Utils;

namespace AppleSkin.Helpers
{
    internal static class ImageHelpers
    {
        public static RawImageData CreateOutline(RawImageData imageData, bool touchesEdge, int thickness = 1)
        {
            int width = imageData.Width;
            int height = imageData.Height;

            RawImageData outline = new(width, height);
            ColorF outlineColor = ColorF.White;

            bool[,] isEdge = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (imageData.GetPixel(x, y).A == 0)
                        continue;

                    bool edge = false;
                    for (int dy = -1; dy <= 1 && !edge; dy++)
                    {
                        for (int dx = -1; dx <= 1 && !edge; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = x + dx, ny = y + dy;

                            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            {
                                if (touchesEdge) edge = true;
                                continue;
                            }
                            if (imageData.GetPixel(nx, ny).A == 0)
                                edge = true;
                        }
                    }

                    if (edge)
                        isEdge[x, y] = true;
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!isEdge[x, y])
                        continue;

                    for (int t = 1; t <= thickness; t++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (Math.Abs(dx) + Math.Abs(dy) != 1)
                                    continue;

                                int nx = touchesEdge ? x - dx * t : x + dx * (t + 0);
                                int ny = touchesEdge ? y - dy * t : y + dy * (t + 0);

                                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                                {
                                    if (imageData.GetPixel(nx, ny).A == 0)
                                        outline.SetPixel(nx, ny, outlineColor);
                                }
                            }
                        }
                    }
                }
            }

            return outline;
        }

        public static RawImageData CropUV(RawImageData source, Rect uvRect)
        {
            int x = (int)(uvRect.X * source.Width);
            int y = (int)(uvRect.Y * source.Height);
            int width = (int)(uvRect.Width * source.Width);
            int height = (int)(uvRect.Height * source.Height);

            x = Math.Clamp(x, 0, source.Width - 1);
            y = Math.Clamp(y, 0, source.Height - 1);
            width = Math.Clamp(width, 1, source.Width - x);
            height = Math.Clamp(height, 1, source.Height - y);

            RawImageData cropped = new(width, height);

            for (int yy = 0; yy < height; yy++)
            {
                for (int xx = 0; xx < width; xx++)
                {
                    uint pixel = source.GetRawPixel(x + xx, y + yy);
                    cropped.SetRawPixel(xx, yy, pixel);
                }
            }

            return cropped;
        }

        internal static RawImageData Walter(RawImageData source, RawImageData? mask = null) // i made this tho if its not obvious (its very obvious)
        {
            bool covid = mask != null;
            int width = source.Width;
            int height = source.Height;
            RawImageData colored = new(width, height);
            if (covid && (mask!.Width != width || mask.Height != height))
                mask = mask.Resized(width, height);
            for (int y = 0; y < height; y++) // this is python real
                for (int x = 0; x < width; x++)
                    if (source.GetPixel(x, y).A != 0 && !(covid && mask!.GetPixel(x, y).A != 0))
                        colored.SetPixel(x, y, ColorF.White);

            return colored;
        }
    }
}