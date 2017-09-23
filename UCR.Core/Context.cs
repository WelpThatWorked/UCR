﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using IOWrapper;
using UCR.Core.Controllers;
using UCR.Core.Models.Device;
using UCR.Core.Models.Plugin;
using UCR.Core.Utilities;

namespace UCR.Core
{
    public class Context
    {
        private static string _contextName = "context.xml";
        private static string _pluginPath = "Plugins";

        // Persistence
        public List<Profile.Profile> Profiles { get; set; }
        public List<DeviceGroup> KeyboardGroups { get; set; }
        public List<DeviceGroup> MiceGroups { get; set; }
        public List<DeviceGroup> JoystickGroups { get; set; }
        public List<DeviceGroup> GenericDeviceGroups { get; set; }

        // Runtime
        [XmlIgnore]
        public bool IsNotSaved { get; private set; }
        [XmlIgnore]
        public Profile.Profile ActiveProfile { get; set; }
        [XmlIgnore]
        public IOController IOController { get; set; }
        [XmlIgnore]
        public ProfilesController ProfilesController { get; set; }
        [XmlIgnore]
        public DeviceGroupsController DeviceGroupsController { get; set; }
        
        internal List<Action> ActiveProfileCallbacks = new List<Action>();
        private PluginLoader PluginLoader;

        public Context()
        {
            Init();
        }

        private void Init()
        {
            IsNotSaved = false;
            Profiles = new List<Profile.Profile>();
            KeyboardGroups = new List<DeviceGroup>();
            MiceGroups = new List<DeviceGroup>();
            JoystickGroups = new List<DeviceGroup>();
            GenericDeviceGroups = new List<DeviceGroup>();

            IOController = new IOController();
            ProfilesController = new ProfilesController(this, Profiles);
            DeviceGroupsController = new DeviceGroupsController(this, JoystickGroups, KeyboardGroups, MiceGroups, GenericDeviceGroups);
            PluginLoader = new PluginLoader(_pluginPath);
        }

        public void SetActiveProfileCallback(Action profileActivated)
        {
            ActiveProfileCallbacks.Add(profileActivated);
        }

        public List<Plugin> GetPlugins()
        {
            return PluginLoader.Plugins;
        }

        public void ContextChanged()
        {
            IsNotSaved = true;
        }

        #region Persistence
        
        public bool SaveContext(List<Type> pluginTypes = null)
        {
            var serializer = GetXmlSerializer(pluginTypes);
            using (var streamWriter = new StreamWriter(_contextName))
            {
                serializer.Serialize(streamWriter, this);
            }
            IsNotSaved = false;
            return true;
        }

        public static Context Load(List<Type> pluginTypes = null)
        {
            Context context;
            var serializer = GetXmlSerializer(pluginTypes);
            try
            {
                using (var fileStream = new FileStream(_contextName, FileMode.Open))
                {
                    context = (Context) serializer.Deserialize(fileStream);
                    context.PostLoad();
                }
            }
            catch (IOException e)
            {
                Console.Write(e.ToString());
                // TODO log exception
                context = new Context();
            }
            return context;
        }

        private void PostLoad()
        {
            foreach (var profile in Profiles)
            {
                profile.PostLoad(this);
            }
        }

        private static XmlSerializer GetXmlSerializer(List<Type> additionalPluginTypes)
        {
            var plugins = new PluginLoader(_pluginPath);
            var pluginTypes = plugins.Plugins.Select(p => p.GetType()).ToList();
            if (additionalPluginTypes != null) pluginTypes.AddRange(additionalPluginTypes);
            return new XmlSerializer(typeof(Context), pluginTypes.ToArray());
        }

        #endregion
    }
}