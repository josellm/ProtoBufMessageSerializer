ProtoBufMessageSerializer is a message serializer for MassTransit(https://github.com/MassTransit/MassTransit) that uses the Protobuf-net library(http://code.google.com/p/protobuf-net/)

Support for add message types to protobuf model dynamically

Example:

var bus	= ServiceBusFactory.New(sbc => {
											sbc.UseRabbitMq();
											sbc.ReceiveFrom("rabbitmq://localhost/kittens");
											sbc.UseProtoBufSerializer();
		                             	});
										
//You need to decorate your message classes with protobuf attributes or you can add the type dynamically to protobuf model:

	ProtoBufMessageSerializer.AddMessageType<TMessage>();