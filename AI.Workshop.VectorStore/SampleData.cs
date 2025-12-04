namespace AI.Workshop.VectorStore;

public static class SampleData
{
    public static readonly List<VectorModel> CloudServices =
    [
        new() {
            Key = 0,
            Name = "PostgreSQL",
            Description = "A powerful, open source object-relational database system with over 35 years of active development. It supports both SQL and JSON querying, and is known for reliability, feature robustness, and performance."
        },
        new() {
                Key = 1,
                Name = "RabbitMQ",
                Description = "A fully managed enterprise message broker supporting both point to point and publish-subscribe integrations. It's ideal for building decoupled applications, queue-based load leveling, or facilitating communication between microservices."
        },
        new() {
                Key = 2,
                Name = "MinIO",
                Description = "MinIO is a high-performance, S3 compatible object store. It allows your applications to store and retrieve files locally or in the cloud. MinIO is highly scalable to store massive amounts of data."
        },
        new() {
                Key = 3,
                Name = "Keycloak",
                Description = "Open source identity and access management. Manage user identities and control access to your apps, data, and resources with single sign-on and multi-factor authentication."
        },
        new() {
                Key = 4,
                Name = "HashiCorp Vault",
                Description = "Store and access application secrets like connection strings and API keys in an encrypted vault with restricted access to make sure your secrets and your application aren't compromised."
        },
        new() {
                Key = 5,
                Name = "Elasticsearch",
                Description = "A distributed, RESTful search and analytics engine for all types of data. Information retrieval at scale for traditional and conversational search applications, with options for AI enrichment and vectorization."
        }
    ];
}
