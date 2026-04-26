//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using SolidWorks.Interop.sldworks;
using XCad.kit;
using XCad.kit.Diagnostics;
using XCad.kit.Windows;

namespace XCad.Sw {
    /// <summary>
    /// Factory for creating <see cref="ISwApplication"/>
    /// </summary>
    public class SwApplicationFactory {
        internal static class CommandLineArguments {
            /// <summary>
            /// Bypasses the Tools/Options settings
            /// </summary>
            public const string SafeMode = "/SWSafeMode /SWDisableExitApp";

            /// <summary>
            /// Runs SOLIDWORKS in background model via SOLIDWORKS Task Scheduler (requires SOLIDWORKS Professional or higher)
            /// </summary>
            public const string BackgroundMode = "/b";

            /// <summary>
            /// Suppresses all popup messages, including the splash screen
            /// </summary>
            public const string SilentMode = "/r";
        }

        internal const string PROG_ID_TEMPLATE = "SldWorks.Application.{0}";

        private const string ADDINS_STARTUP_REG_KEY = @"Software\SolidWorks\AddInsStartup";

        /// <summary>
        /// Disables all startup add-ins
        /// </summary>
        /// <param name="disabledAddInGuids">Guids of the disabled add-ins</param>
        /// <remarks>Call the <see cref="EnableAddInsStartup(IReadOnlyList{string})"/> to restore the add-ins</remarks>
        public static void DisableAllAddInsStartup(out IReadOnlyList<string> disabledAddInGuids) {
            const int DISABLE_VAL = 0;
            const int ENABLE_VAL = 1;

            var localDisabledAddInGuids = new List<string>();

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            if(addinsStartup != null) {
                var addInKeyNames = addinsStartup.GetSubKeyNames();

                if(addInKeyNames != null) {
                    foreach(var addInKeyName in addInKeyNames) {
                        var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);


                        if(int.TryParse(addInKey.GetValue("")?.ToString(), out int enableVal)) {
                            var loadOnStartup = enableVal == ENABLE_VAL;

                            if(loadOnStartup) {
                                addInKey.SetValue("", DISABLE_VAL);
                                localDisabledAddInGuids.Add(addInKeyName);
                            }
                        }
                    }
                }
            }

            disabledAddInGuids = localDisabledAddInGuids;
        }

        /// <summary>
        /// Enables the add-ins at startup
        /// </summary>
        /// <param name="addInGuids">Add-in guids</param>
        public static void EnableAddInsStartup(IReadOnlyList<string> addInGuids) {
            const int ENABLE_VAL = 1;

            var addinsStartup = Registry.CurrentUser.OpenSubKey(ADDINS_STARTUP_REG_KEY, true);

            foreach(var addInKeyName in addInGuids) {
                var addInKey = addinsStartup.OpenSubKey(addInKeyName, true);

                addInKey.SetValue("", ENABLE_VAL);
            }
        }

        /// <summary>
        /// Pre-creates a template for SOLIDWORKS application
        /// </summary>
        /// <returns></returns>
        public static ISwApplication PreCreate() => new SwApplication();

        /// <summary>
        /// Creates <see cref="ISwApplication"/> from SOLIDWORKS pointer
        /// </summary>
        /// <param name="app">Pointer to SOLIDWORKS application</param>
        /// <returns>Instance of <see cref="ISwApplication"/></returns>
        public static ISwApplication FromPointer(ISldWorks app)
            => FromPointer(app, new ServiceCollection());

        /// <inheritdoc cref="FromPointer(ISldWorks)"/>
        /// <param name="services">Custom serives</param>
        public static ISwApplication FromPointer(ISldWorks app, IXServiceCollection services)
            => new SwApplication(app, services);

        /// <summary>
        /// Creates instance of SOLIDWORKS from SLDWORKS.exe process
        /// </summary>
        /// <param name="process">SLDWORKS.exe process</param>
        /// <returns>Pointer to <see cref="ISwApplication"/></returns>
        public static ISwApplication FromProcess(Process process)
            => FromProcess(process, new ServiceCollection());

        /// <inheritdoc cref="FromProcess(Process)"/>
        /// <param name="services">Custom serives</param>
        public static ISwApplication FromProcess(Process process, IXServiceCollection services) {
            var app = RotHelper.TryGetComObjectByMonikerName<ISldWorks>(GetMonikerName(process), new TraceLogger("xCAD.SwApplication"));

            if(app != null) {
                return FromPointer(app, services);
            } else {
                throw new Exception($"Cannot access SOLIDWORKS application at process {process.Id}");
            }
        }

        internal static string GetMonikerName(Process process) => $"SolidWorks_PID_{process.Id}";

        internal static string FindSwPathFromRegKey(RegistryKey swAppRegKey) {
            var clsidKey = swAppRegKey.OpenSubKey("CLSID", false)
                ?? throw new NullReferenceException($"Incorrect registry value, CLSID is missing");

            var clsid = (string)clsidKey.GetValue("")
                ?? throw new NullReferenceException($"Incorrect registry value, LocalServer32 is missing");

            var localServerKey = Registry.ClassesRoot.OpenSubKey(
                $"CLSID\\{clsid}\\LocalServer32", false)
                ?? throw new Exception("Failed to find the class id in the registry. Make sure that application is running as x64 bit process (including 'Prefer 32-bit' option is unchecked in the project settings)");
            var swAppPath = (string)localServerKey.GetValue("");

            if(!File.Exists(swAppPath)) {
                throw new FileNotFoundException($"Path to SOLIDWORKS executable does not exist: {swAppPath}");
            }

            return swAppPath;
        }
    }
}