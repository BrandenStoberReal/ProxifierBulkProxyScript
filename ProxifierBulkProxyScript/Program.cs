using System.Text;
using System.Xml;

XmlDocument configDocument = new XmlDocument();
string configDocumentPath;
string proxiesList = null;

while (true)
{
    Console.Write("Please input path to Proxifier configuration file (.ppx): ");
    string inputFile = Console.ReadLine();
    if (File.Exists(inputFile))
    {
        configDocument.Load(inputFile);
        configDocumentPath = inputFile;
        break;
    }
    else
    {
        Console.WriteLine("Invalid file!");
    }
}

while (true)
{
    Console.Write("Please input path to formatted proxies file (ip:port): ");
    string inputFile = Console.ReadLine();
    if (File.Exists(inputFile))
    {
        proxiesList = File.ReadAllText(inputFile);
        break;
    }
    else
    {
        Console.WriteLine("Invalid file!");
    }
}

XmlNode proxyNodeContainer = configDocument.SelectSingleNode("/ProxifierProfile/ProxyList");

if (proxyNodeContainer != null)
{
    int lastID = 100;
    if (proxyNodeContainer.HasChildNodes)
    {
        XmlNode lastProxyNode = proxyNodeContainer.ChildNodes[proxyNodeContainer.ChildNodes.Count - 1];
        XmlAttribute proxyId = lastProxyNode.Attributes["id"];
        if (proxyId != null)
        {
            lastID = Int32.Parse(proxyId.Value);
        }
    }

    List<string> proxies = proxiesList.Split('\n').ToList();
    List<int> proxyIds = new List<int>();

    foreach (string proxy in proxies)
    {
        List<string> ipAndPort = proxy.Split(":").ToList();
        XmlElement newProxyNode = configDocument.CreateElement("Proxy");
        XmlAttribute newProxyIDAttr = configDocument.CreateAttribute("id");
        XmlAttribute newProxyTypeAttr = configDocument.CreateAttribute("type");

        XmlElement proxyOptionsNode = configDocument.CreateElement("Options");
        XmlElement proxyPortNode = configDocument.CreateElement("Port");
        XmlElement proxyAddressNode = configDocument.CreateElement("Address");

        newProxyIDAttr.Value = (lastID + 1).ToString();
        newProxyTypeAttr.Value = "SOCKS5";

        proxyOptionsNode.InnerText = "48";
        proxyPortNode.InnerText = ipAndPort[1];
        proxyAddressNode.InnerText = ipAndPort[0];

        newProxyNode.SetAttributeNode(newProxyIDAttr);
        newProxyNode.SetAttributeNode(newProxyTypeAttr);

        newProxyNode.AppendChild(proxyOptionsNode);
        newProxyNode.AppendChild(proxyPortNode);
        newProxyNode.AppendChild(proxyAddressNode);

        proxyNodeContainer.AppendChild(newProxyNode);
        lastID = lastID + 1;
        proxyIds.Add(lastID);
    }

    XmlNode chainsNodeContainer = configDocument.SelectSingleNode("/ProxifierProfile/ChainList");
    if (chainsNodeContainer != null)
    {
        if (chainsNodeContainer.HasChildNodes)
        {
            XmlNode proxyChain = chainsNodeContainer.FirstChild;

            foreach (int proxyId in proxyIds)
            {
                XmlElement newProxyNode = configDocument.CreateElement("Proxy");
                XmlAttribute newProxyEnabledAttr = configDocument.CreateAttribute("enabled");

                newProxyNode.SetAttributeNode(newProxyEnabledAttr);
                newProxyNode.InnerText = proxyId.ToString();

                proxyChain.AppendChild(newProxyNode);
            }
        }
    }
    configDocument.Save(configDocumentPath);
    Console.WriteLine("Finished appending XML! Enjoy! Press any key to exit...");
    Console.ReadLine();
}
else
{
    Console.WriteLine("You have no proxy list container! Is this a valid PPX configuration? Press any key to exit...");
    Console.ReadLine();
}