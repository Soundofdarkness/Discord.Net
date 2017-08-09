﻿using System.Text.Json;

namespace Discord.Serialization.Json.Converters
{
    public class ObjectPropertyConverter<T> : IJsonPropertyConverter<T>
        where T : class, new()
    {
        private readonly ModelMap<T> _map;

        public ObjectPropertyConverter(Serializer serializer)
        {
            _map = serializer.MapModel<T>();
        }

        public T Read(PropertyMap map, object model, ref JsonReader reader, bool isTopLevel)
        {
            var subModel = new T();

            if ((isTopLevel && !reader.Read()) || reader.TokenType != JsonTokenType.StartObject)
                throw new SerializationException("Bad input, expected StartObject");
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return subModel;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new SerializationException("Bad input, expected PropertyName");
                
                if (_map.TryGetProperty(reader.Value, out var property))
                    (property as IJsonPropertyMap<T>).Read(subModel, ref reader);
                else
                    JsonReaderUtils.Skip(ref reader); //Unknown property, skip
            }
            throw new SerializationException("Bad input, expected EndObject");
        }
        public void Write(PropertyMap map, object model, ref JsonWriter writer, T value, bool isTopLevel)
        {
            if (isTopLevel)
                writer.WriteObjectStart(map.Key);
            else
                writer.WriteObjectStart();
            for (int i = 0; i < _map.Properties.Length; i++)
                (_map.Properties[i] as IJsonPropertyMap<T>).Write(value, ref writer);
            writer.WriteObjectEnd();
        }
    }
}