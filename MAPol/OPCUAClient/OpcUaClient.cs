using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpcUaClientLibrary
{
    public class OpcUaClient : IDisposable
    {
        private ApplicationConfiguration _config;
        private Session _session;
        private SessionReconnectHandler _reconnectHandler;

        private const int ReconnectPeriod = 10000;

        public bool Connected =>
            _session != null && _session.Connected;

    public async Task<Session> ConnectToOpcUaServerAsync()
    {
        string endpointUrl = "opc.tcp://localhost:4840";

        // Create application configuration
        ApplicationConfiguration config = new ApplicationConfiguration
        {
            ApplicationName = "MyOpcUaClient",
            ApplicationType = ApplicationType.Client,

            SecurityConfiguration = new SecurityConfiguration
            {
                AutoAcceptUntrustedCertificates = true,
                RejectSHA1SignedCertificates = false
            },

            TransportQuotas = new TransportQuotas
            {
                OperationTimeout = 15000
            },

            ClientConfiguration = new ClientConfiguration
            {
                DefaultSessionTimeout = 60000
            }
        };

        await config.Validate(ApplicationType.Client);

        config.CertificateValidator.CertificateValidation += (sender, e) =>
        {
            e.Accept = true;
        };

        // Select OPC UA endpoint
        EndpointDescription selectedEndpoint =
            CoreClientUtils.SelectEndpoint(
                config,
                endpointUrl,
                false);

        // Create configured endpoint
        ConfiguredEndpoint endpoint =
            new ConfiguredEndpoint(
                null,
                selectedEndpoint,
                EndpointConfiguration.Create(config));

        // Create session
        Session session = await Session.Create(
            config,
            endpoint,
            false,
            "OPC UA Session",
            60000,
            new UserIdentity(),
            null);

        return session;
    }

    public void PrintMethods()
        {
            Debug.WriteLine("Methods in CoreClientUtils:");
            foreach (var method in typeof(CoreClientUtils).GetMethods())
            {
                Debug.WriteLine(method);
            }
            Debug.WriteLine("==========================");
        }

        private async Task<ApplicationConfiguration> CreateConfiguration()
        {
            var config = new ApplicationConfiguration
            {
                ApplicationName = "MyOpcUAClient",
                ApplicationType = ApplicationType.Client,
                ApplicationUri =
                    $"urn:{Utils.GetHostName()}:MyOpcUAClient",

                SecurityConfiguration = new SecurityConfiguration
                {
                    ApplicationCertificate =
                        new CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = @"CertificateStores\MachineDefault",
                            SubjectName = "MyOpcUAClient"
                        },

                    TrustedPeerCertificates =
                        new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = @"CertificateStores\UA Applications"
                        },

                    RejectedCertificateStore =
                        new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = @"CertificateStores\RejectedCertificates"
                        },

                    AutoAcceptUntrustedCertificates = true
                },

                TransportQuotas = new TransportQuotas
                {
                    OperationTimeout = 15000
                },

                ClientConfiguration = new ClientConfiguration
                {
                    DefaultSessionTimeout = 60000
                }
            };

            await config.Validate(ApplicationType.Client);

            config.CertificateValidator.CertificateValidation +=
                (sender, e) =>
                {
                    Console.WriteLine(
                        $"Certificate: {e.Certificate.Subject}");

                    e.Accept = true;
                };

            return config;
        }

        private void Session_KeepAlive(
            Session session,
            KeepAliveEventArgs e)
        {
            if (ServiceResult.IsBad(e.Status))
            {
                Console.WriteLine(
                    $"Connection lost: {e.Status}");

                if (_reconnectHandler == null)
                {
                    _reconnectHandler =
                        new SessionReconnectHandler();

                    _reconnectHandler.BeginReconnect(
                        session,
                        ReconnectPeriod,
                        ReconnectComplete);
                }
            }
        }

        private void ReconnectComplete(
            object sender,
            EventArgs e)
        {
            if (!ReferenceEquals(
                    sender,
                    _reconnectHandler))
            {
                return;
            }

            //_session = _reconnectHandler.Session;
            _reconnectHandler.Dispose();
            _reconnectHandler = null;

            Console.WriteLine("Reconnected");
        }

        public object Read(string nodeId)
        {
            var value =
                _session.ReadValue(
                    NodeId.Parse(nodeId));

            return value.Value;
        }

        public T Read<T>(string nodeId)
        {
            var value =
                _session.ReadValue(
                    NodeId.Parse(nodeId));

            return (T)Convert.ChangeType(
                value.Value,
                typeof(T));
        }

        public bool Write(string nodeId, object value)
        {
            var writeValue = new WriteValue
            {
                NodeId = NodeId.Parse(nodeId),
                AttributeId = Attributes.Value,
                Value = new DataValue(
                    new Variant(value))
            };

            var values =
                new WriteValueCollection
                {
                    writeValue
                };

            _session.Write(
                null,
                values,
                out StatusCodeCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            return StatusCode.IsGood(results[0]);
        }

        public List<ReferenceDescription> Browse(NodeId nodeId)
        {
            _session.Browse(
                null,
                null,
                nodeId,
                0,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)(NodeClass.Object | NodeClass.Variable),
                out byte[] cp,
                out ReferenceDescriptionCollection references);

            return new List<ReferenceDescription>(
                references);
        }

        public Subscription CreateSubscription(
            string nodeId,
            int publishingInterval,
            MonitoredItemNotificationEventHandler callback)
        {
            var subscription =
                new Subscription(
                    _session.DefaultSubscription)
                {
                    PublishingInterval =
                        publishingInterval
                };

            var monitoredItem =
                new MonitoredItem(
                    subscription.DefaultItem)
                {
                    DisplayName = nodeId,
                    StartNodeId = nodeId,
                    AttributeId = Attributes.Value
                };

            monitoredItem.Notification += callback;

            subscription.AddItem(monitoredItem);

            _session.AddSubscription(subscription);

            subscription.Create();

            return subscription;
        }

        public void Disconnect()
        {
            if (_session != null)
            {
                _session.Close();
                _session.Dispose();
                _session = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}