namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class PatchDefaultPackageSchemaModule : IModule
{
    private readonly PackageSchema _userPackageSchemaPatch;

    public PatchDefaultPackageSchemaModule(PackageSchema userPackageSchemaPatch)
    {
        _userPackageSchemaPatch = userPackageSchemaPatch;
    }

    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DefaultPackageSchema.Patch(_userPackageSchemaPatch);
    }
}