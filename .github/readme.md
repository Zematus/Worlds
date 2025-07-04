Worlds 0.3.2.2 - by Juan Bernardo Tamez Pena (DrTardigrade)


-- IMPORTING HEIGHTMAPS --

To generate a world using a heightmap image first you need to copy the image to Worlds\\Heigthmap. The image can be any of the following formats (extensions): PSD, TIFF, JPG, TGA, PNG, BMP, and PICT.

The image must be at least 400x200 pixels in size. Any image larger in size will be scaled to have an aspect ratio of 2:1, with a respective resolution of 400x200.

The image doesn't require to be a grayscale image. Internally, the game will convert the source image to grayscale by extracting the grayscale value of each pixel (using Unity's Color.grayscale). Lighter colors will turn into lighter shades of gray and viceversa. When scaling down an image larger than 400x200, the colors of neighboring pixels will be averaged.

Once the image is converted to grayscale, the game will interpret the shades of gray as values between 0 and 1, where 0 is black and 1 is white. These values will then be mapped to height values between -15000 meters and 15000 meters. For example, a grayscale value of 0.2 becomes -9000 meters. 0.6 becomes 3000 meters. 0.5 becomes 0 meters.

Note that the results from using a heightmap to generate a world might not perfect. Upon generating a map, the actual sea level and mountain scales might not be what you expected from the source image. In that case you can use the map editor to correct and adjust the generated world map to better match the source.


-- KEYBOARD SHORTCUTS --

---- Start Scene:

- Ctrl-L : Open 'Load World' dialog
- Ctrl-G : Open 'Generate New World' dialog

---- Main/World Scene:

- Ctrl-L : Open 'Load World' dialog
- Ctrl-G : Open 'Generate New World' dialog
- Ctrl-S : Open 'Save World' dialog
- Ctrl-X : Open 'Export Image' dialog
- Shift-G : Switch between globe map projection and flat map projection
- +/=, Numpad + : Zoom map in
- \_/-, Numpad - : Zoom map out
- L : Switch between sun-lit and front-lit light modes (globe projection only)
- R : Toggle world rotation camera follow (globe projection only)
- Esc : Close current dialog / Open Main Menu
- V : Cycle between Map View Modes
- M : Cycle between 'Miscellaneous' map overlays
- N : Disable active map overlay

------- In Editor Mode:

- 1: Select 'Scale Terrain Altitude' Tool
- 2: Select 'Set Sea Level Offset' Tool
- 3: Select 'Set Average World Temperature' Tool
- 4: Select 'Set Average Yearly Rainfall' Tool
- 5: Select 'Altitude Brush' Tool
- 6: Select 'Temperature Brush' Tool
- 7: Select 'Rainfall Brush' Tool
- Ctrl-Z : Undo editor action
- Ctrl-Shift-Z : Redo undid editor action

------- In Simulator Mode:

- O : Cycle between 'Population' map overlays
- P : Cycle between 'Polity' map overlays
- G : Enable 'General' map overlay
- Tab : Cycle between overlay subtypes when available
- 1: Set simulation speed to 'Max 1 Day/sec'
- 2: Set simulation speed to 'Max 7 Days/sec'
- 3: Set simulation speed to 'Max 30 Days/sec'
- 4: Set simulation speed to 'Max 1 Year/sec'
- 5: Set simulation speed to 'Max 10 Years/sec'
- 6: Set simulation speed to 'Max 100 Years/sec'
- 7: Set simulation speed to 'Max 1000 Years/sec'
- 8: Set simulation speed to 'Unlimited'
- Spacebar: Pause/unpause simulation
