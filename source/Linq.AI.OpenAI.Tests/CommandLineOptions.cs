namespace Linq.AI.OpenAI.Tests
{
    public class CommandLineOptions
    {
        [System.ComponentModel.Description("The project or solution file to operate on. If a file is not specified, the command will search the current directory for one.")]
        public string? ProjectOrSolution { get; set; }

        [System.ComponentModel.Description("Use current runtime as the target runtime.")]
        public bool? UseCurrentRuntime { get; set; }

        [System.ComponentModel.Description("The target framework to build for. The target framework must also be specified in the project file.")]
        public string? Framework { get; set; }

        [System.ComponentModel.Description("The configuration to use for building the project. The default for most projects is 'Debug'.")]
        public string? Configuration { get; set; }

        [System.ComponentModel.Description("The target runtime to build for.")]
        public string? RuntimeIdentifier { get; set; }

        [System.ComponentModel.Description("Set the value of the $(VersionSuffix) property to use when building the project.")]
        public string? VersionSuffix { get; set; }

        [System.ComponentModel.Description("Do not restore the project before building.")]
        public bool? NoRestore { get; set; }

        [System.ComponentModel.Description("Allows the command to stop and wait for user input or action (for example to complete authentication).")]
        public bool? Interactive { get; set; }

        [System.ComponentModel.Description("Set the MSBuild verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic].")]
        public string? Verbosity { get; set; }

        [System.ComponentModel.Description("Include debugging output.")]
        public bool? Debug { get; set; }

        [System.ComponentModel.Description("The output directory to place built artifacts in.")]
        public string? OutputDirectory { get; set; }

        [System.ComponentModel.Description("The artifacts path. All output from the project, including build, publish, and pack output, will go in subfolders under the specified path.")]
        public string? ArtifactsPath { get; set; }

        [System.ComponentModel.Description("Do not use incremental building.")]
        public bool? NoIncremental { get; set; }

        [System.ComponentModel.Description("Do not build project-to-project references and only build the specified project.")]
        public bool? NoDependencies { get; set; }

        [System.ComponentModel.Description("Do not display the startup banner or the copyright message.")]
        public bool? NoLogo { get; set; }

        [System.ComponentModel.Description("Publish the .NET runtime with your application so the runtime doesn't need to be installed on the target machine.")]
        public bool? SelfContained { get; set; }

        [System.ComponentModel.Description("Publish your application as a framework dependent application. A compatible .NET runtime must be installed on the target machine to run your application.")]
        public bool? NoSelfContained { get; set; }

        [System.ComponentModel.Description("The target architecture like x86|x64|Arm32|Arm46 etc.")]
        public string? Architecture { get; set; }

        [System.ComponentModel.Description("The target operating system.")]
        public string? OperatingSystem { get; set; }

        [System.ComponentModel.Description("Force the command to ignore any persistent build servers.")]
        public bool? DisableBuildServers { get; set; }

        [System.ComponentModel.Description("Show command line help.")]
        public bool? Help { get; set; }
    }

}