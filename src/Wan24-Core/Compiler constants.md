# Compiler constants

## `NO_INLINE`

If this compiler constant has been defined, no aggressive method inlining 
would be used.

## `NO_UNSAFE`

If this compiler constant has been defined, no "unsafe" code would be used. 
The "allow unsafe code" compiler flag can be removed in this case.

## `PREVIEW`

If this compiler constant has been defined, also preview features will be 
available in a release build. Regarding .NET preview features you'll also have 
to modify the project configuration to include .NET preview features in a 
release build.
