using System;
using System.IO;
using System.Linq;
using MassTransit.EndpointConfigurators;
using MassTransit.Serialization.Custom;
using MassTransit.Subscriptions.Messages;
using ProtoBuf.Meta;

namespace MassTransit.Serialization
{

	/// <summary>
	/// ProtoBufMessageSerializer use the Protobuf-net library
	/// </summary>
	public class ProtoBufMessageSerializer : IMessageSerializer
	{
		private static RuntimeTypeModel _protobufModel;
		private const string ContentTypeHeaderValue = "application/vnd.masstransit+pbuf";

		static ProtoBufMessageSerializer()
		{
			AddMassiveTransitTypes();
		}

		public string ContentType
		{
			get { return ContentTypeHeaderValue; }
		}

		public void Serialize<T>(Stream output, ISendContext<T> context) where T : class
		{
			context.SetContentType(ContentTypeHeaderValue);
			Envelope envelope = Envelope.Create(context);
			using (var outputStream = new NonClosingStream(output))
			{
				ProtoBuf.Serializer.Serialize(outputStream, envelope);
			}
		}

		public void Deserialize(IReceiveContext context)
		{
			Envelope result;
			using (var inputStream = new NonClosingStream(context.BodyStream))
			{
				result = ProtoBuf.Serializer.Deserialize<Envelope>(inputStream);
			}
			if (result == null || result.Message == null)
				throw new Exception("ProtoBufMessageSerializer::Deserialize. Error Deserializing message.");

			context.SetUsingEnvelope(result);
			context.SetMessageTypeConverter(new StaticMessageTypeConverter(result.Message));
		}

		/// <summary>
		/// Add a message type to protobuf type model dinamically
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public static void AddMessageType<T>()
		{
			AddMessageType(typeof (T));
			//Compile new type
			ProtoBuf.Serializer.PrepareSerializer<T>();
		}


		public static void AddMessageType(Type type)
		{
			if (_protobufModel.IsDefined(type))
				return; //already defined
			AddTypeToModel(type);
		}

		public static void Compile()
		{
			_protobufModel.CompileInPlace();
		}

		private static void AddMassiveTransitTypes()
		{
			_protobufModel = RuntimeTypeModel.Default;
			//Right order...
			AddTypeToModel<AddPeerSubscriptionMessage>();
			AddTypeToModel<AddPeerMessage>();
			AddTypeToModel<RemovePeerMessage>();
			AddTypeToModel<PeerMessage>();

			AddTypeToModel<PeerSubscriptionMessage>();
			AddTypeToModel<SubscriptionAddedMessage>();
			AddTypeToModel<SubscriptionRemovedMessage>();
			AddTypeToModel<RemovePeerSubscriptionMessage>();
			AddTypeToModel<SubscribeToMessage>();
			AddTypeToModel<UnsubscribeFromMessage>();
			AddTypeToModel<SubscriptionMessage>();

			MetaType metaEnvelope = AddTypeToModel<Envelope>();
			//Property 'Message' is a object, we need DynamicType
			metaEnvelope.GetFields()[6].DynamicType = true;
			//Compile all types
			_protobufModel.CompileInPlace();
		}

		/// <summary>
		/// Add type to model on the fly
		/// http://wallaceturner.com/serialization-with-protobuf-net
		/// TODO: Support for generics
		/// TODO: Mark type Object with DynamicType = true
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		private static MetaType AddTypeToModel<T>()
		{
			return AddTypeToModel(typeof (T));
		}

		private static MetaType AddTypeToModel(Type type)
		{
			return _protobufModel.Add(type, true).Add(type.GetProperties().Select(p => p.Name).OrderBy(name => name).ToArray());
		}
	}

	/// <summary>
	/// Extends EndpointFactoryConfigurator
	/// </summary>
	public static class SerializerConfigurationExtensions
	{
		public static T UseProtoBufSerializer<T>(this T configurator)
			where T : EndpointFactoryConfigurator
		{
			configurator.SetDefaultSerializer<ProtoBufMessageSerializer>();
			return configurator;
		}
	}
}