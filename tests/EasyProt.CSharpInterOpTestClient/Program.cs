﻿using System;
using System.Collections.Generic;
using System.IO;
using EasyProt.Core;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using EasyProt.Runtime;


namespace EasyProt.CSharpInterOpTest
{

    // Pipeline member
    public class OutMember1 : IPipelineMember<string, string>
    {
        public string Proceed(string input) => input + "XX";
    }

    public class OutMember2 : IPipelineMember<string, string>
    {
        public string Proceed(string input) => "XX" + input;
    }

    public class OnServerResponse : IPipelineMember<string, string>
    {
        public string Proceed(string input)
        {
            Console.WriteLine("ServerResponse: " + input);
            return input;
        }
    }

    // Messags
    public class Msg1 : IProtMessage<string>
    {
        public bool Validate(string message) => message[0] == '1';
    }

    public class Msg2 : IProtMessage<string>
    {
        public bool Validate(string message) => message[0] == '2';
    }

    public class ServerResponse : IProtMessage<string>
    {
        public bool Validate(string message) => message[0] == 'S';
    }

    
    class Program
    {
        static void Main(string[] args)
        {
            var msg1 = new Msg1();
            var msg1OutPipe = (new OutMember1())
                                .Then(new OutMember2())
                                .CreatePipe();

            //var nonResponder = FSharpOption<IPipelineResponder>.None;

            var rntMngr = new Runtime.RuntimeManager();
            // Register a message with an OutGoing-Pipeline
            rntMngr.RegisterMessageOut(msg1OutPipe, new Msg1());
            // Register a message with default-In- and default-Out-Pipeline
            rntMngr.RegisterMessage(new Msg2());
            // Register a message with an Incoming-Pipeline
            rntMngr.RegisterMessageInc((new OnServerResponse()).CreatePipe(), new ServerResponse());

            var client = rntMngr.GetProtClient();
            client.ConnectAsync("127.0.0.1", 8080).Wait();
            client.ListenAsync();

            while (true)
            {
                var msg = System.Console.ReadLine();
                client.SendAsync(msg).Wait();
            }
        }
    }
}
