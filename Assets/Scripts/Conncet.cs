using Coherence;
using Coherence.Connection;
using Coherence.Toolkit;
using UnityEngine;

public class Conncet : MonoBehaviour
{
    // Replication server rooms service for problems
    void Start()
    {
        var bridge = FindAnyObjectByType<CoherenceBridge>();

        var endpoint = new EndpointData
        {
            region = EndpointData.LocalRegion,
            host = "192.168.11.254",
            port = 42001,
            schemaId = RuntimeSettings.Instance.SchemaID,
        };

        bridge.Connect(endpoint);
    }
}
