# Portrait Helper

**Portrait Helper** allows you to save and load portrait presets, edit camera and pose settings in detail, and checks whether the portrait still matches the character's current appearance and gear.

Portrait Helper opens automatically when you edit a portrait. It's a small bar at the top of the window.

This plugin is hopefully soon available in the Dalamud plugin repository!

## Features

- **Preset Browser**  
  Save your favorite portraits as reusable presets. Simply double-click to load one!
    - Preset previews are saved as png files to the plugins configuration folder and contain the preset string as Exif metadata (in the UserComment field).
- **Clipboard Import/Export**  
  Share your portraits easily with base64-encoded strings.
  - If you want more control over whats being imported, use the Advanced Import Mode to selectively apply only the parts of a preset you care about.
- **Advanced Edit Mode**  
  Fine-tune your portrait with full control over
    - Camera yaw, pitch, distance, position (X/Y)
    - Zoom and rotation
    - Head and eye direction
    - Animation timestamp
- **Alignment Tool**  
  Displays on-screen guide lines to help with portrait composition and symmetry.
- **Reset Button**  
  Quickly revert any changes made since opening the Portrait Editor.
- **Portrait Mismatch Warning**  
  Notifies you in chat when your portrait no longer matches your current appearance or gear.  
  Click the chat message to open the Portrait Editor.


## Known Issues

- **Scrolling the animation timestamp forward does not advance the weapon animation of emotes like Battle Stance or Victory.**  
  Workaround: Scroll backwards once for it to reload the animation.  
  The animation system has not yet been fully understood/reverse engineered yet. Because forward and backward scrolling use different techniques, the animation will reload when scrolling backwards, updating the weapon positions too.
- **It's possible to load animations that are job-specific with other jobs, but they end up stuck.**  
  Yup. Load a different animation then.
