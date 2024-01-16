using System.Text;
using System.Xml;

XmlDocument configDocument = new XmlDocument();
string configDocumentPath;
string proxiesList = null;
string proxyType = "SOCKS5";

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

Console.Write("Please input the proxy type (SOCKS5/SOCKS4/HTTP): ");
proxyType = Console.ReadLine();

XmlNode proxyNodeContainer = configDocument.SelectSingleNode("/ProxifierProfile/ProxyList");

if (proxyNodeContainer != null)
{
    int lastID = 100;

    // Find correct ID offset
    if (proxyNodeContainer.HasChildNodes)
    {
        XmlNode? lastProxyNode = proxyNodeContainer.ChildNodes[proxyNodeContainer.ChildNodes.Count - 1];
        XmlAttribute? proxyId = lastProxyNode.Attributes["id"];
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

        // Create primary proxy node
        XmlElement newProxyNode = configDocument.CreateElement("Proxy");
        XmlAttribute newProxyIDAttr = configDocument.CreateAttribute("id");
        XmlAttribute newProxyTypeAttr = configDocument.CreateAttribute("type");

        // Create child proxy options
        XmlElement proxyOptionsNode = configDocument.CreateElement("Options");
        XmlElement proxyPortNode = configDocument.CreateElement("Port");
        XmlElement proxyAddressNode = configDocument.CreateElement("Address");

        // Assign values to proxy node
        newProxyIDAttr.Value = (lastID + 1).ToString();
        newProxyTypeAttr.Value = proxyType;

        // Assign values to child nodes
        proxyOptionsNode.InnerText = "48";
        proxyPortNode.InnerText = ipAndPort[1];
        proxyAddressNode.InnerText = ipAndPort[0];

        // Apply attributes to proxy node
        newProxyNode.SetAttributeNode(newProxyIDAttr);
        newProxyNode.SetAttributeNode(newProxyTypeAttr);

        // Apply child nodes to proxy node
        newProxyNode.AppendChild(proxyOptionsNode);
        newProxyNode.AppendChild(proxyPortNode);
        newProxyNode.AppendChild(proxyAddressNode);

        // Apply proxy node to the list of proxies
        proxyNodeContainer.AppendChild(newProxyNode);
        lastID = lastID + 1;
        proxyIds.Add(lastID);
    }

    // Fetch the proxy chain list
    XmlNode? chainsNodeContainer = configDocument.SelectSingleNode("/ProxifierProfile/ChainList");
    if (chainsNodeContainer != null)
    {
        if (chainsNodeContainer.HasChildNodes)
        {
            // Fetch the first proxy chain
            XmlNode? proxyChain = chainsNodeContainer.FirstChild;

            // Input proxy IDs into the proxy chain
            foreach (int proxyId in proxyIds)
            {
                // Create new proxy listing
                XmlElement newProxyNode = configDocument.CreateElement("Proxy");
                XmlAttribute newProxyEnabledAttr = configDocument.CreateAttribute("enabled");

                // Apply default values to proxy listing
                newProxyNode.SetAttributeNode(newProxyEnabledAttr);
                newProxyNode.InnerText = proxyId.ToString();

                // Append proxy listing to the chain
                proxyChain.AppendChild(newProxyNode);
            }
        }
    }

    // Flush config back to disk
    configDocument.Save(configDocumentPath);
    Console.WriteLine("Finished appending XML! Enjoy! Press any key to exit...");
    Console.ReadLine();
}
else
{
    Console.WriteLine("You have no proxy list container! Is this a valid PPX configuration? Press any key to exit...");
    Console.ReadLine();
}