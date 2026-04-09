# GreenPng

Fastest PNG decoder for .NET designed to be blazingly fast and memory efficient.

It is focused on decoding 8-bit-per-channel images to little-endian `BGRA` format (like `VK_FORMAT_B8G8R8A8_SRGB` or `SDL_PIXELFORMAT_ARGB8888`).

# Comparsion

| Project | Speed | Memory efficiency | Correctness | Free license |
|-|-|-|-|-|
| **GreenPng** | :zap: | :green_circle: | :construction: WIP | :white_check_mark: |
| StbImageSharp | :crescent_moon: | :yellow_circle: | :white_check_mark: | :white_check_mark: |
| Magick.NET | :snowflake: | :yellow_circle: | :x: | :white_check_mark: |
| SixLabors.ImageSharp | :fire: | :yellow_circle: | :white_check_mark: | :x: |

# Format support

| Type | 1 bit | 2, 4 bits | 8 bit | 16 bit |
|-|-|-|-|-|
| Greyscale | :white_check_mark: | :construction: | :white_check_mark: | :x: |
| Truecolor ||| :white_check_mark: | :x: |
| Indexed | :white_check_mark: | :construction: | :white_check_mark: | :x: |
| Indexed (with transparency) | :white_check_mark: | :construction: | :white_check_mark: | :x: |
| Greyscale with alpha ||| :white_check_mark: | :x: |
| Truecolor with alpha ||| :white_check_mark: | :x: |

# How to use

### Easy use

```cs
byte[] image = PngDecoder.Decode(pngFileData, out PngHeader header);

int width = header.Width;
int height = header.Height;
```

### More advanced way

```cs
bool success = PngDecoder.TryDecodeHeader(pngFileData, out PngHeader header);

byte[] image = new byte[header.ByteSize];

success = PngDecoder.TryDecode(pngFileData, header, image);
```
