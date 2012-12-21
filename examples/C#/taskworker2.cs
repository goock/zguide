﻿//
//  Task worker - design 2
//  Adds pub-sub flow to receive and respond to kill signal
//

//  Author:     Michael Compton
//  Email:      michael.compton@littleedge.co.uk

using System;
using System.Text;
using ZeroMQ;
using System.Threading;

namespace ZMQGuide {
    class TaskWorker2 {
        private ZmqContext context;
        private ZmqSocket receiver;
        private ZmqSocket sender;
        private ZmqSocket controller;
        private bool killCommand;

        public TaskWorker2() {
            context = ZmqContext.Create();
            receiver = context.CreateSocket(SocketType.PULL);
            sender = context.CreateSocket(SocketType.PUSH);
            controller = context.CreateSocket(SocketType.SUB);
        }

        public void Process() {
            //  ZmqSocket to receive messages on
            receiver.Connect("tcp://localhost:5557");
            //  ZmqSocket to send messages to
            sender.Connect("tcp://localhost:5558");
            //  ZmqSocket for control input
            controller.Connect("tcp://localhost:5559");
            controller.Subscribe("", Encoding.Unicode);

            //  Process messages from receiver and controller
            PollItem[] items = new PollItem[2];
            items[0] = receiver.CreatePollItem(Poller.POLLIN);
            items[0].PollInHandler += new PollHandler(ReceiverPollInHandler);
            items[1] = controller.CreatePollItem(Poller.POLLIN);
            items[1].PollInHandler += new PollHandler(ControllerPollInHandler);

            //  Process messages from both sockets
            killCommand = false;
            while (!killCommand) {
                ZmqContext.Poll(items, -1);
            }
        }

        private void ReceiverPollInHandler(ZmqSocket ZmqSocket, Poller revents) {
            //  Process task
            int workload = Convert.ToInt32(ZmqSocket.Receive(Encoding.Unicode));
            //  Do the work
            Thread.Sleep(workload);
            //  Send results to sink
            sender.Send("", Encoding.Unicode);
            Console.WriteLine(".");
            Console.Clear();
        }

        private void ControllerPollInHandler(ZmqSocket ZmqSocket, Poller revents)
        {
            //  Any waiting controller command acts as 'KILL'
            Console.WriteLine("Killed...");
            killCommand = true;
        }
    }

    class Program39 {
        static void Main(string[] args) {
            TaskWorker2 taskworker = new TaskWorker2();
            taskworker.Process();
        }
    }
}
