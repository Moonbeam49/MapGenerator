# MapGenerator
**![](https://lh6.googleusercontent.com/BhgQ_J8aP-P5GJkCkrLL1NryDLY9d4pWswZxJDW-Ic0fUXG3_H6pmtjiVBfRbchYusGmTHwF8Pp3JEpKHGqxmvJp0U_cbyRNTt-8WHrnWsAj4tu-lOy3Dw41YEX7O4RSYXzTF8wTvSsrql27jQMmh8L1pJ7qo002dDUHIo7mFVnGLwuwBdUItzh_oA)**
C# tool developed with Unity, used to generate heightmaps as float arrays with values ranging from 0 to 1. The output array is stored on the drive and can be further used for any purpose. Current maps can be generated using either one of the following algorithms: Noise - a simple perlin noise heightmap, and Shaped - custom algorithm that generates and places a given amount of individual islands.

Tool is built and tested for Windows only.
# Table of contents
- [Installation Guide](#installation-guide)
- [Controls](#controls)
- [Map Generator Options](#map-generator-options)
- [Output File Location](#output-file-location)
- [Shape Algorithm Description](#shape-algorithm-description)
- [Code location and output files](#code-location-and-output-files)
- [Development time](#development-time)

# Installation Guide  
Download the [version for Windows](https://github.com/Moonbeam49/MapGenerator/blob/main/Builds/MapGeneratorWin.zip) from here and unpack the archive on your hard drive, then run the unpacked executable (MapGenerator.exe or MapGenerator.x86_64) to start the application.

Windows version might require the latest version of [directX](https://www.microsoft.com/en-us/Download/confirmation.aspx?id=35) and [vcredist](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170).

[Back to top](#table-of-contents)
# Controls
List of the available hotkeys with their in-app descriptions.

<img src="https://lh3.googleusercontent.com/wKySGiOdncY9RYnwQtRmyGgdHEjsc5kMPesQ9Mn9E9_0vfaAAEndADk61AaUilhOivbzbyNOm8resxJbfO_9pUjUeoQ5GP6xc0kWdPEdfl4ik_JETI66V_0nCL4fSuQtf3sI-VioXT2rYmOp9uUhuqeQbOX92hhFeSdXh2hImVlIvdvFU7HXgfaL4Q" alt="drawing" width="300"/>

[Back to top](#table-of-contents)
# Map Generator Options
<img src="https://lh5.googleusercontent.com/R1RTdfinEh7nUMaE_aeA4cpO4dGBhCrfa_bjJJpL31Um8v2EFalpK3sDCa04WSjyPGnbgurjhtM2tYJAboiDDCUCeENdu0oMkRXGSmI0KmeJLptZlLxvSppVgozOvCf_X2At2o85EINlEuO5asoLo8D64VMW-Z37SkI11Gu_bow1QBAscKZ0o9eeFw" alt="drawing" width = 300 align="left"/>

List of the available map generation options with their in-app description.
## General Settings
### Seed
Seed used for random value generation. Will create a unique map every time if left empty.
### Map size
Defines the amount of tiles the map will have.
### Algorithm
Selects which algorithm will be used to create the heightmap.
Noise - simple perlin noise map
Shaped - creates a number of islands with unique shapes, which are then filled with noise values.
### Create mesh
Defines if the created heightmap should be converted into a 3d mesh. If left unchecked, only the 2d minimap image will be created.
## Shape Algorithm Settings

### Shape count

Sets the amount of individual islands that will be generated. In case the amount of shapes exceeds the map size limitations, actual island count will be displayed here.
### Shape size

Sets the approximate size of the created shapes. Relative value.
## Extra Shape Settings

### Density radius
Defines the distance in tiles at which existing land will be detected during shape creation. Higher values will affect how hard the existing shapes will affect the one that is currently created. Accepts values from 1 to 64.

### Contour radius
Sets the maximum allowed distance in tiles between the finished and baseline contours. Higher values add more curves to the finished contour. Accepts values between 3 and 16.

### Contour consistency
Defines how closely contour will follow its land percentage recommendations. Values represent the percentage range at the current position in which fill recommendations will be ignored, and go from 0.3 to 0.1.
## Noise Sampling Settings
### Noise scale
Sets the scale at which the noise will be sampled. Higher values provide a zoom in effect.
### Noise octaves
Sets the amount of passes for noise sampling.
### Noise persistence
Defines how hard each noise sampling pass will affect the end result. Higher values produce less consistent terrain. Accepted values range from 0 to 1.
### Noise lacunarity
Defines the distance between noise sampling points. Higher values will create less consistent terrain. Values go from 1 to 10.

[Back to top](#table-of-contents)
# Output File Location
Application allows you to save and load maps as well as settings profiles on the hard drive.

Following default folders are used for storage:

Maps - AppLocation\MapGenerator_Data\Maps

Settings - AppLocation\MapGenerator_Data\Settings

[Back to top](#table-of-contents)

# Shape Algorithm Description
In general, the algorithm follows the process of drawing a freeform closed shape by hand.

<img src="https://lh5.googleusercontent.com/P1M3iudgAlL-YDokPOxttck6k_yIxHvckjV_Bifgy0oWJep4rOhPGNYvwEVYruVr2ZEfr-_j29TGmcubpPZSPJD9qhBeBWlQncPQfQjRRkF11Dm7M2KhAie9mntKW7Gw3vBB10wswCsjQdDT8cJBWcVzY-4XaYmh_yTfbTG6kWMXLJA59nhyrFRb5A" alt="drawing" width = 250/>

**Step 1 - setting up limitations.**

Shape creation process starts with a given box (yellow) which will be used as an outer limit for the currently created shape. Box is then shrunk to create an inner limit one (red).

All of the area between those boxes will be available for the shape creation.

<img src="https://lh4.googleusercontent.com/qqXJsBXopw5CmMR0lgpAszAYg6uTNx957pnDmGInmlGdofCIE64vyLkw_Rt4xuBcyQjDeH1fKm-SB21RhrZEhY8Y2Zc51Y_zBaJYGgkNA9G4Sg4jsQbuiWPH-o8B7K29s69s1PFl8cy1klijQhR6ACXtaAGvbSl6iikjp7br_lf-AR25ZKCBzipjfQ" alt="drawing" width = 250/>

**Step 2 - creating dots.**

After the limits are set, the first dot will be sampled in the area between set limitations along the longer side of the outer box (left or top). Going from the first dot, until the shape will be closed, the algorithm selects an angle at which the next dot will be set, while also checking for collisions.

<img src="https://lh4.googleusercontent.com/WHwqV2u6otnHubL1D5WoELuSXikYkOa82r3JM56kbjsp014cMKk_Iue1K4sjRwD1ry7RD6zNH-0VIJgkUOzWZMICPpYEBNejJt4Jcyu9LYSrhmm9mJauQPKNiZRyk5fZ_5xYmD4CVUlttpGOZjPRCdUUMxzwg407u8KBcP25gRx6na-i0EDnU_xlgw" alt="drawing" width = 250/>

**Step 3 - basic contour creation.**

After all of the dots are generated the algorithm creates the shape's basic contour by simply connecting the dots.

<img src="https://lh5.googleusercontent.com/pVvGOPAKDkwGFMx38cKC5_zvk5zhDdNhPHU206TUxnUcaQJCsS5nNNj_Eu17Wq4Bi7RqSePrw15JQoXAgbQz_SzliWPOCEwGvUTfpesTxmOjY9yXHWnAR4f3QygUjHn_Iwv8q4QrQdu1KbkJvYvyT4pI4jEt04dncK5LkvzUmtI2jL0T_L3rLgsmkg" alt="drawing" width = 250/>

**Step 4 - creating perlin noise contour.**

In order to create a less calculated look for the shape’s contour, the algorithm does a final step in contour creation - creates a perlin noise worm guided by the previously created baseline contour.

<img src="https://lh6.googleusercontent.com/5Kjl1I7kidTdsGFdp2QYVW_IcBxzhI17z_7rC9H4lG42IoBcrDG75hGYY8g861dToCGVmmXMytj6jUklcJXm_D0hGvIGzx7quBc6u9riWsaYMPv6TUSn8jm5JjmFECC2l_KVIYxJ6WLuPzMmiHzyKIbw1nMrzSeJcvnOe99c5sdEq8ldo9AiTlCd6Q" alt="drawing" width = 250/>

**Step 5 - filling contour with noise values.**

After the contour is finalized, it is filled with the perlin noise values with the account for each dot’s distance from the contour.

<img src="https://lh6.googleusercontent.com/k3RHOW0X8Ldls2kGnc_UlviNmYqfOQ8CFj3JuQMICM0Gzzxf52UyMTbobUD73OU0LYGd7eSL0kdVrJn1EzMKUWDGFHiA7aalm6cjxfxfnvMdi-zrgcfn91qZLn7QaX-UzGCWj9a_Oddzg15CY2xw_J3KEc0eCJWY5caP1_B5-dDpvgr26hOwpzEZ1Q" alt="drawing" width = 250 />

**Step 6 - converting 2d heightmap into a 3d mesh.**

If requested by the application settings, the resulting heightmap will be converted into a 3d mesh for preview.

[Back to top](#table-of-contents)
# Code location and output files
Source code is located at [source\Assets\Scripts](https://github.com/Moonbeam49/MapGenerator/tree/main/Assets/Scripts)

[Back to top](#table-of-contents)
# Development time
Development to the current state took approximately 6-8 working weeks.

[Back to top](#table-of-contents)