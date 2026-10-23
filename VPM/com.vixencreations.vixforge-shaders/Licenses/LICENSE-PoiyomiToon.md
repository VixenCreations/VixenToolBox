# Poiyomi Toon

Poiyomi Toon by Poiyomi Labs: https://github.com/poiyomi/PoiyomiToonShader

**Dissolve** in all six shaders is adapted from the Dissolve in Poiyomi Toon 10.0 (the free, MIT release), in
`VixForgeDissolve.cginc`: the dissolve front and its padding, Wave Mode, Continuous Dissolve, the Edge Line,
Vertex Height and the 3D dot noise. It keeps Poiyomi's property names. Changes from the original: one field
serves every pass (body, shadow, outlines, fur, shards, trails, HUD and goo), the front is placed without the
pixel footprint so the shadow lands where the body is cut, the dot noise rides on the model instead of the
world, Vertex Height reads the same Amount and mask as the body, and the colours come from AudioLink in place
of Poiyomi's theme colours.

Poiyomi Toon's license file follows, unchanged.

---

MIT License

Copyright (c) 2026 Poiyomi Labs

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