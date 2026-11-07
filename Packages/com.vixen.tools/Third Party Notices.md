# Third Party Notices

Vixens Toolbox includes the third party software below. Each part keeps its own licence. The toolbox's own code is under the MIT licence in `LICENSE.md`.

## Make It MMD

* **By:** enitimeago, https://github.com/enitimeago/make-it-mmd
* **Licence:** MIT License, Copyright (c) 2024 enitimeago.
* **Used for:** VixenMMD reads and writes Make It MMD's `.json` preset format, and its list of MMD morph names builds on Make It MMD's list, which credits Xoriu's MMD Facial Expressions Chart, iY's MMD World sheet, lilxyzw and Avatar Optimizer. VixenMMD also draws names from InochiPM's MMD Facial Chart. No Make It MMD code ships in the toolbox.

## Magick.NET 14.17.2

* **By:** Dirk Lemstra, https://github.com/dlemstra/Magick.NET
* **Licence:** Apache License 2.0. The full text is in `Editor/ImageMagik/Magick.NET-License.txt`.
* **Files:** `Magick.NET-Q16-x64.dll`, `Magick.NET.Core.dll`, `Magick.Native-Q16-x64.dll` and `Magick.Native-Q16-x64.dll.so`, all in `Editor/ImageMagik/`. They are the official NuGet builds, unchanged.

## ImageMagick 7.1.2-32 and the libraries inside Magick.Native

`Magick.Native-Q16-x64.dll` and `Magick.Native-Q16-x64.dll.so` contain ImageMagick, under the ImageMagick License (https://imagemagick.org/license/), and the image, font and compression libraries it is built with, among them FreeType, HarfBuzz, libjpeg-turbo, libpng, libtiff, libwebp, libheif, libraw, OpenJPEG, Little CMS, zlib and zstd.

Every one of them is listed with its copyright notice and full licence in `Editor/ImageMagik/Magick.NET-Notice.txt`, exactly as Magick.NET publishes it. Their source code is available from https://github.com/dlemstra/Magick.NET and from each project named in that file.
