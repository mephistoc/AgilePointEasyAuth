using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Threading;
using Ascentn.Workflow.Base;
using System.ServiceModel.Web;

namespace AgilePoint.Azure.ServiceBindingFactory.AgilePointEasyAuth {
    public class BasicHttpBindingFactory : BaseServiceBindingFactory, IWFServiceBindingFactory {
        /// <summary>
        /// Override the base Initialize method for initializing the Service Binding
        /// </summary>
        /// <param name="homeDirectory"></param>
        /// <param name="parameters"></param>
        public override void Initialize(string homeDirectory, IDictionary<string, string> parameters) {
            base.Initialize(homeDirectory, parameters);

            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 64; // ?

            // Initialize Http Cache
            System.Web.HttpRuntime httpRT = new System.Web.HttpRuntime();

            Logging("AgilePointEasyAuth BasicHttpBindingFactory is being initialized.");
        }

        /// <summary>
        /// A Method to open the ServiceHost based on given endpoints and service type.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="serviceTypes"></param>
        /// <returns>List of ServiceHost opened based on given endpoints</returns>
        public IDictionary<String, List<ServiceHost>> OpenBindings(IDictionary<string, string> endpoints, IDictionary<string, Type> serviceTypes) {
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.MaxBufferPoolSize = Int32.MaxValue;
            binding.MaxReceivedMessageSize = Int32.MaxValue;

            System.Xml.XmlDictionaryReaderQuotas rdQuota = new System.Xml.XmlDictionaryReaderQuotas();
            rdQuota.MaxArrayLength = Int32.MaxValue;
            rdQuota.MaxStringContentLength = Int32.MaxValue;
            binding.ReaderQuotas = rdQuota;

            binding.HostNameComparisonMode = HostNameComparisonMode.Exact;

            List<ServiceHost> serviceHosts = new List<ServiceHost>();

            foreach (string key in endpoints.Keys) {
                string endpoint = endpoints[key];

                foreach (string serviceTypeKey in serviceTypes.Keys) {
                    ServiceHost serviceHost = OpenServiceHost(endpoint, binding, serviceTypeKey, serviceTypes[serviceTypeKey]);
                    serviceHosts.Add(serviceHost);
                }
                m_ServiceHosts.Add(key, serviceHosts);
            }
            Logging("WCF Service binding is opened.");
            return m_ServiceHosts;
        }

        /// <summary>
        /// Method to get the security context of service.
        /// </summary>
        /// <returns></returns>
        public IDictionary<String, Object> GetSecurityContext() {
            Dictionary<String, Object> securityContext = new Dictionary<string, object>();

            //TO DO
            //Put your code here to get the security context.

            return securityContext;
        }

        /// <summary>
        /// Method to open the Service host based on given  endpoint and binding type.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="binding">binding of type BasicHttpBinding</param>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ServiceHost OpenServiceHost(string endpoint, BasicHttpBinding binding, string key, Type type) {
            string url = string.Format("{0}/{1}/{2}", endpoint, AP_SERVICE_NS, key);
            Uri wcfAddress = new Uri(url);
            //Type type = svcs[key];
            Type[] interfaceTypes = type.GetInterfaces();
            ServiceHost wcfHost = new ServiceHost(type, wcfAddress);

            wcfHost.AddServiceEndpoint(interfaceTypes[0], binding, string.Empty);
            wcfHost.Open();
            return wcfHost;
        }

        public bool CheckAuthenticated(string userName, string password) {
            return true;
        }

        public bool CheckAuthenticated() {
            try {
                if (WebOperationContext.Current.IncomingRequest.Method != "OPTIONS") {
                    if (WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null) {

                        string BasicAuthHead = WebOperationContext.Current.IncomingRequest.Headers["Authorization"].ToString();
                        string EncodedAuthString = BasicAuthHead.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();

                        return true;
                    }
                    else {
                        throw new Exception();
                    }
                }
            }
            catch (FaultException fex) {
                Logging(fex);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "Invalid Credential";
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;
                throw;
            }
            catch (Exception ex) {
                Logging(ex);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;
                throw;
            }
            finally {
            }
            return false;
        }

        public string GetUserName() {
            try {
                if (WebOperationContext.Current.IncomingRequest.Method != "OPTIONS") {
                    if (WebOperationContext.Current.IncomingRequest.Headers["AuthUserName"] != null) {
                        return WebOperationContext.Current.IncomingRequest.Headers["AuthUserName"].ToString();
                    }
                    else if (WebOperationContext.Current.IncomingRequest.Headers["Authorization"] != null) {
                        string BasicAuthHead = WebOperationContext.Current.IncomingRequest.Headers["Authorization"].ToString();
                        string EncodedAuthString = BasicAuthHead.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                        string DecodedString = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(EncodedAuthString));

                        if (DecodedString.Contains(":")) {
                            string[] Credential = DecodedString.Split(':');
                            if (Credential.Length >= 2) {

                                string userName = Credential[0];

                                Logging("UserName-->{0}", userName);

                                return userName;
                            }
                            else {
                                Logging("Invalid Credentials-->{0}", EncodedAuthString);
                                throw new Exception();
                            }
                        }
                        else {
                            string userName = DecodedString;
                            if (!string.IsNullOrEmpty(userName)) {
                                return userName;
                            }
                            else {
                                Logging("Invalid Credentials-->{0}", EncodedAuthString);
                                throw new Exception();
                            }
                        }
                    }
                    else {
                        throw new Exception();
                    }
                }
            }
            catch (FaultException fex) {
                Logging(fex);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = "Invalid Credential";
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;
                throw;
            }
            catch (Exception ex) {
                Logging(ex);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                WebOperationContext.Current.OutgoingResponse.SuppressEntityBody = true;
                throw;
            }
            finally {
            }
            return string.Empty;
        }

    }
}
