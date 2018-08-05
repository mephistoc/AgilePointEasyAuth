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

namespace AgilePoint.Azure.ServiceBindingFactory.AgilePointEasyAuth {
    public class BaseServiceBindingFactory {
        public const string AP_SERVICE_NS = "AgilePointService";

        protected string m_HomeDirectory = null;
        //protected List<ServiceHost> m_ServiceHosts = new List<ServiceHost>();
        protected Dictionary<String, List<ServiceHost>> m_ServiceHosts = new Dictionary<string, List<ServiceHost>>();
        private IDictionary<string, string> m_Parameters = null;
        /// <summary>
        /// Initialize Service Binding
        /// </summary>
        /// <param name="homeDirectory"></param>
        /// <param name="parameters"></param>
        public virtual void Initialize(string homeDirectory, IDictionary<string, string> parameters) {
            this.m_HomeDirectory = homeDirectory;
            this.m_Parameters = parameters;
        }
        /// <summary>
        /// Get Configuration value of Service binding
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetConfigurationValue(string key) {
            if (m_Parameters == null || key == null) return null;
            string value = null;
            m_Parameters.TryGetValue(key, out value);
            return value;
        }
        /// <summary>
        /// Method to write log
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public virtual void Logging(string format, params object[] args) {
            Logger.WriteLine("AgilePoint.Azure.ServiceBindingFactory.AgilePointEasyAuth, " + format, args);
        }
        /// <summary>
        /// Method to write log
        /// </summary>
        /// <param name="ex"></param>
        public virtual void Logging(Exception ex) {
            // log exception in the application
            Logger.WriteLine("AgilePoint.Azure.ServiceBindingFactory.AgilePointEasyAuth, Exception: {0} \nInner Exception: {1}",
                    ShUtil.GetSoapMessage(ex),
                    (ex.InnerException == null ? string.Empty : ShUtil.GetSoapMessage(ex.InnerException)));
        }
        /// <summary>
        /// Close all the bindings
        /// </summary>
        public virtual void CloseBindings() {
            foreach (string serviceHostKey in m_ServiceHosts.Keys) {
                foreach (ServiceHost sh in m_ServiceHosts[serviceHostKey]) {
                    try {
                        sh.Close();
                    }
                    catch (Exception ex) {
                        Logging(ex);
                    }
                }
            }
            Logging("WCF Service binding is closed.");
        }

    }
}

