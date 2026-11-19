# Third Party Notices

Vixens Toolbox includes the third party software below. Each part keeps its own licence. The toolbox's own code is under the MIT licence in `LICENSE.md`.

## Magick.NET 14.17.2

* **By:** Dirk Lemstra, https://github.com/dlemstra/Magick.NET
* **Licence:** Apache License 2.0. The full text is in `Editor/ImageMagik/Magick.NET-License.txt`.
* **Files:** `Magick.NET-Q16-x64.dll`, `Magick.NET.Core.dll`, `Magick.Native-Q16-x64.dll` and `Magick.Native-Q16-x64.dll.so`, all in `Editor/ImageMagik/`. They are the official NuGet builds, unchanged.

## ImageMagick 7.1.2-32 and the libraries inside Magick.Native

`Magick.Native-Q16-x64.dll` and `Magick.Native-Q16-x64.dll.so` contain ImageMagick, under the ImageMagick License (https://imagemagick.org/license/), and the image, font and compression libraries it is built with, among them FreeType, HarfBuzz, libjpeg-turbo, libpng, libtiff, libwebp, libheif, libraw, OpenJPEG, Little CMS, zlib and zstd.

Every one of them is listed with its copyright notice and full licence in `Editor/ImageMagik/Magick.NET-Notice.txt`, exactly as Magick.NET publishes it. Their source code is available from https://github.com/dlemstra/Magick.NET and from each project named in that file.

## Thry's Avatar Performance Tools

* **By:** Thryrallo, https://github.com/Thryrallo/VRC-Avatar-Performance-Tools
* **Licence:** MIT, full text below.
* **Used in:** the Optimization Suite (`Editor/Avatar Tools/Avatar Validator/VixenAvatarValidator.cs`). The mesh memory estimate (`MeshBytes`: bytes per vertex attribute, double position, normal and tangent data on skinned meshes, 40 bytes per blend shape vertex), its 80 MB limit, the 100 and 150 Any State transition limits and the grab pass check are adapted from its Avatar Evaluator and VRAM Checker.

```
MIT License

Copyright (c) 2022 Thryrallo

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## d4rkAvatarOptimizer

* **By:** d4rkpl4y3r, https://github.com/d4rkc0d3r/d4rkAvatarOptimizer
* **Licence:** MIT, full text below.
* **Used in:** the Optimization Suite. Removing unused blend shapes and baking the ones set above zero, merging repeated material slots, removing PhysBone colliders that nothing lists, reporting animations that point at missing objects, moving normal maps to BC5, running fixes on the upload copy only, turning PhysBones off with the one mesh they move, removing unused animator parameters, and merging blend shapes that always move in the same ratio follow ideas from d4rkAvatarOptimizer. The ring finger collider placement on the feet (`MoveRingFingerCollidersToFeet` in `Editor/Avatar Tools/Avatar Validator/VixenUploadOptimizer.cs`) is adapted from its code.

```
MIT License

Copyright (c) 2021 d4rkpl4y3r

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## KaleidoVR VRChat Model Optimizer

* **By:** KaleidoVR, https://github.com/KaleidoVR/VRChat-Model-Optimizer. All rights reserved.
* **Used in:** the Optimization Suite. Converting Unity constraints with the VRChat SDK's converter and turning off motion vectors on skinned meshes follow ideas from its README. No code was copied.
