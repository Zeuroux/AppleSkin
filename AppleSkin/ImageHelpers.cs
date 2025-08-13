using OnixRuntime.Api.Maths;
using OnixRuntime.Api.Utils;

namespace AppleSkin
{
    internal static class ImageHelpers
    {

        public static RawImageData CreateOutline(RawImageData imageData, bool touchesEdge)
        {
            int width = imageData.Width;
            int height = imageData.Height;

            RawImageData outline = new(width, height);
            ColorF red = ColorF.White;

            bool[] edges = new bool[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ColorF pixel = imageData.GetPixel(x, y);
                    if (pixel.A != 0)
                    {
                        if (x == 0) edges[y * width] = true;
                        if (y == 0) edges[x] = true;
                        if (x == width - 1) edges[y * width + (width - 1)] = true;
                        if (y == height - 1) edges[(height - 1) * width + x] = true;
                        continue;
                    }
                    if (x > 0 && imageData.GetPixel(x - 1, y).A != 0)
                        edges[y * width + x - 1] = true;
                    if (x < width - 1 && imageData.GetPixel(x + 1, y).A != 0)
                        edges[y * width + x + 1] = true;
                    if (y > 0 && imageData.GetPixel(x, y - 1).A != 0)
                        edges[(y - 1) * width + x] = true;
                    if (y < height - 1 && imageData.GetPixel(x, y + 1).A != 0)
                        edges[(y + 1) * width + x] = true;
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!edges[y * width + x])
                        continue;

                    if (touchesEdge)
                    {
                        outline.SetPixel(x, y, red);
                    }
                    else
                    {
                        int neighborCount = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0)
                                    continue;

                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                                {
                                    if (imageData.GetPixel(nx, ny).A != 0)
                                        neighborCount++;
                                }
                            }
                        }

                        if (neighborCount == 1)
                        {
                            int tx = x + 1, ty = y - 1;
                            int bx = x - 1, by = y + 1;

                            if (tx >= 0 && ty >= 0 && tx < outline.Width && ty < outline.Height)
                                outline.SetPixel(tx, ty, red);

                            if (bx >= 0 && by >= 0 && bx < outline.Width && by < outline.Height)
                                outline.SetPixel(bx, by, red);
                        }

                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (Math.Abs(dx) == Math.Abs(dy))
                                    continue;

                                int nx = x + dx;
                                int ny = y + dy;

                                if (nx >= 0 && ny >= 0 && nx < outline.Width && ny < outline.Height)
                                {
                                    if (imageData.GetPixel(nx, ny).A == 0)
                                        outline.SetPixel(nx, ny, red);
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
    }
}