﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace EDGE.Controller.AuthService {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="BasicHttpBinding_AuthenticationService", Namespace="http://tempuri.org/")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ServiceResponse))]
    public partial class AuthenticationService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback LoginByUsernamePasswordOperationCompleted;
        
        private System.Threading.SendOrPostCallback LoginByAuthTokenOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public AuthenticationService() {
            this.Url = global::EDGE.Controller.Properties.Settings.Default.EDGE_Controller_AuthService_AuthenticationService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event LoginByUsernamePasswordCompletedEventHandler LoginByUsernamePasswordCompleted;
        
        /// <remarks/>
        public event LoginByAuthTokenCompletedEventHandler LoginByAuthTokenCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/AuthenticationService/LoginByUsernamePassword", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public ServiceResponseOfstring LoginByUsernamePassword([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string username, [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string password, int ttlSeconds, [System.Xml.Serialization.XmlIgnoreAttribute()] bool ttlSecondsSpecified) {
            object[] results = this.Invoke("LoginByUsernamePassword", new object[] {
                        username,
                        password,
                        ttlSeconds,
                        ttlSecondsSpecified});
            return ((ServiceResponseOfstring)(results[0]));
        }
        
        /// <remarks/>
        public void LoginByUsernamePasswordAsync(string username, string password, int ttlSeconds, bool ttlSecondsSpecified) {
            this.LoginByUsernamePasswordAsync(username, password, ttlSeconds, ttlSecondsSpecified, null);
        }
        
        /// <remarks/>
        public void LoginByUsernamePasswordAsync(string username, string password, int ttlSeconds, bool ttlSecondsSpecified, object userState) {
            if ((this.LoginByUsernamePasswordOperationCompleted == null)) {
                this.LoginByUsernamePasswordOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginByUsernamePasswordOperationCompleted);
            }
            this.InvokeAsync("LoginByUsernamePassword", new object[] {
                        username,
                        password,
                        ttlSeconds,
                        ttlSecondsSpecified}, this.LoginByUsernamePasswordOperationCompleted, userState);
        }
        
        private void OnLoginByUsernamePasswordOperationCompleted(object arg) {
            if ((this.LoginByUsernamePasswordCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginByUsernamePasswordCompleted(this, new LoginByUsernamePasswordCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/AuthenticationService/LoginByAuthToken", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public ServiceResponseOfstring LoginByAuthToken([System.Xml.Serialization.XmlElementAttribute(IsNullable=true)] string auth, int ttlSeconds, [System.Xml.Serialization.XmlIgnoreAttribute()] bool ttlSecondsSpecified) {
            object[] results = this.Invoke("LoginByAuthToken", new object[] {
                        auth,
                        ttlSeconds,
                        ttlSecondsSpecified});
            return ((ServiceResponseOfstring)(results[0]));
        }
        
        /// <remarks/>
        public void LoginByAuthTokenAsync(string auth, int ttlSeconds, bool ttlSecondsSpecified) {
            this.LoginByAuthTokenAsync(auth, ttlSeconds, ttlSecondsSpecified, null);
        }
        
        /// <remarks/>
        public void LoginByAuthTokenAsync(string auth, int ttlSeconds, bool ttlSecondsSpecified, object userState) {
            if ((this.LoginByAuthTokenOperationCompleted == null)) {
                this.LoginByAuthTokenOperationCompleted = new System.Threading.SendOrPostCallback(this.OnLoginByAuthTokenOperationCompleted);
            }
            this.InvokeAsync("LoginByAuthToken", new object[] {
                        auth,
                        ttlSeconds,
                        ttlSecondsSpecified}, this.LoginByAuthTokenOperationCompleted, userState);
        }
        
        private void OnLoginByAuthTokenOperationCompleted(object arg) {
            if ((this.LoginByAuthTokenCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.LoginByAuthTokenCompleted(this, new LoginByAuthTokenCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ServiceResponseOfstring : ServiceResponse {
        
        private string returnValueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string ReturnValue {
            get {
                return this.returnValueField;
            }
            set {
                this.returnValueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ServiceResponseOfstring))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ServiceResponse {
        
        private ExecMsg[] execMsgsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable=true)]
        public ExecMsg[] ExecMsgs {
            get {
                return this.execMsgsField;
            }
            set {
                this.execMsgsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ExecMsg {
        
        private string messageField;
        
        private int messageIDField;
        
        private bool messageIDFieldSpecified;
        
        private ExecMsgSeverity severityField;
        
        private bool severityFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public string Message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
        
        /// <remarks/>
        public int MessageID {
            get {
                return this.messageIDField;
            }
            set {
                this.messageIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MessageIDSpecified {
            get {
                return this.messageIDFieldSpecified;
            }
            set {
                this.messageIDFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        public ExecMsgSeverity Severity {
            get {
                return this.severityField;
            }
            set {
                this.severityField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SeveritySpecified {
            get {
                return this.severityFieldSpecified;
            }
            set {
                this.severityFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.datacontract.org/2004/07/DataManagement.Framework.Common")]
    public enum ExecMsgSeverity {
        
        /// <remarks/>
        Error,
        
        /// <remarks/>
        Problem,
        
        /// <remarks/>
        Warning,
        
        /// <remarks/>
        Question,
        
        /// <remarks/>
        Info,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void LoginByUsernamePasswordCompletedEventHandler(object sender, LoginByUsernamePasswordCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class LoginByUsernamePasswordCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal LoginByUsernamePasswordCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ServiceResponseOfstring Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ServiceResponseOfstring)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void LoginByAuthTokenCompletedEventHandler(object sender, LoginByAuthTokenCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class LoginByAuthTokenCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal LoginByAuthTokenCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ServiceResponseOfstring Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ServiceResponseOfstring)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591