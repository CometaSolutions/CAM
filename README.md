# CAM

CIL Assembly Manipulator (CAM) is a project providing high-level API to read and emit CLR assemblies and modules. As its side-kick, this repository is also home for UtilPack. UtilPack is small and very portable class library containing various useful classes and extension methods.

## Usage

Currently there isn't that much documentation.
The starting point is always `CILReflectionContext` (acquireable from `CILAssemblyManipulator.DotNET` assembly), and there are extension methods to provide easy wrapping of native System.Reflection objects into CILAssembly variants.

## Threadsafety

Separate `CILReflectionContext`s are always threadsafe as they don't interact with each other.
The following threadsafety rules apply within the same `CILReflectionContext`:
* read operations are always threadsafe (including making generic instances of types and methods),
* write operations changing some single value (e.g. an enum or string) are threadsafe, and
* write operations changing collections (adding/removing a module, type, method, method parameter, field, property, event, etc) are _not_ threadsafe.
