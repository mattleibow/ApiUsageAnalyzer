using Mono.Cecil;

namespace ApiUsageAnalyzer;

public class ApiAnalyzer
{
    public MissingSymbols GetMissingMembers(InputAssembly inputAssembly, InputAssembly dependencyAssembly)
    {
        List<string> missingTypes = [];
        List<string> missingMembers = [];

        using var module = ModuleDefinition.ReadModule(
            inputAssembly.FileName,
            new ReaderParameters
            {
                AssemblyResolver = new Resolver(inputAssembly.SearchPaths),
            });

        using var dependency = ModuleDefinition.ReadModule(
            dependencyAssembly.FileName,
            new ReaderParameters
            {
                AssemblyResolver = new Resolver(dependencyAssembly.SearchPaths),
            });
        var dependencyAssemblyName = dependency.Assembly.Name.Name;

        var typeRefs = module.GetTypeReferences();
        foreach (var typeRef in typeRefs)
        {
            var typeScope = typeRef.Scope;

            if (typeScope.Name != dependencyAssemblyName)
                continue;

            var resolved = dependency.MetadataResolver.Resolve(typeRef);
            if (resolved is null)
            {
                missingTypes.Add(typeRef.FullName);
            }
        }

        var memberRefs = module.GetMemberReferences();
        foreach (var memberRef in memberRefs)
        {
            var memberType = memberRef.DeclaringType;
            var memberScope = memberType.Scope;

            if (memberScope.Name != dependencyAssemblyName)
                continue;

            if (memberRef is MethodReference methodRef)
            {
                var resolved = dependency.MetadataResolver.Resolve(methodRef);
                if (resolved is null)
                {
                    missingMembers.Add(methodRef.FullName);
                }
            }
            else if (memberRef is FieldReference fieldRef)
            {
                var resolved = dependency.MetadataResolver.Resolve(fieldRef);
                if (resolved is null)
                {
                    missingMembers.Add(fieldRef.FullName);
                }
            }
        }

        return new MissingSymbols(missingTypes, missingMembers);
    }
}

public class InputAssembly
{
    public string? FileName { get; set; }

    public IList<string>? SearchPaths { get; set; } = [];
}

public class MissingSymbols
{
    public MissingSymbols(IReadOnlyCollection<string> types, IReadOnlyCollection<string> members)
    {
        Types = types;
        Members = members;
    }

    public IReadOnlyCollection<string> Types { get; }

    public IReadOnlyCollection<string> Members { get; }
}

class Resolver : DefaultAssemblyResolver
{
    public Resolver(IEnumerable<string>? searchPaths)
        : this()
    {
        if (searchPaths is not null)
        {
            foreach (var path in searchPaths)
            {
                if (path is not null)
                    AddSearchDirectory(path);
            }
        }
    }

    public Resolver()
    {
        RemoveSearchDirectory(".");
        RemoveSearchDirectory("bin");
    }
}
