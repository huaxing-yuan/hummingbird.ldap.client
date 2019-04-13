using Hummingbird.TestFramework.Messaging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Hummingbird.TestFramework.Services
{
    internal static class LdapClientHandler
    {
        internal static bool StopAllRequested { get; set; }

        static DirectoryEntry currentEntry;
        static string currentServerAddress;

        private static void Cycle()
        {
            RequestData Object;
            while (!StopAllRequested)
            {
                lock (LdapClient.RequestQueue)
                {
                    if (LdapClient.RequestQueue.Count == 0)
                    {
                        Monitor.Wait(LdapClient.RequestQueue);
                    }
                    if (LdapClient.RequestQueue.Count == 0) continue;
                    Object = (RequestData)LdapClient.RequestQueue.Dequeue();
                }
                SendRequest(Object);
            }
            if (currentEntry != null)
            {
                currentEntry.Close();
                currentEntry = null;
            }
        }

        internal static void StopAll()
        {
            StopAllRequested = true;
            lock (LdapClient.RequestQueue)
            {
                Monitor.PulseAll(LdapClient.RequestQueue);
            }
            try
            {
                currentEntry?.Dispose();
            }
            catch {
                //ignore any error when doing dispose.
            }
        }

        internal static void SendRequest(RequestData obj)
        {
            try
            {
                obj.ReferencedMessage.Status = MessageStatus.Sending;
                obj.ReferencedMessage.RequestText = obj.Data.ToString();
                obj.ReferencedMessage.RequestObject = obj.Data;
                string serverAddress = LdapClient.ServerAddress;
                if (currentEntry == null || serverAddress != currentServerAddress)
                {
                    currentServerAddress = serverAddress;
                    if (string.IsNullOrEmpty(serverAddress))
                    {
                        currentEntry = new DirectoryEntry();
                    }
                    else
                    {
                        currentEntry = new DirectoryEntry(serverAddress);
                    }
                }
                using (DirectorySearcher dSearch = new DirectorySearcher(currentEntry))
                {
                    dSearch.Filter = obj.Data.ToString();
                    dSearch.SizeLimit = LdapClient.SizeLimit;
                    using (var result = dSearch.FindAll())
                    {
                        FormatSearchResult(result, out string responseText, out List<LdapObject> ldapObjects);
                        MessageQueue.UpdateOutput(obj.ReferencedMessage, ldapObjects, responseText, MessageStatus.Sent);
                    }
                }


            }
            catch (Exception ex)
            {
                MessageQueue.UpdateOutput(obj.ReferencedMessage, null, ex.ToString(), MessageStatus.Failed);
                Log.WriteMessage(LogLevel.Error, string.Format("Message failed: {0}", ex.Message));
            }

        }

        internal static void StartNewHandler()
        {
            Thread CyclingThread = new Thread(new ThreadStart(Cycle));
            CyclingThread.IsBackground = true;
            CyclingThread.Start();
        }

        internal static void FormatSearchResult(SearchResultCollection result, out string text, out List<LdapObject> obj)
        {
            obj = new List<LdapObject>();
            if (result == null)
            {
                text = "No result found for your query.";
                return;
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (SearchResult sr in result)
            {
                sb.AppendLine("LDAP Object: " + sr.Properties["distinguishedname"][0].ToString() + " {");
                LdapObject o = new LdapObject() { Name = sr.Properties["distinguishedname"][0].ToString() };
                foreach (string var in sr.Properties.PropertyNames)
                {
                    if (sr.Properties[var].Count == 1)
                    {
                        object value = sr.Properties[var][0];
                        string stringvalue = FormatValue(value);
                        sb.AppendLine(string.Format("    {0} : {1}", var, stringvalue));
                        o.Attributes.Add(new AttributeIndexValue()
                        {
                            Attribute = var,
                            Count = string.Empty,
                            Value = stringvalue,
                        });
                    }
                    else
                    {
                        sb.AppendLine(string.Format("    {0} {{", var));
                        o.Attributes.Add(new AttributeIndexValue()
                        {
                            Attribute = var,
                            Count = sr.Properties[var].Count.ToString(),
                            Value = string.Empty,
                        });
                        for (int i = 0; i < sr.Properties[var].Count; i++)
                        {
                            object value = sr.Properties[var][i];
                            string stringvalue = FormatValue(value);
                            sb.AppendLine(string.Format("        [{0}] => {1}", i, stringvalue));
                            o.Attributes.Add(new AttributeIndexValue()
                            {
                                Attribute = string.Empty,
                                Count = "[" + i + "]",
                                Value = stringvalue,
                            });
                        }
                        sb.AppendLine("    }");
                    }
                }
                sb.AppendLine("}\n");
                obj.Add(o);
            }
            text = sb.ToString();
        }

        private static string FormatValue(object value)
        {
            if (value is byte[])
            {
                try
                {
                    var sid = new SecurityIdentifier(value as byte[], 0);
                    // This gives you what you want
                    return sid.ToString();
                }
                catch
                {
                    return BitConverter.ToString(value as byte[]);
                }
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
