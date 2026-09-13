using Mono.Cecil;
using Microsoft.Build.Framework;

namespace Landoria.Build;

// Validates Harmony patches directly inside the current MSBuild process.
public sealed class HarmonyValidatorTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public string AssemblyPath { get; set; } = string.Empty;

    [Required]
    public ITaskItem[] References { get; set; } = [];

    public override bool Execute()
    {
        try
        {
            return Validate();
        }
        catch (Exception error)
        {
            Log.LogErrorFromException(error, true);
            return false;
        }
    }

    private bool Validate()
    {
        using var resolver = new DefaultAssemblyResolver();
        foreach (var directory in References.Select(reference => reference.ItemSpec)
            .Append(AssemblyPath).Select(Path.GetDirectoryName).Distinct())
        {
            if (!string.IsNullOrEmpty(directory))
            {
                resolver.AddSearchDirectory(directory);
            }
        }

        using var assembly = AssemblyDefinition.ReadAssembly(AssemblyPath,
            new ReaderParameters { AssemblyResolver = resolver });
        var validator = new Validator(Report);
        validator.Check(assembly.MainModule.Types);
        Log.LogMessage(MessageImportance.High,
            $"Harmony targets: {validator.Checked} checked, "
            + $"{validator.Errors} errors, {validator.Warnings} unverified.");
        return validator.Errors == 0;
    }

    private void Report(MethodDefinition patch, string code, string message,
        bool warning)
    {
        string detail = $"{patch.DeclaringType.FullName}.{patch.Name}: {message}";
        if (warning)
        {
            Log.LogWarning(null, code, null, AssemblyPath, 0, 0, 0, 0, detail);
        }
        else
        {
            Log.LogError(null, code, null, AssemblyPath, 0, 0, 0, 0, detail);
        }
    }
}
