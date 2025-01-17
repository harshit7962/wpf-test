﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Test.CompilerServices.Logging;
using Microsoft.Test.Logging;

namespace Microsoft.Test.CompilerServices
{
    /// <summary>
    /// Class containing helper methods for compilation testing
    /// </summary>
    public class CompilationHelper
    {
        private ErrorWarningStore expected;
        private ErrorWarningStore unexpected;
        private ErrorWarningStore unencountered;
        private string binaryPath;

        private bool compilationResult = false;

        public CompilationHelper()
        {
            expected = new ErrorWarningStore();
            unexpected = new ErrorWarningStore();
            unencountered = new ErrorWarningStore();
        }

        #region Public Properties

        /// <summary>
        /// Actual result returned from MSBuild
        /// </summary>
        public bool ActualCompilationResult
        {
            get { return compilationResult; }
        }

        public ErrorWarningStore Unexpected
        {
            get { return unexpected; }
        }

        public ErrorWarningStore Unencountered
        {
            get { return unencountered; }
        }

        public string BinaryPath
        {
            get { return binaryPath; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compiles the project and evaluates the result based on the provided lists of expected errors and warnings
        /// Sets the property ActualCompilationResult to the value returned by MSBuild
        /// </summary>
        /// <returns></returns>
        public bool CompileAndEvaluateResult(string projectFileName, List<BuildError> expectedBuildErrors, List<BuildWarning> expectedBuildWarnings, bool checkWarnings)
        {
            return CompileAndEvaluateResult(projectFileName, string.Empty, expectedBuildErrors, expectedBuildWarnings, checkWarnings);
        }

        /// <summary>
        /// Compiles the project and evaluates the result based on the provided lists of expected errors and warnings
        /// Sets the property ActualCompilationResult to the value returned by MSBuild
        /// </summary>
        /// <returns></returns>
        public bool CompileAndEvaluateResult(string projectFileName, string targetFrameworkVersion, List<BuildError> expectedBuildErrors, List<BuildWarning> expectedBuildWarnings, bool checkWarnings)
        {
            if (String.IsNullOrEmpty(projectFileName))
            {
                throw new ArgumentNullException("projectFileName");
            }
            ResetState(expectedBuildErrors, expectedBuildWarnings);

            BuildLogger logger = new BuildLogger();
            Compiler compiler = new Compiler();
            compilationResult = compiler.CompileProject(projectFileName, logger, targetFrameworkVersion);
            binaryPath = compiler.BinDir + compiler.AssemblyName;
            bool result = EvaluateResult(logger, checkWarnings);
            if (!result)
            {
                SaveLogFiles(logger);
            }
            return result;
        }

        /// <summary>
        /// Evaluates whether the desired compilation result occured based on
        /// the expected errors and warnings
        /// </summary>
        public bool EvaluateResult(BuildLogger logger, List<BuildError> expectedBuildErrors, List<BuildWarning> expectedBuildWarnings, bool checkWarnings)
        {
            ResetState(expectedBuildErrors, expectedBuildWarnings);
            return EvaluateResult(logger, checkWarnings);
        }

        /// <summary>
        /// Saves log files using the GlobalLog.LogFile method
        /// </summary>
        public void SaveLogFiles(BuildLogger logger)
        {
            GlobalLog.LogFile(logger.BuildLogPath);
        }

        /// <summary>
        /// Add system.xaml to the reference if building for 4.0
        /// </summary>
        /// <param name="projectFile">project file name</param>
        /// <returns>tools version to use</returns>
        public static string AddSystemXamlReference(string projectFile)
        {
            GlobalLog.LogDebug("Adding System.Xaml Reference to project: " + projectFile);

            // If the provided project file is not a csproj,
            // vbproj or proj file, dont update the file.
            if (!projectFile.Contains(".csproj") &&
                !projectFile.Contains(".vbproj") &&
                !projectFile.Contains(".proj"))
            {
                GlobalLog.LogDebug("Project file is not csproj, vbproj or proj");
                return string.Empty;
            }

            // Infer the tools version available. If it is 4.0,
            // find if System.Xaml, WindowsBase are present in
            // the references section.
            string toolsVersion = InferToolsVersion();
            if (toolsVersion.Equals("4.0"))
            {
                Project project = new Project(projectFile, null, toolsVersion);

                bool systemXamlReferenced = false;
                bool referenceRequired = false;
                foreach (ProjectItem item in project.Items)
                {
                    if (item.ItemType.Equals("Reference", StringComparison.InvariantCultureIgnoreCase) &&
                        item.EvaluatedInclude.Equals("System.Xaml", StringComparison.InvariantCultureIgnoreCase))
                    {
                        systemXamlReferenced = true;
                    }

                    if (item.ItemType.Equals("Reference", StringComparison.InvariantCultureIgnoreCase) &&
                        item.EvaluatedInclude.Equals("WindowsBase", StringComparison.InvariantCultureIgnoreCase))
                    {
                        referenceRequired = true;
                    }
                }

                // If WindowsBase is present in the references but System.Xaml is not
                // add System.Xaml to the references.
                if (referenceRequired == true && systemXamlReferenced == false)
                {
                    // Add System.Xaml reference to the current project //
                    project.AddItem("Reference", "System.Xaml");
                }

                project.Save(projectFile);

                // Find out all referenced projects in the current
                // project and recursively update them as well.
                // Do this by looking at the DefaultProjects property on the Project,
                // Find the target sections with the appropriate name, find the msbuild
                // task under that target and get the Projects propety. This property
                // will have the name of the project file. Phew - so much code to just
                // find the referenced projects !
                string[] defaultTargets = project.Xml.DefaultTargets.Split(';');
                if (defaultTargets.Length != 0)
                {
                    foreach (string defaultTarget in defaultTargets)
                    {
                        // If the target is build or clean, ignore them
                        if (defaultTarget.Equals("Build", StringComparison.InvariantCultureIgnoreCase) ||
                            defaultTarget.Equals("Clean", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        string projectName = defaultTarget.Trim();
                        if (string.IsNullOrEmpty(projectName))
                        {
                            continue;
                        }
                        GlobalLog.LogDebug("Processing Target: " + projectName);

                        foreach (ProjectTargetInstance target in project.Targets.Values)
                        {
                            if (target.Name.Equals(projectName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (ProjectTaskInstance task in target.Tasks)
                                {
                                    if (task.Name.Equals("msbuild", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        string[] refProjectFiles = task.Parameters["Projects"].Split(';');
                                        foreach (string refProjectFile in refProjectFiles)
                                        {
                                            GlobalLog.LogDebug("Adding System.Xaml reference to referenced project - " + refProjectFile);
                                            AddSystemXamlReference(refProjectFile);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ProjectCollection.GlobalProjectCollection.UnloadProject(project);
            }

            return toolsVersion;
        }

        /// <summary>
        /// Infers supported ToolsVersion based on ProjectCollection.Version
        /// </summary>
        /// <returns>Supported ToolsVersion</returns>
        public static string InferToolsVersion()
        {
            GlobalLog.LogDebug("ProjectCollection.Version: " + ProjectCollection.Version);
            if (ProjectCollection.Version.Major == 3 && ProjectCollection.Version.Minor == 5)
            {
                return "3.5";
            }
            else if (ProjectCollection.Version.Major == 4 && ProjectCollection.Version.Minor == 0)
            {
                return "4.0";
            }
            else if (ProjectCollection.Version.Major == 4 && ProjectCollection.Version.Minor == 5)
            {
                return "4.0";   // Build.Engine 4.5.xxx still only supports ToolsVersion=4.0
            }
            else if (ProjectCollection.Version.Major == 4 && ProjectCollection.Version.Minor == 6)
            {
                return "4.0";   // Build.Engine 4.6.xxx still only supports ToolsVersion=4.0
            }
            else if (ProjectCollection.Version.Major == 4 && ProjectCollection.Version.Minor == 7)
            {
                return "4.0";   // Build.Engine 4.7.xxx still only supports ToolsVersion=4.0
            }
            else if (ProjectCollection.Version.Major == 4 && ProjectCollection.Version.Minor == 8)
            {
                return "4.0";   // Build.Engine 4.8.xxx still only supports ToolsVersion=4.0
            }
            else
            {
                // .NET Core 3.0 : We should be using Current for toolset version.
                return "Current";
            }
        }

        /// <summary>
        /// Infers PlatformTarget based on TestRuntime's PlatformTarget
        /// </summary>
        /// <returns>PlatformTarget</returns>
        internal static string InferPlatformTarget()
        {
            PortableExecutableKinds peKinds;
            ImageFileMachine machine;

            // Call Module.GetPEKind on TestRuntime
            typeof(Microsoft.Test.Logging.GlobalLog).Assembly.ManifestModule.GetPEKind(out peKinds, out machine);

            // If 'machine' has ImageFileMachine.AMD64 flag set - the assembly was built with PlatformTarget=x64
            // If 'machine' has ImageFileMachine.IA64 flag set - the assembly was built with PlatformTarget=Itanium
            // If 'machine' has ImageFileMachine.I386 flag set
            //     If 'peKinds' has PortableExecutableKinds.Required32Bit flag set - assembly was built with PlatformTarget=x86
            //     Else the assembly was built with PlatformTarget=AnyCPU

            if (peKinds.HasFlag(PortableExecutableKinds.Required32Bit))
            {
                // This means we are in a WoW test run
                return "x86";
            }
            else
            {
                // We don't differentiate for x64 and Itanium, as AnyCPU will just work fine 

                return "AnyCPU";
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Evaluates whether the desired compilation result occured based on
        /// the expected errors and warnings
        /// </summary>
        /// <param name="logger">the logger to use in the evalutation</param>
        /// <param name="checkWarnings">whether the warnings should be considered in the result</param>
        /// <returns></returns>
        private bool EvaluateResult(BuildLogger logger, bool checkWarnings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            bool result = true;

            EvaluateErrors(logger);
            EvaluateWarnings(logger);

            if (unexpected.Errors.Count != 0 || unencountered.Errors.Count != 0)
            {
                result &= false;
            }

            if (checkWarnings)
            {
                if (unexpected.Warnings.Count != 0 || unencountered.Warnings.Count != 0)
                {
                    result &= false;
                }
            }
            return result;
        }

        private void EvaluateErrors(BuildLogger logger)
        {
            //Check each encountered error to see if we expected it
            foreach (BuildStatus error in logger.BuildErrors)
            {
                if (!expected.Errors.Contains(error))
                {
                    unexpected.Errors.Add(error);
                }
            }

            //check each expected error to see if it was encountered
            foreach (BuildStatus error in expected.Errors)
            {
                if (!logger.BuildErrors.Contains(error))
                {
                    unencountered.Errors.Add(error);
                }
            }
        }

        private void EvaluateWarnings(BuildLogger logger)
        {
            //Check each encountered warning to see if we expected it
            foreach (BuildStatus warning in logger.BuildWarnings)
            {
                if (!expected.Warnings.Contains(warning))
                {
                    unexpected.Warnings.Add(warning);
                }
            }

            //check each expected warning to see if it was encontered
            foreach (BuildStatus warning in expected.Warnings)
            {
                if (!logger.BuildWarnings.Contains(warning))
                {
                    unencountered.Warnings.Add(warning);
                }
            }
        }

        private void ResetState(List<BuildError> errors, List<BuildWarning> warnings)
        {
            unexpected.Clear();
            unencountered.Clear();
            expected = new ErrorWarningStore(errors, warnings);
        }

        #endregion
    }
}
