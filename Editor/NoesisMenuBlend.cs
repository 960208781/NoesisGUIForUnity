using UnityEngine;
using System.IO;
using System;
using UnityEditor;
using System.Linq;

public class NoesisMenuBlend
{
    [MenuItem("Tools/NoesisGUI/Open Blend Project", false, 30060)]
    [MenuItem("Assets/Open Blend project", false, 1000)]
    static void OpenBlendProject()
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string projectName = Path.GetFileName(projectPath);

        if (!File.Exists(Path.Combine(projectPath, projectName + "-blend.sln")))
        {
            CreateBlendProject(projectPath, projectName);
        }

        OpenBlendProject(projectPath, projectName);
    }

    static void OpenBlendProject(string projectPath, string projectName)
    {
        System.Diagnostics.Process.Start(Path.Combine(projectPath, projectName + "-blend.sln"));
    }

    static void CreateBlendProject(string projectPath, string projectName)
    {
        string solutionGuid = Guid.NewGuid().ToString().ToUpper();
        string projectGuid = Guid.NewGuid().ToString().ToUpper();

        CreateBlendSolution(projectPath, projectName, projectGuid, solutionGuid);
        CreateBlendProject(projectPath, projectName, projectGuid);
    }

    private static void CreateBlendSolution(string projectPath, string projectName, string projectGuid, string solutionGuid)
    {
        using (var writer = File.CreateText(Path.Combine(projectPath, projectName + "-blend.sln")))
        {
            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            writer.WriteLine("# Blend for Visual Studio Version 16");
            writer.WriteLine("VisualStudioVersion = 16.0.31729.503");
            writer.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
            writer.WriteLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{projectName}-blend\", \"{projectName}-blend.csproj\", \"{{{projectGuid}}}\"");
            writer.WriteLine("EndProject");
            writer.WriteLine("Global");
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            writer.WriteLine("\t\tDebug|Any CPU = Debug|Any CPU");
            writer.WriteLine("\t\tRelease|Any CPU = Release|Any CPU");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            writer.WriteLine($"\t\t{{{projectGuid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
            writer.WriteLine($"\t\t{{{projectGuid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
            writer.WriteLine($"\t\t{{{projectGuid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
            writer.WriteLine($"\t\t{{{projectGuid}}}.Release|Any CPU.Build.0 = Release|Any CPU");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ExtensibilityGlobals) = postSolution");
            writer.WriteLine($"\t\tSolutionGuid = {{{solutionGuid}}}");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("EndGlobal");
        }
    }

    static void CreateBlendProject(string projectPath, string projectName, string projectGuid)
    {
        string safeProjectName = Sanitize(projectName);

        using (var writer = File.CreateText(Path.Combine(projectPath, projectName + "-blend.csproj")))
        {
            writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            writer.WriteLine("<Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
            writer.WriteLine("  <PropertyGroup>");
            writer.WriteLine("    <BaseIntermediateOutputPath>Blend\\obj\\</BaseIntermediateOutputPath>");
            writer.WriteLine("    <OutputPath>Blend\\bin\\$(Configuration)\\</OutputPath>");
            writer.WriteLine("  </PropertyGroup>");
            writer.WriteLine("  <Import Project=\"$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props\" Condition=\"Exists('$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props')\" />");
            writer.WriteLine("  <PropertyGroup>");
            writer.WriteLine("    <Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>");
            writer.WriteLine("    <Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>");
            writer.WriteLine($"    <ProjectGuid>{{{projectGuid}}}</ProjectGuid>");
            writer.WriteLine("    <OutputType>WinExe</OutputType>");
            writer.WriteLine($"    <RootNamespace>{safeProjectName}</RootNamespace>");
            writer.WriteLine($"    <AssemblyName>{projectName}</AssemblyName>");
            writer.WriteLine("    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>");
            writer.WriteLine("    <FileAlignment>512</FileAlignment>");
            writer.WriteLine("    <ProjectTypeGuids>{60dc8134-eba5-43b8-bcc9-bb4bc16c2548};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}</ProjectTypeGuids>");
            writer.WriteLine("    <WarningLevel>4</WarningLevel>");
            writer.WriteLine("    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>");
            writer.WriteLine("    <Deterministic>true</Deterministic>");
            writer.WriteLine("  </PropertyGroup>");
            writer.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">");
            writer.WriteLine("    <PlatformTarget>AnyCPU</PlatformTarget>");
            writer.WriteLine("    <DebugSymbols>true</DebugSymbols>");
            writer.WriteLine("    <DebugType>full</DebugType>");
            writer.WriteLine("    <Optimize>false</Optimize>");
            writer.WriteLine("    <DefineConstants>DEBUG;TRACE</DefineConstants>");
            writer.WriteLine("    <ErrorReport>prompt</ErrorReport>");
            writer.WriteLine("    <WarningLevel>4</WarningLevel>");
            writer.WriteLine("  </PropertyGroup>");
            writer.WriteLine("  <PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' \">");
            writer.WriteLine("    <PlatformTarget>AnyCPU</PlatformTarget>");
            writer.WriteLine("    <DebugType>pdbonly</DebugType>");
            writer.WriteLine("    <Optimize>true</Optimize>");
            writer.WriteLine("    <DefineConstants>TRACE</DefineConstants>");
            writer.WriteLine("    <ErrorReport>prompt</ErrorReport>");
            writer.WriteLine("    <WarningLevel>4</WarningLevel>");
            writer.WriteLine("  </PropertyGroup>");
            writer.WriteLine("  <ItemGroup>");
            writer.WriteLine("    <Reference Include=\"System\" />");
            writer.WriteLine("    <Reference Include=\"System.Data\" />");
            writer.WriteLine("    <Reference Include=\"System.Xml\" />");
            writer.WriteLine("    <Reference Include=\"Microsoft.CSharp\" />");
            writer.WriteLine("    <Reference Include=\"System.Core\" />");
            writer.WriteLine("    <Reference Include=\"System.Xml.Linq\" />");
            writer.WriteLine("    <Reference Include=\"System.Data.DataSetExtensions\" />");
            writer.WriteLine("    <Reference Include=\"System.Net.Http\" />");
            writer.WriteLine("    <Reference Include=\"System.Xaml\">");
            writer.WriteLine("      <RequiredTargetFramework>4.0</RequiredTargetFramework>");
            writer.WriteLine("    </Reference>");
            writer.WriteLine("    <Reference Include=\"WindowsBase\" />");
            writer.WriteLine("    <Reference Include=\"PresentationCore\" />");
            writer.WriteLine("    <Reference Include=\"PresentationFramework\" />");
            writer.WriteLine("  </ItemGroup>");
            writer.WriteLine("  <ItemGroup>");
            writer.WriteLine("    <ApplicationDefinition Include=\"Blend\\App.xaml\">");
            writer.WriteLine("      <Generator>MSBuild:Compile</Generator>");
            writer.WriteLine("    </ApplicationDefinition>");
            writer.WriteLine("    <Compile Include=\"Blend\\App.xaml.cs\">");
            writer.WriteLine("      <DependentUpon>App.xaml</DependentUpon>");
            writer.WriteLine("      <SubType>Code</SubType>");
            writer.WriteLine("    </Compile>");
            writer.WriteLine($"    <Page Include=\"Assets\\{projectName}MainView.xaml\">");
            writer.WriteLine("      <Generator>MSBuild:Compile</Generator>");
            writer.WriteLine("      <SubType>Designer</SubType>");
            writer.WriteLine("    </Page>");
            writer.WriteLine($"    <Compile Include=\"Assets\\{projectName}MainView.xaml.cs\">");
            writer.WriteLine($"      <DependentUpon>{projectName}MainView.xaml</DependentUpon>");
            writer.WriteLine("      <SubType>Code</SubType>");
            writer.WriteLine("    </Compile>");
            writer.WriteLine("  </ItemGroup>");
            writer.WriteLine("  <ItemGroup>");
            writer.WriteLine("    <PackageReference Include=\"Noesis.GUI.Extensions\">");
            writer.WriteLine("      <Version>3.0.*</Version>");
            writer.WriteLine("    </PackageReference>");
            writer.WriteLine("  </ItemGroup>");
            writer.WriteLine("  <Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />");
            writer.WriteLine("</Project>");
        }

        Directory.CreateDirectory(Path.Combine(projectPath, "Blend"));

        using (var writer = File.CreateText(Path.Combine(projectPath, "Blend", "App.xaml")))
        {
            writer.WriteLine($"<Application x:Class=\"{safeProjectName}.App\"");
            writer.WriteLine("  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            writer.WriteLine("  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
            writer.WriteLine($"  xmlns:local=\"clr-namespace:{safeProjectName}\"");
            writer.WriteLine($"  StartupUri=\"/{projectName};component/Assets/{projectName}MainView.xaml\">");
            writer.WriteLine("  <Application.Resources>");
            writer.WriteLine("    <ResourceDictionary>");
            writer.WriteLine("      <ResourceDictionary.MergedDictionaries>");
            writer.WriteLine("        <ResourceDictionary Source=\"/Noesis.GUI.Extensions;component/Theme/NoesisTheme.DarkBlue.xaml\"/>");
            writer.WriteLine("      </ResourceDictionary.MergedDictionaries>");
            writer.WriteLine("    </ResourceDictionary>");
            writer.WriteLine("  </Application.Resources>");
            writer.WriteLine("</Application>");
        }

        using (var writer = File.CreateText(Path.Combine(projectPath, "Blend", "App.xaml.cs")))
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Windows;");
            writer.WriteLine("");
            writer.WriteLine($"namespace {safeProjectName}");
            writer.WriteLine("{");
            writer.WriteLine("    /// <summary>");
            writer.WriteLine("    /// Interaction logic for App.xaml");
            writer.WriteLine("    /// </summary>");
            writer.WriteLine("    public partial class App : Application");
            writer.WriteLine("    {");
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        using (var writer = File.CreateText(Path.Combine(projectPath, "Assets", $"{projectName}MainView.xaml")))
        {
            writer.WriteLine($"<UserControl x:Class=\"{safeProjectName}.{safeProjectName}MainView\"");
            writer.WriteLine("  xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
            writer.WriteLine("  xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"> ");
            writer.WriteLine("  <Border Background=\"DodgerBlue\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Padding=\"50,20\">");
            writer.WriteLine("    <StackPanel>");
            writer.WriteLine("      <TextBlock Text=\"Hello World\" FontSize=\"30\" Foreground=\"White\"/>");
            writer.WriteLine("      <Button Content=\"Button\" Margin=\"0,10,0,0\"/>");
            writer.WriteLine("    </StackPanel>");
            writer.WriteLine("  </Border>");
            writer.WriteLine("</UserControl>");
        }

        using (var writer = File.CreateText(Path.Combine(projectPath, "Assets", $"{projectName}MainView.xaml.cs")))
        {
            writer.WriteLine("#if UNITY_5_3_OR_NEWER");
            writer.WriteLine("#define NOESIS");
            writer.WriteLine("using Noesis;");
            writer.WriteLine("#else");
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Windows.Controls;");
            writer.WriteLine("#endif");
            writer.WriteLine("");
            writer.WriteLine($"namespace {safeProjectName}");
            writer.WriteLine("{");
            writer.WriteLine("    /// <summary>");
            writer.WriteLine($"    /// Interaction logic for {safeProjectName}MainView.xaml");
            writer.WriteLine("    /// </summary>");
            writer.WriteLine($"    public partial class {safeProjectName}MainView : UserControl");
            writer.WriteLine("    {");
            writer.WriteLine($"        public {safeProjectName}MainView()");
            writer.WriteLine("        {");
            writer.WriteLine("            InitializeComponent();");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("#if NOESIS");
            writer.WriteLine("        private void InitializeComponent()");
            writer.WriteLine("        {");
            writer.WriteLine("            NoesisUnity.LoadComponent(this);");
            writer.WriteLine("        }");
            writer.WriteLine("#endif");
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }
    }

    private static string Sanitize(string s)
    {
        // Uses '_' for invalid symbols
        return string.Join("", s.AsEnumerable().Select(c => char.IsLetter(c) || char.IsDigit(c) ? c.ToString() : "_" ));
    }
}