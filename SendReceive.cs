using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.Util;
using System.Collections;
using System.Reflection;
using System.IO;

namespace ActiveMQ
{
    class Program
    {
        private static string[] GetConfigSearchPaths()
        {
            ArrayList pathList = new ArrayList();

            // Check the current folder first.
            pathList.Add("");
            AppDomain currentDomain = AppDomain.CurrentDomain;

            // Check the folder the assembly is located in.

            pathList.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            if (null != currentDomain.BaseDirectory)
            {
                pathList.Add(currentDomain.BaseDirectory);
            }

            if (null != currentDomain.RelativeSearchPath)
            {
                pathList.Add(currentDomain.RelativeSearchPath);
            }

            return (string[])pathList.ToArray(typeof(string));
        }

        static void Main(string[] args)
        {
        
            // Example connection strings:
            //    activemq:tcp://localhost:61616
            //    stomp:tcp://activemqhost:61613
            //    ems:tcp://tibcohost:7222
            //    msmq://localhost

            Uri connecturi = new Uri("activemq:tcp://localhost:61616");

            Console.WriteLine("About to connect to " + connecturi);

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            IConnectionFactory factory = new NMSConnectionFactory(connecturi);

            using (IConnection connection = factory.CreateConnection())
            using (ISession session = connection.CreateSession())
            {
                // Examples for getting a destination:
                //
                // Hard coded destinations:
                //    IDestination destination = session.GetQueue("FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetTopic("FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Embedded destination type in the name:
                //    IDestination destination = SessionUtil.GetDestination(session, "queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = SessionUtil.GetDestination(session, "topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);
                //
                // Defaults to queue if type is not specified:
                //    IDestination destination = SessionUtil.GetDestination(session, "FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //
                // .NET 3.5 Supports Extension methods for a simplified syntax:
                //    IDestination destination = session.GetDestination("queue://FOO.BAR");
                //    Debug.Assert(destination is IQueue);
                //    IDestination destination = session.GetDestination("topic://FOO.BAR");
                //    Debug.Assert(destination is ITopic);

                IDestination destination = SessionUtil.GetDestination(session, "queue://audit");
                Console.WriteLine("Using destination: " + destination);

                // Create a consumer and producer
                using (IMessageConsumer consumer = session.CreateConsumer(destination))
                using (IMessageProducer producer = session.CreateProducer(destination))
                {
                    // Start the connection so that messages will be processed.
                    connection.Start();
                    //producer.Persisten = true;

                    // Send a message
                    ITextMessage request = session.CreateTextMessage("Hello World!");
                    request.NMSCorrelationID = "abc";
                    request.Properties["NMSXGroupID"] = "cheese";
                    request.Properties["myHeader"] = "Cheddar";

                    producer.Send(request);

                    // Consume a message
                    ITextMessage message = consumer.Receive() as ITextMessage;
                    if (message == null)
                    {
                        Console.WriteLine("No message received!");
                    }
                    else
                    {
                        Console.WriteLine("Received message with ID:   " + message.NMSMessageId);
                        Console.WriteLine("Received message with text: " + message.Text);
                    }
                }
            }
        }
    }
}