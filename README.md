# ParameterIlliterate

ParameterIlliterate is a CLI tool that allows for .bprm files to be turned into readable JSON files without hashed names.

## Usage

Drag-and-drop a .bprm file onto ParameterIlliterate.exe to convert it to JSON. JSON to .bprm is not supported, but is hopefully planned sometime in the future.
```
ParameterIlliterate.exe <.bprm file name here>
```

## Known Issues

The `deferred_opa_alphamask.f32` parameter and the `mSmoothStopDistance.f32` parameter have a hash collision. `mSmoothStopDistance.f32` is prioritized every time, so some instances of said parameter may be incorrectly labelled.

## Credits

💖 Shadów for [LightByml](https://github.com/shadowninja108/LightByml/) and helping with some stuff.
