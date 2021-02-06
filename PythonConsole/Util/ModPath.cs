using ColossalFramework.Plugins;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ColossalFramework.Plugins.PluginManager;

namespace PythonConsole
{
    public class ModPath
    {
        private static ModPath _instance;
        public static ModPath Instsance
        {
            get
            {
                if (_instance == null)
                    _instance = new ModPath();
                return _instance;
            }
        }

        private PluginInfo _pluginInfo;
        public string AssemblyPath => _pluginInfo.modPath;

        private ModPath()
        {
            var pluginManager = PluginManager.instance;
            var plugins = pluginManager.GetPluginsInfo();

            foreach (var item in plugins)
            {
                try
                {
                    var instances = item.GetInstances<IUserMod>();
                    if (!(instances.FirstOrDefault() is ModInfo))
                        continue;

                    _pluginInfo = item;
                    return;
                }
                catch
                {

                }
            }
            throw new Exception($"Failed to find assembly!");
        }
    }
}
