# Introduction
This git repository is home for CILAssemblyManipulator (CAM) framework, UtilPack library, and CILMerge utility.

# NuGet packages
For people just looking links for NuGet packages:
* UtilPack: http://www.nuget.org/packages/UtilPack
* CAM.Physical: Coming soon
* CAM.Logical: Coming soon
* CAM.Structural: Coming soon

# CILAssemblyManipulator framework
The purpose of CILAssemblyManipulator (CAM) framework is to provide modifiable views onto CIL metadata (see e.g. [ECMA-335 Standard](http://www.ecma-international.org/publications/standards/Ecma-335.htm) to learn more about CIL metadata) on various abstraction levels.
There are three abstraction levels in CAM, from least abstract to most abstract:
* physical,
* structural,
* and logical.
Each abstraction level is represented by one library.
All three libraries have extremely portable API to use; the supported platforms are .NET 4, SL5, WPSL8, WPA8.1, and Win8.
However, not all the platforms currently have working implementation for e.g. cryptographic functions (needed to emit strong-named metadata).

The common guidelines that API on all abstraction levels adhers:
* Freeform, meaning that minimum amount of constraints (like ECMA metadata uniqueness/ordering constraints) are applied on API level,
* [KISS](https://en.wikipedia.org/wiki/KISS_principle) types, meaning that the types contain usually mostly simple data access methods/properties, and extension methods provide the more complicated functionality.

Because of the freeform feature, it is possible to have (and write to disk) invalid metadata.
The responsibility of producing valid metadata is left for user of the framework, be that manually ensuring some of the metadata constraints, or by simply calling methods that restructure the metadata so that at least some constraints are being adhered to then.
One notable example of such method is `OrderTablesAndRemoveDuplicates` extension method for `CILMetaData` type in CAM.Physical library, which will remove duplicates from some tables and re-order some more tables in metadata, so that those tables will conform to ECMA-335 ordering and uniqueness constraints.

Originally CAM started out as a bunch of utilitiers to support code generation in [Qi4CS](https://github.com/CometaSolutions/Qi4CS).
It soon became a fully fledged framework, with API which is now in CAM.Logical library.
After a lengthy refactor in 2015, CAM is now in current form with three libraries, one for each abstraction level.
Currently it is fully capable alternative to [Cecil](https://github.com/jbevain/cecil).

## CAM.Physical library
The CAM.Physical library represents the lowest abstraction level of CAM.
Here, metadata is represented as modifiable tables, rows, and signatures.
The API of those tables, rows, and signatures follows very closely the specifications and abstraction level of [ECMA-335 Standard](http://www.ecma-international.org/publications/standards/Ecma-335.htm).

Because of the low abstraction level, CAM.Physical is very light-weight and quick library.
For similar reasons, it is also not thread-safe to use.
Additionally, once again for similar reasons, equality comparison is exact and precise down to physical representation of metadata and its rows and signatures.
This means that two metadata instances of CAM.Physical library may be considered not equal, even though they would produce exactly equivalently behaving code.

The CAM.Physical library also provides some skeleton types (for portable edition) and ready-to-use types (for .NET edition) that will keep track which files have been loaded as CAM.Physical metadata objects.
This easens up a lot scenarios when it is needed to e.g. resolve assembly references into metadata objects.

With CAM.Physical library, developer gains maximal and full control over how metadata will be emitted.
The library only performs minimal sanity checks when emitting the metadata, trying to perform as quickly as possible without using unsafe blocks.
In this sense, the CAM.Physical library adopts the "the programmer knows what (s)he is doing, just do it quickly" approach familiar in C/C++ world.
This means that it is possible to emit metadata, which will be incorrect and CLR will refuse to load it, so be careful!
The `OrderTablesAndRemoveDuplicates` extension method for `CILMetaData` type should help to adher to most of the important CIL metadata constraints, so when uncertain, use that before emitting the metadata.

Note: if you are not bound too much by portability constraints, and you are looking for extremely quick framework providing **read-only** access ECMA metadata with similar abstraction level as CAM.Physical, you should use [System.Reflection.Metadata](http://www.nuget.org/packages/System.Reflection.Metadata/) package.

## CAM.Structural library
The CAM.Structural library represents the middle abstraction level of CAM.
In this library, metadata is represented as modifiable objects, and the concept of metadata tables and rows is no longer present.
So e.g. in CAM.Physical, in order to specify packing and/or class size for a type, one would add a row to ClassLayout table, containing the class and packing information for a type, and that row would hold information about which TypeDef row this ClassLayout row affects.
The same effect is achieved in CAM.Structural by directly modifying Layout property of affected TypeDefinitionStructure, resulting in a lot more natural API.

The abstraction level, however, is not much higher than CAM.Physical, so the CAM.Structural remains quite light-weight and quick, not thread-safe, and still following closely [ECMA-335 Standard](http://www.ecma-international.org/publications/standards/Ecma-335.htm) (e.g. TypeRefs can have custom attributes, etc).
Unlike in CAM.Physical, the equality comparison of CAM.Structural no longer takes physical representation into account, meaning that two metadata instances producing equivalently behaving code will be considered equal.
The CAM.Structural library also provides extension methods to create CAM.Structural metadata object out of CAM.Physical metadata objects, and vice versa.

The abstraction level provided by CAM.Structural roughly equals the abstraction level of the [Cecil](https://github.com/jbevain/cecil) framework.
However, the CAM.Structural does not have any concept of `universe`, so there are no `import` methods (also because all metadata objects are fully modifiable).
Like the CAM.Physical library, the CAM.Structural library does not have various checks for integrity and correctness of metadata, leaving the responsibility to adher to rules for the user of the library.

## CAM.Logical library
The CAM.Logical library represents the highest abstraction level of CAM.
At this abstraction level, metadata is represented as modifiable objects with API similar to objects of System.Reflection namespace.
However, these objects are no longer so light-weight compared to CAM.Physical and CAM.Structural objects.
The concept of `universe` is also introduced in CAM.Logical, and all metadata objects always belong to a certain universe.

Not all CIL metadata concepts and features are exposed in CAM.Logical.
For example, there is no concept of TypeReference (rows), and thus it is not possible to specify custom attributes for a type reference (if for some weird reason, that would be required).
Furthermore, the signatures are not present in CAM.Logical API (except `CILMethodSignature`, which represents function pointer type), and all metadata objects hold direct references to target methods/fields instead of having signatures of them.

The concept of thread-safety in CAM.Logical is not trivial.
The universes are represented by `CILReflectionContext` interface, and they can be thread-safe or not thread-safe.
If the universe is considered to be thread-safe, making generic methods/types and element types is safe to do concurrently.
However, concurrent modifying of types, properties, and other metadata objects is not safe to do concurrently within the scope of that object.
The "scope of that object" here means that, for example, it is always thread-safe, even in non-thread-safe universe, to add nested to types to different enclosing types.
However, it is never thread-safe to add types to the same enclosing type, or to the same module, even in thread-safe universe.

The equality comparison in CAM.Logical is by-reference, which is similar to how it is done in System.Reflection namespaces (e.g. two references to object representing the same method are always referencing the same object).
Therefore, the explicit equality comparers are not present in CAM.Logical library.

# UtilPack library
This is small and extremely portable library that contains various miscellaneous utility types and methods that I find to be essential in most of my projects, both personal and professional.
In that sense, it is much like [Jon Skeet's Miscellaneous Utility Library](http://www.yoda.arachsys.com/csharp/miscutil/).
However, the UtilPack is also extremely portable; the supported platforms are .NET 4, SL5, WPSL8, WPA8.1, and Win8.
Most of the documentation can be found in the XML documentation file included in the NuGet package, or by browsing source code directly.
Some things on TODO list:
* Various lazies (ReadOnlyLazy, SettableLazy, ResettableLazy)
* More tests.

# CILMerge utility
The CILMerge utility is similar to [ILMerge](http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx) and [ILRepack](https://github.com/gluck/il-repack): it merges multiple CIL assemblies and modules into one assembly.
It is used by many projects of CAM, and it itself utilizes CAM.Physical library to perform the merge.
This utility is available as standalone executable or as MSBuild task.

Many features of ILMerge are supported, and a few new ones are present.
There are still some features on TODO list:
* Win32 resources
* MDB support (is this relevant anymore?)
* Regex-based type renaming
* BAML fixing
* IKVM exports
