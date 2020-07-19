using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace DShop.Common.Mongo
{
    public class MongoDbInitializer : IMongoDbInitializer
    {
        private static bool _initialized;
        private readonly bool _seed;
        private readonly IMongoDatabase _database;
        private readonly IMongoDbSeeder _seeder;

        public MongoDbInitializer(IMongoDatabase database, 
            IMongoDbSeeder seeder,
            MongoDbOptions options)
        {
            _database = database;
            _seeder = seeder;
            _seed = options.Seed;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }
            RegisterConventions();
            _initialized = true;
            if (!_seed)
            {
                return;
            }
            await _seeder.SeedAsync();
        }

        private void RegisterConventions()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
            BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
            BsonSerializer.RegisterSerializer(typeof(IDictionary<string, object>), new ComplexTypeSerializer());

            ConventionRegistry.Register("Conventions", new MongoDbConventions(), x => true);
        }

        private class MongoDbConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }

        private class ComplexTypeSerializer : SerializerBase<object>
        {
            public override object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
                var document = serializer.Deserialize(context, args);

                var bsonDocument = document.ToBsonDocument();

                var result = BsonExtensionMethods.ToJson(bsonDocument);
                return JsonConvert.DeserializeObject<IDictionary<string, object>>(result);
            }

            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
            {
                var jsonDocument = JsonConvert.SerializeObject(value);
                var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

                var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));
                serializer.Serialize(context, bsonDocument.AsBsonValue);
            }
        }
    }
}