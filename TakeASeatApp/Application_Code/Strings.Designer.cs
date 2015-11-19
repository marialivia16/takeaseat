﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace TakeASeatApp.Utils
{
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [DebuggerNonUserCode()]
    [CompilerGenerated()]
    public class Strings {
        
        private static ResourceManager resourceMan;
        
        private static CultureInfo resourceCulture;
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static ResourceManager ResourceManager {
            get {
                if (ReferenceEquals(resourceMan, null)) {
                    ResourceManager temp = new ResourceManager("TakeASeatApp.Application_Code.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Danger.
        /// </summary>
        public static string AppMessage_Danger {
            get {
                return ResourceManager.GetString("AppMessage_Danger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error.
        /// </summary>
        public static string AppMessage_Error {
            get {
                return ResourceManager.GetString("AppMessage_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Info.
        /// </summary>
        public static string AppMessage_Info {
            get {
                return ResourceManager.GetString("AppMessage_Info", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Success.
        /// </summary>
        public static string AppMessage_Success {
            get {
                return ResourceManager.GetString("AppMessage_Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning.
        /// </summary>
        public static string AppMessage_Warning {
            get {
                return ResourceManager.GetString("AppMessage_Warning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An automatic e-mail notification could not be successfully sent. We apologise for the inconvenience. The system error message is:.
        /// </summary>
        public static string EmailHelper_SendEmailMessage_Failed {
            get {
                return ResourceManager.GetString("EmailHelper_SendEmailMessage_Failed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [= No Subject Line =].
        /// </summary>
        public static string EmailHelper_SendEmailMessage_NoSubject {
            get {
                return ResourceManager.GetString("EmailHelper_SendEmailMessage_NoSubject", resourceCulture);
            }
        }
    }
}