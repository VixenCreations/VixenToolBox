# What's New In 2.18

**See what slows the editor down, find what a tool left in memory, and a safer ImageMagick.**

## Scene View Enhancer

Find it at `VixenTools/Unity Engine/Scene View Enhancer`. It caps the editor's frame rate while it is idle, through Unity's own Interaction Mode setting, and puts your old setting back when you turn it off. Its **Vixen Scene Stats** panel in the Scene view shows how often the view redraws and how much memory scripts, Unity and the graphics driver are using.

## Memory Diagnostics

At `VixenTools/Unity Engine/Memory Diagnostics`. Take a snapshot, repeat the task you want to test a few times, then compare. Anything a tool made and never freed shows up in the list. **Largest Textures** shows what is using the most memory.

## World Engine

The performance map is now a tab in the World Engine window instead of a window of its own. Several checks that newer ProTV, TXL, LTCGI and UdonSharp releases had quietly broken work again, and the compute score no longer counts baked reflection probes or cameras that draw nothing.

## Safer Images

ImageMagick, which the toolbox uses to resize and build textures, is updated with 25 security fixes, and it now refuses file formats the toolbox never needs.

***
# Thank You For 3,500 Downloads

**September 14, 2026**

The Vixens Toolbox has passed 3,500 downloads. In July we were thanking you for 1,500, and you have more than doubled it since by installing it, telling us what broke and passing it on to friends. Thank you.

***
### Get The Toolbox

Add it to the Creator Companion from [vixencreations.github.io/VixenToolBox](https://vixencreations.github.io/VixenToolBox/).
