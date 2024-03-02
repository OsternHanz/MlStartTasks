﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Serilog.Events;

namespace Client
{
    public static class ConfigReader
    {
        public static string[] ReadConfigFromFile(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNodeList elements = xmlDoc.SelectNodes("//text()");

            if (elements != null)
            {
                List<string> values = new List<string>();

                foreach (XmlNode element in elements)
                {
                    if (element.ParentNode != null && element.ParentNode.NodeType == XmlNodeType.Element)
                    {
                        values.Add(element.InnerText);
                    }
                }

                return values.ToArray();
            }
            else
            {
                ClientLogger.LogByTemplate(logEventLevel: LogEventLevel.Error, note: "Content node not found in the config file.");
                throw new InvalidOperationException("Content node not found in the config file.");
            }
        }
    }
}
