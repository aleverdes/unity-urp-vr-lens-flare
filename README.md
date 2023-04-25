# VR Lens Flare

Unity URP implementation of Lens Flare optimized for mobile VR.

This pack allows you to use an optimized version of Lens Flares in mobile VR (URP only).

# How to use

1. Add VR Lens Flare Render Feature to your URP Renderer.

![How to add URP VR Lens Flare renderer feature](https://raw.githubusercontent.com/aleverdes/unity-urp-vr-lens-flare/master/README%20Assets/How%20to%20add%20VR%20Lens%20Flare%20renderer%20feature.jpg)

2. Attach the Lens Flare (VR) component to your object or light source.

![How to add VR Lens flare component to game object](https://raw.githubusercontent.com/aleverdes/unity-urp-vr-lens-flare/master/README%20Assets/How%20to%20add%20VR%20Lens%20Flare%20component%20to%20game%20object.jpg)

3. Create and configure or take a ready-made asset with lens flare settings. Ready-made lens flare can be found in the Presets folder.

![How to create VR Lens Flare Data asset](https://raw.githubusercontent.com/aleverdes/unity-urp-vr-lens-flare/master/README%20Assets/How%20to%20create%20VR%20Lens%20Flare%20Data.jpg)

# Limitations

This package version is based only on image lens flares data.

Compatibility with standard Lens Flare Data (URP) and the ability to use procedurally generated lens flare parts will be in future versions.

This version does not work on occlusion, but on raycast to allow you to work in mobile VR with 72 FPS with any number of lens flare sources. You can configure the raycast mask in the add renderer feature menu in your URP asset.

# Screenshots

![VR Lens Flare Screenshot 1](https://raw.githubusercontent.com/aleverdes/unity-urp-vr-lens-flare/master/README%20Assets/VR%20Lens%20Flare%20Screenshot%201.jpg)

![VR Lens Flare Screenshot 2](https://raw.githubusercontent.com/aleverdes/unity-urp-vr-lens-flare/master/README%20Assets/VR%20Lens%20Flare%20Screenshot%202.jpg)
