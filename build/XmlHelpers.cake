using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public void XmlPoke(FilePath filePath, string xpath, string value,
     bool preserveWhitespace = false, Dictionary<string, string> namespaces = null) {

    // ensure the specified xml file exists
    if (!FileExists(filePath)) {
        throw new CakeException("Failed to find Xml File for XmlPoke.");
    }

    var document = new XmlDocument();
    document.PreserveWhitespace = preserveWhitespace;
    document.Load(filePath.FullPath);

    var nsMgr = new XmlNamespaceManager(document.NameTable);
    if(namespaces != null)
    {
        foreach (var xmlNamespace in namespaces) {
            nsMgr.AddNamespace(xmlNamespace.Key /* Prefix */, xmlNamespace.Value /* URI */);
        }
    }
    
    var nodes = document.SelectNodes(xpath, nsMgr);
    if (nodes == null || nodes.Count == 0) {
        throw new CakeException("Failed to find nodes matching that XPath.");
    }

    foreach (XmlNode node in nodes) {
        node.InnerXml = value;
    }
    document.Save(filePath.FullPath);
}