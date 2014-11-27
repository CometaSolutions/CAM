# CAM

CIL Assembly Manipulator (CAM) is a project providing high-level API to read and emit CLR assemblies and modules. As its side-kick, this repository is also home for UtilPack. UtilPack is small and very portable class library containing various useful classes and extension methods.

## Overview and acquiring

There are several NuGet packages - UtilPack, CILAssemblyManipulator.Portable, CILAssemblyManipulator.DotNET, CILMerge.MSBuild.

### UtilPack

This is small collection of miscellaneous but useful utility types and methods, all bundled up in a very portable library.

NuGet: https://www.nuget.org/packages/UtilPack/ (direct link: http://www.nuget.org/api/v2/package/UtilPack/  )

### CILAssemblyManipulator.Portable

This library is the heart of this repository.
It exposes high-level API to read and emit CLR assemblies and modules.
This library is not standalone - it leaves some gaps to be filled by CILAssemblyManipulator.DotNET (and other packages in future).

NuGet: https://www.nuget.org/packages/CILAssemblyManipulator.Portable/ (direct link: http://www.nuget.org/api/v2/package/CILAssemblyManipulator.Portable/  )

### CILAssemblyManipulator.DotNET

CILAssemblyManipulator.DotNET fills the gaps left by CILAssemblyManipulator.Portable, exposing DotNETReflectionContext class which will be used as entry point for CIL manipulating and reading operations of your application.
This library is not portable - it requires .NET 4 Client Profile.

NuGet: https://www.nuget.org/packages/CILAssemblyManipulator.DotNET/ (direct link: http://www.nuget.org/api/v2/package/CILAssemblyManipulator.DotNET/ )

### CILMerge.MSBuild

CILMerge.MSBuild exposes .targets file containing MSBuild task to merge assemblies.
It works in very similar way to other CIL merge utilities out there, except that it is not an .exe but instead a .dll exposed through MSBuild task.

NuGet: https://www.nuget.org/packages/CILMerge.MSBuild/ (direct link: http://www.nuget.org/api/v2/package/CILMerge.MSBuild/ )

## Usage

Currently there isn't that much documentation.
The starting point is always `CILReflectionContext` (acquireable from `CILAssemblyManipulator.DotNET` assembly), and there are extension methods to provide easy wrapping of native System.Reflection objects into CILAssembly variants.

## Threadsafety

Separate `CILReflectionContext`s are always threadsafe as they don't interact with each other.
The following threadsafety rules apply within the same `CILReflectionContext`:
* read operations are always threadsafe (including making generic instances of types and methods),
* write operations changing some single value (e.g. an enum or string) are threadsafe, and
* write operations changing collections (adding/removing a module, type, method, method parameter, field, property, event, etc) are _not_ threadsafe.
