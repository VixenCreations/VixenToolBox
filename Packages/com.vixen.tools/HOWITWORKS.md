# VIXEN WORLD ENGINE : HEURISTICS ARCHITECTURE

The **Vixen Compute Score** is a proprietary, weighted heuristic model designed for the VRChat ecosystem. It evaluates the raw structural complexity of a scene rather than relying on Unity's frame-timing (ms) metrics. 

Because frame times fluctuate drastically based on the end user's CPU/GPU, hardware-based profiling is notoriously unreliable for generalized world optimization. By analyzing the matrix of active components, draw calls, and mathematical operations, the Vixen World Engine provides a deterministic, hardware-agnostic threat level for your world.

***
## THE GHOST COMPONENT FILTER
Unity's internal `FindObjectsOfType()` API suffers from a known serialization issue: it often includes components that are physically disabled, or reside on deactivated GameObjects, bloating performance estimates. 

The Vixen Engine bypasses this using a **Strict Tuple-Keyed Scene Object Cache**. Every component calculated in the Compute Score undergoes a strict `object.enabled && object.gameObject.activeInHierarchy` lock. If a creator toggles off a room or a prop, every single draw call, light, and physics body inside it is instantly zeroed out of the threat level.

***
## HEURISTIC MULTIPLIERS (COMPUTE LOAD)
The compute score is calculated by multiplying active scene elements against their historical performance cost in the VRChat client.

**DRAW CALLS (`x 0.50`)**
Calculated by summing the `sharedMaterials` of all actively rendering meshes. Excessive draw calls bottleneck the CPU as it struggles to instruct the GPU on what to render.

**MESH COLLIDERS (`x 0.50`)**
Unlike primitive colliders (Box, Sphere, Capsule), Mesh Colliders require the physics engine to evaluate complex polygon intersections every frame.

**AUDIO SOURCES (`x 1.50`)**
Active audio sources require CPU time for spatialization, doppler effect calculations, and decoding decompression.

**STATIC LIGHT VOLUMES (`x 1.50`)**
VRC Light Volumes provide excellent baked lighting, but processing the spherical threshold parameters across the scene geometry carries a slight overhead.

**RIGIDBODIES (`x 2.00`)**
Every active rigidbody must be evaluated by the PhysX engine per fixed update step. Note: Rigidbodies are always calculated if their parent GameObject is active, as they do not have a separate `.enabled` toggle.

**POINT LIGHT VOLUMES (`x 4.00`)**
Dynamic variations of light volumes require constant recalculation of the volume matrix against moving objects.

**REFLECTION PROBES (`x 10.00`)**
A Realtime Reflection Probe is essentially a 6-sided camera. It forces the Unity rendering pipeline to draw the surrounding geometry 6 additional times to map the cubemap faces.

**LTCGI SCREENS (`x 15.00`)**
Real-time polygonal area lighting (LTCGI) is revolutionary, but evaluates complex intersection models and shadow masking per-fragment, per-screen.

**ACTIVE ROGUE CAMERAS (`x 50.00`)**
Any camera rendering to the screen (not a RenderTexture) with an active culling mask forces the engine to double-render the entire world geometry. This does *not* include safe UI Event Cameras (Culling Mask = 0).

**RT SHADOW CASTERS (`x 80.00`)**
Realtime Shadow casting lights (Point, Spot, or Directional) force the GPU to render a depth map of the scene for *each* active light. Overlapping realtime shadows will exponentially multiply your draw calls and instantly kill performance.

**AUDIOLINK CORES (`x 150.00`)**
AudioLink is incredibly powerful, but running the core requires Unity to read audio spectrum data, perform Fast Fourier Transforms (FFT), and push that data into a massive Render Texture every single frame. Multiple active cores will cripple a world.

***
## THREAT LEVEL SCALES

- **OPTIMAL (`< 100 Score`):** Flawless architecture. Will run smoothly on Quest standalone and low-end VR hardware.
- **MODERATE (`100 - 249 Score`):** Standard PCVR baseline. Expected performance for medium-sized social instances.
- **HIGH (`250 - 499 Score`):** Heavy compute load. Requires users to have modern hardware; Quest instances will likely suffer severe frame drops.
- **SEVERE (`500+ Score`):** Critical architectural failure. Scene topology is bloated. Expect massive frame hitching, high crash rates, and unplayable conditions on anything but top-tier hardware.

***
## VRAM ESTIMATION (MEMORY FOOTPRINT)
Unlike Compute (which is entirely based on active state), VRAM is calculated using Unity's native `Profiler.GetRuntimeMemorySizeLong()`. 

**Memory footprint remains static regardless of component active state.** Unity pushes all referenced textures, meshes, Lightmaps, LTCGI LUTs, and UI elements into GPU memory immediately upon scene load. To lower your VRAM, you must physically remove the asset from the scene, lower its Max Resolution in the inspector, or utilize aggressive Crunch Compression.