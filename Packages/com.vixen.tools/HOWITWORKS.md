# World Engine: How The Score Works

The **PERFORMANCE MAP** tab in the World Engine gives your world a compute score. It counts what is switched on in your scene and weights each thing by how much it tends to cost in VRChat. It does not time your frames, because frame times depend on each player's PC or Quest, so the score comes out the same on every machine.

Only enabled components on active objects count. Switch off a room and everything in it drops out of the score.

***
## What Counts, And How Much

* **Draw calls, x0.5:** every material slot on an active renderer.
* **Mesh colliders, x0.5.** Box, sphere and capsule colliders are much cheaper and do not count.
* **Audio sources, x1.5.**
* **Light Volumes, x1.5** each.
* **Rigidbodies, x2.** These count whenever their object is active, because a Rigidbody has no on and off switch.
* **Point Light Volumes, x4** each.
* **Realtime reflection probes, x10.** A realtime probe draws the room six more times to build its cubemap. Baked probes cost nothing while the world runs and do not count.
* **LTCGI screens, x15.**
* **Extra cameras, x50.** A camera that renders the world draws the whole scene again, whether it renders to the screen or to a Render Texture. Your main camera and cameras with their Culling Mask set to Nothing do not count.
* **Realtime shadows, x80** for each light that casts shadows and is not set to Baked. Each one draws the scene again for its shadow map.
* **AudioLink, x150** for each active AudioLink.

***
## The Ratings

* **Optimal (under 100):** nothing to worry about, even on Quest.
* **Moderate (100 to 249):** a typical PC world.
* **High (250 to 499):** heavy. Quest players will likely see frame drops.
* **Severe (500 and up):** far too heavy. Expect stutter and crashes on anything but a strong PC.

***
## Memory

The Performance Map also estimates how much memory your world needs: textures, meshes, UI, lightmaps, Light Volumes, AudioLink and LTCGI data. Audio memory is shown on its own line.

Unity loads everything your scene uses as soon as the scene loads, so switching objects off does not lower this number. To bring it down, remove what you do not need, or lower a texture's Max Size or raise its compression.
