﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EDGE.Business.EdgeAuthService {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServiceResponse", Namespace="")]
    [System.SerializableAttribute()]
    [System.Runtime.Serialization.KnownTypeAttribute(typeof(EDGE.Business.EdgeAuthService.ServiceResponseOfstring))]
    public partial class ServiceResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private EDGE.Business.EdgeAuthService.ExecMsg[] ExecMsgsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public EDGE.Business.EdgeAuthService.ExecMsg[] ExecMsgs {
            get {
                return this.ExecMsgsField;
            }
            set {
                if ((object.ReferenceEquals(this.ExecMsgsField, value) != true)) {
                    this.ExecMsgsField = value;
                    this.RaisePropertyChanged("ExecMsgs");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ServiceResponseOfstring", Namespace="")]
    [System.SerializableAttribute()]
    public partial class ServiceResponseOfstring : EDGE.Business.EdgeAuthService.ServiceResponse {
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ReturnValueField;
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ReturnValue {
            get {
                return this.ReturnValueField;
            }
            set {
                if ((object.ReferenceEquals(this.ReturnValueField, value) != true)) {
                    this.ReturnValueField = value;
                    this.RaisePropertyChanged("ReturnValue");
                }
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExecMsg", Namespace="")]
    [System.SerializableAttribute()]
    public partial class ExecMsg : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int MessageIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private EDGE.Business.EdgeAuthService.ExecMsgSeverity SeverityField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Message {
            get {
                return this.MessageField;
            }
            set {
                if ((object.ReferenceEquals(this.MessageField, value) != true)) {
                    this.MessageField = value;
                    this.RaisePropertyChanged("Message");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public int MessageID {
            get {
                return this.MessageIDField;
            }
            set {
                if ((this.MessageIDField.Equals(value) != true)) {
                    this.MessageIDField = value;
                    this.RaisePropertyChanged("MessageID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public EDGE.Business.EdgeAuthService.ExecMsgSeverity Severity {
            get {
                return this.SeverityField;
            }
            set {
                if ((this.SeverityField.Equals(value) != true)) {
                    this.SeverityField = value;
                    this.RaisePropertyChanged("Severity");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExecMsgSeverity", Namespace="http://schemas.datacontract.org/2004/07/DataManagement.Framework.Common")]
    public enum ExecMsgSeverity : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Error = 1000,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Problem = 500,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Warning = 100,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Question = 20,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Info = 10,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="EdgeAuthService.AuthenticationService")]
    public interface AuthenticationService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AuthenticationService/LoginByUsernamePassword", ReplyAction="http://tempuri.org/AuthenticationService/LoginByUsernamePasswordResponse")]
        EDGE.Business.EdgeAuthService.ServiceResponseOfstring LoginByUsernamePassword(string username, string password, int ttlSeconds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AuthenticationService/LoginByUsernamePassword", ReplyAction="http://tempuri.org/AuthenticationService/LoginByUsernamePasswordResponse")]
        System.Threading.Tasks.Task<EDGE.Business.EdgeAuthService.ServiceResponseOfstring> LoginByUsernamePasswordAsync(string username, string password, int ttlSeconds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AuthenticationService/LoginByAuthToken", ReplyAction="http://tempuri.org/AuthenticationService/LoginByAuthTokenResponse")]
        EDGE.Business.EdgeAuthService.ServiceResponseOfstring LoginByAuthToken(string auth, int ttlSeconds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/AuthenticationService/LoginByAuthToken", ReplyAction="http://tempuri.org/AuthenticationService/LoginByAuthTokenResponse")]
        System.Threading.Tasks.Task<EDGE.Business.EdgeAuthService.ServiceResponseOfstring> LoginByAuthTokenAsync(string auth, int ttlSeconds);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface AuthenticationServiceChannel : EDGE.Business.EdgeAuthService.AuthenticationService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AuthenticationServiceClient : System.ServiceModel.ClientBase<EDGE.Business.EdgeAuthService.AuthenticationService>, EDGE.Business.EdgeAuthService.AuthenticationService {
        
        public AuthenticationServiceClient() {
        }
        
        public AuthenticationServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public AuthenticationServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AuthenticationServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AuthenticationServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public EDGE.Business.EdgeAuthService.ServiceResponseOfstring LoginByUsernamePassword(string username, string password, int ttlSeconds) {
            return base.Channel.LoginByUsernamePassword(username, password, ttlSeconds);
        }
        
        public System.Threading.Tasks.Task<EDGE.Business.EdgeAuthService.ServiceResponseOfstring> LoginByUsernamePasswordAsync(string username, string password, int ttlSeconds) {
            return base.Channel.LoginByUsernamePasswordAsync(username, password, ttlSeconds);
        }
        
        public EDGE.Business.EdgeAuthService.ServiceResponseOfstring LoginByAuthToken(string auth, int ttlSeconds) {
            return base.Channel.LoginByAuthToken(auth, ttlSeconds);
        }
        
        public System.Threading.Tasks.Task<EDGE.Business.EdgeAuthService.ServiceResponseOfstring> LoginByAuthTokenAsync(string auth, int ttlSeconds) {
            return base.Channel.LoginByAuthTokenAsync(auth, ttlSeconds);
        }
    }
}