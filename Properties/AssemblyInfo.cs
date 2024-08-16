using MelonLoader;
using System.Reflection;
using System.Runtime.InteropServices;
using TrackingRotator;

[assembly: ComVisible(false)]
[assembly: Guid("652febb3-e7bd-4f5c-8ac4-51e36b19bc9d")]
[assembly: AssemblyTitle(ModBuildInfo.Name)]
[assembly: AssemblyProduct(ModBuildInfo.Name)]
[assembly: AssemblyCopyright("Created by " + ModBuildInfo.Author)]
[assembly: AssemblyVersion(ModBuildInfo.Version)]
[assembly: AssemblyFileVersion(ModBuildInfo.Version)]
[assembly: MelonInfo(typeof(TrackingRotatorMod), ModBuildInfo.Name, ModBuildInfo.Version, ModBuildInfo.Author, ModBuildInfo.DownloadLink)]
[assembly: MelonGame(ModBuildInfo.GameDeveloper, ModBuildInfo.Game)]
[assembly: MelonOptionalDependencies("UIExpansionKit", "ActionMenuApi")]