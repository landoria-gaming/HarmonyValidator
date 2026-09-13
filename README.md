# Harmony Validator

Harmony Validator checks Harmony patch targets during a .NET build. It reads
assembly metadata without loading the application or applying patches.

## Checks

| Check | Result |
| --- | --- |
| Target type and member exist | Error when missing |
| Overload selection is unique | Error when ambiguous |
| Patch parameters are compatible | Error when incompatible |
| Dynamic targets | Warning because they cannot be resolved statically |

## Requirements

- .NET 8 SDK
- A compiled assembly that uses Harmony
- The assembly reference paths used by the build

## Command line

```text
dotnet run --project HarmonyValidator.csproj -- \
  path/to/Assembly.dll path/to/references.txt
```

`references.txt` contains one assembly path per line.

## MSBuild

Import `HarmonyValidator.targets` from a project file:

```xml
<Import Project="path/to/HarmonyValidator.targets" />
```

The build fails when the validator reports an error. Unsupported or dynamic
targets produce warnings.

## Limits

- Runtime-generated targets cannot be verified.
- The validator checks metadata, not patch behavior.
- Reflection-based target selection may require manual review.

## License

MIT
