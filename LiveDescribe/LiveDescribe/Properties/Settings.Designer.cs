﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LiveDescribe.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::LiveDescribe.Model.AudioSourceInfo Microphone {
            get {
                return ((global::LiveDescribe.Model.AudioSourceInfo)(this["Microphone"]));
            }
            set {
                this["Microphone"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string WorkingDirectory {
            get {
                return ((string)(this["WorkingDirectory"]));
            }
            set {
                this["WorkingDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::LiveDescribe.Model.ColourScheme ColourScheme {
            get {
                return ((global::LiveDescribe.Model.ColourScheme)(this["ColourScheme"]));
            }
            set {
                this["ColourScheme"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::LiveDescribe.Model.ObservableDropoutCollection<LiveDescribe.Model.NamedFilePath> RecentProjects {
            get {
                return ((global::LiveDescribe.Model.ObservableDropoutCollection<LiveDescribe.Model.NamedFilePath>)(this["RecentProjects"]));
            }
            set {
                this["RecentProjects"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool AutoGenerateSpaces {
            get {
                return ((bool)(this["AutoGenerateSpaces"]));
            }
            set {
                this["AutoGenerateSpaces"] = value;
            }
        }
    }
}
