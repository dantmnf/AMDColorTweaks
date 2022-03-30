# About
This tool uses an undocumented AMD API, to convert color space before sending them to a display that effectively implements color-managed output (e.g. clamp content to sRGB), based on the chromaticities and transfer characteristics provided.

AMD control panel also provides a "Color Temperature Control" switch to convert colors to color space defined in EDID, but it is not possible to convert colors to custom color spaces (e.g. from measured ICC profile).

⏬ **[Download latest release](https://github.com/dantmnf/AMDColorTweaks/releases/tag/ci-build)**

# Notes for use with ICC profiles
* From [ledoge/novideo_srgb](https://github.com/ledoge/novideo_srgb):
  > * Since the color space conversion is done on the GPU side, the ICC profile must not be selected/loaded in Windows or any other application. If you want, you can do another profiling run on top of the active calibration and then use this profile in applications that support color management to achieve even better color accuracy.
  > * To achieve optimal results, consider creating a custom testchart in DisplayCAL with a high number of neutral (grayscale) patches. With those, a grayscale calibration (setting "Tone curve" to anything other than "As measured") should be unnecessary and might even be detrimental to the accuracy. The number of colored patches should not matter much. Additionally, configuring DisplayCAL to generate a "Curves + matrix" profile with "Black point compensation" disabled may also result in better accuracy than with an XYZ LUT profile.
* Only `vcgt` (if present), `rXYZ`, `gXYZ`, `bXYZ`, `rTRC`, `gTRC`, `bTRC` in ICC profile are used.
* You can generate an ICC profile from EDID in DisplayCAL by selecting "Create profile from extended display identification data..." in "File" menu.
# FAQ

* ### My screen turns very green after clicking apply
  Specify white point coordinate rather than using driver-defined ones from drop-down list.

* ### I toggled some random settings in AMD control panel and the calibration gone
  Simply apply it again.

* ### I clicked on the Apply button and the system hangs
  Ask AMD why they don't validate input from unprivileged userspace API. ¯\\_(ツ)_/¯ 

* ### I see banding!
  ¯\\_(ツ)_/¯ 

* ### What about source transfer?
  It is managed by Windows for proper hardware multi-layer composition.

# Known issues

* The space of chromaticity values passed to the driver is to be determined (PCS-relative vs. illumient-relative).
* The usage / effect of "Apply degamma instead" switch in "Edit Transfer" window is to be determined.

# Building from source

Despite targeting the good old .NET Framework, you need .NET Core SDK to build this tool.

This tool uses [Little CMS](https://www.littlecms.com/color-engine/) for ICC reading, run `littlecms/download.bat` to prepare Little CMS source, and it will be picked up by the solution file.

# Alternatives
* [ledoge/novideo_srgb](https://github.com/ledoge/novideo_srgb): for NVIDIA GPUs.
* [ledoge/dwm_lut](https://github.com/ledoge/dwm_lut): for any GPU on Windows 10+.
* [Intel GPU Control Library](https://intel.github.io/drivers.gpu.control-library/Control/INTRO.html): (API only) hardware matrix-LUT or 3DLUT on Intel GPUs.
* Microsoft Hardware Color v2: used in some recent Windows devices with built-in wide gamut display. Currently no public way to use it with homemade calibration (coming soon™).
