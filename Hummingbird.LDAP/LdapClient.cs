
using Hummingbird.TestFramework.Messaging;
using Hummingbird.TestFramework.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Hummingbird.TestFramework.Services
{
    public sealed class LdapClient : AbstractClient
    {
        internal readonly static Queue RequestQueue = new Queue();
        static private int threadCount = 20;
        static private int queueLength = 500;
        static internal string ServerAddress { get; private set; }
        static internal int SizeLimit { get; private set; } = 30;

        private static string PARAM_LDAP_SERVER = "LDAP Server address";
        private static string PARAM_LDAP_MAX_RESULT = "Max result count";

        public LdapClient()
        {
            this.Name = "LDAP Client";
            this.Id = new Guid("7c616fe4-e025-498f-9e9b-5698eac92d28");
            this.IsRunning = false;
            this.Information = "LDAP Client handler";
            this.Description = Information;
            Parameters.Add(PARAM_LDAP_SERVER, new Parameter() { Name = PARAM_LDAP_SERVER,  DefaultValue = "", Description = "The LDAP server address, ex ldap://serveraddress:port. Empty string means the currect Windows Active Directory", ParameterType = ParameterType.String });
            Parameters.Add(PARAM_LDAP_MAX_RESULT, new Parameter() { Name = PARAM_LDAP_MAX_RESULT, DefaultValue = "30", Description = "The maximum returned result in a query. The value 0 means unlimited (may cause performance issues then query is badly written)", ParameterType = ParameterType.Integer });

            this.SupportedRequests.Add(new AbstractMetadata()
            {
                ApplicationName = "Generic",
                ServiceCategory = "Generic",
                ServiceName = "LDAP Query",
                Description = "Send LDAP query and get the result",
                Id = new Guid("cbeb35ed-2880-4489-86f6-9ef58acf9780"),
                ReferencedService = this,
                RequestEditorType = typeof(ViewLdapEditor),
                RequestType = typeof(string),
                ResponseType = typeof(string),
            });

       }

        public override void Start()
        {
            LdapClientHandler.StopAllRequested = false;
            for (int i = 0; i < threadCount; i++)
            {
                LdapClientHandler.StartNewHandler();
            }
        }

        public override void Stop()
        {
            LdapClientHandler.StopAll();
        }

        public override void ApplySettings(IEnumerable<Parameter> appliedParameters)
        {
            base.ApplySettings(appliedParameters);
            ServerAddress = Parameters[PARAM_LDAP_SERVER].Value;
            SizeLimit = int.Parse(Parameters[PARAM_LDAP_MAX_RESULT].Value);
        }

        protected override void SendRequest(RequestData requestData)
        {
            Message message = requestData.ReferencedMessage;
            message.Status = MessageStatus.Pending;
            message.Title = this.Name + ":" + requestData.Metadata.ToString();
            LdapClientHandler.SendRequest(requestData);
        }

        protected override void SendRequestAsync(RequestData requestData, PropertyChangedEventHandler propertyChanged)
        {
            Message message = requestData.ReferencedMessage;
            message.Status = MessageStatus.Pending;
            message.Title = this.Name + ":" + requestData.Metadata.ToString();
            if (propertyChanged != null)
            {
                message.PropertyChanged += propertyChanged;
            }
            lock (RequestQueue)
            {
                if (RequestQueue.Count > queueLength)
                {
                    message.Status = MessageStatus.Abandoned;
                }
                RequestQueue.Enqueue(requestData);
                Monitor.Pulse(RequestQueue);
            }
        }
    }
}
