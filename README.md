ProtoBufMessageSerializer is a message serializer for MassTransit(https://github.com/MassTransit/MassTransit) that uses the Protobuf-net library(http://code.google.com/p/protobuf-net/)

Support for add message types to protobuf model dynamically

Example:

var bus	= ServiceBusFactory.New(sbc => {
											sbc.UseRabbitMq();
											sbc.ReceiveFrom("rabbitmq://kittens");
											sbc.UseProtoBufSerializer();
		                             	});