﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WeeMasGameFilter.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF32CD32")]
        public global::System.Windows.Media.Color AutoConsoleMatchColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AutoConsoleMatchColor"]));
            }
            set {
                this["AutoConsoleMatchColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF90EE90")]
        public global::System.Windows.Media.Color AutoMatchColor {
            get {
                return ((global::System.Windows.Media.Color)(this["AutoMatchColor"]));
            }
            set {
                this["AutoMatchColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF00BFFF")]
        public global::System.Windows.Media.Color ManualConsoleMatchColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ManualConsoleMatchColor"]));
            }
            set {
                this["ManualConsoleMatchColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("#FF87CEFA")]
        public global::System.Windows.Media.Color ManualMatchColor {
            get {
                return ((global::System.Windows.Media.Color)(this["ManualMatchColor"]));
            }
            set {
                this["ManualMatchColor"] = value;
            }
        }
    }
}
