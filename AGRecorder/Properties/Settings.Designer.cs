﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.0
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace RadioRecorder.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://agqr.jp/timetable/streaming.php")]
        public string 番组表 {
            get {
                return ((string)(this["番组表"]));
            }
            set {
                this["番组表"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Bangumi.xml")]
        public string BgmPath {
            get {
                return ((string)(this["BgmPath"]));
            }
            set {
                this["BgmPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Task.xml")]
        public string TaskPath {
            get {
                return ((string)(this["TaskPath"]));
            }
            set {
                this["TaskPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E:\\Radio")]
        public string 默认路径 {
            get {
                return ((string)(this["默认路径"]));
            }
            set {
                this["默认路径"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{0:yyyyMMdd}-{1}.flv")]
        public string 默认格式 {
            get {
                return ((string)(this["默认格式"]));
            }
            set {
                this["默认格式"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("rtmp://fms-base2.mitene.ad.jp/agqr/aandg11")]
        public string RTMP {
            get {
                return ((string)(this["RTMP"]));
            }
            set {
                this["RTMP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Cmd.xml")]
        public string CmdPath {
            get {
                return ((string)(this["CmdPath"]));
            }
            set {
                this["CmdPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("{0}-{1}-{2}.flv")]
        public string HBKFormat {
            get {
                return ((string)(this["HBKFormat"]));
            }
            set {
                this["HBKFormat"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("rtmpe://cp209391.edgefcs.net/ondemand/")]
        public string RTMPE {
            get {
                return ((string)(this["RTMPE"]));
            }
            set {
                this["RTMPE"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("650")]
        public double 窗体宽 {
            get {
                return ((double)(this["窗体宽"]));
            }
            set {
                this["窗体宽"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("460")]
        public double 窗体高 {
            get {
                return ((double)(this["窗体高"]));
            }
            set {
                this["窗体高"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("60,30")]
        public string 前后时间 {
            get {
                return ((string)(this["前后时间"]));
            }
            set {
                this["前后时间"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool 最小化到托盘 {
            get {
                return ((bool)(this["最小化到托盘"]));
            }
            set {
                this["最小化到托盘"] = value;
            }
        }
    }
}
